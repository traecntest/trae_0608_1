using BioCAD.Domain.Entities;
using BioCAD.Domain.Enums;
using Microsoft.Data.Sqlite;

namespace BioCAD.Data.Repositories;

public class ActivityDataRepository : IRepository<ActivityData>
{
    private readonly BioCADDbContext _dbContext;

    public ActivityDataRepository(BioCADDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ActivityData?> GetByIdAsync(int id)
    {
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, Description, CompoundId, TargetId, ExperimentType,
                   AssayName, ActivityType, ActivityValue, Unit, StandardDeviation,
                   Relation, TargetName, CellLine, AssayConditions, DataSource,
                   Reference, ExperimentDate, CreatedAt, UpdatedAt, CreatedBy, Version, IsDeleted
            FROM ActivityData
            WHERE Id = @Id AND IsDeleted = 0";
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapActivityData(reader);
        }
        return null;
    }

    public async Task<IEnumerable<ActivityData>> GetAllAsync()
    {
        var data = new List<ActivityData>();
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, Description, CompoundId, TargetId, ExperimentType,
                   AssayName, ActivityType, ActivityValue, Unit, StandardDeviation,
                   Relation, TargetName, CellLine, AssayConditions, DataSource,
                   Reference, ExperimentDate, CreatedAt, UpdatedAt, CreatedBy, Version, IsDeleted
            FROM ActivityData WHERE IsDeleted = 0
            ORDER BY CreatedAt DESC";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            data.Add(MapActivityData(reader));
        }
        return data;
    }

    public async Task<IEnumerable<ActivityData>> GetPagedAsync(int page, int pageSize)
    {
        var data = new List<ActivityData>();
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, Description, CompoundId, TargetId, ExperimentType,
                   AssayName, ActivityType, ActivityValue, Unit, StandardDeviation,
                   Relation, TargetName, CellLine, AssayConditions, DataSource,
                   Reference, ExperimentDate, CreatedAt, UpdatedAt, CreatedBy, Version, IsDeleted
            FROM ActivityData WHERE IsDeleted = 0
            ORDER BY CreatedAt DESC
            LIMIT @Limit OFFSET @Offset";
        command.Parameters.AddWithValue("@Limit", pageSize);
        command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            data.Add(MapActivityData(reader));
        }
        return data;
    }

    public async Task<int> AddAsync(ActivityData entity)
    {
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO ActivityData (Name, Description, CompoundId, TargetId, ExperimentType,
                AssayName, ActivityType, ActivityValue, Unit, StandardDeviation,
                Relation, TargetName, CellLine, AssayConditions, DataSource,
                Reference, ExperimentDate, CreatedAt, UpdatedAt, CreatedBy, Version, IsDeleted)
            VALUES (@Name, @Description, @CompoundId, @TargetId, @ExperimentType,
                @AssayName, @ActivityType, @ActivityValue, @Unit, @StandardDeviation,
                @Relation, @TargetName, @CellLine, @AssayConditions, @DataSource,
                @Reference, @ExperimentDate, @CreatedAt, @UpdatedAt, @CreatedBy, @Version, 0);
            SELECT last_insert_rowid();";

        AddActivityDataParameters(command, entity);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task UpdateAsync(ActivityData entity)
    {
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE ActivityData SET
                Name = @Name, Description = @Description, CompoundId = @CompoundId,
                TargetId = @TargetId, ExperimentType = @ExperimentType,
                AssayName = @AssayName, ActivityType = @ActivityType,
                ActivityValue = @ActivityValue, Unit = @Unit,
                StandardDeviation = @StandardDeviation, Relation = @Relation,
                TargetName = @TargetName, CellLine = @CellLine,
                AssayConditions = @AssayConditions, DataSource = @DataSource,
                Reference = @Reference, ExperimentDate = @ExperimentDate,
                UpdatedAt = @UpdatedAt, Version = Version + 1
            WHERE Id = @Id";

        AddActivityDataParameters(command, entity);
        command.Parameters.AddWithValue("@Id", entity.Id);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "UPDATE ActivityData SET IsDeleted = 1, UpdatedAt = @UpdatedAt WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<int> CountAsync()
    {
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM ActivityData WHERE IsDeleted = 0";

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<IEnumerable<ActivityData>> GetByCompoundIdAsync(int compoundId)
    {
        var data = new List<ActivityData>();
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, Description, CompoundId, TargetId, ExperimentType,
                   AssayName, ActivityType, ActivityValue, Unit, StandardDeviation,
                   Relation, TargetName, CellLine, AssayConditions, DataSource,
                   Reference, ExperimentDate, CreatedAt, UpdatedAt, CreatedBy, Version, IsDeleted
            FROM ActivityData
            WHERE CompoundId = @CompoundId AND IsDeleted = 0
            ORDER BY CreatedAt DESC";
        command.Parameters.AddWithValue("@CompoundId", compoundId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            data.Add(MapActivityData(reader));
        }
        return data;
    }

    private static ActivityData MapActivityData(SqliteDataReader reader)
    {
        return new ActivityData
        {
            Id = reader.GetInt32(0),
            Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
            Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
            CompoundId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
            TargetId = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
            ExperimentType = (ExperimentType)reader.GetInt32(5),
            AssayName = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
            ActivityType = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
            ActivityValue = reader.GetDouble(8),
            Unit = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
            StandardDeviation = reader.IsDBNull(10) ? 0 : reader.GetDouble(10),
            Relation = reader.IsDBNull(11) ? "=" : reader.GetString(11),
            TargetName = reader.IsDBNull(12) ? string.Empty : reader.GetString(12),
            CellLine = reader.IsDBNull(13) ? string.Empty : reader.GetString(13),
            AssayConditions = reader.IsDBNull(14) ? string.Empty : reader.GetString(14),
            DataSource = (DataSource)reader.GetInt32(15),
            Reference = reader.IsDBNull(16) ? string.Empty : reader.GetString(16),
            ExperimentDate = reader.IsDBNull(17) ? DateTime.MinValue : DateTime.Parse(reader.GetString(17)),
            CreatedAt = DateTime.Parse(reader.GetString(18)),
            UpdatedAt = DateTime.Parse(reader.GetString(19)),
            CreatedBy = reader.IsDBNull(20) ? string.Empty : reader.GetString(20),
            Version = reader.GetInt32(21),
            IsDeleted = reader.GetBoolean(22)
        };
    }

    private static void AddActivityDataParameters(SqliteCommand command, ActivityData entity)
    {
        command.Parameters.AddWithValue("@Name", entity.Name ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Description", entity.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CompoundId", entity.CompoundId);
        command.Parameters.AddWithValue("@TargetId", entity.TargetId);
        command.Parameters.AddWithValue("@ExperimentType", (int)entity.ExperimentType);
        command.Parameters.AddWithValue("@AssayName", entity.AssayName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ActivityType", entity.ActivityType ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ActivityValue", entity.ActivityValue);
        command.Parameters.AddWithValue("@Unit", entity.Unit ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@StandardDeviation", entity.StandardDeviation);
        command.Parameters.AddWithValue("@Relation", entity.Relation);
        command.Parameters.AddWithValue("@TargetName", entity.TargetName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CellLine", entity.CellLine ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@AssayConditions", entity.AssayConditions ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@DataSource", (int)entity.DataSource);
        command.Parameters.AddWithValue("@Reference", entity.Reference ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@ExperimentDate", entity.ExperimentDate.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@Version", entity.Version);
        command.Parameters.AddWithValue("@CreatedBy", entity.CreatedBy ?? (object)DBNull.Value);
    }
}
