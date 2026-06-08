using BioCAD.Domain.Entities;
using BioCAD.Domain.Enums;
using Newtonsoft.Json;

namespace BioCAD.Engine.Modules;

public class ClusteringModule : ComputationModuleBase
{
    public override string Name => "聚类分析模块";
    public override string Description => "基于化学结构或生物活性的化合物聚类分析";
    public override TaskType SupportedTaskType => TaskType.Clustering;

    public override async Task<bool> ExecuteAsync(ComputationTask task, IProgress<double> progress, CancellationToken cancellationToken)
    {
        try
        {
            var parameters = string.IsNullOrEmpty(task.ParametersJson)
                ? new ClusteringParameters()
                : JsonConvert.DeserializeObject<ClusteringParameters>(task.ParametersJson) ?? new ClusteringParameters();

            await SimulateWorkAsync("加载数据", 500, 0, 10, progress, cancellationToken,
                step => task.CurrentStep = step);

            await SimulateWorkAsync("计算相似度矩阵", 1500, 10, 35, progress, cancellationToken,
                step => task.CurrentStep = step);

            await SimulateWorkAsync("执行聚类算法", 2000, 35, 70, progress, cancellationToken,
                step => task.CurrentStep = step);

            await SimulateWorkAsync("聚类结果分析", 800, 70, 90, progress, cancellationToken,
                step => task.CurrentStep = step);

            var result = GenerateClusteringResult(parameters);
            task.OutputData = JsonConvert.SerializeObject(result);
            task.ResultFilePath = $"results/clustering_{task.Id}.json";

            await SimulateWorkAsync("保存结果", 200, 90, 100, progress, cancellationToken,
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

    private static ClusteringResult GenerateClusteringResult(ClusteringParameters parameters)
    {
        var random = new Random();
        int compoundCount = parameters.CompoundCount;
        int clusterCount = parameters.NumClusters;

        var clusters = new List<Cluster>();
        var allCompounds = new List<ClusterCompound>();

        int baseSize = compoundCount / clusterCount;
        int remainder = compoundCount % clusterCount;

        for (int i = 0; i < clusterCount; i++)
        {
            int clusterSize = baseSize + (i < remainder ? 1 : 0);
            var clusterCompounds = new List<ClusterCompound>();

            for (int j = 0; j < clusterSize; j++)
            {
                var compound = new ClusterCompound
                {
                    CompoundId = $"CMPD_{i * 100 + j}",
                    Name = $"Compound_{i * 100 + j}",
                    X = random.NextDouble() * 10 - 5 + (i - clusterCount / 2) * 3,
                    Y = random.NextDouble() * 10 - 5 + (i % 2) * 3,
                    ClusterId = i
                };
                clusterCompounds.Add(compound);
                allCompounds.Add(compound);
            }

            clusters.Add(new Cluster
            {
                ClusterId = i,
                Size = clusterSize,
                Compounds = clusterCompounds,
                CentroidX = clusterCompounds.Average(c => c.X),
                CentroidY = clusterCompounds.Average(c => c.Y),
                DiversityScore = random.NextDouble() * 0.5 + 0.3
            });
        }

        return new ClusteringResult
        {
            Method = parameters.Method,
            NumClusters = clusterCount,
            TotalCompounds = compoundCount,
            Clusters = clusters,
            AllCompounds = allCompounds,
            SilhouetteScore = 0.4 + random.NextDouble() * 0.4
        };
    }
}

public class ClusteringParameters
{
    public string Method { get; set; } = "KMeans";
    public int NumClusters { get; set; } = 5;
    public int CompoundCount { get; set; } = 100;
    public string DistanceMetric { get; set; } = "Tanimoto";
    public string DescriptorType { get; set; } = "Morgan";
    public int MaxIterations { get; set; } = 300;
}

public class ClusteringResult
{
    public string Method { get; set; } = string.Empty;
    public int NumClusters { get; set; }
    public int TotalCompounds { get; set; }
    public double SilhouetteScore { get; set; }
    public List<Cluster> Clusters { get; set; } = new();
    public List<ClusterCompound> AllCompounds { get; set; } = new();
}

public class Cluster
{
    public int ClusterId { get; set; }
    public int Size { get; set; }
    public double CentroidX { get; set; }
    public double CentroidY { get; set; }
    public double DiversityScore { get; set; }
    public List<ClusterCompound> Compounds { get; set; } = new();
}

public class ClusterCompound
{
    public string CompoundId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public int ClusterId { get; set; }
}
