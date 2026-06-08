using BioCAD.Domain.Entities;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace BioCAD.Data.ImportExport;

public interface IDataExportService
{
    void ExportToCsv<T>(IEnumerable<T> data, string filePath) where T : class;
    IEnumerable<T> ImportFromCsv<T>(string filePath) where T : class, new();
    void ExportToExcel<T>(IEnumerable<T> data, string filePath, string sheetName = "Data") where T : class;
    IEnumerable<T> ImportFromExcel<T>(string filePath, string sheetName = "Data") where T : class, new();
}

public class DataExportService : IDataExportService
{
    public void ExportToCsv<T>(IEnumerable<T> data, string filePath) where T : class
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ",",
            HasHeaderRecord = true
        };

        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, config);
        csv.WriteRecords(data);
    }

    public IEnumerable<T> ImportFromCsv<T>(string filePath) where T : class, new()
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ",",
            HasHeaderRecord = true,
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);
        var records = new List<T>();
        var recordsEnumerable = csv.GetRecords<T>();
        records.AddRange(recordsEnumerable);
        return records;
    }

    public void ExportToExcel<T>(IEnumerable<T> data, string filePath, string sheetName = "Data") where T : class
    {
        using var package = new OfficeOpenXml.ExcelPackage(new FileInfo(filePath));
        var worksheet = package.Workbook.Worksheets.Add(sheetName);

        var properties = typeof(T).GetProperties()
            .Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string) || p.PropertyType == typeof(DateTime))
            .ToList();

        for (int col = 0; col < properties.Count; col++)
        {
            worksheet.Cells[1, col + 1].Value = properties[col].Name;
        }

        int row = 2;
        foreach (var item in data)
        {
            for (int col = 0; col < properties.Count; col++)
            {
                var value = properties[col].GetValue(item);
                worksheet.Cells[row, col + 1].Value = value ?? string.Empty;
            }
            row++;
        }

        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        package.Save();
    }

    public IEnumerable<T> ImportFromExcel<T>(string filePath, string sheetName = "Data") where T : class, new()
    {
        var results = new List<T>();

        using var package = new OfficeOpenXml.ExcelPackage(new FileInfo(filePath));
        var worksheet = package.Workbook.Worksheets[sheetName] ?? package.Workbook.Worksheets.FirstOrDefault();
        if (worksheet == null) return results;

        int rowCount = worksheet.Dimension?.Rows ?? 0;
        int colCount = worksheet.Dimension?.Columns ?? 0;
        if (rowCount < 2) return results;

        var headers = new List<string>();
        for (int col = 1; col <= colCount; col++)
        {
            headers.Add(worksheet.Cells[1, col].Value?.ToString() ?? string.Empty);
        }

        var properties = typeof(T).GetProperties().ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

        for (int row = 2; row <= rowCount; row++)
        {
            var item = new T();
            for (int col = 1; col <= colCount; col++)
            {
                var header = headers[col - 1];
                if (properties.TryGetValue(header, out var prop))
                {
                    var value = worksheet.Cells[row, col].Value;
                    if (value != null)
                    {
                        try
                        {
                            var convertedValue = Convert.ChangeType(value, prop.PropertyType, CultureInfo.InvariantCulture);
                            prop.SetValue(item, convertedValue);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            results.Add(item);
        }

        return results;
    }
}

public class CompoundCsvMap : ClassMap<Compound>
{
    public CompoundCsvMap()
    {
        Map(m => m.Id).Name("Id");
        Map(m => m.Name).Name("Name");
        Map(m => m.Description).Name("Description");
        Map(m => m.Smiles).Name("Smiles");
        Map(m => m.Formula).Name("Formula");
        Map(m => m.MolecularWeight).Name("MolecularWeight");
        Map(m => m.LogP).Name("LogP");
        Map(m => m.HBD).Name("HBD");
        Map(m => m.HBA).Name("HBA");
        Map(m => m.TPSA).Name("TPSA");
        Map(m => m.RotatableBonds).Name("RotatableBonds");
    }
}
