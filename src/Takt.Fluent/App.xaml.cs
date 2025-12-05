// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent
// 文件名称：App.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：WPF 应用程序入口，配置依赖注入和启动流程
//
// 版权信息：Copyright (c) 2025 Takt  All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险.
// ========================================

using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.IO;
using System.Diagnostics;
using Takt.Common.Config;
using Takt.Common.Logging;
using Takt.Domain.Interfaces;
using Takt.Fluent.Services;
using Takt.Fluent.ViewModels;
using Takt.Fluent.ViewModels.Identity;
using Takt.Fluent.ViewModels.Logging;
using Takt.Fluent.ViewModels.Routine;
using Takt.Fluent.ViewModels.Settings;
using Takt.Fluent.ViewModels.Generator;
using Takt.Fluent.Views;
using Takt.Fluent.Views.About;
using Takt.Fluent.Views.Dashboard;
using Takt.Fluent.Views.Identity;
using Takt.Fluent.Views.Identity.MenuComponent;
using Takt.Fluent.Views.Identity.RoleComponent;
using Takt.Fluent.Views.Identity.UserComponent;
using Takt.Fluent.Views.Logging;
using Takt.Fluent.Views.Routine;
using Takt.Fluent.Views.Routine.SettingComponent;
using Takt.Fluent.Views.Settings;
using Takt.Fluent.ViewModels.Logistics.Materials;
using Takt.Fluent.ViewModels.Logistics.Serials;
using Takt.Fluent.ViewModels.Logistics.Visitors;
using Takt.Fluent.Views.Logistics.Materials;
using Takt.Fluent.Views.Logistics.Serials;
using Takt.Fluent.Views.Logistics.Visitors;
using Takt.Fluent.Views.Generator;
using Takt.Fluent.Views.Generator.CodeGenComponent;
using Takt.Infrastructure.DependencyInjection;
using System.Windows.Controls;
using System.Windows.Automation;
using System.Windows.Data;
using Takt.Fluent.Controls;

namespace Takt.Fluent;

/// <summary>
/// WPF 应用程序入口
/// </summary>
public partial class App : System.Windows.Application
{
    private IHost? _host;

    /// <summary>
    /// 服务提供者（用于全局访问依赖注入容器）
    /// </summary>
    public static IServiceProvider? Services { get; private set; }

    /// <summary>
    /// 启动日志管理器（在 Services 初始化之前使用，使用 InitLogManager 记录初始化过程）
    /// </summary>
    public static InitLogManager? StartupLogManager { get; private set; }

    /// <summary>
    /// 初始化启动日志管理器（在 Services 初始化之前使用）
    /// </summary>
    public static void InitializeStartupLogger()
    {
        if (StartupLogManager != null) return;

        // 创建一个临时的 Serilog Logger 用于 InitLogManager
        var tempLogger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .CreateLogger();

        // 使用 InitLogManager 记录启动日志（专门用于初始化过程）
        StartupLogManager = new InitLogManager(tempLogger);
    }

    /// <summary>
    /// 应用程序构造函数
    /// </summary>
    public App()
    {
        InitializeStartupLogger();
        StartupLogManager?.Information("App 构造函数被调用");

        // 手动加载 App.xaml 中的资源字典
        // 当 EnableDefaultApplicationDefinition=false 时，需要手动加载
        try
        {
            StartupLogManager?.Information("准备加载 App.xaml 资源字典");
            LoadAppXamlResources();
            StartupLogManager?.Information("App.xaml 资源字典加载成功");
        }
        catch (Exception ex)
        {
            StartupLogManager?.Error(ex, "App.xaml 资源字典加载失败");
            throw;
        }
    }

    /// <summary>
    /// 手动加载 App.xaml 中的资源字典
    /// </summary>
    private void LoadAppXamlResources()
    {
        // 初始化 Resources（如果为 null）
        if (this.Resources == null)
        {
            this.Resources = new ResourceDictionary();
        }

        var mainDict = this.Resources as ResourceDictionary;
        if (mainDict == null)
        {
            this.Resources = new ResourceDictionary();
            mainDict = this.Resources as ResourceDictionary;
        }

        StartupLogManager?.Information("准备从资源流加载 App.xaml");

        // 从资源流中加载 App.xaml
        var resourceStream = System.Reflection.Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Takt.Fluent.App.xaml");

        if (resourceStream == null)
        {
            // 如果资源流不存在，尝试使用 Application.LoadComponent
            StartupLogManager?.Warning("无法从资源流加载 App.xaml，尝试使用 Application.LoadComponent");
            try
            {
                var uri = new Uri("/Takt.Fluent;component/App.xaml", UriKind.Relative);
                System.Windows.Application.LoadComponent(this, uri);
                StartupLogManager?.Information("使用 Application.LoadComponent 加载成功");
            }
            catch (Exception ex)
            {
                StartupLogManager?.Error(ex, "Application.LoadComponent 加载失败");
                throw;
            }
        }
        else
        {
            // 从资源流加载 XAML
            using (var reader = new System.IO.StreamReader(resourceStream))
            {
                var xaml = reader.ReadToEnd();
                var appObject = System.Windows.Markup.XamlReader.Parse(xaml);
                StartupLogManager?.Information("从资源流加载成功，对象类型: {Type}", appObject?.GetType().Name ?? "null");
            }
        }

        // 验证资源字典是否已加载
        var mergedCount = mainDict?.MergedDictionaries?.Count ?? 0;
        StartupLogManager?.Information("资源字典加载完成，合并的资源字典数量: {Count}", mergedCount);

        // 如果资源字典仍然为空，手动加载 App.xaml 中定义的资源
        if (mergedCount == 0 && mainDict != null)
        {
            StartupLogManager?.Warning("合并的资源字典数量为 0，手动加载 App.xaml 中定义的资源字典");
            LoadAppXamlResourcesManually(mainDict);
        }
    }

    /// <summary>
    /// 手动加载 App.xaml 中定义的资源字典
    /// </summary>
    private void LoadAppXamlResourcesManually(ResourceDictionary mainDict)
    {
        // 根据 App.xaml 中的定义，手动添加资源字典
        StartupLogManager?.Information("开始手动加载 Material Design 资源字典");

        // 1. BundledTheme
        try
        {
            var bundledTheme = new MaterialDesignThemes.Wpf.BundledTheme
            {
                BaseTheme = MaterialDesignThemes.Wpf.BaseTheme.Light,
                PrimaryColor = MaterialDesignColors.PrimaryColor.Teal,
                SecondaryColor = MaterialDesignColors.SecondaryColor.Cyan
            };
            mainDict.MergedDictionaries.Add(bundledTheme);
            StartupLogManager?.Information("BundledTheme 已添加");
        }
        catch (Exception ex)
        {
            StartupLogManager?.Error(ex, "添加 BundledTheme 失败");
        }

        // 2. MaterialDesign3.Defaults.xaml
        try
        {
            var materialDesign3Defaults = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesign3.Defaults.xaml")
            };
            mainDict.MergedDictionaries.Add(materialDesign3Defaults);
            StartupLogManager?.Information("MaterialDesign3.Defaults.xaml 已添加");
        }
        catch (Exception ex)
        {
            StartupLogManager?.Error(ex, "添加 MaterialDesign3.Defaults.xaml 失败: {Message}", ex.Message);
        }

        // 3. 项目自定义资源字典
        var customResources = new[]
        {
            "pack://application:,,,/Takt.Fluent;component/Resources/ButtonColors.xaml",
            "pack://application:,,,/Takt.Fluent;component/Resources/ButtonDefaultStyles.xaml",
            "pack://application:,,,/Takt.Fluent;component/Resources/ButtonDefaultNoStyles.xaml",
            "pack://application:,,,/Takt.Fluent;component/Resources/ButtonDefaultPlainStyles.xaml",
            "pack://application:,,,/Takt.Fluent;component/Resources/ButtonDefaultIconStyles.xaml",
            "pack://application:,,,/Takt.Fluent;component/Resources/ButtonCircleStyles.xaml"
        };

        foreach (var resourceUri in customResources)
        {
            try
            {
                var resourceDict = new ResourceDictionary
                {
                    Source = new Uri(resourceUri)
                };
                mainDict.MergedDictionaries.Add(resourceDict);
                StartupLogManager?.Information("已添加自定义资源字典: {Uri}", resourceUri);
            }
            catch (Exception ex)
            {
                StartupLogManager?.Warning("添加自定义资源字典失败: {Uri}, 错误: {Error}", resourceUri, ex.Message);
            }
        }

        StartupLogManager?.Information("手动加载完成，合并的资源字典数量: {Count}", mainDict.MergedDictionaries.Count);
    }

    /// <summary>
    /// 手动触发启动事件（用于 EnableDefaultApplicationDefinition=false 的情况）
    /// </summary>
    public void StartApplication()
    {
        StartupLogManager?.Information("StartApplication 方法被调用");

        // StartupEventArgs 的构造函数是内部的，需要通过反射获取正确的构造函数
        // 尝试不同的构造函数签名
        StartupEventArgs? args = null;
        var argsArray = Environment.GetCommandLineArgs();

        try
        {
            // 尝试使用 string[] 参数的构造函数
            var constructor = typeof(StartupEventArgs).GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                new[] { typeof(string[]) },
                null);

            if (constructor != null)
            {
                args = (StartupEventArgs)constructor.Invoke(new object[] { argsArray });
            }
        }
        catch (Exception ex)
        {
            StartupLogManager?.Warning("无法通过反射创建 StartupEventArgs: {0}", ex.Message);
        }

        // 如果反射失败，尝试使用无参构造函数或直接调用 OnStartup
        if (args == null)
        {
            try
            {
                // 尝试无参构造函数
                args = (StartupEventArgs)Activator.CreateInstance(
                    typeof(StartupEventArgs),
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                    null,
                    Array.Empty<object>(),
                    null)!;
            }
            catch
            {
                // 如果都失败，创建一个包装类
                StartupLogManager?.Warning("无法创建 StartupEventArgs，将使用默认方式");
                // 直接调用 OnStartup，传入 null 或创建一个简单的包装
                // 实际上，app.Run() 可能会自动触发 OnStartup，所以这里可能不需要手动调用
                return;
            }
        }

        if (args != null)
        {
            OnStartup(args);
        }
    }

    /// <summary>
    /// 应用程序启动
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        StartupLogManager?.Information("OnStartup 方法被调用");

        // 先调用基类方法，加载 App.xaml 中的资源字典
        // 如果资源字典加载失败，这里会抛出异常
        try
        {
            StartupLogManager?.Information("准备调用 base.OnStartup(e) 加载资源字典");
            base.OnStartup(e);
            StartupLogManager?.Information("base.OnStartup(e) 执行成功");

            // 验证资源字典是否真的加载成功
            VerifyResourceDictionaryLoaded();
        }
        catch (Exception ex)
        {
            StartupLogManager?.Error(ex, "base.OnStartup(e) 执行失败，资源字典加载失败");
            MessageBox.Show(
                $"资源字典加载失败：{ex.Message}\n\n详细信息：{ex}",
                "启动错误",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            this.Shutdown();
            return;
        }

        // 资源字典加载成功后，进行异步初始化
        StartupLogManager?.Information("资源字典验证完成，开始异步初始化");
        _ = InitializeApplicationAsync();
    }

    /// <summary>
    /// 验证资源字典是否真的加载成功
    /// </summary>
    private void VerifyResourceDictionaryLoaded()
    {
        StartupLogManager?.Information("开始验证资源字典加载情况");

        // 检查 Application.Resources 是否存在
        if (this.Resources == null)
        {
            StartupLogManager?.Error("Application.Resources 为 null");
            throw new InvalidOperationException("Application.Resources 未初始化");
        }

        var mainDict = this.Resources as ResourceDictionary;
        var mergedCount = mainDict?.MergedDictionaries?.Count ?? 0;
        StartupLogManager?.Information("Application.Resources 存在，合并的资源字典数量: {Count}", mergedCount);

        // 验证 Material Design 资源（使用 Application.Current.Resources.TryFindResource）
        var materialDesignPaper = this.TryFindResource("MaterialDesignPaper");
        if (materialDesignPaper != null)
        {
            StartupLogManager?.Information("Material Design 资源已加载（找到 MaterialDesignPaper）");
        }
        else
        {
            StartupLogManager?.Warning("Material Design 资源未找到（MaterialDesignPaper 不存在）");
        }

        // 验证 MaterialDesignIconButton 样式
        var iconButtonStyle = this.TryFindResource("MaterialDesignIconButton");
        if (iconButtonStyle != null)
        {
            StartupLogManager?.Information("MaterialDesignIconButton 样式已找到，类型: {Type}", iconButtonStyle.GetType().Name);
        }
        else
        {
            StartupLogManager?.Error("MaterialDesignIconButton 样式未找到");

            // 尝试直接访问资源字典
            if (mainDict != null && mainDict.Contains("MaterialDesignIconButton"))
            {
                StartupLogManager?.Information("MaterialDesignIconButton 在资源字典中直接存在");
            }
            else
            {
                StartupLogManager?.Error("MaterialDesignIconButton 在资源字典中也不存在");
            }
        }

        // 列出所有合并的资源字典
        if (mainDict?.MergedDictionaries != null)
        {
            StartupLogManager?.Information("合并的资源字典列表（共 {Count} 个）：", mergedCount);
            for (int i = 0; i < mainDict.MergedDictionaries.Count; i++)
            {
                var dict = mainDict.MergedDictionaries[i];
                if (dict is ResourceDictionary rd)
                {
                    var source = rd.Source?.ToString() ?? "无 Source";
                    StartupLogManager?.Information("  [{Index}] {Source}", i, source);
                }
                else
                {
                    StartupLogManager?.Information("  [{Index}] {Type}", i, dict?.GetType().Name ?? "未知类型");
                }
            }
        }
    }

    /// <summary>
    /// 异步初始化应用程序
    /// </summary>
    private async Task InitializeApplicationAsync()
    {
        try
        {
            StartupLogManager?.Information("开始构建 Host");
            // 构建 Host
            _host = CreateHostBuilder(Environment.GetCommandLineArgs()).Build();

            // 设置全局服务提供者
            Services = _host.Services;
            StartupLogManager?.Information("Host 构建成功，Services 已初始化");

            // 初始化数据库和种子数据
            await InitializeApplicationDataAsync();

            // 显示登录窗口
            StartupLogManager?.Information("准备显示登录窗口");
            var loginWindow = Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();

            // 设置主窗口（登录窗口）
            this.Dispatcher.Invoke(() =>
            {
                this.MainWindow = loginWindow;
            });

            StartupLogManager?.Information("应用程序启动完成，登录窗口已显示");

            StartupLogManager?.Information("应用程序启动完成，登录窗口已显示");
        }
        catch (Exception ex)
        {
            StartupLogManager?.Error(ex, "应用程序启动失败");
            this.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    $"应用程序启动失败：{ex.Message}\n\n详细信息：{ex}",
                    "启动错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                this.Shutdown();
            });
        }
    }

    /// <summary>
    /// 初始化应用程序数据（数据库初始化、种子数据）
    /// </summary>
    private async Task InitializeApplicationDataAsync()
    {
        if (Services == null)
        {
            throw new InvalidOperationException("Services 未初始化");
        }

        try
        {
            var operLog = Services.GetService<OperLogManager>();
            operLog?.Information("开始初始化应用程序...");

            // 获取数据库配置
            var databaseSettings = Services.GetRequiredService<IConfiguration>()
                .GetSection("DatabaseSettings").Get<HbtDatabaseSettings>() ?? new HbtDatabaseSettings();

            // 如果 CodeFirst 和 SeedData 都禁用，检查数据库是否已初始化
            if (!databaseSettings.EnableCodeFirst && !databaseSettings.EnableSeedData)
            {
                WriteDiagnosticLog("🟣 [App.xaml.cs] CodeFirst 和 SeedData 都已禁用，检查数据库是否已初始化");
                System.Diagnostics.Debug.WriteLine("🟣 [App.xaml.cs] CodeFirst 和 SeedData 都已禁用，检查数据库是否已初始化");

                var dbContext = Services.GetRequiredService<Takt.Infrastructure.Data.DbContext>();
                
                // 检查数据库连接
                var isConnected = await dbContext.CheckConnectionAsync();
                if (!isConnected)
                {
                    // 数据库连接失败，显示提示消息框
                    this.Dispatcher.Invoke(() =>
                    {
                        TaktMessageBox.Error(
                            "数据库连接失败，无法启动系统。\n\n" +
                            "请检查：\n" +
                            "1. 数据库服务器是否正常运行\n" +
                            "2. 连接字符串配置是否正确（appsettings.json）\n" +
                            "3. 数据库是否已创建\n\n" +
                            "如需自动初始化数据库，请在 appsettings.json 中设置：\n" +
                            "- \"EnableCodeFirst\": true（自动创建表结构）\n" +
                            "- \"EnableSeedData\": true（自动初始化种子数据）",
                            "数据库初始化提示",
                            null); // 显式传入 null，避免使用 MainWindow（启动早期可能为 null）
                        this.Shutdown();
                    });
                    return;
                }

                // 检查关键表是否存在（使用用户表作为检查标准）
                var db = dbContext.Db;
                var userTableExists = db.DbMaintenance.IsAnyTable("takt_oidc_user");
                var menuTableExists = db.DbMaintenance.IsAnyTable("takt_oidc_menu");

                if (!userTableExists || !menuTableExists)
                {
                    // 数据库表不存在，显示提示消息框
                    this.Dispatcher.Invoke(() =>
                    {
                        TaktMessageBox.Error(
                            "数据库未初始化，无法启动系统。\n\n" +
                            "检测到数据库表不存在，系统需要初始化数据库。\n\n" +
                            "请在 appsettings.json 中设置以下配置之一：\n\n" +
                            "方案一：自动创建表结构\n" +
                            "  \"EnableCodeFirst\": true\n\n" +
                            "方案二：自动初始化完整数据\n" +
                            "  \"EnableCodeFirst\": true\n" +
                            "  \"EnableSeedData\": true\n\n" +
                            "配置完成后，请重新启动应用程序。",
                            "数据库初始化提示",
                            null); // 显式传入 null，避免使用 MainWindow（启动早期可能为 null）
                        this.Shutdown();
                    });
                    return;
                }

                WriteDiagnosticLog("🟣 [App.xaml.cs] 数据库检查通过，表已存在");
                System.Diagnostics.Debug.WriteLine("🟣 [App.xaml.cs] 数据库检查通过，表已存在");
            }

            // 初始化数据库表
            System.Diagnostics.Debug.WriteLine("🟣 [App.xaml.cs] 准备解析 DbTableInitializer");
            WriteDiagnosticLog("🟣 [App.xaml.cs] 准备解析 DbTableInitializer");
            
            var dbTableInitializer = Services.GetRequiredService<Takt.Infrastructure.Data.DbTableInitializer>();
            
            System.Diagnostics.Debug.WriteLine("🟣 [App.xaml.cs] DbTableInitializer 解析成功");
            WriteDiagnosticLog("🟣 [App.xaml.cs] DbTableInitializer 解析成功");
            
            await dbTableInitializer.InitializeAsync();

            // 初始化种子数据（如果启用）
            if (databaseSettings.EnableSeedData)
            {
                // 临时禁用差异日志，避免启动时连接冲突
                // 种子数据初始化不应该记录差异日志
                WriteDiagnosticLog("🟣 [App.xaml.cs] 准备禁用差异日志（种子数据初始化前）");
                System.Diagnostics.Debug.WriteLine("🟣 [App.xaml.cs] 准备禁用差异日志（种子数据初始化前）");
                Takt.Infrastructure.Data.SqlSugarAop.SetDiffLogEnabled(false);
                
                try
                {
                    // 使用协调器统一执行所有种子数据初始化
                    WriteDiagnosticLog("🟣 [App.xaml.cs] 开始执行种子数据初始化");
                    System.Diagnostics.Debug.WriteLine("🟣 [App.xaml.cs] 开始执行种子数据初始化");
                    var dbSeedCoordinator = Services.GetRequiredService<Takt.Infrastructure.Data.DbSeedCoordinator>();
                    await dbSeedCoordinator.InitializeAsync();
                    WriteDiagnosticLog("🟣 [App.xaml.cs] 种子数据初始化完成");
                    System.Diagnostics.Debug.WriteLine("🟣 [App.xaml.cs] 种子数据初始化完成");
                }
                finally
                {
                    // 种子数据初始化完成后，重新启用差异日志
                    WriteDiagnosticLog("🟣 [App.xaml.cs] 准备启用差异日志（种子数据初始化后）");
                    System.Diagnostics.Debug.WriteLine("🟣 [App.xaml.cs] 准备启用差异日志（种子数据初始化后）");
                    Takt.Infrastructure.Data.SqlSugarAop.SetDiffLogEnabled(true);
                    WriteDiagnosticLog("🟣 [App.xaml.cs] 差异日志已启用");
                    System.Diagnostics.Debug.WriteLine("🟣 [App.xaml.cs] 差异日志已启用");
                }
            }
            else
            {
                // 如果种子数据未启用，确保差异日志是启用的
                WriteDiagnosticLog("🟣 [App.xaml.cs] 种子数据未启用，确保差异日志已启用");
                System.Diagnostics.Debug.WriteLine("🟣 [App.xaml.cs] 种子数据未启用，确保差异日志已启用");
                Takt.Infrastructure.Data.SqlSugarAop.SetDiffLogEnabled(true);
            }

            // 初始化主题服务
            var themeService = Services.GetRequiredService<ThemeService>();
            themeService.InitializeTheme();

            // 初始化本地化（预加载翻译）
            var localizationManager = Services.GetRequiredService<ILocalizationManager>();
            // 先初始化 LocalizationManager（加载语言列表和默认语言翻译）
            await localizationManager.InitializeAsync();
            
            // 然后切换到保存的语言（如果与默认语言不同）
            var savedLang = Takt.Common.Helpers.AppSettingsHelper.GetLanguage();
            if (!string.IsNullOrWhiteSpace(savedLang) && savedLang != localizationManager.CurrentLanguage)
            {
                localizationManager.ChangeLanguage(savedLang);
            }

            // 初始化并启动 Quartz 调度器，从数据库加载任务
            try
            {
                StartupLogManager?.Information("开始初始化 Quartz 调度器...");
                var quartzSchedulerManager = Services.GetRequiredService<Takt.Domain.Interfaces.IQuartzSchedulerManager>();
                await quartzSchedulerManager.InitializeAsync();
                await quartzSchedulerManager.StartAsync();
                StartupLogManager?.Information("Quartz 调度器初始化完成");
                
                StartupLogManager?.Information("开始从数据库加载 Quartz 任务...");
                await quartzSchedulerManager.LoadJobsFromDatabaseAsync();
                StartupLogManager?.Information("从数据库加载 Quartz 任务完成");
            }
            catch (Exception ex)
            {
                StartupLogManager?.Error(ex, "初始化或加载 Quartz 调度器失败，但应用将继续启动");
                operLog?.Error(ex, "初始化或加载 Quartz 调度器失败");
            }

            operLog?.Information("应用程序初始化完成");
        }
        catch (Exception ex)
        {
            var operLog = Services.GetService<OperLogManager>();
            operLog?.Error(ex, "应用程序初始化失败");
            throw;
        }
    }

    /// <summary>
    /// 创建 Host 构建器
    /// </summary>
    private IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(context, services);
            })
            .ConfigureContainer<ContainerBuilder>((context, builder) =>
            {
                ConfigureAutofacContainer(context, builder);
            })
            .UseSerilog((context, configuration) =>
            {
                // 使用符合 Windows 规范的日志目录（AppData\Local）
                var logDirectory = Takt.Common.Helpers.PathHelper.GetLogDirectory();
                var logFilePath = Path.Combine(logDirectory, "app-.txt");

                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File(
                        path: logFilePath,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        fileSizeLimitBytes: 8 * 1024 * 1024,  // 单个文件最大 8MB
                        rollOnFileSizeLimit: true,  // 达到文件大小限制时自动滚动
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                        encoding: System.Text.Encoding.UTF8);
            });
    }

    /// <summary>
    /// 配置服务（表现层服务、View、ViewModel）
    /// </summary>
    private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // ========== 1. 全局服务（Singleton）==========
        services.AddSingleton<ThemeService>();

        services.AddSingleton<LocalizationAdapter>(sp =>
        {
            var localizationManager = sp.GetRequiredService<ILocalizationManager>();
            return new LocalizationAdapter(localizationManager);
        });

        // ========== 2. 主窗口和主窗口 ViewModel（Singleton）==========
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>();

        // ========== 3. 登录窗口和登录 ViewModel ==========
        services.AddSingleton<LoginWindow>();
        services.AddTransient<LoginViewModel>(); // 每次登录可能需要新实例

        // ========== 4. 业务 ViewModel 和 View（Transient）==========
        // Dashboard
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<DashboardView>();

        // Identity 模块
        services.AddTransient<Views.Identity.IdentityPage>();
        services.AddTransient<UserViewModel>();
        services.AddTransient<UserView>();
        services.AddTransient<UserFormViewModel>();
        services.AddTransient<UserForm>();
        services.AddTransient<UserProfileViewModel>();
        services.AddTransient<UserProfile>();
        services.AddTransient<UserAssignRoleViewModel>();
        services.AddTransient<UserAssignRole>();
        services.AddTransient<RoleViewModel>();
        services.AddTransient<RoleView>();
        services.AddTransient<RoleFormViewModel>();
        services.AddTransient<RoleForm>();
        services.AddTransient<RoleAssignMenuViewModel>();
        services.AddTransient<RoleAssignMenu>();
        services.AddTransient<MenuViewModel>();
        services.AddTransient<MenuView>();
        services.AddTransient<MenuFormViewModel>();
        services.AddTransient<MenuForm>();

        // Routine 模块
        services.AddTransient<Views.Routine.RoutinePage>();
        services.AddTransient<LocalizationViewModel>();
        services.AddTransient<LocalizationView>();
        services.AddTransient<DictionaryViewModel>();
        services.AddTransient<DictionaryView>();
        services.AddTransient<DictionaryFormViewModel>();
        services.AddTransient<Views.Routine.DictionaryComponent.DictionaryForm>();
        services.AddTransient<SettingViewModel>();
        services.AddTransient<SettingView>();
        services.AddTransient<SettingFormViewModel>();
        services.AddTransient<SettingForm>();
           services.AddTransient<QuartzJobViewModel>();
           services.AddTransient<QuartzJobView>();
           services.AddTransient<QuartzJobFormViewModel>();
           services.AddTransient<Views.Routine.QuartzJobComponent.QuartzJobForm>();

        // Logging 模块
        services.AddTransient<Views.Logging.LoggingPage>();
        services.AddTransient<OperLogViewModel>();
        services.AddTransient<OperLogView>();
        services.AddTransient<LoginLogViewModel>();
        services.AddTransient<LoginLogView>();
        services.AddTransient<DiffLogViewModel>();
        services.AddTransient<DiffLogView>();
        services.AddTransient<QuartzJobLogViewModel>();
        services.AddTransient<QuartzJobLogView>();

        // Logistics 模块
        services.AddTransient<Views.Logistics.LogisticsPage>();
        services.AddTransient<Views.Logistics.Materials.MaterialsPage>();
        services.AddTransient<MaterialViewModel>();
        services.AddTransient<MaterialView>();
        services.AddTransient<PackingViewModel>();
        services.AddTransient<Views.Logistics.Materials.PackingView>();
        services.AddTransient<ModelViewModel>();
        services.AddTransient<ModelView>();
        services.AddTransient<Views.Logistics.Serials.SerialsPage>();
        services.AddTransient<SerialInboundViewModel>();
        services.AddTransient<SerialInboundView>();
        services.AddTransient<SerialInboundFormViewModel>();
        services.AddTransient<Views.Logistics.Serials.SerialComponent.SerialInboundForm>();
        services.AddTransient<SerialOutboundViewModel>();
        services.AddTransient<SerialOutboundView>();
        services.AddTransient<SerialOutboundFormViewModel>();
        services.AddTransient<Views.Logistics.Serials.SerialComponent.SerialOutboundForm>();
        services.AddTransient<SerialScanningViewModel>();
        services.AddTransient<SerialScanningView>();
        services.AddTransient<Views.Logistics.Visitors.VisitorsPage>();
        services.AddTransient<VisitorViewModel>();
        services.AddTransient<VisitorView>();
        services.AddTransient<VisitorFormViewModel>();
        services.AddTransient<Views.Logistics.Visitors.VisitorComponent.VisitorForm>();
        services.AddTransient<DigitalSignageViewModel>();
        services.AddTransient<DigitalSignageView>();

        // Settings 模块
        services.AddTransient<MySettingsViewModel>();
        services.AddTransient<MySettingsView>();

        // About 模块
        services.AddTransient<AboutView>();
        services.AddTransient<MySystemView>();

        // Generator 模块
        services.AddTransient<Views.Generator.GeneratorPage>();
        services.AddTransient<CodeGeneratorViewModel>();
        services.AddTransient<CodeGeneratorView>();
        services.AddTransient<CodeGenFormViewModel>();
        services.AddTransient<CodeGenForm>();
        services.AddTransient<ImportTableViewModel>();
        services.AddTransient<ImportTableView>();
    }

    /// <summary>
    /// 配置 Autofac 容器（应用层服务、基础设施层服务）
    /// </summary>
    private void ConfigureAutofacContainer(HostBuilderContext context, ContainerBuilder builder)
    {
        // 获取配置
        var configuration = context.Configuration;
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("未找到数据库连接字符串 'DefaultConnection'");

        var databaseSettings = configuration.GetSection("DatabaseSettings").Get<HbtDatabaseSettings>()
            ?? new HbtDatabaseSettings();

        // 注册 Autofac 模块
        builder.RegisterModule(new AutofacModule(connectionString, databaseSettings));
    }


    /// <summary>
    /// 应用程序关闭
    /// </summary>
    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }
    
    /// <summary>
    /// 写入诊断日志到文件
    /// </summary>
    private static void WriteDiagnosticLog(string message)
    {
        try
        {
            var logDir = Takt.Common.Helpers.PathHelper.GetLogDirectory();
            var logFile = Path.Combine(logDir, "diagnostic.log");
            var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}\r\n";
            File.AppendAllText(logFile, logMessage);
        }
        catch
        {
            // 忽略文件写入错误
        }
    }
}

/// <summary>
/// 应用程序入口点
/// </summary>
/// <remarks>
/// 注意：此应用程序允许多个实例同时运行。
/// 每个实例都有独立的依赖注入容器和数据库连接，不会相互干扰。
/// </remarks>
public class Program
{
    /// <summary>
    /// 应用程序主入口点
    /// </summary>
    /// <remarks>
    /// 允许多个实例同时运行，没有单实例限制（Mutex）。
    /// 每个实例独立运行，共享同一个数据库服务器。
    /// </remarks>
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            // 初始化启动日志（在 App 创建之前）
            App.InitializeStartupLogger();
            App.StartupLogManager?.Information("========== 应用程序启动开始 ==========");
            App.StartupLogManager?.Information("Program.Main 开始执行");
            App.StartupLogManager?.Information("应用程序允许多个实例同时运行");

            App.StartupLogManager?.Information("准备创建 App 实例");
            var app = new App();
            App.StartupLogManager?.Information("App 实例创建成功");

            // 启动消息循环（app.Run() 会自动触发 OnStartup 事件）
            App.StartupLogManager?.Information("准备调用 app.Run() 启动消息循环");
            app.Run();
            App.StartupLogManager?.Information("app.Run() 执行完成，应用程序退出");
        }
        catch (Exception ex)
        {
            App.StartupLogManager?.Error(ex, "Program.Main 捕获异常");

            // 尝试显示错误消息框
            try
            {
                MessageBox.Show(
                    $"应用程序启动失败：{ex.Message}\n\n详细信息：{ex}",
                    "启动错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch
            {
                // 如果 MessageBox 也失败，至少输出到控制台和日志
                Console.WriteLine($"应用程序启动失败：{ex}");
                App.StartupLogManager?.Error(ex, "MessageBox 显示失败，已输出到控制台");
            }
        }
    }
}

