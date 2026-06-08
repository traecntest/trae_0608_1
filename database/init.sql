-- BioCAD 生物计算平台数据库初始化脚本
-- 数据库类型: SQLite
-- 版本: 1.0.0

-- ============================================================
-- 蛋白质表
-- ============================================================
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

-- ============================================================
-- 化合物表
-- ============================================================
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

-- ============================================================
-- 原子表
-- ============================================================
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

-- ============================================================
-- 键表
-- ============================================================
CREATE TABLE IF NOT EXISTS Bonds (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CompoundId INTEGER,
    Atom1Id INTEGER,
    Atom2Id INTEGER,
    BondOrder INTEGER,
    BondType TEXT,
    FOREIGN KEY (CompoundId) REFERENCES Compounds(Id)
);

-- ============================================================
-- 基因组数据表
-- ============================================================
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

-- ============================================================
-- 活性数据表
-- ============================================================
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

-- ============================================================
-- 元数据表
-- ============================================================
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

-- ============================================================
-- 版本记录表
-- ============================================================
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

-- ============================================================
-- 数据溯源表
-- ============================================================
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

-- ============================================================
-- 计算任务表
-- ============================================================
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

-- ============================================================
-- 任务日志表
-- ============================================================
CREATE TABLE IF NOT EXISTS TaskLogs (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TaskId INTEGER,
    Timestamp TEXT DEFAULT CURRENT_TIMESTAMP,
    LogLevel TEXT,
    Message TEXT,
    Progress REAL,
    FOREIGN KEY (TaskId) REFERENCES ComputationTasks(Id)
);

-- ============================================================
-- 索引
-- ============================================================
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
CREATE INDEX IF NOT EXISTS idx_versions_parent ON VersionRecords(ParentId, ParentType);

-- ============================================================
-- 示例数据
-- ============================================================

-- 插入示例蛋白质
INSERT INTO Proteins (Name, Description, Sequence, GeneName, Organism, SequenceLength, MolecularWeight, IsoelectricPoint, PdbId, Family, UniProtId)
VALUES 
('EGFR', 'Epidermal growth factor receptor', 'MRPSGTAGAALLALLAALCPASSR...', 'EGFR', 'Homo sapiens', 1210, 134.2, 6.8, '1M17', 'Receptor tyrosine kinase', 'P00533'),
('AKT1', 'RAC-alpha serine/threonine-protein kinase', 'MSDVAIVKEGWLHKRGEYIKTWR...', 'AKT1', 'Homo sapiens', 480, 55.7, 5.8, '4EJN', 'AGC kinase', 'P31749'),
('CYP3A4', 'Cytochrome P450 3A4', 'MALIPDLAMETWLLLAVSLV...', 'CYP3A4', 'Homo sapiens', 503, 57.3, 7.5, '4K9T', 'Cytochrome P450', 'P08684');

-- 插入示例化合物
INSERT INTO Compounds (Name, Description, Smiles, Formula, MolecularWeight, LogP, HBD, HBA, TPSA, RotatableBonds, CasNumber)
VALUES 
('Aspirin', 'Acetylsalicylic acid - NSAID', 'CC(=O)OC1=CC=CC=C1C(=O)O', 'C9H8O4', 180.16, 1.2, 1, 3, 63.6, 3, '50-78-2'),
('Paracetamol', 'Acetaminophen - analgesic', 'CC(=O)NC1=CC=C(C=C1)O', 'C8H9NO2', 151.16, 0.5, 2, 2, 49.3, 2, '103-90-2'),
('Ibuprofen', 'Nonsteroidal anti-inflammatory drug', 'CC(C)CC1=CC=C(C=C1)C(C)C(=O)O', 'C13H18O2', 206.28, 3.5, 1, 2, 37.3, 4, '15687-27-1'),
('Gefitinib', 'EGFR inhibitor - anticancer', 'C1=CC2=C(C=C1C(=O)N)N=C(N2)NCCCOC3=CC(=C(C=C3)Cl)F', 'C22H24ClFN4O3', 446.9, 3.2, 2, 6, 84.5, 8, '184475-35-2'),
('Metformin', 'Antidiabetic drug', 'CN(C)C(=N)N=C(N)N', 'C4H11N5', 129.16, -2.6, 4, 5, 97.2, 1, '657-24-9');

-- 插入示例活性数据
INSERT INTO ActivityData (Name, CompoundId, TargetId, ExperimentType, AssayName, ActivityType, ActivityValue, Unit, TargetName, DataSource, Reference)
VALUES 
('EGFR-Gefitinib binding', 4, 1, 0, 'Kinase assay', 'IC50', 33.0, 'nM', 'EGFR', 2, 'Nature 2002'),
('CYP3A4-Metformin', 5, 3, 1, 'Metabolism assay', 'Km', 1250.0, 'uM', 'CYP3A4', 2, 'Drug Metab Dispos 2018');

print '数据库初始化完成';
