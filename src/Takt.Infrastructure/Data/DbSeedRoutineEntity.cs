// ========================================
// 项目名称：Takt.Wpf
// 命名空间：Takt.Infrastructure.Data
// 文件名称：DbSeedRoutineEntity.cs
// 创建时间：2025-11-11
// 创建人：Takt365(Cursor AI)
// 功能描述：实体字段翻译种子数据
// 
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.Collections.Generic;
using System.Linq;
using Takt.Common.Logging;
using Takt.Domain.Entities.Routine;
using Takt.Domain.Repositories;

namespace Takt.Infrastructure.Data;

/// <summary>
/// Routine 模块实体字段翻译种子初始化器
/// </summary>
public class DbSeedRoutineEntity
{
    private readonly InitLogManager _initLog;
    private readonly IBaseRepository<Translation> _translationRepository;
    private readonly IBaseRepository<Language> _languageRepository;

    public DbSeedRoutineEntity(
        InitLogManager initLog,
        IBaseRepository<Translation> translationRepository,
        IBaseRepository<Language> languageRepository)
    {
        _initLog = initLog ?? throw new ArgumentNullException(nameof(initLog));
        _translationRepository = translationRepository ?? throw new ArgumentNullException(nameof(translationRepository));
        _languageRepository = languageRepository ?? throw new ArgumentNullException(nameof(languageRepository));
    }

    /// <summary>
    /// 初始化实体字段翻译
    /// </summary>
    public void Run()
    {
        var languages = _languageRepository.AsQueryable().ToList();
        if (languages.Count == 0)
        {
            _initLog.Warning("语言数据为空，跳过实体字段翻译初始化");
            return;
        }

        var zhCn = languages.FirstOrDefault(l => l.LanguageCode == "zh-CN");
        var enUs = languages.FirstOrDefault(l => l.LanguageCode == "en-US");
        var jaJp = languages.FirstOrDefault(l => l.LanguageCode == "ja-JP");

        if (zhCn == null || enUs == null || jaJp == null)
        {
            _initLog.Warning("语言数据不完整（zh-CN/en-US/ja-JP 缺失），跳过实体字段翻译初始化");
            return;
        }

        foreach (var seed in BuildTranslationSeeds())
        {
            CreateOrUpdateTranslation(zhCn, seed.Key, seed.Module, seed.ZhCn);
            CreateOrUpdateTranslation(enUs, seed.Key, seed.Module, seed.EnUs);
            CreateOrUpdateTranslation(jaJp, seed.Key, seed.Module, seed.JaJp);
        }

        _initLog.Information("✅ 实体字段翻译初始化完成");
    }

    private void CreateOrUpdateTranslation(Language language, string key, string module, string value)
    {
        var existing = _translationRepository.GetFirst(t =>
            t.LanguageCode == language.LanguageCode &&
            t.TranslationKey == key &&
            t.Module == module);

        if (existing == null)
        {
            var translation = new Translation
            {
                LanguageCode = language.LanguageCode,
                TranslationKey = key,
                TranslationValue = value,
                Module = module,
                OrderNum = 0
            };
            _translationRepository.Create(translation, "Takt365");
        }
        else
        {
            existing.TranslationValue = value;
            existing.Module = module;
            _translationRepository.Update(existing, "Takt365");
        }
    }

    private static List<TranslationSeed> BuildTranslationSeeds()
    {
        return new List<TranslationSeed>
        {
            // 用户实体字段（严格对齐实体）
            new("Identity.User", "Frontend", "用户", "User", "ユーザー"),
            new("Identity.User.Username", "Frontend", "用户名", "Username", "ユーザー名"),
            new("Identity.User.Avatar", "Frontend", "头像", "Avatar", "アバター"),
            new("Identity.User.Password", "Frontend", "密码", "Password", "パスワード"),
            new("Identity.User.PasswordConfirm", "Frontend", "确认密码", "Confirm Password", "パスワード確認"),
            new("Identity.User.Email", "Frontend", "邮箱", "Email", "メール"),
            new("Identity.User.Phone", "Frontend", "手机号", "Phone", "携帯番号"),
            new("Identity.User.RealName", "Frontend", "真实姓名", "Real Name", "氏名"),
            new("Identity.User.Nickname", "Frontend", "昵称", "Nickname", "ニックネーム"),
            new("Identity.User.UserType", "Frontend", "用户类型", "User Type", "ユーザー種別"),
            new("Identity.User.UserType.System", "Frontend", "系统用户", "System User", "システムユーザー"),
            new("Identity.User.UserType.Normal", "Frontend", "普通用户", "Normal User", "一般ユーザー"),
            new("Identity.User.UserGender", "Frontend", "性别", "Gender", "性別"),
            new("Identity.User.UserGender.Unknown", "Frontend", "未知", "Unknown", "不明"),
            new("Identity.User.UserGender.Male", "Frontend", "男", "Male", "男性"),
            new("Identity.User.UserGender.Female", "Frontend", "女", "Female", "女性"),
            new("Identity.User.UserStatus", "Frontend", "状态", "Status", "状態"),
            new("Identity.User.UserStatus.Normal", "Frontend", "正常", "Normal", "正常"),
            new("Identity.User.UserStatus.Disabled", "Frontend", "禁用", "Disabled", "無効"),
            new("Identity.User.Roles", "Frontend", "角色用户", "Role Users", "ロールユーザー"),
            new("Identity.User.Keyword", "Frontend", "用户名、手机", "username, phone", "ユーザー名、電話"),
            new("Identity.User.DetailTitle", "Frontend", "用户信息", "User Information", "ユーザー情報"),
            new("Identity.User.DetailSubtitle", "Frontend", "查看用户详细信息", "View user details", "ユーザー詳細を表示"),
            new("Identity.User.BasicInfo", "Frontend", "基本信息", "Basic Information", "基本情報"),
            new("Identity.User.StatusInfo", "Frontend", "状态信息", "Status Information", "状態情報"),
            new("Identity.User.RemarksInfo", "Frontend", "备注信息", "Remarks Information", "備考情報"),
            new("Identity.User.Create", "Frontend", "新建用户", "Create User", "ユーザーを作成"),
            new("Identity.User.Update", "Frontend", "编辑用户", "Edit User", "ユーザーを編集"),
            
            // 角色实体字段（严格对齐实体）
            new("Identity.Role", "Frontend", "角色", "Role", "ロール"),
            new("Identity.Role.RoleName", "Frontend", "角色名称", "Role Name", "ロール名"),
            new("Identity.Role.RoleCode", "Frontend", "角色编码", "Role Code", "ロールコード"),
            new("Identity.Role.Description", "Frontend", "描述", "Description", "説明"),
            new("Identity.Role.DataScope", "Frontend", "数据范围", "Data Scope", "データ範囲"),
            new("Identity.Role.DataScope.All", "Frontend", "全部", "All", "すべて"),
            new("Identity.Role.DataScope.Custom", "Frontend", "自定义", "Custom", "カスタム"),
            new("Identity.Role.DataScope.Department", "Frontend", "本部门", "Department", "部門"),
            new("Identity.Role.DataScope.DepartmentAndBelow", "Frontend", "本部门及以下", "Department and Below", "部門以下"),
            new("Identity.Role.DataScope.Self", "Frontend", "仅本人", "Self", "本人のみ"),
            new("Identity.Role.UserCount", "Frontend", "用户数", "User Count", "ユーザー数"),
            new("Identity.Role.OrderNum", "Frontend", "排序号", "Order Num", "並び順"),
            new("Identity.Role.RoleStatus", "Frontend", "状态", "Status", "状態"),
            new("Identity.Role.RoleStatus.Normal", "Frontend", "正常", "Normal", "正常"),
            new("Identity.Role.RoleStatus.Disabled", "Frontend", "禁用", "Disabled", "無効"),
            new("Identity.Role.Users", "Frontend", "角色用户", "Role Users", "ロールユーザー"),
            new("Identity.Role.Menus", "Frontend", "角色菜单", "Role Menus", "ロールメニュー"),
            new("Identity.Role.Keyword", "Frontend", "角色名称、编码", "role name, code", "ロール名、コード"),
            new("Identity.Role.BasicInfo", "Frontend", "基本信息", "Basic Information", "基本情報"),
            new("Identity.Role.StatusInfo", "Frontend", "状态信息", "Status Information", "状態情報"),
            new("Identity.Role.RemarksInfo", "Frontend", "备注信息", "Remarks Information", "備考情報"),
            new("Identity.Role.Create", "Frontend", "新建角色", "Create Role", "ロールを作成"),
            new("Identity.Role.Update", "Frontend", "编辑角色", "Edit Role", "ロールを編集"),

            // 菜单实体字段（严格对齐实体）
            new("Identity.Menu", "Frontend", "菜单", "Menu", "メニュー"),
            new("Identity.Menu.MenuName", "Frontend", "菜单名称", "Menu Name", "メニュー名"),
            new("Identity.Menu.MenuCode", "Frontend", "菜单编码", "Menu Code", "メニューコード"),
            new("Identity.Menu.I18nKey", "Frontend", "国际化键", "I18n Key", "多言語キー"),
            new("Identity.Menu.PermCode", "Frontend", "权限码", "Permission Code", "権限コード"),
            new("Identity.Menu.MenuType", "Frontend", "菜单类型", "Menu Type", "メニュー種別"),
            new("Identity.Menu.MenuType.Directory", "Frontend", "目录", "Directory", "ディレクトリ"),
            new("Identity.Menu.MenuType.Menu", "Frontend", "菜单", "Menu", "メニュー"),
            new("Identity.Menu.MenuType.Button", "Frontend", "按钮", "Button", "ボタン"),
            new("Identity.Menu.MenuType.Api", "Frontend", "接口", "API", "API"),
            new("Identity.Menu.ParentId", "Frontend", "父级ID", "Parent ID", "上位ID"),
            new("Identity.Menu.RoutePath", "Frontend", "路由路径", "Route Path", "ルートパス"),
            new("Identity.Menu.Icon", "Frontend", "图标", "Icon", "アイコン"),
            new("Identity.Menu.Component", "Frontend", "组件路径", "Component Path", "コンポーネントパス"),
            new("Identity.Menu.IsExternal", "Frontend", "是否外链", "Is External", "外部リンクか"),
            new("Identity.Menu.IsExternal.External", "Frontend", "外链", "External", "外部リンク"),
            new("Identity.Menu.IsExternal.NotExternal", "Frontend", "非外链", "Not External", "非外部リンク"),
            new("Identity.Menu.IsCache", "Frontend", "是否缓存", "Is Cache", "キャッシュか"),
            new("Identity.Menu.IsCache.Cache", "Frontend", "缓存", "Cache", "キャッシュ"),
            new("Identity.Menu.IsCache.NoCache", "Frontend", "不缓存", "No Cache", "キャッシュなし"),
            new("Identity.Menu.IsVisible", "Frontend", "是否可见", "Is Visible", "表示か"),
            new("Identity.Menu.IsVisible.Visible", "Frontend", "可见", "Visible", "表示"),
            new("Identity.Menu.IsVisible.Invisible", "Frontend", "不可见", "Invisible", "非表示"),
            new("Identity.Menu.OrderNum", "Frontend", "排序号", "Order Num", "並び順"),
            new("Identity.Menu.MenuStatus", "Frontend", "状态", "Status", "状態"),
            new("Identity.Menu.MenuStatus.Normal", "Frontend", "正常", "Normal", "正常"),
            new("Identity.Menu.MenuStatus.Disabled", "Frontend", "禁用", "Disabled", "無効"),
            new("Identity.Menu.Roles", "Frontend", "角色菜单", "Role Menus", "ロールメニュー"),
            new("Identity.Menu.Keyword", "Frontend", "菜单名称、编码", "menu name, code", "メニュー名、コード"),
            new("Identity.Menu.BasicInfo", "Frontend", "基本信息", "Basic Information", "基本情報"),
            new("Identity.Menu.RouteInfo", "Frontend", "路由信息", "Route Information", "ルート情報"),
            new("Identity.Menu.StatusInfo", "Frontend", "状态信息", "Status Information", "状態情報"),
            new("Identity.Menu.RemarksInfo", "Frontend", "备注信息", "Remarks Information", "備考情報"),
            new("Identity.Menu.Create", "Frontend", "新建菜单", "Create Menu", "メニューを作成"),
            new("Identity.Menu.Update", "Frontend", "编辑菜单", "Edit Menu", "メニューを編集"),

            // 用户会话实体字段
            new("Identity.UserSession", "Frontend", "用户会话", "User Session", "ユーザーセッション"),
            new("Identity.UserSession.SessionId", "Frontend", "会话ID", "Session ID", "セッションID"),
            new("Identity.UserSession.UserId", "Frontend", "用户ID", "User ID", "ユーザーID"),
            new("Identity.UserSession.Username", "Frontend", "用户名", "Username", "ユーザー名"),
            new("Identity.UserSession.RealName", "Frontend", "真实姓名", "Real Name", "氏名"),
            new("Identity.UserSession.RoleId", "Frontend", "角色ID", "Role ID", "ロールID"),
            new("Identity.UserSession.RoleName", "Frontend", "角色名称", "Role Name", "ロール名"),
            new("Identity.UserSession.LoginTime", "Frontend", "登录时间", "Login Time", "ログイン時間"),
            new("Identity.UserSession.ExpiresAt", "Frontend", "过期时间", "Expires At", "有効期限"),
            new("Identity.UserSession.LastActivityTime", "Frontend", "最后活动时间", "Last Activity Time", "最終活動時刻"),
            new("Identity.UserSession.LoginIp", "Frontend", "登录IP", "Login IP", "ログインIP"),
            new("Identity.UserSession.ClientInfo", "Frontend", "客户端信息", "Client Info", "クライアント情報"),
            new("Identity.UserSession.ClientSnapshot", "Frontend", "客户端快照", "Client Snapshot", "クライアントスナップショット"),
            new("Identity.UserSession.OsDescription", "Frontend", "操作系统描述", "OS Description", "OS説明"),
            new("Identity.UserSession.OsVersion", "Frontend", "操作系统版本", "OS Version", "OSバージョン"),
            new("Identity.UserSession.OsType", "Frontend", "操作系统类型", "OS Type", "OS種別"),
            new("Identity.UserSession.OsArchitecture", "Frontend", "操作系统架构", "OS Architecture", "OSアーキテクチャ"),
            new("Identity.UserSession.MachineName", "Frontend", "机器名称", "Machine Name", "マシン名"),
            new("Identity.UserSession.MacAddress", "Frontend", "MAC地址", "MAC Address", "MACアドレス"),
            new("Identity.UserSession.FrameworkVersion", "Frontend", ".NET运行时版本", ".NET Runtime Version", ".NETランタイム"),
            new("Identity.UserSession.ProcessArchitecture", "Frontend", "进程架构", "Process Architecture", "プロセスアーキテクチャ"),
            new("Identity.UserSession.IsActive", "Frontend", "是否活跃", "Is Active", "有効か"),
            new("Identity.UserSession.LogoutTime", "Frontend", "登出时间", "Logout Time", "ログアウト時間"),
            new("Identity.UserSession.LogoutReason", "Frontend", "登出原因", "Logout Reason", "ログアウト理由"),
            new("Identity.UserSession.Keyword", "Frontend", "用户名、手机", "username, phone", "ユーザー名、電話"),

            // 角色菜单/用户角色关联实体字段
            new("Identity.RoleMenu", "Frontend", "角色菜单", "Role Menu", "ロールメニュー"),
            new("Identity.RoleMenu.RoleId", "Frontend", "角色ID", "Role ID", "ロールID"),
            new("Identity.RoleMenu.MenuId", "Frontend", "菜单ID", "Menu ID", "メニューID"),
            new("Identity.RoleMenu.Keyword", "Frontend", "角色ID、菜单ID", "role id, menu id", "ロールID、メニューID"),
            new("Identity.UserRole", "Frontend", "用户角色", "User Role", "ユーザーロール"),
            new("Identity.UserRole.UserId", "Frontend", "用户ID", "User ID", "ユーザーID"),
            new("Identity.UserRole.RoleId", "Frontend", "角色ID", "Role ID", "ロールID"),
            new("Identity.UserRole.Keyword", "Frontend", "用户名、角色名称", "username, role name", "ユーザー名、ロール名"),

            // 字典实体字段（严格对齐实体）
            new("Routine.DictionaryType", "Frontend", "字典类型", "Dictionary Type", "辞書タイプ"),
            new("Routine.DictionaryType.TypeCode", "Frontend", "类型代码", "Type Code", "タイプコード"),
            new("Routine.DictionaryType.TypeName", "Frontend", "类型名称", "Type Name", "タイプ名"),
            new("Routine.DictionaryType.DataSource", "Frontend", "数据源", "Data Source", "データソース"),
            new("Routine.DictionaryType.SqlScript", "Frontend", "SQL脚本", "SQL Script", "SQLスクリプト"),
            new("Routine.DictionaryType.OrderNum", "Frontend", "排序号", "Order Number", "順序"),
            new("Routine.DictionaryType.IsBuiltin", "Frontend", "内置", "Is Built-in", "内蔵か"),
            new("Routine.DictionaryType.TypeStatus", "Frontend", "状态", "Status", "状態"),
            new("Routine.DictionaryType.Keyword", "Frontend", "类型代码、类型名称", "type code, type name", "タイプコード、タイプ名"),
            new("Routine.DictionaryData", "Frontend", "字典数据", "Dictionary Data", "辞書データ"),
            new("Routine.DictionaryData.TypeCode", "Frontend", "类型代码", "Type Code", "タイプコード"),
            new("Routine.DictionaryData.DataLabel", "Frontend", "数据标签", "Data Label", "データラベル"),
            new("Routine.DictionaryData.I18nKey", "Frontend", "国际化键", "I18n Key", "多言語キー"),
            new("Routine.DictionaryData.DataValue", "Frontend", "数据值", "Data Value", "データ値"),
            new("Routine.DictionaryData.ExtLabel", "Frontend", "扩展标签", "Ext Label", "拡張ラベル"),
            new("Routine.DictionaryData.ExtValue", "Frontend", "扩展值", "Ext Value", "拡張値"),
            new("Routine.DictionaryData.CssClass", "Frontend", "CSS类名", "CSS Class", "CSSクラス"),
            new("Routine.DictionaryData.ListClass", "Frontend", "列表CSS类名", "List CSS Class", "リストCSSクラス"),
            new("Routine.DictionaryData.OrderNum", "Frontend", "排序号", "Order Number", "順序"),
            new("Routine.DictionaryData.Keyword", "Frontend", "数据标签、数据值、国际化键", "data label, data value, i18n key", "データラベル、データ値、多言語キー"),
            
            new("Routine.Language", "Frontend", "语言", "Language", "言語"),
            new("Routine.Language.LanguageCode", "Frontend", "语言代码", "Language Code", "言語コード"),
            new("Routine.Language.LanguageName", "Frontend", "语言名称", "Language Name", "言語名"),
            new("Routine.Language.NativeName", "Frontend", "本地化名称", "Native Name", "現地名"),
            new("Routine.Language.LanguageIcon", "Frontend", "语言图标", "Language Icon", "言語アイコン"),
            new("Routine.Language.IsDefault", "Frontend", "是否默认", "Is Default", "既定か"),
            new("Routine.Language.OrderNum", "Frontend", "排序号", "Order Number", "順序"),
            new("Routine.Language.IsBuiltin", "Frontend", "是否内置", "Is Built-in", "内蔵か"),
            new("Routine.Language.LanguageStatus", "Frontend", "语言状态", "Language Status", "言語状態"),
            new("Routine.Language.Keyword", "Frontend", "语言代码、语言名称、本地化名称", "language code, language name, native name", "言語コード、言語名、現地名"),
            new("Routine.Translation", "Frontend", "翻译", "Translation", "翻訳"),
            new("Routine.Translation.LanguageCode", "Frontend", "语言代码", "Language Code", "言語コード"),
            new("Routine.Translation.TranslationKey", "Frontend", "翻译键", "Translation Key", "翻訳キー"),
            new("Routine.Translation.TranslationValue", "Frontend", "翻译值", "Translation Value", "翻訳値"),
            new("Routine.Translation.Module", "Frontend", "模块", "Module", "モジュール"),
            new("Routine.Translation.Description", "Frontend", "描述", "Description", "説明"),
            new("Routine.Translation.OrderNum", "Frontend", "排序号", "Order Number", "順序"),
            new("Routine.Translation.Keyword", "Frontend", "语言代码、翻译键", "language code, translation key", "言語コード、翻訳キー"),
            // 设置实体字段（严格对齐实体）
            new("Routine.Setting", "Frontend", "系统设置", "System Setting", "システム設定"),
            new("Routine.Setting.Key", "Frontend", "配置键", "Setting Key", "設定キー"),
            new("Routine.Setting.Value", "Frontend", "配置值", "Setting Value", "設定値"),
            new("Routine.Setting.Category", "Frontend", "分类", "Category", "カテゴリ"),
            new("Routine.Setting.OrderNum", "Frontend", "排序号", "Order Num", "並び順"),
            new("Routine.Setting.Type", "Frontend", "配置类型", "Setting Type", "設定タイプ"),
            new("Routine.Setting.Type.String", "Frontend", "字符串", "String", "文字列"),
            new("Routine.Setting.Type.Number", "Frontend", "数字", "Number", "数値"),
            new("Routine.Setting.Type.Boolean", "Frontend", "布尔值", "Boolean", "ブール値"),
            new("Routine.Setting.Type.Json", "Frontend", "JSON", "JSON", "JSON"),
            new("Routine.Setting.IsBuiltin", "Frontend", "是否内置", "Is Built-in", "内蔵か"),
            new("Routine.Setting.IsDefault", "Frontend", "是否默认", "Is Default", "デフォルトか"),
            new("Routine.Setting.IsEditable", "Frontend", "是否可修改", "Is Editable", "編集可か"),
            new("Routine.Setting.Description", "Frontend", "设置描述", "Description", "説明"),
            new("Routine.Setting.Keyword", "Frontend", "配置键、分类", "setting key, category", "設定キー、カテゴリ"),

            // 任务管理实体字段（严格对齐实体）
            new("Routine.QuartzJob", "Frontend", "任务管理", "Job Management", "タスク管理"),
            new("Routine.QuartzJob.JobName", "Frontend", "任务名称", "Job Name", "タスク名"),
            new("Routine.QuartzJob.JobGroup", "Frontend", "任务组", "Job Group", "タスクグループ"),
            new("Routine.QuartzJob.TriggerName", "Frontend", "触发器名称", "Trigger Name", "トリガー名"),
            new("Routine.QuartzJob.TriggerGroup", "Frontend", "触发器组", "Trigger Group", "トリガーグループ"),
            new("Routine.QuartzJob.CronExpression", "Frontend", "Cron表达式", "Cron Expression", "Cron式"),
            new("Routine.QuartzJob.JobClassName", "Frontend", "任务类名", "Job Class Name", "タスククラス名"),
            new("Routine.QuartzJob.JobDescription", "Frontend", "任务描述", "Job Description", "タスク説明"),
            new("Routine.QuartzJob.Status", "Frontend", "任务状态", "Status", "状態"),
            new("Routine.QuartzJob.Status.Enabled", "Frontend", "启用", "Enabled", "有効"),
            new("Routine.QuartzJob.Status.Disabled", "Frontend", "禁用", "Disabled", "無効"),
            new("Routine.QuartzJob.Status.Running", "Frontend", "运行中", "Running", "実行中"),
            new("Routine.QuartzJob.Status.Paused", "Frontend", "暂停", "Paused", "一時停止"),
            new("Routine.QuartzJob.JobParams", "Frontend", "任务参数", "Job Parameters", "タスクパラメータ"),
            new("Routine.QuartzJob.LastRunTime", "Frontend", "最后执行时间", "Last Run Time", "最終実行時刻"),
            new("Routine.QuartzJob.NextRunTime", "Frontend", "下次执行时间", "Next Run Time", "次回実行時刻"),
            new("Routine.QuartzJob.RunCount", "Frontend", "执行次数", "Run Count", "実行回数"),
            new("Routine.QuartzJob.Keyword", "Frontend", "任务名称、任务组、任务描述", "job name, job group, job description", "タスク名、タスクグループ、タスク説明"),
            new("Routine.QuartzJob.BasicInfo", "Frontend", "基本信息", "Basic Information", "基本情報"),
            new("Routine.QuartzJob.TriggerConfig", "Frontend", "触发器配置", "Trigger Configuration", "トリガー設定"),
            new("Routine.QuartzJob.AdvancedSettings", "Frontend", "高级设置", "Advanced Settings", "高度な設定"),

            // 任务日志实体字段
            new("Logging.QuartzJobLog", "Frontend", "任务日志", "Job Log", "タスクログ"),
            new("Logging.QuartzJobLog.QuartzId", "Frontend", "任务ID", "Job ID", "タスクID"),
            new("Logging.QuartzJobLog.JobName", "Frontend", "任务名称", "Job Name", "タスク名"),
            new("Logging.QuartzJobLog.JobGroup", "Frontend", "任务组", "Job Group", "タスクグループ"),
            new("Logging.QuartzJobLog.TriggerName", "Frontend", "触发器名称", "Trigger Name", "トリガー名"),
            new("Logging.QuartzJobLog.TriggerGroup", "Frontend", "触发器组", "Trigger Group", "トリガーグループ"),
            new("Logging.QuartzJobLog.StartTime", "Frontend", "开始时间", "Start Time", "開始時間"),
            new("Logging.QuartzJobLog.EndTime", "Frontend", "结束时间", "End Time", "終了時間"),
            new("Logging.QuartzJobLog.ElapsedTime", "Frontend", "执行耗时", "Elapsed Time", "処理時間"),
            new("Logging.QuartzJobLog.ExecuteResult", "Frontend", "执行结果", "Execute Result", "実行結果"),
            new("Logging.QuartzJobLog.ErrorMessage", "Frontend", "错误信息", "Error Message", "エラーメッセージ"),
            new("Logging.QuartzJobLog.JobParams", "Frontend", "执行参数", "Job Parameters", "実行パラメーター"),
            new("Logging.QuartzJobLog.Keyword", "Frontend", "任务名称、任务组、触发器名称、触发器组、错误信息", "job name, job group, trigger name, trigger group, error message", "タスク名、タスクグループ、トリガー名、トリガーグループ、エラーメッセージ"),

            // 登录日志实体字段
            new("Logging.LoginLog", "Frontend", "登录日志", "Login Log", "ログインログ"),
            new("Logging.LoginLog.Username", "Frontend", "用户名", "Username", "ユーザー名"),
            new("Logging.LoginLog.LoginTime", "Frontend", "登录时间", "Login Time", "ログイン時間"),
            new("Logging.LoginLog.LogoutTime", "Frontend", "登出时间", "Logout Time", "ログアウト時間"),
            new("Logging.LoginLog.LoginIp", "Frontend", "登录IP", "Login IP", "ログインIP"),
            new("Logging.LoginLog.MacAddress", "Frontend", "MAC地址", "MAC Address", "MACアドレス"),
            new("Logging.LoginLog.MachineName", "Frontend", "机器名称", "Machine Name", "マシン名"),
            new("Logging.LoginLog.LoginLocation", "Frontend", "登录地点", "Login Location", "ログイン場所"),
            new("Logging.LoginLog.Client", "Frontend", "客户端", "Client", "クライアント"),
            new("Logging.LoginLog.Os", "Frontend", "操作系统", "Operating System", "OS"),
            new("Logging.LoginLog.OsVersion", "Frontend", "操作系统版本", "OS Version", "OSバージョン"),
            new("Logging.LoginLog.OsArchitecture", "Frontend", "系统架构", "OS Architecture", "OSアーキテクチャ"),
            new("Logging.LoginLog.CpuInfo", "Frontend", "CPU信息", "CPU Info", "CPU情報"),
            new("Logging.LoginLog.TotalMemoryGb", "Frontend", "物理内存(GB)", "Total Memory (GB)", "物理メモリ(GB)"),
            new("Logging.LoginLog.FrameworkVersion", "Frontend", ".NET运行时", ".NET Runtime", ".NETランタイム"),
            new("Logging.LoginLog.IsAdmin", "Frontend", "是否管理员", "Is Admin", "管理者か"),
            new("Logging.LoginLog.ClientType", "Frontend", "客户端类型", "Client Type", "クライアント種別"),
            new("Logging.LoginLog.ClientVersion", "Frontend", "客户端版本", "Client Version", "クライアントバージョン"),
            new("Logging.LoginLog.LoginStatus", "Frontend", "登录状态", "Login Status", "ログイン状態"),
            new("Logging.LoginLog.FailReason", "Frontend", "失败原因", "Fail Reason", "失敗理由"),
            new("Logging.LoginLog.Keyword", "Frontend", "用户名、登录时间、登录IP、MAC地址、机器名称、登录地点、客户端、操作系统、操作系统版本、系统架构、CPU信息、物理内存(GB)、.NET运行时、是否管理员、客户端类型、客户端版本、登录状态、失败原因", "username, login time, login IP, MAC address, machine name, login location, client, operating system, operating system version, OS architecture, CPU info, total memory (GB), .NET runtime, is admin, client type, client version, login status, fail reason", "ユーザー名、ログイン時間、ログインIP、MACアドレス、マシン名、ログイン場所、クライアント、OS、OSバージョン、OSアーキテクチャ、CPU情報、物理メモリ(GB)、.NETランタイム、管理者か、クライアント種別、クライアントバージョン、ログイン状態、失敗理由"),

            // 操作日志实体字段
            new("Logging.OperLog", "Frontend", "操作日志", "Operation Log", "操作ログ"),
            new("Logging.OperLog.Username", "Frontend", "用户名", "Username", "ユーザー名"),
            new("Logging.OperLog.OperationType", "Frontend", "操作类型", "Operation Type", "操作種別"),
            new("Logging.OperLog.OperationModule", "Frontend", "操作模块", "Operation Module", "操作モジュール"),
            new("Logging.OperLog.OperationDesc", "Frontend", "操作描述", "Operation Description", "操作説明"),
            new("Logging.OperLog.OperationTime", "Frontend", "操作时间", "Operation Time", "操作時間"),
            new("Logging.OperLog.RequestPath", "Frontend", "请求路径", "Request Path", "リクエストパス"),
            new("Logging.OperLog.RequestMethod", "Frontend", "请求方法", "Request Method", "リクエスト方法"),
            new("Logging.OperLog.RequestParams", "Frontend", "请求参数", "Request Parameters", "リクエストパラメーター"),
            new("Logging.OperLog.ResponseResult", "Frontend", "响应结果", "Response Result", "応答結果"),
            new("Logging.OperLog.ElapsedTime", "Frontend", "执行耗时", "Elapsed Time", "処理時間"),
            new("Logging.OperLog.IpAddress", "Frontend", "IP地址", "IP Address", "IPアドレス"),
            new("Logging.OperLog.UserAgent", "Frontend", "用户代理", "User Agent", "ユーザーエージェント"),
            new("Logging.OperLog.Os", "Frontend", "操作系统", "Operating System", "OS"),
            new("Logging.OperLog.Browser", "Frontend", "浏览器", "Browser", "ブラウザー"),
            new("Logging.OperLog.OperationResult", "Frontend", "操作结果", "Operation Result", "操作結果"),
            new("Logging.OperLog.Keyword", "Frontend", "用户名、操作类型、操作模块、操作描述、操作时间、请求路径、请求方法、请求参数、响应结果、执行耗时、IP地址、用户代理、操作系统、浏览器、操作结果", "username, operation type, operation module, operation description, operation time, request path, request method, request parameters, response result, elapsed time, IP address, user agent, operating system, browser, operation result", "ユーザー名、操作種別、操作モジュール、操作説明、操作時間、リクエストパス、リクエスト方法、リクエストパラメーター、応答結果、処理時間、IPアドレス、ユーザーエージェント、OS、ブラウザー、操作結果"),

            // 差异日志实体字段
            new("Logging.DiffLog", "Frontend", "差异日志", "Diff Log", "差分ログ"),
            new("Logging.DiffLog.TableName", "Frontend", "表名", "Table Name", "テーブル名"),
            new("Logging.DiffLog.DiffType", "Frontend", "差异类型", "Diff Type", "差分種別"),
            new("Logging.DiffLog.BusinessData", "Frontend", "业务数据", "Business Data", "業務データ"),
            new("Logging.DiffLog.BeforeData", "Frontend", "变更前数据", "Before Data", "変更前データ"),
            new("Logging.DiffLog.AfterData", "Frontend", "变更后数据", "After Data", "変更後データ"),
            new("Logging.DiffLog.Sql", "Frontend", "执行SQL", "Executed SQL", "実行SQL"),
            new("Logging.DiffLog.Parameters", "Frontend", "SQL参数", "SQL Parameters", "SQLパラメーター"),
            new("Logging.DiffLog.DiffTime", "Frontend", "差异时间", "Diff Time", "差分時間"),
            new("Logging.DiffLog.ElapsedTime", "Frontend", "执行耗时", "Elapsed Time", "処理時間"),
            new("Logging.DiffLog.Username", "Frontend", "用户名", "Username", "ユーザー名"),
            new("Logging.DiffLog.IpAddress", "Frontend", "IP地址", "IP Address", "IPアドレス"),
            new("Logging.DiffLog.Keyword", "Frontend", "表名、差异类型、业务数据、变更前数据、变更后数据、执行SQL、SQL参数、差异时间、执行耗时、用户名、IP地址", "table name, diff type, business data, before data, after data, executed SQL, SQL parameters, diff time, elapsed time, username, IP address", "テーブル名、差分種別、業務データ、変更前データ、変更後データ、実行SQL、SQLパラメーター、差分時間、処理時間、ユーザー名、IPアドレス"),

            // 生产物料实体字段
            new("Logistics.Materials.ProdMaterial", "Frontend", "生产物料", "Production Material", "生産資材"),
            new("Logistics.Materials.ProdMaterial.Plant", "Frontend", "工厂", "Plant", "工場"),
            new("Logistics.Materials.ProdMaterial.MaterialCode", "Frontend", "物料编码", "Material Code", "資材コード"),
            new("Logistics.Materials.ProdMaterial.IndustryField", "Frontend", "行业领域", "Industry Field", "業界分野"),
            new("Logistics.Materials.ProdMaterial.MaterialType", "Frontend", "物料类型", "Material Type", "資材種別"),
            new("Logistics.Materials.ProdMaterial.MaterialDescription", "Frontend", "物料描述", "Material Description", "資材説明"),
            new("Logistics.Materials.ProdMaterial.BaseUnit", "Frontend", "基本计量单位", "Base Unit", "基準単位"),
            new("Logistics.Materials.ProdMaterial.ProductHierarchy", "Frontend", "产品层次", "Product Hierarchy", "製品階層"),
            new("Logistics.Materials.ProdMaterial.MaterialGroup", "Frontend", "物料组", "Material Group", "資材グループ"),
            new("Logistics.Materials.ProdMaterial.PurchaseGroup", "Frontend", "采购组", "Purchase Group", "購買グループ"),
            new("Logistics.Materials.ProdMaterial.PurchaseType", "Frontend", "采购类型", "Purchase Type", "購買種別"),
            new("Logistics.Materials.ProdMaterial.SpecialPurchaseType", "Frontend", "特殊采购类", "Special Purchase Type", "特別購買種"),
            new("Logistics.Materials.ProdMaterial.BulkMaterial", "Frontend", "散装物料", "Bulk Material", "バルク資材"),
            new("Logistics.Materials.ProdMaterial.MinimumOrderQuantity", "Frontend", "最小起订量", "Minimum Order Quantity", "最小発注量"),
            new("Logistics.Materials.ProdMaterial.RoundingValue", "Frontend", "舍入值", "Rounding Value", "丸め値"),
            new("Logistics.Materials.ProdMaterial.PlannedDeliveryTime", "Frontend", "计划交货时间", "Planned Delivery Time", "計画納期"),
            new("Logistics.Materials.ProdMaterial.SelfProductionDays", "Frontend", "自制生产天数", "Self Production Days", "自社生産日数"),
            new("Logistics.Materials.ProdMaterial.PostToInspectionStock", "Frontend", "过账到检验库存", "Post To Inspection Stock", "検査在庫へ転記"),
            new("Logistics.Materials.ProdMaterial.ProfitCenter", "Frontend", "利润中心", "Profit Center", "損益センター"),
            new("Logistics.Materials.ProdMaterial.VarianceCode", "Frontend", "差异码", "Variance Code", "差異コード"),
            new("Logistics.Materials.ProdMaterial.BatchManagement", "Frontend", "批次管理", "Batch Management", "ロット管理"),
            new("Logistics.Materials.ProdMaterial.ManufacturerPartNumber", "Frontend", "制造商零件编号", "Manufacturer Part Number", "メーカー部品番号"),
            new("Logistics.Materials.ProdMaterial.Manufacturer", "Frontend", "制造商", "Manufacturer", "メーカー"),
            new("Logistics.Materials.ProdMaterial.EvaluationType", "Frontend", "评估类", "Evaluation Type", "評価種類"),
            new("Logistics.Materials.ProdMaterial.MovingAveragePrice", "Frontend", "移动平均价", "Moving Average Price", "移動平均価格"),
            new("Logistics.Materials.ProdMaterial.Currency", "Frontend", "货币", "Currency", "通貨"),
            new("Logistics.Materials.ProdMaterial.PriceControl", "Frontend", "价格控制", "Price Control", "価格管理"),
            new("Logistics.Materials.ProdMaterial.PriceUnit", "Frontend", "价格单位", "Price Unit", "価格単位"),
            new("Logistics.Materials.ProdMaterial.ProductionStorageLocation", "Frontend", "生产仓储地点", "Production Storage Location", "生産保管場所"),
            new("Logistics.Materials.ProdMaterial.ExternalPurchaseStorageLocation", "Frontend", "外部采购仓储地点", "External Purchase Storage Location", "外部調達保管場所"),
            new("Logistics.Materials.ProdMaterial.StoragePosition", "Frontend", "仓位", "Storage Position", "保管位置"),
            new("Logistics.Materials.ProdMaterial.CrossPlantMaterialStatus", "Frontend", "跨工厂物料状态", "Cross-Plant Material Status", "プラント間資材状態"),
            new("Logistics.Materials.ProdMaterial.StockQuantity", "Frontend", "在库数量", "Stock Quantity", "在庫数量"),
            new("Logistics.Materials.ProdMaterial.HsCode", "Frontend", "HS编码", "HS Code", "HSコード"),
            new("Logistics.Materials.ProdMaterial.HsName", "Frontend", "HS名称", "HS Name", "HS名称"),
            new("Logistics.Materials.ProdMaterial.MaterialWeight", "Frontend", "重量", "Material Weight", "重量"),
            new("Logistics.Materials.ProdMaterial.MaterialVolume", "Frontend", "容积", "Material Volume", "容積"),
            new("Logistics.Materials.ProdMaterial.Keyword", "Frontend", "物料编码、物料名称、物料描述、基本计量单位、产品层次、物料组、采购组、采购类型、特殊采购类、散装物料、最小起订量、舍入值、计划交货时间、自制生产天数、过账到检验库存、利润中心、差异码、批次管理、制造商零件编号、制造商、评估类、移动平均价、货币、价格控制、价格单位、生产仓储地点、外部采购仓储地点、仓位、跨工厂物料状态、在库数量、HS编码、HS名称、重量、容积", "material code, material name, material description, base unit, product hierarchy, material group, purchase group, purchase type, special purchase type, bulk material, minimum order quantity, rounding value, planned delivery time, self production days, post to inspection stock, profit center, variance code, batch management, manufacturer part number, manufacturer, evaluation type, moving average price, currency, price control, price unit, production storage location, external purchase storage location, storage position, cross-plant material status, stock quantity, HS code, HS name, material weight, material volume", "資材コード、資材名、資材説明、基準単位、製品階層、資材グループ、購買グループ、購買種別、特別購買種、バルク資材、最小発注量、丸め値、計画納期、自社生産日数、検査在庫へ転記、損益センター、差異コード、ロット管理、メーカー部品番号、メーカー、評価種類、移動平均価格、通貨、価格管理、価格単位、生産保管場所、外部調達保管場所、保管位置、プラント間資材状態、在庫数量、HSコード、HS名称、重量、容積"),

            // 产品机种实体字段
            new("Logistics.Materials.ProdModel", "Frontend", "产品机种", "Product Model", "製品機種"),
            new("Logistics.Materials.ProdModel.MaterialCode", "Frontend", "物料编码", "Material Code", "資材コード"),
            new("Logistics.Materials.ProdModel.ModelCode", "Frontend", "机种编码", "Model Code", "機種コード"),
            new("Logistics.Materials.ProdModel.DestCode", "Frontend", "仕向编码", "Destination Code", "仕向コード"),
            new("Logistics.Materials.ProdModel.Keyword", "Frontend", "物料编码、机种编码、仕向编码", "material code, model code, destination code", "資材コード、機種コード、仕向コード"),

            // 包装信息实体字段
            new("Logistics.Packing", "Frontend", "包装信息", "Packing Information", "包装情報"),
            new("Logistics.Packing.MaterialCode", "Frontend", "物料编码", "Material Code", "資材コード"),
            new("Logistics.Packing.PackingType", "Frontend", "包装类型", "Packing Type", "包装種別"),
            new("Logistics.Packing.PackingUnit", "Frontend", "包装单位", "Packing Unit", "包装単位"),
            new("Logistics.Packing.GrossWeight", "Frontend", "毛重", "Gross Weight", "総重量"),
            new("Logistics.Packing.NetWeight", "Frontend", "净重", "Net Weight", "正味重量"),
            new("Logistics.Packing.WeightUnit", "Frontend", "重量单位", "Weight Unit", "重量単位"),
            new("Logistics.Packing.BusinessVolume", "Frontend", "业务量", "Business Volume", "容积"),
            new("Logistics.Packing.VolumeUnit", "Frontend", "体积单位", "Volume Unit", "体積単位"),
            new("Logistics.Packing.SizeDimension", "Frontend", "大小/量纲", "Size Dimension", "サイズ/次元"),
            new("Logistics.Packing.QuantityPerPacking", "Frontend", "每包装数量", "Quantity Per Packing", "包装あたり数量"),
            new("Logistics.Packing.Keyword", "Frontend", "物料编码", "material code", "資材コード"),

            // 产品序列号主表字段
            new("Logistics.Serials.ProdSerial", "Frontend", "产品序列号", "Product Serial", "製品シリアル"),
            new("Logistics.Serials.ProdSerial.MaterialCode", "Frontend", "物料编码", "Material Code", "資材コード"),
            new("Logistics.Serials.ProdSerial.ModelCode", "Frontend", "机种编码", "Model Code", "機種コード"),
            new("Logistics.Serials.ProdSerial.DestCode", "Frontend", "仕向编码", "Destination Code", "仕向コード"),
            new("Logistics.Serials.ProdSerial.Keyword", "Frontend", "物料编码、机种编码、仕向编码", "material code, model code, destination code", "資材コード、機種コード、仕向コード"),

            // 产品序列号入库字段
            new("Logistics.Serials.ProdSerialInbound", "Frontend", "序列号入库", "Serial Inbound", "シリアル入庫"),
            new("Logistics.Serials.ProdSerialInbound.FullSerialNumber", "Frontend", "完整序列号", "Full Serial Number", "完全シリアル番号"),
            new("Logistics.Serials.ProdSerialInbound.MaterialCode", "Frontend", "物料编码", "Material Code", "資材コード"),
            new("Logistics.Serials.ProdSerialInbound.SerialNumber", "Frontend", "真正序列号", "Serial Number", "シリアル番号"),
            new("Logistics.Serials.ProdSerialInbound.Quantity", "Frontend", "数量", "Quantity", "数量"),
            new("Logistics.Serials.ProdSerialInbound.InboundNo", "Frontend", "入库单号", "Inbound No.", "入庫番号"),
            new("Logistics.Serials.ProdSerialInbound.InboundDate", "Frontend", "入库日期", "Inbound Date", "入庫日"),
            new("Logistics.Serials.ProdSerialInbound.Warehouse", "Frontend", "仓库", "Warehouse", "倉庫"),
            new("Logistics.Serials.ProdSerialInbound.Location", "Frontend", "库位", "Location", "ロケーション"),
            new("Logistics.Serials.ProdSerialInbound.Keyword", "Frontend", "完整序列号、物料编码、真正序列号、数量、入库单号、入库日期、仓库、库位", "full serial number, material code, serial number, quantity, inbound no., inbound date, warehouse, location", "完全シリアル番号、資材コード、シリアル番号、数量、入庫番号、入庫日、倉庫、ロケーション"),

            // 产品序列号出库字段
            new("Logistics.Serials.ProdSerialOutbound", "Frontend", "序列号出库", "Serial Outbound", "シリアル出庫"),
            new("Logistics.Serials.ProdSerialOutbound.FullSerialNumber", "Frontend", "完整序列号", "Full Serial Number", "完全シリアル番号"),
            new("Logistics.Serials.ProdSerialOutbound.MaterialCode", "Frontend", "物料编码", "Material Code", "資材コード"),
            new("Logistics.Serials.ProdSerialOutbound.SerialNumber", "Frontend", "真正序列号", "Serial Number", "シリアル番号"),
            new("Logistics.Serials.ProdSerialOutbound.Quantity", "Frontend", "数量", "Quantity", "数量"),
            new("Logistics.Serials.ProdSerialOutbound.OutboundNo", "Frontend", "出库单号", "Outbound No.", "出庫番号"),
            new("Logistics.Serials.ProdSerialOutbound.OutboundDate", "Frontend", "出库日期", "Outbound Date", "出庫日"),
            new("Logistics.Serials.ProdSerialOutbound.DestCode", "Frontend", "仕向编码", "Destination Code", "仕向コード"),
            new("Logistics.Serials.ProdSerialOutbound.DestPort", "Frontend", "目的地港口", "Destination Port", "目的地港"),
            new("Logistics.Serials.ProdSerialOutbound.Keyword", "Frontend", "完整序列号、物料编码、真正序列号、数量、出库单号、出库日期、仕向编码、目的地港口", "full serial number, material code, serial number, quantity, outbound no., outbound date, destination code, destination port", "完全シリアル番号、資材コード、シリアル番号、数量、出庫番号、出庫日、仕向コード、目的地港"),

            // 产品序列号扫描记录字段
            new("Logistics.Serials.ProdSerialScanning", "Frontend", "序列号扫描记录", "Serial Scanning Record", "シリアルスキャン記録"),
            new("Logistics.Serials.ProdSerialScanning.InboundFullSerialNumber", "Frontend", "入库完整序列号", "Inbound Full Serial Number", "入庫完全シリアル番号"),
            new("Logistics.Serials.ProdSerialScanning.InboundDate", "Frontend", "入库日期", "Inbound Date", "入庫日"),
            new("Logistics.Serials.ProdSerialScanning.InboundClient", "Frontend", "入库用户", "Inbound User", "入庫ユーザー"),
            new("Logistics.Serials.ProdSerialScanning.InboundIp", "Frontend", "入库IP", "Inbound IP", "入庫IP"),
            new("Logistics.Serials.ProdSerialScanning.InboundMachineName", "Frontend", "入库机器名称", "Inbound Machine Name", "入庫マシン名"),
            new("Logistics.Serials.ProdSerialScanning.InboundLocation", "Frontend", "入库地点", "Inbound Location", "入庫場所"),
            new("Logistics.Serials.ProdSerialScanning.InboundOs", "Frontend", "入库OS", "Inbound OS", "入庫OS"),
            new("Logistics.Serials.ProdSerialScanning.OutboundFullSerialNumber", "Frontend", "出库完整序列号", "Outbound Full Serial Number", "出庫完全シリアル番号"),
            new("Logistics.Serials.ProdSerialScanning.OutboundDate", "Frontend", "出库日期", "Outbound Date", "出庫日"),
            new("Logistics.Serials.ProdSerialScanning.OutboundClient", "Frontend", "出库用户", "Outbound User", "出庫ユーザー"),
            new("Logistics.Serials.ProdSerialScanning.OutboundIp", "Frontend", "出库IP", "Outbound IP", "出庫IP"),
            new("Logistics.Serials.ProdSerialScanning.OutboundMachineName", "Frontend", "出库机器名称", "Outbound Machine Name", "出庫マシン名"),
            new("Logistics.Serials.ProdSerialScanning.OutboundLocation", "Frontend", "出库地点", "Outbound Location", "出庫場所"),
            new("Logistics.Serials.ProdSerialScanning.OutboundOs", "Frontend", "出库OS", "Outbound OS", "出庫OS"),
            new("Logistics.Serials.ProdSerialScanning.Keyword", "Frontend", "入库完整序列号、入库时间、入库客户端、入库IP、入库机器名称、入库地点、入库OS、出库完整序列号、出库时间、出库客户端、出库IP、出库机器名称、出库地点、出库OS", "inbound full serial number, inbound time, inbound client, inbound IP, inbound machine name, inbound location, inbound OS, outbound full serial number, outbound time, outbound client, outbound IP, outbound machine name, outbound location, outbound OS", "入庫完全シリアル番号、入庫時間、入庫クライアント、入庫IP、入庫マシン名、入庫場所、入庫OS、出庫完全シリアル番号、出庫時間、出庫クライアント、出庫IP、出庫マシン名、出庫場所、出庫OS"),

            // 产品序列号扫描异常记录字段
            new("Logistics.Serials.ProdSerialScanningEx", "Frontend", "序列号扫描异常记录", "Serial Scanning Exception Record", "シリアルスキャン異常記録"),
            new("Logistics.Serials.ProdSerialScanningEx.InboundFullSerialNumber", "Frontend", "入库完整序列号", "Inbound Full Serial Number", "入庫完全シリアル番号"),
            new("Logistics.Serials.ProdSerialScanningEx.InboundDate", "Frontend", "入库日期", "Inbound Date", "入庫日"),
            new("Logistics.Serials.ProdSerialScanningEx.InboundClient", "Frontend", "入库用户", "Inbound User", "入庫ユーザー"),
            new("Logistics.Serials.ProdSerialScanningEx.InboundIp", "Frontend", "入库IP", "Inbound IP", "入庫IP"),
            new("Logistics.Serials.ProdSerialScanningEx.InboundMachineName", "Frontend", "入库机器名称", "Inbound Machine Name", "入庫マシン名"),
            new("Logistics.Serials.ProdSerialScanningEx.InboundLocation", "Frontend", "入库地点", "Inbound Location", "入庫場所"),
            new("Logistics.Serials.ProdSerialScanningEx.InboundOs", "Frontend", "入库OS", "Inbound OS", "入庫OS"),
            new("Logistics.Serials.ProdSerialScanningEx.InboundDesc", "Frontend", "入库异常描述", "Inbound Exception Description", "入庫異常説明"),
            new("Logistics.Serials.ProdSerialScanningEx.OutboundNo", "Frontend", "出库单号", "Outbound No.", "出庫番号"),
            new("Logistics.Serials.ProdSerialScanningEx.DestCode", "Frontend", "仕向编码", "Destination Code", "仕向コード"),
            new("Logistics.Serials.ProdSerialScanningEx.DestPort", "Frontend", "目的地港口", "Destination Port", "目的地港"),
            new("Logistics.Serials.ProdSerialScanningEx.OutboundDate", "Frontend", "出库日期", "Outbound Date", "出庫日"),
            new("Logistics.Serials.ProdSerialScanningEx.OutboundFullSerialNumber", "Frontend", "出库完整序列号", "Outbound Full Serial Number", "出庫完全シリアル番号"),
            new("Logistics.Serials.ProdSerialScanningEx.OutboundClient", "Frontend", "出库用户", "Outbound User", "出庫ユーザー"),
            new("Logistics.Serials.ProdSerialScanningEx.OutboundIp", "Frontend", "出库IP", "Outbound IP", "出庫IP"),
            new("Logistics.Serials.ProdSerialScanningEx.OutboundMachineName", "Frontend", "出库机器名称", "Outbound Machine Name", "出庫マシン名"),
            new("Logistics.Serials.ProdSerialScanningEx.OutboundLocation", "Frontend", "出库地点", "Outbound Location", "出庫場所"),
            new("Logistics.Serials.ProdSerialScanningEx.OutboundOs", "Frontend", "出库OS", "Outbound OS", "出庫OS"),
            new("Logistics.Serials.ProdSerialScanningEx.OutboundDesc", "Frontend", "出库异常描述", "Outbound Exception Description", "出庫異常説明"),
            new("Logistics.Serials.ProdSerialScanningEx.Keyword", "Frontend", "入库完整序列号、入库日期、入库用户、入库IP、入库机器名称、入库地点、入库OS、入库异常描述、出库单号、仕向编码、目的地港口、出库日期、出库完整序列号、出库用户、出库IP、出库机器名称、出库地点、出库OS、出库异常描述", "inbound full serial number, inbound date, inbound user, inbound IP, inbound machine name, inbound location, inbound OS, inbound exception description, outbound no., destination code, destination port, outbound date, outbound full serial number, outbound user, outbound IP, outbound machine name, outbound location, outbound OS, outbound exception description", "入庫完全シリアル番号、入庫日、入庫ユーザー、入庫IP、入庫マシン名、入庫場所、入庫OS、入庫異常説明、出庫番号、仕向コード、目的地港、出庫日、出庫完全シリアル番号、出庫ユーザー、出庫IP、出庫マシン名、出庫場所、出庫OS、出庫異常説明"),

            // 访客实体字段
            new("Logistics.Visitors.Visitor", "Frontend", "访客", "Visitor", "訪問者"),
            new("Logistics.Visitors.Visitor.CompanyName", "Frontend", "公司名称", "Company Name", "会社名"),
            new("Logistics.Visitors.Visitor.StartTime", "Frontend", "起始时间", "Start Time", "開始時間"),
            new("Logistics.Visitors.Visitor.EndTime", "Frontend", "结束时间", "End Time", "終了時間"),
            new("Logistics.Visitors.Visitor.Keyword", "Frontend", "公司名称、起始时间、结束时间", "company name, start time, end time", "会社名、開始時間、終了時間"),

            // 访客详情实体字段
            new("Logistics.Visitors.VisitorDetail", "Frontend", "访客详情", "Visitor Detail", "訪問者詳細"),
            new("Logistics.Visitors.VisitorDetail.VisitorId", "Frontend", "访客ID", "Visitor ID", "訪問者ID"),
            new("Logistics.Visitors.VisitorDetail.Department", "Frontend", "部门", "Department", "部署"),
            new("Logistics.Visitors.VisitorDetail.Name", "Frontend", "姓名", "Name", "氏名"),
            new("Logistics.Visitors.VisitorDetail.Position", "Frontend", "职务", "Position", "職務"),
            new("Logistics.Visitors.VisitorDetail.Keyword", "Frontend", "访客ID、部门、姓名、职务", "visitor id, department, name, position", "訪問者ID、部署、氏名、職務"),

            // 代码生成表配置实体字段
            new("Generator.GenTable", "Frontend", "代码生成表配置", "Code Generation Table Config", "コード生成テーブル設定"),
            new("Generator.GenTable.TableName", "Frontend", "库表名称", "Table Name", "テーブル名"),
            new("Generator.GenTable.TableNameHint", "Frontend", "提示：用于无数据表的手动配置，表名可以不存在于数据库中", "Hint: For manual configuration without database table, table name may not exist in database", "ヒント：データベーステーブルなしの手動設定用。テーブル名はデータベースに存在しない場合があります"),
            new("Generator.GenTable.TableDescription", "Frontend", "库表描述", "Table Description", "テーブル説明"),
            new("Generator.GenTable.ClassName", "Frontend", "实体类名称", "Class Name", "クラス名"),
            new("Generator.GenTable.Namespace", "Frontend", "命名空间", "Namespace", "名前空間"),
            new("Generator.GenTable.ModuleCode", "Frontend", "模块标识", "Module Code", "モジュールコード"),
            new("Generator.GenTable.ModuleName", "Frontend", "模块名称", "Module Name", "モジュール名"),
            new("Generator.GenTable.ParentTableId", "Frontend", "主表ID", "Parent Table ID", "親テーブルID"),
            new("Generator.GenTable.DetailTableName", "Frontend", "子表名称", "Detail Table Name", "詳細テーブル名"),
            new("Generator.GenTable.DetailComment", "Frontend", "子表描述", "Detail Comment", "詳細コメント"),
            new("Generator.GenTable.DetailRelationField", "Frontend", "子表关联字段", "Detail Relation Field", "詳細関連フィールド"),
            new("Generator.GenTable.TreeCodeField", "Frontend", "树编码字段", "Tree Code Field", "ツリーコードフィールド"),
            new("Generator.GenTable.TreeParentCodeField", "Frontend", "树父编码字段", "Tree Parent Code Field", "ツリー親コードフィールド"),
            new("Generator.GenTable.TreeNameField", "Frontend", "树名称字段", "Tree Name Field", "ツリー名フィールド"),
            new("Generator.GenTable.Author", "Frontend", "作者", "Author", "作成者"),
            new("Generator.GenTable.TemplateType", "Frontend", "生成模板类型", "Template Type", "テンプレートタイプ"),
            new("Generator.GenTable.GenNamespacePrefix", "Frontend", "命名空间前缀", "Namespace Prefix", "名前空間プレフィックス"),
            new("Generator.GenTable.GenBusinessName", "Frontend", "生成业务名称", "Business Name", "ビジネス名"),
            new("Generator.GenTable.GenModuleName", "Frontend", "生成模块名称", "Gen Module Name", "生成モジュール名"),
            new("Generator.GenTable.GenFunctionName", "Frontend", "生成功能名", "Function Name", "機能名"),
            new("Generator.GenTable.GenType", "Frontend", "生成方式", "Gen Type", "生成タイプ"),
            new("Generator.GenTable.GenFunctions", "Frontend", "生成功能", "Gen Functions", "生成機能"),
            new("Generator.GenTable.GenPath", "Frontend", "代码生成路径", "Gen Path", "生成パス"),
            new("Generator.GenTable.Options", "Frontend", "其它生成选项", "Options", "その他のオプション"),
            new("Generator.GenTable.ParentMenuName", "Frontend", "上级菜单名称", "Parent Menu Name", "親メニュー名"),
            new("Generator.GenTable.PermissionPrefix", "Frontend", "权限前缀", "Permission Prefix", "権限プレフィックス"),
            new("Generator.GenTable.IsDatabaseTable", "Frontend", "是否有表", "Is Database Table", "テーブルがあるか"),
            new("Generator.GenTable.IsGenMenu", "Frontend", "是否生成菜单", "Is Gen Menu", "メニュー生成か"),
            new("Generator.GenTable.IsGenTranslation", "Frontend", "是否生成翻译", "Is Gen Translation", "翻訳生成か"),
            new("Generator.GenTable.IsGenCode", "Frontend", "是否生成代码", "Is Gen Code", "コード生成か"),
            new("Generator.GenTable.DefaultSortField", "Frontend", "默认排序字段", "Default Sort Field", "デフォルトソートフィールド"),
            new("Generator.GenTable.DefaultSortOrder", "Frontend", "默认排序", "Default Sort Order", "デフォルトソート順"),
            new("Generator.GenTable.Keyword", "Frontend", "库表名称、库表描述、实体类名称、模块标识、模块名称", "table name, table description, class name, module code, module name", "テーブル名、テーブル説明、クラス名、モジュールコード、モジュール名"),
            new("Generator.GenTable.BasicInfo", "Frontend", "基本信息", "Basic Information", "基本情報"),
            new("Generator.GenTable.GenConfig", "Frontend", "生成配置", "Generation Config", "生成設定"),
            new("Generator.GenTable.ColumnInfo", "Frontend", "列配置", "Column Config", "カラム設定"),
            new("Generator.GenTable.Create", "Frontend", "新建代码生成配置", "Create Code Generation Config", "コード生成設定を作成"),
            new("Generator.GenTable.Update", "Frontend", "编辑代码生成配置", "Edit Code Generation Config", "コード生成設定を編集"),
            new("Generator.GenTable.DeleteConfirm", "Frontend", "确定要删除该代码生成配置吗？", "Are you sure you want to delete this code generation config?", "このコード生成設定を削除してもよろしいですか？"),
            new("Generator.GenTable.SyncTitle", "Frontend", "选择同步方向", "Select Sync Direction", "同期方向を選択"),
            new("Generator.GenTable.SyncFromDatabase", "Frontend", "从数据库同步到配置", "Sync from Database to Config", "データベースから設定へ同期"),
            new("Generator.GenTable.SyncToDatabase", "Frontend", "从配置同步到数据库", "Sync from Config to Database", "設定からデータベースへ同期"),
            new("Generator.GenTable.SyncFromDatabaseConfirm", "Frontend", "确定要从数据库同步表结构到配置吗？", "Are you sure you want to sync table structure from database to config?", "データベースから設定へテーブル構造を同期してもよろしいですか？"),
            new("Generator.GenTable.SyncToDatabaseConfirm", "Frontend", "确定要从配置同步表结构到数据库吗？", "Are you sure you want to sync table structure from config to database?", "設定からデータベースへテーブル構造を同期してもよろしいですか？"),
            new("Generator.GenTable.SyncFromDatabaseSuccess", "Frontend", "从数据库同步成功", "Sync from database successful", "データベースから同期成功"),
            new("Generator.GenTable.SyncToDatabaseSuccess", "Frontend", "同步到数据库成功", "Sync to database successful", "データベースへ同期成功"),

            // 代码生成列配置实体字段
            new("Generator.GenColumn", "Frontend", "代码生成列配置", "Code Generation Column Config", "コード生成カラム設定"),           
            new("Generator.GenColumn.TableName", "Frontend", "表名", "Table Name", "テーブル名"),
            new("Generator.GenColumn.ColumnName", "Frontend", "列名", "Column Name", "カラム名"),
            new("Generator.GenColumn.ColumnDescription", "Frontend", "列描述", "Column Description", "カラム説明"),
            new("Generator.GenColumn.ColumnDataType", "Frontend", "库列类型", "Column Data Type", "カラムデータタイプ"),
            new("Generator.GenColumn.PropertyName", "Frontend", "属性名称", "Property Name", "プロパティ名"),
            new("Generator.GenColumn.DataType", "Frontend", "C#类型", "C# Type", "C#タイプ"),
            new("Generator.GenColumn.IsNullable", "Frontend", "可空", "Nullable", "NULL許可"),
            new("Generator.GenColumn.IsPrimaryKey", "Frontend", "主键", "Primary Key", "主キー"),
            new("Generator.GenColumn.IsIdentity", "Frontend", "自增", "Identity", "自動増分"),
            new("Generator.GenColumn.Length", "Frontend", "长度", "Length", "長さ"),
            new("Generator.GenColumn.DecimalPlaces", "Frontend", "精度", "Decimal Places", "小数点以下桁数"),
            new("Generator.GenColumn.DefaultValue", "Frontend", "默认值", "Default Value", "デフォルト値"),
            new("Generator.GenColumn.OrderNum", "Frontend", "库列排序", "Order Number", "順序"),
            new("Generator.GenColumn.IsQuery", "Frontend", "查询", "Query", "クエリ"),
            new("Generator.GenColumn.QueryType", "Frontend", "查询方式", "Query Type", "クエリタイプ"),
            new("Generator.GenColumn.IsCreate", "Frontend", "创建", "Create", "作成"),
            new("Generator.GenColumn.IsUpdate", "Frontend", "更新", "Update", "更新"),
            new("Generator.GenColumn.IsDelete", "Frontend", "删除", "Delete", "削除"),
            new("Generator.GenColumn.IsList", "Frontend", "列表", "List", "リスト"),
            new("Generator.GenColumn.IsExport", "Frontend", "导出", "Export", "エクスポート"),
            new("Generator.GenColumn.IsSort", "Frontend", "排序", "Sort", "ソート"),
            new("Generator.GenColumn.IsRequired", "Frontend", "必填", "Required", "必須"),
            new("Generator.GenColumn.IsForm", "Frontend", "表单显示", "Form Display", "フォーム表示"),
            new("Generator.GenColumn.FormControlType", "Frontend", "表单类型", "Form Type", "フォームタイプ"),
            new("Generator.GenColumn.DictType", "Frontend", "字典类型", "Dict Type", "辞書タイプ"),
            new("Generator.GenColumn.Keyword", "Frontend", "表名、列名、属性名称、列描述", "table name, column name, property name, column description", "テーブル名、カラム名、プロパティ名、カラム説明"),

            // 代码生成导入表相关
            new("Generator.ImportTable", "Frontend", "导入表", "Import Table", "テーブルをインポート"),
            new("Generator.ImportTable.Title", "Frontend", "导入数据库表", "Import Database Table", "データベーステーブルをインポート"),
            new("Generator.ImportTable.TableName", "Frontend", "表名", "Table Name", "テーブル名"),
            new("Generator.ImportTable.Description", "Frontend", "表描述", "Table Description", "テーブル説明"),
            new("Generator.ImportTable.ColumnName", "Frontend", "列名", "Column Name", "列名"),
            new("Generator.ImportTable.ColumnDescription", "Frontend", "列描述", "Column Description", "列説明"),
            new("Generator.ImportTable.DataType", "Frontend", "数据类型", "Data Type", "データ型"),
            new("Generator.ImportTable.Length", "Frontend", "长度", "Length", "長さ"),
            new("Generator.ImportTable.DecimalPlaces", "Frontend", "精度", "Decimal Places", "小数点以下桁数"),
            new("Generator.ImportTable.DefaultValue", "Frontend", "默认值", "Default Value", "デフォルト値"),
            new("Generator.ImportTable.IsPrimaryKey", "Frontend", "主键", "Primary Key", "主キー"),
            new("Generator.ImportTable.IsIdentity", "Frontend", "自增", "Identity", "自動増分"),
            new("Generator.ImportTable.IsNullable", "Frontend", "可空", "Nullable", "NULL許可"),
            new("Generator.ImportTable.NoSelection", "Frontend", "请至少选择一个表", "Please select at least one table", "少なくとも1つのテーブルを選択してください"),
        };
    }

    private sealed record TranslationSeed(string Key, string Module, string ZhCn, string EnUs, string JaJp);
}

