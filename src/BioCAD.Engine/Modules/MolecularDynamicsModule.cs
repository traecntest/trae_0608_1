using BioCAD.Domain.Entities;
using BioCAD.Domain.Enums;
using Newtonsoft.Json;

namespace BioCAD.Engine.Modules;

public class MolecularDynamicsModule : ComputationModuleBase
{
    public override string Name => "分子动力学模拟模块";
    public override string Description => "分子动力学模拟与轨迹分析";
    public override TaskType SupportedTaskType => TaskType.MolecularDynamics;

    public override async Task<bool> ExecuteAsync(ComputationTask task, IProgress<double> progress, CancellationToken cancellationToken)
    {
        try
        {
            var parameters = string.IsNullOrEmpty(task.ParametersJson)
                ? new MDParameters()
                : JsonConvert.DeserializeObject<MDParameters>(task.ParametersJson) ?? new MDParameters();

            await SimulateWorkAsync("加载系统结构", 800, 0, 5, progress, cancellationToken,
                step => task.CurrentStep = step);

            await SimulateWorkAsync("拓扑文件生成", 600, 5, 10, progress, cancellationToken,
                step => task.CurrentStep = step);

            await SimulateWorkAsync("溶剂化与离子添加", 500, 10, 15, progress, cancellationToken,
                step => task.CurrentStep = step);

            await SimulateWorkAsync("能量最小化", 1000, 15, 25, progress, cancellationToken,
                step => task.CurrentStep = step);

            await SimulateWorkAsync("NVT 平衡", 1200, 25, 40, progress, cancellationToken,
                step => task.CurrentStep = step);

            await SimulateWorkAsync("NPT 平衡", 1200, 40, 55, progress, cancellationToken,
                step => task.CurrentStep = step);

            int totalSteps = (int)(parameters.SimulationTime / 0.001);
            int reportSteps = 10;
            for (int i = 0; i < reportSteps; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                double startProgress = 55 + (double)i / reportSteps * 35;
                double endProgress = 55 + (double)(i + 1) / reportSteps * 35;

                await SimulateWorkAsync(
                    $"生产模拟 {i * 10}%",
                    400, startProgress, endProgress,
                    progress, cancellationToken,
                    step => task.CurrentStep = step);
            }

            await SimulateWorkAsync("轨迹分析", 800, 90, 97, progress, cancellationToken,
                step => task.CurrentStep = step);

            var result = GenerateMDResult(parameters);
            task.OutputData = JsonConvert.SerializeObject(result);
            task.ResultFilePath = $"results/md_{task.Id}.nc";

            await SimulateWorkAsync("保存结果", 200, 97, 100, progress, cancellationToken,
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

    private static MDResult GenerateMDResult(MDParameters parameters)
    {
        var random = new Random();
        int frameCount = (int)(parameters.SimulationTime / parameters.OutputInterval);
        frameCount = Math.Max(10, Math.Min(1000, frameCount));

        var energyFrames = new List<EnergyFrame>();
        var rmsdFrames = new List<RmsdFrame>();

        double baseEnergy = -10000 + random.NextDouble() * -5000;
        double baseRmsd = 1.0 + random.NextDouble() * 2.0;

        for (int i = 0; i < frameCount; i++)
        {
            double time = i * parameters.OutputInterval;
            energyFrames.Add(new EnergyFrame
            {
                Time = time,
                PotentialEnergy = baseEnergy + random.NextDouble() * 200 - 100,
                KineticEnergy = Math.Abs(baseEnergy) * 0.4 + random.NextDouble() * 50,
                TotalEnergy = baseEnergy * 0.6 + random.NextDouble() * 100 - 50,
                Temperature = parameters.Temperature + random.NextDouble() * 20 - 10,
                Pressure = 1.0 + random.NextDouble() * 0.5 - 0.25
            });

            rmsdFrames.Add(new RmsdFrame
            {
                Time = time,
                ProteinRmsd = baseRmsd + Math.Sin(i * 0.1) * 0.5 + random.NextDouble() * 0.2,
                BackboneRmsd = baseRmsd * 0.7 + Math.Sin(i * 0.08) * 0.3 + random.NextDouble() * 0.15,
                LigandRmsd = 0.5 + Math.Sin(i * 0.15) * 0.3 + random.NextDouble() * 0.2
            });
        }

        return new MDResult
        {
            SimulationTime = parameters.SimulationTime,
            TimeStep = parameters.TimeStep,
            Temperature = parameters.Temperature,
            Pressure = parameters.Pressure,
            FrameCount = frameCount,
            EnergyFrames = energyFrames,
            RmsdFrames = rmsdFrames,
            FinalRmsd = rmsdFrames.Last().ProteinRmsd,
            AverageTemperature = rmsdFrames.Average(f => f.ProteinRmsd)
        };
    }
}

public class MDParameters
{
    public double SimulationTime { get; set; } = 10.0;
    public double TimeStep { get; set; } = 0.002;
    public double Temperature { get; set; } = 300.0;
    public double Pressure { get; set; } = 1.0;
    public string ForceField { get; set; } = "AMBER14";
    public string WaterModel { get; set; } = "TIP3P";
    public double OutputInterval { get; set; } = 0.01;
    public string Ensemble { get; set; } = "NPT";
    public bool RestrainProtein { get; set; } = false;
    public double RestraintForce { get; set; } = 10.0;
}

public class MDResult
{
    public double SimulationTime { get; set; }
    public double TimeStep { get; set; }
    public double Temperature { get; set; }
    public double Pressure { get; set; }
    public int FrameCount { get; set; }
    public double FinalRmsd { get; set; }
    public double AverageTemperature { get; set; }
    public List<EnergyFrame> EnergyFrames { get; set; } = new();
    public List<RmsdFrame> RmsdFrames { get; set; } = new();
}

public class EnergyFrame
{
    public double Time { get; set; }
    public double PotentialEnergy { get; set; }
    public double KineticEnergy { get; set; }
    public double TotalEnergy { get; set; }
    public double Temperature { get; set; }
    public double Pressure { get; set; }
}

public class RmsdFrame
{
    public double Time { get; set; }
    public double ProteinRmsd { get; set; }
    public double BackboneRmsd { get; set; }
    public double LigandRmsd { get; set; }
}
