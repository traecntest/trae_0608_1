using BioCAD.Domain.Enums;

namespace BioCAD.Domain.Entities;

public class ActivityData : EntityBase
{
    public int CompoundId { get; set; }
    public int TargetId { get; set; }
    public ExperimentType ExperimentType { get; set; }
    public string AssayName { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public double ActivityValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public double StandardDeviation { get; set; }
    public string Relation { get; set; } = "=";
    public string TargetName { get; set; } = string.Empty;
    public string CellLine { get; set; } = string.Empty;
    public string AssayConditions { get; set; } = string.Empty;
    public DataSource DataSource { get; set; } = DataSource.Experimental;
    public string Reference { get; set; } = string.Empty;
    public DateTime ExperimentDate { get; set; } = DateTime.Now;
    public Compound? Compound { get; set; }
    public Protein? Target { get; set; }
}
