using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Data.Interfaces;

public interface IEquipmentRepository : IRepository<Equipment>
{
    // işte burdan dicez ki ilgilli equipment'ın kime gideceğini belirlicez falan 
    Task<User?> GetEquipmentsUser(Equipment equipment, CancellationToken cancellationToken = default);
    
    // add update remove var zaten
    // get all , Get by id de var
    
    // Bir equipment kiralanabilir, rezerve edilebilir, arızasi bildirilebilir?, geri teslim edilebilir.
    // Kim tarafından teslim alındı ? 
    
}