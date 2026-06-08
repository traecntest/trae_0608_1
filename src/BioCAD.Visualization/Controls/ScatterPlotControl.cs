namespace BioCAD.Visualization.Controls;

public class ScatterPlotControl : UserControl
{
    private List<ScatterSeries> _series = new();
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

    public ScatterPlotControl()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    public void AddSeries(string name, List<(double x, double y)> points, Color? color = null)
    {
        _series.Add(new ScatterSeries
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

        if (!_series.Any(s => s.Points.Any()))
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
            using var titleFont = new Font(Font.FontFamily, Font.Size * 1.4f, FontStyle.Bold);
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
        int marginBottom = 50;

        if (_showLegend)
            marginRight = 130;

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
            var label = value.ToString("F1");
            var labelSize = g.MeasureString(label, Font);
            g.DrawString(label, Font, labelBrush, x - labelSize.Width / 2, chartArea.Bottom + 5);
        }

        int ySteps = 5;
        for (int i = 0; i <= ySteps; i++)
        {
            float y = chartArea.Top + (float)i / ySteps * chartArea.Height;
            double value = yMax - (yMax - yMin) * (double)i / ySteps;
            var label = value.ToString("F1");
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

    private void DrawSeries(Graphics g, ScatterSeries series, Rectangle chartArea)
    {
        if (!series.Points.Any()) return;

        var (xMin, xMax, yMin, yMax) = GetDataRange();
        if (xMax <= xMin || yMax <= yMin) return;

        using var brush = new SolidBrush(series.Color);
        using var pen = new Pen(Color.FromArgb(100, series.Color));
        float pointSize = 6;

        foreach (var p in series.Points)
        {
            float x = chartArea.Left + (float)((p.x - xMin) / (xMax - xMin) * chartArea.Width);
            float y = chartArea.Top + (float)((1 - (p.y - yMin) / (yMax - yMin)) * chartArea.Height);

            if (x >= chartArea.Left && x <= chartArea.Right && y >= chartArea.Top && y <= chartArea.Bottom)
            {
                g.FillEllipse(brush, x - pointSize / 2, y - pointSize / 2, pointSize, pointSize);
            }
        }
    }

    private void DrawLegend(Graphics g)
    {
        if (!_series.Any()) return;

        int itemHeight = 22;
        int padding = 8;
        int width = 110;
        int height = _series.Count * itemHeight + padding * 2;

        int x = ClientSize.Width - width - 10;
        int y = 50;

        using var bgBrush = new SolidBrush(Color.FromArgb(245, 245, 245));
        using var borderPen = new Pen(Color.LightGray);
        g.FillRectangle(bgBrush, x, y, width, height);
        g.DrawRectangle(borderPen, x, y, width, height);

        for (int i = 0; i < _series.Count; i++)
        {
            var series = _series[i];
            int itemY = y + padding + i * itemHeight;

            using var colorBrush = new SolidBrush(series.Color);
            g.FillEllipse(colorBrush, x + padding, itemY + 5, 10, 10);

            using var textBrush = new SolidBrush(Color.Black);
            g.DrawString(series.Name, Font, textBrush, x + padding + 18, itemY + 2);
        }
    }

    private (double xMin, double xMax, double yMin, double yMax) GetDataRange()
    {
        var allPoints = _series.Where(s => s.Points.Any()).SelectMany(s => s.Points).ToList();
        if (!allPoints.Any())
            return (-1, 1, -1, 1);

        double xMin = allPoints.Min(p => p.x);
        double xMax = allPoints.Max(p => p.x);
        double yMin = allPoints.Min(p => p.y);
        double yMax = allPoints.Max(p => p.y);

        double xPadding = Math.Abs(xMax - xMin) * 0.1;
        double yPadding = Math.Abs(yMax - yMin) * 0.1;
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

    private class ScatterSeries
    {
        public string Name { get; set; } = string.Empty;
        public List<(double x, double y)> Points { get; set; } = new();
        public Color Color { get; set; }
    }
}
