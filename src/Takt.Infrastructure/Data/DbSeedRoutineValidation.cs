// ========================================
// 项目名称：节拍(Takt)中小企业管理平台 · Takt SMEs Platform
// 命名空间：Takt.Infrastructure.Data
// 文件名称：DbSeedRoutineValidation.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：验证消息翻译种子数据
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
/// 验证消息翻译种子初始化器
/// </summary>
public class DbSeedRoutineValidation
{
    private readonly InitLogManager _initLog;
    private readonly IBaseRepository<Translation> _translationRepository;
    private readonly IBaseRepository<Language> _languageRepository;

    public DbSeedRoutineValidation(
        InitLogManager initLog,
        IBaseRepository<Translation> translationRepository,
        IBaseRepository<Language> languageRepository)
    {
        _initLog = initLog ?? throw new ArgumentNullException(nameof(initLog));
        _translationRepository = translationRepository ?? throw new ArgumentNullException(nameof(translationRepository));
        _languageRepository = languageRepository ?? throw new ArgumentNullException(nameof(languageRepository));
    }

    /// <summary>
    /// 初始化验证消息翻译
    /// </summary>
    public void Run()
    {
        var languages = _languageRepository.AsQueryable().ToList();
        if (languages.Count == 0)
        {
            _initLog.Warning("语言数据为空，跳过验证消息翻译初始化");
            return;
        }

        var zhCn = languages.FirstOrDefault(l => l.LanguageCode == "zh-CN");
        var enUs = languages.FirstOrDefault(l => l.LanguageCode == "en-US");
        var jaJp = languages.FirstOrDefault(l => l.LanguageCode == "ja-JP");

        if (zhCn == null || enUs == null || jaJp == null)
        {
            _initLog.Warning("语言数据不完整（zh-CN/en-US/ja-JP 缺失），跳过验证消息翻译初始化");
            return;
        }

        foreach (var seed in BuildValidationSeeds())
        {
            CreateOrUpdateTranslation(zhCn, seed.Key, seed.Module, seed.ZhCn);
            CreateOrUpdateTranslation(enUs, seed.Key, seed.Module, seed.EnUs);
            CreateOrUpdateTranslation(jaJp, seed.Key, seed.Module, seed.JaJp);
        }

        _initLog.Information("✅ 验证消息翻译初始化完成");
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

    private static List<ValidationSeed> BuildValidationSeeds()
    {
        return new List<ValidationSeed>
        {
            // 用户验证相关（仅保留特定业务规则，通用验证使用 validation.required/format/maxLength/minLength/invalid）
            new("Identity.User.Validation.UsernameRequired", "Frontend", "用户名不能为空", "Username cannot be empty", "ユーザー名は空にできません"),
            new("Identity.User.Validation.UsernameMinLength", "Frontend", "用户名长度不能少于4位", "Username must be at least 4 characters", "ユーザー名は4文字以上である必要があります"),
            new("Identity.User.Validation.UsernameMaxLength", "Frontend", "用户名长度不能超过10位", "Username cannot exceed 10 characters", "ユーザー名は10文字を超えることはできません"),
            new("Identity.User.Validation.UsernameInvalid", "Frontend", "用户名必须以小写字母开头，只能包含小写字母和数字，长度4-10位", "Username must start with a lowercase letter and contain only lowercase letters and numbers, 4-10 characters", "ユーザー名は小文字で始まり、小文字と数字のみを含む必要があります（4-10文字）"),
            new("Identity.User.Validation.UsernamePasswordRequired", "Frontend", "用户名和密码不能为空", "Username and password cannot be empty", "ユーザー名とパスワードは空にできません"),
            new("Identity.User.Validation.RealNameRequired", "Frontend", "真实姓名不能为空", "Real name cannot be empty", "本名は空にできません"),
            new("Identity.User.Validation.RealNameHint", "Frontend", "不允许数字、点号、空格开头，英文字母首字母大写，30字以内", "Cannot start with digits, dots, or spaces. English letters must be uppercase. Max 30 characters", "数字、ドット、スペースで始めることはできません。英語の文字は大文字である必要があります。最大30文字"),
            new("Identity.User.Validation.RealNameInvalid", "Frontend", "不允许数字、点号、空格开头，英文字母首字母必须大写", "Cannot start with digits, dots, or spaces. English letters must be uppercase", "数字、ドット、スペースで始めることはできません。英語の文字は大文字である必要があります"),
            new("Identity.User.Validation.NicknameRequired", "Frontend", "昵称不能为空", "Nickname cannot be empty", "ニックネームは空にできません"),
            new("Identity.User.Validation.NicknameMaxLength", "Frontend", "昵称长度不能超过40个字符", "Nickname cannot exceed 40 characters", "ニックネームは40文字を超えることはできません"),
            new("Identity.User.Validation.NicknameInvalid", "Frontend", "昵称不允许数字、点号、空格开头，如果首字符是英文字母则必须是大写，允许字母、数字、点和空格，支持中文、日文、韩文、越南文等，如：Cheng.Jianhong、Joseph Robinette Biden Jr. 或 张三", "Nickname cannot start with digits, dots, or spaces, if the first character is an English letter it must be uppercase, allow letters, numbers, dots, and spaces, support Chinese, Japanese, Korean, Vietnamese, etc., e.g., Cheng.Jianhong, Joseph Robinette Biden Jr. or 张三", "ニックネームは数字、ドット、スペースで始めることはできません。最初の文字が英語の文字の場合は大文字である必要があります。文字、数字、ドット、スペースを含むことができ、中国語、日本語、韓国語、ベトナム語などをサポートします。例：Cheng.Jianhong、Joseph Robinette Biden Jr. または 张三"),
            new("Identity.User.Validation.EmailRequired", "Frontend", "邮箱不能为空", "Email cannot be empty", "メールアドレスは空にできません"),
            new("Identity.User.Validation.EmailInvalid", "Frontend", "邮箱格式不正确", "Invalid email format", "メールアドレスの形式が正しくありません"),
            new("Identity.User.Validation.PhoneRequired", "Frontend", "手机号不能为空", "Phone cannot be empty", "電話番号は空にできません"),
            new("Identity.User.Validation.PhoneInvalid", "Frontend", "手机号格式不正确，必须是11位数字，以1开头，第二位为3-9", "Invalid phone number format. Must be 11 digits starting with 1, second digit 3-9", "携帯番号の形式が正しくありません。1で始まり、2桁目が3-9の11桁の数字である必要があります"),
            new("Identity.User.Validation.AvatarRequired", "Frontend", "头像不能为空", "Avatar cannot be empty", "アバターは空にできません"),
            new("Identity.User.Validation.AvatarMustBeRelativePath", "Frontend", "头像必须是相对路径，不能使用绝对路径或URL", "Avatar must be a relative path, absolute paths or URLs are not allowed", "アバターは相対パスである必要があります。絶対パスやURLは使用できません"),
            new("Identity.User.Validation.AvatarPathTooLong", "Frontend", "头像路径长度不能超过256个字符", "Avatar path cannot exceed 256 characters", "アバターパスは256文字を超えることはできません"),
            new("Identity.User.Validation.PasswordRequired", "Frontend", "密码不能为空", "Password cannot be empty", "パスワードは空にできません"),
            new("Identity.User.Validation.PasswordMinLength", "Frontend", "密码长度不能少于6位", "Password must be at least 6 characters", "パスワードは少なくとも6文字以上である必要があります"),
            new("Identity.User.Validation.PasswordConfirmRequired", "Frontend", "确认密码不能为空", "Password confirmation cannot be empty", "パスワード確認は空にできません"),
            new("Identity.User.Validation.PasswordMismatch", "Frontend", "两次输入的密码不一致", "The two passwords do not match", "2つのパスワードが一致しません"),
            new("Identity.User.Validation.PasswordConfirmHint", "Frontend", "请再次输入密码以确认", "Please enter password again to confirm", "確認のため、パスワードを再度入力してください"),
            new("Identity.User.Validation.RemarksHint", "Frontend", "请输入备注信息（可选）", "Please enter remarks (optional)", "備考情報を入力してください（オプション）"),
            new("Identity.User.Validation.UserTypeHint", "Frontend", "请选择用户类型", "Please select user type", "ユーザー種別を選択してください"),
            new("Identity.User.Validation.UserGenderHint", "Frontend", "请选择性别", "Please select gender", "性別を選択してください"),
            new("Identity.User.Validation.UserStatusHint", "Frontend", "请选择用户状态", "Please select user status", "ユーザー状態を選択してください"),
            new("Identity.User.Validation.AvatarHint", "Frontend", "请选择头像，必须是相对路径", "Please select avatar, must be a relative path", "アバターを選択してください。相対パスである必要があります"),

            // 角色验证相关
            new("Identity.Role.Validation.RoleNameRequired", "Frontend", "角色名称不能为空", "Role name cannot be empty", "ロール名は空にできません"),
            new("Identity.Role.Validation.RoleNameMaxLength", "Frontend", "角色名称长度不能超过128个字符", "Role name cannot exceed 128 characters", "ロール名は128文字を超えることはできません"),
            new("Identity.Role.Validation.RoleNameInvalid", "Frontend", "角色名称不能为空，长度不能超过128个字符", "Role name cannot be empty and cannot exceed 128 characters", "ロール名は空にできず、128文字を超えることはできません"),
            new("Identity.Role.Validation.RoleNameHint", "Frontend", "角色名称不能为空，长度不能超过128个字符", "Role name cannot be empty, length cannot exceed 128 characters", "ロール名は空にできず、128文字を超えることはできません"),
            new("Identity.Role.Validation.RoleCodeRequired", "Frontend", "角色编码不能为空", "Role code cannot be empty", "ロールコードは空にできません"),
            new("Identity.Role.Validation.RoleCodeMaxLength", "Frontend", "角色编码长度不能超过10个字符", "Role code cannot exceed 10 characters", "ロールコードは10文字を超えることはできません"),
            new("Identity.Role.Validation.RoleCodeInvalid", "Frontend", "角色编码不能为空，长度不能超过10个字符，只能包含小写字母、数字和下划线", "Role code cannot be empty, cannot exceed 10 characters, and can only contain lowercase letters, numbers, and underscores", "ロールコードは空にできず、10文字を超えることはできず、小文字、数字、アンダースコアのみを含むことができます"),
            new("Identity.Role.Validation.RoleCodeHint", "Frontend", "角色编码不能为空，长度不能超过10个字符，只能包含小写字母、数字和下划线", "Role code cannot be empty, length cannot exceed 10 characters, only lowercase letters, numbers and underscores", "ロールコードは空にできず、10文字を超えることはできず、小文字、数字、アンダースコアのみを含むことができます"),
            new("Identity.Role.Validation.DescriptionHint", "Frontend", "请输入角色描述（可选）", "Please enter role description (optional)", "ロール説明を入力してください（オプション）"),
            new("Identity.Role.Validation.RoleStatusHint", "Frontend", "请选择角色状态", "Please select role status", "ロール状態を選択してください"),
            new("Identity.Role.Validation.OrderNumHint", "Frontend", "请输入排序号，数字越小越靠前", "Please enter order number, smaller numbers appear first", "順序番号を入力してください。数字が小さいほど前に表示されます"),
            new("Identity.Role.Validation.RemarksHint", "Frontend", "请输入备注信息（可选）", "Please enter remarks (optional)", "備考情報を入力してください（オプション）"),

            // 菜单验证相关
            new("Identity.Menu.Validation.MenuNameRequired", "Frontend", "菜单名称不能为空", "Menu name cannot be empty", "メニュー名は空にできません"),
            new("Identity.Menu.Validation.MenuNameMaxLength", "Frontend", "菜单名称长度不能超过128个字符", "Menu name cannot exceed 128 characters", "メニュー名は128文字を超えることはできません"),
            new("Identity.Menu.Validation.MenuCodeRequired", "Frontend", "菜单编码不能为空", "Menu code cannot be empty", "メニューコードは空にできません"),
            new("Identity.Menu.Validation.MenuCodeMaxLength", "Frontend", "菜单编码长度不能超过50个字符", "Menu code cannot exceed 50 characters", "メニューコードは50文字を超えることはできません"),
            new("Identity.Menu.Validation.MenuNameHint", "Frontend", "请输入菜单名称", "Please enter menu name", "メニュー名を入力してください"),
            new("Identity.Menu.Validation.MenuCodeHint", "Frontend", "请输入菜单编码，建议使用小写字母和下划线", "Please enter menu code, lowercase letters and underscores recommended", "メニューコードを入力してください。小文字とアンダースコアを推奨します"),
            new("Identity.Menu.Validation.I18nKeyHint", "Frontend", "请输入国际化键（可选）", "Please enter i18n key (optional)", "多言語キーを入力してください（オプション）"),
            new("Identity.Menu.Validation.PermCodeHint", "Frontend", "请输入权限码（可选）", "Please enter permission code (optional)", "権限コードを入力してください（オプション）"),
            new("Identity.Menu.Validation.RoutePathHint", "Frontend", "请输入路由路径（可选）", "Please enter route path (optional)", "ルートパスを入力してください（オプション）"),
            new("Identity.Menu.Validation.IconHint", "Frontend", "请输入图标名称或路径（可选）", "Please enter icon name or path (optional)", "アイコン名またはパスを入力してください（オプション）"),
            new("Identity.Menu.Validation.ComponentHint", "Frontend", "请输入组件路径（可选）", "Please enter component path (optional)", "コンポーネントパスを入力してください（オプション）"),
            new("Identity.Menu.Validation.ParentIdHint", "Frontend", "父级菜单ID（0表示顶级菜单，留空表示0）", "Parent menu ID (0 for top-level menu, empty means 0)", "親メニューID（0はトップレベルメニュー、空欄は0を意味します）"),
            new("Identity.Menu.Validation.MenuTypeHint", "Frontend", "请选择菜单类型", "Please select menu type", "メニュー種別を選択してください"),
            new("Identity.Menu.Validation.MenuStatusHint", "Frontend", "请选择菜单状态", "Please select menu status", "メニュー状態を選択してください"),
            new("Identity.Menu.Validation.OrderNumHint", "Frontend", "请输入排序号，数字越小越靠前", "Please enter order number, smaller numbers appear first", "順序番号を入力してください。数字が小さいほど前に表示されます"),
            new("Identity.Menu.Validation.RemarksHint", "Frontend", "请输入备注信息（可选）", "Please enter remarks (optional)", "備考情報を入力してください（オプション）"),

            // 字典验证相关
            new("Routine.Dictionary.Validation.TypeCodeHint", "Frontend", "请输入类型代码，格式：xxx_xxx_xxx，只包含小写字母和数字，且不能以数字开头", "Please enter type code, format: xxx_xxx_xxx, only lowercase letters and numbers, cannot start with a number", "タイプコードを入力してください。形式：xxx_xxx_xxx、小文字と数字のみ、数字で始めることはできません"),
            new("Routine.Dictionary.Validation.TypeNameHint", "Frontend", "请输入类型名称", "Please enter type name", "タイプ名を入力してください"),
            new("Routine.Dictionary.Validation.DataSourceHint", "Frontend", "请选择数据源类型", "Please select data source type", "データソースタイプを選択してください"),
            new("Routine.Dictionary.Validation.SqlScriptHint", "Frontend", "当数据源为SQL时，请输入SQL脚本", "When data source is SQL, please enter SQL script", "データソースがSQLの場合、SQLスクリプトを入力してください"),
            new("Routine.Dictionary.Validation.TypeStatusHint", "Frontend", "请选择类型状态", "Please select type status", "タイプ状態を選択してください"),
            new("Routine.Dictionary.Validation.OrderNumHint", "Frontend", "请输入排序号，数字越小越靠前", "Please enter order number, smaller numbers appear first", "順序番号を入力してください。数字が小さいほど前に表示されます"),
            new("Routine.Dictionary.Validation.RemarksHint", "Frontend", "请输入备注信息（可选）", "Please enter remarks (optional)", "備考情報を入力してください（オプション）"),

            // 设置验证相关
            new("Routine.Setting.Validation.KeyRequired", "Frontend", "配置键不能为空", "Setting key cannot be empty", "設定キーは空にできません"),
            new("Routine.Setting.Validation.KeyMaxLength", "Frontend", "配置键长度不能超过200个字符", "Setting key cannot exceed 200 characters", "設定キーは200文字を超えることはできません"),
            new("Routine.Setting.Validation.ValueRequired", "Frontend", "配置值不能为空", "Setting value cannot be empty", "設定値は空にできません"),
            new("Routine.Setting.Validation.KeyHint", "Frontend", "请输入配置键，建议使用小写字母、数字和下划线", "Please enter setting key, lowercase letters, numbers and underscores recommended", "設定キーを入力してください。小文字、数字、アンダースコアを推奨します"),
            new("Routine.Setting.Validation.ValueHint", "Frontend", "请输入配置值", "Please enter setting value", "設定値を入力してください"),
            new("Routine.Setting.Validation.CategoryHint", "Frontend", "请输入分类（可选）", "Please enter category (optional)", "カテゴリを入力してください（オプション）"),
            new("Routine.Setting.Validation.TypeHint", "Frontend", "请选择配置类型", "Please select setting type", "設定タイプを選択してください"),

            // 翻译验证相关
            new("Routine.Translation.Validation.LanguageCodeRequired", "Frontend", "语言代码不能为空", "Language code cannot be empty", "言語コードは空にできません"),
            new("Routine.Translation.Validation.TranslationKeyRequired", "Frontend", "翻译键不能为空", "Translation key cannot be empty", "翻訳キーは空にできません"),
            new("Routine.Translation.Validation.TranslationValueRequired", "Frontend", "翻译值不能为空", "Translation value cannot be empty", "翻訳値は空にできません"),
            new("Routine.Translation.Validation.LanguageCodeHint", "Frontend", "请选择语言代码", "Please select language code", "言語コードを選択してください"),
            new("Routine.Translation.Validation.TranslationKeyHint", "Frontend", "请输入翻译键", "Please enter translation key", "翻訳キーを入力してください"),
            new("Routine.Translation.Validation.TranslationValueHint", "Frontend", "请输入翻译值", "Please enter translation value", "翻訳値を入力してください"),
            new("Routine.Translation.Validation.ModuleHint", "Frontend", "请输入模块名称（可选）", "Please enter module name (optional)", "モジュール名を入力してください（オプション）"),

            // 字典验证相关（验证消息）
            new("Routine.Dictionary.Validation.TypeCodeRequired", "Frontend", "类型代码不能为空", "Type code cannot be empty", "タイプコードは空にできません"),
            new("Routine.Dictionary.Validation.TypeCodeMaxLength", "Frontend", "类型代码长度不能超过50个字符", "Type code cannot exceed 50 characters", "タイプコードは50文字を超えることはできません"),
            new("Routine.Dictionary.Validation.TypeCodeFormat", "Frontend", "类型代码格式不正确，必须是 xxx_xxx_xxx 格式，只包含小写字母和数字，且不能以数字开头", "Type code format is incorrect. Must be xxx_xxx_xxx format, only lowercase letters and numbers, and cannot start with a number", "タイプコードの形式が正しくありません。xxx_xxx_xxx形式である必要があり、小文字と数字のみを含み、数字で始めることはできません"),
            new("Routine.Dictionary.Validation.TypeNameRequired", "Frontend", "类型名称不能为空", "Type name cannot be empty", "タイプ名は空にできません"),
            new("Routine.Dictionary.Validation.TypeNameMaxLength", "Frontend", "类型名称长度不能超过100个字符", "Type name cannot exceed 100 characters", "タイプ名は100文字を超えることはできません"),
            new("Routine.Dictionary.Validation.TypeStatusInvalid", "Frontend", "类型状态无效，必须是0或1", "Type status is invalid, must be 0 or 1", "タイプ状態が無効です。0または1である必要があります"),
            new("Routine.Dictionary.Validation.SqlScriptRequired", "Frontend", "SQL脚本不能为空", "SQL script cannot be empty", "SQLスクリプトは空にできません"),

            // 序列号出库验证相关
            new("Logistics.Serials.ProdSerialOutbound.Validation.OutboundNoRequired", "Frontend", "出库单号不能为空", "Outbound number cannot be empty", "出庫番号は空にできません"),
            new("Logistics.Serials.ProdSerialOutbound.Validation.OutboundNoMaxLength", "Frontend", "出库单号长度不能超过50个字符", "Outbound number cannot exceed 50 characters", "出庫番号は50文字を超えることはできません"),
            new("Logistics.Serials.ProdSerialOutbound.Validation.OutboundDateRequired", "Frontend", "出库日期不能为空", "Outbound date cannot be empty", "出庫日は空にできません"),
            new("Logistics.Serials.ProdSerialOutbound.Validation.DestPortMaxLength", "Frontend", "目的地港口长度不能超过100个字符", "Destination port cannot exceed 100 characters", "目的地港は100文字を超えることはできません"),
            new("Logistics.Serials.ProdSerialOutbound.Validation.FullSerialNumberRequired", "Frontend", "完整序列号不能为空", "Full serial number cannot be empty", "完全シリアル番号は空にできません"),
            new("Logistics.Serials.ProdSerialOutbound.Validation.FullSerialNumberMaxLength", "Frontend", "完整序列号长度不能超过200个字符", "Full serial number cannot exceed 200 characters", "完全シリアル番号は200文字を超えることはできません"),
            new("Logistics.Serials.ProdSerialOutbound.Validation.OutboundNoHint", "Frontend", "请输入出库单号", "Please enter outbound number", "出庫番号を入力してください"),
            new("Logistics.Serials.ProdSerialOutbound.Validation.OutboundDateHint", "Frontend", "请选择出库日期", "Please select outbound date", "出庫日を選択してください"),
            new("Logistics.Serials.ProdSerialOutbound.Validation.DestCodeHint", "Frontend", "请输入仕向编码（可选）", "Please enter destination code (optional)", "仕向コードを入力してください（オプション）"),
            new("Logistics.Serials.ProdSerialOutbound.Validation.DestPortHint", "Frontend", "请输入目的地港口（可选）", "Please enter destination port (optional)", "目的地港を入力してください（オプション）"),
            new("Logistics.Serials.ProdSerialOutbound.Validation.FullSerialNumberHint", "Frontend", "请输入完整序列号", "Please enter full serial number", "完全シリアル番号を入力してください"),
            new("Logistics.Serials.ProdSerialOutbound.Validation.MaterialCodeHint", "Frontend", "请输入物料编码（可选）", "Please enter material code (optional)", "資材コードを入力してください（オプション）"),
            new("Logistics.Serials.ProdSerialOutbound.Validation.SerialNumberHint", "Frontend", "请输入真正序列号（可选）", "Please enter serial number (optional)", "シリアル番号を入力してください（オプション）"),
            new("Logistics.Serials.ProdSerialOutbound.Validation.QuantityHint", "Frontend", "请输入数量", "Please enter quantity", "数量を入力してください"),

            // 序列号入库验证相关
            new("Logistics.Serials.ProdSerialInbound.Validation.FullSerialNumberRequired", "Frontend", "完整序列号不能为空", "Full serial number cannot be empty", "完全シリアル番号は空にできません"),
            new("Logistics.Serials.ProdSerialInbound.Validation.FullSerialNumberMaxLength", "Frontend", "完整序列号长度不能超过200个字符", "Full serial number cannot exceed 200 characters", "完全シリアル番号は200文字を超えることはできません"),
            new("Logistics.Serials.ProdSerialInbound.Validation.FullSerialNumberHint", "Frontend", "请输入完整序列号", "Please enter full serial number", "完全シリアル番号を入力してください"),
            new("Logistics.Serials.ProdSerialInbound.Validation.MaterialCodeHint", "Frontend", "请输入物料编码（可选）", "Please enter material code (optional)", "資材コードを入力してください（オプション）"),
            new("Logistics.Serials.ProdSerialInbound.Validation.SerialNumberHint", "Frontend", "请输入真正序列号（可选）", "Please enter serial number (optional)", "シリアル番号を入力してください（オプション）"),
            new("Logistics.Serials.ProdSerialInbound.Validation.QuantityHint", "Frontend", "请输入数量", "Please enter quantity", "数量を入力してください"),
            new("Logistics.Serials.ProdSerialInbound.Validation.InboundNoHint", "Frontend", "请输入入库单号（可选）", "Please enter inbound number (optional)", "入庫番号を入力してください（オプション）"),
            new("Logistics.Serials.ProdSerialInbound.Validation.InboundDateHint", "Frontend", "请选择入库日期（可选）", "Please select inbound date (optional)", "入庫日を選択してください（オプション）"),
            new("Logistics.Serials.ProdSerialInbound.Validation.WarehouseHint", "Frontend", "请输入仓库（可选）", "Please enter warehouse (optional)", "倉庫を入力してください（オプション）"),
            new("Logistics.Serials.ProdSerialInbound.Validation.LocationHint", "Frontend", "请输入库位（可选）", "Please enter location (optional)", "ロケーションを入力してください（オプション）"),
            new("Logistics.Serials.ProdSerialInbound.Validation.OperatorHint", "Frontend", "请输入入库员（可选）", "Please enter operator (optional)", "入庫担当を入力してください（オプション）"),

            // 访客验证相关
            new("Logistics.Visitors.Validation.CompanyNameRequired", "Frontend", "公司名称不能为空", "Company name cannot be empty", "会社名は空にできません"),
            new("Logistics.Visitors.Validation.CompanyNameMaxLength", "Frontend", "公司名称长度不能超过200个字符", "Company name cannot exceed 200 characters", "会社名は200文字を超えることはできません"),
            new("Logistics.Visitors.Validation.CompanyNameInvalid", "Frontend", "公司名称不能为空，长度不能超过200个字符", "Company name cannot be empty and cannot exceed 200 characters", "会社名は空にできず、200文字を超えることはできません"),
            new("Logistics.Visitors.Validation.StartTimeInvalid", "Frontend", "开始时间不能为空，且必须早于结束时间", "Start time cannot be empty and must be earlier than end time", "開始時間は空にできず、終了時間より早い必要があります"),
            new("Logistics.Visitors.Validation.EndTimeInvalid", "Frontend", "结束时间不能为空，且必须晚于开始时间", "End time cannot be empty and must be later than start time", "終了時間は空にできず、開始時間より遅い必要があります"),
            new("Logistics.Visitors.Validation.StartTimeMustBeforeEndTime", "Frontend", "开始时间必须早于结束时间", "Start time must be earlier than end time", "開始時間は終了時間より早い必要があります"),
            new("Logistics.Visitors.Validation.EndTimeMustAfterStartTime", "Frontend", "结束时间必须晚于开始时间", "End time must be later than start time", "終了時間は開始時間より遅い必要があります"),
            new("Logistics.Visitors.Validation.CompanyNameHint", "Frontend", "请输入公司名称", "Please enter company name", "会社名を入力してください"),
            new("Logistics.Visitors.Validation.StartTimeHint", "Frontend", "请选择开始时间，必须早于结束时间", "Please select start time, must be earlier than end time", "開始時間を選択してください。終了時間より早い必要があります"),
            new("Logistics.Visitors.Validation.EndTimeHint", "Frontend", "请选择结束时间，必须晚于开始时间", "Please select end time, must be later than start time", "終了時間を選択してください。開始時間より遅い必要があります"),

            // 访客详情验证相关
            new("Logistics.Visitors.VisitorDetail.Validation.DepartmentHint", "Frontend", "请输入部门（可选）", "Please enter department (optional)", "部署を入力してください（オプション）"),
            new("Logistics.Visitors.VisitorDetail.Validation.NameHint", "Frontend", "请输入姓名（可选）", "Please enter name (optional)", "氏名を入力してください（オプション）"),
            new("Logistics.Visitors.VisitorDetail.Validation.PositionHint", "Frontend", "请输入职务（可选）", "Please enter position (optional)", "職務を入力してください（オプション）"),

            // 代码生成表验证相关
            new("Generator.GenTable.Validation.TableNameRequired", "Frontend", "表名不能为空", "Table name cannot be empty", "テーブル名は空にできません"),
            new("Generator.GenTable.Validation.TableNameHint", "Frontend", "请输入库表名称，用于无数据表的手动配置时表名可以不存在于数据库中", "Please enter table name, for manual configuration without database table, table name may not exist in database", "テーブル名を入力してください。データベーステーブルなしの手動設定用の場合、テーブル名はデータベースに存在しない場合があります"),
            new("Generator.GenTable.Validation.TableDescriptionHint", "Frontend", "请输入库表描述（可选）", "Please enter table description (optional)", "テーブル説明を入力してください（オプション）"),
            new("Generator.GenTable.Validation.ClassNameHint", "Frontend", "请输入实体类名称（可选）", "Please enter class name (optional)", "クラス名を入力してください（オプション）"),
            new("Generator.GenTable.Validation.NamespaceHint", "Frontend", "请输入命名空间（可选）", "Please enter namespace (optional)", "名前空間を入力してください（オプション）"),
            new("Generator.GenTable.Validation.ModuleCodeHint", "Frontend", "请输入模块标识（可选）", "Please enter module code (optional)", "モジュールコードを入力してください（オプション）"),
            new("Generator.GenTable.Validation.ModuleNameHint", "Frontend", "请输入模块名称（可选）", "Please enter module name (optional)", "モジュール名を入力してください（オプション）"),
            new("Generator.GenTable.Validation.AuthorHint", "Frontend", "请输入作者（可选）", "Please enter author (optional)", "作成者を入力してください（オプション）"),
            new("Generator.GenTable.Validation.TemplateTypeHint", "Frontend", "请选择生成模板类型", "Please select template type", "テンプレートタイプを選択してください"),
            new("Generator.GenTable.Validation.GenNamespacePrefixHint", "Frontend", "请输入命名空间前缀（可选）", "Please enter namespace prefix (optional)", "名前空間プレフィックスを入力してください（オプション）"),
            new("Generator.GenTable.Validation.GenBusinessNameHint", "Frontend", "请输入生成业务名称（可选）", "Please enter business name (optional)", "ビジネス名を入力してください（オプション）"),
            new("Generator.GenTable.Validation.GenModuleNameHint", "Frontend", "请输入生成模块名称（可选）", "Please enter gen module name (optional)", "生成モジュール名を入力してください（オプション）"),
            new("Generator.GenTable.Validation.GenFunctionNameHint", "Frontend", "请输入生成功能名（可选）", "Please enter function name (optional)", "機能名を入力してください（オプション）"),
            new("Generator.GenTable.Validation.GenTypeHint", "Frontend", "请选择生成方式", "Please select gen type", "生成タイプを選択してください"),
            new("Generator.GenTable.Validation.GenPathHint", "Frontend", "请输入代码生成路径（可选）", "Please enter gen path (optional)", "生成パスを入力してください（オプション）"),
            new("Generator.GenTable.Validation.ParentMenuNameHint", "Frontend", "请输入上级菜单名称（可选）", "Please enter parent menu name (optional)", "親メニュー名を入力してください（オプション）"),
            new("Generator.GenTable.Validation.PermissionPrefixHint", "Frontend", "请输入权限前缀（可选）", "Please enter permission prefix (optional)", "権限プレフィックスを入力してください（オプション）"),
            new("Generator.GenTable.Validation.DefaultSortFieldHint", "Frontend", "请输入默认排序字段（可选）", "Please enter default sort field (optional)", "デフォルトソートフィールドを入力してください（オプション）"),
            new("Generator.GenTable.Validation.DefaultSortOrderHint", "Frontend", "请选择默认排序", "Please select default sort order", "デフォルトソート順を選択してください"),

            // 代码生成列验证相关
            new("Generator.GenColumn.Validation.ColumnNameHint", "Frontend", "请输入列名", "Please enter column name", "カラム名を入力してください"),
            new("Generator.GenColumn.Validation.ColumnDescriptionHint", "Frontend", "请输入列描述（可选）", "Please enter column description (optional)", "カラム説明を入力してください（オプション）"),
            new("Generator.GenColumn.Validation.PropertyNameHint", "Frontend", "请输入属性名称（可选）", "Please enter property name (optional)", "プロパティ名を入力してください（オプション）"),
            new("Generator.GenColumn.Validation.DataTypeHint", "Frontend", "请选择C#类型", "Please select C# type", "C#タイプを選択してください"),
            new("Generator.GenColumn.Validation.LengthHint", "Frontend", "请输入长度（可选）", "Please enter length (optional)", "長さを入力してください（オプション）"),
            new("Generator.GenColumn.Validation.DecimalPlacesHint", "Frontend", "请输入精度（可选）", "Please enter decimal places (optional)", "小数点以下桁数を入力してください（オプション）"),
            new("Generator.GenColumn.Validation.DefaultValueHint", "Frontend", "请输入默认值（可选）", "Please enter default value (optional)", "デフォルト値を入力してください（オプション）"),
            new("Generator.GenColumn.Validation.OrderNumHint", "Frontend", "请输入库列排序号", "Please enter order number", "順序番号を入力してください"),
            new("Generator.GenColumn.Validation.QueryTypeHint", "Frontend", "请选择查询方式", "Please select query type", "クエリタイプを選択してください"),
            new("Generator.GenColumn.Validation.FormControlTypeHint", "Frontend", "请选择表单类型", "Please select form control type", "フォームタイプを選択してください"),
            new("Generator.GenColumn.Validation.DictTypeHint", "Frontend", "请输入字典类型（可选）", "Please enter dict type (optional)", "辞書タイプを入力してください（オプション）"),
        };
    }

    private sealed record ValidationSeed(string Key, string Module, string ZhCn, string EnUs, string JaJp);
}

