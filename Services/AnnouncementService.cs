using MarkerspaceFablabPlatform.Data;
using MarkerspaceFablabPlatform.Dtos.Announcement;
using MarkerspaceFablabPlatform.Dtos.Common;
using MarkerspaceFablabPlatform.Entitys;
using MarkerspaceFablabPlatform.Entitys.Enums;
using MarkerspaceFablabPlatform.Excepitons;
using MarkerspaceFablabPlatform.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ValidationException = MarkerspaceFablabPlatform.Excepitons.ValidationException;

namespace MarkerspaceFablabPlatform.Services;

public class AnnouncementService : IAnnouncementService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<AnnouncementService> _logger;
    private readonly IValidator<CreateRequest> _createValidator;
    private readonly IValidator<UpdateRequest> _updateValidator;
    
    
    public AnnouncementService(AppDbContext dbContext, ILogger<AnnouncementService> logger, IValidator<CreateRequest> createValidator, IValidator<UpdateRequest> updateValidator)
    {
        _logger = logger;
        _dbContext = dbContext;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Response?> GetByIdAsync(Guid announcementId, bool isAdmin)
    {
        var result = await _dbContext.Announcements
            .Include(a => a.Category)
            .Include(a => a.CreatedBy)
            .FirstOrDefaultAsync(a => a.Id == announcementId);

        if (result is null)
            throw new NotFoundException(nameof(Announcement), announcementId);

        if (!isAdmin && result.Status != ContentStatus.Published)
            throw new NotFoundException(nameof(Announcement), announcementId);

        return ToResponse(result);
    }

    public async Task<PagedResponse<Response>> GetAllAsync(ListRequest request, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _dbContext.Announcements.AsNoTracking().AsQueryable();

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
            .Select(a => new Response
            {
                Id = a.Id,
                Title = a.Title,
                Content = a.Content,
                CreatedByUserId = a.CreatedByUserId,
                CreatedByName = a.CreatedBy.FirstName + " " + a.CreatedBy.LastName,
                CategoryName = a.Category.Name,
                CategoryId = a.CategoryId,
                Status = a.Status,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
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
        
        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId);
        
        if (category is null)
            throw new NotFoundException(nameof(Category), request.CategoryId);
        
        var nameCount = await _dbContext.Announcements
            .CountAsync(a => a.Title == request.Title || a.Title.StartsWith(request.Title + " "));
        
        var title = nameCount > 0 ? $"{request.Title} {nameCount + 1}" : request.Title;
        
        var creator = await _dbContext.Users
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
        
        _dbContext.Announcements.Add(newAnnouncement);
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Created announcement {AnnouncementId} with title {Title} by user {UserId}", newAnnouncement.Id, title, currentUserId);
        
        return new Response()
        {
            Id = newAnnouncement.Id,
            Title = newAnnouncement.Title,
            Content = newAnnouncement.Content,
            CreatedByUserId = newAnnouncement.CreatedByUserId,
            CreatedByName = creator.FirstName + " " + creator.LastName,

            CategoryName = category.Name,
            CategoryId = newAnnouncement.CategoryId,
            Status = newAnnouncement.Status,
            CreatedAt = newAnnouncement.CreatedAt,
            UpdatedAt = newAnnouncement.UpdatedAt
        };

    }



    public async Task<Response?> UpdateAsync(UpdateRequest request, Guid currentUserId, bool isAdmin)
    {
        var validation = await _updateValidator.ValidateAsync(request);
        
        if (!validation.IsValid)
            throw new ValidationException(validation.ToDictionary());

        var announcement = await _dbContext.Announcements
            .Include(a => a.Category)
            .Include(a => a.CreatedBy)
            .FirstOrDefaultAsync(a => a.Id == request.Id);

        if (announcement is null)
            throw new NotFoundException(nameof(Announcement), request.Id);
        
        
        
        // Admin değilse yalnızca kendi duyurusunu düzenleyebilir
        if (!isAdmin && announcement.CreatedByUserId != currentUserId)
            throw new ForbiddenException("Bu duyuruyu düzenleme yetkiniz yok.");
        
        var categoryExists = await _dbContext.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
            throw new NotFoundException(nameof(Category), request.CategoryId);
        
        var nameCount = await _dbContext.Announcements
            .CountAsync(a => a.Id != announcement.Id &&
                (a.Title == request.Title || a.Title.StartsWith(request.Title + " ")));
        
        var title = nameCount > 0 ? $"{request.Title} {nameCount + 1}" : request.Title;
        
        announcement.Title = title;
        announcement.Content = request.Content;
        announcement.CategoryId = request.CategoryId;
        announcement.UpdatedAt = DateTime.UtcNow;
        
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Updated announcement {AnnouncementId} with title {Title} by user {UserId}", announcement.Id, title, currentUserId);

        return ToResponse(announcement);
    }
    
    public async Task<Response?> PublishAsync(Guid announcementId, Guid currentUserId)
    {
        var announcement = await _dbContext.Announcements
            .Include(a => a.Category)
            .Include(a => a.CreatedBy)
            .FirstOrDefaultAsync(a => a.Id == announcementId);
        
        if (announcement is null)
            throw new NotFoundException(nameof(Announcement), announcementId);

        if (announcement.Status == ContentStatus.Published)
            throw new ConflictException("Duyuru zaten yayında.");
        if (announcement.Status == ContentStatus.Archived)
            throw new ConflictException("Arşivlenmiş duyuru yayına alınamaz.");

        announcement.Status = ContentStatus.Published;
        announcement.UpdatedAt = DateTime.UtcNow;
        
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Published announcement {AnnouncementId} by user {UserId}", announcement.Id, currentUserId);

        return ToResponse(announcement);
    }

    public async Task<Response?> UnpublishAsync(Guid announcementId, Guid currentUserId)
    {
        var announcement = await _dbContext.Announcements
            .Include(a => a.Category)
            .Include(a => a.CreatedBy)
            .FirstOrDefaultAsync(a => a.Id == announcementId);
        
        if (announcement is null)
            throw new NotFoundException(nameof(Announcement), announcementId);

        if (announcement.Status != ContentStatus.Published)
            throw new ConflictException("Yayında olmayan duyuru yayından kaldırılamaz.");

        announcement.Status = ContentStatus.Passive;
        announcement.UpdatedAt = DateTime.UtcNow;
        
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Unpublished announcement {AnnouncementId} by user {UserId}", announcement.Id, currentUserId);

        return ToResponse(announcement);
    }

    public async Task<bool> ArchiveAsync(Guid announcementId, Guid currentUserId)
    {
        var announcement = await _dbContext.Announcements.FirstOrDefaultAsync(a => a.Id == announcementId);

        if (announcement is null)
            throw new NotFoundException(nameof(Announcement), announcementId);

        announcement.Status = ContentStatus.Archived;
        announcement.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Archived announcement {AnnouncementId} by user {UserId}", announcement.Id, currentUserId);

        return true;
    }

    private static Response ToResponse(Announcement a) => new()
    {
        Id = a.Id,
        Title = a.Title,
        Content = a.Content,
        CreatedByUserId = a.CreatedByUserId,
        CreatedByName = a.CreatedBy.FirstName + " " + a.CreatedBy.LastName,
        CategoryName = a.Category.Name,
        CategoryId = a.CategoryId,
        Status = a.Status,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };
}