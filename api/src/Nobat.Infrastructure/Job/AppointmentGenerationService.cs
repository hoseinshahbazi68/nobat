using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Nobat.Application.Appointments;

namespace Nobat.Infrastructure.Job;

/// <summary>
/// سرویس پس‌زمینه برای تولید خودکار نوبت‌ها
/// هر روز یکبار نوبت‌های 30 روز آینده را تولید می‌کند
/// </summary>
public class AppointmentGenerationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AppointmentGenerationService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromDays(1); // هر 24 ساعت یکبار
    private readonly int _daysAhead = 30; // تولید نوبت برای 30 روز آینده

    public AppointmentGenerationService(
        IServiceProvider serviceProvider,
        ILogger<AppointmentGenerationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Appointment Generation Service started");

        // اجرای اولیه بلافاصله پس از راه‌اندازی
        await GenerateAppointments(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // انتظار برای interval بعدی
                await Task.Delay(_checkInterval, stoppingToken);

                // تولید نوبت‌ها
                await GenerateAppointments(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // در صورت لغو، از حلقه خارج شو
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating appointments");
            }
        }

        _logger.LogInformation("Appointment Generation Service stopped");
    }

    private async Task GenerateAppointments(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var appointmentService = scope.ServiceProvider.GetRequiredService<IAppointmentService>();

        try
        {
            var startDate = DateTime.Today;
            var endDate = startDate.AddDays(_daysAhead);

            _logger.LogInformation("Starting appointment generation from {StartDate} to {EndDate}", startDate, endDate);

            var response = await appointmentService.GenerateAppointmentsAsync(startDate, endDate, cancellationToken);

            if (response.Status)
            {
                _logger.LogInformation(
                    "Appointment generation completed successfully. Created {Count} appointments from {StartDate} to {EndDate}",
                    response.Data, startDate, endDate);
            }
            else
            {
                _logger.LogWarning("Appointment generation failed: {Message}", response.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GenerateAppointments method");
        }
    }
}
