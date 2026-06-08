namespace BioCAD.Visualization.Controls;

public class LineChartControl : UserControl
{
    private List<ChartSeries> _series = new();
    private string _title = string.Empty;
    private string _xLabel = string.Empty;
    private string _yLabel = string.Empty;
    private bool _showLegend = true;
    private bool _showGrid = true;

    public string Title
    {
        get => _title;
        set { _title = value; Invalidate(); }
    }

    public string XLabel
    {
        get => _xLabel;
        set { _xLabel = value; Invalidate(); }
    }

    public string YLabel
    {
        get => _yLabel;
        set { _yLabel = value; Invalidate(); }
    }

    public bool ShowLegend
    {
        get => _showLegend;
        set { _showLegend = value; Invalidate(); }
    }

    public bool ShowGrid
    {
        get => _showGrid;
        set { _showGrid = value; Invalidate(); }
    }

    public LineChartControl()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    public void AddSeries(string name, List<(double x, double y)> points, Color? color = null)
    {
        _series.Add(new ChartSeries
        {
            Name = name,
            Points = points,
            Color = color ?? GetNextColor(_series.Count)
        });
        Invalidate();
    }

    public void ClearSeries()
    {
        _series.Clear();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.White);

        if (!_series.Any())
        {
            g.DrawString("无数据", Font, Brushes.Gray, ClientRectangle,
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            return;
        }

        var chartArea = CalculateChartArea();
        if (chartArea.Width < 10 || chartArea.Height < 10) return;

        if (_showGrid)
            DrawGrid(g, chartArea);

        DrawAxes(g, chartArea);

        foreach (var series in _series)
        {
            DrawSeries(g, series, chartArea);
        }

        if (!string.IsNullOrEmpty(_title))
        {
            using var titleFont = new Font(Font.FontFamily, Font.Size * 1.5f, FontStyle.Bold);
            var titleSize = g.MeasureString(_title, titleFont);
            g.DrawString(_title, titleFont, Brushes.Black,
                (ClientSize.Width - titleSize.Width) / 2, 5);
        }

        if (_showLegend)
            DrawLegend(g);
    }

    private Rectangle CalculateChartArea()
    {
        int marginLeft = 60;
        int marginRight = 20;
        int marginTop = 40;
        int marginBottom = 60;

        if (_showLegend)
            marginRight = 120;

        if (!string.IsNullOrEmpty(_title))
            marginTop += 20;

        return new Rectangle(
            marginLeft, marginTop,
            ClientSize.Width - marginLeft - marginRight,
            ClientSize.Height - marginTop - marginBottom);
    }

    private void DrawGrid(Graphics g, Rectangle chartArea)
    {
        var (xMin, xMax, yMin, yMax) = GetDataRange();
        if (xMax <= xMin || yMax <= yMin) return;

        using var gridPen = new Pen(Color.FromArgb(230, 230, 230));

        int xSteps = 5;
        for (int i = 0; i <= xSteps; i++)
        {
            float x = chartArea.Left + (float)i / xSteps * chartArea.Width;
            g.DrawLine(gridPen, x, chartArea.Top, x, chartArea.Bottom);
        }

        int ySteps = 5;
        for (int i = 0; i <= ySteps; i++)
        {
            float y = chartArea.Top + (float)i / ySteps * chartArea.Height;
            g.DrawLine(gridPen, chartArea.Left, y, chartArea.Right, y);
        }
    }

    private void DrawAxes(Graphics g, Rectangle chartArea)
    {
        using var axisPen = new Pen(Color.Black, 2);
        g.DrawLine(axisPen, chartArea.Left, chartArea.Top, chartArea.Left, chartArea.Bottom);
        g.DrawLine(axisPen, chartArea.Left, chartArea.Bottom, chartArea.Right, chartArea.Bottom);

        var (xMin, xMax, yMin, yMax) = GetDataRange();
        if (xMax <= xMin || yMax <= yMin) return;

        using var labelBrush = new SolidBrush(Color.Black);
        int xSteps = 5;
        for (int i = 0; i <= xSteps; i++)
        {
            float x = chartArea.Left + (float)i / xSteps * chartArea.Width;
            double value = xMin + (xMax - xMin) * (double)i / xSteps;
            var label = value.ToString("F2");
            var labelSize = g.MeasureString(label, Font);
            g.DrawString(label, Font, labelBrush, x - labelSize.Width / 2, chartArea.Bottom + 5);
        }

        int ySteps = 5;
        for (int i = 0; i <= ySteps; i++)
        {
            float y = chartArea.Top + (float)i / ySteps * chartArea.Height;
            double value = yMax - (yMax - yMin) * (double)i / ySteps;
            var label = value.ToString("F2");
            var labelSize = g.MeasureString(label, Font);
            g.DrawString(label, Font, labelBrush, chartArea.Left - labelSize.Width - 5, y - labelSize.Height / 2);
        }

        if (!string.IsNullOrEmpty(_xLabel))
        {
            var labelSize = g.MeasureString(_xLabel, Font);
            g.DrawString(_xLabel, Font, labelBrush,
                chartArea.Left + (chartArea.Width - labelSize.Width) / 2,
                ClientSize.Height - 20);
        }

        if (!string.IsNullOrEmpty(_yLabel))
        {
            var labelSize = g.MeasureString(_yLabel, Font);
            var state = g.Save();
            g.TranslateTransform(15, chartArea.Top + (chartArea.Height - labelSize.Width) / 2);
            g.RotateTransform(-90);
            g.DrawString(_yLabel, Font, labelBrush, 0, 0);
            g.Restore(state);
        }
    }

    private void DrawSeries(Graphics g, ChartSeries series, Rectangle chartArea)
    {
        if (series.Points.Count < 2) return;

        var (xMin, xMax, yMin, yMax) = GetDataRange();
        if (xMax <= xMin || yMax <= yMin) return;

        using var pen = new Pen(series.Color, 2);
        var points = new List<PointF>();

        foreach (var p in series.Points)
        {
            float x = chartArea.Left + (float)((p.x - xMin) / (xMax - xMin) * chartArea.Width);
            float y = chartArea.Top + (float)((1 - (p.y - yMin) / (yMax - yMin)) * chartArea.Height);
            x = Math.Max(chartArea.Left, Math.Min(chartArea.Right, x));
            y = Math.Max(chartArea.Top, Math.Min(chartArea.Bottom, y));
            points.Add(new PointF(x, y));
        }

        g.DrawLines(pen, points.ToArray());

        using var brush = new SolidBrush(series.Color);
        foreach (var p in points)
        {
            g.FillEllipse(brush, p.X - 3, p.Y - 3, 6, 6);
        }
    }

    private void DrawLegend(Graphics g)
    {
        if (!_series.Any()) return;

        int itemHeight = 20;
        int padding = 5;
        int width = 100;
        int height = _series.Count * itemHeight + padding * 2;

        int x = ClientSize.Width - width - 10;
        int y = 10;

        using var bgBrush = new SolidBrush(Color.FromArgb(240, 240, 240));
        using var borderPen = new Pen(Color.LightGray);
        g.FillRectangle(bgBrush, x, y, width, height);
        g.DrawRectangle(borderPen, x, y, width, height);

        for (int i = 0; i < _series.Count; i++)
        {
            var series = _series[i];
            int itemY = y + padding + i * itemHeight;

            using var colorBrush = new SolidBrush(series.Color);
            g.FillRectangle(colorBrush, x + padding, itemY + 4, 20, 4);

            using var textBrush = new SolidBrush(Color.Black);
            g.DrawString(series.Name, Font, textBrush, x + padding + 25, itemY);
        }
    }

    private (double xMin, double xMax, double yMin, double yMax) GetDataRange()
    {
        if (!_series.Any() || !_series.Any(s => s.Points.Any()))
            return (0, 1, 0, 1);

        double xMin = _series.Min(s => s.Points.Min(p => p.x));
        double xMax = _series.Max(s => s.Points.Max(p => p.x));
        double yMin = _series.Min(s => s.Points.Min(p => p.y));
        double yMax = _series.Max(s => s.Points.Max(p => p.y));

        double xPadding = (xMax - xMin) * 0.05;
        double yPadding = (yMax - yMin) * 0.05;
        xMin -= xPadding;
        xMax += xPadding;
        yMin -= yPadding;
        yMax += yPadding;

        return (xMin, xMax, yMin, yMax);
    }

    private static Color GetNextColor(int index)
    {
        var colors = new[]
        {
            Color.FromArgb(52, 152, 219),
            Color.FromArgb(231, 76, 60),
            Color.FromArgb(46, 204, 113),
            Color.FromArgb(241, 196, 15),
            Color.FromArgb(155, 89, 182),
            Color.FromArgb(26, 188, 156),
            Color.FromArgb(230, 126, 34),
            Color.FromArgb(52, 73, 94)
        };
        return colors[index % colors.Length];
    }

    private class ChartSeries
    {
        public string Name { get; set; } = string.Empty;
        public List<(double x, double y)> Points { get; set; } = new();
        public Color Color { get; set; }
    }
}

public class ROCChartControl : LineChartControl
{
    public double AUC { get; set; }

    public ROCChartControl()
    {
        Title = "ROC 曲线";
        XLabel = "假阳性率 (FPR)";
        YLabel = "真阳性率 (TPR)";

        var diagonal = new List<(double x, double y)> { (0, 0), (1, 1) };
    }

    public void SetData(List<(double fpr, double tpr)> points, double auc)
    {
        ClearSeries();
        AddSeries("ROC 曲线", points.Select(p => (p.fpr, p.tpr)).ToList(), Color.FromArgb(52, 152, 219));
        AddSeries("随机分类", new List<(double x, double y)> { (0, 0), (1, 1) }, Color.FromArgb(200, 200, 200));
        AUC = auc;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;

        using var brush = new SolidBrush(Color.FromArgb(240, 240, 240));
        g.FillRectangle(brush, ClientSize.Width - 130, 80, 120, 30);
        using var textBrush = new SolidBrush(Color.Black);
        g.DrawString($"AUC = {AUC:F3}", Font, textBrush, ClientSize.Width - 120, 85);
    }
}
