using BioCAD.Domain.Entities;
using BioCAD.Domain.Enums;
using BioCAD.Visualization.Controls;

namespace BioCAD.App.Pages;

public class DataManagementPage : UserControl
{
    private TabControl? _tabControl;
    private DataGridView? _compoundGrid;
    private DataGridView? _proteinGrid;
    private DataGridView? _activityGrid;
    private MoleculeViewer2D? _previewViewer;
    private Label? _detailInfoLabel;

    public DataManagementPage()
    {
        InitializeComponent();
        LoadSampleData();
    }

    private void InitializeComponent()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(245, 247, 250);
        Padding = new Padding(10);

        var titlePanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = Color.Transparent
        };

        var titleLabel = new Label
        {
            Text = "数据管理",
            Font = new Font("Microsoft YaHei UI", 18f, FontStyle.Bold),
            ForeColor = Color.FromArgb(44, 62, 80),
            AutoSize = true,
            Location = new Point(0, 5)
        };
        titlePanel.Controls.Add(titleLabel);

        var subtitleLabel = new Label
        {
            Text = "管理化合物、蛋白质、基因组与活性数据",
            Font = new Font("Microsoft YaHei UI", 9f),
            ForeColor = Color.FromArgb(127, 140, 141),
            AutoSize = true,
            Location = new Point(0, 35)
        };
        titlePanel.Controls.Add(subtitleLabel);
        Controls.Add(titlePanel);

        var toolbar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 45,
            BackColor = Color.White,
            Padding = new Padding(10)
        };

        var importBtn = CreateButton("导入数据", Color.FromArgb(52, 152, 219), 10, 8);
        importBtn.Click += (s, e) => ImportData();
        toolbar.Controls.Add(importBtn);

        var exportBtn = CreateButton("导出数据", Color.FromArgb(46, 204, 113), 120, 8);
        exportBtn.Click += (s, e) => ExportData();
        toolbar.Controls.Add(exportBtn);

        var addBtn = CreateButton("添加记录", Color.FromArgb(155, 89, 182), 230, 8);
        toolbar.Controls.Add(addBtn);

        var deleteBtn = CreateButton("删除选中", Color.FromArgb(231, 76, 60), 340, 8);
        toolbar.Controls.Add(deleteBtn);

        var searchBox = new TextBox
        {
            Text = "  搜索...",
            Width = 250,
            Height = 28,
            Location = new Point(500, 10),
            ForeColor = Color.Gray,
            Font = new Font("Microsoft YaHei UI", 9f)
        };
        toolbar.Controls.Add(searchBox);

        Controls.Add(toolbar);

        var splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 700,
            BackColor = Color.FromArgb(245, 247, 250),
            Padding = new Padding(0, 10, 0, 0)
        };

        _tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Microsoft YaHei UI", 9f)
        };

        _compoundGrid = CreateDataGrid();
        _proteinGrid = CreateDataGrid();
        _activityGrid = CreateDataGrid();

        var compoundPage = new TabPage("化合物库");
        compoundPage.Controls.Add(_compoundGrid);
        _tabControl.TabPages.Add(compoundPage);

        var proteinPage = new TabPage("蛋白质库");
        proteinPage.Controls.Add(_proteinGrid);
        _tabControl.TabPages.Add(proteinPage);

        var activityPage = new TabPage("活性数据");
        activityPage.Controls.Add(_activityGrid);
        _tabControl.TabPages.Add(activityPage);

        var genomicPage = new TabPage("基因组数据");
        var genomicGrid = CreateDataGrid();
        genomicPage.Controls.Add(genomicGrid);
        _tabControl.TabPages.Add(genomicPage);

        _tabControl.SelectedIndexChanged += (s, e) => OnTabChanged();

        splitContainer.Panel1.Controls.Add(_tabControl);

        var previewPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(10)
        };

        var previewTitle = new Label
        {
            Text = "分子预览",
            Font = new Font("Microsoft YaHei UI", 12f, FontStyle.Bold),
            ForeColor = Color.FromArgb(44, 62, 80),
            Dock = DockStyle.Top,
            Height = 30,
            TextAlign = ContentAlignment.MiddleLeft
        };
        previewPanel.Controls.Add(previewTitle);

        _previewViewer = new MoleculeViewer2D
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };
        previewPanel.Controls.Add(_previewViewer);

        var infoPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 180,
            BackColor = Color.FromArgb(248, 249, 250),
            Padding = new Padding(10)
        };

        var infoTitle = new Label
        {
            Text = "详细信息",
            Font = new Font("Microsoft YaHei UI", 10f, FontStyle.Bold),
            ForeColor = Color.FromArgb(44, 62, 80),
            Dock = DockStyle.Top,
            Height = 28
        };
        infoPanel.Controls.Add(infoTitle);

        _detailInfoLabel = new Label
        {
            Text = "请选择一条记录查看详细信息",
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(52, 73, 94),
            TextAlign = ContentAlignment.TopLeft,
            Font = new Font("Microsoft YaHei UI", 9f),
            Padding = new Padding(0, 8, 0, 0)
        };
        infoPanel.Controls.Add(_detailInfoLabel);

        previewPanel.Controls.Add(infoPanel);

        splitContainer.Panel2.Controls.Add(previewPanel);
        Controls.Add(splitContainer);
    }

    private static Button CreateButton(string text, Color color, int x, int y)
    {
        return new Button
        {
            Text = text,
            Left = x,
            Top = y,
            Width = 100,
            Height = 30,
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Microsoft YaHei UI", 9f),
            Cursor = Cursors.Hand
        };
    }

    private static DataGridView CreateDataGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false,
            BackgroundColor = Color.White,
            GridColor = Color.FromArgb(236, 240, 241),
            DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(52, 73, 94),
                SelectionBackColor = Color.FromArgb(52, 152, 219),
                SelectionForeColor = Color.White,
                Font = new Font("Microsoft YaHei UI", 9f),
                Padding = new Padding(5)
            },
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(248, 249, 250),
                ForeColor = Color.FromArgb(44, 62, 80),
                Font = new Font("Microsoft YaHei UI", 9f, FontStyle.Bold),
                Padding = new Padding(5)
            },
            EnableHeadersVisualStyles = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        };

        return grid;
    }

    private void LoadSampleData()
    {
        if (_compoundGrid != null)
        {
            _compoundGrid.Columns.Add("Id", "ID");
            _compoundGrid.Columns.Add("Name", "名称");
            _compoundGrid.Columns.Add("Formula", "分子式");
            _compoundGrid.Columns.Add("MolecularWeight", "分子量");
            _compoundGrid.Columns.Add("LogP", "LogP");
            _compoundGrid.Columns.Add("TPSA", "TPSA");

            var random = new Random();
            var names = new[] { "Aspirin", "Ibuprofen", "Paracetamol", "Diclofenac", "Celecoxib",
                "Metformin", "Atorvastatin", "Omeprazole", "Amoxicillin", "Lisinopril" };
            var formulas = new[] { "C9H8O4", "C13H18O2", "C8H9NO2", "C14H11Cl2NO2", "C17H14F3N3O2S",
                "C4H11N5", "C33H35FN2O5", "C17H19N3O3S", "C16H19N3O5S", "C21H31N3O5" };

            for (int i = 0; i < 20; i++)
            {
                _compoundGrid.Rows.Add(
                    i + 1,
                    names[i % names.Length] + (i / names.Length > 0 ? $"_{i / names.Length}" : ""),
                    formulas[i % formulas.Length],
                    Math.Round(150 + random.NextDouble() * 350, 2),
                    Math.Round(-1 + random.NextDouble() * 6, 2),
                    Math.Round(20 + random.NextDouble() * 120, 1)
                );
            }

            _compoundGrid.SelectionChanged += (s, e) => UpdateCompoundPreview();
        }

        if (_proteinGrid != null)
        {
            _proteinGrid.Columns.Add("Id", "ID");
            _proteinGrid.Columns.Add("Name", "名称");
            _proteinGrid.Columns.Add("Gene", "基因名");
            _proteinGrid.Columns.Add("Organism", "物种");
            _proteinGrid.Columns.Add("Length", "长度");
            _proteinGrid.Columns.Add("MW", "分子量(kDa)");

            var proteins = new[]
            {
                ("EGFR", "Epidermal growth factor receptor", "Homo sapiens", 1210),
                ("ALK", "Anaplastic lymphoma kinase", "Homo sapiens", 1620),
                ("BRAF", "B-Raf proto-oncogene", "Homo sapiens", 766),
                ("KRAS", "Kirsten rat sarcoma viral oncogene homolog", "Homo sapiens", 189),
                ("VEGFA", "Vascular endothelial growth factor A", "Homo sapiens", 232),
                ("JAK2", "Janus kinase 2", "Homo sapiens", 1132),
                ("PIK3CA", "Phosphatidylinositol-4,5-bisphosphate 3-kinase", "Homo sapiens", 1068),
                ("MYC", "MYC proto-oncogene", "Homo sapiens", 439)
            };

            for (int i = 0; i < proteins.Length; i++)
            {
                _proteinGrid.Rows.Add(
                    i + 1,
                    proteins[i].Item1,
                    proteins[i].Item2,
                    proteins[i].Item3,
                    proteins[i].Item4,
                    Math.Round(proteins[i].Item4 * 0.11, 1)
                );
            }

            _proteinGrid.SelectionChanged += (s, e) => UpdateProteinDetail();
        }

        if (_activityGrid != null)
        {
            _activityGrid.Columns.Add("Id", "ID");
            _activityGrid.Columns.Add("Compound", "化合物");
            _activityGrid.Columns.Add("Target", "靶标");
            _activityGrid.Columns.Add("Type", "类型");
            _activityGrid.Columns.Add("Value", "IC50(nM)");
            _activityGrid.Columns.Add("Source", "来源");

            var random = new Random();
            for (int i = 0; i < 30; i++)
            {
                _activityGrid.Rows.Add(
                    i + 1,
                    $"Compound_{i + 1}",
                    $"Target_{(i % 8) + 1}",
                    i % 3 == 0 ? "Binding" : i % 3 == 1 ? "Enzyme" : "Cell",
                    Math.Round(1 + random.NextDouble() * 1000, 2),
                    random.Next(2) == 0 ? "实验" : "文献"
                );
            }

            _activityGrid.SelectionChanged += (s, e) => UpdateActivityDetail();
        }
    }

    private void UpdateCompoundPreview()
    {
        if (_compoundGrid == null || _previewViewer == null || _detailInfoLabel == null) return;
        if (_compoundGrid.SelectedRows.Count == 0) return;

        var row = _compoundGrid.SelectedRows[0];
        string name = row.Cells["Name"].Value?.ToString() ?? "";
        string formula = row.Cells["Formula"].Value?.ToString() ?? "";
        string mw = row.Cells["MolecularWeight"].Value?.ToString() ?? "";
        string logp = row.Cells["LogP"].Value?.ToString() ?? "";
        string tpsa = row.Cells["TPSA"].Value?.ToString() ?? "";

        var compound = new Compound
        {
            Name = name,
            Formula = formula,
            MolecularWeight = double.TryParse(mw, out double mwVal) ? mwVal : 0,
            LogP = double.TryParse(logp, out double logpVal) ? logpVal : 0,
            TPSA = double.TryParse(tpsa, out double tpsaVal) ? tpsaVal : 0
        };

        var random = new Random(name.GetHashCode());
        int atomCount = 15 + random.Next(15);

        for (int i = 0; i < atomCount; i++)
        {
            double angle = (double)i / atomCount * Math.PI * 2;
            double radius = 50 + random.NextDouble() * 30;
            var elements = new[] { "C", "C", "C", "C", "O", "N", "H" };
            compound.Atoms.Add(new Atom
            {
                Id = i + 1,
                Element = elements[random.Next(elements.Length)],
                X = Math.Cos(angle) * radius + random.NextDouble() * 20 - 10,
                Y = Math.Sin(angle) * radius + random.NextDouble() * 20 - 10,
                Z = random.NextDouble() * 10 - 5
            });
        }

        for (int i = 0; i < atomCount - 1; i++)
        {
            compound.Bonds.Add(new Bond
            {
                Id = i + 1,
                Atom1Id = i + 1,
                Atom2Id = i + 2,
                Order = random.Next(1, 4)
            });
        }

        _previewViewer.Compound = compound;

        _detailInfoLabel.Text = 
            $"名称: {name}\n" +
            $"分子式: {formula}\n" +
            $"分子量: {mw}\n" +
            $"LogP: {logp}\n" +
            $"TPSA: {tpsa}\n" +
            $"原子数: {atomCount}\n" +
            $"键数: {atomCount - 1}\n" +
            $"来源: 示例数据库";
    }

    private void UpdateProteinDetail()
    {
        if (_proteinGrid == null || _detailInfoLabel == null || _previewViewer == null) return;
        if (_proteinGrid.SelectedRows.Count == 0) return;

        var row = _proteinGrid.SelectedRows[0];
        string name = row.Cells["Name"].Value?.ToString() ?? "";
        string gene = row.Cells["Gene"].Value?.ToString() ?? "";
        string organism = row.Cells["Organism"].Value?.ToString() ?? "";
        string length = row.Cells["Length"].Value?.ToString() ?? "";
        string mw = row.Cells["MW"].Value?.ToString() ?? "";

        _detailInfoLabel.Text = 
            $"名称: {name}\n" +
            $"基因名: {gene}\n" +
            $"物种: {organism}\n" +
            $"序列长度: {length} aa\n" +
            $"分子量: {mw} kDa\n" +
            $"来源: UniProtKB\n" +
            $"状态: 已验证";

        var compound = new Compound { Name = name };
        _previewViewer.Compound = compound;
    }

    private void UpdateActivityDetail()
    {
        if (_activityGrid == null || _detailInfoLabel == null || _previewViewer == null) return;
        if (_activityGrid.SelectedRows.Count == 0) return;

        var row = _activityGrid.SelectedRows[0];
        string compoundName = row.Cells["Compound"].Value?.ToString() ?? "";
        string target = row.Cells["Target"].Value?.ToString() ?? "";
        string type = row.Cells["Type"].Value?.ToString() ?? "";
        string value = row.Cells["Value"].Value?.ToString() ?? "";
        string source = row.Cells["Source"].Value?.ToString() ?? "";

        _detailInfoLabel.Text = 
            $"化合物: {compoundName}\n" +
            $"靶标: {target}\n" +
            $"实验类型: {type}\n" +
            $"IC50: {value} nM\n" +
            $"数据来源: {source}\n" +
            $"测定方法: 荧光偏振\n" +
            $"质量等级: A";

        var compound = new Compound { Name = compoundName };
        _previewViewer.Compound = compound;
    }

    private void OnTabChanged()
    {
        if (_tabControl == null || _detailInfoLabel == null) return;

        switch (_tabControl.SelectedIndex)
        {
            case 0:
                UpdateCompoundPreview();
                break;
            case 1:
                UpdateProteinDetail();
                break;
            case 2:
                UpdateActivityDetail();
                break;
            default:
                _detailInfoLabel.Text = "请选择一条记录查看详细信息";
                break;
        }
    }

    private static void ImportData()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "支持的格式|*.sdf;*.mol2;*.fasta;*.fa;*.csv;*.xlsx;*.xls|所有文件|*.*",
            Title = "导入生物数据"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            MessageBox.Show($"已导入文件: {dialog.FileName}", "导入成功",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private static void ExportData()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Excel 文件|*.xlsx|CSV 文件|*.csv|SDF 文件|*.sdf|FASTA 文件|*.fasta",
            Title = "导出数据"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            MessageBox.Show($"已导出到: {dialog.FileName}", "导出成功",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
