namespace MakerspaceFablabPlatform.Excepitons;

public class AppException : Exception
{
    protected AppException(string message) : base(message)
    {
    }
}

//404
public class NotFoundException : AppException
{
    public NotFoundException(string name, object key) : base($"{name} not found. (Id: {key})") { }
}

// 400
public class ValidationException : AppException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(string message) : base(message)
        => Errors = new Dictionary<string, string[]>();

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Bir veya daha fazla doğrulama hatası oluştu.")
        => Errors = errors;
}

// 401 — kimlik belirsiz (token yok/geçersiz)
public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Kimlik doğrulaması gerekli.")
        : base(message) { }
}

// 403 — kimlik belli ama yetki yok
public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "Bu işlem için yetkiniz yok.")
        : base(message) { }
}
// 409 - Conflict 
public class ConflictException : AppException
{
    public ConflictException(string message) : base(message) { }
}

public class EquipmentNotAvailableException : ConflictException
{
    public EquipmentNotAvailableException(string message = "Bu ekipman şu an müsait değil.") : base(message) { }
}

public class EquipmentNotPortableException : ConflictException
{
    public EquipmentNotPortableException(string message = "Bu ekipman bu işlem için uygun yerleşim tipinde değil.") : base(message) { }
}

public class EquipmentNotRentedException : ConflictException
{
    public EquipmentNotRentedException(string message = "Bu ekipman şu an kiralı değil.") : base(message) { }
}
public class RentalLimitExceededException : ConflictException
{
    public RentalLimitExceededException(string message = "İzin verilen maksimum aktif kiralama sayısına ulaştınız.") : base(message) { }
}

public class InsufficientEquipmentLevelException : ForbiddenException
{
    public InsufficientEquipmentLevelException(string message = "Bu ekipmanı kullanmak için gereken seviyeye sahip değilsiniz.") : base(message) { }
}

public class NotResourceOwnerException : ForbiddenException
{
    public NotResourceOwnerException(string message = "Bu kaynak üzerinde işlem yapma yetkiniz yok.") : base(message) { }
}
public class InvalidStateTransitionException : ConflictException
{
    public InvalidStateTransitionException(string message) : base(message) { }
}

public class InvalidCredentialsException : UnauthorizedException
{
    public InvalidCredentialsException(string message = "Email veya şifre hatalı.") : base(message) { }
}

public class InvalidCurrentPasswordException : ConflictException
{
    public InvalidCurrentPasswordException(string message = "Mevcut şifre hatalı.") : base(message) { }
}

public class DuplicateEntityException : ConflictException
{
    public DuplicateEntityException(string message) : base(message) { }
}

public class InsufficientBalanceException : ConflictException
{
    public InsufficientBalanceException(decimal required, decimal available)
        : base($"Yetersiz bakiye. Gereken: {required:0.00} ₺, mevcut: {available:0.00} ₺.") { }
}

public class ConcurrencyConflictException : ConflictException
{
    public ConcurrencyConflictException(string message = "Bu kayıt başka bir işlem tarafından aynı anda güncellendi. Lütfen tekrar deneyin.") : base(message) { }
}
