using BioCAD.Domain.Entities;
using BioCAD.Domain.Enums;
using TaskStatus = BioCAD.Domain.Enums.TaskStatus;

namespace BioCAD.Engine;

public interface IComputationModule
{
    string Name { get; }
    string Description { get; }
    TaskType SupportedTaskType { get; }
    Task<bool> ExecuteAsync(ComputationTask task, IProgress<double> progress, CancellationToken cancellationToken);
}

public abstract class ComputationModuleBase : IComputationModule
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract TaskType SupportedTaskType { get; }

    public abstract Task<bool> ExecuteAsync(ComputationTask task, IProgress<double> progress, CancellationToken cancellationToken);

    protected async Task SimulateWorkAsync(string stepName, int durationMs, double startProgress, double endProgress,
        IProgress<double> progress, CancellationToken cancellationToken, Action<string>? onStepStart = null)
    {
        onStepStart?.Invoke(stepName);

        int steps = Math.Max(10, durationMs / 100);
        for (int i = 0; i <= steps; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            double currentProgress = startProgress + (endProgress - startProgress) * ((double)i / steps);
            progress.Report(currentProgress);
            await Task.Delay(durationMs / steps, cancellationToken);
        }

        progress.Report(endProgress);
    }
}
