using Microsoft.Data.Sqlite;

namespace BioCAD.Data;

public class BioCADDbContext
{
    private readonly string _connectionString;

    public BioCADDbContext(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
    }

    public SqliteConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }

    public void InitializeDatabase()
    {
        using var connection = CreateConnection();
        connection.Open();
        CreateTables(connection);
        CreateIndexes(connection);
    }

    private void CreateTables(SqliteConnection connection)
    {
        var createTableSql = GetCreateTableSql();
        using var command = new SqliteCommand(createTableSql, connection);
        command.ExecuteNonQuery();
    }

    private void CreateIndexes(SqliteConnection connection)
    {
        var indexSql = GetIndexSql();
        using var command = new SqliteCommand(indexSql, connection);
        command.ExecuteNonQuery();
    }

    private string GetCreateTableSql()
    {
        return @"
CREATE TABLE IF NOT EXISTS Proteins (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT,
    Sequence TEXT,
    GeneName TEXT,
    Organism TEXT,
    SequenceLength INTEGER,
    MolecularWeight REAL,
    IsoelectricPoint REAL,
    PdbId TEXT,
    Family TEXT,
    SecondaryStructure TEXT,
    PdbFilePath TEXT,
    UniProtId TEXT,
    MoleculeType INTEGER DEFAULT 0,
    CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
    CreatedBy TEXT,
    Version INTEGER DEFAULT 1,
    IsDeleted INTEGER DEFAULT 0
);

CREATE TABLE IF NOT EXISTS Compounds (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT,
    Smiles TEXT,
    InChI TEXT,
    InChIKey TEXT,
    Formula TEXT,
    MolecularWeight REAL,
    LogP REAL,
    HBD REAL,
    HBA REAL,
    TPSA REAL,
    RotatableBonds INTEGER,
    SdfFilePath TEXT,
    Mol2FilePath TEXT,
    CasNumber TEXT,
    PubChemCid TEXT,
    MoleculeType INTEGER DEFAULT 1,
    CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
    CreatedBy TEXT,
    Version INTEGER DEFAULT 1,
    IsDeleted INTEGER DEFAULT 0
);

CREATE TABLE IF NOT EXISTS Atoms (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CompoundId INTEGER,
    AtomIndex INTEGER,
    Element TEXT,
    X REAL,
    Y REAL,
    Z REAL,
    Charge REAL,
    AtomType TEXT,
    FOREIGN KEY (CompoundId) REFERENCES Compounds(Id)
);

CREATE TABLE IF NOT EXISTS Bonds (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CompoundId INTEGER,
    Atom1Id INTEGER,
    Atom2Id INTEGER,
    BondOrder INTEGER,
    BondType TEXT,
    FOREIGN KEY (CompoundId) REFERENCES Compounds(Id)
);

CREATE TABLE IF NOT EXISTS GenomicData (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT,
    Chromosome TEXT,
    StartPosition INTEGER,
    EndPosition INTEGER,
    Strand TEXT,
    Sequence TEXT,
    GeneId TEXT,
    TranscriptId TEXT,
    Organism TEXT,
    AssemblyVersion TEXT,
    SequenceType TEXT,
    SequenceLength INTEGER,
    GCContent REAL,
    DataSource INTEGER,
    CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
    CreatedBy TEXT,
    Version INTEGER DEFAULT 1,
    IsDeleted INTEGER DEFAULT 0
);

CREATE TABLE IF NOT EXISTS ActivityData (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT,
    Description TEXT,
    CompoundId INTEGER,
    TargetId INTEGER,
    ExperimentType INTEGER,
    AssayName TEXT,
    ActivityType TEXT,
    ActivityValue REAL,
    Unit TEXT,
    StandardDeviation REAL,
    Relation TEXT,
    TargetName TEXT,
    CellLine TEXT,
    AssayConditions TEXT,
    DataSource INTEGER,
    Reference TEXT,
    ExperimentDate TEXT,
    CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
    CreatedBy TEXT,
    Version INTEGER DEFAULT 1,
    IsDeleted INTEGER DEFAULT 0,
    FOREIGN KEY (CompoundId) REFERENCES Compounds(Id),
    FOREIGN KEY (TargetId) REFERENCES Proteins(Id)
);

CREATE TABLE IF NOT EXISTS MetadataEntries (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ParentId INTEGER,
    ParentType TEXT,
    MetadataKey TEXT,
    MetadataValue TEXT,
    ValueType TEXT,
    Description TEXT,
    CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS VersionRecords (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ParentId INTEGER,
    ParentType TEXT,
    VersionNumber INTEGER,
    ChangeDescription TEXT,
    ChangedBy TEXT,
    ChangedAt TEXT DEFAULT CURRENT_TIMESTAMP,
    PreviousVersionData TEXT
);

CREATE TABLE IF NOT EXISTS DataProvenance (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    EntityId INTEGER,
    EntityType TEXT,
    Source TEXT,
    SourceType TEXT,
    OriginalFormat TEXT,
    ImportedBy TEXT,
    ImportedAt TEXT DEFAULT CURRENT_TIMESTAMP,
    TransformationHistory TEXT,
    QualityControlStatus TEXT,
    Notes TEXT
);

CREATE TABLE IF NOT EXISTS ComputationTasks (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT,
    Description TEXT,
    TaskType INTEGER,
    TaskStatus INTEGER,
    ParametersJson TEXT,
    InputData TEXT,
    OutputData TEXT,
    LogFilePath TEXT,
    ResultFilePath TEXT,
    StartedAt TEXT,
    CompletedAt TEXT,
    Progress REAL DEFAULT 0,
    CurrentStep TEXT,
    Priority INTEGER DEFAULT 5,
    CpuCores INTEGER DEFAULT 1,
    UseGpu INTEGER DEFAULT 0,
    ErrorMessage TEXT,
    UserId TEXT,
    QueueName TEXT DEFAULT 'default',
    CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
    CreatedBy TEXT,
    Version INTEGER DEFAULT 1,
    IsDeleted INTEGER DEFAULT 0
);

CREATE TABLE IF NOT EXISTS TaskLogs (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TaskId INTEGER,
    Timestamp TEXT DEFAULT CURRENT_TIMESTAMP,
    LogLevel TEXT,
    Message TEXT,
    Progress REAL,
    FOREIGN KEY (TaskId) REFERENCES ComputationTasks(Id)
);";
    }

    private string GetIndexSql()
    {
        return @"
CREATE INDEX IF NOT EXISTS idx_compounds_smiles ON Compounds(Smiles);
CREATE INDEX IF NOT EXISTS idx_compounds_inchikey ON Compounds(InChIKey);
CREATE INDEX IF NOT EXISTS idx_compounds_name ON Compounds(Name);
CREATE INDEX IF NOT EXISTS idx_proteins_name ON Proteins(Name);
CREATE INDEX IF NOT EXISTS idx_proteins_gene ON Proteins(GeneName);
CREATE INDEX IF NOT EXISTS idx_activity_compound ON ActivityData(CompoundId);
CREATE INDEX IF NOT EXISTS idx_activity_target ON ActivityData(TargetId);
CREATE INDEX IF NOT EXISTS idx_tasks_status ON ComputationTasks(TaskStatus);
CREATE INDEX IF NOT EXISTS idx_tasks_type ON ComputationTasks(TaskType);
CREATE INDEX IF NOT EXISTS idx_tasks_queue ON ComputationTasks(QueueName);
CREATE INDEX IF NOT EXISTS idx_tasklogs_task ON TaskLogs(TaskId);
CREATE INDEX IF NOT EXISTS idx_metadata_parent ON MetadataEntries(ParentId, ParentType);
CREATE INDEX IF NOT EXISTS idx_versions_parent ON VersionRecords(ParentId, ParentType);";
    }
}
