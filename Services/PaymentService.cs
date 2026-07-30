using MakerspaceFablabPlatform.Dtos.Common;
using MakerspaceFablabPlatform.Dtos.Payment;
using MakerspaceFablabPlatform.Services.Interfaces;

namespace MakerspaceFablabPlatform.Services;

public class PaymentService : IPaymentService
{
    public Task<Response> CreateAsync(CreateRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<PagedResponse<Response>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<PagedResponse<Response>> GetAllByUserIdAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<Response> GetByIdAsync(Guid paymentId)
    {
        throw new NotImplementedException();
    }
}