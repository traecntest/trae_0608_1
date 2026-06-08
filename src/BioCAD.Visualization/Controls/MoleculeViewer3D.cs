using BioCAD.Domain.Entities;

namespace BioCAD.Visualization.Controls;

public partial class MoleculeViewer3D : UserControl
{
    private Compound? _compound;
    private float _rotationX = 20f;
    private float _rotationY = 30f;
    private float _zoom = 1.0f;
    private bool _isDragging = false;
    private Point _lastMousePos;
    private readonly System.Windows.Forms.Timer _animationTimer;
    private bool _autoRotate = false;

    public Compound? Compound
    {
        get => _compound;
        set { _compound = value; Invalidate(); }
    }

    public bool AutoRotate
    {
        get => _autoRotate;
        set { _autoRotate = value; _animationTimer.Enabled = value; }
    }

    public MoleculeViewer3D()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

        _animationTimer = new System.Windows.Forms.Timer { Interval = 50 };
        _animationTimer.Tick += (s, e) =>
        {
            if (_autoRotate)
            {
                _rotationY += 1f;
                Invalidate();
            }
        };
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.Black);

        if (_compound == null || !_compound.Atoms.Any())
        {
            using var brush = new SolidBrush(Color.Gray);
            g.DrawString("请加载分子结构\n(3D视图)", Font, brush, ClientRectangle,
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            return;
        }

        DrawMolecule3D(g);
        DrawControls(g);
    }

    private void DrawMolecule3D(Graphics g)
    {
        if (_compound == null) return;

        var projectedAtoms = new List<(Atom atom, PointF pos, float z, float radius)>();
        float centerX = ClientSize.Width / 2f;
        float centerY = ClientSize.Height / 2f;

        var bounds = GetMoleculeBounds();
        float maxDim = Math.Max(bounds.Width, Math.Max(bounds.Height, bounds.Depth));
        float scale = Math.Min(ClientSize.Width, ClientSize.Height) / (maxDim * 2.5f) * _zoom;

        foreach (var atom in _compound.Atoms)
        {
            var p = RotatePoint(
                (float)atom.X - bounds.CenterX,
                (float)atom.Y - bounds.CenterY,
                (float)atom.Z - bounds.CenterZ,
                _rotationX, _rotationY);

            float z = p.Z;
            float screenX = centerX + p.X * scale;
            float screenY = centerY + p.Y * scale;
            float radius = GetAtomRadius(atom.Element) * _zoom * (1 + z / (maxDim * 2));

            projectedAtoms.Add((atom, new PointF(screenX, screenY), z, Math.Max(2, radius)));
        }

        var sortedAtoms = projectedAtoms.OrderBy(a => a.z).ToList();

        var bondList = new List<(PointF p1, PointF p2, float z, int order)>();
        foreach (var bond in _compound.Bonds)
        {
            var atom1 = _compound.Atoms.FirstOrDefault(a => a.Id == bond.Atom1Id);
            var atom2 = _compound.Atoms.FirstOrDefault(a => a.Id == bond.Atom2Id);
            if (atom1 != null && atom2 != null)
            {
                var p1 = RotatePoint(
                    (float)atom1.X - bounds.CenterX,
                    (float)atom1.Y - bounds.CenterY,
                    (float)atom1.Z - bounds.CenterZ,
                    _rotationX, _rotationY);
                var p2 = RotatePoint(
                    (float)atom2.X - bounds.CenterX,
                    (float)atom2.Y - bounds.CenterY,
                    (float)atom2.Z - bounds.CenterZ,
                    _rotationX, _rotationY);

                float avgZ = (p1.Z + p2.Z) / 2;
                bondList.Add((
                    new PointF(centerX + p1.X * scale, centerY + p1.Y * scale),
                    new PointF(centerX + p2.X * scale, centerY + p2.Y * scale),
                    avgZ, bond.Order));
            }
        }

        var sortedBonds = bondList.OrderBy(b => b.z).ToList();

        int atomIdx = 0;
        int bondIdx = 0;

        while (atomIdx < sortedAtoms.Count && bondIdx < sortedBonds.Count)
        {
            if (sortedAtoms[atomIdx].z < sortedBonds[bondIdx].z)
            {
                var a = sortedAtoms[atomIdx];
                DrawAtom3D(g, a.pos, a.radius, a.atom);
                atomIdx++;
            }
            else
            {
                var b = sortedBonds[bondIdx];
                DrawBond3D(g, b.p1, b.p2, b.order, b.z);
                bondIdx++;
            }
        }

        while (atomIdx < sortedAtoms.Count)
        {
            var a = sortedAtoms[atomIdx];
            DrawAtom3D(g, a.pos, a.radius, a.atom);
            atomIdx++;
        }

        while (bondIdx < sortedBonds.Count)
        {
            var b = sortedBonds[bondIdx];
            DrawBond3D(g, b.p1, b.p2, b.order, b.z);
            bondIdx++;
        }
    }

    private static void DrawAtom3D(Graphics g, PointF pos, float radius, Atom atom)
    {
        var color = GetAtomColor(atom.Element);

        var gradientRect = new RectangleF(pos.X - radius, pos.Y - radius, radius * 2, radius * 2);
        using var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddEllipse(gradientRect);
        using var gradient = new System.Drawing.Drawing2D.PathGradientBrush(path);
        gradient.CenterPoint = new PointF(pos.X - radius * 0.3f, pos.Y - radius * 0.3f);
        gradient.CenterColor = Color.FromArgb(
            Math.Min(255, color.R + 80),
            Math.Min(255, color.G + 80),
            Math.Min(255, color.B + 80));
        gradient.SurroundColors = new[] { color };

        g.FillEllipse(gradient, gradientRect);

        var highlightRect = new RectangleF(
            pos.X - radius * 0.6f, pos.Y - radius * 0.6f,
            radius * 0.8f, radius * 0.8f);
        using var highlightBrush = new SolidBrush(Color.FromArgb(100, Color.White));
        g.FillEllipse(highlightBrush, highlightRect);

        if (radius > 8)
        {
            using var textBrush = new SolidBrush(GetTextColor(color));
            var textSize = g.MeasureString(atom.Element, SystemFonts.DefaultFont);
            g.DrawString(atom.Element, SystemFonts.DefaultFont, textBrush,
                pos.X - textSize.Width / 2, pos.Y - textSize.Height / 2);
        }
    }

    private static void DrawBond3D(Graphics g, PointF p1, PointF p2, int order, float z)
    {
        int alpha = (int)Math.Min(255, 150 + (z + 50) * 2);
        alpha = Math.Max(50, Math.Min(255, alpha));

        using var pen = new Pen(Color.FromArgb(alpha, Color.LightGray), 2);
        float dx = p2.X - p1.X;
        float dy = p2.Y - p1.Y;
        float length = (float)Math.Sqrt(dx * dx + dy * dy);
        if (length < 1) return;

        float nx = -dy / length * 1.5f;
        float ny = dx / length * 1.5f;

        switch (order)
        {
            case 1:
                g.DrawLine(pen, p1, p2);
                break;
            case 2:
                g.DrawLine(pen, p1.X + nx, p1.Y + ny, p2.X + nx, p2.Y + ny);
                g.DrawLine(pen, p1.X - nx, p1.Y - ny, p2.X - nx, p2.Y - ny);
                break;
            default:
                g.DrawLine(pen, p1, p2);
                break;
        }
    }

    private static (float X, float Y, float Z) RotatePoint(float x, float y, float z, float angleX, float angleY)
    {
        float radX = angleX * (float)Math.PI / 180f;
        float radY = angleY * (float)Math.PI / 180f;

        float cosX = (float)Math.Cos(radX);
        float sinX = (float)Math.Sin(radX);
        float y1 = y * cosX - z * sinX;
        float z1 = y * sinX + z * cosX;

        float cosY = (float)Math.Cos(radY);
        float sinY = (float)Math.Sin(radY);
        float x2 = x * cosY + z1 * sinY;
        float z2 = -x * sinY + z1 * cosY;

        return (x2, y1, z2);
    }

    private static float GetAtomRadius(string element)
    {
        return element switch
        {
            "C" => 12,
            "H" => 5,
            "O" => 11,
            "N" => 10,
            "S" => 14,
            "P" => 13,
            "F" => 8,
            "Cl" => 14,
            "Br" => 16,
            "I" => 18,
            _ => 12
        };
    }

    private static Color GetAtomColor(string element)
    {
        return element switch
        {
            "C" => Color.FromArgb(100, 100, 100),
            "H" => Color.White,
            "O" => Color.FromArgb(220, 30, 30),
            "N" => Color.FromArgb(30, 30, 220),
            "S" => Color.FromArgb(220, 200, 30),
            "P" => Color.FromArgb(255, 140, 0),
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

    private struct MoleculeBounds
    {
        public float Left, Right, Top, Bottom, Front, Back;
        public float Width => Right - Left;
        public float Height => Bottom - Top;
        public float Depth => Back - Front;
        public float CenterX => (Left + Right) / 2;
        public float CenterY => (Top + Bottom) / 2;
        public float CenterZ => (Front + Back) / 2;
    }

    private MoleculeBounds GetMoleculeBounds()
    {
        if (_compound == null || !_compound.Atoms.Any())
            return new MoleculeBounds { Left = -50, Right = 50, Top = -50, Bottom = 50, Front = -50, Back = 50 };

        float minX = _compound.Atoms.Min(a => (float)a.X);
        float maxX = _compound.Atoms.Max(a => (float)a.X);
        float minY = _compound.Atoms.Min(a => (float)a.Y);
        float maxY = _compound.Atoms.Max(a => (float)a.Y);
        float minZ = _compound.Atoms.Min(a => (float)a.Z);
        float maxZ = _compound.Atoms.Max(a => (float)a.Z);

        return new MoleculeBounds
        {
            Left = minX, Right = maxX,
            Top = minY, Bottom = maxY,
            Front = minZ, Back = maxZ
        };
    }

    private void DrawControls(Graphics g)
    {
        using var brush = new SolidBrush(Color.FromArgb(180, Color.Black));
        g.FillRectangle(brush, 5, 5, 120, 60);

        using var textBrush = new SolidBrush(Color.White);
        g.DrawString($"旋转: X={_rotationX:F0}° Y={_rotationY:F0}°", Font, textBrush, 10, 10);
        g.DrawString($"缩放: {_zoom:F1}x", Font, textBrush, 10, 30);
        g.DrawString($"原子数: {_compound?.Atoms.Count ?? 0}", Font, textBrush, 10, 50);
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
            _rotationY += (e.X - _lastMousePos.X) * 0.5f;
            _rotationX -= (e.Y - _lastMousePos.Y) * 0.5f;
            _rotationX = Math.Max(-90, Math.Min(90, _rotationX));
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
        _rotationX = 20f;
        _rotationY = 30f;
        _zoom = 1.0f;
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _animationTimer?.Dispose();
        }
        base.Dispose(disposing);
    }
}
