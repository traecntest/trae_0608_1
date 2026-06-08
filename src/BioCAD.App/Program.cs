namespace BioCAD.App;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "biocad.db");
        AppServices.Initialize(dbPath);

        Application.Run(new MainForm());
    }
}