using DogWalking.Application.Interfaces;
using DogWalking.Infrastructure.Extensions;
using DogWalking.WinForms.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

using App = System.Windows.Forms.Application;

namespace DogWalking.WinForms;

static class Program
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Configuration
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        // Serilog — console + rolling file
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .CreateLogger();

        try
        {
            // DI container
            var services = new ServiceCollection();
            services.AddLogging(lb => lb.AddSerilog());
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

            ServiceProvider.InitializeDatabaseAsync()
                .GetAwaiter()
                .GetResult();

            // Start LAN notification listener
            var notifier = ServiceProvider.GetRequiredService<INotificationService>();
            notifier.StartAsync()
                .GetAwaiter()
                .GetResult();

            Log.Information("Application started");
            App.Run(ServiceProvider.GetRequiredService<LoginForm>());

            // Clean shutdown
            notifier.StopAsync()
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
