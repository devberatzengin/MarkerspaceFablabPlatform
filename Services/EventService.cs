using MarkerspaceFablabPlatform.Data;
using MarkerspaceFablabPlatform.Dtos.Common;
using MarkerspaceFablabPlatform.Dtos.Event;
using MarkerspaceFablabPlatform.Entitys;
using MarkerspaceFablabPlatform.Entitys.Enums;
using MarkerspaceFablabPlatform.Excepitons;
using MarkerspaceFablabPlatform.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ValidationException = MarkerspaceFablabPlatform.Excepitons.ValidationException;

namespace MarkerspaceFablabPlatform.Services;

public class EventService : IEventService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<EventService> _logger;
    private readonly IValidator<CreateRequest> _createValidator;
    private readonly IValidator<UpdateRequest> _updateValidator;

    public EventService(AppDbContext dbContext, ILogger<EventService> logger, IValidator<CreateRequest> createValidator, IValidator<UpdateRequest> updateValidator)
    {
        _dbContext = dbContext;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Response> CreateAsync(CreateRequest createRequest, Guid currentUserId)
    {
        var validation = await _createValidator.ValidateAsync(createRequest);

        if (!validation.IsValid)
            throw new ValidationException(validation.ToDictionary());

        var nameCount = await _dbContext.Events
            .CountAsync(e => e.Name == createRequest.Name || e.Name.StartsWith(createRequest.Name + " "));

        var title = nameCount > 0 ? $"{createRequest.Name} {nameCount + 1}" : createRequest.Name;

        var categoryExists = await _dbContext.Categories.AnyAsync(c => c.Id == createRequest.CategoryId);

        if (!categoryExists)
            throw new NotFoundException(nameof(Category), createRequest.CategoryId);

        if (createRequest.EndDate <= DateTime.UtcNow)
            throw new ValidationException("Bitiş tarihi geçmiş bir zaman olamaz.");
        if (createRequest.EndDate <= createRequest.StartDate)
            throw new ValidationException("Bitiş tarihi başlangıçtan önce olamaz.");

        Event newEvent = new Event()
        {
            Id = Guid.NewGuid(),
            Name = title,
            Description = createRequest.Description,
            Location = createRequest.Location,
            CategoryId = createRequest.CategoryId,
            CreatedByUserId = currentUserId,
            StartDate = createRequest.StartDate,
            EndDate = createRequest.EndDate,

            Status = ContentStatus.Draft, 
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Events.Add(newEvent);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created event {EventId} with name {Name}", newEvent.Id, newEvent.Name);

        return ToResponse(newEvent);
    }

    public async Task<PagedResponse<Response>> GetAllAsync(ListRequest request, bool isAdmin)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _dbContext.Events.AsNoTracking().AsQueryable();
        
        if (!isAdmin)
            query = query.Where(e => e.Status == ContentStatus.Published);
        else if (request.Status is not null)
            query = query.Where(e => e.Status == request.Status);

        if (request.CategoryId is not null)
            query = query.Where(e => e.CategoryId == request.CategoryId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search.Trim()}%";
            query = query.Where(e =>
                EF.Functions.ILike(e.Name, pattern) ||
                EF.Functions.ILike(e.Description, pattern));
        }

        if (request.StartFrom is not null)
            query = query.Where(e => e.StartDate >= request.StartFrom);
        if (request.StartTo is not null)
            query = query.Where(e => e.StartDate <= request.StartTo);

        // upcoming events
        var now = DateTime.UtcNow;
        if (request.Period == EventPeriod.Upcoming)
            query = query.Where(e => e.StartDate >= now);
        else if (request.Period == EventPeriod.Past)
            query = query.Where(e => e.EndDate < now);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(e => e.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new Response
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                Location = e.Location,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                CategoryId = e.CategoryId,
                Status = e.Status,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            })
            .ToListAsync();

        _logger.LogInformation("Listed {Count}/{Total} events (page {Page})", items.Count, totalCount, page);

        return new PagedResponse<Response>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Response?> GetByIdAsync(Guid eventId, bool isAdmin)
    {
        var result = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == eventId);

        if (result is null)
            throw new NotFoundException(nameof(Event), eventId);

        if (!isAdmin && result.Status != ContentStatus.Published)
            throw new NotFoundException(nameof(Event), eventId);

        return ToResponse(result);
    }

    public async Task<Response?> UpdateAsync(UpdateRequest request, Guid currentUserId)
    {
        var validation = await _updateValidator.ValidateAsync(request);

        if (!validation.IsValid)
            throw new ValidationException(validation.ToDictionary());

        var result = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == request.Id);

        if (result is null)
            throw new NotFoundException(nameof(Event), request.Id);

        var categoryExists = await _dbContext.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
            throw new NotFoundException(nameof(Category), request.CategoryId);

        if (request.EndDate <= request.StartDate)
            throw new ValidationException("Bitiş tarihi başlangıçtan önce olamaz.");

        result.Name = request.Name;
        result.Description = request.Description;
        result.Location = request.Location;
        result.StartDate = request.StartDate;
        result.EndDate = request.EndDate;
        result.CategoryId = request.CategoryId;
        result.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Updated event {EventId} by user {UserId}", result.Id, currentUserId);

        return ToResponse(result);
    }

    public async Task<Response?> PublishAsync(Guid eventId, Guid currentUserId)
    {
        var result = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (result is null)
            throw new NotFoundException(nameof(Event), eventId);

        if (result.Status == ContentStatus.Published)
            throw new ConflictException("Event already published");
        if (result.Status == ContentStatus.Archived)
            throw new ConflictException("Event cannot publish if it is already archived");

        result.Status = ContentStatus.Published;
        result.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Published event {EventId} by user {UserId}", result.Id, currentUserId);

        return ToResponse(result);
    }

    public async Task<Response?> UnpublishAsync(Guid eventId, Guid currentUserId)
    {
        var result = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == eventId);

        if (result is null)
            throw new NotFoundException(nameof(Event), eventId);

        if (result.Status != ContentStatus.Published)
            throw new ConflictException("Unpublished event can not uunpublish again.");

        result.Status = ContentStatus.Passive;
        result.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Unpublished event {EventId} by user {UserId}", result.Id, currentUserId);

        return ToResponse(result);
    }

    public async Task<bool> ArchiveAsync(Guid eventId, Guid currentUserId)
    {
        var result = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == eventId);

        if (result is null)
            throw new NotFoundException(nameof(Event), eventId);

        result.Status = ContentStatus.Archived;
        result.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Archived event {EventId} by user {UserId}", result.Id, currentUserId);

        return true;
    }

    private static Response ToResponse(Event e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description,
        Location = e.Location,
        StartDate = e.StartDate,
        EndDate = e.EndDate,
        CategoryId = e.CategoryId,
        Status = e.Status,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };
}
