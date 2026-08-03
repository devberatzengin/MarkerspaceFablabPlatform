using AutoMapper;
using AutoMapper.QueryableExtensions;
using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Dtos.Announcement;
using MakerspaceFablabPlatform.Dtos.Common;
using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Entities.Enums;
using MakerspaceFablabPlatform.Excepitons;
using MakerspaceFablabPlatform.Services.Interfaces;
using FluentValidation;
using MakerspaceFablabPlatform.Data;
using MakerspaceFablabPlatform.States.AnnouncementStates;
using Microsoft.EntityFrameworkCore;
using ValidationException = MakerspaceFablabPlatform.Excepitons.ValidationException;

namespace MakerspaceFablabPlatform.Services;

public class AnnouncementService : IAnnouncementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUserRepository _userRepository;


    private readonly ILogger<AnnouncementService> _logger;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateRequest> _createValidator;
    private readonly IValidator<UpdateRequest> _updateValidator;


    public AnnouncementService(IUnitOfWork unitOfWork, IAnnouncementRepository announcementRepository, ICategoryRepository categoryRepository, IUserRepository userRepository, ILogger<AnnouncementService> logger, IMapper mapper, IValidator<CreateRequest> createValidator, IValidator<UpdateRequest> updateValidator)
    {
        _unitOfWork = unitOfWork;
        _announcementRepository = announcementRepository;
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
        _logger = logger;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Response?> GetByIdAsync(Guid announcementId, bool isAdmin)
    {
        var result = await _unitOfWork.Announcements.GetByIdWithDetailsAsync(announcementId);

        if (result is null)
            throw new NotFoundException(nameof(Announcement), announcementId);

        if (!isAdmin && result.Status != ContentStatus.Published)
            throw new NotFoundException(nameof(Announcement), announcementId);

        return _mapper.Map<Response>(result);
    }

    public async Task<PagedResponse<Response>> GetAllAsync(ListRequest request, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _unitOfWork.Announcements.Query();

        if (!isAdmin)
            query = query.Where(a => a.Status == ContentStatus.Published);
        else if (request.Status is not null)
            query = query.Where(a => a.Status == request.Status);

        if (request.CategoryId is not null)
            query = query.Where(a => a.CategoryId == request.CategoryId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search.Trim()}%";
            query = query.Where(a =>
                EF.Functions.ILike(a.Title, pattern) ||
                EF.Functions.ILike(a.Content, pattern));
        }

        if (request.CreatedFrom is not null)
            query = query.Where(a => a.CreatedAt >= request.CreatedFrom);
        if (request.CreatedTo is not null)
            query = query.Where(a => a.CreatedAt <= request.CreatedTo);

        var totalCount = await query.CountAsync(cancellationToken);

        // 499 Exception Test code
        // await Task.Delay(5000, cancellationToken);  // 15 saniye bekle
        

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<Response>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Listed {Count}/{Total} announcements (page {Page})", items.Count, totalCount, page);

        return new PagedResponse<Response>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Response> CreateAsync(CreateRequest request, Guid currentUserId)
    {
        var validation = await _createValidator.ValidateAsync(request);

        if (!validation.IsValid)
            throw new ValidationException(validation.ToDictionary());
        
        var category = await _unitOfWork.Categories.Query().FirstOrDefaultAsync(c => c.Id == request.CategoryId);
        
        if (category is null)
            throw new NotFoundException(nameof(Category), request.CategoryId);
        
        var nameCount = await _unitOfWork.Announcements.CountByTitleAsync(request.Title);
                    
        var title = nameCount > 0 ? $"{request.Title} {nameCount + 1}" : request.Title;
        
        var creator = await _unitOfWork.Users.Query()
            .Where(u => u.Id == currentUserId)
            .Select(u => new { u.FirstName, u.LastName })
            .FirstAsync();
        
        Announcement newAnnouncement = new Announcement
        {
            Id = Guid.NewGuid(),
            Title = title,
            Content = request.Content,
            CreatedByUserId = currentUserId,
            CategoryId = request.CategoryId,

            Status = ContentStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        await _unitOfWork.Announcements.AddAsync(newAnnouncement);
        await _unitOfWork.SaveChangesAsync();
            
        _logger.LogInformation("Created announcement {AnnouncementId} with title {Title} by user {UserId}", newAnnouncement.Id, title, currentUserId);
        
        var response = _mapper.Map<Response>(newAnnouncement);
        response.CreatedByName = creator.FirstName + " " + creator.LastName;
        response.CategoryName = category.Name;
        return response;

    }



    public async Task<Response> UpdateAsync(UpdateRequest request, Guid currentUserId, bool isAdmin)
    {
        var validation = await _updateValidator.ValidateAsync(request);

        if (!validation.IsValid)
            throw new ValidationException(validation.ToDictionary());

        var announcement = await _unitOfWork.Announcements.GetByIdWithDetailsAsync(request.Id, asNoTracking: false);

        if (announcement is null)
            throw new NotFoundException(nameof(Announcement), request.Id);



        // Admin değilse yalnızca kendi duyurusunu düzenleyebilir
        if (!isAdmin && announcement.CreatedByUserId != currentUserId)
            throw new NotResourceOwnerException("Bu duyuruyu düzenleme yetkiniz yok.");

        var categoryExists = await _unitOfWork.Categories.ExistsAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
            throw new NotFoundException(nameof(Category), request.CategoryId);

        var nameCount = await _unitOfWork.Announcements.CountByTitleAsync(request.Title, announcement.Id);

        var title = nameCount > 0 ? $"{request.Title} {nameCount + 1}" : request.Title;

        announcement.Title = title;
        announcement.Content = request.Content;
        announcement.CategoryId = request.CategoryId;
        announcement.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Announcements.Update(announcement);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Updated announcement {AnnouncementId} with title {Title} by user {UserId}", announcement.Id, title, currentUserId);

        return _mapper.Map<Response>(announcement);
    }

    public async Task<Response> PublishAsync(Guid announcementId, Guid currentUserId)
    {
        var announcement = await _unitOfWork.Announcements.GetByIdWithDetailsAsync(announcementId, asNoTracking: false);

        if (announcement is null)
            throw new NotFoundException(nameof(Announcement), announcementId);

        var state = GetStateFor(announcement.Status);
        await state.PublishAsync(announcement);
        
        _unitOfWork.Announcements.Update(announcement);
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation("Published announcement {AnnouncementId} by user {UserId}", announcement.Id, currentUserId);

        return _mapper.Map<Response>(announcement);
    }

    public async Task<Response> UnpublishAsync(Guid announcementId, Guid currentUserId)
    {
        var announcement = await _unitOfWork.Announcements.GetByIdWithDetailsAsync(announcementId, asNoTracking: false);

        if (announcement is null)
            throw new NotFoundException(nameof(Announcement), announcementId);

        var state = GetStateFor(announcement.Status);
        await state.UnpublishAsync(announcement);
        _unitOfWork.Announcements.Update(announcement);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Unpublished announcement {AnnouncementId} by user {UserId}", announcement.Id, currentUserId);

        return _mapper.Map<Response>(announcement);
    }

    public async Task<bool> ArchiveAsync(Guid announcementId, Guid currentUserId)
    {
        var announcement = await _unitOfWork.Announcements.GetByIdAsync(announcementId);

        if (announcement is null)
            throw new NotFoundException(nameof(Announcement), announcementId);

        var state = GetStateFor(announcement.Status);
        await state.ArchiveAsync(announcement);
        _unitOfWork.Announcements.Update(announcement);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Archived announcement {AnnouncementId} by user {UserId}", announcement.Id, currentUserId);

        return true;
    }

    private IAnnouncementState GetStateFor(ContentStatus status)
    {
        return status switch
        {
            ContentStatus.Draft => new DraftState(),
            ContentStatus.Published => new PublishedState(),
            ContentStatus.Unpublished => new UnpublishedState(),
            ContentStatus.Archived => new ArchivedState(),
            _ => throw new InvalidOperationException($"Unknown content status {status}")
        };
    }
}
