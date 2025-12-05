<div align="center">

<img src="https://github.com/Lean365/Takt.Wpf/raw/master/src/Takt.Fluent/Assets/takt.png" width="64" alt="Takt Logo">

# Takt SMEs Platform

基于 WPF 开发的企业级中后台管理系统，采用分层架构设计，支持多语言、多主题、RBAC 权限管理等核心功能。

> ⚠️ **重要说明**: 本项目使用 Cursor AI 辅助开发完成，**不接受任何 Issues 提交**。

</div>

## 技术栈

- **.NET 9.0** + **WPF**
- **UI**: MaterialDesignThemes, FontAwesome.Sharp
- **架构**: Clean Architecture + MVVM
- **依赖注入**: Autofac + Microsoft.Extensions.DependencyInjection
- **ORM**: SqlSugar
- **日志**: Serilog
- **模板引擎**: Scriban

## 快速开始

### 环境要求

- Windows 10/11
- .NET 9.0 SDK
- SQL Server 2019+

### 安装步骤

```bash
# 克隆项目
git clone https://github.com/Lean365/Takt.Wpf.git
cd Takt.Wpf

# 配置数据库（编辑 appsettings.json）
# 修改 ConnectionStrings.DefaultConnection

# 构建项目
dotnet build

# 运行
cd src/Takt.Fluent
dotnet run
```

### 构建安装包

```bash
# 方式一：批处理脚本
.\scripts\build-installer.bat

# 方式二：PowerShell
.\scripts\build-installer.ps1 -Configuration Release

# 方式三：dotnet CLI
dotnet publish src/Takt.Fluent/Takt.Fluent.csproj `
    --configuration Release `
    --output ./publish `
    --runtime win-x64 `
    -p:WindowsPackageType=MSIX
```

输出文件位于 `publish/` 目录。

## 配置说明

编辑 `src/Takt.Fluent/appsettings.json`：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Takt_Wpf_Dev;User Id=sa;Password=YourPassword;TrustServerCertificate=true;"
  },
  "DatabaseSettings": {
    "EnableCodeFirst": false,
    "EnableSeedData": false
  }
}
```

## 功能模块

### 身份认证 (Identity)
- 用户管理：CRUD、密码管理、状态管理
- 角色管理：角色配置、权限分配
- 菜单管理：动态菜单树、权限控制
- RBAC 权限模型

### 基础模块 (Routine)
- 多语言管理：中文、英文、日文
- 翻译管理：翻译键值对
- 字典管理：系统字典
- 系统设置：应用配置

### 后勤模块 (Logistics)
- 物料管理：产品物料、型号
- 序列号管理：入库/出库记录
- 访客管理：访客信息管理

### 日志模块 (Logging)
- 登录日志、操作日志、差异日志
- 自动清理：每月1号0点执行，保留最近7天

### 代码生成 (Generator)
- 表配置管理：从数据库导入表结构
- 代码生成：基于模板自动生成 Entity、DTO、Service、ViewModel、View
- 支持 CRUD、MasterDetail、Tree 模板类型

## 项目结构

```
Takt.Wpf/
├── src/
│   ├── Takt.Fluent/         # 表现层（WPF UI）
│   ├── Takt.Application/    # 应用层（业务逻辑）
│   ├── Takt.Domain/         # 领域层（领域模型）
│   ├── Takt.Infrastructure/ # 基础设施层（数据访问）
│   └── Takt.Common/         # 通用层（共享组件）
├── docs/                    # 文档
└── scripts/                 # 构建脚本
```

## 开发规范

### 命名规范

- **类名**: 以 `Takt` 开头，PascalCase
- **接口**: 以 `ITakt` 开头，PascalCase
- **异步方法**: 以 `Async` 结尾
- **变量**: camelCase

### 架构原则

- 分层架构：Fluent → Application → Domain → Infrastructure → Common
- 依赖方向只能向下
- MVVM 模式：View 只负责 UI，ViewModel 处理逻辑
- 依赖注入：通过构造函数注入

## 多语言使用

### XAML 中使用

```xml
<TextBlock Text="{local:Loc Key=Identity.User.Title}"/>
<Button Content="{local:Loc Key=Button.Save}"/>
```

### C# 中使用

```csharp
var title = _localizationManager.GetString("Identity.User.Title");
```

翻译数据存储在数据库 `takt_routine_translation` 表中。

## 路径管理

使用 `PathHelper` 统一管理路径：

- **日志**: `AppData\Local\Takt\Takt SMEs\Logs`
- **配置**: `AppData\Roaming\Takt\Takt SMEs`
- **模板**: `AppData\Roaming\Takt\Takt SMEs\Templates`

## 数据库

### 主要表

- **Identity**: `takt_oidc_user`, `takt_oidc_role`, `takt_oidc_menu`
- **Routine**: `takt_routine_language`, `takt_routine_translation`, `takt_routine_setting`
- **Logging**: `takt_logging_login_log`, `takt_logging_operation_log`
- **Logistics**: `takt_logistics_prod_material`, `takt_logistics_prod_serial_inbound`

### 实体规范

- 表名: `takt_模块名_实体名`
- 主键: `id` (bigint, 雪花ID)
- 审计字段: `created_by`, `created_time`, `updated_by`, `updated_time`, `is_deleted`

## 常见问题

**数据库连接失败**
- 检查 `appsettings.json` 连接字符串
- 确认 SQL Server 服务已启动

**菜单不显示**
- 检查数据库 `takt_oidc_menu` 表
- 确认用户角色权限配置

**翻译不生效**
- 检查 `takt_routine_translation` 表
- 确认语言代码正确（zh-CN, en-US, ja-JP）

## 版本信息

- **当前版本**: 0.0.2
- **.NET 版本**: 9.0
- **最后更新**: 2025-12-05

## 许可证

MIT License

**免责声明**: 此软件使用 MIT License，作者不承担任何使用风险。

## 相关链接

- **项目地址**: https://github.com/Lean365/Takt.Wpf
- **安装包构建**: [docs/INSTALLER.md](./docs/INSTALLER.md)
- **架构规范**: [.cursor/rules/architecture.mdc](.cursor/rules/architecture.mdc)

## 更新日志

### v0.0.2 (2025-12-05)
- 新增代码生成模块
- 实现日志自动清理
- 路径管理规范化
- 优化代码生成表单

### v0.0.1 (2025-11-03)
- 初始版本发布
- 完成身份认证、基础、后勤、日志模块
- 完成多语言、多主题支持
