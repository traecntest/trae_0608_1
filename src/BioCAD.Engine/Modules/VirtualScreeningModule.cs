using BioCAD.Domain.Entities;
using BioCAD.Domain.Enums;
using Newtonsoft.Json;

namespace BioCAD.Engine.Modules;

public class VirtualScreeningModule : ComputationModuleBase
{
    public override string Name => "虚拟筛选模块";
    public override string Description => "高通量虚拟筛选，从化合物库中筛选潜在活性化合物";
    public override TaskType SupportedTaskType => TaskType.VirtualScreening;

    public override async Task<bool> ExecuteAsync(ComputationTask task, IProgress<double> progress, CancellationToken cancellationToken)
    {
        try
        {
            var parameters = string.IsNullOrEmpty(task.ParametersJson)
                ? new VirtualScreeningParameters()
                : JsonConvert.DeserializeObject<VirtualScreeningParameters>(task.ParametersJson) ?? new VirtualScreeningParameters();

            await SimulateWorkAsync("加载化合物库", 1000, 0, 10, progress, cancellationToken,
                step => task.CurrentStep = step);

            await SimulateWorkAsync("准备靶标结构", 800, 10, 20, progress, cancellationToken,
                step => task.CurrentStep = step);

            int totalCompounds = parameters.CompoundCount;
            int batchSize = Math.Max(1, totalCompounds / 50);

            for (int i = 0; i < totalCompounds; i += batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int batchEnd = Math.Min(i + batchSize, totalCompounds);
                double startProgress = 20 + (double)i / totalCompounds * 70;
                double endProgress = 20 + (double)batchEnd / totalCompounds * 70;

                await SimulateWorkAsync(
                    $"筛选化合物 {i + 1}-{batchEnd}/{totalCompounds}",
                    200, startProgress, endProgress,
                    progress, cancellationToken,
                    step => task.CurrentStep = step);
            }

            await SimulateWorkAsync("结果排序与分析", 600, 90, 97, progress, cancellationToken,
                step => task.CurrentStep = step);

            var result = GenerateScreeningResult(parameters);
            task.OutputData = JsonConvert.SerializeObject(result);
            task.ResultFilePath = $"results/screening_{task.Id}.csv";

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

    private static VirtualScreeningResult GenerateScreeningResult(VirtualScreeningParameters parameters)
    {
        var random = new Random();
        var hits = new List<ScreeningHit>();

        int hitCount = Math.Min(parameters.TopN, parameters.CompoundCount);
        for (int i = 0; i < hitCount; i++)
        {
            hits.Add(new ScreeningHit
            {
                Rank = i + 1,
                CompoundId = $"CMPD_{random.Next(10000, 99999)}",
                CompoundName = $"Compound_{i + 1}",
                Score = -5.0 - random.NextDouble() * 8.0,
                MolecularWeight = 200 + random.NextDouble() * 300,
                LogP = -1 + random.NextDouble() * 6,
                TPSA = 30 + random.NextDouble() * 100
            });
        }

        return new VirtualScreeningResult
        {
            TotalCompounds = parameters.CompoundCount,
            HitCount = hitCount,
            TopHits = hits.OrderBy(h => h.Score).ToList(),
            TargetId = parameters.TargetId,
            ScoringMethod = parameters.ScoringMethod
        };
    }
}

public class VirtualScreeningParameters
{
    public string TargetId { get; set; } = string.Empty;
    public string LibraryName { get; set; } = "Default";
    public int CompoundCount { get; set; } = 1000;
    public int TopN { get; set; } = 100;
    public string ScoringMethod { get; set; } = "Docking";
    public double MinScoreThreshold { get; set; } = -7.0;
    public bool FilterByLipinski { get; set; } = true;
}

public class VirtualScreeningResult
{
    public int TotalCompounds { get; set; }
    public int HitCount { get; set; }
    public string TargetId { get; set; } = string.Empty;
    public string ScoringMethod { get; set; } = string.Empty;
    public List<ScreeningHit> TopHits { get; set; } = new();
}

public class ScreeningHit
{
    public int Rank { get; set; }
    public string CompoundId { get; set; } = string.Empty;
    public string CompoundName { get; set; } = string.Empty;
    public double Score { get; set; }
    public double MolecularWeight { get; set; }
    public double LogP { get; set; }
    public double TPSA { get; set; }
}
