//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Infrastructure.Data
// 文件名 : DbSeedRoutine.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 版本号 : 0.0.1
// 描述    : Routine 模块种子数据初始化服务
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
//
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
//===================================================================

using Takt.Common.Helpers;
using Takt.Common.Logging;
using Takt.Domain.Entities.Routine;
using Takt.Domain.Repositories;

namespace Takt.Infrastructure.Data;

/// <summary>
/// Routine 模块种子数据初始化服务
/// 创建语言、翻译、字典和系统设置的种子数据
/// </summary>
public class DbSeedRoutineLanguage
{
    private readonly InitLogManager _initLog;
    private readonly IBaseRepository<Language> _languageRepository;
    private readonly IBaseRepository<Translation> _translationRepository;

    public DbSeedRoutineLanguage(
        InitLogManager initLog,
        IBaseRepository<Language> languageRepository,
        IBaseRepository<Translation> translationRepository)
    {
        _initLog = initLog;
        _languageRepository = languageRepository;
        _translationRepository = translationRepository;
    }

    /// <summary>
    /// 初始化 Routine 模块种子数据
    /// </summary>
    public void Initialize()
    {
        _initLog.Information("开始初始化 Routine 模块种子数据..");

        // 1. 初始化语言数据
        var languages = InitializeLanguages();

        // 2. 初始化翻译数据
        InitializeTranslations(languages);

        _initLog.Information("Routine 模块通用翻译初始化完成");
    }

    /// <summary>
    /// 初始化语言数据（中英日三语）
    /// </summary>
    private List<Language> InitializeLanguages()
    {
        var languages = new List<Language>();

        // 中文（简体）
        var zhCn = _languageRepository.GetFirst(l => l.LanguageCode == "zh-CN");
        if (zhCn == null)
        {
            zhCn = new Language
            {
                LanguageCode = "zh-CN",
                LanguageName = "简体中文",
                NativeName = "简体中文",
                LanguageIcon = "🇨🇳",
                IsDefault = 0,  // 布尔字段：0=是（默认）
                IsBuiltin = 0,  // 布尔字段：0=是（内置）
                OrderNum = 1,
                LanguageStatus = 0
            };
            _languageRepository.Create(zhCn, "Takt365");
            _initLog.Information("✅ 创建语言：简体中文");
        }
        languages.Add(zhCn);

        // 英文（美国）
        var enUs = _languageRepository.GetFirst(l => l.LanguageCode == "en-US");
        if (enUs == null)
        {
            enUs = new Language
            {
                LanguageCode = "en-US",
                LanguageName = "English",
                NativeName = "English",
                LanguageIcon = "🇺🇸",
                IsDefault = 1,  // 布尔字段：1=否（非默认）
                IsBuiltin = 0,  // 布尔字段：0=是（内置）
                OrderNum = 2,
                LanguageStatus = 0
            };
            _languageRepository.Create(enUs, "Takt365");
            _initLog.Information("✅ 创建语言：English");
        }
        languages.Add(enUs);

        // 日文
        var jaJp = _languageRepository.GetFirst(l => l.LanguageCode == "ja-JP");
        if (jaJp == null)
        {
            jaJp = new Language
            {
                LanguageCode = "ja-JP",
                LanguageName = "日本語",
                NativeName = "日本語",
                LanguageIcon = "🇯🇵",
                IsDefault = 1,  // 布尔字段：1=否（非默认）
                IsBuiltin = 0,  // 布尔字段：0=是（内置）
                OrderNum = 3,
                LanguageStatus = 0
            };
            _languageRepository.Create(jaJp, "Takt365");
            _initLog.Information("✅ 创建语言：日本語");
        }
        languages.Add(jaJp);

        return languages;
    }

    /// <summary>
    /// 初始化翻译数据
    /// </summary>
    private void InitializeTranslations(List<Language> languages)
    {
        var zhCn = languages.FirstOrDefault(l => l.LanguageCode == "zh-CN");
        var enUs = languages.FirstOrDefault(l => l.LanguageCode == "en-US");
        var jaJp = languages.FirstOrDefault(l => l.LanguageCode == "ja-JP");

        if (zhCn == null || enUs == null || jaJp == null)
        {
            _initLog.Warning("语言数据不完整，跳过翻译数据初始化");
            return;
        }

        // 翻译数据定义（基础通用项）
        var translations = new List<dynamic>
        {
            // 登录相关
            new { Key = "Login.Welcome", Module = "Frontend", ZhCn = "欢迎登录", EnUs = "Welcome", JaJp = "ようこそ" },
            new { Key = "Login.Title", Module = "Frontend", ZhCn = "节拍中小企业平台 - 登录", EnUs = "Takt SMEs Platform - Login", JaJp = "Takt SMEsプラットフォーム - ログイン" },
            new { Key = "Login.Username", Module = "Frontend", ZhCn = "用户名", EnUs = "Username", JaJp = "ユーザー名" },
            new { Key = "Login.Password", Module = "Frontend", ZhCn = "密码", EnUs = "Password", JaJp = "パスワード" },
            new { Key = "Login.RememberMe", Module = "Frontend", ZhCn = "记住密码", EnUs = "Remember Me", JaJp = "パスワードを記憶する" },
            new { Key = "Login.Forgot", Module = "Frontend", ZhCn = "忘记密码？", EnUs = "Forgot password?", JaJp = "パスワードをお忘れの方" },
            new { Key = "Login.Button", Module = "Frontend", ZhCn = "登录", EnUs = "Login", JaJp = "ログイン" },
            new { Key = "Login.Loading", Module = "Frontend", ZhCn = "登录中...", EnUs = "Signing in...", JaJp = "ログイン中..." },
            new { Key = "Login.Error", Module = "Frontend", ZhCn = "登录失败：{0}", EnUs = "Login failed: {0}", JaJp = "ログイン失敗：{0}" },
            new { Key = "Login.Success", Module = "Frontend", ZhCn = "登录成功", EnUs = "Login successful", JaJp = "ログイン成功" },
            new { Key = "Login.Failed.Default", Module = "Frontend", ZhCn = "登录失败，请检查用户名和密码", EnUs = "Login failed, please check your username and password", JaJp = "ログインに失敗しました。ユーザー名とパスワードを確認してください" },
            new { Key = "Login.Description", Module = "Frontend", ZhCn = "请输入您的账号信息", EnUs = "Please enter your account information", JaJp = "アカウント情報を入力してください" },
            
            // 审计字段相关
            new { Key = "common.audit.Id", Module = "Frontend", ZhCn = "主键", EnUs = "ID", JaJp = "ID" },
            new { Key = "common.audit.id", Module = "Frontend", ZhCn = "主键", EnUs = "ID", JaJp = "ID" }, // 小写版本（兼容性）
            new { Key = "common.audit.Remarks", Module = "Frontend", ZhCn = "备注", EnUs = "Remarks", JaJp = "備考" },
            new { Key = "common.audit.remarks", Module = "Frontend", ZhCn = "备注", EnUs = "Remarks", JaJp = "備考" }, // 小写版本（兼容性）
            new { Key = "common.audit.CreatedBy", Module = "Frontend", ZhCn = "创建人", EnUs = "Created By", JaJp = "作成者" },
            new { Key = "common.audit.CreatedTime", Module = "Frontend", ZhCn = "创建时间", EnUs = "Created Time", JaJp = "作成時間" },
            new { Key = "common.audit.created_time", Module = "Frontend", ZhCn = "创建时间", EnUs = "Created Time", JaJp = "作成時間" }, // 下划线版本（兼容性）
            new { Key = "common.audit.UpdatedBy", Module = "Frontend", ZhCn = "更新人", EnUs = "Updated By", JaJp = "更新者" },
            new { Key = "common.audit.UpdatedTime", Module = "Frontend", ZhCn = "更新时间", EnUs = "Updated Time", JaJp = "更新時間" },
            new { Key = "common.audit.DeletedBy", Module = "Frontend", ZhCn = "删除人", EnUs = "Deleted By", JaJp = "削除者" },
            new { Key = "common.audit.DeletedTime", Module = "Frontend", ZhCn = "删除时间", EnUs = "Deleted Time", JaJp = "削除時間" },
            new { Key = "common.audit.IsDeleted", Module = "Frontend", ZhCn = "是否删除", EnUs = "Is Deleted", JaJp = "削除済み" },
            
            
            // 通用基础信息
            new { Key = "Common.CompanyName", Module = "Frontend", ZhCn = "节拍Takt", EnUs = "Takt", JaJp = "タクトTakt" },
            new { Key = "Common.CompanySlogan", Module = "Frontend", ZhCn = "中小企业平台", EnUs = "SMEs Platform", JaJp = "SMEs Platform" },
            new { Key = "Common.CompanyTagline", Module = "Frontend", ZhCn = "数据驱动・决策精准・赋能业务", EnUs = "Data. Precision. Empowerment.", JaJp = "データ・精密・業務革新" },
            new { Key = "Common.Copyright", Module = "Frontend", ZhCn = "Takt All Rights Reserved.节拍信息 保留所有权利.", EnUs = "Takt All Rights Reserved.", JaJp = "タクト情報技術 全著作権所有." },
            new { Key = "Common.CopyrightShort", Module = "Frontend", ZhCn = "节拍", EnUs = "Takt", JaJp = "タクト" },
            new { Key = "Common.Loading", Module = "Frontend", ZhCn = "加载中...", EnUs = "Loading...", JaJp = "読み込み中..." },
            
            // 仪表盘欢迎语
            new { Key = "dashboard.greeting.morning", Module = "Frontend", ZhCn = "早上好", EnUs = "Good morning", JaJp = "おはようございます" },
            new { Key = "dashboard.greeting.noon", Module = "Frontend", ZhCn = "中午好", EnUs = "Good noon", JaJp = "こんにちは" },
            new { Key = "dashboard.greeting.afternoon", Module = "Frontend", ZhCn = "下午好", EnUs = "Good afternoon", JaJp = "こんにちは" },
            new { Key = "dashboard.greeting.evening", Module = "Frontend", ZhCn = "晚上好", EnUs = "Good evening", JaJp = "こんばんは" },
            new { Key = "dashboard.greeting.night", Module = "Frontend", ZhCn = "夜深了，请注意休息", EnUs = "It's late, please take a rest", JaJp = "夜も遅いです。ごゆっくりお休みください" },
            new { Key = "dashboard.greeting.welcomeFormat", Module = "Frontend", ZhCn = "{0}，欢迎 {1}", EnUs = "{0}, welcome {1}", JaJp = "{0}、ようこそ {1} さん" },
            new { Key = "dashboard.greeting.anonymousName", Module = "Frontend", ZhCn = "访客", EnUs = "Guest", JaJp = "ゲスト" },
            new { Key = "dashboard.greeting.fullFormat", Module = "Frontend", ZhCn = "{0}，欢迎 {1}，今天是{2}年{3}月{4}日，{5}，（第{6}天，第{7}季，第{8}周）", EnUs = "{0}, welcome {1}. Today is {2}-{3}-{4}, {5}, (Day {6}, Quarter {7}, Week {8})", JaJp = "{0}、{1} さん、ようこそ。本日は{2}年{3}月{4}日、{5}、（{6}日目、第{7}四半期、第{8}週）" },
            new { Key = "dashboard.greeting.line1Format", Module = "Frontend", ZhCn = "{0}，欢迎 {1}", EnUs = "{0}, welcome {1}", JaJp = "{0}、ようこそ {1} さん" },
            new { Key = "dashboard.greeting.line2Format", Module = "Frontend", ZhCn = "今天是{0}年{1}月{2}日，{3}，（第{4}天，第{5}季，第{6}周）", EnUs = "Today is {0}-{1}-{2}, {3}, (Day {4}, Quarter {5}, Week {6})", JaJp = "本日は{0}年{1}月{2}日、{3}、（{4}日目、第{5}四半期、第{6}週）" },
            
            // 仪表盘卡片统计
            new { Key = "dashboard.card.onlineUsers", Module = "Frontend", ZhCn = "在线用户", EnUs = "Online Users", JaJp = "オンラインユーザー" },
            new { Key = "dashboard.card.todayInbound", Module = "Frontend", ZhCn = "今日入库", EnUs = "Today Inbound", JaJp = "本日入庫" },
            new { Key = "dashboard.card.todayOutbound", Module = "Frontend", ZhCn = "今日出库", EnUs = "Today Outbound", JaJp = "本日出庫" },
            new { Key = "dashboard.card.todayVisitors", Module = "Frontend", ZhCn = "今日来访", EnUs = "Today Visitors", JaJp = "本日訪問者" },
            
            // 仪表盘目的地
            new { Key = "dashboard.destination.usa", Module = "Frontend", ZhCn = "美国", EnUs = "USA", JaJp = "アメリカ" },
            new { Key = "dashboard.destination.eur", Module = "Frontend", ZhCn = "欧洲", EnUs = "EUR", JaJp = "ヨーロッパ" },
            new { Key = "dashboard.destination.china", Module = "Frontend", ZhCn = "中国", EnUs = "CHINA", JaJp = "中国" },
            new { Key = "dashboard.destination.japan", Module = "Frontend", ZhCn = "日本", EnUs = "JAPAN", JaJp = "日本" },
            new { Key = "dashboard.destination.other", Module = "Frontend", ZhCn = "其他", EnUs = "OTHER", JaJp = "その他" },
            
            // 关于页面
            new { Key = "about.description", Module = "Frontend", ZhCn = "节拍（Takt）中小企业平台是一套面向中小企业的智能化管理系统，提供身份认证、权限控制、日常事务和后勤等一体化能力。", EnUs = "The SMEs Platform is an intelligent management suite for small and medium enterprises, offering integrated identity, authorization, routine and logistics capabilities.", JaJp = "SMEsプラットフォームは、中小企業向けの統合管理システムで、認証、権限管理、日常業務、ロジスティクスなどを一体的に提供します。" },
            new { Key = "about.technology", Module = "Frontend", ZhCn = "技术栈", EnUs = "Technology Stack", JaJp = "技術スタック" },
            new { Key = "about.framework", Module = "Frontend", ZhCn = "开发框架", EnUs = "Development Framework", JaJp = "開発フレームワーク" },
            new { Key = "about.framework.value", Module = "Frontend", ZhCn = ".NET 9.0", EnUs = ".NET 9.0", JaJp = ".NET 9.0" },
            new { Key = "about.uiFramework", Module = "Frontend", ZhCn = "界面框架", EnUs = "UI Framework", JaJp = "UIフレームワーク" },
            new { Key = "about.uiFramework.value", Module = "Frontend", ZhCn = "WPF（Windows Presentation Foundation）", EnUs = "WPF (Windows Presentation Foundation)", JaJp = "WPF（Windows Presentation Foundation）" },
            new { Key = "about.database", Module = "Frontend", ZhCn = "数据存储", EnUs = "Data Storage", JaJp = "データストレージ" },
            new { Key = "about.database.value", Module = "Frontend", ZhCn = "SQL Server / SqlSugar ORM", EnUs = "SQL Server / SqlSugar ORM", JaJp = "SQL Server / SqlSugar ORM" },
            new { Key = "about.architecture", Module = "Frontend", ZhCn = "架构模式", EnUs = "Architecture Pattern", JaJp = "アーキテクチャパターン" },
            new { Key = "about.architecture.value", Module = "Frontend", ZhCn = "MVVM（Model-View-ViewModel）", EnUs = "MVVM (Model-View-ViewModel)", JaJp = "MVVM（Model-View-ViewModel）" },
            new { Key = "about.buildDate", Module = "Frontend", ZhCn = "构建时间", EnUs = "Build Time", JaJp = "ビルド日時" },
            new { Key = "about.edition", Module = "Frontend", ZhCn = "社区版 (64 位) - Current", EnUs = "Community (64-bit) - Current", JaJp = "コミュニティ版 (64 ビット) - Current" },
            new { Key = "about.version.format", Module = "Frontend", ZhCn = "版本 {0}", EnUs = "Version {0}", JaJp = "バージョン {0}" },
            new { Key = "about.dotnetVersion.format", Module = "Frontend", ZhCn = ".NET {0}", EnUs = ".NET {0}", JaJp = ".NET {0}" },
            new { Key = "about.links.licenseStatus", Module = "Frontend", ZhCn = "许可状态", EnUs = "License Status", JaJp = "ライセンス状態" },
            new { Key = "about.links.licenseTerms", Module = "Frontend", ZhCn = "许可证条款", EnUs = "License Terms", JaJp = "ライセンス条項" },
            new { Key = "about.links.licenseStatus.message", Module = "Frontend", ZhCn = "许可状态功能尚未实现。", EnUs = "License status is not implemented yet.", JaJp = "ライセンス状態はまだ実装されていません。" },
            new { Key = "about.links.licenseTerms.message", Module = "Frontend", ZhCn = "许可证条款查看功能尚未实现。", EnUs = "Viewing license terms is not implemented yet.", JaJp = "ライセンス条項の表示はまだ実装されていません。" },
            new { Key = "about.section.productInfo", Module = "Frontend", ZhCn = "产品信息", EnUs = "Product Information", JaJp = "製品情報" },
            new { Key = "about.section.environmentInfo", Module = "Frontend", ZhCn = "环境信息", EnUs = "Environment Information", JaJp = "環境情報" },
            new { Key = "about.label.productName", Module = "Frontend", ZhCn = "产品名称", EnUs = "Product Name", JaJp = "製品名" },
            new { Key = "about.label.edition", Module = "Frontend", ZhCn = "版本类型", EnUs = "Edition", JaJp = "エディション" },
            new { Key = "about.label.version", Module = "Frontend", ZhCn = "版本号", EnUs = "Version", JaJp = "バージョン" },
            new { Key = "about.label.company", Module = "Frontend", ZhCn = "公司", EnUs = "Company", JaJp = "会社" },
            new { Key = "about.label.installationPath", Module = "Frontend", ZhCn = "安装位置", EnUs = "Installation Path", JaJp = "インストール場所" },
            new { Key = "about.label.dotnetVersion", Module = "Frontend", ZhCn = ".NET 版本", EnUs = ".NET Version", JaJp = ".NET バージョン" },
            new { Key = "about.label.osVersion", Module = "Frontend", ZhCn = "操作系统", EnUs = "Operating System", JaJp = "オペレーティングシステム" },
            new { Key = "about.label.architecture", Module = "Frontend", ZhCn = "体系结构", EnUs = "Architecture", JaJp = "アーキテクチャ" },
            new { Key = "about.label.processorCount", Module = "Frontend", ZhCn = "处理器数量", EnUs = "Processor Count", JaJp = "プロセッサ数" },
            new { Key = "about.label.runtimeIdentifier", Module = "Frontend", ZhCn = "运行时标识", EnUs = "Runtime Identifier", JaJp = "ランタイム識別子" },
            new { Key = "about.installedProducts.title", Module = "Frontend", ZhCn = "已安装的组件", EnUs = "Installed Products", JaJp = "インストール済みコンポーネント" },
            new { Key = "about.installedProducts.subtitle", Module = "Frontend", ZhCn = "以下组件已经安装并可供使用。", EnUs = "The following components are installed and ready to use.", JaJp = "次のコンポーネントがインストールされ、使用できます。" },
            new { Key = "about.installed.item1", Module = "Frontend", ZhCn = ".NET 9.0 SDK 与 Windows 桌面运行时", EnUs = ".NET 9.0 SDK & Windows Desktop Runtime", JaJp = ".NET 9.0 SDK と Windows デスクトップ ランタイム" },
            new { Key = "about.installed.item2", Module = "Frontend", ZhCn = "MaterialDesignThemes UI 库", EnUs = "MaterialDesignThemes UI Library", JaJp = "MaterialDesignThemes UI ライブラリ" },
            new { Key = "about.installed.item3", Module = "Frontend", ZhCn = "CommunityToolkit.Mvvm", EnUs = "CommunityToolkit.Mvvm", JaJp = "CommunityToolkit.Mvvm" },
            new { Key = "about.installed.item4", Module = "Frontend", ZhCn = "SqlSugar ORM", EnUs = "SqlSugar ORM", JaJp = "SqlSugar ORM" },
            new { Key = "about.installed.item5", Module = "Frontend", ZhCn = "Autofac 依赖注入容器", EnUs = "Autofac Dependency Injection", JaJp = "Autofac 依存性注入" },
            new { Key = "about.installed.item6", Module = "Frontend", ZhCn = "Serilog 日志框架", EnUs = "Serilog Logging Framework", JaJp = "Serilog ログフレームワーク" },
            new { Key = "about.installed.item7", Module = "Frontend", ZhCn = "FontAwesome.Sharp 图标库", EnUs = "FontAwesome.Sharp Icon Library", JaJp = "FontAwesome.Sharp アイコンライブラリ" },
            new { Key = "about.installed.item8", Module = "Frontend", ZhCn = "Microsoft.Data.SqlClient 数据驱动程序", EnUs = "Microsoft.Data.SqlClient Driver", JaJp = "Microsoft.Data.SqlClient ドライバー" },
            new { Key = "about.dialog.title", Module = "Frontend", ZhCn = "关于", EnUs = "About", JaJp = "バージョン情報" },
            new { Key = "about.button.systemInfo", Module = "Frontend", ZhCn = "系统信息", EnUs = "System Information", JaJp = "システム情報" },
            new { Key = "about.systemInfo.title", Module = "Frontend", ZhCn = "系统信息", EnUs = "System Information", JaJp = "システム情報" },
            new { Key = "about.systemInfo.copySuccess", Module = "Frontend", ZhCn = "系统信息已复制到剪贴板", EnUs = "System information copied to clipboard", JaJp = "システム情報がクリップボードにコピーされました" },
            new { Key = "about.systemInfo.copyFailed", Module = "Frontend", ZhCn = "复制失败", EnUs = "Copy failed", JaJp = "コピーに失敗しました" },
            new { Key = "about.systemInfo.tab.summary", Module = "Frontend", ZhCn = "系统摘要", EnUs = "System Summary", JaJp = "システム概要" },
            new { Key = "about.systemInfo.tab.hardware", Module = "Frontend", ZhCn = "硬件信息", EnUs = "Hardware Information", JaJp = "ハードウェア情報" },
            new { Key = "about.systemInfo.tab.software", Module = "Frontend", ZhCn = "软件信息", EnUs = "Software Information", JaJp = "ソフトウェア情報" },
            new { Key = "about.systemInfo.tab.network", Module = "Frontend", ZhCn = "网络信息", EnUs = "Network Information", JaJp = "ネットワーク情報" },
            new { Key = "about.systemInfo.column.item", Module = "Frontend", ZhCn = "项目", EnUs = "Item", JaJp = "項目" },
            new { Key = "about.systemInfo.column.value", Module = "Frontend", ZhCn = "值", EnUs = "Value", JaJp = "値" },
            new { Key = "about.systemInfo.network.adapters", Module = "Frontend", ZhCn = "网络适配器", EnUs = "Network Adapters", JaJp = "ネットワークアダプター" },
            new { Key = "about.systemInfo.network.name", Module = "Frontend", ZhCn = "名称", EnUs = "Name", JaJp = "名前" },
            new { Key = "about.systemInfo.network.mac", Module = "Frontend", ZhCn = "MAC地址", EnUs = "MAC Address", JaJp = "MACアドレス" },
            new { Key = "about.systemInfo.network.ip", Module = "Frontend", ZhCn = "IP地址", EnUs = "IP Address", JaJp = "IPアドレス" },
            new { Key = "about.systemInfo.network.speed", Module = "Frontend", ZhCn = "速度", EnUs = "Speed", JaJp = "速度" },
            new { Key = "about.systemInfo.network.status", Module = "Frontend", ZhCn = "状态", EnUs = "Status", JaJp = "状態" },
            new { Key = "about.systemInfo.network.active", Module = "Frontend", ZhCn = "活动", EnUs = "Active", JaJp = "アクティブ" },
            new { Key = "about.systemInfo.network.inactive", Module = "Frontend", ZhCn = "非活动", EnUs = "Inactive", JaJp = "非アクティブ" },
            new { Key = "about.systemInfo.disk.name", Module = "Frontend", ZhCn = "磁盘", EnUs = "Disk", JaJp = "ディスク" },
            new { Key = "about.systemInfo.software.name", Module = "Frontend", ZhCn = "软件名称", EnUs = "Software Name", JaJp = "ソフトウェア名" },
            new { Key = "about.systemInfo.software.version", Module = "Frontend", ZhCn = "版本", EnUs = "Version", JaJp = "バージョン" },
            new { Key = "about.systemInfo.software.publisher", Module = "Frontend", ZhCn = "发布者", EnUs = "Publisher", JaJp = "発行者" },
            new { Key = "about.systemInfo.user.name", Module = "Frontend", ZhCn = "用户名", EnUs = "User Name", JaJp = "ユーザー名" },
            new { Key = "about.systemInfo.user.fullName", Module = "Frontend", ZhCn = "全名", EnUs = "Full Name", JaJp = "フルネーム" },
            new { Key = "about.systemInfo.user.isAdmin", Module = "Frontend", ZhCn = "管理员", EnUs = "Administrator", JaJp = "管理者" },
            new { Key = "about.systemInfo.user.yes", Module = "Frontend", ZhCn = "是", EnUs = "Yes", JaJp = "はい" },
            new { Key = "about.systemInfo.user.no", Module = "Frontend", ZhCn = "否", EnUs = "No", JaJp = "いいえ" },
            new { Key = "about.systemInfo.summary.systemLanguage", Module = "Frontend", ZhCn = "系统语言", EnUs = "System Language", JaJp = "システム言語" },
            new { Key = "about.systemInfo.hardware.diskInfo", Module = "Frontend", ZhCn = "磁盘信息", EnUs = "Disk Information", JaJp = "ディスク情報" },
            new { Key = "about.systemInfo.copy.title", Module = "Frontend", ZhCn = "========== 系统信息 ==========", EnUs = "========== System Information ==========", JaJp = "========== システム情報 ==========" },
            new { Key = "about.systemInfo.copy.section.summary", Module = "Frontend", ZhCn = "【系统摘要】", EnUs = "【System Summary】", JaJp = "【システム概要】" },
            new { Key = "about.systemInfo.copy.section.hardware", Module = "Frontend", ZhCn = "【硬件信息】", EnUs = "【Hardware Information】", JaJp = "【ハードウェア情報】" },
            new { Key = "about.systemInfo.copy.section.software", Module = "Frontend", ZhCn = "【软件信息】", EnUs = "【Software Information】", JaJp = "【ソフトウェア情報】" },
            new { Key = "about.systemInfo.copy.section.network", Module = "Frontend", ZhCn = "【网络信息】", EnUs = "【Network Information】", JaJp = "【ネットワーク情報】" },
            new { Key = "about.systemInfo.key.os", Module = "Frontend", ZhCn = "操作系统", EnUs = "Operating System", JaJp = "オペレーティングシステム" },
            new { Key = "about.systemInfo.key.osVersion", Module = "Frontend", ZhCn = "系统版本", EnUs = "OS Version", JaJp = "OSバージョン" },
            new { Key = "about.systemInfo.key.osType", Module = "Frontend", ZhCn = "系统类型", EnUs = "OS Type", JaJp = "OSタイプ" },
            new { Key = "about.systemInfo.key.osArchitecture", Module = "Frontend", ZhCn = "系统架构", EnUs = "OS Architecture", JaJp = "OSアーキテクチャ" },
            new { Key = "about.systemInfo.key.machineName", Module = "Frontend", ZhCn = "机器名称", EnUs = "Machine Name", JaJp = "マシン名" },
            new { Key = "about.systemInfo.key.userName", Module = "Frontend", ZhCn = "用户名", EnUs = "User Name", JaJp = "ユーザー名" },
            new { Key = "about.systemInfo.key.isAdmin", Module = "Frontend", ZhCn = "是否管理员", EnUs = "Is Administrator", JaJp = "管理者か" },
            new { Key = "about.systemInfo.key.runtime", Module = "Frontend", ZhCn = "运行时", EnUs = "Runtime", JaJp = "ランタイム" },
            new { Key = "about.systemInfo.key.processArchitecture", Module = "Frontend", ZhCn = "进程架构", EnUs = "Process Architecture", JaJp = "プロセスアーキテクチャ" },
            new { Key = "about.systemInfo.key.systemUptime", Module = "Frontend", ZhCn = "系统运行时间", EnUs = "System Uptime", JaJp = "システム稼働時間" },
            new { Key = "about.systemInfo.key.cpu", Module = "Frontend", ZhCn = "CPU", EnUs = "CPU", JaJp = "CPU" },
            new { Key = "about.systemInfo.key.cpuName", Module = "Frontend", ZhCn = "CPU名称", EnUs = "CPU Name", JaJp = "CPU名" },
            new { Key = "about.systemInfo.key.cpuCores", Module = "Frontend", ZhCn = "CPU核心", EnUs = "CPU Cores", JaJp = "CPUコア" },
            new { Key = "about.systemInfo.key.totalMemory", Module = "Frontend", ZhCn = "物理内存", EnUs = "Total Memory", JaJp = "物理メモリ" },
            new { Key = "about.systemInfo.key.availableMemory", Module = "Frontend", ZhCn = "可用内存", EnUs = "Available Memory", JaJp = "利用可能メモリ" },
            new { Key = "about.systemInfo.key.memoryUsage", Module = "Frontend", ZhCn = "内存使用率", EnUs = "Memory Usage", JaJp = "メモリ使用率" },
            new { Key = "about.systemInfo.key.processMemory", Module = "Frontend", ZhCn = "进程内存", EnUs = "Process Memory", JaJp = "プロセスメモリ" },
            new { Key = "about.systemInfo.key.ipAddress", Module = "Frontend", ZhCn = "IP地址", EnUs = "IP Address", JaJp = "IPアドレス" },
            new { Key = "about.systemInfo.key.macAddress", Module = "Frontend", ZhCn = "MAC地址", EnUs = "MAC Address", JaJp = "MACアドレス" },
            new { Key = "about.disclaimer", Module = "Frontend", ZhCn = "警告: 本计算机程序受著作权法以及国际版权公约保护。未经授权而擅自复制或传播本程序（或其中任何部分），将受到严厉的民事及刑事处罚，并将在法律许可的最大限度内受到追诉。", EnUs = "Warning: This computer program is protected by copyright law and international copyright conventions. Unauthorized copying or distribution of this program (or any part thereof) will be subject to severe civil and criminal penalties, and will be prosecuted to the fullest extent permitted by law.", JaJp = "警告: 本コンピュータプログラムは著作権法および国際著作権条約によって保護されています。許可なく本プログラム（またはその一部）を複製または配布することは、厳しい民事および刑事罰の対象となり、法律で許可される最大限の範囲で起訴されます。" },

            // 用户管理相关（通用功能，非实体字段）
            new { Key = "common.button.assignrole", Module = "Frontend", ZhCn = "分配角色", EnUs = "Assign Role", JaJp = "ロールを割り当て" },
            new { Key = "common.button.assigndept", Module = "Frontend", ZhCn = "分配部门", EnUs = "Assign Department", JaJp = "部門を割り当て" },
            new { Key = "common.button.assignmenu", Module = "Frontend", ZhCn = "分配菜单", EnUs = "Assign Menu", JaJp = "メニューを割り当て" },
            new { Key = "common.button.assignpost", Module = "Frontend", ZhCn = "分配岗位", EnUs = "Assign Position", JaJp = "ポジションを割り当て" },
            new { Key = "Identity.User.LoadRolesFailed", Module = "Frontend", ZhCn = "加载角色列表失败", EnUs = "Failed to load role list", JaJp = "ロールリストの読み込みに失敗しました" },
            new { Key = "Identity.User.LoadUserRolesFailed", Module = "Frontend", ZhCn = "加载用户角色失败", EnUs = "Failed to load user roles", JaJp = "ユーザーロールの読み込みに失敗しました" },
            new { Key = "Identity.User.AssignRoleFailed", Module = "Frontend", ZhCn = "分配角色失败", EnUs = "Failed to assign roles", JaJp = "ロールの割り当てに失敗しました" },
            new { Key = "Identity.User.AssignRoleSuccess", Module = "Frontend", ZhCn = "分配角色成功", EnUs = "Roles assigned successfully", JaJp = "ロールが正常に割り当てられました" },
            
            // 用户验证相关（登录表单验证 - 仅保留特定业务规则，表单验证在 DbSeedRoutineEntity.cs）
            new { Key = "Identity.User.Validation.UsernameNotFound", Module = "Frontend", ZhCn = "该用户名不存在", EnUs = "Username does not exist", JaJp = "このユーザー名は存在しません" },
            new { Key = "Identity.User.Validation.PasswordIncorrect", Module = "Frontend", ZhCn = "密码不正确", EnUs = "Password is incorrect", JaJp = "パスワードが正しくありません" },
            new { Key = "Identity.User.Transfer.Unassigned", Module = "Frontend", ZhCn = "未分配角色", EnUs = "Unassigned Roles", JaJp = "未割り当てロール" },
            new { Key = "Identity.User.Transfer.Assigned", Module = "Frontend", ZhCn = "已分配角色", EnUs = "Assigned Roles", JaJp = "割り当て済みロール" },
            new { Key = "Identity.User.Transfer.MoveRight", Module = "Frontend", ZhCn = "添加到已分配", EnUs = "Add to Assigned", JaJp = "割り当て済みに追加" },
            new { Key = "Identity.User.Transfer.MoveAllRight", Module = "Frontend", ZhCn = "全部添加到已分配", EnUs = "Add All to Assigned", JaJp = "すべてを割り当て済みに追加" },
            new { Key = "Identity.User.Transfer.MoveLeft", Module = "Frontend", ZhCn = "从已分配移除", EnUs = "Remove from Assigned", JaJp = "割り当て済みから削除" },
            new { Key = "Identity.User.Transfer.MoveAllLeft", Module = "Frontend", ZhCn = "全部从已分配移除", EnUs = "Remove All from Assigned", JaJp = "すべてを割り当て済みから削除" },
            new { Key = "Button.Close", Module = "Frontend", ZhCn = "关闭", EnUs = "Close", JaJp = "閉じる" },

            // 通用操作结果提示（成功类）
            new { Key = "common.success.create", Module = "Frontend", ZhCn = "{0}创建成功", EnUs = "{0} created successfully", JaJp = "{0} が作成されました" },
            new { Key = "common.success.update", Module = "Frontend", ZhCn = "{0}更新成功", EnUs = "{0} updated successfully", JaJp = "{0} が更新されました" },
            new { Key = "common.success.delete", Module = "Frontend", ZhCn = "{0}删除成功", EnUs = "{0} deleted successfully", JaJp = "{0} が削除されました" },
            new { Key = "common.success.import", Module = "Frontend", ZhCn = "{0}导入成功", EnUs = "{0} imported successfully", JaJp = "{0} のインポートに成功しました" },
            new { Key = "common.success.export", Module = "Frontend", ZhCn = "{0}导出成功", EnUs = "{0} exported successfully", JaJp = "{0} のエクスポートに成功しました" },
            new { Key = "common.success.authorize.user", Module = "Frontend", ZhCn = "用户名{0}已成功授权", EnUs = "User {0} has been authorized successfully", JaJp = "ユーザー{0}の権限付与に成功しました" },
            new { Key = "common.success.enable.name", Module = "Frontend", ZhCn = "{0}启用成功", EnUs = "{0} enabled successfully", JaJp = "{0} が有効化されました" },
            new { Key = "common.success.disable.name", Module = "Frontend", ZhCn = "{0}禁用成功", EnUs = "{0} disabled successfully", JaJp = "{0} が無効化されました" },
            new { Key = "common.success.submit", Module = "Frontend", ZhCn = "提交成功", EnUs = "Submitted successfully", JaJp = "提出に成功しました" },
            new { Key = "common.success.approve.name", Module = "Frontend", ZhCn = "{0}审核成功", EnUs = "{0} approved successfully", JaJp = "{0} の承認に成功しました" },
            new { Key = "common.success.reject.name", Module = "Frontend", ZhCn = "{0}撤销成功", EnUs = "{0} revoked successfully", JaJp = "{0} の取り消しに成功しました" },

            // 通用删除确认
            new { Key = "common.confirm.delete", Module = "Frontend", ZhCn = "确定要删除{0}吗？", EnUs = "Are you sure you want to delete {0}?", JaJp = "{0} を削除してもよろしいですか？" },
            new { Key = "common.confirm.delete.by_entity_ids", Module = "Frontend", ZhCn = "确定要删除{0}表ID为[{1}]的{2}条记录吗？", EnUs = "Are you sure to delete {2} records in {0} with IDs [{1}]?", JaJp = "{0}テーブルのIDが[{1}]の{2}件のレコードを削除してもよろしいですか？" },

            // 通用校验
            new { Key = "validation.required", Module = "Frontend", ZhCn = "{0}为必填项", EnUs = "{0} is required", JaJp = "{0} は必須です" },
            new { Key = "validation.format", Module = "Frontend", ZhCn = "{0}格式不正确", EnUs = "Invalid {0} format", JaJp = "{0} の形式が正しくありません" },
            new { Key = "validation.maxLength", Module = "Frontend", ZhCn = "{0}长度不能超过{1}个字符", EnUs = "{0} cannot exceed {1} characters", JaJp = "{0} は{1}文字を超えることはできません" },
            new { Key = "validation.minLength", Module = "Frontend", ZhCn = "{0}长度不能少于{1}位", EnUs = "{0} must be at least {1} characters", JaJp = "{0} は{1}文字以上である必要があります" },
            new { Key = "validation.invalid", Module = "Frontend", ZhCn = "{0}无效", EnUs = "{0} is invalid", JaJp = "{0} が無効です" },
            
            // 通用操作结果（失败类）
            new { Key = "common.failed.create", Module = "Frontend", ZhCn = "{0}创建失败", EnUs = "Failed to create {0}", JaJp = "{0} の作成に失敗しました" },
            new { Key = "common.failed.update", Module = "Frontend", ZhCn = "{0}更新失败", EnUs = "Failed to update {0}", JaJp = "{0} の更新に失敗しました" },
            new { Key = "common.failed.delete", Module = "Frontend", ZhCn = "{0}删除失败", EnUs = "Failed to delete {0}", JaJp = "{0} の削除に失敗しました" },
            new { Key = "common.failed.import", Module = "Frontend", ZhCn = "{0}导入失败", EnUs = "Failed to import {0}", JaJp = "{0} のインポートに失敗しました" },
            new { Key = "common.failed.export", Module = "Frontend", ZhCn = "{0}导出失败", EnUs = "Failed to export {0}", JaJp = "{0} のエクスポートに失敗しました" },
            new { Key = "common.saveFailed", Module = "Frontend", ZhCn = "保存失败", EnUs = "Save failed", JaJp = "保存に失敗しました" },
            
            // 消息框相关
            new { Key = "common.messageBox.information", Module = "Frontend", ZhCn = "信息", EnUs = "Information", JaJp = "情報" },
            new { Key = "common.messageBox.warning", Module = "Frontend", ZhCn = "警告", EnUs = "Warning", JaJp = "警告" },
            new { Key = "common.messageBox.error", Module = "Frontend", ZhCn = "错误", EnUs = "Error", JaJp = "エラー" },
            new { Key = "common.messageBox.question", Module = "Frontend", ZhCn = "确认", EnUs = "Question", JaJp = "確認" },
            new { Key = "common.messageBox.confirmDelete", Module = "Frontend", ZhCn = "确认删除", EnUs = "Confirm Delete", JaJp = "削除の確認" },
            new { Key = "common.messageBox.confirmDeleteMessage", Module = "Frontend", ZhCn = "确定要删除这条记录吗？", EnUs = "Are you sure you want to delete this record?", JaJp = "このレコードを削除してもよろしいですか？" },
            new { Key = "common.saveFailed", Module = "Frontend", ZhCn = "保存失败：{0}", EnUs = "Save failed: {0}", JaJp = "保存に失敗しました：{0}" },

            // 通用占位（参数化占位符）
            new { Key = "common.noData", Module = "Frontend", ZhCn = "暂无数据", EnUs = "No Data", JaJp = "データなし" },
            new { Key = "common.template", Module = "Frontend", ZhCn = "模板", EnUs = "Template", JaJp = "テンプレート" },
            new { Key = "common.file", Module = "Frontend", ZhCn = "文件", EnUs = "File", JaJp = "ファイル" },
            new { Key = "common.placeholder.input", Module = "Frontend", ZhCn = "请输入{0}", EnUs = "Please enter {0}", JaJp = "{0} を入力してください" },
            new { Key = "common.placeholder.select", Module = "Frontend", ZhCn = "请选择{0}", EnUs = "Please select {0}", JaJp = "{0} を選択してください" },
            new { Key = "common.placeholder.search", Module = "Frontend", ZhCn = "请输入{0}进行搜索", EnUs = "Please enter {0} to search", JaJp = "検索するには {0} を入力してください" },
            new { Key = "common.placeholder.range", Module = "Frontend", ZhCn = "请选择{0}范围", EnUs = "Select {0} range", JaJp = "{0} の範囲を選択" },
            new { Key = "common.placeholder.keywordHint", Module = "Frontend", ZhCn = "请输入{0}等关键字查询", EnUs = "Enter {0} keywords", JaJp = "{0}などのキーワードを入力" },
            new { Key = "common.selectionHeaderHint", Module = "Frontend", ZhCn = "全选/取消全选", EnUs = "Select/Deselect all", JaJp = "全選択/全解除" },
            new { Key = "common.selectionRowHint", Module = "Frontend", ZhCn = "选择/取消选择该行", EnUs = "Select/Deselect this row", JaJp = "この行を選択/解除" },
            new { Key = "common.goTo", Module = "Frontend", ZhCn = "前往", EnUs = "Go to", JaJp = "移動" },

            // 通用操作
            new { Key = "common.selection", Module = "Frontend", ZhCn = "选择", EnUs = "Selection", JaJp = "選択" },
            new { Key = "common.operation", Module = "Frontend", ZhCn = "操作", EnUs = "Operation", JaJp = "操作" },
            new { Key = "common.search", Module = "Frontend", ZhCn = "搜索", EnUs = "Search", JaJp = "検索" },
            new { Key = "common.reset", Module = "Frontend", ZhCn = "重置", EnUs = "Reset", JaJp = "リセット" },
            new { Key = "common.advancedQuery", Module = "Frontend", ZhCn = "高级查询", EnUs = "Advanced Query", JaJp = "高度検索" },
            new { Key = "common.toggleColumns", Module = "Frontend", ZhCn = "显隐列", EnUs = "Toggle Columns", JaJp = "列の表示/非表示" },
            new { Key = "common.toggleQueryBar", Module = "Frontend", ZhCn = "显隐查询栏", EnUs = "Toggle Query Bar", JaJp = "検索バーの表示/非表示" },
            new { Key = "common.expandAll", Module = "Frontend", ZhCn = "展开全部", EnUs = "Expand All", JaJp = "すべて展開" },
            new { Key = "common.collapseAll", Module = "Frontend", ZhCn = "折叠全部", EnUs = "Collapse All", JaJp = "すべて折りたたむ" },
            new { Key = "common.firstPage", Module = "Frontend", ZhCn = "首页", EnUs = "First Page", JaJp = "最初のページ" },
            new { Key = "common.prevPage", Module = "Frontend", ZhCn = "上一页", EnUs = "Previous Page", JaJp = "前のページ" },
            new { Key = "common.nextPage", Module = "Frontend", ZhCn = "下一页", EnUs = "Next Page", JaJp = "次のページ" },
            new { Key = "common.lastPage", Module = "Frontend", ZhCn = "末页", EnUs = "Last Page", JaJp = "最後のページ" },
            new { Key = "common.total", Module = "Frontend", ZhCn = "共 {0} 条记录", EnUs = "Total {0} records", JaJp = "合計 {0} 件" },
            new { Key = "common.pageDisplay", Module = "Frontend", ZhCn = "第 {0} / {1} 页", EnUs = "Page {0} / {1}", JaJp = "{0} / {1} ページ" },
            new { Key = "common.pageSizeHint", Module = "Frontend", ZhCn = "每页", EnUs = "Per Page", JaJp = "1ページあたり" },
            new { Key = "common.pageInputHint", Module = "Frontend", ZhCn = "页码", EnUs = "Page", JaJp = "ページ" },
            
            // 通用按钮（统一键）
            new { Key = "common.button.close", Module = "Frontend", ZhCn = "关闭", EnUs = "Close", JaJp = "閉じる" },
            new { Key = "common.button.changeTheme", Module = "Frontend", ZhCn = "切换主题", EnUs = "Toggle Theme", JaJp = "テーマを切り替え" },
            new { Key = "common.button.changeLanguage", Module = "Frontend", ZhCn = "切换语言", EnUs = "Toggle Language", JaJp = "言語を切り替え" },
            new { Key = "common.button.query", Module = "Frontend", ZhCn = "查询", EnUs = "Query", JaJp = "検索" },
            new { Key = "common.button.read", Module = "Frontend", ZhCn = "查看", EnUs = "Read", JaJp = "閲覧" },
            new { Key = "common.button.create", Module = "Frontend", ZhCn = "新增", EnUs = "Create", JaJp = "新規" },
            new { Key = "common.button.createrow", Module = "Frontend", ZhCn = "新增行", EnUs = "Create Row", JaJp = "新規行" },
            new { Key = "common.button.createcolumn", Module = "Frontend", ZhCn = "新增列", EnUs = "Create Column", JaJp = "新規列" },
            new { Key = "common.button.update", Module = "Frontend", ZhCn = "更新", EnUs = "Update", JaJp = "更新" },
            new { Key = "common.button.delete", Module = "Frontend", ZhCn = "删除", EnUs = "Delete", JaJp = "削除" },
            new { Key = "common.button.detail", Module = "Frontend", ZhCn = "详情", EnUs = "Detail", JaJp = "詳細" },
            new { Key = "common.button.export", Module = "Frontend", ZhCn = "导出", EnUs = "Export", JaJp = "エクスポート" },
            new { Key = "common.button.import", Module = "Frontend", ZhCn = "导入", EnUs = "Import", JaJp = "インポート" },
            new { Key = "common.button.print", Module = "Frontend", ZhCn = "打印", EnUs = "Print", JaJp = "印刷" },
            new { Key = "common.button.preview", Module = "Frontend", ZhCn = "预览", EnUs = "Preview", JaJp = "プレビュー" },
            new { Key = "common.button.enable", Module = "Frontend", ZhCn = "启用", EnUs = "Enable", JaJp = "有効化" },
            new { Key = "common.button.disable", Module = "Frontend", ZhCn = "禁用", EnUs = "Disable", JaJp = "無効化" },
            new { Key = "common.enabled", Module = "Frontend", ZhCn = "启用", EnUs = "Enabled", JaJp = "有効" },
            new { Key = "common.disabled", Module = "Frontend", ZhCn = "禁用", EnUs = "Disabled", JaJp = "無効" },
            new { Key = "common.button.lock", Module = "Frontend", ZhCn = "锁定", EnUs = "Lock", JaJp = "ロック" },
            new { Key = "common.button.unlock", Module = "Frontend", ZhCn = "解锁", EnUs = "Unlock", JaJp = "アンロック" },
            new { Key = "common.button.authorize", Module = "Frontend", ZhCn = "授权", EnUs = "Authorize", JaJp = "権限付与" },
            new { Key = "common.button.grant", Module = "Frontend", ZhCn = "授予", EnUs = "Grant", JaJp = "付与" },
            new { Key = "common.button.revoke", Module = "Frontend", ZhCn = "收回", EnUs = "Revoke", JaJp = "剥奪" },
            new { Key = "common.button.run", Module = "Frontend", ZhCn = "运行", EnUs = "Run", JaJp = "実行" },
            new { Key = "common.button.generate", Module = "Frontend", ZhCn = "生成", EnUs = "Generate", JaJp = "生成" },
            new { Key = "common.button.start", Module = "Frontend", ZhCn = "启动", EnUs = "Start", JaJp = "開始" },
            new { Key = "common.button.stop", Module = "Frontend", ZhCn = "停止", EnUs = "Stop", JaJp = "停止" },
            new { Key = "common.button.pause", Module = "Frontend", ZhCn = "暂停", EnUs = "Pause", JaJp = "一時停止" },
            new { Key = "common.button.resume", Module = "Frontend", ZhCn = "恢复", EnUs = "Resume", JaJp = "再開" },
            new { Key = "common.button.restart", Module = "Frontend", ZhCn = "重启", EnUs = "Restart", JaJp = "再起動" },
            new { Key = "common.button.submit", Module = "Frontend", ZhCn = "提交", EnUs = "Submit", JaJp = "提出" },
            new { Key = "common.button.save", Module = "Frontend", ZhCn = "保存", EnUs = "Save", JaJp = "保存" },
            new { Key = "common.button.ok", Module = "Frontend", ZhCn = "确定", EnUs = "OK", JaJp = "OK" },
            new { Key = "common.button.yes", Module = "Frontend", ZhCn = "是", EnUs = "Yes", JaJp = "はい" },
            new { Key = "common.button.no", Module = "Frontend", ZhCn = "否", EnUs = "No", JaJp = "いいえ" },
            new { Key = "common.button.cancel", Module = "Frontend", ZhCn = "取消", EnUs = "Cancel", JaJp = "キャンセル" },
            new { Key = "common.button.approve", Module = "Frontend", ZhCn = "通过", EnUs = "Approve", JaJp = "承認" },
            new { Key = "common.button.reject", Module = "Frontend", ZhCn = "驳回", EnUs = "Reject", JaJp = "却下" },
            new { Key = "common.button.recall", Module = "Frontend", ZhCn = "撤回", EnUs = "Recall", JaJp = "取り消し" },
            new { Key = "common.button.send", Module = "Frontend", ZhCn = "发送", EnUs = "Send", JaJp = "送信" },
            new { Key = "common.button.publish", Module = "Frontend", ZhCn = "发布", EnUs = "Publish", JaJp = "公開" },
            new { Key = "common.button.notify", Module = "Frontend", ZhCn = "通知", EnUs = "Notify", JaJp = "通知" },
            new { Key = "common.button.download", Module = "Frontend", ZhCn = "{0}下载", EnUs = "Download {0}", JaJp = "{0}をダウンロード" },
            new { Key = "common.button.upload", Module = "Frontend", ZhCn = "{0}上传", EnUs = "Upload {0}", JaJp = "{0}をアップロード" },
            new { Key = "common.button.attach", Module = "Frontend", ZhCn = "附件", EnUs = "Attach", JaJp = "添付" },
            new { Key = "common.button.favorite", Module = "Frontend", ZhCn = "收藏", EnUs = "Favorite", JaJp = "お気に入り" },
            new { Key = "common.button.like", Module = "Frontend", ZhCn = "点赞", EnUs = "Like", JaJp = "いいね" },
            new { Key = "common.button.comment", Module = "Frontend", ZhCn = "评论", EnUs = "Comment", JaJp = "コメント" },
            new { Key = "common.button.share", Module = "Frontend", ZhCn = "分享", EnUs = "Share", JaJp = "共有" },
            new { Key = "common.button.subscribe", Module = "Frontend", ZhCn = "订阅", EnUs = "Subscribe", JaJp = "購読" },
            new { Key = "common.button.reset", Module = "Frontend", ZhCn = "重置", EnUs = "Reset", JaJp = "リセット" },
            new { Key = "common.button.sync", Module = "Frontend", ZhCn = "同步", EnUs = "Sync", JaJp = "同期" },
            new { Key = "common.button.copy", Module = "Frontend", ZhCn = "复制", EnUs = "Copy", JaJp = "コピー" },
            new { Key = "common.button.clone", Module = "Frontend", ZhCn = "克隆", EnUs = "Clone", JaJp = "クローン" },
            new { Key = "common.button.refresh", Module = "Frontend", ZhCn = "刷新", EnUs = "Refresh", JaJp = "リフレッシュ" },
            new { Key = "common.button.archive", Module = "Frontend", ZhCn = "归档", EnUs = "Archive", JaJp = "アーカイブ" },
            new { Key = "common.button.restore", Module = "Frontend", ZhCn = "还原", EnUs = "Restore", JaJp = "復元" },
            new { Key = "common.button.apply", Module = "Frontend", ZhCn = "应用", EnUs = "Apply", JaJp = "適用" },
            new { Key = "common.button.clear", Module = "Frontend", ZhCn = "清理", EnUs = "Clear", JaJp = "クリア" },
            
            // 日志清理相关
            new { Key = "Logging.Cleanup.Confirm", Module = "Frontend", ZhCn = "确定要清理超过7天的日志吗？此操作将删除文本日志文件和数据表日志记录，且无法恢复。", EnUs = "Are you sure you want to clean up logs older than 7 days? This operation will delete text log files and database log records, and cannot be undone.", JaJp = "7日を超えるログをクリーンアップしてもよろしいですか？この操作はテキストログファイルとデータベースログレコードを削除し、元に戻すことはできません。" },
            new { Key = "Logging.Cleanup.Success", Module = "Frontend", ZhCn = "日志清理成功：清理了 {0} 个文件（{1}）和 {2} 条数据库记录", EnUs = "Log cleanup successful: {0} files ({1}) and {2} database records cleaned", JaJp = "ログクリーンアップ成功：{0} ファイル（{1}）と {2} データベースレコードをクリーンアップしました" },
            new { Key = "Logging.Cleanup.Failed", Module = "Frontend", ZhCn = "日志清理失败：{0}", EnUs = "Log cleanup failed: {0}", JaJp = "ログクリーンアップ失敗：{0}" },
            new { Key = "Logging.Cleanup.InProgress", Module = "Frontend", ZhCn = "正在清理日志，请稍候...", EnUs = "Cleaning up logs, please wait...", JaJp = "ログをクリーンアップしています。お待ちください..." },
            new { Key = "Logging.Cleanup.FileSize", Module = "Frontend", ZhCn = "{0} KB", EnUs = "{0} KB", JaJp = "{0} KB" },
            
            // 日志清理范围选项
            new { Key = "Logging.Cleanup.Range.Today", Module = "Frontend", ZhCn = "按日", EnUs = "By Day", JaJp = "日ごと" },
            new { Key = "Logging.Cleanup.Range.SevenDays", Module = "Frontend", ZhCn = "按周", EnUs = "By Week", JaJp = "週ごと" },
            new { Key = "Logging.Cleanup.Range.ThirtyDays", Module = "Frontend", ZhCn = "按月", EnUs = "By Month", JaJp = "月ごと" },
            new { Key = "Logging.Cleanup.Range.All", Module = "Frontend", ZhCn = "全部", EnUs = "All", JaJp = "すべて" },
            

            // 应用标题
            new { Key = "application.title", Module = "Frontend", ZhCn = "Takt SMEs", EnUs = "Takt SMEs", JaJp = "Takt SMEs" },
            
            // 设置页面翻译
            new { Key = "Settings.Customize.Title", Module = "Frontend", ZhCn = "用户设置", EnUs = "User Settings", JaJp = "ユーザー設定" },
            new { Key = "Settings.Customize.Description", Module = "Frontend", ZhCn = "自定义应用程序的外观和行为", EnUs = "Customize the appearance and behavior of the application", JaJp = "アプリケーションの外観と動作をカスタマイズ" },
            new { Key = "Settings.Customize.AppearanceAndBehavior", Module = "Frontend", ZhCn = "外观与行为", EnUs = "Appearance & Behavior", JaJp = "外観と動作" },
            new { Key = "Settings.Customize.ThemeMode", Module = "Frontend", ZhCn = "主题模式", EnUs = "Theme Mode", JaJp = "テーマモード" },
            new { Key = "Settings.Customize.ThemeMode.Description", Module = "Frontend", ZhCn = "选择应用程序的主题模式", EnUs = "Select the theme mode for the application", JaJp = "アプリケーションのテーマモードを選択" },
            
            // 主题选项（统一使用 common.theme.* 前缀）
            new { Key = "common.theme.system", Module = "Frontend", ZhCn = "跟随系统", EnUs = "Follow System", JaJp = "システムに従う" },
            new { Key = "common.theme.light", Module = "Frontend", ZhCn = "浅色", EnUs = "Light", JaJp = "ライト" },
            new { Key = "common.theme.dark", Module = "Frontend", ZhCn = "深色", EnUs = "Dark", JaJp = "ダーク" },
            
            // 主题切换提示文本
            new { Key = "common.clickToSwitch", Module = "Frontend", ZhCn = "点击切换到", EnUs = "Click to switch to", JaJp = "クリックして切り替え" },
            new { Key = "Settings.Customize.Language", Module = "Frontend", ZhCn = "语言", EnUs = "Language", JaJp = "言語" },
            new { Key = "Settings.Customize.Language.Description", Module = "Frontend", ZhCn = "选择应用程序的显示语言", EnUs = "Select the display language for the application", JaJp = "アプリケーションの表示言語を選択" },
            new { Key = "Settings.Customize.FontSettings", Module = "Frontend", ZhCn = "字体设置", EnUs = "Font Settings", JaJp = "フォント設定" },
            new { Key = "Settings.Customize.FontFamily", Module = "Frontend", ZhCn = "字体族", EnUs = "Font Family", JaJp = "フォントファミリー" },
            new { Key = "Settings.Customize.FontFamily.Description", Module = "Frontend", ZhCn = "选择应用程序的字体族", EnUs = "Select the font family for the application", JaJp = "アプリケーションのフォントファミリーを選択" },
            new { Key = "Settings.Customize.FontPreview", Module = "Frontend", ZhCn = "字体预览：", EnUs = "Font Preview: ", JaJp = "フォントプレビュー：" },
            new { Key = "Settings.Customize.FontSize", Module = "Frontend", ZhCn = "字体大小", EnUs = "Font Size", JaJp = "フォントサイズ" },
            new { Key = "Settings.Customize.FontSize.Description", Module = "Frontend", ZhCn = "选择应用程序的字体大小", EnUs = "Select the font size for the application", JaJp = "アプリケーションのフォントサイズを選択" },
            new { Key = "Settings.Customize.FontSizePreview", Module = "Frontend", ZhCn = "字体大小预览", EnUs = "Font Size Preview", JaJp = "フォントサイズプレビュー" },
            new { Key = "Settings.Customize.FontSizePreview.Sample", Module = "Frontend", ZhCn = "这是字体大小预览文本", EnUs = "This is a font size preview text", JaJp = "これはフォントサイズのプレビューテキストです" },
            new { Key = "Settings.Customize.LoadFailed", Module = "Frontend", ZhCn = "加载设置失败：{0}", EnUs = "Failed to load settings: {0}", JaJp = "設定の読み込みに失敗しました：{0}" },
            new { Key = "Settings.Customize.LanguageNotSelected", Module = "Frontend", ZhCn = "本地化管理器未初始化或未选择语言", EnUs = "Localization manager not initialized or language not selected", JaJp = "ローカライゼーションマネージャーが初期化されていないか、言語が選択されていません" },
            new { Key = "Settings.Customize.LanguageChanged", Module = "Frontend", ZhCn = "语言已切换为 {0}", EnUs = "Language changed to {0}", JaJp = "言語が {0} に変更されました" },
            new { Key = "Settings.Customize.LanguageChangeFailed", Module = "Frontend", ZhCn = "切换语言失败：{0}", EnUs = "Failed to change language: {0}", JaJp = "言語の変更に失敗しました：{0}" },
            new { Key = "Settings.Customize.FontFamilyNotSelected", Module = "Frontend", ZhCn = "请选择字体", EnUs = "Please select a font family", JaJp = "フォントファミリーを選択してください" },
            new { Key = "Settings.Customize.FontFamilyChanged", Module = "Frontend", ZhCn = "字体已切换为 {0}", EnUs = "Font family changed to {0}", JaJp = "フォントファミリーが {0} に変更されました" },
            new { Key = "Settings.Customize.FontFamilyChangeFailed", Module = "Frontend", ZhCn = "切换字体失败：{0}", EnUs = "Failed to change font family: {0}", JaJp = "フォントファミリーの変更に失敗しました：{0}" },
            new { Key = "Settings.Customize.FontSizeNotSelected", Module = "Frontend", ZhCn = "请选择字体大小", EnUs = "Please select a font size", JaJp = "フォントサイズを選択してください" },
            new { Key = "Settings.Customize.FontSizeChanged", Module = "Frontend", ZhCn = "字体大小已切换为 {0}", EnUs = "Font size changed to {0}", JaJp = "フォントサイズが {0} に変更されました" },
            new { Key = "Settings.Customize.FontSizeChangeFailed", Module = "Frontend", ZhCn = "切换字体大小失败：{0}", EnUs = "Failed to change font size: {0}", JaJp = "フォントサイズの変更に失敗しました：{0}" },
            new { Key = "Settings.Customize.SaveSuccess", Module = "Frontend", ZhCn = "设置已保存成功", EnUs = "Settings saved successfully", JaJp = "設定が正常に保存されました" },
            new { Key = "Settings.Customize.RestartRequired", Module = "Frontend", ZhCn = "设置已保存成功。请重启应用程序以使更改生效。", EnUs = "Settings saved successfully. Please restart the application for changes to take effect.", JaJp = "設定が正常に保存されました。変更を反映するには、アプリケーションを再起動してください。" },
            
            // 主窗口菜单项
            new { Key = "MainWindow.UserInfoCenter", Module = "Frontend", ZhCn = "用户信息", EnUs = "User Info", JaJp = "ユーザー情報" },
            new { Key = "MainWindow.Logout", Module = "Frontend", ZhCn = "退出登录", EnUs = "Log out", JaJp = "ログアウト" },
            new { Key = "MainWindow.Logout.Confirm", Module = "Frontend", ZhCn = "确定要退出登录吗？", EnUs = "Are you sure you want to log out?", JaJp = "ログアウトしてもよろしいですか？" },
            new { Key = "MainWindow.Logout.ConfirmTitle", Module = "Frontend", ZhCn = "确认登出", EnUs = "Confirm Logout", JaJp = "ログアウトの確認" },
            
            // 菜单翻译（从菜单种子数据中的 I18nKey）
            new { Key = "menu.dashboard", Module = "Frontend", ZhCn = "仪表盘", EnUs = "Dashboard", JaJp = "ダッシュボード" },
            new { Key = "menu.logistics", Module = "Frontend", ZhCn = "后勤管理", EnUs = "Logistics", JaJp = "ロジスティクス" },
            new { Key = "menu.identity", Module = "Frontend", ZhCn = "身份认证", EnUs = "Identity", JaJp = "アイデンティティ" },
            new { Key = "menu.logging", Module = "Frontend", ZhCn = "日志管理", EnUs = "Logging", JaJp = "ログ管理" },
            new { Key = "menu.routine", Module = "Frontend", ZhCn = "日常事务", EnUs = "Routine", JaJp = "日常業務" },
            new { Key = "menu.about", Module = "Frontend", ZhCn = "关于", EnUs = "About", JaJp = "について" },
            new { Key = "menu.logistics.materials", Module = "Frontend", ZhCn = "物料管理", EnUs = "Materials", JaJp = "資材管理" },
            new { Key = "menu.logistics.materials.material", Module = "Frontend", ZhCn = "生产物料", EnUs = "Prod Material", JaJp = "生産資材" },
            new { Key = "menu.logistics.materials.model", Module = "Frontend", ZhCn = "机种仕向", EnUs = "Model", JaJp = "機種仕向" },
            new { Key = "menu.logistics.materials.packing", Module = "Frontend", ZhCn = "包装信息", EnUs = "Packing", JaJp = "包装情報" },
            new { Key = "menu.logistics.serials", Module = "Frontend", ZhCn = "序列号管理", EnUs = "Serial", JaJp = "シリアル管理" },
            new { Key = "menu.logistics.serials.inbound", Module = "Frontend", ZhCn = "序列号入库", EnUs = "Inbound", JaJp = "シリアル入庫" },
            new { Key = "menu.logistics.serials.outbound", Module = "Frontend", ZhCn = "序列号出库", EnUs = "Outbound", JaJp = "シリアル出庫" },
            new { Key = "menu.logistics.serials.scanning", Module = "Frontend", ZhCn = "序列号扫描", EnUs = "Scanning", JaJp = "シリアルスキャン" },
            new { Key = "menu.logistics.visitors", Module = "Frontend", ZhCn = "访客服务", EnUs = "Visitor Service", JaJp = "訪問者サービス" },
            new { Key = "menu.logistics.visitors.management", Module = "Frontend", ZhCn = "访客管理", EnUs = "Management", JaJp = "訪問者" },
            new { Key = "menu.logistics.visitors.signage", Module = "Frontend", ZhCn = "数字标牌", EnUs = "Signage", JaJp = "サイネージ" },
            new { Key = "menu.logistics.reports", Module = "Frontend", ZhCn = "报表管理", EnUs = "Report", JaJp = "レポート" },
            new { Key = "menu.logistics.reports.export", Module = "Frontend", ZhCn = "报表导出", EnUs = "Export", JaJp = "エクスポート" },
            new { Key = "menu.logistics.reports.import", Module = "Frontend", ZhCn = "报表导入", EnUs = "Import", JaJp = "インポート" },
            new { Key = "menu.routine.localization", Module = "Frontend", ZhCn = "本地化", EnUs = "Localization", JaJp = "現地化" },
            new { Key = "menu.routine.dictionary", Module = "Frontend", ZhCn = "字典", EnUs = "Dictionary", JaJp = "辞書" },
            new { Key = "menu.routine.setting", Module = "Frontend", ZhCn = "系统设置", EnUs = "Settings", JaJp = "システム設定" },
            new { Key = "menu.routine.quartz.job", Module = "Frontend", ZhCn = "任务管理", EnUs = "QuartzJob", JaJp = "タスク管理" },
            new { Key = "menu.identity.user", Module = "Frontend", ZhCn = "用户管理", EnUs = "User", JaJp = "ユーザー管理" },
            new { Key = "menu.identity.role", Module = "Frontend", ZhCn = "角色管理", EnUs = "Role", JaJp = "ロール管理" },
            new { Key = "menu.identity.menu", Module = "Frontend", ZhCn = "菜单管理", EnUs = "Menu", JaJp = "メニュー管理" },
            new { Key = "menu.logging.login", Module = "Frontend", ZhCn = "登录日志", EnUs = "Login", JaJp = "ログインログ" },
            new { Key = "menu.logging.oper", Module = "Frontend", ZhCn = "操作日志", EnUs = "Oper", JaJp = "操作ログ" },
            new { Key = "menu.logging.diff", Module = "Frontend", ZhCn = "差异日志", EnUs = "Diff", JaJp = "差分ログ" },
            new { Key = "menu.logging.quartz.log", Module = "Frontend", ZhCn = "任务日志", EnUs = "QuartzJob", JaJp = "タスクログ" },
            new { Key = "menu.generator", Module = "Frontend", ZhCn = "代码管理", EnUs = "Code", JaJp = "コード管理" },
            new { Key = "menu.generator.code", Module = "Frontend", ZhCn = "代码生成", EnUs = "Generator", JaJp = "コード生成" },
            new { Key = "menu.settings", Module = "Frontend", ZhCn = "设置", EnUs = "Settings", JaJp = "設定" },
            
            // 字典操作相关
            new { Key = "Routine.Dictionary.CreateType", Module = "Frontend", ZhCn = "新建字典类型", EnUs = "Create Dictionary Type", JaJp = "辞書タイプを作成" },
            new { Key = "Routine.Dictionary.UpdateType", Module = "Frontend", ZhCn = "编辑字典类型", EnUs = "Edit Dictionary Type", JaJp = "辞書タイプを編集" },
            new { Key = "Routine.Dictionary.CreateData", Module = "Frontend", ZhCn = "新建字典数据", EnUs = "Create Dictionary Data", JaJp = "辞書データを作成" },
            new { Key = "Routine.Dictionary.UpdateData", Module = "Frontend", ZhCn = "编辑字典数据", EnUs = "Edit Dictionary Data", JaJp = "辞書データを編集" },
            new { Key = "Routine.Dictionary.LoadTypesFailed", Module = "Frontend", ZhCn = "加载字典类型失败", EnUs = "Failed to load dictionary types", JaJp = "辞書タイプの読み込みに失敗しました" },
            new { Key = "Routine.Dictionary.LoadDataFailed", Module = "Frontend", ZhCn = "加载字典数据失败", EnUs = "Failed to load dictionary data", JaJp = "辞書データの読み込みに失敗しました" },
            new { Key = "Routine.Dictionary.DeleteTypeConfirm", Module = "Frontend", ZhCn = "确定要删除该字典类型吗？删除后关联的字典数据也会被删除。", EnUs = "Are you sure you want to delete this dictionary type? Associated dictionary data will also be deleted.", JaJp = "この辞書タイプを削除してもよろしいですか？関連する辞書データも削除されます。" },
            new { Key = "Routine.Dictionary.DeleteDataConfirm", Module = "Frontend", ZhCn = "确定要删除该字典数据吗？", EnUs = "Are you sure you want to delete this dictionary data?", JaJp = "この辞書データを削除してもよろしいですか？" },
            new { Key = "Routine.Dictionary.DataLabelRequired", Module = "Frontend", ZhCn = "数据标签不能为空", EnUs = "Data label cannot be empty", JaJp = "データラベルは空にできません" },
            new { Key = "Routine.Dictionary.I18nKeyRequired", Module = "Frontend", ZhCn = "国际化键不能为空", EnUs = "I18n key cannot be empty", JaJp = "多言語キーは空にできません" },
            new { Key = "Routine.Dictionary.PleaseSaveOrCancelData", Module = "Frontend", ZhCn = "请先保存或取消正在编辑的字典数据", EnUs = "Please save or cancel the dictionary data being edited", JaJp = "編集中の辞書データを保存またはキャンセルしてください" },
            new { Key = "Routine.Dictionary.DataFieldsRequired", Module = "Frontend", ZhCn = "数据标签、国际化键不能为空", EnUs = "Data label and I18n key cannot be empty", JaJp = "データラベルと多言語キーは空にできません" },
            
            // 字典验证相关（仅保留特定业务规则，通用验证使用 validation.required/format/maxLength/minLength/invalid）
            new { Key = "Routine.Dictionary.Validation.TypeCodeRequired", Module = "Frontend", ZhCn = "类型代码不能为空", EnUs = "Type code cannot be empty", JaJp = "タイプコードは空にできません" },
            new { Key = "Routine.Dictionary.Validation.TypeCodeMaxLength", Module = "Frontend", ZhCn = "类型代码长度不能超过50个字符", EnUs = "Type code cannot exceed 50 characters", JaJp = "タイプコードは50文字を超えることはできません" },
            new { Key = "Routine.Dictionary.Validation.TypeCodeFormat", Module = "Frontend", ZhCn = "类型代码格式不正确，必须是 xxx_xxx_xxx 格式，只包含小写字母和数字，且不能以数字开头", EnUs = "Type code format is invalid, must be in xxx_xxx_xxx format, containing only lowercase letters and numbers, and cannot start with a number", JaJp = "タイプコードの形式が無効です。xxx_xxx_xxx 形式である必要があり、小文字と数字のみを含み、数字で始めることはできません" },
            new { Key = "Routine.Dictionary.Validation.TypeNameRequired", Module = "Frontend", ZhCn = "类型名称不能为空", EnUs = "Type name cannot be empty", JaJp = "タイプ名は空にできません" },
            new { Key = "Routine.Dictionary.Validation.TypeNameMaxLength", Module = "Frontend", ZhCn = "类型名称长度不能超过100个字符", EnUs = "Type name cannot exceed 100 characters", JaJp = "タイプ名は100文字を超えることはできません" },
            new { Key = "Routine.Dictionary.Validation.TypeStatusInvalid", Module = "Frontend", ZhCn = "类型状态无效，必须是0或1", EnUs = "Type status is invalid, must be 0 or 1", JaJp = "タイプ状態が無効です。0または1である必要があります" },
            new { Key = "Routine.Dictionary.Validation.DataLabelRequired", Module = "Frontend", ZhCn = "数据标签不能为空", EnUs = "Data label cannot be empty", JaJp = "データラベルは空にできません" },
            new { Key = "Routine.Dictionary.Validation.DataLabelMaxLength", Module = "Frontend", ZhCn = "数据标签长度不能超过100个字符", EnUs = "Data label cannot exceed 100 characters", JaJp = "データラベルは100文字を超えることはできません" },
            new { Key = "Routine.Dictionary.Validation.I18nKeyRequired", Module = "Frontend", ZhCn = "国际化键不能为空", EnUs = "I18n key cannot be empty", JaJp = "多言語キーは空にできません" },
            new { Key = "Routine.Dictionary.Validation.I18nKeyMaxLength", Module = "Frontend", ZhCn = "国际化键长度不能超过64个字符", EnUs = "I18n key cannot exceed 64 characters", JaJp = "多言語キーは64文字を超えることはできません" },
            new { Key = "Routine.Dictionary.Validation.SqlScriptRequired", Module = "Frontend", ZhCn = "SQL脚本不能为空", EnUs = "SQL script cannot be empty", JaJp = "SQLスクリプトは空にできません" },
            
            // 系统设置操作相关
            new { Key = "Routine.Setting.Create", Module = "Frontend", ZhCn = "新建系统设置", EnUs = "Create System Setting", JaJp = "システム設定を作成" },
            new { Key = "Routine.Setting.Update", Module = "Frontend", ZhCn = "编辑系统设置", EnUs = "Edit System Setting", JaJp = "システム設定を編集" },
            
            // 系统设置验证相关
            new { Key = "Routine.Setting.Validation.KeyRequired", Module = "Frontend", ZhCn = "设置键不能为空", EnUs = "Setting key cannot be empty", JaJp = "設定キーは空にできません" },
            new { Key = "Routine.Setting.Validation.KeyMaxLength", Module = "Frontend", ZhCn = "设置键长度不能超过100个字符", EnUs = "Setting key cannot exceed 100 characters", JaJp = "設定キーは100文字を超えることはできません" },
            new { Key = "Routine.Setting.Validation.KeyInvalid", Module = "Frontend", ZhCn = "设置键只能包含字母、数字、点号、下划线和连字符", EnUs = "Setting key can only contain letters, numbers, dots, underscores and hyphens", JaJp = "設定キーには、文字、数字、ピリオド、アンダースコア、ハイフンのみを含めることができます" },
            new { Key = "Routine.Setting.Validation.ValueRequired", Module = "Frontend", ZhCn = "设置值不能为空", EnUs = "Setting value cannot be empty", JaJp = "設定値は空にできません" },
            new { Key = "Routine.Setting.Validation.ValueMaxLength", Module = "Frontend", ZhCn = "设置值长度不能超过2000个字符", EnUs = "Setting value cannot exceed 2000 characters", JaJp = "設定値は2000文字を超えることはできません" },
            new { Key = "Routine.Setting.Validation.CategoryMaxLength", Module = "Frontend", ZhCn = "分类长度不能超过50个字符", EnUs = "Category cannot exceed 50 characters", JaJp = "カテゴリーは50文字を超えることはできません" },
            new { Key = "Routine.Setting.Validation.DescriptionMaxLength", Module = "Frontend", ZhCn = "设置描述长度不能超过500个字符", EnUs = "Setting description cannot exceed 500 characters", JaJp = "設定説明は500文字を超えることはできません" },
            new { Key = "Routine.Setting.Validation.TypeInvalid", Module = "Frontend", ZhCn = "设置类型无效，必须是0-3之间的值", EnUs = "Setting type is invalid, must be a value between 0-3", JaJp = "設定タイプが無効です。0-3の間の値である必要があります" },
            
            // 访客操作相关
            // 数字标牌相关
            new { Key = "Logistics.DigitalSignage.Header", Module = "Frontend", ZhCn = "热烈欢迎", EnUs = "Warm Welcome", JaJp = "心より歓迎" },
            new { Key = "Logistics.DigitalSignage.Footer", Module = "Frontend", ZhCn = "莅临指导", EnUs = "Visit and Guidance", JaJp = "ご来訪とご指導" },
            new { Key = "Logistics.DigitalSignage.Welcome", Module = "Frontend", ZhCn = "欢迎", EnUs = "Welcome", JaJp = "ようこそ" },
            
            new { Key = "Logistics.Visitors.Create", Module = "Frontend", ZhCn = "新建访客", EnUs = "Create Visitor", JaJp = "訪問者を作成" },
            new { Key = "Logistics.Visitors.Update", Module = "Frontend", ZhCn = "编辑访客", EnUs = "Edit Visitor", JaJp = "訪問者を編集" },
            new { Key = "Logistics.Visitors.PleaseSaveOrCancelDetail", Module = "Frontend", ZhCn = "请先保存或取消正在编辑的访客详情", EnUs = "Please save or cancel the visitor detail being edited", JaJp = "編集中の訪問者詳細を保存またはキャンセルしてください" },
            new { Key = "Logistics.Visitors.CreateFailed", Module = "Frontend", ZhCn = "创建访客失败", EnUs = "Failed to create visitor", JaJp = "訪問者の作成に失敗しました" },
            new { Key = "Logistics.Visitors.UpdateFailed", Module = "Frontend", ZhCn = "更新访客失败", EnUs = "Failed to update visitor", JaJp = "訪問者の更新に失敗しました" },
            new { Key = "Logistics.Visitors.DetailFieldsRequired", Module = "Frontend", ZhCn = "部门、姓名、职务不能为空", EnUs = "Department, name, and position cannot be empty", JaJp = "部署、氏名、職務は空にできません" },
            
            // 访客验证相关（仅保留特定业务规则，通用验证使用 validation.required/format/maxLength/minLength/invalid）
            new { Key = "Logistics.Visitors.Validation.CompanyNameRequired", Module = "Frontend", ZhCn = "公司名称不能为空", EnUs = "Company name cannot be empty", JaJp = "会社名は空にできません" },
            new { Key = "Logistics.Visitors.Validation.CompanyNameMaxLength", Module = "Frontend", ZhCn = "公司名称长度不能超过200个字符", EnUs = "Company name cannot exceed 200 characters", JaJp = "会社名は200文字を超えることはできません" },
            new { Key = "Logistics.Visitors.Validation.CompanyNameInvalid", Module = "Frontend", ZhCn = "公司名称不能为空，长度不能超过200个字符", EnUs = "Company name cannot be empty and cannot exceed 200 characters", JaJp = "会社名は空にできず、200文字を超えることはできません" },
            new { Key = "Logistics.Visitors.Validation.StartTimeInvalid", Module = "Frontend", ZhCn = "开始时间不能为空，且必须早于结束时间", EnUs = "Start time cannot be empty and must be before end time", JaJp = "開始時間は空にできず、終了時間より前である必要があります" },
            new { Key = "Logistics.Visitors.Validation.EndTimeInvalid", Module = "Frontend", ZhCn = "结束时间不能为空，且必须晚于开始时间", EnUs = "End time cannot be empty and must be after start time", JaJp = "終了時間は空にできず、開始時間より後である必要があります" },
            new { Key = "Logistics.Visitors.Validation.StartTimeMustBeforeEndTime", Module = "Frontend", ZhCn = "开始时间必须早于结束时间", EnUs = "Start time must be before end time", JaJp = "開始時間は終了時間より前である必要があります" },
            new { Key = "Logistics.Visitors.Validation.EndTimeMustAfterStartTime", Module = "Frontend", ZhCn = "结束时间必须晚于开始时间", EnUs = "End time must be after start time", JaJp = "終了時間は開始時間より後である必要があります" },
            
            // 产品序列号主表相关
            new { Key = "Logistics.Serials.ProdSerial.DestCode", Module = "Frontend", ZhCn = "仕向编码", EnUs = "Destination Code", JaJp = "仕向コード" },
            new { Key = "Logistics.Serials.ProdSerial.DestCodeHint", Module = "Frontend", ZhCn = "请选择或输入仕向编码", EnUs = "Please select or enter destination code", JaJp = "仕向コードを選択または入力してください" },
            
            // 序列号入库操作相关
            new { Key = "Logistics.Serials.ProdSerialInbound.ScanInbound", Module = "Frontend", ZhCn = "扫描入库", EnUs = "Scan Inbound", JaJp = "スキャン入庫" },
            new { Key = "Logistics.Serials.ProdSerialInbound.FullSerialNumber", Module = "Frontend", ZhCn = "完整序列号", EnUs = "Full Serial Number", JaJp = "完全シリアル番号" },
            new { Key = "Logistics.Serials.ProdSerialInbound.FullSerialNumberHint", Module = "Frontend", ZhCn = "请扫描或输入完整序列号", EnUs = "Please scan or enter the full serial number", JaJp = "完全シリアル番号をスキャンまたは入力してください" },
            new { Key = "Logistics.Serials.ProdSerialInbound.CreateFailed", Module = "Frontend", ZhCn = "创建入库记录失败", EnUs = "Failed to create inbound record", JaJp = "入庫記録の作成に失敗しました" },
            
            // 序列号入库验证相关（仅保留特定业务规则，通用验证使用 validation.required/format/maxLength/minLength/invalid）
            new { Key = "Logistics.Serials.ProdSerialInbound.Validation.FullSerialNumberRequired", Module = "Frontend", ZhCn = "完整序列号不能为空", EnUs = "Full serial number cannot be empty", JaJp = "完全シリアル番号は空にできません" },
            new { Key = "Logistics.Serials.ProdSerialInbound.Validation.FullSerialNumberMaxLength", Module = "Frontend", ZhCn = "完整序列号长度不能超过200个字符", EnUs = "Full serial number cannot exceed 200 characters", JaJp = "完全シリアル番号は200文字を超えることはできません" },
            
            // 序列号出库操作相关
            new { Key = "Logistics.Serials.ProdSerialOutbound.ScanOutbound", Module = "Frontend", ZhCn = "扫描出库", EnUs = "Scan Outbound", JaJp = "スキャン出庫" },
            new { Key = "Logistics.Serials.ProdSerialOutbound.OutboundNo", Module = "Frontend", ZhCn = "出库单号", EnUs = "Outbound No.", JaJp = "出庫番号" },
            new { Key = "Logistics.Serials.ProdSerialOutbound.OutboundDate", Module = "Frontend", ZhCn = "出库日期", EnUs = "Outbound Date", JaJp = "出庫日" },
            new { Key = "Logistics.Serials.ProdSerialOutbound.OutboundDateHint", Module = "Frontend", ZhCn = "请选择出库日期", EnUs = "Please select the outbound date", JaJp = "出庫日を選択してください" },
            new { Key = "Logistics.Serials.ProdSerialOutbound.DestPort", Module = "Frontend", ZhCn = "目的地港口", EnUs = "Destination Port", JaJp = "目的地港" },
            new { Key = "Logistics.Serials.ProdSerialOutbound.DestPortHint", Module = "Frontend", ZhCn = "请输入目的地港口（可选）", EnUs = "Please enter destination port (optional)", JaJp = "目的地港を入力してください（オプション）" },
            new { Key = "Logistics.Serials.ProdSerialOutbound.FullSerialNumber", Module = "Frontend", ZhCn = "完整序列号", EnUs = "Full Serial Number", JaJp = "完全シリアル番号" },
            new { Key = "Logistics.Serials.ProdSerialOutbound.FullSerialNumberHint", Module = "Frontend", ZhCn = "请扫描或输入完整序列号", EnUs = "Please scan or enter the full serial number", JaJp = "完全シリアル番号をスキャンまたは入力してください" },
            new { Key = "Logistics.Serials.ProdSerialOutbound.CreateFailed", Module = "Frontend", ZhCn = "创建出库记录失败", EnUs = "Failed to create outbound record", JaJp = "出庫記録の作成に失敗しました" },
            
            // 序列号出库验证相关（仅保留特定业务规则，通用验证使用 validation.required/format/maxLength/minLength/invalid）
            new { Key = "Logistics.Serials.ProdSerialOutbound.Validation.OutboundNoRequired", Module = "Frontend", ZhCn = "出库单号不能为空", EnUs = "Outbound number cannot be empty", JaJp = "出庫番号は空にできません" },
            new { Key = "Logistics.Serials.ProdSerialOutbound.Validation.OutboundNoMaxLength", Module = "Frontend", ZhCn = "出库单号长度不能超过50个字符", EnUs = "Outbound number cannot exceed 50 characters", JaJp = "出庫番号は50文字を超えることはできません" },
            new { Key = "Logistics.Serials.ProdSerialOutbound.Validation.OutboundDateRequired", Module = "Frontend", ZhCn = "出库日期不能为空", EnUs = "Outbound date cannot be empty", JaJp = "出庫日は空にできません" },
            new { Key = "Logistics.Serials.ProdSerialOutbound.Validation.DestPortMaxLength", Module = "Frontend", ZhCn = "目的地港口长度不能超过100个字符", EnUs = "Destination port cannot exceed 100 characters", JaJp = "目的地港は100文字を超えることはできません" },
            new { Key = "Logistics.Serials.ProdSerialOutbound.Validation.FullSerialNumberRequired", Module = "Frontend", ZhCn = "完整序列号不能为空", EnUs = "Full serial number cannot be empty", JaJp = "完全シリアル番号は空にできません" },
            new { Key = "Logistics.Serials.ProdSerialOutbound.Validation.FullSerialNumberMaxLength", Module = "Frontend", ZhCn = "完整序列号长度不能超过200个字符", EnUs = "Full serial number cannot exceed 200 characters", JaJp = "完全シリアル番号は200文字を超えることはできません" },
            
            // 序列号扫描操作相关
            new { Key = "Logistics.Serials.ProdSerialScanning.ScanningKeyword", Module = "Frontend", ZhCn = "扫描关键词", EnUs = "Scanning Keyword", JaJp = "スキャンキーワード" },
            new { Key = "Logistics.Serials.ProdSerialScanning.InboundFullSerialNumber", Module = "Frontend", ZhCn = "入库完整序列号", EnUs = "Inbound Full Serial Number", JaJp = "入庫完全シリアル番号" },
            new { Key = "Logistics.Serials.ProdSerialScanning.InboundDate", Module = "Frontend", ZhCn = "入库日期", EnUs = "Inbound Date", JaJp = "入庫日" },
            new { Key = "Logistics.Serials.ProdSerialScanning.InboundIp", Module = "Frontend", ZhCn = "入库IP", EnUs = "Inbound IP", JaJp = "入庫IP" },
            new { Key = "Logistics.Serials.ProdSerialScanning.InboundMachineName", Module = "Frontend", ZhCn = "入库机器名称", EnUs = "Inbound Machine Name", JaJp = "入庫マシン名" },
            new { Key = "Logistics.Serials.ProdSerialScanning.OutboundFullSerialNumber", Module = "Frontend", ZhCn = "出库完整序列号", EnUs = "Outbound Full Serial Number", JaJp = "出庫完全シリアル番号" },
            new { Key = "Logistics.Serials.ProdSerialScanning.OutboundDate", Module = "Frontend", ZhCn = "出库日期", EnUs = "Outbound Date", JaJp = "出庫日" },
            new { Key = "Logistics.Serials.ProdSerialScanning.OutboundIp", Module = "Frontend", ZhCn = "出库IP", EnUs = "Outbound IP", JaJp = "出庫IP" },
            new { Key = "Logistics.Serials.ProdSerialScanning.OutboundMachineName", Module = "Frontend", ZhCn = "出库机器名称", EnUs = "Outbound Machine Name", JaJp = "出庫マシン名" },
            new { Key = "Logistics.Serials.ProdSerialScanning.LoadFailed", Module = "Frontend", ZhCn = "加载序列号扫描记录失败", EnUs = "Failed to load serial scanning records", JaJp = "シリアルスキャン記録の読み込みに失敗しました" },
            new { Key = "Logistics.Serials.ProdSerialOutbound.Validation.SerialNotInbound", Module = "Frontend", ZhCn = "该序列号尚未入库，无法出库", EnUs = "This serial number has not been inbound, cannot outbound", JaJp = "このシリアル番号はまだ入庫されていません。出庫できません" },
            
            // 字典数据翻译种子（dict. 开头）
            // sys_common_gender
            new { Key = "dict.sys_common_gender.male", Module = "Frontend", ZhCn = "男", EnUs = "Male", JaJp = "男性" },
            new { Key = "dict.sys_common_gender.female", Module = "Frontend", ZhCn = "女", EnUs = "Female", JaJp = "女性" },
            new { Key = "dict.sys_common_gender.unknown", Module = "Frontend", ZhCn = "未知", EnUs = "Unknown", JaJp = "不明" },
            
            // sys_common_yes_no
            new { Key = "dict.sys_common_yes_no.yes", Module = "Frontend", ZhCn = "是", EnUs = "Yes", JaJp = "はい" },
            new { Key = "dict.sys_common_yes_no.no", Module = "Frontend", ZhCn = "否", EnUs = "No", JaJp = "いいえ" },
            
            // sys_common_status
            new { Key = "dict.sys_common_status.normal", Module = "Frontend", ZhCn = "正常", EnUs = "Normal", JaJp = "正常" },
            new { Key = "dict.sys_common_status.disabled", Module = "Frontend", ZhCn = "禁用", EnUs = "Disabled", JaJp = "無効" },
            
            // sys_user_type
            new { Key = "dict.sys_user_type.Takt365", Module = "Frontend", ZhCn = "系统用户", EnUs = "System User", JaJp = "システムユーザー" },
            new { Key = "dict.sys_user_type.normal", Module = "Frontend", ZhCn = "普通用户", EnUs = "Normal User", JaJp = "一般ユーザー" },
            
            // sys_data_scope
            new { Key = "dict.sys_data_scope.all", Module = "Frontend", ZhCn = "全部数据", EnUs = "All Data", JaJp = "全データ" },
            new { Key = "dict.sys_data_scope.custom", Module = "Frontend", ZhCn = "自定义数据", EnUs = "Custom Data", JaJp = "カスタムデータ" },
            new { Key = "dict.sys_data_scope.department", Module = "Frontend", ZhCn = "本部门数据", EnUs = "Department Data", JaJp = "部門データ" },
            new { Key = "dict.sys_data_scope.department_below", Module = "Frontend", ZhCn = "本部门及以下数据", EnUs = "Department and Below Data", JaJp = "部門および下位データ" },
            new { Key = "dict.sys_data_scope.self", Module = "Frontend", ZhCn = "仅本人数据", EnUs = "Self Data Only", JaJp = "本人データのみ" },
            
            // sys_dict_data_source
            new { Key = "dict.sys_dict_data_source.system", Module = "Frontend", ZhCn = "系统", EnUs = "System", JaJp = "システム" },
            new { Key = "dict.sys_dict_data_source.sql_script", Module = "Frontend", ZhCn = "SQL脚本", EnUs = "SQL Script", JaJp = "SQLスクリプト" },
            
            // sys_dict_is_builtin
            new { Key = "dict.sys_dict_is_builtin.yes", Module = "Frontend", ZhCn = "是", EnUs = "Yes", JaJp = "はい" },
            new { Key = "dict.sys_dict_is_builtin.no", Module = "Frontend", ZhCn = "否", EnUs = "No", JaJp = "いいえ" },
            
            // menu_link_external
            new { Key = "dict.menu_link_external.external", Module = "Frontend", ZhCn = "外链", EnUs = "External Link", JaJp = "外部リンク" },
            new { Key = "dict.menu_link_external.not_external", Module = "Frontend", ZhCn = "不是外链", EnUs = "Not External Link", JaJp = "外部リンクではない" },
            
            // menu_cache_enable
            new { Key = "dict.menu_cache_enable.cache", Module = "Frontend", ZhCn = "缓存", EnUs = "Cache", JaJp = "キャッシュ" },
            new { Key = "dict.menu_cache_enable.no_cache", Module = "Frontend", ZhCn = "不缓存", EnUs = "No Cache", JaJp = "キャッシュなし" },
            
            // menu_visible_enable
            new { Key = "dict.menu_visible_enable.visible", Module = "Frontend", ZhCn = "可见", EnUs = "Visible", JaJp = "表示" },
            new { Key = "dict.menu_visible_enable.invisible", Module = "Frontend", ZhCn = "不可见", EnUs = "Invisible", JaJp = "非表示" },
            
            // menu_type_category
            new { Key = "dict.menu_type_category.directory", Module = "Frontend", ZhCn = "目录", EnUs = "Directory", JaJp = "ディレクトリ" },
            new { Key = "dict.menu_type_category.menu", Module = "Frontend", ZhCn = "菜单", EnUs = "Menu", JaJp = "メニュー" },
            new { Key = "dict.menu_type_category.button", Module = "Frontend", ZhCn = "按钮", EnUs = "Button", JaJp = "ボタン" },
            new { Key = "dict.menu_type_category.api", Module = "Frontend", ZhCn = "API", EnUs = "API", JaJp = "API" },
            
            // setting_value_type
            new { Key = "dict.setting_value_type.string", Module = "Frontend", ZhCn = "字符串", EnUs = "String", JaJp = "文字列" },
            new { Key = "dict.setting_value_type.number", Module = "Frontend", ZhCn = "数字", EnUs = "Number", JaJp = "数値" },
            new { Key = "dict.setting_value_type.boolean", Module = "Frontend", ZhCn = "布尔值", EnUs = "Boolean", JaJp = "ブール値" },
            new { Key = "dict.setting_value_type.json", Module = "Frontend", ZhCn = "JSON", EnUs = "JSON", JaJp = "JSON" },
            
            // setting_is_default
            new { Key = "dict.setting_is_default.yes", Module = "Frontend", ZhCn = "是", EnUs = "Yes", JaJp = "はい" },
            new { Key = "dict.setting_is_default.no", Module = "Frontend", ZhCn = "否", EnUs = "No", JaJp = "いいえ" },
            
            // setting_is_editable
            new { Key = "dict.setting_is_editable.yes", Module = "Frontend", ZhCn = "是", EnUs = "Yes", JaJp = "はい" },
            new { Key = "dict.setting_is_editable.no", Module = "Frontend", ZhCn = "否", EnUs = "No", JaJp = "いいえ" },
            
            // sys_ui_css_class
            new { Key = "dict.sys_ui_css_class.primary", Module = "Frontend", ZhCn = "主要", EnUs = "Primary", JaJp = "プライマリ" },
            new { Key = "dict.sys_ui_css_class.success", Module = "Frontend", ZhCn = "成功", EnUs = "Success", JaJp = "成功" },
            new { Key = "dict.sys_ui_css_class.info", Module = "Frontend", ZhCn = "信息", EnUs = "Info", JaJp = "情報" },
            new { Key = "dict.sys_ui_css_class.warning", Module = "Frontend", ZhCn = "警告", EnUs = "Warning", JaJp = "警告" },
            new { Key = "dict.sys_ui_css_class.danger", Module = "Frontend", ZhCn = "危险", EnUs = "Danger", JaJp = "危険" },
            new { Key = "dict.sys_ui_css_class.secondary", Module = "Frontend", ZhCn = "次要", EnUs = "Secondary", JaJp = "セカンダリ" },
            new { Key = "dict.sys_ui_css_class.light", Module = "Frontend", ZhCn = "浅色", EnUs = "Light", JaJp = "ライト" },
            new { Key = "dict.sys_ui_css_class.dark", Module = "Frontend", ZhCn = "深色", EnUs = "Dark", JaJp = "ダーク" },
            
            // sys_module_name
            new { Key = "dict.sys_module_name.Frontend", Module = "Frontend", ZhCn = "前端", EnUs = "Frontend", JaJp = "フロントエンド" },
            new { Key = "dict.sys_module_name.Backend", Module = "Frontend", ZhCn = "后端", EnUs = "Backend", JaJp = "バックエンド" },
            new { Key = "dict.sys_module_name.Mobile", Module = "Frontend", ZhCn = "移动端", EnUs = "Mobile", JaJp = "モバイル" },
            
            // setting_category_type
            new { Key = "dict.setting_category_type.system", Module = "Frontend", ZhCn = "系统设置", EnUs = "System Settings", JaJp = "システム設定" },
            new { Key = "dict.setting_category_type.user", Module = "Frontend", ZhCn = "用户设置", EnUs = "User Settings", JaJp = "ユーザー設定" },
            new { Key = "dict.setting_category_type.appearance", Module = "Frontend", ZhCn = "外观设置", EnUs = "Appearance Settings", JaJp = "外観設定" },
            new { Key = "dict.setting_category_type.security", Module = "Frontend", ZhCn = "安全设置", EnUs = "Security Settings", JaJp = "セキュリティ設定" },
            new { Key = "dict.setting_category_type.notification", Module = "Frontend", ZhCn = "通知设置", EnUs = "Notification Settings", JaJp = "通知設定" },
            
            // material_plant_code
            new { Key = "dict.material_plant_code.1000", Module = "Frontend", ZhCn = "工厂1000", EnUs = "Plant 1000", JaJp = "工場1000" },
            new { Key = "dict.material_plant_code.2000", Module = "Frontend", ZhCn = "工厂2000", EnUs = "Plant 2000", JaJp = "工場2000" },
            new { Key = "dict.material_plant_code.3000", Module = "Frontend", ZhCn = "工厂3000", EnUs = "Plant 3000", JaJp = "工場3000" },
            
            // material_industry_field
            new { Key = "dict.material_industry_field.M", Module = "Frontend", ZhCn = "机械制造", EnUs = "Machinery Manufacturing", JaJp = "機械製造" },
            new { Key = "dict.material_industry_field.E", Module = "Frontend", ZhCn = "电子制造", EnUs = "Electronics Manufacturing", JaJp = "電子製造" },
            new { Key = "dict.material_industry_field.C", Module = "Frontend", ZhCn = "化工", EnUs = "Chemical", JaJp = "化学" },
            new { Key = "dict.material_industry_field.F", Module = "Frontend", ZhCn = "食品", EnUs = "Food", JaJp = "食品" },
            
            // material_type_category
            new { Key = "dict.material_type_category.FERT", Module = "Frontend", ZhCn = "成品", EnUs = "Finished Product", JaJp = "完成品" },
            new { Key = "dict.material_type_category.HALB", Module = "Frontend", ZhCn = "半成品", EnUs = "Semi-Finished Product", JaJp = "半完成品" },
            new { Key = "dict.material_type_category.ROH", Module = "Frontend", ZhCn = "原材料", EnUs = "Raw Material", JaJp = "原材料" },
            new { Key = "dict.material_type_category.HIBE", Module = "Frontend", ZhCn = "贸易商品", EnUs = "Trading Goods", JaJp = "貿易商品" },
            new { Key = "dict.material_type_category.NLAG", Module = "Frontend", ZhCn = "非库存物料", EnUs = "Non-Stock Material", JaJp = "在庫外資材" },
            
            // material_base_unit
            new { Key = "dict.material_base_unit.PC", Module = "Frontend", ZhCn = "件", EnUs = "Piece", JaJp = "個" },
            new { Key = "dict.material_base_unit.KG", Module = "Frontend", ZhCn = "千克", EnUs = "Kilogram", JaJp = "キログラム" },
            new { Key = "dict.material_base_unit.M", Module = "Frontend", ZhCn = "米", EnUs = "Meter", JaJp = "メートル" },
            new { Key = "dict.material_base_unit.M2", Module = "Frontend", ZhCn = "平方米", EnUs = "Square Meter", JaJp = "平方メートル" },
            new { Key = "dict.material_base_unit.M3", Module = "Frontend", ZhCn = "立方米", EnUs = "Cubic Meter", JaJp = "立方メートル" },
            new { Key = "dict.material_base_unit.L", Module = "Frontend", ZhCn = "升", EnUs = "Liter", JaJp = "リットル" },
            new { Key = "dict.material_base_unit.BOX", Module = "Frontend", ZhCn = "箱", EnUs = "Box", JaJp = "箱" },
            new { Key = "dict.material_base_unit.PCS", Module = "Frontend", ZhCn = "个", EnUs = "Pieces", JaJp = "個" },
            
            // material_group_code
            new { Key = "dict.material_group_code.001", Module = "Frontend", ZhCn = "原材料组", EnUs = "Raw Material Group", JaJp = "原材料グループ" },
            new { Key = "dict.material_group_code.002", Module = "Frontend", ZhCn = "半成品组", EnUs = "Semi-Finished Product Group", JaJp = "半完成品グループ" },
            new { Key = "dict.material_group_code.003", Module = "Frontend", ZhCn = "成品组", EnUs = "Finished Product Group", JaJp = "完成品グループ" },
            new { Key = "dict.material_group_code.004", Module = "Frontend", ZhCn = "包装材料组", EnUs = "Packaging Material Group", JaJp = "包装材料グループ" },
            new { Key = "dict.material_group_code.005", Module = "Frontend", ZhCn = "辅助材料组", EnUs = "Auxiliary Material Group", JaJp = "補助材料グループ" },
            
            // material_purchase_group
            new { Key = "dict.material_purchase_group.001", Module = "Frontend", ZhCn = "采购组001", EnUs = "Purchase Group 001", JaJp = "購買グループ001" },
            new { Key = "dict.material_purchase_group.002", Module = "Frontend", ZhCn = "采购组002", EnUs = "Purchase Group 002", JaJp = "購買グループ002" },
            new { Key = "dict.material_purchase_group.003", Module = "Frontend", ZhCn = "采购组003", EnUs = "Purchase Group 003", JaJp = "購買グループ003" },
            
            // material_purchase_type
            new { Key = "dict.material_purchase_type.E", Module = "Frontend", ZhCn = "外部采购", EnUs = "External Purchase", JaJp = "外部調達" },
            new { Key = "dict.material_purchase_type.F", Module = "Frontend", ZhCn = "自制", EnUs = "In-House Production", JaJp = "自社製造" },
            new { Key = "dict.material_purchase_type.X", Module = "Frontend", ZhCn = "两者皆可", EnUs = "Both", JaJp = "両方" },
            
            // material_special_purchase
            new { Key = "dict.material_special_purchase.10", Module = "Frontend", ZhCn = "标准采购", EnUs = "Standard Purchase", JaJp = "標準調達" },
            new { Key = "dict.material_special_purchase.20", Module = "Frontend", ZhCn = "寄售", EnUs = "Consignment", JaJp = "委託販売" },
            new { Key = "dict.material_special_purchase.30", Module = "Frontend", ZhCn = "分包", EnUs = "Subcontracting", JaJp = "外注" },
            new { Key = "dict.material_special_purchase.40", Module = "Frontend", ZhCn = "第三方", EnUs = "Third Party", JaJp = "第三者" },
            
            // material_bulk_type
            new { Key = "dict.material_bulk_type.X", Module = "Frontend", ZhCn = "散装物料", EnUs = "Bulk Material", JaJp = "バルク資材" },
            new { Key = "dict.material_bulk_type.space", Module = "Frontend", ZhCn = "非散装物料", EnUs = "Non-Bulk Material", JaJp = "非バルク資材" },
            
            // material_inspection_stock
            new { Key = "dict.material_inspection_stock.X", Module = "Frontend", ZhCn = "过账到检验库存", EnUs = "Post to Inspection Stock", JaJp = "検査在庫に転記" },
            new { Key = "dict.material_inspection_stock.space", Module = "Frontend", ZhCn = "不过账到检验库存", EnUs = "Do Not Post to Inspection Stock", JaJp = "検査在庫に転記しない" },
            
            // material_profit_center
            new { Key = "dict.material_profit_center.1000", Module = "Frontend", ZhCn = "利润中心1000", EnUs = "Profit Center 1000", JaJp = "利益センター1000" },
            new { Key = "dict.material_profit_center.2000", Module = "Frontend", ZhCn = "利润中心2000", EnUs = "Profit Center 2000", JaJp = "利益センター2000" },
            new { Key = "dict.material_profit_center.3000", Module = "Frontend", ZhCn = "利润中心3000", EnUs = "Profit Center 3000", JaJp = "利益センター3000" },
            
            // material_batch_management
            new { Key = "dict.material_batch_management.X", Module = "Frontend", ZhCn = "批次管理", EnUs = "Batch Management", JaJp = "ロット管理" },
            new { Key = "dict.material_batch_management.space", Module = "Frontend", ZhCn = "非批次管理", EnUs = "Non-Batch Management", JaJp = "非ロット管理" },
            
            // material_evaluation_type
            new { Key = "dict.material_evaluation_type.V", Module = "Frontend", ZhCn = "移动平均", EnUs = "Moving Average", JaJp = "移動平均" },
            new { Key = "dict.material_evaluation_type.S", Module = "Frontend", ZhCn = "标准价格", EnUs = "Standard Price", JaJp = "標準価格" },
            new { Key = "dict.material_evaluation_type.L", Module = "Frontend", ZhCn = "期间单位价格", EnUs = "Period Unit Price", JaJp = "期間単位価格" },
            
            // material_currency_code
            new { Key = "dict.material_currency_code.CNY", Module = "Frontend", ZhCn = "人民币", EnUs = "Chinese Yuan", JaJp = "人民元" },
            new { Key = "dict.material_currency_code.USD", Module = "Frontend", ZhCn = "美元", EnUs = "US Dollar", JaJp = "米ドル" },
            new { Key = "dict.material_currency_code.EUR", Module = "Frontend", ZhCn = "欧元", EnUs = "Euro", JaJp = "ユーロ" },
            new { Key = "dict.material_currency_code.JPY", Module = "Frontend", ZhCn = "日元", EnUs = "Japanese Yen", JaJp = "日本円" },
            new { Key = "dict.material_currency_code.HKD", Module = "Frontend", ZhCn = "港币", EnUs = "Hong Kong Dollar", JaJp = "香港ドル" },
            
            // material_price_control
            new { Key = "dict.material_price_control.V", Module = "Frontend", ZhCn = "移动平均", EnUs = "Moving Average", JaJp = "移動平均" },
            new { Key = "dict.material_price_control.S", Module = "Frontend", ZhCn = "标准价格", EnUs = "Standard Price", JaJp = "標準価格" },
            
            // material_cross_plant_status
            new { Key = "dict.material_cross_plant_status.01", Module = "Frontend", ZhCn = "已释放", EnUs = "Released", JaJp = "リリース済み" },
            new { Key = "dict.material_cross_plant_status.02", Module = "Frontend", ZhCn = "已锁定", EnUs = "Locked", JaJp = "ロック済み" },
            new { Key = "dict.material_cross_plant_status.03", Module = "Frontend", ZhCn = "已归档", EnUs = "Archived", JaJp = "アーカイブ済み" },
            
            // material_variance_code
            new { Key = "dict.material_variance_code.PPV", Module = "Frontend", ZhCn = "采购价格差异", EnUs = "Purchase Price Variance", JaJp = "購買価格差異" },
            new { Key = "dict.material_variance_code.MPV", Module = "Frontend", ZhCn = "移动价格差异", EnUs = "Moving Price Variance", JaJp = "移動価格差異" },
            new { Key = "dict.material_variance_code.MUV", Module = "Frontend", ZhCn = "物料使用差异", EnUs = "Material Usage Variance", JaJp = "資材使用差異" },
            new { Key = "dict.material_variance_code.LEV", Module = "Frontend", ZhCn = "人工效率差异", EnUs = "Labor Efficiency Variance", JaJp = "労働効率差異" },
            
            // material_manufacturer
            new { Key = "dict.material_manufacturer.M001", Module = "Frontend", ZhCn = "制造商001", EnUs = "Manufacturer 001", JaJp = "メーカー001" },
            new { Key = "dict.material_manufacturer.M002", Module = "Frontend", ZhCn = "制造商002", EnUs = "Manufacturer 002", JaJp = "メーカー002" },
            new { Key = "dict.material_manufacturer.M003", Module = "Frontend", ZhCn = "制造商003", EnUs = "Manufacturer 003", JaJp = "メーカー003" },
            
            // material_storage_location
            new { Key = "dict.material_storage_location.0001", Module = "Frontend", ZhCn = "仓储地点0001", EnUs = "Storage Location 0001", JaJp = "保管場所0001" },
            new { Key = "dict.material_storage_location.0002", Module = "Frontend", ZhCn = "仓储地点0002", EnUs = "Storage Location 0002", JaJp = "保管場所0002" },
            new { Key = "dict.material_storage_location.0003", Module = "Frontend", ZhCn = "仓储地点0003", EnUs = "Storage Location 0003", JaJp = "保管場所0003" },
            new { Key = "dict.material_storage_location.1001", Module = "Frontend", ZhCn = "生产仓储地点1001", EnUs = "Production Storage Location 1001", JaJp = "生産保管場所1001" },
            new { Key = "dict.material_storage_location.2001", Module = "Frontend", ZhCn = "外部采购仓储地点2001", EnUs = "External Purchase Storage Location 2001", JaJp = "外部調達保管場所2001" },
            
            // material_storage_position
            new { Key = "dict.material_storage_position.A_01_01", Module = "Frontend", ZhCn = "A区-01排-01位", EnUs = "Zone A-Row 01-Position 01", JaJp = "A区-01列-01位置" },
            new { Key = "dict.material_storage_position.A_01_02", Module = "Frontend", ZhCn = "A区-01排-02位", EnUs = "Zone A-Row 01-Position 02", JaJp = "A区-01列-02位置" },
            new { Key = "dict.material_storage_position.B_01_01", Module = "Frontend", ZhCn = "B区-01排-01位", EnUs = "Zone B-Row 01-Position 01", JaJp = "B区-01列-01位置" },
            new { Key = "dict.material_storage_position.B_01_02", Module = "Frontend", ZhCn = "B区-01排-02位", EnUs = "Zone B-Row 01-Position 02", JaJp = "B区-01列-02位置" },

        };

        foreach (var trans in translations)
        {
            CreateOrUpdateTranslation(zhCn, trans.Key, trans.Module, trans.ZhCn);
            CreateOrUpdateTranslation(enUs, trans.Key, trans.Module, trans.EnUs);
            CreateOrUpdateTranslation(jaJp, trans.Key, trans.Module, trans.JaJp);
        }

        _initLog.Information("✅ 翻译数据初始化完成");
    }

    /// <summary>
    /// 创建或更新翻译
    /// </summary>
    private void CreateOrUpdateTranslation(Language language, string key, string module, string value)
    {
        // 统一按语言代码与翻译键判重；WPF 前端固定模块 Frontend
        var existing = _translationRepository.GetFirst(t =>
            t.LanguageCode == language.LanguageCode && t.TranslationKey == key && t.Module == "Frontend");
        
        if (existing == null)
        {
            var translation = new Translation
            {
                LanguageCode = language.LanguageCode,
                TranslationKey = key,
                TranslationValue = value,
                Module = "Frontend",
                OrderNum = 0
            };
            _translationRepository.Create(translation, "Takt365");
        }
        else
        {
            existing.TranslationValue = value;
            existing.Module = "Frontend";
            _translationRepository.Update(existing, "Takt365");
        }
    }
}

