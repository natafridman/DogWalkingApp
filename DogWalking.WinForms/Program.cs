using DogWalking.Application.Interfaces;
using DogWalking.Infrastructure.Extensions;
using DogWalking.WinForms.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using App = System.Windows.Forms.Application;

namespace DogWalking.WinForms;

/// <summary>
/// Application entry point. Loads configuration from appsettings.json,
/// sets up the DI container, applies migrations, starts the LAN notification
/// listener, and launches the login form.
/// </summary>
static class Program
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    [STAThread]
    static async Task Main()
    {
        ApplicationConfiguration.Initialize();

        // Configuration
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        // DI container
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDatabase(config);
        services.AddServices();
        services.AddCaching();
        services.AddNotifications();
        services.AddAI(config);

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
