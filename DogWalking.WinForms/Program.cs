using DogWalking.Application.Interfaces;
using DogWalking.Infrastructure.Extensions;
using DogWalking.WinForms.Forms;
using Microsoft.Extensions.DependencyInjection;

using App = System.Windows.Forms.Application;

namespace DogWalking.WinForms;

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