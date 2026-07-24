using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Dtos.Category;
using MakerspaceFablabPlatform.Entitys;
using MakerspaceFablabPlatform.Excepitons;
using MakerspaceFablabPlatform.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ValidationException = MakerspaceFablabPlatform.Excepitons.ValidationException;


namespace MakerspaceFablabPlatform.Services;

public class CategoryService : ICategoryService
{
    
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CategoryService> _logger;
    private readonly IValidator<CreateRequest> _createValidator;
    private readonly IValidator<UpdateRequest> _updateValidator;
    public CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger, IValidator<CreateRequest> createValidator, IValidator<UpdateRequest> updateValidator)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }
    
    
    public async Task<Response> CreateAsync(CreateRequest createRequest)
    {
        
        var validation = await _createValidator.ValidateAsync(createRequest);

        if (!validation.IsValid)
            throw new ValidationException(validation.ToDictionary());
        
        
        bool nameExist = await _categoryRepository.NameExistsAsync(createRequest.Name);
        
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
        
        await _categoryRepository.AddAsync(newCategory);
        await _categoryRepository.SaveChangesAsync();
        
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
        var categories = await _categoryRepository.Query()
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
        var category = await _categoryRepository.Query()                     // salt okuma (AsNoTracking varsayılan)
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
        
        var category = await _categoryRepository.GetByIdAsync(updateRequest.Id);

        if (category is null)
            throw new NotFoundException(nameof(Category), updateRequest.Id);
        
        bool nameExist = await _categoryRepository.NameExistsAsync(updateRequest.Name, updateRequest.Id);

        if (nameExist)
            throw new ConflictException($"'{updateRequest.Name}' adında kategori zaten var.");

        category.Name = updateRequest.Name;
        category.Type = updateRequest.Type;
        category.IsActive = updateRequest.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await _categoryRepository.SaveChangesAsync();

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
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        
        if (category is null)
            throw new NotFoundException(nameof(Category), categoryId);
        
        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;

        await _categoryRepository.SaveChangesAsync();

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
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        
        if (category is null)
            throw new NotFoundException(nameof(Category), categoryId);
            
        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;
        await _categoryRepository.SaveChangesAsync();

        _logger.LogInformation("Deleted category {CategoryId}", category.Id);

        return true;
    }
}
