using BioCAD.Domain.Entities;
using BioCAD.Domain.Enums;
using Microsoft.Data.Sqlite;
using TaskStatus = BioCAD.Domain.Enums.TaskStatus;

namespace BioCAD.Data.Repositories;

public class ComputationTaskRepository : IRepository<ComputationTask>
{
    private readonly BioCADDbContext _dbContext;

    public ComputationTaskRepository(BioCADDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ComputationTask?> GetByIdAsync(int id)
    {
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, Description, TaskType, TaskStatus, ParametersJson,
                   InputData, OutputData, LogFilePath, ResultFilePath,
                   StartedAt, CompletedAt, Progress, CurrentStep, Priority,
                   CpuCores, UseGpu, ErrorMessage, UserId, QueueName,
                   CreatedAt, UpdatedAt, CreatedBy, Version, IsDeleted
            FROM ComputationTasks
            WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var task = MapTask(reader);
            task.Logs = await GetTaskLogsAsync(connection, id);
            return task;
        }
        return null;
    }

    public async Task<IEnumerable<ComputationTask>> GetAllAsync()
    {
        var tasks = new List<ComputationTask>();
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, Description, TaskType, TaskStatus, ParametersJson,
                   InputData, OutputData, LogFilePath, ResultFilePath,
                   StartedAt, CompletedAt, Progress, CurrentStep, Priority,
                   CpuCores, UseGpu, ErrorMessage, UserId, QueueName,
                   CreatedAt, UpdatedAt, CreatedBy, Version, IsDeleted
            FROM ComputationTasks
            WHERE IsDeleted = 0
            ORDER BY CreatedAt DESC";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tasks.Add(MapTask(reader));
        }
        return tasks;
    }

    public async Task<IEnumerable<ComputationTask>> GetPagedAsync(int page, int pageSize)
    {
        var tasks = new List<ComputationTask>();
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, Description, TaskType, TaskStatus, ParametersJson,
                   InputData, OutputData, LogFilePath, ResultFilePath,
                   StartedAt, CompletedAt, Progress, CurrentStep, Priority,
                   CpuCores, UseGpu, ErrorMessage, UserId, QueueName,
                   CreatedAt, UpdatedAt, CreatedBy, Version, IsDeleted
            FROM ComputationTasks
            WHERE IsDeleted = 0
            ORDER BY CreatedAt DESC
            LIMIT @Limit OFFSET @Offset";
        command.Parameters.AddWithValue("@Limit", pageSize);
        command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tasks.Add(MapTask(reader));
        }
        return tasks;
    }

    public async Task<int> AddAsync(ComputationTask entity)
    {
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO ComputationTasks (Name, Description, TaskType, TaskStatus, ParametersJson,
                InputData, OutputData, LogFilePath, ResultFilePath,
                StartedAt, CompletedAt, Progress, CurrentStep, Priority,
                CpuCores, UseGpu, ErrorMessage, UserId, QueueName,
                CreatedAt, UpdatedAt, CreatedBy, Version, IsDeleted)
            VALUES (@Name, @Description, @TaskType, @TaskStatus, @ParametersJson,
                @InputData, @OutputData, @LogFilePath, @ResultFilePath,
                @StartedAt, @CompletedAt, @Progress, @CurrentStep, @Priority,
                @CpuCores, @UseGpu, @ErrorMessage, @UserId, @QueueName,
                @CreatedAt, @UpdatedAt, @CreatedBy, @Version, 0);
            SELECT last_insert_rowid();";

        AddTaskParameters(command, entity);
        var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        command.Parameters.AddWithValue("@CreatedAt", now);
        command.Parameters.AddWithValue("@UpdatedAt", now);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task UpdateAsync(ComputationTask entity)
    {
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE ComputationTasks SET
                Name = @Name, Description = @Description, TaskType = @TaskType,
                TaskStatus = @TaskStatus, ParametersJson = @ParametersJson,
                InputData = @InputData, OutputData = @OutputData,
                LogFilePath = @LogFilePath, ResultFilePath = @ResultFilePath,
                StartedAt = @StartedAt, CompletedAt = @CompletedAt,
                Progress = @Progress, CurrentStep = @CurrentStep,
                Priority = @Priority, CpuCores = @CpuCores,
                UseGpu = @UseGpu, ErrorMessage = @ErrorMessage,
                UserId = @UserId, QueueName = @QueueName,
                UpdatedAt = @UpdatedAt, Version = Version + 1
            WHERE Id = @Id";

        AddTaskParameters(command, entity);
        command.Parameters.AddWithValue("@Id", entity.Id);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "UPDATE ComputationTasks SET IsDeleted = 1, UpdatedAt = @UpdatedAt WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<int> CountAsync()
    {
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM ComputationTasks WHERE IsDeleted = 0";

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<IEnumerable<ComputationTask>> GetByStatusAsync(TaskStatus status)
    {
        var tasks = new List<ComputationTask>();
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, Description, TaskType, TaskStatus, ParametersJson,
                   InputData, OutputData, LogFilePath, ResultFilePath,
                   StartedAt, CompletedAt, Progress, CurrentStep, Priority,
                   CpuCores, UseGpu, ErrorMessage, UserId, QueueName,
                   CreatedAt, UpdatedAt, CreatedBy, Version, IsDeleted
            FROM ComputationTasks
            WHERE IsDeleted = 0 AND TaskStatus = @Status
            ORDER BY Priority DESC, CreatedAt ASC";
        command.Parameters.AddWithValue("@Status", (int)status);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tasks.Add(MapTask(reader));
        }
        return tasks;
    }

    public async Task<IEnumerable<ComputationTask>> GetByQueueAsync(string queueName)
    {
        var tasks = new List<ComputationTask>();
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, Description, TaskType, TaskStatus, ParametersJson,
                   InputData, OutputData, LogFilePath, ResultFilePath,
                   StartedAt, CompletedAt, Progress, CurrentStep, Priority,
                   CpuCores, UseGpu, ErrorMessage, UserId, QueueName,
                   CreatedAt, UpdatedAt, CreatedBy, Version, IsDeleted
            FROM ComputationTasks
            WHERE IsDeleted = 0 AND QueueName = @QueueName
            ORDER BY Priority DESC, CreatedAt ASC";
        command.Parameters.AddWithValue("@QueueName", queueName);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tasks.Add(MapTask(reader));
        }
        return tasks;
    }

    public async Task AddLogEntryAsync(int taskId, string logLevel, string message, double? progress = null)
    {
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO TaskLogs (TaskId, Timestamp, LogLevel, Message, Progress)
            VALUES (@TaskId, @Timestamp, @LogLevel, @Message, @Progress)";
        command.Parameters.AddWithValue("@TaskId", taskId);
        command.Parameters.AddWithValue("@Timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@LogLevel", logLevel);
        command.Parameters.AddWithValue("@Message", message);
        command.Parameters.AddWithValue("@Progress", progress ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateStatusAsync(int taskId, TaskStatus status, double progress = 0, string currentStep = "")
    {
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE ComputationTasks
            SET TaskStatus = @Status, Progress = @Progress, CurrentStep = @CurrentStep,
                UpdatedAt = @UpdatedAt";

        if (status == TaskStatus.Running)
        {
            command.CommandText += ", StartedAt = @StartedAt WHERE StartedAt IS NULL";
            command.Parameters.AddWithValue("@StartedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        if (status == TaskStatus.Completed || status == TaskStatus.Failed)
        {
            command.CommandText += ", CompletedAt = @CompletedAt";
            command.Parameters.AddWithValue("@CompletedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        command.CommandText += " WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", taskId);
        command.Parameters.AddWithValue("@Status", (int)status);
        command.Parameters.AddWithValue("@Progress", progress);
        command.Parameters.AddWithValue("@CurrentStep", currentStep);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        await command.ExecuteNonQueryAsync();
    }

    private static ComputationTask MapTask(SqliteDataReader reader)
    {
        return new ComputationTask
        {
            Id = reader.GetInt32(0),
            Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
            Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
            TaskType = (TaskType)reader.GetInt32(3),
            Status = (TaskStatus)reader.GetInt32(4),
            ParametersJson = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
            InputData = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
            OutputData = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
            LogFilePath = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
            ResultFilePath = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
            StartedAt = reader.IsDBNull(10) ? null : DateTime.Parse(reader.GetString(10)),
            CompletedAt = reader.IsDBNull(11) ? null : DateTime.Parse(reader.GetString(11)),
            Progress = reader.GetDouble(12),
            CurrentStep = reader.IsDBNull(13) ? string.Empty : reader.GetString(13),
            Priority = reader.GetInt32(14),
            CpuCores = reader.GetInt32(15),
            UseGpu = reader.GetBoolean(16),
            ErrorMessage = reader.IsDBNull(17) ? string.Empty : reader.GetString(17),
            UserId = reader.IsDBNull(18) ? string.Empty : reader.GetString(18),
            QueueName = reader.IsDBNull(19) ? "default" : reader.GetString(19),
            CreatedAt = DateTime.Parse(reader.GetString(20)),
            UpdatedAt = DateTime.Parse(reader.GetString(21)),
            CreatedBy = reader.IsDBNull(22) ? string.Empty : reader.GetString(22),
            Version = reader.GetInt32(23),
            IsDeleted = reader.GetBoolean(24)
        };
    }

    private static void AddTaskParameters(SqliteCommand command, ComputationTask entity)
    {
        command.Parameters.AddWithValue("@Name", entity.Name ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Description", entity.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@TaskType", (int)entity.TaskType);
        command.Parameters.AddWithValue("@TaskStatus", (int)entity.Status);
        command.Parameters.AddWithValue("@ParametersJson", entity.ParametersJson ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@InputData", entity.InputData ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@OutputData", entity.OutputData ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@LogFilePath", entity.LogFilePath ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ResultFilePath", entity.ResultFilePath ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@StartedAt", entity.StartedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CompletedAt", entity.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Progress", entity.Progress);
        command.Parameters.AddWithValue("@CurrentStep", entity.CurrentStep ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Priority", entity.Priority);
        command.Parameters.AddWithValue("@CpuCores", entity.CpuCores);
        command.Parameters.AddWithValue("@UseGpu", entity.UseGpu);
        command.Parameters.AddWithValue("@ErrorMessage", entity.ErrorMessage ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@UserId", entity.UserId ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@QueueName", entity.QueueName);
        command.Parameters.AddWithValue("@Version", entity.Version);
        command.Parameters.AddWithValue("@CreatedBy", entity.CreatedBy ?? (object)DBNull.Value);
    }

    private static async Task<List<TaskLogEntry>> GetTaskLogsAsync(SqliteConnection connection, int taskId)
    {
        var logs = new List<TaskLogEntry>();
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, TaskId, Timestamp, LogLevel, Message, Progress
            FROM TaskLogs
            WHERE TaskId = @TaskId
            ORDER BY Timestamp ASC";
        command.Parameters.AddWithValue("@TaskId", taskId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            logs.Add(new TaskLogEntry
            {
                Id = reader.GetInt32(0),
                TaskId = reader.GetInt32(1),
                Timestamp = DateTime.Parse(reader.GetString(2)),
                LogLevel = reader.GetString(3),
                Message = reader.GetString(4),
                Progress = reader.IsDBNull(5) ? null : reader.GetDouble(5)
            });
        }
        return logs;
    }
}
