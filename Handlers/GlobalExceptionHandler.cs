using MarkerspaceFablabPlatform.Excepitons;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MarkerspaceFablabPlatform.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // exception tipi → HTTP status eşlemesi
        var (status, title) = exception switch
        {
            NotFoundException     => (StatusCodes.Status404NotFound, "Not Found"),
            ValidationException   => (StatusCodes.Status400BadRequest, "Bad Request"),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ForbiddenException    => (StatusCodes.Status403Forbidden, "Forbidden"),
            ConflictException     => (StatusCodes.Status409Conflict, "Conflict"),
            OperationCanceledException => (StatusCodes.Status499ClientClosedRequest, "Operation canceled"),
            _                     => (StatusCodes.Status500InternalServerError, "Server Error")
        };

        // 500'leri Error, beklenen hataları Warning logla
        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Beklenmeyen hata: {Message}", exception.Message);
        else
            _logger.LogWarning("Handled exception ({Status}): {Message}", status, exception.Message);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            // GÜVENLİK: 500'de iç detayı sızdırma!
            Detail = status == 500 ? "Beklenmeyen bir hata oluştu." : exception.Message,
            Instance = httpContext.Request.Path
        };

        // ValidationException ise alan hatalarını ekle
        if (exception is ValidationException vex && vex.Errors.Any())
            problem.Extensions["errors"] = vex.Errors;

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true; // "ben hallettim", pipeline'da başka handler aranmaz
    }
}
