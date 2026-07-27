using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Entities;

namespace MakerspaceFablabPlatform.States.EquipmentStates;

public interface IEquipmentState
{
    
    // ilk Available ile başlar
    // Burdan reserved yada rented olur 
    // reserved yada rented 'den de ya maintenance olur yada available olur
    // Maintenance dan'da sadece available a göner
    
    
    
    //Available, relaize ile salarsam avaiable olur 
    //Reserved,
    //Rented,
    //Maintenance relaize ile saldım ama bir şey olmalı ki bu state'e düşmeli ama nasıl
    //basic bir bool kontrolümü yoksa acama belli bir count geçtikten sonra otomatik 2-5 saat aralığında bir bakım süresimi acaba ?_
    
    Task AvailableAsync(Equipment equipment, IUnitOfWork unitOfWork);
    Task ReservedAsync(Equipment equipment, IUnitOfWork unitOfWork);
    Task RentedAsync(Equipment equipment, IUnitOfWork unitOfWork);
    Task MaintenanceAsync(Equipment equipment, IUnitOfWork unitOfWork);
        
    
}