using System.Transactions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Dtos.Common;
using MakerspaceFablabPlatform.Dtos.Payment;
using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Entities.Enums;
using MakerspaceFablabPlatform.Excepitons;
using MakerspaceFablabPlatform.Services.Interfaces;
using MakerspaceFablabPlatform.Strategies.MembershipStrategies;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MakerspaceFablabPlatform.Services;

public class PaymentService : IPaymentService
{
    private const decimal DailyRentalRate = 100m;

    private readonly ILogger<PaymentService> _logger;

    private readonly IApplicationDbContext _applicationDbContext;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMembershipStrategyFactory _membershipStrategyFactory;

    public PaymentService(IApplicationDbContext applicationDbContext,IMembershipStrategyFactory membershipStrategyFactory, ILogger<PaymentService> logger, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _applicationDbContext = applicationDbContext;
        _membershipStrategyFactory = membershipStrategyFactory;
        _logger = logger;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    private static decimal CalculateRentalFee(DateTime rentedAt, DateTime expectedReturnAt)
    {
        var duration = expectedReturnAt - rentedAt;

        if (duration <= TimeSpan.Zero)
            return 0m;

        return (decimal)Math.Ceiling(duration.TotalDays) * DailyRentalRate;
    }

    private static decimal CalculateTotalAmount(decimal baseRentalFee, decimal lateFee, decimal discountAmount)
    {
        return Math.Max(0m, baseRentalFee + lateFee - discountAmount);
    }

    private static decimal Round(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);
    private PendingResponse BuildPreview(EquipmentRental rental, User user)
    {
        var releasedAt = rental.ReleasedAt!.Value;

        var overtime = releasedAt - rental.ExpectedReturnAt;
        if (overtime < TimeSpan.Zero)
            overtime = TimeSpan.Zero;

        var strategy = _membershipStrategyFactory.Create(user.Status);

        var rentalFee = Round(CalculateRentalFee(rental.RentedAt, rental.ExpectedReturnAt));
        var lateFee = Round(overtime > TimeSpan.Zero ? strategy.CalculateReleaseOverTimeCost(overtime) : 0m);
        var discountAmount = Round(strategy.CalculateDiscountAmount(rentalFee + lateFee));
        var totalAmount = Round(CalculateTotalAmount(rentalFee, lateFee, discountAmount));

        return new PendingResponse
        {
            EquipmentRentalId = rental.Id,
            EquipmentId = rental.EquipmentId,
            EquipmentName = rental.Equipment?.Name ?? string.Empty,

            RentedAt = rental.RentedAt,
            ExpectedReturnAt = rental.ExpectedReturnAt,
            ReleasedAt = releasedAt,

            IsOverdue = overtime > TimeSpan.Zero,
            OverdueBy = overtime > TimeSpan.Zero ? overtime : null,

            RentalFee = rentalFee,
            LateFee = lateFee,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,

            UserBalance = user.Balance
        };
    }
    private async Task<PagedResponse<Response>> QueryPaymentsAsync(IQueryable<Payment> query, ListRequest request, CancellationToken token)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        if (request.Status is not null)
            query = query.Where(p => p.Status == request.Status);

        if (request.PaymentMethod is not null)
            query = query.Where(p => p.PaymentMethod == request.PaymentMethod);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search.Trim()}%";
            query = query.Where(p => EF.Functions.ILike(p.PaymentNumber, pattern));
        }

        var totalCount = await query.CountAsync(token);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<Response>(_mapper.ConfigurationProvider)
            .ToListAsync(token);

        return new PagedResponse<Response>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
    
    
    public async Task<Response> CreateAsync(CreateRequest request, Guid currentUserId, bool isAdmin, CancellationToken token = default)
    {
        
        var targetUserId = request.UserId ?? currentUserId;

        if (targetUserId != currentUserId && !isAdmin)
            throw new NotResourceOwnerException("Başka bir kullanıcı adına ödeme oluşturamazsınız.");

        var dbuser = await _unitOfWork.Users.GetByIdAsync(targetUserId, token);

        if (dbuser is null)
            throw new NotFoundException(nameof(User), targetUserId);

        var equipmentRental = await _unitOfWork.EquipmentRentals.GetByIdAsync(request.EquipmentRentalId, token);

        if (equipmentRental is null)
            throw new NotFoundException(nameof(EquipmentRental), request.EquipmentRentalId);

        if (equipmentRental.UserId != targetUserId)
            throw new NotResourceOwnerException("Bu kiralama belirtilen kullanıcıya ait değil.");

        if (equipmentRental.ReleasedAt is null)
            throw new InvalidStateTransitionException("Ekipman henüz teslim edilmedi, ödeme oluşturulamaz.");

        if (equipmentRental.IsPaid)
            throw new DuplicateEntityException("Bu kiralama için zaten ödeme alınmış.");

        var hasOpenPayment = await _unitOfWork.Payments.ExistsAsync(p =>
            p.EquipmentRentalId == equipmentRental.Id &&
            !p.IsDeleted &&
            (p.Status == PaymentStatus.Paid || p.Status == PaymentStatus.Pending), token);

        if (hasOpenPayment)
            throw new DuplicateEntityException("Bu kiralama için zaten ödeme alınmış.");

        // Burda validation işlemerini bitiriyoruz
        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                  var preview = BuildPreview(equipmentRental, dbuser);

        if (dbuser.Balance < preview.TotalAmount)
            throw new InsufficientBalanceException(preview.TotalAmount, dbuser.Balance);

        var now = DateTime.UtcNow;

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            PaymentNumber = $"PAY-{now:yyyyMMddHHmmssfff}-{Guid.NewGuid().ToString("N")[..8]}",

            EquipmentRentalId = equipmentRental.Id,
            UserId = targetUserId,

            RentalFee = preview.RentalFee,
            LateFee = preview.LateFee,
            DiscountAmount = preview.DiscountAmount,
            TotalAmount = preview.TotalAmount,

            PaymentMethod = PaymentMethod.Balance,

            Status = PaymentStatus.Paid,

            CreatedAt = now,
            PaidAt = now,
            IsDeleted = false
        };

        await _unitOfWork.Payments.AddAsync(payment, token);

        equipmentRental.PaymentId = payment.Id;
        equipmentRental.IsPaid = true;
        equipmentRental.PaidAt = now;
        equipmentRental.IsOverdue = preview.IsOverdue;
        equipmentRental.OverdueBy = preview.OverdueBy;

        _unitOfWork.EquipmentRentals.Update(equipmentRental);

        dbuser.Balance -= preview.TotalAmount;
        _unitOfWork.Users.Update(dbuser);

        await _unitOfWork.SaveChangesAsync(token);

        _logger.LogInformation(
            "Ödeme alındı. PaymentId: {PaymentId}, EquipmentRentalId: {EquipmentRentalId}, UserId: {UserId}, Total: {TotalAmount}, Kalan bakiye: {Balance}",
            payment.Id, equipmentRental.Id, targetUserId, preview.TotalAmount, dbuser.Balance);

        return _mapper.Map<Response>(payment);

            }
            catch (DbUpdateConcurrencyException)
            {
                // dbuser.Balance başka bir eşzamanlı ödeme tarafından değiştirildi (xmin uyuşmuyor).
                throw new ConcurrencyConflictException("Bakiye başka bir işlem tarafından aynı anda güncellendi. Lütfen tekrar deneyin.");
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                // EquipmentRentalId unique index'i, aynı kiralama için eşzamanlı ikinci ödemeyi burada engelledi.
                throw new DuplicateEntityException("Bu kiralama için zaten ödeme alınmış.");
            }
            catch
            {
                throw;
            }
        }

          }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        => ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
    
    public async Task<PagedResponse<Response>> GetAllAsync(ListRequest request, CancellationToken token = default)
    {
        return await QueryPaymentsAsync(_unitOfWork.Payments.Query().Where(p => !p.IsDeleted), request, token);
    }
    
    public async Task<PagedResponse<Response>> GetAllByUserIdAsync(Guid userId, ListRequest request, CancellationToken token = default)
    {
        var query = _unitOfWork.Payments.Query().Where(p => p.UserId == userId);

        return await QueryPaymentsAsync(query, request, token);
    }
    
    public async Task<Response> GetByIdAsync(Guid paymentId, Guid currentUserId, bool isAdmin, CancellationToken token = default)
    {
        var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId, token);

        if (payment is null || payment.IsDeleted)
            throw new NotFoundException(nameof(Payment), paymentId);

        if (payment.UserId != currentUserId && !isAdmin)
            throw new NotResourceOwnerException("Bu ödeme kaydına erişim yetkiniz yok.");

        return _mapper.Map<Response>(payment);
    }
    
    public async Task<PagedResponse<PendingResponse>> GetPendingAsync(Guid userId, ListRequest request, CancellationToken token = default)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var user = await _unitOfWork.Users.GetByIdAsync(userId, token);

        if (user is null)
            throw new NotFoundException(nameof(User), userId);

        // Teslim edilmiş (ReleasedAt dolu) ve henüz ödenmemiş kiralamalar.
        var query = _unitOfWork.EquipmentRentals.Query()
            .Where(r => r.UserId == userId && r.ReleasedAt != null && !r.IsPaid);

        var totalCount = await query.CountAsync(token);

        var rentals = await query
            .Include(r => r.Equipment)
            .OrderByDescending(r => r.ReleasedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(token);

        return new PagedResponse<PendingResponse>
        {
            Items = rentals.Select(r => BuildPreview(r, user)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
    
    public async Task<PendingResponse> GetPreviewAsync(Guid equipmentRentalId, Guid currentUserId, bool isAdmin, CancellationToken token = default)
    {
        var rental = await _unitOfWork.EquipmentRentals.Query()
            .FirstOrDefaultAsync(r => r.Id == equipmentRentalId, token);

        if (rental is null)
            throw new NotFoundException(nameof(EquipmentRental), equipmentRentalId);

        if (rental.UserId != currentUserId && !isAdmin)
            throw new NotResourceOwnerException("Bu kiralama kaydına erişim yetkiniz yok.");

        if (rental.ReleasedAt is null)
            throw new InvalidStateTransitionException("Ekipman henüz teslim edilmedi, tutar hesaplanamaz.");

        var user = await _unitOfWork.Users.GetByIdAsync(rental.UserId, token);

        if (user is null)
            throw new NotFoundException(nameof(User), rental.UserId);

        return BuildPreview(rental, user);
    }
}
