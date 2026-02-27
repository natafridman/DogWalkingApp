using DogWalking.Application.Interfaces;
using DogWalking.Infrastructure.Extensions;
using DogWalking.WinForms.Forms;
using Microsoft.Extensions.DependencyInjection;

using App = System.Windows.Forms.Application;

namespace DogWalking.WinForms;

/// <summary>
/// Application entry point. Sets up the DI container, applies migrations,
/// starts the LAN notification listener, and launches the login form.
/// </summary>
static class Program
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    const string conn = "Server=localhost;Database=DogWalkingApp;Trusted_Connection=True;TrustServerCertificate=True";

    [STAThread]
    static async Task Main()
    {
        ApplicationConfiguration.Initialize();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDogWalkingServices(conn);

        // WinForms — Transient so each form is a fresh instance
        services.AddTransient<LoginForm>();
        services.AddTransient<MainForm>();
        services.AddTransient<DogDialog>();
        services.AddTransient<WalkEventForm>();

        ServiceProvider = services.BuildServiceProvider();

        await ServiceProvider.InitializeDatabaseAsync();

        // Start LAN notification listener
        var notifier = ServiceProvider.GetRequiredService<INotificationService>();
        await notifier.StartAsync();

        App.Run(ServiceProvider.GetRequiredService<LoginForm>());

        // Clean shutdown
        await notifier.StopAsync();
    }
}