namespace BioCAD.Domain.Enums;

public enum MoleculeType
{
    Protein,
    Compound,
    NucleicAcid,
    Complex
}

public enum DataSource
{
    Imported,
    Calculated,
    Experimental,
    Predicted
}

public enum ExperimentType
{
    BindingAffinity,
    EnzymeActivity,
    CellAssay,
    ADMET,
    Other
}

public enum TaskStatus
{
    Pending,
    Queued,
    Running,
    Completed,
    Failed,
    Cancelled,
    Paused
}

public enum TaskType
{
    MolecularDocking,
    VirtualScreening,
    PharmacophoreModeling,
    MolecularDynamics,
    QSARModeling,
    Clustering,
    DataAnalysis
}
