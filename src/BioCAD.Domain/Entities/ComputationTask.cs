using BioCAD.Domain.Enums;
using TaskStatus = BioCAD.Domain.Enums.TaskStatus;

namespace BioCAD.Domain.Entities;

public class ComputationTask : EntityBase
{
    public TaskType TaskType { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    public string ParametersJson { get; set; } = string.Empty;
    public string InputData { get; set; } = string.Empty;
    public string OutputData { get; set; } = string.Empty;
    public string LogFilePath { get; set; } = string.Empty;
    public string ResultFilePath { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public double Progress { get; set; }
    public string CurrentStep { get; set; } = string.Empty;
    public int Priority { get; set; } = 5;
    public int CpuCores { get; set; } = 1;
    public bool UseGpu { get; set; } = false;
    public string ErrorMessage { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string QueueName { get; set; } = "default";
    public List<TaskLogEntry> Logs { get; set; } = new();
}

public class TaskLogEntry
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string LogLevel { get; set; } = "INFO";
    public string Message { get; set; } = string.Empty;
    public double? Progress { get; set; }
}

public class TaskQueue
{
    public string Name { get; set; } = string.Empty;
    public int MaxConcurrentTasks { get; set; } = 2;
    public List<ComputationTask> PendingTasks { get; set; } = new();
    public List<ComputationTask> RunningTasks { get; set; } = new();
}

public class ResourceConfig
{
    public int TotalCpuCores { get; set; } = Environment.ProcessorCount;
    public int AvailableCpuCores { get; set; } = Environment.ProcessorCount;
    public bool GpuAvailable { get; set; } = false;
    public int GpuCount { get; set; } = 0;
    public long TotalMemoryMb { get; set; }
    public long AvailableMemoryMb { get; set; }
}
