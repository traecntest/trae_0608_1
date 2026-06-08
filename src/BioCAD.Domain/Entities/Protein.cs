using BioCAD.Domain.Enums;

namespace BioCAD.Domain.Entities;

public class Protein : EntityBase
{
    public string Sequence { get; set; } = string.Empty;
    public string GeneName { get; set; } = string.Empty;
    public string Organism { get; set; } = string.Empty;
    public int SequenceLength { get; set; }
    public double MolecularWeight { get; set; }
    public double IsoelectricPoint { get; set; }
    public string PdbId { get; set; } = string.Empty;
    public string Family { get; set; } = string.Empty;
    public string SecondaryStructure { get; set; } = string.Empty;
    public string PdbFilePath { get; set; } = string.Empty;
    public string UniProtId { get; set; } = string.Empty;
    public MoleculeType MoleculeType { get; set; } = MoleculeType.Protein;
    public List<MetadataEntry> Metadata { get; set; } = new();
    public List<ActivityData> Activities { get; set; } = new();
    public List<VersionRecord> VersionHistory { get; set; } = new();
}
