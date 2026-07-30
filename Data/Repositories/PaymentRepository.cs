using System.Linq.Expressions;
using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities;

namespace MakerspaceFablabPlatform.Data.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(IApplicationDbContext dbContext) : base(dbContext)
    {
        
    }
    
    
    
}