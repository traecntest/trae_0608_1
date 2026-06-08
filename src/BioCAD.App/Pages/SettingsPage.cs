namespace BioCAD.App.Pages;

public class SettingsPage : UserControl
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(245, 247, 250);
        Padding = new Padding(10);

        var scrollPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.Transparent
        };

        var contentPanel = new Panel
        {
            Width = 800,
            AutoSize = true,
            Left = 0,
            Top = 0
        };

        var titleLabel = new Label
        {
            Text = "系统设置",
            Font = new Font("Microsoft YaHei UI", 18f, FontStyle.Bold),
            ForeColor = Color.FromArgb(44, 62, 80),
            AutoSize = true,
            Location = new Point(0, 0)
        };
        contentPanel.Controls.Add(titleLabel);

        var subtitleLabel = new Label
        {
            Text = "配置系统参数与计算资源",
            Font = new Font("Microsoft YaHei UI", 9f),
            ForeColor = Color.FromArgb(127, 140, 141),
            AutoSize = true,
            Location = new Point(0, 35)
        };
        contentPanel.Controls.Add(subtitleLabel);

        int y = 80;

        var dbSection = CreateSection("数据库设置", 0, ref y, contentPanel);
        AddSettingRow(dbSection, "数据库路径", "biocad.db", "file", ref y);
        AddSettingRow(dbSection, "自动备份", "开启", "switch", ref y);
        AddSettingRow(dbSection, "备份频率", "每天", "combo", ref y, new[] { "每小时", "每天", "每周", "每月" });
        y += 20;

        var computeSection = CreateSection("计算资源", y - 80, ref y, contentPanel);
        AddSettingRow(computeSection, "最大并行任务数", "2", "number", ref y, null, 1, 10);
        AddSettingRow(computeSection, "CPU核心数", Environment.ProcessorCount.ToString(), "number", ref y, null, 1, Environment.ProcessorCount);
        AddSettingRow(computeSection, "启用GPU加速", "关闭", "switch", ref y);
        AddSettingRow(computeSection, "GPU设备", "GPU 0", "combo", ref y, new[] { "GPU 0", "GPU 1", "所有GPU" });
        AddSettingRow(computeSection, "任务队列名称", "default", "text", ref y);
        y += 20;

        var fileSection = CreateSection("文件设置", y - 80, ref y, contentPanel);
        AddSettingRow(fileSection, "工作目录", "./workspace", "text", ref y);
        AddSettingRow(fileSection, "结果目录", "./results", "text", ref y);
        AddSettingRow(fileSection, "临时目录", "./temp", "text", ref y);
        AddSettingRow(fileSection, "自动清理临时文件", "开启", "switch", ref y);
        y += 20;

        var visualSection = CreateSection("可视化设置", y - 80, ref y, contentPanel);
        AddSettingRow(visualSection, "背景颜色", "白色", "combo", ref y, new[] { "白色", "黑色", "灰色", "蓝色" });
        AddSettingRow(visualSection, "显示原子标签", "开启", "switch", ref y);
        AddSettingRow(visualSection, "默认显示模式", "2D", "combo", ref y, new[] { "2D", "3D" });
        AddSettingRow(visualSection, "抗锯齿", "高", "combo", ref y, new[] { "低", "中", "高", "最高" });
        y += 20;

        var aboutSection = CreateSection("关于", y - 80, ref y, contentPanel);
        AddAboutRow(aboutSection, "软件版本", "BioCAD v1.0.0", ref y);
        AddAboutRow(aboutSection, ".NET 版本", ".NET 8.0", ref y);
        AddAboutRow(aboutSection, "开发团队", "BioCAD Team", ref y);
        AddAboutRow(aboutSection, "版权信息", "© 2024 All Rights Reserved", ref y);

        var saveBtn = new Button
        {
            Text = "保存设置",
            Left = 0,
            Top = y + 10,
            Width = 120,
            Height = 35,
            BackColor = Color.FromArgb(52, 152, 219),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Microsoft YaHei UI", 10f),
            Cursor = Cursors.Hand
        };
        saveBtn.Click += (s, e) => MessageBox.Show("设置已保存", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        contentPanel.Controls.Add(saveBtn);

        var resetBtn = new Button
        {
            Text = "恢复默认",
            Left = 140,
            Top = y + 10,
            Width = 120,
            Height = 35,
            BackColor = Color.FromArgb(127, 140, 141),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Microsoft YaHei UI", 10f),
            Cursor = Cursors.Hand
        };
        resetBtn.Click += (s, e) => MessageBox.Show("已恢复默认设置", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        contentPanel.Controls.Add(resetBtn);

        scrollPanel.Controls.Add(contentPanel);
        Controls.Add(scrollPanel);
    }

    private static Panel CreateSection(string title, int top, ref int y, Control parent)
    {
        var section = new Panel
        {
            Left = 0,
            Top = top + 80,
            Width = 760,
            Height = 0,
            BackColor = Color.White,
            Padding = new Padding(15)
        };

        var titleLabel = new Label
        {
            Text = title,
            Font = new Font("Microsoft YaHei UI", 12f, FontStyle.Bold),
            ForeColor = Color.FromArgb(44, 62, 80),
            Dock = DockStyle.Top,
            Height = 35,
            Padding = new Padding(0, 5, 0, 10)
        };
        section.Controls.Add(titleLabel);

        var separator = new Panel
        {
            Height = 1,
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(236, 240, 241)
        };
        section.Controls.Add(separator);

        parent.Controls.Add(section);
        y = top + 80 + 50;
        return section;
    }

    private static void AddSettingRow(Panel parent, string label, string value, string type,
        ref int y, string[]? comboItems = null, int min = 0, int max = 100)
    {
        int rowHeight = 40;
        int labelWidth = 180;
        int controlLeft = 200;
        int controlWidth = 300;

        var labelCtrl = new Label
        {
            Text = label,
            Left = 15,
            Top = parent.Height + 10,
            Width = labelWidth,
            ForeColor = Color.FromArgb(52, 73, 94),
            TextAlign = ContentAlignment.MiddleLeft
        };
        parent.Controls.Add(labelCtrl);

        Control? valueCtrl = null;

        switch (type)
        {
            case "text":
                valueCtrl = new TextBox
                {
                    Text = value,
                    Left = controlLeft,
                    Top = parent.Height + 8,
                    Width = controlWidth
                };
                break;
            case "number":
                valueCtrl = new NumericUpDown
                {
                    Value = int.TryParse(value, out int v) ? v : 0,
                    Minimum = min,
                    Maximum = max,
                    Left = controlLeft,
                    Top = parent.Height + 8,
                    Width = 150
                };
                break;
            case "combo":
                var combo = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Left = controlLeft,
                    Top = parent.Height + 8,
                    Width = 200
                };
                if (comboItems != null)
                {
                    combo.Items.AddRange(comboItems);
                    combo.SelectedItem = value;
                }
                valueCtrl = combo;
                break;
            case "switch":
                valueCtrl = new CheckBox
                {
                    Text = value,
                    Checked = value == "开启",
                    Left = controlLeft,
                    Top = parent.Height + 10,
                    AutoSize = true,
                    ForeColor = Color.FromArgb(52, 73, 94)
                };
                break;
            case "file":
                var panel = new Panel
                {
                    Left = controlLeft,
                    Top = parent.Height + 8,
                    Width = controlWidth + 80,
                    Height = 28
                };
                var text = new TextBox
                {
                    Text = value,
                    Left = 0,
                    Top = 0,
                    Width = controlWidth
                };
                var btn = new Button
                {
                    Text = "浏览",
                    Left = controlWidth + 5,
                    Top = -2,
                    Width = 70,
                    Height = 26,
                    BackColor = Color.FromArgb(52, 152, 219),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Microsoft YaHei UI", 8f)
                };
                panel.Controls.Add(text);
                panel.Controls.Add(btn);
                valueCtrl = panel;
                break;
        }

        if (valueCtrl != null)
        {
            parent.Controls.Add(valueCtrl);
        }

        parent.Height += rowHeight + 10;
        y += rowHeight + 10;
    }

    private static void AddAboutRow(Panel parent, string label, string value, ref int y)
    {
        int rowHeight = 35;
        int labelWidth = 180;

        var labelCtrl = new Label
        {
            Text = label,
            Left = 15,
            Top = parent.Height + 8,
            Width = labelWidth,
            ForeColor = Color.FromArgb(127, 140, 141),
            TextAlign = ContentAlignment.MiddleLeft
        };
        parent.Controls.Add(labelCtrl);

        var valueCtrl = new Label
        {
            Text = value,
            Left = 200,
            Top = parent.Height + 8,
            AutoSize = true,
            ForeColor = Color.FromArgb(52, 73, 94)
        };
        parent.Controls.Add(valueCtrl);

        parent.Height += rowHeight;
        y += rowHeight;
    }
}
