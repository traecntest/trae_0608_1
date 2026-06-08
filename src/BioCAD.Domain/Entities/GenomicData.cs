using BioCAD.Domain.Enums;

namespace BioCAD.Domain.Entities;

public class GenomicData : EntityBase
{
    public string Chromosome { get; set; } = string.Empty;
    public long StartPosition { get; set; }
    public long EndPosition { get; set; }
    public string Strand { get; set; } = string.Empty;
    public string Sequence { get; set; } = string.Empty;
    public string GeneId { get; set; } = string.Empty;
    public string TranscriptId { get; set; } = string.Empty;
    public string Organism { get; set; } = string.Empty;
    public string AssemblyVersion { get; set; } = string.Empty;
    public string SequenceType { get; set; } = string.Empty;
    public int SequenceLength { get; set; }
    public double GCContent { get; set; }
    public DataSource DataSource { get; set; } = DataSource.Imported;
    public List<MetadataEntry> Metadata { get; set; } = new();
    public List<VersionRecord> VersionHistory { get; set; } = new();
}
