using BioCAD.Domain.Entities;
using System.Globalization;
using System.Text;

namespace BioCAD.Data.ImportExport;

public interface IStructureFileParser
{
    List<Compound> Parse(string filePath);
    Task<List<Compound>> ParseAsync(string filePath);
}

public class SdfParser : IStructureFileParser
{
    public List<Compound> Parse(string filePath)
    {
        var compounds = new List<Compound>();
        var lines = File.ReadAllLines(filePath);
        var currentCompound = new StringBuilder();
        bool inCompound = false;

        foreach (var line in lines)
        {
            currentCompound.AppendLine(line);

            if (line.Trim() == "$$$$")
            {
                var compound = ParseSdfEntry(currentCompound.ToString());
                if (compound != null)
                {
                    compounds.Add(compound);
                }
                currentCompound.Clear();
                inCompound = false;
            }
        }

        if (currentCompound.Length > 0)
        {
            var compound = ParseSdfEntry(currentCompound.ToString());
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

    private static Compound? ParseSdfEntry(string sdfEntry)
    {
        var lines = sdfEntry.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        if (lines.Count < 4) return null;

        var compound = new Compound
        {
            Name = lines[0].Trim(),
            Description = lines[1].Trim()
        };

        if (lines.Count >= 4 && int.TryParse(lines[3].Substring(0, 3).Trim(), out int atomCount))
        {
            int bondCount = 0;
            if (lines[3].Length >= 6 && int.TryParse(lines[3].Substring(3, 3).Trim(), out int bc))
            {
                bondCount = bc;
            }

            int atomStartIndex = 4;
            for (int i = 0; i < atomCount && atomStartIndex + i < lines.Count; i++)
            {
                var atomLine = lines[atomStartIndex + i];
                if (atomLine.Length >= 34)
                {
                    var atom = new Atom
                    {
                        Id = i + 1,
                        X = double.Parse(atomLine.Substring(0, 10).Trim(), CultureInfo.InvariantCulture),
                        Y = double.Parse(atomLine.Substring(10, 10).Trim(), CultureInfo.InvariantCulture),
                        Z = double.Parse(atomLine.Substring(20, 10).Trim(), CultureInfo.InvariantCulture),
                        Element = atomLine.Substring(31, 3).Trim()
                    };
                    compound.Atoms.Add(atom);
                }
            }

            int bondStartIndex = atomStartIndex + atomCount;
            for (int i = 0; i < bondCount && bondStartIndex + i < lines.Count; i++)
            {
                var bondLine = lines[bondStartIndex + i];
                if (bondLine.Length >= 9)
                {
                    var bond = new Bond
                    {
                        Id = i + 1,
                        Atom1Id = int.Parse(bondLine.Substring(0, 3).Trim()),
                        Atom2Id = int.Parse(bondLine.Substring(3, 3).Trim()),
                        Order = int.Parse(bondLine.Substring(6, 3).Trim())
                    };
                    compound.Bonds.Add(bond);
                }
            }

            CalculateCompoundProperties(compound);
        }

        ParseSdfProperties(lines, compound);
        return compound;
    }

    private static void ParseSdfProperties(List<string> lines, Compound compound)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].StartsWith("> "))
            {
                var propName = lines[i].Trim('>', ' ', '<', '>');
                var propValue = string.Empty;
                int j = i + 1;
                while (j < lines.Count && !lines[j].StartsWith("> ") && lines[j].Trim() != "$$$$")
                {
                    propValue += lines[j] + " ";
                    j++;
                }
                propValue = propValue.Trim();

                switch (propName.ToLower())
                {
                    case "smiles":
                        compound.Smiles = propValue;
                        break;
                    case "inchi":
                        compound.InChI = propValue;
                        break;
                    case "inchikey":
                        compound.InChIKey = propValue;
                        break;
                    case "formula":
                        compound.Formula = propValue;
                        break;
                    case "molecular weight":
                    case "mol_weight":
                    case "mw":
                        if (double.TryParse(propValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double mw))
                            compound.MolecularWeight = mw;
                        break;
                    case "logp":
                        if (double.TryParse(propValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double logp))
                            compound.LogP = logp;
                        break;
                    case "hbd":
                        if (double.TryParse(propValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double hbd))
                            compound.HBD = hbd;
                        break;
                    case "hba":
                        if (double.TryParse(propValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double hba))
                            compound.HBA = hba;
                        break;
                    case "tpsa":
                        if (double.TryParse(propValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double tpsa))
                            compound.TPSA = tpsa;
                        break;
                    case "rotatable_bonds":
                    case "rotbonds":
                        if (int.TryParse(propValue, out int rotb))
                            compound.RotatableBonds = rotb;
                        break;
                    case "cas":
                    case "cas_number":
                        compound.CasNumber = propValue;
                        break;
                    case "pubchem_cid":
                    case "cid":
                        compound.PubChemCid = propValue;
                        break;
                    default:
                        compound.Metadata.Add(new MetadataEntry
                        {
                            Key = propName,
                            Value = propValue
                        });
                        break;
                }

                i = j - 1;
            }
        }
    }

    private static void CalculateCompoundProperties(Compound compound)
    {
        if (compound.MolecularWeight == 0 && compound.Atoms.Count > 0)
        {
            var atomicWeights = new Dictionary<string, double>
            {
                {"H", 1.008}, {"C", 12.011}, {"N", 14.007}, {"O", 15.999},
                {"F", 18.998}, {"P", 30.974}, {"S", 32.06}, {"Cl", 35.45},
                {"Br", 79.904}, {"I", 126.904}
            };

            double mw = 0;
            foreach (var atom in compound.Atoms)
            {
                if (atomicWeights.TryGetValue(atom.Element, out double w))
                {
                    mw += w;
                }
            }
            compound.MolecularWeight = Math.Round(mw, 2);
        }

        if (compound.Formula == string.Empty && compound.Atoms.Count > 0)
        {
            var elementCounts = new Dictionary<string, int>();
            foreach (var atom in compound.Atoms)
            {
                if (!elementCounts.ContainsKey(atom.Element))
                    elementCounts[atom.Element] = 0;
                elementCounts[atom.Element]++;
            }

            var formula = new StringBuilder();
            var order = new[] { "C", "H", "N", "O", "S", "P", "F", "Cl", "Br", "I" };
            foreach (var el in order)
            {
                if (elementCounts.TryGetValue(el, out int count))
                {
                    formula.Append(el);
                    if (count > 1) formula.Append(count);
                }
            }
            foreach (var kvp in elementCounts)
            {
                if (!order.Contains(kvp.Key))
                {
                    formula.Append(kvp.Key);
                    if (kvp.Value > 1) formula.Append(kvp.Value);
                }
            }
            compound.Formula = formula.ToString();
        }
    }
}
