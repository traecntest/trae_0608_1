using BioCAD.Data.Repositories;
using BioCAD.Data.ImportExport;

namespace BioCAD.Data;

public class DataService
{
    private readonly BioCADDbContext _dbContext;
    private CompoundRepository? _compoundRepository;
    private ProteinRepository? _proteinRepository;
    private ActivityDataRepository? _activityRepository;
    private ComputationTaskRepository? _taskRepository;

    public CompoundRepository Compounds
    {
        get { _compoundRepository ??= new CompoundRepository(_dbContext); return _compoundRepository; }
    }

    public ProteinRepository Proteins
    {
        get { _proteinRepository ??= new ProteinRepository(_dbContext); return _proteinRepository; }
    }

    public ActivityDataRepository Activities
    {
        get { _activityRepository ??= new ActivityDataRepository(_dbContext); return _activityRepository; }
    }

    public ComputationTaskRepository Tasks
    {
        get { _taskRepository ??= new ComputationTaskRepository(_dbContext); return _taskRepository; }
    }

    public DataExportService Exporter { get; } = new();
    public SdfParser SdfParser { get; } = new();
    public Mol2Parser Mol2Parser { get; } = new();
    public FastaParser FastaParser { get; } = new();

    public DataService(string dbPath)
    {
        _dbContext = new BioCADDbContext(dbPath);
    }

    public void InitializeDatabase()
    {
        _dbContext.InitializeDatabase();
    }

    public BioCADDbContext GetDbContext()
    {
        return _dbContext;
    }
}
