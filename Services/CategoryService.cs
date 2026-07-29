using AutoMapper;
using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Dtos.Category;
using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Excepitons;
using MakerspaceFablabPlatform.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ValidationException = MakerspaceFablabPlatform.Excepitons.ValidationException;


namespace MakerspaceFablabPlatform.Services;

public class CategoryService : ICategoryService
{
    
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CategoryService> _logger;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateRequest> _createValidator;
    private readonly IValidator<UpdateRequest> _updateValidator;
    public CategoryService(IUnitOfWork unitOfWork, ICategoryRepository categoryRepository, ILogger<CategoryService> logger, IMapper mapper, IValidator<CreateRequest> createValidator, IValidator<UpdateRequest> updateValidator)
    {
        _unitOfWork = unitOfWork;
        _categoryRepository = categoryRepository;
        _logger = logger;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }
    
    
    public async Task<Response> CreateAsync(CreateRequest createRequest)
    {
        
        var validation = await _createValidator.ValidateAsync(createRequest);
        
        if (!validation.IsValid)
            throw new ValidationException(validation.ToDictionary());
        
        
        bool nameExist = await _unitOfWork.Categories.NameExistsAsync(createRequest.Name);
        
        if (nameExist)
            throw new DuplicateEntityException($"'{createRequest.Name}' adında kategori zaten var.");
        
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
        
        await _unitOfWork.Categories.AddAsync(newCategory);
        await _unitOfWork.Categories.SaveChangesAsync();
        
        _logger.LogInformation("Created category {CategoryId} with name {Name}", newCategory.Id, newCategory.Name);

        return _mapper.Map<Response>(newCategory);
        
    }

    public async Task<List<Response>> GetAllAsync(bool includeUnactivated = false)
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();

        if (!includeUnactivated)
            categories = categories.Where(c => c.IsActive).ToList();

        return _mapper.Map<List<Response>>(categories);
    }

    public async Task<Response?> GetByIdAsync(Guid categoryId, bool includeUnactivated = false)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);

        if (category is null)
            throw new NotFoundException(nameof(Category), categoryId);

        if (!includeUnactivated && !category.IsActive)
            throw new NotFoundException(nameof(Category), categoryId);

        return _mapper.Map<Response>(category);
    }

    public async Task<Response?> UpdateAsync(UpdateRequest updateRequest)
    {
        var validation = await _updateValidator.ValidateAsync(updateRequest);
        
        if (!validation.IsValid)
            throw new ValidationException(validation.ToDictionary());
        
        var category = await _unitOfWork.Categories.GetByIdAsync(updateRequest.Id);

        if (category is null)
            throw new NotFoundException(nameof(Category), updateRequest.Id);
        
        bool nameExist = await _unitOfWork.Categories.NameExistsAsync(updateRequest.Name, updateRequest.Id);

        if (nameExist)
            throw new DuplicateEntityException($"'{updateRequest.Name}' adında kategori zaten var.");

        category.Name = updateRequest.Name;
        category.Type = updateRequest.Type;
        category.IsActive = updateRequest.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.Categories.SaveChangesAsync();

        _logger.LogInformation("Updated category {CategoryId}", category.Id);

        return _mapper.Map<Response>(category);
    }

    public async Task<Response?> DeactivateAsync(Guid categoryId)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
        
        if (category is null)
            throw new NotFoundException(nameof(Category), categoryId);
        
        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.Categories.SaveChangesAsync();

        _logger.LogInformation("Deactivated category {CategoryId}", category.Id);

        return _mapper.Map<Response>(category);

    }

    public async Task<bool> DeleteAsync(Guid categoryId)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
        
        if (category is null)
            throw new NotFoundException(nameof(Category), categoryId);
            
        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;
        
        _unitOfWork.Categories.Update(category);
        await _unitOfWork.Categories.SaveChangesAsync();

        _logger.LogInformation("Deleted category {CategoryId}", category.Id);

        return true;
    }
}
