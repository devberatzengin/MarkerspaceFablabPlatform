using MakerspaceFablabPlatform.Dtos.Common;
using MakerspaceFablabPlatform.Dtos.Payment;


namespace MakerspaceFablabPlatform.Services.Interfaces;

public interface IPaymentService
{
    public Task<Response> CreateAsync(CreateRequest request);
    
    //Admin Method
    //
    public Task<PagedResponse<Response>> GetAllAsync(); 
    
    // Kendi receipt'lerini alman için
    // Admin isen istediğin birisinikini almak için
    // login olan herkes /my-payments
    public Task<PagedResponse<Response>> GetAllByUserIdAsync(Guid userId);
    
    // Belirli bir fişi almak için işte login olan herkes
    public Task<Response> GetByIdAsync(Guid paymentId);
    
}