using BioCAD.Domain.Enums;
using TaskStatus = BioCAD.Domain.Enums.TaskStatus;

namespace BioCAD.App.Pages;

public class TaskManagerPage : UserControl
{
    private DataGridView? _taskGrid;
    private Panel? _detailPanel;
    private ProgressBar? _taskProgress;
    private RichTextBox? _logTextBox;
    private System.Windows.Forms.Timer? _refreshTimer;

    private readonly List<TaskItem> _tasks = new();
    private int _nextTaskId = 11;

    public TaskManagerPage()
    {
        InitializeComponent();
        LoadSampleTasks();
        StartRefreshTimer();
    }

    private void InitializeComponent()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(245, 247, 250);
        Padding = new Padding(10);

        var splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 600,
            BackColor = Color.FromArgb(245, 247, 250),
            Padding = new Padding(0, 10, 0, 0)
        };

        _taskGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false,
            BackgroundColor = Color.White,
            GridColor = Color.FromArgb(236, 240, 241),
            DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(52, 73, 94),
                SelectionBackColor = Color.FromArgb(52, 152, 219),
                SelectionForeColor = Color.White,
                Font = new Font("Microsoft YaHei UI", 9f),
                Padding = new Padding(3)
            },
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(248, 249, 250),
                ForeColor = Color.FromArgb(44, 62, 80),
                Font = new Font("Microsoft YaHei UI", 9f, FontStyle.Bold)
            },
            EnableHeadersVisualStyles = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        };

        _taskGrid.Columns.Add("Id", "ID");
        _taskGrid.Columns.Add("Name", "任务名称");
        _taskGrid.Columns.Add("Type", "类型");
        _taskGrid.Columns.Add("Status", "状态");
        _taskGrid.Columns.Add("Progress", "进度");
        _taskGrid.Columns.Add("Priority", "优先级");
        _taskGrid.Columns.Add("CreatedAt", "创建时间");

        _taskGrid.Columns["Id"].Width = 50;
        _taskGrid.Columns["Progress"].Width = 100;

        _taskGrid.SelectionChanged += (s, e) => UpdateTaskDetail();
        splitContainer.Panel1.Controls.Add(_taskGrid);

        _detailPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(15)
        };

        var detailTitle = new Label
        {
            Text = "任务详情",
            Font = new Font("Microsoft YaHei UI", 12f, FontStyle.Bold),
            ForeColor = Color.FromArgb(44, 62, 80),
            Dock = DockStyle.Top,
            Height = 30
        };
        _detailPanel.Controls.Add(detailTitle);

        var progressPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(0, 10, 0, 10)
        };

        var progressLabel = new Label
        {
            Text = "任务进度",
            AutoSize = true,
            Location = new Point(0, 0),
            ForeColor = Color.FromArgb(52, 73, 94)
        };

        _taskProgress = new ProgressBar
        {
            Left = 0,
            Top = 25,
            Width = _detailPanel.Width - 50,
            Height = 20,
            Style = ProgressBarStyle.Continuous,
            ForeColor = Color.FromArgb(52, 152, 219)
        };

        var progressPercentLabel = new Label
        {
            Text = "0%",
            AutoSize = true,
            Location = new Point(_detailPanel.Width - 45, 28),
            ForeColor = Color.FromArgb(52, 152, 219),
            Font = new Font("Microsoft YaHei UI", 9f, FontStyle.Bold)
        };

        progressPanel.Controls.Add(progressLabel);
        progressPanel.Controls.Add(_taskProgress);
        progressPanel.Controls.Add(progressPercentLabel);
        _detailPanel.Controls.Add(progressPanel);

        var logTitle = new Label
        {
            Text = "执行日志",
            Font = new Font("Microsoft YaHei UI", 10f, FontStyle.Bold),
            ForeColor = Color.FromArgb(44, 62, 80),
            Dock = DockStyle.Top,
            Height = 25,
            Padding = new Padding(0, 5, 0, 0)
        };
        _detailPanel.Controls.Add(logTitle);

        _logTextBox = new RichTextBox
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(248, 249, 250),
            ForeColor = Color.FromArgb(52, 73, 94),
            Font = new Font("Consolas", 9f),
            ReadOnly = true,
            BorderStyle = BorderStyle.FixedSingle
        };
        _detailPanel.Controls.Add(_logTextBox);

        splitContainer.Panel2.Controls.Add(_detailPanel);

        var toolbar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = Color.White,
            Padding = new Padding(10)
        };

        var newTaskBtn = CreateButton("新建任务", Color.FromArgb(52, 152, 219), 10, 10);
        newTaskBtn.Click += (s, e) => ShowNewTaskDialog();
        toolbar.Controls.Add(newTaskBtn);

        var startBtn = CreateButton("开始", Color.FromArgb(46, 204, 113), 120, 10);
        startBtn.Click += (s, e) => StartSelectedTask();
        toolbar.Controls.Add(startBtn);

        var pauseBtn = CreateButton("暂停", Color.FromArgb(241, 196, 15), 230, 10);
        pauseBtn.Click += (s, e) => PauseSelectedTask();
        toolbar.Controls.Add(pauseBtn);

        var cancelBtn = CreateButton("取消", Color.FromArgb(231, 76, 60), 340, 10);
        cancelBtn.Click += (s, e) => CancelSelectedTask();
        toolbar.Controls.Add(cancelBtn);

        var deleteBtn = CreateButton("删除", Color.FromArgb(127, 140, 141), 450, 10);
        deleteBtn.Click += (s, e) => DeleteSelectedTask();
        toolbar.Controls.Add(deleteBtn);

        var titlePanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 65,
            BackColor = Color.Transparent
        };

        var titleLabel = new Label
        {
            Text = "任务管理",
            Font = new Font("Microsoft YaHei UI", 18f, FontStyle.Bold),
            ForeColor = Color.FromArgb(44, 62, 80),
            AutoSize = true,
            Location = new Point(0, 2)
        };
        titlePanel.Controls.Add(titleLabel);

        var subtitleLabel = new Label
        {
            Text = "管理和监控计算任务的执行",
            Font = new Font("Microsoft YaHei UI", 9f),
            ForeColor = Color.FromArgb(127, 140, 141),
            AutoSize = true,
            Location = new Point(0, 38)
        };
        titlePanel.Controls.Add(subtitleLabel);

        Controls.Add(splitContainer);
        Controls.Add(toolbar);
        Controls.Add(titlePanel);
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

    private void LoadSampleTasks()
    {
        if (_taskGrid == null) return;

        _tasks.Clear();
        _tasks.AddRange(new[]
        {
            new TaskItem { Id = 1, Name = "分子对接 - EGFR & Gefitinib", Type = "分子对接", Status = TaskStatus.Completed, Progress = 100, Priority = 5, CreatedAt = DateTime.Now.AddDays(-2) },
            new TaskItem { Id = 2, Name = "虚拟筛选 - ZINC库 10k", Type = "虚拟筛选", Status = TaskStatus.Running, Progress = 65, Priority = 7, CreatedAt = DateTime.Now.AddHours(-3) },
            new TaskItem { Id = 3, Name = "药效团模型构建", Type = "药效团建模", Status = TaskStatus.Completed, Progress = 100, Priority = 4, CreatedAt = DateTime.Now.AddDays(-1) },
            new TaskItem { Id = 4, Name = "MD模拟 - 10ns", Type = "分子动力学", Status = TaskStatus.Running, Progress = 32, Priority = 8, CreatedAt = DateTime.Now.AddHours(-8) },
            new TaskItem { Id = 5, Name = "化合物聚类分析", Type = "聚类分析", Status = TaskStatus.Completed, Progress = 100, Priority = 3, CreatedAt = DateTime.Now.AddDays(-3) },
            new TaskItem { Id = 6, Name = "QSAR模型训练", Type = "AI模型", Status = TaskStatus.Failed, Progress = 45, Priority = 6, CreatedAt = DateTime.Now.AddDays(-1) },
            new TaskItem { Id = 7, Name = "批量分子对接", Type = "分子对接", Status = TaskStatus.Queued, Progress = 0, Priority = 5, CreatedAt = DateTime.Now.AddHours(-1) },
            new TaskItem { Id = 8, Name = "MD轨迹分析", Type = "数据分析", Status = TaskStatus.Pending, Progress = 0, Priority = 4, CreatedAt = DateTime.Now.AddMinutes(-30) },
            new TaskItem { Id = 9, Name = "ADMET性质预测", Type = "AI模型", Status = TaskStatus.Queued, Progress = 0, Priority = 6, CreatedAt = DateTime.Now.AddMinutes(-15) },
            new TaskItem { Id = 10, Name = "虚拟筛选 - Enamine库", Type = "虚拟筛选", Status = TaskStatus.Paused, Progress = 28, Priority = 5, CreatedAt = DateTime.Now.AddHours(-5) }
        });

        _nextTaskId = 11;
        RefreshGrid();
        _taskGrid.AutoResizeColumns();
    }

    private void RefreshGrid()
    {
        if (_taskGrid == null) return;

        _taskGrid.Rows.Clear();
        foreach (var task in _tasks)
        {
            int idx = _taskGrid.Rows.Add(task.Id, task.Name, task.Type, task.Status, task.Progress + "%",
                new string('★', task.Priority), task.CreatedAt.ToString("yyyy-MM-dd HH:mm"));

            var row = _taskGrid.Rows[idx];
            var statusCell = row.Cells["Status"];
            statusCell.Style.BackColor = GetStatusColor(task.Status);
            statusCell.Style.ForeColor = Color.White;
            statusCell.Style.SelectionBackColor = GetStatusColor(task.Status);
            statusCell.Style.Padding = new Padding(5);
        }
    }

    private static Color GetStatusColor(TaskStatus status)
    {
        return status switch
        {
            TaskStatus.Completed => Color.FromArgb(46, 204, 113),
            TaskStatus.Running => Color.FromArgb(52, 152, 219),
            TaskStatus.Queued => Color.FromArgb(241, 196, 15),
            TaskStatus.Pending => Color.FromArgb(189, 195, 199),
            TaskStatus.Failed => Color.FromArgb(231, 76, 60),
            TaskStatus.Cancelled => Color.FromArgb(127, 140, 141),
            TaskStatus.Paused => Color.FromArgb(230, 126, 34),
            _ => Color.Gray
        };
    }

    private void UpdateTaskDetail()
    {
        if (_taskGrid == null || _taskGrid.SelectedRows.Count == 0) return;
        if (_logTextBox == null || _taskProgress == null) return;

        var taskIdCell = _taskGrid.SelectedRows[0].Cells["Id"].Value;
        if (taskIdCell == null || !int.TryParse(taskIdCell.ToString(), out int taskId)) return;

        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null) return;

        _taskProgress.Value = task.Progress;

        _logTextBox.Clear();
        _logTextBox.AppendText($"任务: {task.Name}\n");
        _logTextBox.AppendText($"状态: {task.Status}\n");
        _logTextBox.AppendText($"进度: {task.Progress}%\n\n");
        _logTextBox.AppendText("=== 执行日志 ===\n");
        _logTextBox.AppendText($"[{task.CreatedAt:HH:mm:ss}] INFO: 任务创建\n");
        _logTextBox.AppendText($"[{task.CreatedAt.AddMinutes(5):HH:mm:ss}] INFO: 初始化计算环境\n");
        _logTextBox.AppendText($"[{task.CreatedAt.AddMinutes(10):HH:mm:ss}] INFO: 加载输入数据\n");
        if (task.Progress > 0)
        {
            _logTextBox.AppendText($"[{task.CreatedAt.AddMinutes(15):HH:mm:ss}] INFO: 开始计算\n");
            _logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] INFO: 计算中... ({task.Progress}%)\n");
        }
        if (task.Status == TaskStatus.Completed)
        {
            _logTextBox.AppendText($"[{task.CreatedAt.AddHours(2):HH:mm:ss}] INFO: 任务完成\n");
        }
        else if (task.Status == TaskStatus.Failed)
        {
            _logTextBox.AppendText($"[{task.CreatedAt.AddHours(1):HH:mm:ss}] ERROR: 计算失败 - 参数错误\n");
        }
        else if (task.Status == TaskStatus.Cancelled)
        {
            _logTextBox.AppendText($"[{task.CreatedAt.AddMinutes(30):HH:mm:ss}] WARN: 任务已取消\n");
        }
        else if (task.Status == TaskStatus.Paused)
        {
            _logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] WARN: 任务已暂停\n");
        }
    }

    private void StartRefreshTimer()
    {
        _refreshTimer = new System.Windows.Forms.Timer { Interval = 2000 };
        _refreshTimer.Tick += (s, e) => RefreshTaskStatus();
        _refreshTimer.Start();
    }

    private void RefreshTaskStatus()
    {
        if (_taskGrid == null) return;

        bool hasChanges = false;
        foreach (var task in _tasks)
        {
            if (task.Status == TaskStatus.Running)
            {
                task.Progress = Math.Min(100, task.Progress + 1);
                if (task.Progress >= 100)
                {
                    task.Status = TaskStatus.Completed;
                }
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            var selectedId = -1;
            if (_taskGrid.SelectedRows.Count > 0 && _taskGrid.SelectedRows[0].Cells["Id"].Value != null)
            {
                int.TryParse(_taskGrid.SelectedRows[0].Cells["Id"].Value.ToString(), out selectedId);
            }

            RefreshGrid();

            if (selectedId > 0)
            {
                foreach (DataGridViewRow row in _taskGrid.Rows)
                {
                    if (row.Cells["Id"].Value?.ToString() == selectedId.ToString())
                    {
                        row.Selected = true;
                        break;
                    }
                }
            }
        }
    }

    private void ShowNewTaskDialog()
    {
        using var dialog = new Form
        {
            Text = "新建计算任务",
            Width = 500,
            Height = 450,
            StartPosition = FormStartPosition.CenterParent,
            Font = new Font("Microsoft YaHei UI", 9f),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var taskTypeLabel = new Label { Text = "任务类型:", Left = 30, Top = 30, Width = 80 };
        var taskTypeCombo = new ComboBox
        {
            Left = 120,
            Top = 27,
            Width = 320,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        taskTypeCombo.Items.AddRange(new object[] { "分子对接", "虚拟筛选", "药效团建模", "分子动力学模拟", "聚类分析", "QSAR模型训练" });
        taskTypeCombo.SelectedIndex = 0;

        var nameLabel = new Label { Text = "任务名称:", Left = 30, Top = 70, Width = 80 };
        var nameTextBox = new TextBox { Left = 120, Top = 67, Width = 320, Text = "新计算任务" };

        var priorityLabel = new Label { Text = "优先级:", Left = 30, Top = 110, Width = 80 };
        var priorityTrackBar = new TrackBar { Left = 120, Top = 105, Width = 200, Minimum = 1, Maximum = 10, Value = 5 };

        var cpuLabel = new Label { Text = "CPU核心:", Left = 30, Top = 160, Width = 80 };
        var cpuNumeric = new NumericUpDown { Left = 120, Top = 157, Width = 100, Minimum = 1, Maximum = Environment.ProcessorCount, Value = 2 };

        var gpuCheck = new CheckBox { Text = "使用GPU加速", Left = 120, Top = 195, Checked = false };

        var paramLabel = new Label { Text = "参数设置:", Left = 30, Top = 230, Width = 80 };
        var paramTextBox = new TextBox
        {
            Left = 120,
            Top = 227,
            Width = 320,
            Height = 80,
            Multiline = true,
            Text = "{}"
        };

        var okButton = new Button
        {
            Text = "提交任务",
            Left = 250,
            Top = 350,
            Width = 100,
            Height = 35,
            BackColor = Color.FromArgb(52, 152, 219),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            DialogResult = DialogResult.OK
        };

        var cancelButton = new Button
        {
            Text = "取消",
            Left = 360,
            Top = 350,
            Width = 80,
            Height = 35,
            BackColor = Color.FromArgb(127, 140, 141),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            DialogResult = DialogResult.Cancel
        };

        dialog.Controls.AddRange(new Control[]
        {
            taskTypeLabel, taskTypeCombo, nameLabel, nameTextBox,
            priorityLabel, priorityTrackBar, cpuLabel, cpuNumeric,
            gpuCheck, paramLabel, paramTextBox, okButton, cancelButton
        });

        dialog.AcceptButton = okButton;
        dialog.CancelButton = cancelButton;

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            var newTask = new TaskItem
            {
                Id = _nextTaskId++,
                Name = nameTextBox.Text,
                Type = taskTypeCombo.SelectedItem?.ToString() ?? "未知",
                Status = TaskStatus.Queued,
                Progress = 0,
                Priority = priorityTrackBar.Value,
                CreatedAt = DateTime.Now
            };

            _tasks.Insert(0, newTask);
            RefreshGrid();

            if (_taskGrid != null && _taskGrid.Rows.Count > 0)
            {
                _taskGrid.Rows[0].Selected = true;
            }

            UpdateTaskDetail();
            MessageBox.Show("任务已提交到队列", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private TaskItem? GetSelectedTask()
    {
        if (_taskGrid == null || _taskGrid.SelectedRows.Count == 0) return null;
        var idCell = _taskGrid.SelectedRows[0].Cells["Id"].Value;
        if (idCell == null || !int.TryParse(idCell.ToString(), out int id)) return null;
        return _tasks.FirstOrDefault(t => t.Id == id);
    }

    private void StartSelectedTask()
    {
        var task = GetSelectedTask();
        if (task == null)
        {
            MessageBox.Show("请先选择一个任务", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (task.Status is TaskStatus.Running or TaskStatus.Completed)
        {
            MessageBox.Show("该任务无法开始", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        task.Status = TaskStatus.Running;
        RefreshGrid();
        UpdateTaskDetail();
    }

    private void PauseSelectedTask()
    {
        var task = GetSelectedTask();
        if (task == null)
        {
            MessageBox.Show("请先选择一个任务", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (task.Status != TaskStatus.Running)
        {
            MessageBox.Show("只有运行中的任务才能暂停", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        task.Status = TaskStatus.Paused;
        RefreshGrid();
        UpdateTaskDetail();
    }

    private void CancelSelectedTask()
    {
        var task = GetSelectedTask();
        if (task == null)
        {
            MessageBox.Show("请先选择一个任务", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (task.Status is TaskStatus.Completed or TaskStatus.Cancelled or TaskStatus.Failed)
        {
            MessageBox.Show("该任务无法取消", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = MessageBox.Show("确定要取消该任务吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result == DialogResult.Yes)
        {
            task.Status = TaskStatus.Cancelled;
            RefreshGrid();
            UpdateTaskDetail();
        }
    }

    private void DeleteSelectedTask()
    {
        var task = GetSelectedTask();
        if (task == null)
        {
            MessageBox.Show("请先选择一个任务", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = MessageBox.Show("确定要删除该任务吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result == DialogResult.Yes)
        {
            _tasks.Remove(task);
            RefreshGrid();
            UpdateTaskDetail();
        }
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        _refreshTimer?.Stop();
        _refreshTimer?.Dispose();
        base.OnHandleDestroyed(e);
    }
}

public class TaskItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public TaskStatus Status { get; set; }
    public int Progress { get; set; }
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; }
}
