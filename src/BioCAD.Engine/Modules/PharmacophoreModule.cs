using BioCAD.Domain.Entities;
using BioCAD.Domain.Enums;
using Newtonsoft.Json;

namespace BioCAD.Engine.Modules;

public class PharmacophoreModule : ComputationModuleBase
{
    public override string Name => "药效团建模模块";
    public override string Description => "基于活性化合物的药效团模型构建与分析";
    public override TaskType SupportedTaskType => TaskType.PharmacophoreModeling;

    public override async Task<bool> ExecuteAsync(ComputationTask task, IProgress<double> progress, CancellationToken cancellationToken)
    {
        try
        {
            var parameters = string.IsNullOrEmpty(task.ParametersJson)
                ? new PharmacophoreParameters()
                : JsonConvert.DeserializeObject<PharmacophoreParameters>(task.ParametersJson) ?? new PharmacophoreParameters();

            await SimulateWorkAsync("加载活性化合物", 600, 0, 10, progress, cancellationToken,
                step => task.CurrentStep = step);

            await SimulateWorkAsync("构象生成与优化", 1200, 10, 30, progress, cancellationToken,
                step => task.CurrentStep = step);

            await SimulateWorkAsync("药效特征识别", 1000, 30, 50, progress, cancellationToken,
                step => task.CurrentStep = step);

            await SimulateWorkAsync("分子对齐与叠加", 1500, 50, 70, progress, cancellationToken,
                step => task.CurrentStep = step);

            await SimulateWorkAsync("药效团模型构建", 800, 70, 85, progress, cancellationToken,
                step => task.CurrentStep = step);

            await SimulateWorkAsync("模型验证与评估", 600, 85, 95, progress, cancellationToken,
                step => task.CurrentStep = step);

            var result = GeneratePharmacophoreResult(parameters);
            task.OutputData = JsonConvert.SerializeObject(result);
            task.ResultFilePath = $"results/pharmacophore_{task.Id}.json";

            await SimulateWorkAsync("保存模型", 200, 95, 100, progress, cancellationToken,
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

    private static PharmacophoreResult GeneratePharmacophoreResult(PharmacophoreParameters parameters)
    {
        var random = new Random();
        var features = new List<PharmacophoreFeature>
        {
            new() { Type = "HydrogenBondDonor", X = 0, Y = 0, Z = 0, Radius = 1.5 },
            new() { Type = "HydrogenBondAcceptor", X = 2.5, Y = 1.0, Z = 0.5, Radius = 1.5 },
            new() { Type = "Hydrophobic", X = -1.5, Y = 2.0, Z = -0.5, Radius = 2.0 },
            new() { Type = "AromaticRing", X = 1.0, Y = -1.5, Z = 1.0, Radius = 2.5 }
        };

        for (int i = 0; i < random.Next(2, 5); i++)
        {
            features.Add(new PharmacophoreFeature
            {
                Type = new[] { "HydrogenBondDonor", "HydrogenBondAcceptor", "Hydrophobic", "PositiveIonizable", "NegativeIonizable" }[random.Next(5)],
                X = random.NextDouble() * 10 - 5,
                Y = random.NextDouble() * 10 - 5,
                Z = random.NextDouble() * 10 - 5,
                Radius = 1.0 + random.NextDouble() * 2.0
            });
        }

        return new PharmacophoreResult
        {
            ModelName = parameters.ModelName,
            FeatureCount = features.Count,
            Features = features,
            TrainingSetSize = parameters.TrainingSetSize,
            CorrelationCoefficient = 0.6 + random.NextDouble() * 0.35,
            Rmsd = 0.5 + random.NextDouble() * 2.0,
            Distances = GenerateFeatureDistances(features)
        };
    }

    private static List<FeatureDistance> GenerateFeatureDistances(List<PharmacophoreFeature> features)
    {
        var distances = new List<FeatureDistance>();
        for (int i = 0; i < features.Count; i++)
        {
            for (int j = i + 1; j < features.Count; j++)
            {
                double dx = features[i].X - features[j].X;
                double dy = features[i].Y - features[j].Y;
                double dz = features[i].Z - features[j].Z;
                distances.Add(new FeatureDistance
                {
                    Feature1Index = i,
                    Feature2Index = j,
                    Distance = Math.Sqrt(dx * dx + dy * dy + dz * dz)
                });
            }
        }
        return distances;
    }
}

public class PharmacophoreParameters
{
    public string ModelName { get; set; } = "Model1";
    public int TrainingSetSize { get; set; } = 20;
    public string AlignmentMethod { get; set; } = "Flexible";
    public bool IncludeHydrophobic { get; set; } = true;
    public bool IncludeHBD { get; set; } = true;
    public bool IncludeHBA { get; set; } = true;
    public bool IncludeAromatic { get; set; } = true;
}

public class PharmacophoreResult
{
    public string ModelName { get; set; } = string.Empty;
    public int FeatureCount { get; set; }
    public List<PharmacophoreFeature> Features { get; set; } = new();
    public int TrainingSetSize { get; set; }
    public double CorrelationCoefficient { get; set; }
    public double Rmsd { get; set; }
    public List<FeatureDistance> Distances { get; set; } = new();
}

public class PharmacophoreFeature
{
    public string Type { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double Radius { get; set; }
}

public class FeatureDistance
{
    public int Feature1Index { get; set; }
    public int Feature2Index { get; set; }
    public double Distance { get; set; }
}
