using BioCAD.Domain.Entities;
using BioCAD.Domain.Enums;
using Newtonsoft.Json;

namespace BioCAD.Engine.Modules;

public class MolecularDockingModule : ComputationModuleBase
{
    public override string Name => "分子对接模块";
    public override string Description => "基于打分函数的蛋白质-配体分子对接计算";
    public override TaskType SupportedTaskType => TaskType.MolecularDocking;

    public override async Task<bool> ExecuteAsync(ComputationTask task, IProgress<double> progress, CancellationToken cancellationToken)
    {
        try
        {
            var parameters = string.IsNullOrEmpty(task.ParametersJson)
                ? new DockingParameters()
                : JsonConvert.DeserializeObject<DockingParameters>(task.ParametersJson) ?? new DockingParameters();

            await SimulateWorkAsync("加载受体结构", 500, 0, 5, progress, cancellationToken,
                step => task.CurrentStep = step);

            await SimulateWorkAsync("加载配体结构", 300, 5, 10, progress, cancellationToken,
                step => task.CurrentStep = step);

            await SimulateWorkAsync("结合位点识别", 800, 10, 20, progress, cancellationToken,
                step => task.CurrentStep = step);

            await SimulateWorkAsync("构象搜索", 2000, 20, 50, progress, cancellationToken,
                step => task.CurrentStep = step);

            await SimulateWorkAsync("打分计算", 1500, 50, 80, progress, cancellationToken,
                step => task.CurrentStep = step);

            await SimulateWorkAsync("结果分析与排序", 600, 80, 95, progress, cancellationToken,
                step => task.CurrentStep = step);

            var result = GenerateDockingResult(parameters);
            task.OutputData = JsonConvert.SerializeObject(result);
            task.ResultFilePath = $"results/docking_{task.Id}.json";

            await SimulateWorkAsync("保存结果", 200, 95, 100, progress, cancellationToken,
                step => task.CurrentStep = step);

            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static DockingResult GenerateDockingResult(DockingParameters parameters)
    {
        var random = new Random();
        var poses = new List<DockingPose>();

        for (int i = 0; i < parameters.NumPoses; i++)
        {
            poses.Add(new DockingPose
            {
                PoseId = i + 1,
                BindingAffinity = -5.0 - random.NextDouble() * 8.0,
                Rmsd = random.NextDouble() * 3.0,
                LigandEfficiency = -0.2 - random.NextDouble() * 0.5,
                Interactions = new List<string>
                {
                    "Hydrogen Bond",
                    "Hydrophobic Interaction",
                    "Pi-Stacking"
                }.OrderBy(_ => random.Next()).Take(random.Next(1, 4)).ToList()
            });
        }

        return new DockingResult
        {
            BestPose = poses.OrderBy(p => p.BindingAffinity).First(),
            AllPoses = poses.OrderBy(p => p.BindingAffinity).ToList(),
            BindingSiteCenter = new[] { 0.0, 0.0, 0.0 },
            BindingSiteSize = new[] { 20.0, 20.0, 20.0 },
            ScoringFunction = parameters.ScoringFunction
        };
    }
}

public class DockingParameters
{
    public string ReceptorId { get; set; } = string.Empty;
    public string LigandId { get; set; } = string.Empty;
    public string ScoringFunction { get; set; } = "Vina";
    public int NumPoses { get; set; } = 10;
    public double CenterX { get; set; }
    public double CenterY { get; set; }
    public double CenterZ { get; set; }
    public double SizeX { get; set; } = 20;
    public double SizeY { get; set; } = 20;
    public double SizeZ { get; set; } = 20;
    public double Exhaustiveness { get; set; } = 8;
}

public class DockingResult
{
    public DockingPose BestPose { get; set; } = new();
    public List<DockingPose> AllPoses { get; set; } = new();
    public double[] BindingSiteCenter { get; set; } = new double[3];
    public double[] BindingSiteSize { get; set; } = new double[3];
    public string ScoringFunction { get; set; } = string.Empty;
}

public class DockingPose
{
    public int PoseId { get; set; }
    public double BindingAffinity { get; set; }
    public double Rmsd { get; set; }
    public double LigandEfficiency { get; set; }
    public List<string> Interactions { get; set; } = new();
    public double[] Coordinates { get; set; } = new double[3];
}
