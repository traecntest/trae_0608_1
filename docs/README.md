# BioCAD - 生物计算与AI辅助药物研发平台

## 项目简介

BioCAD 是一个基于 .NET 8 WinForms 的生物计算与 AI 辅助药物研发平台，集成了数据管理、计算引擎和可视化交互三大核心模块，为药物研发提供一体化解决方案。

## 系统架构

### 三层架构设计

```
┌──────────────────────────────────────────────────────────┐
│              可视化交互层 (Visualization)                 │
│  主窗体 · 数据管理 · 分子查看器 · 任务管理 · 结果分析      │
└──────────────────────────────┬───────────────────────────┘
                               │
┌──────────────────────────────▼───────────────────────────┐
│              计算引擎层 (Engine)                          │
│  任务队列 · 分子对接 · 虚拟筛选 · 药效团 · MD模拟        │
└──────────────────────────────┬───────────────────────────┘
                               │
┌──────────────────────────────▼───────────────────────────┐
│              数据管理层 (Data)                           │
│  SQLite · 数据模型 · 导入导出 · 仓储模式 · 元数据       │
└──────────────────────────────────────────────────────────┘
```

### 项目结构

```
BioCAD/
├── src/
│   ├── BioCAD.Domain/          # 领域模型层
│   │   ├── Entities/            # 实体类
│   │   └── Enums/               # 枚举类型
│   ├── BioCAD.Data/             # 数据访问层
│   │   ├── Repositories/        # 仓储模式实现
│   │   ├── ImportExport/        # 数据导入导出服务
│   │   ├── BioCADDbContext.cs   # 数据库上下文
│   │   └── DataService.cs       # 数据服务入口
│   ├── BioCAD.Engine/           # 计算引擎层
│   │   ├── Modules/             # 计算模块
│   │   ├── ComputationEngine.cs # 引擎入口
│   │   └── TaskQueueManager.cs  # 任务队列管理
│   ├── BioCAD.Visualization/    # 可视化组件层
│   │   └── Controls/            # 可视化控件
│   └── BioCAD.App/              # WinForms 主应用
│       ├── Pages/               # 功能页面
│       ├── MainForm.cs          # 主窗体
│       ├── AppServices.cs       # 应用服务
│       └── Program.cs           # 程序入口
├── database/
│   └── init.sql                 # 数据库初始化脚本
├── sample_data/                 # 示例数据集
│   ├── sample_compounds.sdf     # 示例SDF文件
│   ├── sample_proteins.fasta    # 示例FASTA文件
│   └── compounds.csv            # 示例CSV文件
└── docs/                        # 文档目录
```

## 功能模块

### 1. 数据管理层

#### 支持的数据格式
- **蛋白质序列**: FASTA 格式
- **化合物结构**: SDF、MOL2 格式
- **基因组数据**: 标准序列格式
- **活性数据**: Excel、CSV 格式

#### 核心功能
- 统一数据模型，支持元数据标注
- 版本控制与数据溯源
- SQLite 本地数据库存储
- 多格式导入导出 (Excel、CSV、SDF、FASTA)

### 2. 计算引擎层

#### 支持的计算任务
- **分子对接 (Molecular Docking)**: 蛋白质-配体结合模式预测
- **虚拟筛选 (Virtual Screening)**: 高通量化合物库筛选
- **药效团建模 (Pharmacophore Modeling)**: 基于活性化合物的药效团模型
- **分子动力学模拟 (Molecular Dynamics)**: 体系动力学行为模拟
- **聚类分析 (Clustering)**: 化合物结构/活性聚类
- **QSAR 建模**: 定量构效关系模型

#### 核心功能
- 任务队列管理
- 并行执行支持
- CPU/GPU 资源配置
- 实时任务状态监控
- 任务日志与进度追踪

### 3. 可视化交互层

#### 可视化组件
- **2D 分子结构查看器**: 交互式 2D 结构显示
- **3D 分子结构查看器**: 3D 空间结构可视化、旋转、缩放
- **折线图/ROC曲线**: 数据趋势与模型性能展示
- **热图**: 二维数据热度可视化
- **散点图**: 聚类分析结果展示

#### 功能页面
- 仪表盘: 系统概览与统计
- 数据管理: 化合物、蛋白质、活性数据管理
- 分子查看器: 2D/3D 分子结构浏览
- 任务管理: 计算任务提交与监控
- 结果分析: 多维度结果可视化
- 系统设置: 参数与资源配置

## 环境要求

### 开发环境
- **操作系统**: Windows 10/11
- **开发工具**: Visual Studio 2022 / Rider
- **.NET SDK**: .NET 8.0 或更高版本
- **数据库**: SQLite (内置)

### 运行环境
- **操作系统**: Windows 10/11 或更高版本
- **运行时**: .NET 8 Desktop Runtime
- **内存**: 建议 8GB 以上
- **磁盘空间**: 至少 500MB

## 部署步骤

### 方法一: 使用 Visual Studio 部署

1. **克隆项目**
   ```bash
   git clone <repository-url>
   cd BioCAD
   ```

2. **打开解决方案**
   - 启动 Visual Studio 2022
   - 打开 `BioCAD.slnx` 解决方案文件

3. **还原 NuGet 包**
   - 右键解决方案 → "还原 NuGet 程序包"
   - 或在包管理器控制台执行: `Update-Package -reinstall`

4. **编译项目**
   - 选择 "Release" 配置
   - 生成 → 生成解决方案 (Ctrl+Shift+B)

5. **运行应用**
   - 按 F5 或点击 "启动" 按钮

### 方法二: 使用命令行部署

1. **构建项目**
   ```bash
   dotnet build BioCAD.slnx --configuration Release
   ```

2. **发布应用**
   ```bash
   dotnet publish src/BioCAD.App/BioCAD.App.csproj -c Release -o ./publish
   ```

3. **运行应用**
   ```bash
   cd publish
   ./BioCAD.App.exe
   ```

### 方法三: 发布为单文件

```bash
dotnet publish src/BioCAD.App/BioCAD.App.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  /p:PublishSingleFile=true \
  -o ./publish-single
```

## 快速开始

### 1. 首次运行

应用首次启动时会：
- 自动创建 SQLite 数据库 (`biocad.db`)
- 初始化数据库表结构
- 创建必要的工作目录

### 2. 导入示例数据

1. 进入 "数据管理" 页面
2. 点击 "导入数据" 按钮
3. 选择 `sample_data/` 目录下的示例文件
4. 查看导入结果

### 3. 提交计算任务

1. 进入 "任务管理" 页面
2. 点击 "新建任务" 按钮
3. 选择任务类型并设置参数
4. 提交任务并监控执行状态

### 4. 查看分析结果

1. 进入 "结果分析" 页面
2. 选择要分析的结果
3. 通过多种图表查看分析结果

## 数据库说明

### 数据库文件位置
- 默认位置: 应用程序同级目录下的 `biocad.db`
- 可在 "系统设置" 中修改数据库路径

### 主要数据表

| 表名 | 说明 |
|------|------|
| Compounds | 化合物信息 |
| Atoms | 原子坐标 |
| Bonds | 化学键信息 |
| Proteins | 蛋白质信息 |
| GenomicData | 基因组数据 |
| ActivityData | 活性测定数据 |
| ComputationTasks | 计算任务 |
| TaskLogs | 任务执行日志 |
| MetadataEntries | 元数据 |
| VersionRecords | 版本记录 |
| DataProvenance | 数据溯源 |

### 数据库备份

定期备份 `biocad.db` 文件以确保数据安全。

## 二次开发指南

### 添加新的计算模块

1. 在 `BioCAD.Engine/Modules/` 中创建新模块类
2. 继承 `ComputationModuleBase` 抽象类
3. 实现 `ExecuteAsync` 方法
4. 在 `ComputationEngine` 中注册新模块

### 添加新的可视化控件

1. 在 `BioCAD.Visualization/Controls/` 中创建控件类
2. 继承 `UserControl` 或相应基类
3. 实现自定义绘制逻辑
4. 在 WinForms 页面中使用

### 扩展数据模型

1. 在 `BioCAD.Domain/Entities/` 中添加实体类
2. 继承 `EntityBase` 基类
3. 在 `BioCADDbContext` 中添加表结构
4. 创建对应的仓储类

## 依赖项

### NuGet 包

| 包名 | 用途 |
|------|------|
| Microsoft.Data.Sqlite | SQLite 数据库访问 |
| EPPlus | Excel 文件读写 |
| CsvHelper | CSV 文件处理 |
| Newtonsoft.Json | JSON 序列化 |

## 版本历史

- **v1.0.0** (2024)
  - 初始版本发布
  - 核心三大功能模块
  - 多种可视化组件
  - 任务队列与并行执行

## 技术支持

如有问题或建议，请通过以下方式联系：
- 提交 Issue
- 发送邮件至开发团队

## 许可证

本项目采用 MIT 许可证，详见 LICENSE 文件。
