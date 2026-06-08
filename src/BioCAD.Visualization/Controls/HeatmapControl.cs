namespace BioCAD.Visualization.Controls;

public class HeatmapControl : UserControl
{
    private double[,]? _data;
    private string[]? _rowLabels;
    private string[]? _columnLabels;
    private string _title = string.Empty;
    private bool _showValues = true;

    public string Title
    {
        get => _title;
        set { _title = value; Invalidate(); }
    }

    public bool ShowValues
    {
        get => _showValues;
        set { _showValues = value; Invalidate(); }
    }

    public HeatmapControl()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    public void SetData(double[,] data, string[]? rowLabels = null, string[]? columnLabels = null)
    {
        _data = data;
        _rowLabels = rowLabels;
        _columnLabels = columnLabels;
        Invalidate();
    }

    public void GenerateSampleData(int rows, int cols)
    {
        var random = new Random();
        _data = new double[rows, cols];
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                _data[i, j] = random.NextDouble() * 2 - 1;

        _rowLabels = Enumerable.Range(0, rows).Select(i => $"Row {i + 1}").ToArray();
        _columnLabels = Enumerable.Range(0, cols).Select(i => $"Col {i + 1}").ToArray();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.White);

        if (_data == null)
        {
            g.DrawString("无数据", Font, Brushes.Gray, ClientRectangle,
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            return;
        }

        var heatmapArea = CalculateHeatmapArea();
        if (heatmapArea.Width <= 0 || heatmapArea.Height <= 0) return;

        DrawHeatmap(g, heatmapArea);
        DrawRowLabels(g, heatmapArea);
        DrawColumnLabels(g, heatmapArea);
        DrawColorBar(g, heatmapArea);

        if (!string.IsNullOrEmpty(_title))
        {
            using var titleFont = new Font(Font.FontFamily, Font.Size * 1.3f, FontStyle.Bold);
            var titleSize = g.MeasureString(_title, titleFont);
            g.DrawString(_title, titleFont, Brushes.Black,
                (ClientSize.Width - titleSize.Width) / 2, 5);
        }
    }

    private Rectangle CalculateHeatmapArea()
    {
        int marginLeft = _rowLabels != null ? 80 : 20;
        int marginRight = 50;
        int marginTop = !string.IsNullOrEmpty(_title) ? 40 : 20;
        int marginBottom = _columnLabels != null ? 60 : 20;

        if (_columnLabels != null)
            marginBottom = 80;

        marginRight = 80;

        return new Rectangle(
            marginLeft, marginTop,
            ClientSize.Width - marginLeft - marginRight,
            ClientSize.Height - marginTop - marginBottom);
    }

    private void DrawHeatmap(Graphics g, Rectangle area)
    {
        if (_data == null) return;

        int rows = _data.GetLength(0);
        int cols = _data.GetLength(1);

        float cellWidth = (float)area.Width / cols;
        float cellHeight = (float)area.Height / rows;

        double minVal = double.MaxValue;
        double maxVal = double.MinValue;
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
            {
                minVal = Math.Min(minVal, _data[i, j]);
                maxVal = Math.Max(maxVal, _data[i, j]);
            }

        double range = maxVal - minVal;
        if (range == 0) range = 1;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                float x = area.Left + j * cellWidth;
                float y = area.Top + i * cellHeight;
                var cellRect = new RectangleF(x, y, cellWidth, cellHeight);

                double normalized = (_data[i, j] - minVal) / range;
                var color = GetHeatmapColor(normalized);

                using var brush = new SolidBrush(color);
                g.FillRectangle(brush, cellRect);

                if (_showValues && cellWidth > 30 && cellHeight > 20)
                {
                    var textBrush = new SolidBrush(normalized > 0.5 ? Color.White : Color.Black);
                    var text = _data[i, j].ToString("F2");
                    var textSize = g.MeasureString(text, Font);
                    g.DrawString(text, Font, textBrush,
                        x + (cellWidth - textSize.Width) / 2,
                        y + (cellHeight - textSize.Height) / 2);
                    textBrush.Dispose();
                }
            }
        }

        using var pen = new Pen(Color.Black);
        g.DrawRectangle(pen, area);
    }

    private void DrawRowLabels(Graphics g, Rectangle area)
    {
        if (_rowLabels == null || _data == null) return;

        int rows = _data.GetLength(0);
        float cellHeight = (float)area.Height / rows;

        using var brush = new SolidBrush(Color.Black);
        for (int i = 0; i < rows && i < _rowLabels.Length; i++)
        {
            float y = area.Top + i * cellHeight + cellHeight / 2;
            var label = _rowLabels[i];
            var labelSize = g.MeasureString(label, Font);
            g.DrawString(label, Font, brush,
                area.Left - labelSize.Width - 5,
                y - labelSize.Height / 2);
        }
    }

    private void DrawColumnLabels(Graphics g, Rectangle area)
    {
        if (_columnLabels == null || _data == null) return;

        int cols = _data.GetLength(1);
        float cellWidth = (float)area.Width / cols;

        using var brush = new SolidBrush(Color.Black);
        var state = g.Save();
        for (int j = 0; j < cols && j < _columnLabels.Length; j++)
        {
            float x = area.Left + j * cellWidth + cellWidth / 2;
            var label = _columnLabels[j];
            var labelSize = g.MeasureString(label, Font);

            g.TranslateTransform(x, area.Bottom + 5);
            g.RotateTransform(-45);
            g.DrawString(label, Font, brush, 0, 0);
            g.ResetTransform();
        }
        g.Restore(state);
    }

    private void DrawColorBar(Graphics g, Rectangle area)
    {
        if (_data == null) return;

        int barWidth = 20;
        int barHeight = area.Height;
        int barX = area.Right + 10;
        int barY = area.Top;

        double minVal = double.MaxValue;
        double maxVal = double.MinValue;
        int rows = _data.GetLength(0);
        int cols = _data.GetLength(1);
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
            {
                minVal = Math.Min(minVal, _data[i, j]);
                maxVal = Math.Max(maxVal, _data[i, j]);
            }

        for (int i = 0; i < barHeight; i++)
        {
            double normalized = 1.0 - (double)i / barHeight;
            var color = GetHeatmapColor(normalized);
            using var pen = new Pen(color);
            g.DrawLine(pen, barX, barY + i, barX + barWidth, barY + i);
        }

        using var borderPen = new Pen(Color.Black);
        g.DrawRectangle(borderPen, barX, barY, barWidth, barHeight);

        using var brush = new SolidBrush(Color.Black);
        g.DrawString(maxVal.ToString("F2"), Font, brush, barX + barWidth + 3, barY);
        g.DrawString(minVal.ToString("F2"), Font, brush, barX + barWidth + 3, barY + barHeight - 15);
    }

    private static Color GetHeatmapColor(double normalized)
    {
        normalized = Math.Max(0, Math.Min(1, normalized));

        int r, g, b;
        if (normalized < 0.25)
        {
            double t = normalized / 0.25;
            r = 0;
            g = (int)(255 * t);
            b = 255;
        }
        else if (normalized < 0.5)
        {
            double t = (normalized - 0.25) / 0.25;
            r = 0;
            g = 255;
            b = (int)(255 * (1 - t));
        }
        else if (normalized < 0.75)
        {
            double t = (normalized - 0.5) / 0.25;
            r = (int)(255 * t);
            g = 255;
            b = 0;
        }
        else
        {
            double t = (normalized - 0.75) / 0.25;
            r = 255;
            g = (int)(255 * (1 - t));
            b = 0;
        }

        return Color.FromArgb(r, g, b);
    }
}
