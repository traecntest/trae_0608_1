using BioCAD.Domain.Entities;

namespace BioCAD.Data.ImportExport;

public interface ISequenceParser
{
    List<Protein> ParseFasta(string filePath);
    Task<List<Protein>> ParseFastaAsync(string filePath);
    string ExportFasta(IEnumerable<Protein> proteins);
}

public class FastaParser : ISequenceParser
{
    public List<Protein> ParseFasta(string filePath)
    {
        var proteins = new List<Protein>();
        var lines = File.ReadAllLines(filePath);

        var currentProtein = new Protein();
        var sequenceBuilder = new System.Text.StringBuilder();
        bool hasHeader = false;

        foreach (var line in lines)
        {
            if (line.StartsWith(">"))
            {
                if (hasHeader && sequenceBuilder.Length > 0)
                {
                    currentProtein.Sequence = sequenceBuilder.ToString();
                    currentProtein.SequenceLength = currentProtein.Sequence.Length;
                    CalculateProteinProperties(currentProtein);
                    proteins.Add(currentProtein);
                }

                currentProtein = new Protein();
                sequenceBuilder.Clear();
                hasHeader = true;

                var header = line.Substring(1).Trim();
                var headerParts = header.Split(new[] { ' ', '\t' }, 2);
                currentProtein.Name = headerParts[0];

                if (headerParts.Length > 1)
                {
                    currentProtein.Description = headerParts[1];
                }

                if (header.Contains("|"))
                {
                    var parts = header.Split('|');
                    if (parts.Length >= 2)
                    {
                        currentProtein.UniProtId = parts[1].Trim();
                    }
                }
            }
            else if (!string.IsNullOrWhiteSpace(line) && hasHeader)
            {
                sequenceBuilder.Append(line.Trim());
            }
        }

        if (hasHeader && sequenceBuilder.Length > 0)
        {
            currentProtein.Sequence = sequenceBuilder.ToString();
            currentProtein.SequenceLength = currentProtein.Sequence.Length;
            CalculateProteinProperties(currentProtein);
            proteins.Add(currentProtein);
        }

        return proteins;
    }

    public async Task<List<Protein>> ParseFastaAsync(string filePath)
    {
        return await Task.Run(() => ParseFasta(filePath));
    }

    public string ExportFasta(IEnumerable<Protein> proteins)
    {
        var fasta = new System.Text.StringBuilder();
        foreach (var protein in proteins)
        {
            fasta.AppendLine($">{protein.Name} {protein.Description}".Trim());
            var sequence = protein.Sequence;
            for (int i = 0; i < sequence.Length; i += 80)
            {
                int length = Math.Min(80, sequence.Length - i);
                fasta.AppendLine(sequence.Substring(i, length));
            }
        }
        return fasta.ToString();
    }

    private static void CalculateProteinProperties(Protein protein)
    {
        if (protein.SequenceLength == 0) return;

        var aaWeights = new Dictionary<char, double>
        {
            {'A', 71.0371}, {'R', 156.1011}, {'N', 114.0429}, {'D', 115.0269},
            {'C', 103.0092}, {'E', 129.0426}, {'Q', 128.0586}, {'G', 57.0215},
            {'H', 137.0589}, {'I', 113.0841}, {'L', 113.0841}, {'K', 128.0950},
            {'M', 131.0405}, {'F', 147.0684}, {'P', 97.0528}, {'S', 87.0320},
            {'T', 101.0477}, {'W', 186.0793}, {'Y', 163.0633}, {'V', 99.0684}
        };

        double totalWeight = 18.0106;
        var aaCounts = new Dictionary<char, int>();
        foreach (var aa in protein.Sequence)
        {
            var upper = char.ToUpper(aa);
            if (!aaCounts.ContainsKey(upper))
                aaCounts[upper] = 0;
            aaCounts[upper]++;

            if (aaWeights.TryGetValue(upper, out double weight))
            {
                totalWeight += weight;
            }
        }

        protein.MolecularWeight = Math.Round(totalWeight, 2);

        int basicCount = 0, acidicCount = 0;
        if (aaCounts.ContainsKey('K')) basicCount += aaCounts['K'];
        if (aaCounts.ContainsKey('R')) basicCount += aaCounts['R'];
        if (aaCounts.ContainsKey('H')) basicCount += aaCounts['H'];
        if (aaCounts.ContainsKey('D')) acidicCount += aaCounts['D'];
        if (aaCounts.ContainsKey('E')) acidicCount += aaCounts['E'];

        protein.IsoelectricPoint = 7.0 + (basicCount - acidicCount) * 0.2;
        protein.IsoelectricPoint = Math.Round(protein.IsoelectricPoint, 2);
    }
}
