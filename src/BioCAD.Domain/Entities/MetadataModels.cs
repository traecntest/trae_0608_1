namespace BioCAD.Domain.Entities;

public class MetadataEntry
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public string ParentType { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string ValueType { get; set; } = "string";
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class VersionRecord
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public string ParentType { get; set; } = string.Empty;
    public int VersionNumber { get; set; }
    public string ChangeDescription { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.Now;
    public string PreviousVersionData { get; set; } = string.Empty;
}

public class DataProvenance
{
    public int Id { get; set; }
    public int EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string OriginalFormat { get; set; } = string.Empty;
    public string ImportedBy { get; set; } = string.Empty;
    public DateTime ImportedAt { get; set; } = DateTime.Now;
    public string TransformationHistory { get; set; } = string.Empty;
    public string QualityControlStatus { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
