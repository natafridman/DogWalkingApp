namespace DogWalking.WinForms;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        System.Windows.Forms.Application.Run(new Form1());
    }    
}