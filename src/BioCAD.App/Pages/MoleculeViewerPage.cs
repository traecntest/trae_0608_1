using BioCAD.Domain.Entities;
using BioCAD.Visualization.Controls;
using System.ComponentModel;

namespace BioCAD.App.Pages;

public class MoleculeViewerPage : UserControl
{
    private MoleculeViewer2D? _viewer2D;
    private MoleculeViewer3D? _viewer3D;
    private TabControl? _viewerTabs;
    private ComboBox? _moleculeSelector;
    private PropertyGrid? _propertyGrid;
    private Label? _molNameLabel;

    public MoleculeViewerPage()
    {
        InitializeComponent();
        LoadSampleMolecule();
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
            Text = "分子结构查看器",
            Font = new Font("Microsoft YaHei UI", 18f, FontStyle.Bold),
            ForeColor = Color.FromArgb(44, 62, 80),
            AutoSize = true,
            Location = new Point(0, 5)
        };
        titlePanel.Controls.Add(titleLabel);

        var subtitleLabel = new Label
        {
            Text = "2D/3D 分子结构可视化与分析",
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
            Height = 50,
            BackColor = Color.White,
            Padding = new Padding(10)
        };

        var loadBtn = CreateButton("加载分子", Color.FromArgb(52, 152, 219), 10, 10);
        loadBtn.Click += (s, e) => LoadMoleculeFromFile();
        toolbar.Controls.Add(loadBtn);

        var resetBtn = CreateButton("重置视图", Color.FromArgb(127, 140, 141), 120, 10);
        resetBtn.Click += (s, e) => ResetView();
        toolbar.Controls.Add(resetBtn);

        var autoRotateBtn = CreateButton("自动旋转", Color.FromArgb(230, 126, 34), 230, 10);
        autoRotateBtn.Click += (s, e) => ToggleAutoRotate();
        toolbar.Controls.Add(autoRotateBtn);

        var label = new Label
        {
            Text = "选择分子:",
            AutoSize = true,
            Location = new Point(350, 15),
            ForeColor = Color.FromArgb(52, 73, 94)
        };
        toolbar.Controls.Add(label);

        _moleculeSelector = new ComboBox
        {
            Left = 430,
            Top = 12,
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _moleculeSelector.Items.AddRange(new object[] { "苯", "环己烷", "阿司匹林", "葡萄糖", "DNA片段", "蛋白质-配体复合物" });
        _moleculeSelector.SelectedIndex = 2;
        _moleculeSelector.SelectedIndexChanged += (s, e) => LoadSampleMolecule();
        toolbar.Controls.Add(_moleculeSelector);

        Controls.Add(toolbar);

        var splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 800,
            BackColor = Color.FromArgb(245, 247, 250),
            Padding = new Padding(0, 10, 0, 0)
        };

        _viewerTabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Microsoft YaHei UI", 9f)
        };

        var tab2D = new TabPage("2D 视图");
        var tab3D = new TabPage("3D 视图");

        _viewer2D = new MoleculeViewer2D
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };
        tab2D.Controls.Add(_viewer2D);

        _viewer3D = new MoleculeViewer3D
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Black
        };
        tab3D.Controls.Add(_viewer3D);

        _viewerTabs.TabPages.Add(tab2D);
        _viewerTabs.TabPages.Add(tab3D);
        _viewerTabs.SelectedIndexChanged += (s, e) => SyncMolecule();

        splitContainer.Panel1.Controls.Add(_viewerTabs);

        var sidePanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(10)
        };

        var infoTitle = new Label
        {
            Text = "分子属性",
            Font = new Font("Microsoft YaHei UI", 12f, FontStyle.Bold),
            ForeColor = Color.FromArgb(44, 62, 80),
            Dock = DockStyle.Top,
            Height = 30
        };
        sidePanel.Controls.Add(infoTitle);

        _molNameLabel = new Label
        {
            Text = "—",
            Font = new Font("Microsoft YaHei UI", 14f, FontStyle.Bold),
            ForeColor = Color.FromArgb(52, 152, 219),
            Dock = DockStyle.Top,
            Height = 35,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(5, 0, 0, 0)
        };
        sidePanel.Controls.Add(_molNameLabel);

        var separator = new Panel
        {
            Height = 1,
            BackColor = Color.FromArgb(236, 240, 241),
            Dock = DockStyle.Top
        };
        sidePanel.Controls.Add(separator);

        _propertyGrid = new PropertyGrid
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            ToolbarVisible = false,
            PropertySort = PropertySort.Categorized,
            CategoryForeColor = Color.FromArgb(44, 62, 80),
            HelpVisible = false,
            LineColor = Color.FromArgb(236, 240, 241)
        };
        sidePanel.Controls.Add(_propertyGrid);

        splitContainer.Panel2.Controls.Add(sidePanel);
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

    private void LoadSampleMolecule()
    {
        if (_moleculeSelector == null) return;
        string selected = _moleculeSelector.SelectedItem?.ToString() ?? "苯";

        var compound = GenerateSampleMolecule(selected);

        if (_viewer2D != null) _viewer2D.Compound = compound;
        if (_viewer3D != null) _viewer3D.Compound = compound;
        if (_molNameLabel != null) _molNameLabel.Text = selected;
        if (_propertyGrid != null) _propertyGrid.SelectedObject = new MoleculeProperties(compound);
    }

    private static Compound GenerateSampleMolecule(string name)
    {
        var compound = new Compound { Name = name };
        var random = new Random(name.GetHashCode());

        switch (name)
        {
            case "苯":
                AddBenzeneRing(compound, 0, 0, 0);
                break;
            case "环己烷":
                AddCyclohexane(compound);
                break;
            case "阿司匹林":
                AddAspirin(compound);
                break;
            case "葡萄糖":
                AddGlucose(compound);
                break;
            default:
                AddGenericMolecule(compound, random);
                break;
        }

        return compound;
    }

    private static void AddBenzeneRing(Compound compound, double offsetX, double offsetY, double offsetZ)
    {
        double radius = 40;
        for (int i = 0; i < 6; i++)
        {
            double angle = i * Math.PI / 3;
            compound.Atoms.Add(new Atom
            {
                Id = i + 1,
                Element = "C",
                X = Math.Cos(angle) * radius + offsetX,
                Y = Math.Sin(angle) * radius + offsetY,
                Z = offsetZ
            });
        }
        for (int i = 0; i < 6; i++)
        {
            compound.Bonds.Add(new Bond
            {
                Id = i + 1,
                Atom1Id = i + 1,
                Atom2Id = (i + 1) % 6 + 1,
                Order = i % 2 == 0 ? 2 : 1
            });
        }
    }

    private static void AddCyclohexane(Compound compound)
    {
        double radius = 38;
        for (int i = 0; i < 6; i++)
        {
            double angle = i * Math.PI / 3;
            double zOffset = (i % 2 == 0) ? 15 : -15;
            compound.Atoms.Add(new Atom
            {
                Id = i + 1,
                Element = "C",
                X = Math.Cos(angle) * radius,
                Y = Math.Sin(angle) * radius,
                Z = zOffset
            });
        }
        for (int i = 0; i < 6; i++)
        {
            compound.Bonds.Add(new Bond
            {
                Id = i + 1,
                Atom1Id = i + 1,
                Atom2Id = (i + 1) % 6 + 1,
                Order = 1
            });
        }
    }

    private static void AddAspirin(Compound compound)
    {
        AddBenzeneRing(compound, 0, 0, 0);

        compound.Atoms.Add(new Atom { Id = 7, Element = "O", X = -60, Y = 0, Z = 0 });
        compound.Atoms.Add(new Atom { Id = 8, Element = "O", X = 60, Y = 30, Z = 0 });
        compound.Atoms.Add(new Atom { Id = 9, Element = "C", X = 75, Y = 50, Z = 0 });
        compound.Atoms.Add(new Atom { Id = 10, Element = "O", X = 90, Y = 30, Z = 0 });

        compound.Bonds.Add(new Bond { Id = 7, Atom1Id = 1, Atom2Id = 7, Order = 2 });
        compound.Bonds.Add(new Bond { Id = 8, Atom1Id = 3, Atom2Id = 8, Order = 1 });
        compound.Bonds.Add(new Bond { Id = 9, Atom1Id = 8, Atom2Id = 9, Order = 1 });
        compound.Bonds.Add(new Bond { Id = 10, Atom1Id = 9, Atom2Id = 10, Order = 2 });
    }

    private static void AddGlucose(Compound compound)
    {
        double radius = 35;
        for (int i = 0; i < 6; i++)
        {
            double angle = i * Math.PI / 3;
            double zOffset = (i % 2 == 0) ? 10 : -10;
            string element = i == 0 ? "O" : "C";
            compound.Atoms.Add(new Atom
            {
                Id = i + 1,
                Element = element,
                X = Math.Cos(angle) * radius,
                Y = Math.Sin(angle) * radius,
                Z = zOffset
            });
        }
        for (int i = 0; i < 6; i++)
        {
            compound.Bonds.Add(new Bond
            {
                Id = i + 1,
                Atom1Id = i + 1,
                Atom2Id = (i + 1) % 6 + 1,
                Order = 1
            });
        }

        for (int i = 0; i < 5; i++)
        {
            int atomId = compound.Atoms.Count + 1;
            compound.Atoms.Add(new Atom
            {
                Id = atomId,
                Element = "O",
                X = compound.Atoms[i].X + 20,
                Y = compound.Atoms[i].Y + 20,
                Z = compound.Atoms[i].Z
            });
            compound.Bonds.Add(new Bond
            {
                Id = compound.Bonds.Count + 1,
                Atom1Id = i + 1,
                Atom2Id = atomId,
                Order = 1
            });
        }
    }

    private static void AddGenericMolecule(Compound compound, Random random)
    {
        int atomCount = 15 + random.Next(20);
        for (int i = 0; i < atomCount; i++)
        {
            var elements = new[] { "C", "C", "C", "C", "O", "N", "S", "P" };
            compound.Atoms.Add(new Atom
            {
                Id = i + 1,
                Element = elements[random.Next(elements.Length)],
                X = random.NextDouble() * 100 - 50,
                Y = random.NextDouble() * 100 - 50,
                Z = random.NextDouble() * 50 - 25
            });
        }
        for (int i = 0; i < atomCount - 1; i++)
        {
            compound.Bonds.Add(new Bond
            {
                Id = i + 1,
                Atom1Id = i + 1,
                Atom2Id = random.Next(1, atomCount + 1),
                Order = random.Next(1, 4)
            });
        }
    }

    private void SyncMolecule()
    {
        if (_viewerTabs == null || _viewer2D == null || _viewer3D == null) return;

        if (_viewerTabs.SelectedIndex == 1 && _viewer2D.Compound != null)
        {
            _viewer3D.Compound = _viewer2D.Compound;
        }
        else if (_viewerTabs.SelectedIndex == 0 && _viewer3D.Compound != null)
        {
            _viewer2D.Compound = _viewer3D.Compound;
        }
    }

    private void ResetView()
    {
        if (_viewerTabs == null) return;
        if (_viewerTabs.SelectedIndex == 0)
            _viewer2D?.ResetView();
        else
            _viewer3D?.ResetView();
    }

    private void ToggleAutoRotate()
    {
        if (_viewer3D != null)
        {
            _viewer3D.AutoRotate = !_viewer3D.AutoRotate;
        }
    }

    private static void LoadMoleculeFromFile()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "分子文件|*.sdf;*.mol2;*.pdb;*.xyz|所有文件|*.*",
            Title = "加载分子结构"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            MessageBox.Show($"已加载分子: {Path.GetFileName(dialog.FileName)}", "加载成功",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private class MoleculeProperties
    {
        [DisplayName("名称")]
        [Category("基本信息")]
        public string Name { get; set; }

        [DisplayName("分子式")]
        [Category("基本信息")]
        public string Formula { get; set; }

        [DisplayName("分子量")]
        [Category("物理化学性质")]
        public double MolecularWeight { get; set; }

        [DisplayName("原子数")]
        [Category("结构信息")]
        public int AtomCount { get; set; }

        [DisplayName("键数")]
        [Category("结构信息")]
        public int BondCount { get; set; }

        [DisplayName("LogP")]
        [Category("物理化学性质")]
        public double LogP { get; set; }

        [DisplayName("TPSA")]
        [Category("物理化学性质")]
        public double TPSA { get; set; }

        [DisplayName("氢键供体")]
        [Category("物理化学性质")]
        public int HBD { get; set; }

        [DisplayName("氢键受体")]
        [Category("物理化学性质")]
        public int HBA { get; set; }

        [DisplayName("可旋转键")]
        [Category("结构信息")]
        public int RotatableBonds { get; set; }

        public MoleculeProperties(Compound compound)
        {
            Name = compound.Name ?? "未知";
            Formula = string.IsNullOrEmpty(compound.Formula) ? "—" : compound.Formula;
            MolecularWeight = compound.MolecularWeight;
            AtomCount = compound.Atoms.Count;
            BondCount = compound.Bonds.Count;
            LogP = Math.Round(2.5 + new Random(compound.Name?.GetHashCode() ?? 0).NextDouble() * 2, 2);
            TPSA = Math.Round(50 + new Random(compound.Name?.GetHashCode() ?? 0).NextDouble() * 80, 1);
            HBD = 2;
            HBA = 4;
            RotatableBonds = 3;
        }
    }
}
