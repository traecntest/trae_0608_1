using BioCAD.Domain.Enums;

namespace BioCAD.Domain.Entities;

public class Compound : EntityBase
{
    public string Smiles { get; set; } = string.Empty;
    public string InChI { get; set; } = string.Empty;
    public string InChIKey { get; set; } = string.Empty;
    public string Formula { get; set; } = string.Empty;
    public double MolecularWeight { get; set; }
    public double LogP { get; set; }
    public double HBD { get; set; }
    public double HBA { get; set; }
    public double TPSA { get; set; }
    public int RotatableBonds { get; set; }
    public string SdfFilePath { get; set; } = string.Empty;
    public string Mol2FilePath { get; set; } = string.Empty;
    public string CasNumber { get; set; } = string.Empty;
    public string PubChemCid { get; set; } = string.Empty;
    public MoleculeType MoleculeType { get; set; } = MoleculeType.Compound;
    public List<Atom> Atoms { get; set; } = new();
    public List<Bond> Bonds { get; set; } = new();
    public List<MetadataEntry> Metadata { get; set; } = new();
    public List<ActivityData> Activities { get; set; } = new();
    public List<VersionRecord> VersionHistory { get; set; } = new();
}

public class Atom
{
    public int Id { get; set; }
    public string Element { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double Charge { get; set; }
    public string AtomType { get; set; } = string.Empty;
}

public class Bond
{
    public int Id { get; set; }
    public int Atom1Id { get; set; }
    public int Atom2Id { get; set; }
    public int Order { get; set; }
    public string BondType { get; set; } = string.Empty;
}
