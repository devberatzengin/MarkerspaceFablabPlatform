using MakerspaceFablabPlatform.Dtos.Common;
using MakerspaceFablabPlatform.Dtos.Payment;


namespace MakerspaceFablabPlatform.Services.Interfaces;

public interface IPaymentService
{
    // Ödemeyi oluşturur ve tutarı kullanıcının bakiyesinden düşer.
    // request.UserId doluysa ve currentUserId'den farklıysa isAdmin şart.
    public Task<Response> CreateAsync(CreateRequest request, Guid currentUserId, bool isAdmin, CancellationToken token = default);

    //Admin Method
    public Task<PagedResponse<Response>> GetAllAsync(ListRequest request, CancellationToken token = default);

    // Kendi receipt'lerini alman için
    // Admin isen istediğin birisinikini almak için
    // login olan herkes /my-payments
    public Task<PagedResponse<Response>> GetAllByUserIdAsync(Guid userId, ListRequest request, CancellationToken token = default);

    // Belirli bir fişi almak için, sahibi veya Admin
    public Task<Response> GetByIdAsync(Guid paymentId, Guid currentUserId, bool isAdmin, CancellationToken token = default);

    // Teslim edilmiş ama ödenmemiş kiralamalar, hesaplanmış tutarlarıyla
    public Task<PagedResponse<PendingResponse>> GetPendingAsync(Guid userId, ListRequest request, CancellationToken token = default);

    // Tek bir kiralama için ödeme önizlemesi (kaydetmez), sahibi veya Admin
    public Task<PendingResponse> GetPreviewAsync(Guid equipmentRentalId, Guid currentUserId, bool isAdmin, CancellationToken token = default);
}
