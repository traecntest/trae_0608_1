namespace BioCAD.App.Pages;

public class DashboardPage : UserControl
{
    public DashboardPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(245, 247, 250);
        AutoScroll = true;
        Padding = new Padding(15);

        var mainPanel = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            BackColor = Color.Transparent
        };

        var recentPanel = CreateCard("最近计算任务", 0, 0, 980, 310);
        FillRecentTasks(recentPanel);

        var recentWrapper = new Panel
        {
            Dock = DockStyle.Top,
            Height = 320,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 15, 0, 0)
        };
        recentWrapper.Controls.Add(recentPanel);
        mainPanel.Controls.Add(recentWrapper);

        var chartsPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 360,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 15, 0, 0)
        };

        var leftChart = CreateCard("最近任务状态", 0, 0, 480, 340);
        var rightChart = CreateCard("活性数据分布", 500, 0, 480, 340);
        chartsPanel.Controls.Add(leftChart);
        chartsPanel.Controls.Add(rightChart);

        FillTaskStatusChart(leftChart);
        FillActivityDistributionChart(rightChart);

        mainPanel.Controls.Add(chartsPanel);

        var statsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 110,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 10, 0, 0)
        };

        statsPanel.Controls.Add(CreateStatCard("化合物", "1,234", "个", Color.FromArgb(52, 152, 219)));
        statsPanel.Controls.Add(CreateStatCard("蛋白质", "86", "个", Color.FromArgb(46, 204, 113)));
        statsPanel.Controls.Add(CreateStatCard("活性数据", "5,678", "条", Color.FromArgb(230, 126, 34)));
        statsPanel.Controls.Add(CreateStatCard("计算任务", "23", "个", Color.FromArgb(155, 89, 182)));

        mainPanel.Controls.Add(statsPanel);

        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 65,
            BackColor = Color.Transparent
        };

        var titleLabel = new Label
        {
            Text = "仪表盘",
            Font = new Font("Microsoft YaHei UI", 20f, FontStyle.Bold),
            ForeColor = Color.FromArgb(44, 62, 80),
            AutoSize = true,
            Location = new Point(5, 2),
            BackColor = Color.Transparent
        };
        headerPanel.Controls.Add(titleLabel);

        var subtitleLabel = new Label
        {
            Text = "系统概览与快速统计",
            Font = new Font("Microsoft YaHei UI", 10f),
            ForeColor = Color.FromArgb(127, 140, 141),
            AutoSize = true,
            Location = new Point(5, 40),
            BackColor = Color.Transparent
        };
        headerPanel.Controls.Add(subtitleLabel);

        mainPanel.Controls.Add(headerPanel);

        Controls.Add(mainPanel);
    }

    private static Panel CreateStatCard(string label, string value, string unit, Color color)
    {
        var card = new Panel
        {
            Width = 230,
            Height = 100,
            BackColor = Color.White,
            Padding = new Padding(15),
            Margin = new Padding(0, 0, 15, 10)
        };

        var colorBar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 5,
            BackColor = color
        };
        card.Controls.Add(colorBar);

        var contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12, 8, 0, 0)
        };

        var labelCtrl = new Label
        {
            Text = label,
            Font = new Font("Microsoft YaHei UI", 9f),
            ForeColor = Color.FromArgb(127, 140, 141),
            AutoSize = true,
            Location = new Point(12, 8),
            BackColor = Color.Transparent
        };

        var valueLabel = new Label
        {
            Text = value,
            Font = new Font("Microsoft YaHei UI", 24f, FontStyle.Bold),
            ForeColor = color,
            AutoSize = true,
            Location = new Point(12, 28),
            BackColor = Color.Transparent
        };

        var unitLabel = new Label
        {
            Text = unit,
            Font = new Font("Microsoft YaHei UI", 10f),
            ForeColor = Color.FromArgb(127, 140, 141),
            AutoSize = true,
            Location = new Point(12, 70),
            BackColor = Color.Transparent
        };

        contentPanel.Controls.Add(labelCtrl);
        contentPanel.Controls.Add(valueLabel);
        contentPanel.Controls.Add(unitLabel);
        card.Controls.Add(contentPanel);

        return card;
    }

    private static Panel CreateCard(string title, int x, int y, int width, int height)
    {
        var card = new Panel
        {
            Left = x,
            Top = y,
            Width = width,
            Height = height,
            BackColor = Color.White,
            Padding = new Padding(15)
        };

        var titleLabel = new Label
        {
            Text = title,
            Font = new Font("Microsoft YaHei UI", 12f, FontStyle.Bold),
            ForeColor = Color.FromArgb(44, 62, 80),
            AutoSize = true,
            Location = new Point(15, 12),
            Dock = DockStyle.Top,
            Padding = new Padding(0, 5, 0, 10)
        };
        card.Controls.Add(titleLabel);

        var separator = new Panel
        {
            Height = 1,
            BackColor = Color.FromArgb(236, 240, 241),
            Dock = DockStyle.Top
        };
        card.Controls.Add(separator);

        return card;
    }

    private static void FillTaskStatusChart(Panel card)
    {
        var chartArea = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20)
        };

        var data = new[]
        {
            new { Name = "已完成", Count = 45, Color = Color.FromArgb(46, 204, 113) },
            new { Name = "运行中", Count = 8, Color = Color.FromArgb(52, 152, 219) },
            new { Name = "等待中", Count = 12, Color = Color.FromArgb(241, 196, 15) },
            new { Name = "失败", Count = 3, Color = Color.FromArgb(231, 76, 60) }
        };

        int barY = 50;
        int total = data.Sum(d => d.Count);

        foreach (var item in data)
        {
            var row = new Panel
            {
                Left = 20,
                Top = barY,
                Width = card.Width - 60,
                Height = 35
            };

            var nameLabel = new Label
            {
                Text = item.Name,
                AutoSize = true,
                Location = new Point(0, 8),
                Width = 60,
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            var barBg = new Panel
            {
                Left = 70,
                Top = 10,
                Width = 250,
                Height = 15,
                BackColor = Color.FromArgb(236, 240, 241)
            };

            double percent = (double)item.Count / total;
            var barFg = new Panel
            {
                Left = 0,
                Top = 0,
                Width = (int)(250 * percent),
                Height = 15,
                BackColor = item.Color
            };
            barBg.Controls.Add(barFg);

            var countLabel = new Label
            {
                Text = $"{item.Count} ({percent * 100:F1}%)",
                AutoSize = true,
                Location = new Point(330, 8),
                ForeColor = item.Color,
                Font = new Font("Microsoft YaHei UI", 9f, FontStyle.Bold)
            };

            row.Controls.Add(nameLabel);
            row.Controls.Add(barBg);
            row.Controls.Add(countLabel);
            chartArea.Controls.Add(row);

            barY += 45;
        }

        card.Controls.Add(chartArea);
    }

    private static void FillActivityDistributionChart(Panel card)
    {
        var chartArea = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20, 30, 20, 20)
        };

        var data = new[]
        {
            new { Range = "< 1nM", Count = 45 },
            new { Range = "1-10nM", Count = 120 },
            new { Range = "10-100nM", Count = 280 },
            new { Range = "100nM-1μM", Count = 450 },
            new { Range = "1-10μM", Count = 380 },
            new { Range = "> 10μM", Count = 210 }
        };

        int maxCount = data.Max(d => d.Count);
        int chartHeight = 220;
        int barWidth = 55;
        int startX = 30;
        int startY = 250;

        for (int i = 0; i < data.Length; i++)
        {
            int barHeight = (int)((double)data[i].Count / maxCount * chartHeight);
            int barX = startX + i * (barWidth + 15);

            var bar = new Panel
            {
                Left = barX,
                Top = startY - barHeight,
                Width = barWidth,
                Height = barHeight,
                BackColor = Color.FromArgb(52, 152, 219)
            };

            var countLabel = new Label
            {
                Text = data[i].Count.ToString(),
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 73, 94),
                Font = new Font("Microsoft YaHei UI", 8f),
                Location = new Point(barX + barWidth / 2 - 15, startY - barHeight - 18)
            };

            var labelLabel = new Label
            {
                Text = data[i].Range,
                AutoSize = true,
                ForeColor = Color.FromArgb(127, 140, 141),
                Font = new Font("Microsoft YaHei UI", 8f),
                Location = new Point(barX + barWidth / 2 - 20, startY + 5)
            };

            chartArea.Controls.Add(bar);
            chartArea.Controls.Add(countLabel);
            chartArea.Controls.Add(labelLabel);
        }

        card.Controls.Add(chartArea);
    }

    private static void FillRecentTasks(Panel card)
    {
        var listPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(15, 10, 15, 15),
            AutoScroll = true
        };

        var tasks = new[]
        {
            new { Name = "分子对接 - EGFR", Type = "分子对接", Status = "已完成", StatusColor = Color.FromArgb(46, 204, 113), Time = "2024-01-15 14:30" },
            new { Name = "虚拟筛选 - ZINC库", Type = "虚拟筛选", Status = "运行中", StatusColor = Color.FromArgb(52, 152, 219), Time = "2024-01-15 10:00" },
            new { Name = "药效团模型构建", Type = "药效团建模", Status = "已完成", StatusColor = Color.FromArgb(46, 204, 113), Time = "2024-01-14 16:45" },
            new { Name = "MD模拟 - 10ns", Type = "分子动力学", Status = "等待中", StatusColor = Color.FromArgb(241, 196, 15), Time = "2024-01-15 15:00" },
            new { Name = "聚类分析", Type = "数据分析", Status = "已完成", StatusColor = Color.FromArgb(46, 204, 113), Time = "2024-01-14 09:20" },
            new { Name = "QSAR模型训练", Type = "AI模型", Status = "失败", StatusColor = Color.FromArgb(231, 76, 60), Time = "2024-01-13 11:30" }
        };

        int headerY = 10;
        var header = new Panel
        {
            Left = 15,
            Top = headerY,
            Width = listPanel.Width - 30,
            Height = 30,
            BackColor = Color.FromArgb(248, 249, 250)
        };
        header.Controls.Add(new Label { Text = "任务名称", Left = 20, Top = 8, Width = 200, ForeColor = Color.FromArgb(52, 73, 94), Font = new Font("Microsoft YaHei UI", 9f, FontStyle.Bold) });
        header.Controls.Add(new Label { Text = "类型", Left = 280, Top = 8, Width = 120, ForeColor = Color.FromArgb(52, 73, 94), Font = new Font("Microsoft YaHei UI", 9f, FontStyle.Bold) });
        header.Controls.Add(new Label { Text = "状态", Left = 450, Top = 8, Width = 100, ForeColor = Color.FromArgb(52, 73, 94), Font = new Font("Microsoft YaHei UI", 9f, FontStyle.Bold) });
        header.Controls.Add(new Label { Text = "时间", Left = 600, Top = 8, Width = 200, ForeColor = Color.FromArgb(52, 73, 94), Font = new Font("Microsoft YaHei UI", 9f, FontStyle.Bold) });
        listPanel.Controls.Add(header);

        for (int i = 0; i < tasks.Length; i++)
        {
            var task = tasks[i];
            var row = new Panel
            {
                Left = 15,
                Top = 45 + i * 38,
                Width = listPanel.Width - 30,
                Height = 35,
                BackColor = i % 2 == 0 ? Color.White : Color.FromArgb(248, 249, 250)
            };

            row.Controls.Add(new Label { Text = task.Name, Left = 20, Top = 10, Width = 240, ForeColor = Color.FromArgb(52, 73, 94) });
            row.Controls.Add(new Label { Text = task.Type, Left = 280, Top = 10, Width = 120, ForeColor = Color.FromArgb(127, 140, 141) });

            var statusBadge = new Label
            {
                Text = task.Status,
                Left = 450,
                Top = 7,
                Width = 60,
                Height = 22,
                ForeColor = Color.White,
                BackColor = task.StatusColor,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft YaHei UI", 8f),
                FlatStyle = FlatStyle.Flat
            };
            row.Controls.Add(statusBadge);

            row.Controls.Add(new Label { Text = task.Time, Left = 600, Top = 10, Width = 200, ForeColor = Color.FromArgb(127, 140, 141) });

            listPanel.Controls.Add(row);
        }

        card.Controls.Add(listPanel);
    }
}
