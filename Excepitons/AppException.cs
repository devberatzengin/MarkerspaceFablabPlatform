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
