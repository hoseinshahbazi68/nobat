using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Nobat.Application.Chat;

namespace Nobat.Infrastructure.Job;

/// <summary>
/// سرویس پس‌زمینه برای حذف خودکار پیام‌های قدیمی چت
/// هر 24 ساعت یکبار پیام‌های قدیمی‌تر از 3 روز را حذف می‌کند
/// </summary>
public class ChatCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ChatCleanupService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // هر 24 ساعت یکبار

    public ChatCleanupService(
        IServiceProvider serviceProvider,
        ILogger<ChatCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Chat Cleanup Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredMessages(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up expired chat messages");
            }

            // انتظار برای interval بعدی
            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CleanupExpiredMessages(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var chatService = scope.ServiceProvider.GetRequiredService<IChatService>();

        _logger.LogInformation("Starting cleanup of expired chat messages...");

        var response = await chatService.DeleteExpiredMessagesAsync(cancellationToken);

        if (response.Status)
        {
            _logger.LogInformation("Cleanup completed. Deleted {Count} expired messages", response.Data);
        }
        else
        {
            _logger.LogWarning("Cleanup failed: {Message}", response.Message);
        }
    }
}
