using BioCAD.Visualization.Controls;
using BioCAD.Engine.Modules;
using Newtonsoft.Json;

namespace BioCAD.App.Pages;

public class ResultAnalysisPage : UserControl
{
    private TabControl? _chartTabs;
    private ComboBox? _resultSelector;

    public ResultAnalysisPage()
    {
        InitializeComponent();
        LoadSampleResults();
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
            Text = "结果分析",
            Font = new Font("Microsoft YaHei UI", 18f, FontStyle.Bold),
            ForeColor = Color.FromArgb(44, 62, 80),
            AutoSize = true,
            Location = new Point(0, 5)
        };
        titlePanel.Controls.Add(titleLabel);

        var subtitleLabel = new Label
        {
            Text = "可视化分析计算结果与数据",
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

        var resultLabel = new Label
        {
            Text = "选择结果:",
            AutoSize = true,
            Location = new Point(10, 15),
            ForeColor = Color.FromArgb(52, 73, 94)
        };
        toolbar.Controls.Add(resultLabel);

        _resultSelector = new ComboBox
        {
            Left = 90,
            Top = 12,
            Width = 250,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _resultSelector.Items.AddRange(new object[] {
            "分子对接结果 - EGFR",
            "虚拟筛选结果 - ZINC库",
            "MD模拟轨迹分析",
            "聚类分析结果",
            "ROC曲线 - 预测模型"
        });
        _resultSelector.SelectedIndex = 0;
        _resultSelector.SelectedIndexChanged += (s, e) => UpdateCharts();
        toolbar.Controls.Add(_resultSelector);

        var exportBtn = CreateButton("导出图表", Color.FromArgb(46, 204, 113), 360, 10);
        toolbar.Controls.Add(exportBtn);

        var reportBtn = CreateButton("生成报告", Color.FromArgb(155, 89, 182), 470, 10);
        toolbar.Controls.Add(reportBtn);

        Controls.Add(toolbar);

        _chartTabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Microsoft YaHei UI", 9f),
            Padding = new Point(10, 5)
        };
        _chartTabs.Padding = new Point(15, 5);

        var dockingTab = new TabPage("对接结果");
        var screeningTab = new TabPage("虚拟筛选");
        var mdTab = new TabPage("MD模拟");
        var clusteringTab = new TabPage("聚类分析");
        var rocTab = new TabPage("ROC曲线");

        var dockingChart = new LineChartControl
        {
            Dock = DockStyle.Fill,
            Title = "分子对接打分分布",
            XLabel = "化合物",
            YLabel = "结合亲和力 (kcal/mol)",
            ShowLegend = true
        };
        dockingTab.Controls.Add(dockingChart);

        var heatmap = new HeatmapControl
        {
            Dock = DockStyle.Fill,
            Title = "结合模式热力图",
            ShowValues = true
        };
        heatmap.GenerateSampleData(8, 10);
        var heatmapTab = new TabPage("结合模式");
        heatmapTab.Controls.Add(heatmap);

        var screeningChart = new LineChartControl
        {
            Dock = DockStyle.Fill,
            Title = "虚拟筛选结果排名",
            XLabel = "化合物排名",
            YLabel = "打分",
            ShowLegend = false
        };
        screeningTab.Controls.Add(screeningChart);

        var mdChart = new LineChartControl
        {
            Dock = DockStyle.Fill,
            Title = "分子动力学 - RMSD分析",
            XLabel = "时间 (ns)",
            YLabel = "RMSD (Å)",
            ShowLegend = true
        };
        mdTab.Controls.Add(mdChart);

        var clusterChart = new ScatterPlotControl
        {
            Dock = DockStyle.Fill,
            Title = "化合物聚类分析",
            XLabel = "PC1",
            YLabel = "PC2",
            ShowLegend = true
        };
        clusteringTab.Controls.Add(clusterChart);

        var rocChart = new ROCChartControl
        {
            Dock = DockStyle.Fill
        };
        rocTab.Controls.Add(rocChart);

        _chartTabs.TabPages.Add(dockingTab);
        _chartTabs.TabPages.Add(heatmapTab);
        _chartTabs.TabPages.Add(screeningTab);
        _chartTabs.TabPages.Add(mdTab);
        _chartTabs.TabPages.Add(clusteringTab);
        _chartTabs.TabPages.Add(rocTab);

        Controls.Add(_chartTabs);

        LoadDockingChartData(dockingChart);
        LoadScreeningChartData(screeningChart);
        LoadMDChartData(mdChart);
        LoadClusteringData(clusterChart);
        LoadROCData(rocChart);
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

    private static void LoadDockingChartData(LineChartControl chart)
    {
        var random = new Random(42);
        var points = new List<(double x, double y)>();
        for (int i = 0; i < 20; i++)
        {
            points.Add((i + 1, -5 - random.NextDouble() * 6));
        }
        chart.AddSeries("结合亲和力", points, Color.FromArgb(52, 152, 219));
    }

    private static void LoadScreeningChartData(LineChartControl chart)
    {
        var random = new Random(123);
        var points = new List<(double x, double y)>();
        for (int i = 0; i < 100; i++)
        {
            double score = -4 - random.NextDouble() * 8 - (i / 100.0) * 2;
            points.Add((i + 1, score));
        }
        chart.AddSeries("打分", points.OrderBy(p => p.y).Select(p => (p.x, p.y)).ToList(),
            Color.FromArgb(231, 76, 60));
    }

    private static void LoadMDChartData(LineChartControl chart)
    {
        var random = new Random(456);
        var proteinPoints = new List<(double x, double y)>();
        var ligandPoints = new List<(double x, double y)>();
        var backbonePoints = new List<(double x, double y)>();

        double proteinRmsd = 1.0;
        double ligandRmsd = 0.5;
        double backboneRmsd = 0.8;

        for (int i = 0; i <= 100; i++)
        {
            double time = i * 0.1;
            proteinRmsd += random.NextDouble() * 0.1 - 0.05;
            ligandRmsd += random.NextDouble() * 0.15 - 0.07;
            backboneRmsd += random.NextDouble() * 0.08 - 0.04;

            proteinRmsd = Math.Max(0.5, Math.Min(5.0, proteinRmsd));
            ligandRmsd = Math.Max(0.2, Math.Min(3.0, ligandRmsd));
            backboneRmsd = Math.Max(0.3, Math.Min(3.5, backboneRmsd));

            proteinPoints.Add((time, Math.Round(proteinRmsd, 2)));
            ligandPoints.Add((time, Math.Round(ligandRmsd, 2)));
            backbonePoints.Add((time, Math.Round(backboneRmsd, 2)));
        }

        chart.AddSeries("蛋白质", proteinPoints, Color.FromArgb(52, 152, 219));
        chart.AddSeries("配体", ligandPoints, Color.FromArgb(231, 76, 60));
        chart.AddSeries("主链", backbonePoints, Color.FromArgb(46, 204, 113));
    }

    private static void LoadClusteringData(ScatterPlotControl chart)
    {
        var random = new Random(789);
        var colors = new[]
        {
            Color.FromArgb(52, 152, 219),
            Color.FromArgb(231, 76, 60),
            Color.FromArgb(46, 204, 113),
            Color.FromArgb(155, 89, 182),
            Color.FromArgb(241, 196, 15)
        };

        var centers = new[]
        {
            (-3.0, -2.0),
            (2.5, -1.5),
            (0.0, 3.0),
            (-2.0, 2.5),
            (3.0, 2.0)
        };

        for (int c = 0; c < 5; c++)
        {
            var points = new List<(double x, double y)>();
            for (int i = 0; i < 30; i++)
            {
                double x = centers[c].Item1 + random.NextDouble() * 2 - 1;
                double y = centers[c].Item2 + random.NextDouble() * 2 - 1;
                points.Add((x, y));
            }
            chart.AddSeries($"Cluster {c + 1}", points, colors[c]);
        }
    }

    private static void LoadROCData(ROCChartControl chart)
    {
        var points = new List<(double fpr, double tpr)>();
        var random = new Random(999);

        double tpr = 0;
        for (int i = 0; i <= 50; i++)
        {
            double fpr = i / 50.0;
            tpr = Math.Min(1.0, tpr + random.NextDouble() * 0.08 + 0.02);
            tpr = Math.Max(tpr, fpr);
            points.Add((fpr, Math.Min(1.0, tpr)));
        }

        chart.SetData(points, 0.875);
    }

    private void LoadSampleResults()
    {
    }

    private void UpdateCharts()
    {
    }
}
