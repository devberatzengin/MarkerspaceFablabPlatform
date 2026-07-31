using AutoMapper;
using PaymentEntity = MakerspaceFablabPlatform.Entities.Payment;
using PaymentResponse = MakerspaceFablabPlatform.Dtos.Payment.Response;

namespace MakerspaceFablabPlatform.Profiles;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<PaymentEntity, PaymentResponse>();
    }
}
