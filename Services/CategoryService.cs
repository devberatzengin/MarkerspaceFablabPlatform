using MarkerspaceFablabPlatform.Data;
using MarkerspaceFablabPlatform.Dtos.Category;
using MarkerspaceFablabPlatform.Entitys;
using MarkerspaceFablabPlatform.Entitys.Enums;
using MarkerspaceFablabPlatform.Excepitons;
using MarkerspaceFablabPlatform.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ValidationException = MarkerspaceFablabPlatform.Excepitons.ValidationException;


namespace MarkerspaceFablabPlatform.Services;

public class CategoryService : ICategoryService
{
    
    private readonly AppDbContext _dbContext;
    private readonly ILogger<CategoryService> _logger;
    private readonly IValidator<CreateRequest> _createValidator;
    private readonly IValidator<UpdateRequest> _updateValidator;
    public CategoryService(AppDbContext dbContext, ILogger<CategoryService> logger, IValidator<CreateRequest> createValidator, IValidator<UpdateRequest> updateValidator)
    {
        _dbContext = dbContext;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }
    
    
    public async Task<Response> CreateAsync(CreateRequest createRequest)
    {
        
        var validation = await _createValidator.ValidateAsync(createRequest);

        if (!validation.IsValid)
            throw new ValidationException(validation.ToDictionary());
        
        
        bool nameExist = await _dbContext.Categories
            .AnyAsync(c => c.Name == createRequest.Name);
        
        if (nameExist)
            throw new ConflictException($"'{createRequest.Name}' adında kategori zaten var.");
        
        Category newCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = createRequest.Name,
            Type = createRequest.Type,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _dbContext.Categories.Add(newCategory);
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Created category {CategoryId} with name {Name}", newCategory.Id, newCategory.Name);

        return new Response
        {
            Id = newCategory.Id,
            Name = newCategory.Name,
            Type = newCategory.Type,
            IsActive = newCategory.IsActive
        };
        
    }

    public async Task<List<Response>> GetAllAsync(bool includeUnactivated = false)
    {
        var categories =  await _dbContext.Categories
            .Where(c => includeUnactivated || c.IsActive)
            .ToListAsync();

        var responses = new List<Response>();
        foreach (var category in categories)
        {
            responses.Add(new Response
            {
                Id = category.Id,
                Name = category.Name,
                Type = category.Type,
                IsActive = category.IsActive
            });
        }
        
        return responses;
        
    }
    
    public async Task<Response?> GetByIdAsync(Guid categoryId,bool includeUnactivated = false)
    {
        var category = await _dbContext.Categories
            .AsNoTracking()                                        // salt okuma
            .Where(c => c.Id == categoryId)
            .Where(c => includeUnactivated || c.IsActive)          // pasifler dahil mi?
            .FirstOrDefaultAsync();  
        
        if (category is null)
            throw new NotFoundException(nameof(Category), categoryId);        
        
        return new Response()
        {
            Id = category.Id,
            Name = category.Name,
            Type = category.Type,
            IsActive = category.IsActive
        };

    }

    public async Task<Response?> UpdateAsync(UpdateRequest updateRequest)
    {
        var validation = await _updateValidator.ValidateAsync(updateRequest);
        
        if (!validation.IsValid)
            throw new ValidationException(validation.ToDictionary());
        
        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == updateRequest.Id);

        if (category is null)
            throw new NotFoundException(nameof(Category), updateRequest.Id);
        
        bool nameExist = await _dbContext.Categories.AnyAsync(c => c.Id != updateRequest.Id && c.Name == updateRequest.Name);

        if (nameExist)
            throw new ConflictException($"'{updateRequest.Name}' adında kategori zaten var.");

        category.Name = updateRequest.Name;
        category.Type = updateRequest.Type;
        category.IsActive = updateRequest.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Updated category {CategoryId}", category.Id);

        return new Response()
        {
            Id = category.Id,
            Name = category.Name,
            Type = category.Type,
            IsActive = category.IsActive
        };
    }

    public async Task<Response?> DeactivateAsync(Guid categoryId)
    {
        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == categoryId); 
        
        if (category is null)
            throw new NotFoundException(nameof(Category), categoryId);
        
        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Deactivated category {CategoryId}", category.Id);

        return new Response()
        {
            Id = category.Id,
            Name = category.Name,
            Type = category.Type,
            IsActive = category.IsActive
        };

    }

    public async Task<bool> DeleteAsync(Guid categoryId)
    {
        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
        
        if (category is null)
            throw new NotFoundException(nameof(Category), categoryId);
            
        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Deleted category {CategoryId}", category.Id);

        return true;
    }
}