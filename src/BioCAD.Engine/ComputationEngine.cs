using BioCAD.Data;
using BioCAD.Data.Repositories;
using BioCAD.Engine.Modules;

namespace BioCAD.Engine;

public class ComputationEngine
{
    private readonly DataService _dataService;
    private readonly TaskQueueManager _taskQueueManager;

    public TaskQueueManager QueueManager => _taskQueueManager;

    public ComputationEngine(DataService dataService)
    {
        _dataService = dataService;
        _taskQueueManager = new TaskQueueManager(_dataService.Tasks);

        RegisterModules();
    }

    private void RegisterModules()
    {
        _taskQueueManager.RegisterModule(new MolecularDockingModule());
        _taskQueueManager.RegisterModule(new VirtualScreeningModule());
        _taskQueueManager.RegisterModule(new PharmacophoreModule());
        _taskQueueManager.RegisterModule(new MolecularDynamicsModule());
        _taskQueueManager.RegisterModule(new ClusteringModule());
    }

    public void Start()
    {
        _taskQueueManager.Start();
    }

    public void Stop()
    {
        _taskQueueManager.Stop();
    }

    public async Task<int> SubmitDockingTaskAsync(string name, int proteinId, int compoundId,
        double centerX, double centerY, double centerZ,
        double sizeX, double sizeY, double sizeZ,
        string scoringFunction = "Vina", int numPoses = 10)
    {
        var parameters = new DockingParameters
        {
            ReceptorId = proteinId.ToString(),
            LigandId = compoundId.ToString(),
            CenterX = centerX,
            CenterY = centerY,
            CenterZ = centerZ,
            SizeX = sizeX,
            SizeY = sizeY,
            SizeZ = sizeZ,
            ScoringFunction = scoringFunction,
            NumPoses = numPoses
        };

        var task = new Domain.Entities.ComputationTask
        {
            Name = name,
            Description = $"分子对接: 蛋白{proteinId} - 化合物{compoundId}",
            TaskType = Domain.Enums.TaskType.MolecularDocking,
            ParametersJson = Newtonsoft.Json.JsonConvert.SerializeObject(parameters),
            Priority = 5,
            CpuCores = 2,
            UseGpu = false
        };

        return await _taskQueueManager.SubmitTaskAsync(task);
    }

    public async Task<int> SubmitVirtualScreeningTaskAsync(string name, int targetId,
        int compoundCount, int topN = 100, string scoringMethod = "Docking")
    {
        var parameters = new VirtualScreeningParameters
        {
            TargetId = targetId.ToString(),
            CompoundCount = compoundCount,
            TopN = topN,
            ScoringMethod = scoringMethod
        };

        var task = new Domain.Entities.ComputationTask
        {
            Name = name,
            Description = $"虚拟筛选: 靶标{targetId}, {compoundCount}个化合物",
            TaskType = Domain.Enums.TaskType.VirtualScreening,
            ParametersJson = Newtonsoft.Json.JsonConvert.SerializeObject(parameters),
            Priority = 5,
            CpuCores = 4,
            UseGpu = false
        };

        return await _taskQueueManager.SubmitTaskAsync(task);
    }

    public async Task<int> SubmitPharmacophoreTaskAsync(string name, int trainingSetSize,
        string alignmentMethod = "Flexible", string modelName = "Model1")
    {
        var parameters = new PharmacophoreParameters
        {
            ModelName = modelName,
            TrainingSetSize = trainingSetSize,
            AlignmentMethod = alignmentMethod
        };

        var task = new Domain.Entities.ComputationTask
        {
            Name = name,
            Description = $"药效团建模: {modelName}, 训练集{trainingSetSize}个化合物",
            TaskType = Domain.Enums.TaskType.PharmacophoreModeling,
            ParametersJson = Newtonsoft.Json.JsonConvert.SerializeObject(parameters),
            Priority = 5,
            CpuCores = 2,
            UseGpu = false
        };

        return await _taskQueueManager.SubmitTaskAsync(task);
    }

    public async Task<int> SubmitMolecularDynamicsTaskAsync(string name,
        double simulationTime = 10.0, double temperature = 300.0, double pressure = 1.0,
        string forceField = "AMBER14")
    {
        var parameters = new MDParameters
        {
            SimulationTime = simulationTime,
            Temperature = temperature,
            Pressure = pressure,
            ForceField = forceField
        };

        var task = new Domain.Entities.ComputationTask
        {
            Name = name,
            Description = $"分子动力学模拟: {simulationTime}ns, {temperature}K",
            TaskType = Domain.Enums.TaskType.MolecularDynamics,
            ParametersJson = Newtonsoft.Json.JsonConvert.SerializeObject(parameters),
            Priority = 5,
            CpuCores = 8,
            UseGpu = true
        };

        return await _taskQueueManager.SubmitTaskAsync(task);
    }

    public async Task<int> SubmitClusteringTaskAsync(string name, int compoundCount,
        int numClusters = 5, string method = "KMeans")
    {
        var parameters = new ClusteringParameters
        {
            CompoundCount = compoundCount,
            NumClusters = numClusters,
            Method = method
        };

        var task = new Domain.Entities.ComputationTask
        {
            Name = name,
            Description = $"聚类分析: {method}, {numClusters}簇",
            TaskType = Domain.Enums.TaskType.Clustering,
            ParametersJson = Newtonsoft.Json.JsonConvert.SerializeObject(parameters),
            Priority = 5,
            CpuCores = 2,
            UseGpu = false
        };

        return await _taskQueueManager.SubmitTaskAsync(task);
    }

    public async Task<Domain.Entities.ComputationTask?> GetTaskAsync(int taskId)
    {
        return await _dataService.Tasks.GetByIdAsync(taskId);
    }

    public async Task<IEnumerable<Domain.Entities.ComputationTask>> GetAllTasksAsync()
    {
        return await _dataService.Tasks.GetAllAsync();
    }

    public async Task<bool> CancelTaskAsync(int taskId)
    {
        return await _taskQueueManager.CancelTaskAsync(taskId);
    }
}
