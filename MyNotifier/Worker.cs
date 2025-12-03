using CommunityToolkit.WinUI.Notifications;

namespace MyNotifier
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }

                new ToastContentBuilder()
                    .AddText("站起来走两步！")
                    .AddText($"Worker running at: {DateTimeOffset.Now}")
                    ;

                await Task.Delay(40 * 60 * 1000, stoppingToken);
            }
        }
    }
}
