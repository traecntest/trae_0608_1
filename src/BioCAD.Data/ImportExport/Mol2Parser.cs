using BioCAD.Domain.Entities;
using System.Globalization;

namespace BioCAD.Data.ImportExport;

public class Mol2Parser : IStructureFileParser
{
    public List<Compound> Parse(string filePath)
    {
        var compounds = new List<Compound>();
        var content = File.ReadAllText(filePath);
        var molecules = content.Split(new[] { "@<TRIPOS>MOLECULE" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var molContent in molecules)
        {
            var compound = ParseMol2Molecule(molContent);
            if (compound != null)
            {
                compounds.Add(compound);
            }
        }

        if (compounds.Count == 0 && content.Trim().Length > 0)
        {
            var compound = ParseMol2Molecule(content);
            if (compound != null)
            {
                compounds.Add(compound);
            }
        }

        return compounds;
    }

    public async Task<List<Compound>> ParseAsync(string filePath)
    {
        return await Task.Run(() => Parse(filePath));
    }

    private static Compound? ParseMol2Molecule(string molContent)
    {
        var lines = molContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        if (lines.Count == 0) return null;

        var compound = new Compound();
        int i = 0;

        while (i < lines.Count)
        {
            var line = lines[i].Trim();

            if (line.StartsWith("@<TRIPOS>"))
            {
                var section = line.Substring(9).Trim();
                i++;

                switch (section.ToUpper())
                {
                    case "MOLECULE":
                        ParseMoleculeSection(lines, ref i, compound);
                        break;
                    case "ATOM":
                        ParseAtomSection(lines, ref i, compound);
                        break;
                    case "BOND":
                        ParseBondSection(lines, ref i, compound);
                        break;
                    default:
                        while (i < lines.Count && !lines[i].StartsWith("@<TRIPOS>"))
                        {
                            i++;
                        }
                        break;
                }
            }
            else
            {
                i++;
            }
        }

        return compound;
    }

    private static void ParseMoleculeSection(List<string> lines, ref int currentIndex, Compound compound)
    {
        if (currentIndex < lines.Count)
        {
            compound.Name = lines[currentIndex].Trim();
            currentIndex++;
        }

        if (currentIndex < lines.Count)
        {
            var counts = lines[currentIndex].Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            currentIndex++;
        }

        if (currentIndex < lines.Count)
        {
            var type = lines[currentIndex].Trim();
            currentIndex++;
        }

        while (currentIndex < lines.Count && !lines[currentIndex].StartsWith("@<TRIPOS>"))
        {
            currentIndex++;
        }
    }

    private static void ParseAtomSection(List<string> lines, ref int currentIndex, Compound compound)
    {
        while (currentIndex < lines.Count && !lines[currentIndex].StartsWith("@<TRIPOS>"))
        {
            var parts = lines[currentIndex].Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 5)
            {
                var atom = new Atom
                {
                    Id = int.Parse(parts[0]),
                    X = double.Parse(parts[2], CultureInfo.InvariantCulture),
                    Y = double.Parse(parts[3], CultureInfo.InvariantCulture),
                    Z = double.Parse(parts[4], CultureInfo.InvariantCulture),
                    AtomType = parts[5]
                };

                var elementPart = parts[5].Split('.')[0];
                atom.Element = new string(elementPart.TakeWhile(char.IsLetter).ToArray());

                if (parts.Length > 8 && double.TryParse(parts[8], NumberStyles.Any, CultureInfo.InvariantCulture, out double charge))
                {
                    atom.Charge = charge;
                }

                compound.Atoms.Add(atom);
            }
            currentIndex++;
        }
    }

    private static void ParseBondSection(List<string> lines, ref int currentIndex, Compound compound)
    {
        while (currentIndex < lines.Count && !lines[currentIndex].StartsWith("@<TRIPOS>"))
        {
            var parts = lines[currentIndex].Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 4)
            {
                var bond = new Bond
                {
                    Id = int.Parse(parts[0]),
                    Atom1Id = int.Parse(parts[1]),
                    Atom2Id = int.Parse(parts[2]),
                    BondType = parts[3]
                };

                bond.Order = parts[3] switch
                {
                    "1" or "ar" => 1,
                    "2" => 2,
                    "3" => 3,
                    "am" => 1,
                    _ => 1
                };

                compound.Bonds.Add(bond);
            }
            currentIndex++;
        }
    }
}
