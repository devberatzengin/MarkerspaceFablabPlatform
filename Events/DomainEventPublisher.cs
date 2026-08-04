namespace MakerspaceFablabPlatform.Events;


// Bu bizim ISubjectimiz yani dinleyicilerimizi falan tutan taraf
public class DomainEventPublisher : IDomainEventPublisher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventPublisher> _logger;

    public DomainEventPublisher(IServiceProvider serviceProvider, ILogger<DomainEventPublisher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        var handlers = _serviceProvider.GetServices<IDomainEventHandler<TEvent>>().ToList();

        if (handlers.Count == 0)
        {
            _logger.LogWarning("{EventName} için kayıtlı handler bulunamadı", typeof(TEvent).Name);
            return;
        }

        foreach (var handler in handlers)
        {
            try
            {
                await handler.HandleAsync(domainEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{HandlerName} çalışırken hata aldı. Olay: {EventName}",
                    handler.GetType().Name, typeof(TEvent).Name);
            }
        }
    }
}
