using BioCAD.Domain.Entities;
using BioCAD.Domain.Enums;
using Microsoft.Data.Sqlite;

namespace BioCAD.Data.Repositories;

public class ProteinRepository : IRepository<Protein>
{
    private readonly BioCADDbContext _dbContext;

    public ProteinRepository(BioCADDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Protein?> GetByIdAsync(int id)
    {
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, Description, Sequence, GeneName, Organism,
                   SequenceLength, MolecularWeight, IsoelectricPoint, PdbId,
                   Family, SecondaryStructure, PdbFilePath, UniProtId, MoleculeType,
                   CreatedAt, UpdatedAt, CreatedBy, Version, IsDeleted
            FROM Proteins
            WHERE Id = @Id AND IsDeleted = 0";
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapProtein(reader);
        }
        return null;
    }

    public async Task<IEnumerable<Protein>> GetAllAsync()
    {
        var proteins = new List<Protein>();
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, Description, Sequence, GeneName, Organism,
                   SequenceLength, MolecularWeight, IsoelectricPoint, PdbId,
                   Family, SecondaryStructure, PdbFilePath, UniProtId, MoleculeType,
                   CreatedAt, UpdatedAt, CreatedBy, Version, IsDeleted
            FROM Proteins WHERE IsDeleted = 0 ORDER BY Name";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            proteins.Add(MapProtein(reader));
        }
        return proteins;
    }

    public async Task<IEnumerable<Protein>> GetPagedAsync(int page, int pageSize)
    {
        var proteins = new List<Protein>();
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, Description, Sequence, GeneName, Organism,
                   SequenceLength, MolecularWeight, IsoelectricPoint, PdbId,
                   Family, SecondaryStructure, PdbFilePath, UniProtId, MoleculeType,
                   CreatedAt, UpdatedAt, CreatedBy, Version, IsDeleted
            FROM Proteins WHERE IsDeleted = 0
            ORDER BY Name LIMIT @Limit OFFSET @Offset";
        command.Parameters.AddWithValue("@Limit", pageSize);
        command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            proteins.Add(MapProtein(reader));
        }
        return proteins;
    }

    public async Task<int> AddAsync(Protein entity)
    {
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Proteins (Name, Description, Sequence, GeneName, Organism,
                SequenceLength, MolecularWeight, IsoelectricPoint, PdbId,
                Family, SecondaryStructure, PdbFilePath, UniProtId, MoleculeType,
                CreatedAt, UpdatedAt, CreatedBy, Version, IsDeleted)
            VALUES (@Name, @Description, @Sequence, @GeneName, @Organism,
                @SequenceLength, @MolecularWeight, @IsoelectricPoint, @PdbId,
                @Family, @SecondaryStructure, @PdbFilePath, @UniProtId, @MoleculeType,
                @CreatedAt, @UpdatedAt, @CreatedBy, @Version, 0);
            SELECT last_insert_rowid();";

        AddProteinParameters(command, entity);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task UpdateAsync(Protein entity)
    {
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Proteins SET
                Name = @Name, Description = @Description, Sequence = @Sequence,
                GeneName = @GeneName, Organism = @Organism,
                SequenceLength = @SequenceLength, MolecularWeight = @MolecularWeight,
                IsoelectricPoint = @IsoelectricPoint, PdbId = @PdbId,
                Family = @Family, SecondaryStructure = @SecondaryStructure,
                PdbFilePath = @PdbFilePath, UniProtId = @UniProtId,
                UpdatedAt = @UpdatedAt, Version = Version + 1
            WHERE Id = @Id";

        AddProteinParameters(command, entity);
        command.Parameters.AddWithValue("@Id", entity.Id);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "UPDATE Proteins SET IsDeleted = 1, UpdatedAt = @UpdatedAt WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<int> CountAsync()
    {
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Proteins WHERE IsDeleted = 0";

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    private static Protein MapProtein(SqliteDataReader reader)
    {
        return new Protein
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
            Sequence = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
            GeneName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
            Organism = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
            SequenceLength = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
            MolecularWeight = reader.IsDBNull(7) ? 0 : reader.GetDouble(7),
            IsoelectricPoint = reader.IsDBNull(8) ? 0 : reader.GetDouble(8),
            PdbId = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
            Family = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
            SecondaryStructure = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
            PdbFilePath = reader.IsDBNull(12) ? string.Empty : reader.GetString(12),
            UniProtId = reader.IsDBNull(13) ? string.Empty : reader.GetString(13),
            MoleculeType = (MoleculeType)reader.GetInt32(14),
            CreatedAt = DateTime.Parse(reader.GetString(15)),
            UpdatedAt = DateTime.Parse(reader.GetString(16)),
            CreatedBy = reader.IsDBNull(17) ? string.Empty : reader.GetString(17),
            Version = reader.GetInt32(18),
            IsDeleted = reader.GetBoolean(19)
        };
    }

    private static void AddProteinParameters(SqliteCommand command, Protein entity)
    {
        command.Parameters.AddWithValue("@Name", entity.Name);
        command.Parameters.AddWithValue("@Description", entity.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Sequence", entity.Sequence ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@GeneName", entity.GeneName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Organism", entity.Organism ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@SequenceLength", entity.SequenceLength);
        command.Parameters.AddWithValue("@MolecularWeight", entity.MolecularWeight);
        command.Parameters.AddWithValue("@IsoelectricPoint", entity.IsoelectricPoint);
        command.Parameters.AddWithValue("@PdbId", entity.PdbId ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Family", entity.Family ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@SecondaryStructure", entity.SecondaryStructure ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@PdbFilePath", entity.PdbFilePath ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@UniProtId", entity.UniProtId ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@MoleculeType", (int)entity.MoleculeType);
        command.Parameters.AddWithValue("@Version", entity.Version);
        command.Parameters.AddWithValue("@CreatedBy", entity.CreatedBy ?? (object)DBNull.Value);
    }
}
