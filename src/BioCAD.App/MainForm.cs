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
        SuspendLayout();

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
            Text = "BioCAD 生物计算平台",
            ForeColor = Color.White,
            Font = new Font("Microsoft YaHei UI", 14f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Dock = DockStyle.Left,
            Width = 280,
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

        var statusBar = new StatusStrip
        {
            BackColor = Color.FromArgb(236, 240, 241),
            SizingGrip = false
        };
        statusBar.Items.Add(new ToolStripStatusLabel("就绪"));
        statusBar.Items.Add(new ToolStripStatusLabel { Spring = true });
        statusBar.Items.Add(new ToolStripStatusLabel($"CPU: {Environment.ProcessorCount} 核"));
        statusBar.Items.Add(new ToolStripStatusLabel("|"));
        statusBar.Items.Add(new ToolStripStatusLabel(DateTime.Now.ToString("yyyy-MM-dd")));
        Controls.Add(statusBar);

        _contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(245, 247, 250),
            Padding = new Padding(18)
        };
        Controls.Add(_contentPanel);

        Controls.SetChildIndex(_contentPanel, 0);
        Controls.SetChildIndex(statusBar, 1);
        Controls.SetChildIndex(_sidebar, 2);
        Controls.SetChildIndex(titleBar, 3);

        ResumeLayout(false);
        PerformLayout();
    }

    private void SetupNavigation()
    {
        if (_sidebar == null) return;

        int y = 10;
        var navItems = new[]
        {
            new { Name = "Dashboard", Text = "仪表盘", Icon = NavIconType.Chart, Color = Color.FromArgb(52, 152, 219) },
            new { Name = "DataManagement", Text = "数据管理", Icon = NavIconType.Folder, Color = Color.FromArgb(46, 204, 113) },
            new { Name = "MoleculeViewer", Text = "分子查看器", Icon = NavIconType.Molecule, Color = Color.FromArgb(155, 89, 182) },
            new { Name = "TaskManager", Text = "任务管理", Icon = NavIconType.Bolt, Color = Color.FromArgb(230, 126, 34) },
            new { Name = "ResultAnalysis", Text = "结果分析", Icon = NavIconType.Graph, Color = Color.FromArgb(231, 76, 60) },
            new { Name = "Settings", Text = "系统设置", Icon = NavIconType.Settings, Color = Color.FromArgb(127, 140, 141) }
        };

        foreach (var item in navItems)
        {
            var button = new NavButton
            {
                Text = item.Text,
                Icon = item.Icon,
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

public enum NavIconType
{
    Chart,
    Folder,
    Molecule,
    Bolt,
    Graph,
    Settings
}

public class NavButton : Control
{
    public Color NormalColor { get; set; } = Color.FromArgb(52, 73, 94);
    public Color HoverColor { get; set; } = Color.FromArgb(44, 62, 80);
    public Color ActiveColor { get; set; } = Color.FromArgb(52, 152, 219);
    public Color TextColor { get; set; } = Color.White;
    public bool IsActive { get; set; } = false;
    public bool IsHovering { get; set; } = false;
    public NavIconType Icon { get; set; } = NavIconType.Chart;

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

        int iconSize = 20;
        int iconX = 15;
        int iconY = (Height - iconSize) / 2;

        using var iconBrush = new SolidBrush(TextColor);
        DrawIcon(g, Icon, iconBrush, iconX, iconY, iconSize);

        using var textBrush = new SolidBrush(TextColor);
        var textFormat = new StringFormat
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Center
        };
        var textRect = new Rectangle(45, 0, Width - 45, Height);
        g.DrawString(Text, Font, textBrush, textRect, textFormat);

        if (IsActive)
        {
            using var indicatorBrush = new SolidBrush(Color.White);
            g.FillRectangle(indicatorBrush, 0, 10, 4, Height - 20);
        }
    }

    private static void DrawIcon(Graphics g, NavIconType icon, Brush brush, int x, int y, int size)
    {
        float cx = x + size / 2f;
        float cy = y + size / 2f;

        switch (icon)
        {
            case NavIconType.Chart:
                DrawChartIcon(g, brush, x, y, size);
                break;
            case NavIconType.Folder:
                DrawFolderIcon(g, brush, x, y, size);
                break;
            case NavIconType.Molecule:
                DrawMoleculeIcon(g, brush, x, y, size);
                break;
            case NavIconType.Bolt:
                DrawBoltIcon(g, brush, x, y, size);
                break;
            case NavIconType.Graph:
                DrawGraphIcon(g, brush, x, y, size);
                break;
            case NavIconType.Settings:
                DrawSettingsIcon(g, brush, x, y, size);
                break;
        }
    }

    private static void DrawChartIcon(Graphics g, Brush brush, int x, int y, int size)
    {
        int barW = size / 5;
        int gap = size / 8;

        g.FillRectangle(brush, x + gap, y + size / 2, barW, size / 2 - gap);
        g.FillRectangle(brush, x + gap + barW + gap / 2, y + size / 4, barW, size * 3 / 4 - gap);
        g.FillRectangle(brush, x + gap + 2 * (barW + gap / 2), y + size / 6, barW, size * 5 / 6 - gap);
    }

    private static void DrawFolderIcon(Graphics g, Brush brush, int x, int y, int size)
    {
        using var path = new System.Drawing.Drawing2D.GraphicsPath();
        int tabH = size / 4;
        int bodyY = y + tabH;
        int bodyH = size - tabH - 2;

        var tabRect = new Rectangle(x, y + size / 6, size / 2, tabH);
        path.AddRectangle(tabRect);

        var bodyRect = new Rectangle(x, bodyY, size - 2, bodyH);
        using var bodyPath = new System.Drawing.Drawing2D.GraphicsPath();
        bodyPath.AddRectangle(bodyRect);
        path.AddPath(bodyPath, false);

        g.FillPath(brush, path);
    }

    private static void DrawMoleculeIcon(Graphics g, Brush brush, int x, int y, int size)
    {
        float cx = x + size / 2f;
        float cy = y + size / 2f;
        float r = size / 6f;

        g.FillEllipse(brush, cx - r, cy - r - size / 4, r * 2, r * 2);
        g.FillEllipse(brush, cx - r - size / 4, cy - r + size / 6, r * 2, r * 2);
        g.FillEllipse(brush, cx - r + size / 4, cy - r + size / 6, r * 2, r * 2);

        using var pen = new Pen(brush, 1.5f);
        g.DrawLine(pen, cx, cy - size / 4 + r, cx - size / 4 + r, cy + size / 6 - r);
        g.DrawLine(pen, cx, cy - size / 4 + r, cx + size / 4 - r, cy + size / 6 - r);
        g.DrawLine(pen, cx - size / 4 + r, cy + size / 6 + r, cx + size / 4 - r, cy + size / 6 + r);
    }

    private static void DrawBoltIcon(Graphics g, Brush brush, int x, int y, int size)
    {
        using var path = new System.Drawing.Drawing2D.GraphicsPath();
        float w = size * 0.4f;
        float h = size * 0.8f;
        float sx = x + (size - w) / 2f;
        float sy = y + (size - h) / 2f;

        PointF[] points = {
            new PointF(sx + w * 0.6f, sy),
            new PointF(sx, sy + h * 0.45f),
            new PointF(sx + w * 0.4f, sy + h * 0.45f),
            new PointF(sx + w * 0.2f, sy + h),
            new PointF(sx + w, sy + h * 0.5f),
            new PointF(sx + w * 0.5f, sy + h * 0.5f),
            new PointF(sx + w * 0.8f, sy)
        };
        path.AddPolygon(points);
        g.FillPath(brush, path);
    }

    private static void DrawGraphIcon(Graphics g, Brush brush, int x, int y, int size)
    {
        using var pen = new Pen(brush, 2f);
        int margin = 3;
        int bottom = y + size - margin;
        int left = x + margin;
        int right = x + size - margin;
        int top = y + margin;

        g.DrawLine(pen, left, bottom, right, bottom);
        g.DrawLine(pen, left, top, left, bottom);

        PointF[] linePoints = {
            new PointF(left + 2, bottom - 4),
            new PointF(left + size / 4f, bottom - size / 2f),
            new PointF(left + size / 2f, bottom - size / 3f),
            new PointF(left + size * 3 / 4f, bottom - size / 1.5f),
            new PointF(right - 2, top + 2)
        };
        g.DrawLines(pen, linePoints);
    }

    private static void DrawSettingsIcon(Graphics g, Brush brush, int x, int y, int size)
    {
        float cx = x + size / 2f;
        float cy = y + size / 2f;
        float outerR = size / 2f - 1;
        float innerR = size / 4f;
        int teeth = 8;

        using var path = new System.Drawing.Drawing2D.GraphicsPath();
        for (int i = 0; i < teeth * 2; i++)
        {
            double angle = (i * Math.PI / teeth) - Math.PI / 2;
            float r = (i % 2 == 0) ? outerR : innerR + 2;
            float px = cx + (float)(r * Math.Cos(angle));
            float py = cy + (float)(r * Math.Sin(angle));
            if (i == 0)
                path.StartFigure();
            else
                path.AddLine(px, py, px, py);
        }
        path.CloseFigure();

        using var pen = new Pen(brush, 2f);
        g.DrawEllipse(pen, cx - outerR + 1, cy - outerR + 1, outerR * 2 - 2, outerR * 2 - 2);
        g.FillEllipse(brush, cx - innerR, cy - innerR, innerR * 2, innerR * 2);

        using var centerBrush = new SolidBrush(Color.FromArgb(52, 73, 94));
        g.FillEllipse(centerBrush, cx - innerR + 3, cy - innerR + 3, innerR * 2 - 6, innerR * 2 - 6);
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
