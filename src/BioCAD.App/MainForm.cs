namespace BioCAD.App;

public partial class MainForm : Form
{
    private Panel? _sidebar;
    private Panel? _contentPanel;
    private Label? _titleLabel;
    private readonly List<NavButton> _navButtons = new();
    private UserControl? _currentPage;

    public MainForm()
    {
        InitializeComponent();
        SetupNavigation();
        ShowPage("Dashboard");
    }

    private void InitializeComponent()
    {
        Text = "BioCAD - 生物计算与AI辅助药物研发平台";
        Width = 1400;
        Height = 900;
        MinimumSize = new Size(1024, 700);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(245, 247, 250);
        Font = new Font("Microsoft YaHei UI", 9f);

        var titleBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = Color.FromArgb(44, 62, 80)
        };

        _titleLabel = new Label
        {
            Text = "🧬 BioCAD 生物计算平台",
            ForeColor = Color.White,
            Font = new Font("Microsoft YaHei UI", 14f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Dock = DockStyle.Left,
            Width = 350,
            Padding = new Padding(20, 0, 0, 0)
        };
        titleBar.Controls.Add(_titleLabel);

        var subtitleLabel = new Label
        {
            Text = "生物计算 · AI辅助药物研发 · 数据分析",
            ForeColor = Color.FromArgb(189, 195, 199),
            Font = new Font("Microsoft YaHei UI", 9f),
            TextAlign = ContentAlignment.MiddleLeft,
            Dock = DockStyle.Fill,
            Padding = new Padding(10, 0, 0, 0)
        };
        titleBar.Controls.Add(subtitleLabel);

        Controls.Add(titleBar);

        _sidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 220,
            BackColor = Color.FromArgb(52, 73, 94)
        };
        Controls.Add(_sidebar);

        _contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(245, 247, 250),
            Padding = new Padding(20)
        };
        Controls.Add(_contentPanel);

        var statusBar = new StatusStrip
        {
            BackColor = Color.FromArgb(236, 240, 241)
        };
        statusBar.Items.Add(new ToolStripStatusLabel("就绪"));
        statusBar.Items.Add(new ToolStripStatusLabel { Spring = true });
        statusBar.Items.Add(new ToolStripStatusLabel($"CPU: {Environment.ProcessorCount} 核"));
        statusBar.Items.Add(new ToolStripStatusLabel("|"));
        statusBar.Items.Add(new ToolStripStatusLabel(DateTime.Now.ToString("yyyy-MM-dd")));
        Controls.Add(statusBar);
    }

    private void SetupNavigation()
    {
        if (_sidebar == null) return;

        int y = 10;
        var navItems = new[]
        {
            new { Name = "Dashboard", Text = "📊 仪表盘", Color = Color.FromArgb(52, 152, 219) },
            new { Name = "DataManagement", Text = "📁 数据管理", Color = Color.FromArgb(46, 204, 113) },
            new { Name = "MoleculeViewer", Text = "🔬 分子查看器", Color = Color.FromArgb(155, 89, 182) },
            new { Name = "TaskManager", Text = "⚡ 任务管理", Color = Color.FromArgb(230, 126, 34) },
            new { Name = "ResultAnalysis", Text = "📈 结果分析", Color = Color.FromArgb(231, 76, 60) },
            new { Name = "Settings", Text = "⚙️ 系统设置", Color = Color.FromArgb(127, 140, 141) }
        };

        foreach (var item in navItems)
        {
            var button = new NavButton
            {
                Text = item.Text,
                Tag = item.Name,
                NormalColor = Color.FromArgb(52, 73, 94),
                HoverColor = Color.FromArgb(44, 62, 80),
                ActiveColor = item.Color,
                TextColor = Color.White,
                Height = 50,
                Width = _sidebar.Width - 20,
                Left = 10,
                Top = y,
                Font = new Font("Microsoft YaHei UI", 10f),
                Cursor = Cursors.Hand
            };
            button.Click += (s, e) => ShowPage(item.Name);
            _sidebar.Controls.Add(button);
            _navButtons.Add(button);
            y += 55;
        }

        var versionLabel = new Label
        {
            Text = "v1.0.0",
            ForeColor = Color.FromArgb(127, 140, 141),
            Font = new Font("Microsoft YaHei UI", 8f),
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Bottom,
            Height = 30
        };
        _sidebar.Controls.Add(versionLabel);
    }

    public void ShowPage(string pageName)
    {
        if (_contentPanel == null) return;

        foreach (var btn in _navButtons)
        {
            btn.IsActive = (btn.Tag?.ToString() == pageName);
        }

        _contentPanel.Controls.Clear();
        _currentPage?.Dispose();
        _currentPage = null;

        UserControl page = pageName switch
        {
            "Dashboard" => new Pages.DashboardPage(),
            "DataManagement" => new Pages.DataManagementPage(),
            "MoleculeViewer" => new Pages.MoleculeViewerPage(),
            "TaskManager" => new Pages.TaskManagerPage(),
            "ResultAnalysis" => new Pages.ResultAnalysisPage(),
            "Settings" => new Pages.SettingsPage(),
            _ => CreateNotFoundPage()
        };

        page.Dock = DockStyle.Fill;
        _contentPanel.Controls.Add(page);
        _currentPage = page;
    }

    private static UserControl CreateNotFoundPage()
    {
        var page = new UserControl();
        var label = new Label
        {
            Text = "页面不存在",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.Gray,
            Font = new Font("Microsoft YaHei UI", 14f)
        };
        page.Controls.Add(label);
        return page;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        AppServices.Shutdown();
        base.OnFormClosing(e);
    }
}

public class NavButton : Control
{
    public Color NormalColor { get; set; } = Color.FromArgb(52, 73, 94);
    public Color HoverColor { get; set; } = Color.FromArgb(44, 62, 80);
    public Color ActiveColor { get; set; } = Color.FromArgb(52, 152, 219);
    public Color TextColor { get; set; } = Color.White;
    public bool IsActive { get; set; } = false;
    public bool IsHovering { get; set; } = false;

    public NavButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        Color bgColor = IsActive ? ActiveColor : (IsHovering ? HoverColor : NormalColor);

        using var brush = new SolidBrush(bgColor);
        g.FillRoundedRectangle(brush, ClientRectangle, 6);

        using var textBrush = new SolidBrush(TextColor);
        var textFormat = new StringFormat
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Center
        };
        var textRect = new Rectangle(15, 0, Width - 15, Height);
        g.DrawString(Text, Font, textBrush, textRect, textFormat);

        if (IsActive)
        {
            using var indicatorBrush = new SolidBrush(Color.White);
            g.FillRectangle(indicatorBrush, 0, 10, 4, Height - 20);
        }
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        IsHovering = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        IsHovering = false;
        Invalidate();
    }
}

public static class GraphicsExtensions
{
    public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle rect, int radius)
    {
        using var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
        path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
        path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
        path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
        path.CloseFigure();
        g.FillPath(brush, path);
    }
}
