using BioCAD.Domain.Entities;

namespace BioCAD.Visualization.Controls;

public partial class MoleculeViewer2D : UserControl
{
    private Compound? _compound;
    private float _zoom = 1.0f;
    private PointF _offset = PointF.Empty;
    private bool _isDragging = false;
    private Point _lastMousePos;

    public Compound? Compound
    {
        get => _compound;
        set { _compound = value; Invalidate(); }
    }

    public float Zoom
    {
        get => _zoom;
        set { _zoom = Math.Max(0.1f, Math.Min(10f, value)); Invalidate(); }
    }

    public MoleculeViewer2D()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.White);

        if (_compound == null || !_compound.Atoms.Any())
        {
            g.DrawString("请加载分子结构", Font, Brushes.Gray, ClientRectangle,
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            return;
        }

        DrawMolecule(g);
    }

    private void DrawMolecule(Graphics g)
    {
        if (_compound == null) return;

        var bounds = GetMoleculeBounds();
        float centerX = ClientSize.Width / 2f;
        float centerY = ClientSize.Height / 2f;

        float scaleX = (ClientSize.Width - 40) / bounds.Width;
        float scaleY = (ClientSize.Height - 40) / bounds.Height;
        float scale = Math.Min(scaleX, scaleY) * _zoom;

        float offsetX = centerX - ((bounds.Left + bounds.Right) / 2) * scale + _offset.X;
        float offsetY = centerY - ((bounds.Top + bounds.Bottom) / 2) * scale + _offset.Y;

        var atomPositions = new Dictionary<int, PointF>();
        foreach (var atom in _compound.Atoms)
        {
            atomPositions[atom.Id] = new PointF(
                (float)(atom.X * scale + offsetX),
                (float)(atom.Y * scale + offsetY));
        }

        foreach (var bond in _compound.Bonds)
        {
            if (atomPositions.TryGetValue(bond.Atom1Id, out var p1) &&
                atomPositions.TryGetValue(bond.Atom2Id, out var p2))
            {
                DrawBond(g, p1, p2, bond.Order);
            }
        }

        foreach (var atom in _compound.Atoms)
        {
            if (atomPositions.TryGetValue(atom.Id, out var pos))
            {
                DrawAtom(g, pos, atom);
            }
        }
    }

    private static void DrawBond(Graphics g, PointF p1, PointF p2, int order)
    {
        using var pen = new Pen(Color.Black, 2);
        float dx = p2.X - p1.X;
        float dy = p2.Y - p1.Y;
        float length = (float)Math.Sqrt(dx * dx + dy * dy);
        if (length < 1) return;

        float nx = -dy / length * 2;
        float ny = dx / length * 2;

        switch (order)
        {
            case 1:
                g.DrawLine(pen, p1, p2);
                break;
            case 2:
                g.DrawLine(pen, p1.X + nx, p1.Y + ny, p2.X + nx, p2.Y + ny);
                g.DrawLine(pen, p1.X - nx, p1.Y - ny, p2.X - nx, p2.Y - ny);
                break;
            case 3:
                g.DrawLine(pen, p1, p2);
                g.DrawLine(pen, p1.X + nx * 1.5f, p1.Y + ny * 1.5f, p2.X + nx * 1.5f, p2.Y + ny * 1.5f);
                g.DrawLine(pen, p1.X - nx * 1.5f, p1.Y - ny * 1.5f, p2.X - nx * 1.5f, p2.Y - ny * 1.5f);
                break;
            default:
                g.DrawLine(pen, p1, p2);
                break;
        }
    }

    private static void DrawAtom(Graphics g, PointF pos, Atom atom)
    {
        float radius = GetAtomRadius(atom.Element);
        var color = GetAtomColor(atom.Element);

        using var brush = new SolidBrush(color);
        g.FillEllipse(brush, pos.X - radius, pos.Y - radius, radius * 2, radius * 2);

        using var pen = new Pen(Color.DarkGray, 1);
        g.DrawEllipse(pen, pos.X - radius, pos.Y - radius, radius * 2, radius * 2);

        if (radius > 8)
        {
            using var textBrush = new SolidBrush(GetTextColor(color));
            var textSize = g.MeasureString(atom.Element, SystemFonts.DefaultFont);
            g.DrawString(atom.Element, SystemFonts.DefaultFont, textBrush,
                pos.X - textSize.Width / 2, pos.Y - textSize.Height / 2);
        }
    }

    private static float GetAtomRadius(string element)
    {
        return element switch
        {
            "C" => 10,
            "H" => 5,
            "O" => 9,
            "N" => 9,
            "S" => 12,
            "P" => 11,
            "F" => 7,
            "Cl" => 12,
            "Br" => 14,
            "I" => 16,
            _ => 10
        };
    }

    private static Color GetAtomColor(string element)
    {
        return element switch
        {
            "C" => Color.DarkGray,
            "H" => Color.White,
            "O" => Color.Red,
            "N" => Color.Blue,
            "S" => Color.Yellow,
            "P" => Color.Orange,
            "F" => Color.LightGreen,
            "Cl" => Color.Green,
            "Br" => Color.DarkRed,
            "I" => Color.Purple,
            _ => Color.LightBlue
        };
    }

    private static Color GetTextColor(Color bgColor)
    {
        double luminance = 0.299 * bgColor.R + 0.587 * bgColor.G + 0.114 * bgColor.B;
        return luminance > 128 ? Color.Black : Color.White;
    }

    private RectangleF GetMoleculeBounds()
    {
        if (_compound == null || !_compound.Atoms.Any())
            return new RectangleF(0, 0, 100, 100);

        float minX = _compound.Atoms.Min(a => (float)a.X);
        float maxX = _compound.Atoms.Max(a => (float)a.X);
        float minY = _compound.Atoms.Min(a => (float)a.Y);
        float maxY = _compound.Atoms.Max(a => (float)a.Y);

        return RectangleF.FromLTRB(minX, minY, maxX, maxY);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            _isDragging = true;
            _lastMousePos = e.Location;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_isDragging)
        {
            _offset.X += e.X - _lastMousePos.X;
            _offset.Y += e.Y - _lastMousePos.Y;
            _lastMousePos = e.Location;
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _isDragging = false;
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        float delta = e.Delta > 0 ? 1.1f : 0.9f;
        _zoom = Math.Max(0.1f, Math.Min(10f, _zoom * delta));
        Invalidate();
    }

    public void ResetView()
    {
        _zoom = 1.0f;
        _offset = PointF.Empty;
        Invalidate();
    }
}
