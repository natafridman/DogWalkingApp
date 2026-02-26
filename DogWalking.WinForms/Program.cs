using DogWalking.Infrastructure.Extensions;
using DogWalking.WinForms.Forms;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddDogWalkingServices(conn);

        // WinForms — Transient so each form is a fresh instance
        services.AddTransient<LoginForm>();
        services.AddTransient<MainForm>();

        ServiceProvider = services.BuildServiceProvider();

        await ServiceProvider.InitializeDatabaseAsync();

        System.Windows.Forms.Application.Run(ServiceProvider.GetRequiredService<LoginForm>());
    }    
}