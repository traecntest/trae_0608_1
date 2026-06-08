using BioCAD.Domain.Entities;
using BioCAD.Domain.Enums;
using BioCAD.Data.Repositories;
using Microsoft.Data.Sqlite;
using TaskStatus = BioCAD.Domain.Enums.TaskStatus;

namespace BioCAD.Engine;

public class TaskQueueManager
{
    private readonly ComputationTaskRepository _taskRepository;
    private readonly Dictionary<string, TaskQueue> _queues = new();
    private readonly Dictionary<int, CancellationTokenSource> _runningTasks = new();
    private readonly Dictionary<TaskType, IComputationModule> _modules = new();
    private readonly object _lockObj = new();
    private bool _isRunning = false;
    private Thread? _dispatcherThread;

    public ResourceConfig ResourceConfig { get; private set; } = new();

    public TaskQueueManager(ComputationTaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
        _queues["default"] = new TaskQueue { Name = "default", MaxConcurrentTasks = 2 };
    }

    public void RegisterModule(IComputationModule module)
    {
        _modules[module.SupportedTaskType] = module;
    }

    public void RegisterQueue(string name, int maxConcurrentTasks)
    {
        if (!_queues.ContainsKey(name))
        {
            _queues[name] = new TaskQueue { Name = name, MaxConcurrentTasks = maxConcurrentTasks };
        }
        else
        {
            _queues[name].MaxConcurrentTasks = maxConcurrentTasks;
        }
    }

    public void Start()
    {
        if (_isRunning) return;

        _isRunning = true;
        _dispatcherThread = new Thread(DispatchLoop)
        {
            IsBackground = true,
            Name = "TaskDispatcher"
        };
        _dispatcherThread.Start();
    }

    public void Stop()
    {
        _isRunning = false;
        lock (_lockObj)
        {
            foreach (var cts in _runningTasks.Values)
            {
                cts.Cancel();
            }
        }
    }

    public async Task<int> SubmitTaskAsync(ComputationTask task)
    {
        task.Status = TaskStatus.Queued;
        task.Progress = 0;
        task.CurrentStep = "已加入队列";
        int taskId = await _taskRepository.AddAsync(task);
        await _taskRepository.AddLogEntryAsync(taskId, "INFO", $"任务已提交到队列 {task.QueueName}");
        return taskId;
    }

    public async Task<bool> CancelTaskAsync(int taskId)
    {
        lock (_lockObj)
        {
            if (_runningTasks.TryGetValue(taskId, out var cts))
            {
                cts.Cancel();
                return true;
            }
        }

        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task != null && (task.Status == TaskStatus.Queued || task.Status == TaskStatus.Pending))
        {
            await _taskRepository.UpdateStatusAsync(taskId, TaskStatus.Cancelled, 0, "已取消");
            await _taskRepository.AddLogEntryAsync(taskId, "INFO", "任务已取消");
            return true;
        }

        return false;
    }

    public async Task PauseTaskAsync(int taskId)
    {
        await _taskRepository.UpdateStatusAsync(taskId, TaskStatus.Paused, 0, "已暂停");
        await _taskRepository.AddLogEntryAsync(taskId, "INFO", "任务已暂停");
    }

    public async Task ResumeTaskAsync(int taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task != null && task.Status == TaskStatus.Paused)
        {
            await _taskRepository.UpdateStatusAsync(taskId, TaskStatus.Queued, task.Progress, "已恢复");
            await _taskRepository.AddLogEntryAsync(taskId, "INFO", "任务已恢复执行");
        }
    }

    private void DispatchLoop()
    {
        while (_isRunning)
        {
            try
            {
                DispatchPendingTasks().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"调度器错误: {ex.Message}");
            }

            Thread.Sleep(1000);
        }
    }

    private async Task DispatchPendingTasks()
    {
        foreach (var queue in _queues.Values)
        {
            int runningCount = 0;
            lock (_lockObj)
            {
                runningCount = _runningTasks.Count(kvp =>
                {
                    var task = _taskRepository.GetByIdAsync(kvp.Key).GetAwaiter().GetResult();
                    return task != null && task.QueueName == queue.Name;
                });
            }

            if (runningCount >= queue.MaxConcurrentTasks)
                continue;

            var pendingTasks = (await _taskRepository.GetByStatusAsync(TaskStatus.Queued))
                .Where(t => t.QueueName == queue.Name)
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.CreatedAt)
                .ToList();

            int slots = queue.MaxConcurrentTasks - runningCount;
            foreach (var task in pendingTasks.Take(slots))
            {
                _ = StartTaskExecution(task);
            }
        }
    }

    private async Task StartTaskExecution(ComputationTask task)
    {
        var cts = new CancellationTokenSource();
        lock (_lockObj)
        {
            _runningTasks[task.Id] = cts;
        }

        try
        {
            await _taskRepository.UpdateStatusAsync(task.Id, TaskStatus.Running, 0, "初始化中...");
            await _taskRepository.AddLogEntryAsync(task.Id, "INFO", "任务开始执行");

            if (!_modules.TryGetValue(task.TaskType, out var module))
            {
                throw new InvalidOperationException($"不支持的任务类型: {task.TaskType}");
            }

            var progress = new Progress<double>(p =>
            {
                _ = _taskRepository.UpdateStatusAsync(task.Id, TaskStatus.Running, p, task.CurrentStep);
            });

            bool success = await module.ExecuteAsync(task, progress, cts.Token);

            if (success)
            {
                await _taskRepository.UpdateStatusAsync(task.Id, TaskStatus.Completed, 100, "完成");
                await _taskRepository.AddLogEntryAsync(task.Id, "INFO", "任务执行成功完成");
            }
            else
            {
                await _taskRepository.UpdateStatusAsync(task.Id, TaskStatus.Failed, task.Progress, "执行失败");
                await _taskRepository.AddLogEntryAsync(task.Id, "ERROR", "任务执行失败");
            }
        }
        catch (OperationCanceledException)
        {
            await _taskRepository.UpdateStatusAsync(task.Id, TaskStatus.Cancelled, task.Progress, "已取消");
            await _taskRepository.AddLogEntryAsync(task.Id, "INFO", "任务被取消");
        }
        catch (Exception ex)
        {
            await _taskRepository.UpdateStatusAsync(task.Id, TaskStatus.Failed, task.Progress, "错误");
            await _taskRepository.AddLogEntryAsync(task.Id, "ERROR", $"执行错误: {ex.Message}");
        }
        finally
        {
            lock (_lockObj)
            {
                _runningTasks.Remove(task.Id);
            }
            cts.Dispose();
        }
    }

    public int GetRunningTaskCount()
    {
        lock (_lockObj)
        {
            return _runningTasks.Count;
        }
    }

    public bool IsTaskRunning(int taskId)
    {
        lock (_lockObj)
        {
            return _runningTasks.ContainsKey(taskId);
        }
    }

    public async Task<IEnumerable<TaskLogEntry>> GetTaskLogsAsync(int taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        return task?.Logs ?? Enumerable.Empty<TaskLogEntry>();
    }
}
