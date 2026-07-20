using WaqfENau.Api.Infrastructure.Interfaces.Services;

namespace WaqfENau.Api.WorkerServices
{
    public class DailyWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DailyWorker> _logger;

        public DailyWorker(IServiceProvider serviceProvider, ILogger<DailyWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Daily Worker started at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var nextRun = now.Date.AddDays(1).AddHours(2); // 2 AM UTC daily
                var delay = nextRun - now;

                _logger.LogInformation("Next run scheduled at: {time}", nextRun);
                await Task.Delay(delay, stoppingToken);

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var worker = scope.ServiceProvider.GetRequiredService<IBackgroundWorkerService>();

                    _logger.LogInformation("Running background tasks...");

                    await worker.CheckInactiveMembersAsync();
                    await worker.ResetBrokenStreaksAsync();
                    await worker.UpdateLeaderboardRanksAsync();

                    _logger.LogInformation("Background tasks completed successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error running background tasks.");
                }
            }
        }
    }
}
