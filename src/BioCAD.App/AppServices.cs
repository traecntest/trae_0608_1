using BioCAD.Data;
using BioCAD.Engine;

namespace BioCAD.App;

public static class AppServices
{
    private static DataService? _dataService;
    private static ComputationEngine? _engine;

    public static DataService Data => _dataService ?? throw new InvalidOperationException("应用未初始化");
    public static ComputationEngine Engine => _engine ?? throw new InvalidOperationException("应用未初始化");

    public static void Initialize(string dbPath)
    {
        _dataService = new DataService(dbPath);
        _dataService.InitializeDatabase();

        _engine = new ComputationEngine(_dataService);
        _engine.Start();
    }

    public static void Shutdown()
    {
        _engine?.Stop();
    }
}
