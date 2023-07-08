namespace awsDatabase.Services
{
    public class QueueMessageService : BackgroundService
    {
        private readonly ILogger<QueueMessageService> _logger;
        private readonly TimeSpan _delay = TimeSpan.FromSeconds(3);
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public QueueMessageService(
            ILogger<QueueMessageService> logger,
            IServiceScopeFactory serviceScopeFactory
            )
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("QueueMessageService Hosted Service running.");
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    var messages = await notificationService.ReadMessages();
                    foreach (var message in messages)
                    {
                        await notificationService.SendMessageToTopic(message.Body.ToString());
                    }
                }
                await Task.Delay(_delay, stoppingToken);
            }
        }
    }
}
