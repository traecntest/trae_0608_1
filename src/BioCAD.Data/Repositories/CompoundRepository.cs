using BioCAD.Domain.Entities;
using Microsoft.Data.Sqlite;

namespace BioCAD.Data.Repositories;

public class CompoundRepository : IRepository<Compound>
{
    private readonly BioCADDbContext _dbContext;

    public CompoundRepository(BioCADDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Compound?> GetByIdAsync(int id)
    {
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, Description, Smiles, InChI, InChIKey, Formula,
                   MolecularWeight, LogP, HBD, HBA, TPSA, RotatableBonds,
                   SdfFilePath, Mol2FilePath, CasNumber, PubChemCid, MoleculeType,
                   CreatedAt, UpdatedAt, CreatedBy, Version, IsDeleted
            FROM Compounds
            WHERE Id = @Id AND IsDeleted = 0";
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var compound = MapCompound(reader);
            compound.Atoms = await GetAtomsAsync(connection, id);
            compound.Bonds = await GetBondsAsync(connection, id);
            compound.Metadata = await GetMetadataAsync(connection, id, "Compound");
            return compound;
        }
        return null;
    }

    public async Task<IEnumerable<Compound>> GetAllAsync()
    {
        var compounds = new List<Compound>();
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, Description, Smiles, InChI, InChIKey, Formula,
                   MolecularWeight, LogP, HBD, HBA, TPSA, RotatableBonds,
                   SdfFilePath, Mol2FilePath, CasNumber, PubChemCid, MoleculeType,
                   CreatedAt, UpdatedAt, CreatedBy, Version, IsDeleted
            FROM Compounds
            WHERE IsDeleted = 0
            ORDER BY Name";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            compounds.Add(MapCompound(reader));
        }
        return compounds;
    }

    public async Task<IEnumerable<Compound>> GetPagedAsync(int page, int pageSize)
    {
        var compounds = new List<Compound>();
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, Description, Smiles, InChI, InChIKey, Formula,
                   MolecularWeight, LogP, HBD, HBA, TPSA, RotatableBonds,
                   SdfFilePath, Mol2FilePath, CasNumber, PubChemCid, MoleculeType,
                   CreatedAt, UpdatedAt, CreatedBy, Version, IsDeleted
            FROM Compounds
            WHERE IsDeleted = 0
            ORDER BY Name
            LIMIT @Limit OFFSET @Offset";
        command.Parameters.AddWithValue("@Limit", pageSize);
        command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            compounds.Add(MapCompound(reader));
        }
        return compounds;
    }

    public async Task<int> AddAsync(Compound entity)
    {
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Compounds (Name, Description, Smiles, InChI, InChIKey, Formula,
                    MolecularWeight, LogP, HBD, HBA, TPSA, RotatableBonds,
                    SdfFilePath, Mol2FilePath, CasNumber, PubChemCid, MoleculeType,
                    CreatedAt, UpdatedAt, CreatedBy, Version, IsDeleted)
                VALUES (@Name, @Description, @Smiles, @InChI, @InChIKey, @Formula,
                    @MolecularWeight, @LogP, @HBD, @HBA, @TPSA, @RotatableBonds,
                    @SdfFilePath, @Mol2FilePath, @CasNumber, @PubChemCid, @MoleculeType,
                    @CreatedAt, @UpdatedAt, @CreatedBy, @Version, 0);
                SELECT last_insert_rowid();";

            AddCompoundParameters(command, entity);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            var result = await command.ExecuteScalarAsync();
            int compoundId = Convert.ToInt32(result);

            if (entity.Atoms.Any())
            {
                await InsertAtomsAsync(connection, compoundId, entity.Atoms);
            }
            if (entity.Bonds.Any())
            {
                await InsertBondsAsync(connection, compoundId, entity.Bonds);
            }
            if (entity.Metadata.Any())
            {
                await InsertMetadataAsync(connection, compoundId, "Compound", entity.Metadata);
            }

            transaction.Commit();
            return compoundId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task UpdateAsync(Compound entity)
    {
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Compounds SET
                    Name = @Name, Description = @Description, Smiles = @Smiles,
                    InChI = @InChI, InChIKey = @InChIKey, Formula = @Formula,
                    MolecularWeight = @MolecularWeight, LogP = @LogP, HBD = @HBD,
                    HBA = @HBA, TPSA = @TPSA, RotatableBonds = @RotatableBonds,
                    SdfFilePath = @SdfFilePath, Mol2FilePath = @Mol2FilePath,
                    CasNumber = @CasNumber, PubChemCid = @PubChemCid,
                    UpdatedAt = @UpdatedAt, Version = Version + 1
                WHERE Id = @Id";

            AddCompoundParameters(command, entity);
            command.Parameters.AddWithValue("@Id", entity.Id);
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            await command.ExecuteNonQueryAsync();
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "UPDATE Compounds SET IsDeleted = 1, UpdatedAt = @UpdatedAt WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<int> CountAsync()
    {
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Compounds WHERE IsDeleted = 0";

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<IEnumerable<Compound>> SearchAsync(string searchTerm)
    {
        var compounds = new List<Compound>();
        using var connection = _dbContext.CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, Description, Smiles, InChI, InChIKey, Formula,
                   MolecularWeight, LogP, HBD, HBA, TPSA, RotatableBonds,
                   SdfFilePath, Mol2FilePath, CasNumber, PubChemCid, MoleculeType,
                   CreatedAt, UpdatedAt, CreatedBy, Version, IsDeleted
            FROM Compounds
            WHERE IsDeleted = 0 AND (Name LIKE @SearchTerm OR Smiles LIKE @SearchTerm OR InChIKey LIKE @SearchTerm)
            ORDER BY Name
            LIMIT 100";
        command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            compounds.Add(MapCompound(reader));
        }
        return compounds;
    }

    private static Compound MapCompound(SqliteDataReader reader)
    {
        return new Compound
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
            Smiles = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
            InChI = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
            InChIKey = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
            Formula = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
            MolecularWeight = reader.IsDBNull(7) ? 0 : reader.GetDouble(7),
            LogP = reader.IsDBNull(8) ? 0 : reader.GetDouble(8),
            HBD = reader.IsDBNull(9) ? 0 : reader.GetDouble(9),
            HBA = reader.IsDBNull(10) ? 0 : reader.GetDouble(10),
            TPSA = reader.IsDBNull(11) ? 0 : reader.GetDouble(11),
            RotatableBonds = reader.IsDBNull(12) ? 0 : reader.GetInt32(12),
            SdfFilePath = reader.IsDBNull(13) ? string.Empty : reader.GetString(13),
            Mol2FilePath = reader.IsDBNull(14) ? string.Empty : reader.GetString(14),
            CasNumber = reader.IsDBNull(15) ? string.Empty : reader.GetString(15),
            PubChemCid = reader.IsDBNull(16) ? string.Empty : reader.GetString(16),
            MoleculeType = (Domain.Enums.MoleculeType)reader.GetInt32(17),
            CreatedAt = DateTime.Parse(reader.GetString(18)),
            UpdatedAt = DateTime.Parse(reader.GetString(19)),
            CreatedBy = reader.IsDBNull(20) ? string.Empty : reader.GetString(20),
            Version = reader.GetInt32(21),
            IsDeleted = reader.GetBoolean(22)
        };
    }

    private static void AddCompoundParameters(SqliteCommand command, Compound entity)
    {
        command.Parameters.AddWithValue("@Name", entity.Name);
        command.Parameters.AddWithValue("@Description", entity.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Smiles", entity.Smiles ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@InChI", entity.InChI ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@InChIKey", entity.InChIKey ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Formula", entity.Formula ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@MolecularWeight", entity.MolecularWeight);
        command.Parameters.AddWithValue("@LogP", entity.LogP);
        command.Parameters.AddWithValue("@HBD", entity.HBD);
        command.Parameters.AddWithValue("@HBA", entity.HBA);
        command.Parameters.AddWithValue("@TPSA", entity.TPSA);
        command.Parameters.AddWithValue("@RotatableBonds", entity.RotatableBonds);
        command.Parameters.AddWithValue("@SdfFilePath", entity.SdfFilePath ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Mol2FilePath", entity.Mol2FilePath ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CasNumber", entity.CasNumber ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@PubChemCid", entity.PubChemCid ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@MoleculeType", (int)entity.MoleculeType);
        command.Parameters.AddWithValue("@Version", entity.Version);
    }

    private static async Task<List<Atom>> GetAtomsAsync(SqliteConnection connection, int compoundId)
    {
        var atoms = new List<Atom>();
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Element, X, Y, Z, Charge, AtomType
            FROM Atoms WHERE CompoundId = @CompoundId
            ORDER BY AtomIndex";
        command.Parameters.AddWithValue("@CompoundId", compoundId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            atoms.Add(new Atom
            {
                Id = reader.GetInt32(0),
                Element = reader.GetString(1),
                X = reader.GetDouble(2),
                Y = reader.GetDouble(3),
                Z = reader.GetDouble(4),
                Charge = reader.IsDBNull(5) ? 0 : reader.GetDouble(5),
                AtomType = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
            });
        }
        return atoms;
    }

    private static async Task<List<Bond>> GetBondsAsync(SqliteConnection connection, int compoundId)
    {
        var bonds = new List<Bond>();
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Atom1Id, Atom2Id, BondOrder, BondType
            FROM Bonds WHERE CompoundId = @CompoundId";
        command.Parameters.AddWithValue("@CompoundId", compoundId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            bonds.Add(new Bond
            {
                Id = reader.GetInt32(0),
                Atom1Id = reader.GetInt32(1),
                Atom2Id = reader.GetInt32(2),
                Order = reader.GetInt32(3),
                BondType = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
            });
        }
        return bonds;
    }

    private static async Task InsertAtomsAsync(SqliteConnection connection, int compoundId, List<Atom> atoms)
    {
        for (int i = 0; i < atoms.Count; i++)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Atoms (CompoundId, AtomIndex, Element, X, Y, Z, Charge, AtomType)
                VALUES (@CompoundId, @AtomIndex, @Element, @X, @Y, @Z, @Charge, @AtomType)";
            command.Parameters.AddWithValue("@CompoundId", compoundId);
            command.Parameters.AddWithValue("@AtomIndex", i);
            command.Parameters.AddWithValue("@Element", atoms[i].Element);
            command.Parameters.AddWithValue("@X", atoms[i].X);
            command.Parameters.AddWithValue("@Y", atoms[i].Y);
            command.Parameters.AddWithValue("@Z", atoms[i].Z);
            command.Parameters.AddWithValue("@Charge", atoms[i].Charge);
            command.Parameters.AddWithValue("@AtomType", atoms[i].AtomType ?? (object)DBNull.Value);
            await command.ExecuteNonQueryAsync();
        }
    }

    private static async Task InsertBondsAsync(SqliteConnection connection, int compoundId, List<Bond> bonds)
    {
        foreach (var bond in bonds)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Bonds (CompoundId, Atom1Id, Atom2Id, BondOrder, BondType)
                VALUES (@CompoundId, @Atom1Id, @Atom2Id, @BondOrder, @BondType)";
            command.Parameters.AddWithValue("@CompoundId", compoundId);
            command.Parameters.AddWithValue("@Atom1Id", bond.Atom1Id);
            command.Parameters.AddWithValue("@Atom2Id", bond.Atom2Id);
            command.Parameters.AddWithValue("@BondOrder", bond.Order);
            command.Parameters.AddWithValue("@BondType", bond.BondType ?? (object)DBNull.Value);
            await command.ExecuteNonQueryAsync();
        }
    }

    private static async Task<List<MetadataEntry>> GetMetadataAsync(SqliteConnection connection, int parentId, string parentType)
    {
        var metadata = new List<MetadataEntry>();
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, ParentId, ParentType, MetadataKey, MetadataValue, ValueType, Description, CreatedAt
            FROM MetadataEntries
            WHERE ParentId = @ParentId AND ParentType = @ParentType";
        command.Parameters.AddWithValue("@ParentId", parentId);
        command.Parameters.AddWithValue("@ParentType", parentType);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            metadata.Add(new MetadataEntry
            {
                Id = reader.GetInt32(0),
                ParentId = reader.GetInt32(1),
                ParentType = reader.GetString(2),
                Key = reader.GetString(3),
                Value = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                ValueType = reader.IsDBNull(5) ? "string" : reader.GetString(5),
                Description = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                CreatedAt = DateTime.Parse(reader.GetString(7))
            });
        }
        return metadata;
    }

    private static async Task InsertMetadataAsync(SqliteConnection connection, int parentId, string parentType, List<MetadataEntry> metadata)
    {
        foreach (var entry in metadata)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO MetadataEntries (ParentId, ParentType, MetadataKey, MetadataValue, ValueType, Description, CreatedAt)
                VALUES (@ParentId, @ParentType, @MetadataKey, @MetadataValue, @ValueType, @Description, @CreatedAt)";
            command.Parameters.AddWithValue("@ParentId", parentId);
            command.Parameters.AddWithValue("@ParentType", parentType);
            command.Parameters.AddWithValue("@MetadataKey", entry.Key);
            command.Parameters.AddWithValue("@MetadataValue", entry.Value ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ValueType", entry.ValueType);
            command.Parameters.AddWithValue("@Description", entry.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            await command.ExecuteNonQueryAsync();
        }
    }
}
