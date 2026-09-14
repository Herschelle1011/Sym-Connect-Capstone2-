using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sym_Connect__Capstone2_.Services.Interfaces;

namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class EventReminderBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EventReminderBackgroundService> _logger;

        public EventReminderBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<EventReminderBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var eventService =
                        scope.ServiceProvider.GetRequiredService<IEventService>();

                    await eventService.CheckPendingEventRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "An error occurred while checking event reminders.");
                }

                // Check every 5 minutes
                await Task.Delay(
                    TimeSpan.FromSeconds(20),
                    stoppingToken);
            }
        }
    }
}