//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : UserFormViewModel.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-10-30
// 版本号 : 0.0.1
// 描述    : 用户表单视图模型（新建/更新）
//===================================================================

using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Takt.Application.Dtos.Identity;
using Takt.Application.Services.Identity;
using Takt.Application.Services.Routine;
using Takt.Domain.Interfaces;
using Takt.Fluent.Controls;
using Takt.Common.Logging;
using Takt.Common.Models;

namespace Takt.Fluent.ViewModels.Identity;

/// <summary>
/// 用户表单视图模型（新建/更新）
/// 使用 WPF 原生验证系统 INotifyDataErrorInfo
/// </summary>
public partial class UserFormViewModel : ObservableObject, INotifyDataErrorInfo
{
    private readonly IUserService _userService;
    private readonly IDictionaryTypeService _dictionaryTypeService;
    private readonly ILocalizationManager _localizationManager;
    private static readonly OperLogManager? _operLog = App.Services?.GetService<OperLogManager>();

    [ObservableProperty] private string _title = "";
    [ObservableProperty] private bool _isCreate = true;
    [ObservableProperty] private string _username = "";
    [ObservableProperty] private string? _realName;
    [ObservableProperty] private string _nickname = "";
    [ObservableProperty] private string? _email;
    [ObservableProperty] private string? _phone;
    [ObservableProperty] private int _userType;
    [ObservableProperty] private int _userGender;
    [ObservableProperty] private int _userStatus;
    [ObservableProperty] private string? _avatar;
    [ObservableProperty] private string? _remarks;
    [ObservableProperty] private long _id;
    [ObservableProperty] private string _error = string.Empty;

    /// <summary>
    /// 用户类型选项列表
    /// </summary>
    public ObservableCollection<SelectOptionModel> UserTypeOptions { get; } = new();

    /// <summary>
    /// 性别选项列表
    /// </summary>
    public ObservableCollection<SelectOptionModel> UserGenderOptions { get; } = new();

    /// <summary>
    /// 状态选项列表
    /// </summary>
    public ObservableCollection<SelectOptionModel> UserStatusOptions { get; } = new();
    
    // Hint 提示属性
    public string UsernameHint => _localizationManager.GetString("Identity.User.Validation.UsernameInvalid") ?? "用户名必须以小写字母开头，只能包含小写字母和数字，长度4-10位";
    
    public string EmailHint => _localizationManager.GetString("Identity.User.Validation.EmailInvalid") ?? "邮箱格式不正确";
    
    public string RealNameHint => _localizationManager.GetString("Identity.User.Validation.RealNameHint") ?? "不允许数字、点号、空格开头，英文字母首字母大写，30字以内";
    
    public string NicknameHint => _localizationManager.GetString("Identity.User.Validation.NicknameInvalid") ?? "昵称不允许数字、点号、空格开头，如果首字符是英文字母则必须是大写，允许字母、数字、点和空格，支持中文、日文、韩文、越南文等，如：Cheng.Jianhong、Joseph Robinette Biden Jr. 或 张三";
    
    public string PhoneHint => _localizationManager.GetString("Identity.User.Validation.PhoneInvalid") ?? "手机号格式不正确，必须是11位数字，以1开头，第二位为3-9";
    
    public string PasswordHint => _localizationManager.GetString("Identity.User.Validation.PasswordMinLength") ?? "密码长度不能少于6位";
    
    public string PasswordConfirmHint => _localizationManager.GetString("Identity.User.Validation.PasswordConfirmHint") ?? "请再次输入密码以确认";
    
    public string RemarksHint => _localizationManager.GetString("Identity.User.Validation.RemarksHint") ?? "请输入备注信息（可选）";
    
    // 错误消息属性（保留用于向后兼容，同时更新 INotifyDataErrorInfo）
    private string _usernameError = string.Empty;
    public string UsernameError
    {
        get => _usernameError;
        private set
        {
            if (SetProperty(ref _usernameError, value))
            {
                SetError(nameof(Username), value);
            }
        }
    }

    private string _realNameError = string.Empty;
    public string RealNameError
    {
        get => _realNameError;
        private set
        {
            if (SetProperty(ref _realNameError, value))
            {
                SetError(nameof(RealName), value);
            }
        }
    }

    private string _nicknameError = string.Empty;
    public string NicknameError
    {
        get => _nicknameError;
        private set
        {
            if (SetProperty(ref _nicknameError, value))
            {
                SetError(nameof(Nickname), value);
            }
        }
    }

    private string _emailError = string.Empty;
    public string EmailError
    {
        get => _emailError;
        private set
        {
            if (SetProperty(ref _emailError, value))
            {
                SetError(nameof(Email), value);
            }
        }
    }

    private string _phoneError = string.Empty;
    public string PhoneError
    {
        get => _phoneError;
        private set
        {
            if (SetProperty(ref _phoneError, value))
            {
                SetError(nameof(Phone), value);
            }
        }
    }

    private string _avatarError = string.Empty;
    public string AvatarError
    {
        get => _avatarError;
        private set
        {
            if (SetProperty(ref _avatarError, value))
            {
                SetError(nameof(Avatar), value);
            }
        }
    }

    private string _passwordError = string.Empty;
    public string PasswordError
    {
        get => _passwordError;
        private set
        {
            if (SetProperty(ref _passwordError, value))
            {
                SetError(nameof(PasswordError), value); // 注意：密码字段没有对应的属性
            }
        }
    }

    private string _passwordConfirmError = string.Empty;
    public string PasswordConfirmError
    {
        get => _passwordConfirmError;
        private set
        {
            if (SetProperty(ref _passwordConfirmError, value))
            {
                SetError(nameof(PasswordConfirmError), value); // 注意：确认密码字段没有对应的属性
            }
        }
    }

    private string _userTypeError = string.Empty;
    public string UserTypeError
    {
        get => _userTypeError;
        private set
        {
            if (SetProperty(ref _userTypeError, value))
            {
                SetError(nameof(UserType), value);
            }
        }
    }

    private string _userGenderError = string.Empty;
    public string UserGenderError
    {
        get => _userGenderError;
        private set
        {
            if (SetProperty(ref _userGenderError, value))
            {
                SetError(nameof(UserGender), value);
            }
        }
    }

    private string _userStatusError = string.Empty;
    public string UserStatusError
    {
        get => _userStatusError;
        private set
        {
            if (SetProperty(ref _userStatusError, value))
            {
                SetError(nameof(UserStatus), value);
            }
        }
    }

    private string _remarksError = string.Empty;
    public string RemarksError
    {
        get => _remarksError;
        private set
        {
            if (SetProperty(ref _remarksError, value))
            {
                SetError(nameof(Remarks), value);
            }
        }
    }

    // WPF 原生验证系统：INotifyDataErrorInfo 实现
    private readonly Dictionary<string, List<string>> _errors = new();

    public bool HasErrors => _errors.Any();

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return _errors.Values.SelectMany(e => e);
        }

        return _errors.TryGetValue(propertyName, out var errors) ? errors : Enumerable.Empty<string>();
    }

    /// <summary>
    /// 设置字段错误（WPF 原生验证，替换现有错误）
    /// </summary>
    private void SetError(string propertyName, string? error)
    {
        if (string.IsNullOrEmpty(error))
        {
            if (_errors.Remove(propertyName))
            {
                OnErrorsChanged(propertyName);
            }
        }
        else
        {
            _errors[propertyName] = new List<string> { error };
            OnErrorsChanged(propertyName);
        }
    }

    /// <summary>
    /// 清除所有错误（WPF 原生验证）
    /// </summary>
    private void ClearAllValidationErrors()
    {
        var propertyNames = _errors.Keys.ToList();
        _errors.Clear();
        foreach (var propertyName in propertyNames)
        {
            OnErrorsChanged(propertyName);
        }
    }

    private void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        OnPropertyChanged(nameof(HasErrors));
    }

    private Func<(string pwd, string confirm)>? _passwordAccessor;
    public void AttachPasswordAccess(Func<(string pwd, string confirm)> accessor) => _passwordAccessor = accessor;
    
    // 文本字段值访问器（类似密码的方式，直接从控件读取值）
    private Func<(string username, string realName, string nickname, string email, string phone, string avatar, string remarks)>? _textFieldsAccessor;
    public void AttachTextFieldsAccess(Func<(string username, string realName, string nickname, string email, string phone, string avatar, string remarks)> accessor) => _textFieldsAccessor = accessor;

    /// <summary>
    /// 保存成功后的回调，用于关闭窗口
    /// </summary>
    public Action? SaveSuccessCallback { get; set; }

    public UserFormViewModel(IUserService userService, IDictionaryTypeService dictionaryTypeService, ILocalizationManager localizationManager)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _dictionaryTypeService = dictionaryTypeService ?? throw new ArgumentNullException(nameof(dictionaryTypeService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));

        // 初始化字典选项
        _ = InitializeDictionaryOptionsAsync();
    }

    /// <summary>
    /// 初始化字典选项列表
    /// </summary>
    private async Task InitializeDictionaryOptionsAsync()
    {
        await InitializeUserTypeOptionsAsync();
        await InitializeUserGenderOptionsAsync();
        await InitializeUserStatusOptionsAsync();
    }

    /// <summary>
    /// 初始化用户类型选项列表
    /// </summary>
    private async Task InitializeUserTypeOptionsAsync()
    {
        try
        {
            var result = await _dictionaryTypeService.GetOptionsAsync("sys_user_type");
            if (result.Success && result.Data != null)
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    UserTypeOptions.Clear();
                    foreach (var option in result.Data)
                    {
                        // 将字典值转换为枚举值存储在 ExtValue 中
                        var enumValue = ConvertUserTypeToEnum(option.DataValue);
                        UserTypeOptions.Add(new SelectOptionModel
                        {
                            DataValue = enumValue.ToString(),
                            DataLabel = option.DataLabel,
                            ExtValue = option.DataValue, // 保存原始字典值
                            OrderNum = option.OrderNum
                        });
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[UserForm] 加载用户类型选项失败");
        }
    }

    /// <summary>
    /// 初始化性别选项列表
    /// </summary>
    private async Task InitializeUserGenderOptionsAsync()
    {
        try
        {
            var result = await _dictionaryTypeService.GetOptionsAsync("sys_common_gender");
            if (result.Success && result.Data != null)
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    UserGenderOptions.Clear();
                    foreach (var option in result.Data)
                    {
                        // 将字典值转换为枚举值存储在 ExtValue 中
                        var enumValue = ConvertUserGenderToEnum(option.DataValue);
                        UserGenderOptions.Add(new SelectOptionModel
                        {
                            DataValue = enumValue.ToString(),
                            DataLabel = option.DataLabel,
                            ExtValue = option.DataValue, // 保存原始字典值
                            OrderNum = option.OrderNum
                        });
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[UserForm] 加载性别选项失败");
        }
    }

    /// <summary>
    /// 初始化状态选项列表
    /// </summary>
    private async Task InitializeUserStatusOptionsAsync()
    {
        try
        {
            var result = await _dictionaryTypeService.GetOptionsAsync("sys_common_status");
            if (result.Success && result.Data != null)
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    UserStatusOptions.Clear();
                    foreach (var option in result.Data)
                    {
                        // 将字典值转换为枚举值存储在 ExtValue 中
                        var enumValue = ConvertUserStatusToEnum(option.DataValue);
                        UserStatusOptions.Add(new SelectOptionModel
                        {
                            DataValue = enumValue.ToString(),
                            DataLabel = option.DataLabel,
                            ExtValue = option.DataValue, // 保存原始字典值
                            OrderNum = option.OrderNum
                        });
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[UserForm] 加载状态选项失败");
        }
    }

    /// <summary>
    /// 将字典值转换为用户类型枚举值
    /// </summary>
    private int ConvertUserTypeToEnum(string dictValue)
    {
        return dictValue switch
        {
            "Takt365" => 0, // 系统用户
            "normal" => 1,  // 普通用户
            _ => 1 // 默认普通用户
        };
    }

    /// <summary>
    /// 将字典值转换为性别枚举值
    /// </summary>
    private int ConvertUserGenderToEnum(string dictValue)
    {
        return dictValue switch
        {
            "unknown" => 0, // 未知
            "male" => 1,    // 男
            "female" => 2,  // 女
            _ => 0 // 默认未知
        };
    }

    /// <summary>
    /// 将字典值转换为状态枚举值
    /// </summary>
    private int ConvertUserStatusToEnum(string dictValue)
    {
        return dictValue switch
        {
            "normal" => 0,   // 正常
            "disabled" => 1, // 禁用
            _ => 0 // 默认正常
        };
    }

    /// <summary>
    /// 设置用户类型命令
    /// </summary>
    [RelayCommand]
    private void SetUserType(string? value)
    {
        if (int.TryParse(value, out var intValue))
        {
            UserType = intValue;
        }
    }

    /// <summary>
    /// 设置性别命令
    /// </summary>
    [RelayCommand]
    private void SetUserGender(string? value)
    {
        if (int.TryParse(value, out var intValue))
        {
            UserGender = intValue;
        }
    }

    /// <summary>
    /// 设置状态命令
    /// </summary>
    [RelayCommand]
    private void SetUserStatus(string? value)
    {
        if (int.TryParse(value, out var intValue))
        {
            UserStatus = intValue;
        }
    }

    public void ForCreate()
    {
        // 清除所有错误消息
        ClearAllErrors();
        
        IsCreate = true;
        Title = _localizationManager.GetString("Identity.User.Create");
        Username = string.Empty;
        RealName = null;
        Nickname = string.Empty;
        Email = null;
        Phone = null;
        UserType = 1; // 默认普通用户（0=系统用户，1=普通用户）
        UserGender = 0;
        UserStatus = 0; // 启用
        Avatar = "assets/avatar.png"; // 默认头像路径
        Remarks = null;
    }

    public void ForUpdate(UserDto dto)
    {
        // 清除所有错误消息
        ClearAllErrors();
        
        IsCreate = false;
        Title = _localizationManager.GetString("Identity.User.Update");
        Id = dto.Id;
        Username = dto.Username ?? string.Empty;
        RealName = dto.RealName;
        Nickname = dto.Nickname ?? string.Empty;
        Email = dto.Email;
        Phone = dto.Phone;
        UserType = (int)dto.UserType;
        UserGender = (int)dto.UserGender;
        UserStatus = (int)dto.UserStatus;
        Avatar = string.IsNullOrWhiteSpace(dto.Avatar) ? "assets/avatar.png" : dto.Avatar; // 如果为空则使用默认头像
        Remarks = dto.Remarks;
    }

    /// <summary>
    /// 清除所有错误消息
    /// </summary>
    private void ClearAllErrors()
    {
        UsernameError = string.Empty;
        RealNameError = string.Empty;
        NicknameError = string.Empty;
        EmailError = string.Empty;
        PhoneError = string.Empty;
        AvatarError = string.Empty;
        PasswordError = string.Empty;
        PasswordConfirmError = string.Empty;
        UserTypeError = string.Empty;
        UserGenderError = string.Empty;
        UserStatusError = string.Empty;
        RemarksError = string.Empty;
        Error = string.Empty;
        // 同时清除 WPF 原生验证错误
        ClearAllValidationErrors();
    }

    // 属性变更时进行实时验证
    partial void OnUsernameChanged(string value)
    {
        _operLog?.Debug("[UserFormViewModel] OnUsernameChanged: Username='{Username}'", value);
        ValidateUsername();
    }

    partial void OnRealNameChanged(string? value)
    {
        ValidateRealName();
    }

    partial void OnNicknameChanged(string value)
    {
        ValidateNickname();
    }

    partial void OnEmailChanged(string? value)
    {
        ValidateEmail();
    }

    partial void OnPhoneChanged(string? value)
    {
        ValidatePhone();
    }

    partial void OnAvatarChanged(string? value)
    {
        ValidateAvatar();
    }

    /// <summary>
    /// 验证用户名（实时验证）
    /// </summary>
    public void ValidateUsername(string? username = null)
    {
        var usernameToValidate = username ?? Username;
        _operLog?.Debug("[UserFormViewModel] ValidateUsername: 开始验证，Username='{Username}'", usernameToValidate);
        
        // 失去焦点时立即验证，不等待提交
        // 统一逻辑：空值时清除错误，等待提交时验证（与其他字段保持一致）
        if (string.IsNullOrWhiteSpace(usernameToValidate))
        {
            UsernameError = string.Empty; // 空值时不清除错误，等待提交时验证
            return;
        }

        if (usernameToValidate.Length < 4)
        {
            UsernameError = _localizationManager.GetString("Identity.User.Validation.UsernameMinLength") ?? "用户名长度不能少于4位";
            _operLog?.Debug("[UserFormViewModel] ValidateUsername: 验证失败-长度不足，UsernameError='{Error}'", UsernameError);
        }
        else if (usernameToValidate.Length > 10)
        {
            UsernameError = _localizationManager.GetString("Identity.User.Validation.UsernameMaxLength") ?? "用户名长度不能超过10位";
            _operLog?.Debug("[UserFormViewModel] ValidateUsername: 验证失败-长度超限，UsernameError='{Error}'", UsernameError);
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(usernameToValidate, @"^[a-z][a-z0-9]{3,9}$"))
        {
            UsernameError = _localizationManager.GetString("Identity.User.Validation.UsernameInvalid");
            _operLog?.Debug("[UserFormViewModel] ValidateUsername: 验证失败-格式不正确，UsernameError='{Error}'", UsernameError);
        }
        else
        {
            UsernameError = string.Empty; // 验证通过，清除错误
            _operLog?.Debug("[UserFormViewModel] ValidateUsername: 验证通过");
        }
    }

    /// <summary>
    /// 验证真实姓名（实时验证）
    /// </summary>
    public void ValidateRealName(string? realName = null)
    {
        var realNameToValidate = realName ?? RealName;
        if (string.IsNullOrWhiteSpace(realNameToValidate))
        {
            RealNameError = string.Empty; // 空值时不清除错误，等待提交时验证
            return;
        }

        // 不允许数字、点号、空格开头
        // 如果首字符是英文字母，必须是大写
        // 如果首字符是其他语言的字母（中文、日文等），直接允许
        // 后续字符可以是：任何语言的字母、数字、点号、空格
        if (realNameToValidate.Length == 0)
        {
            RealNameError = string.Empty;
            return;
        }

        var firstChar = realNameToValidate[0];
        bool isValidFirstChar = false;
        
        // 检查首字符：不允许数字、点号、空格开头
        if (char.IsDigit(firstChar) || firstChar == '.' || char.IsWhiteSpace(firstChar))
        {
            // 首字符不能是数字、点号、空格
            isValidFirstChar = false;
        }
        else if (char.IsLetter(firstChar))
        {
            // 首字符是字母
            if (firstChar >= 'A' && firstChar <= 'Z')
            {
                // 英文字母，必须是大写
                isValidFirstChar = true;
            }
            else if (firstChar >= 'a' && firstChar <= 'z')
            {
                // 英文字母，但是小写，不符合要求
                isValidFirstChar = false;
            }
            else
            {
                // 其他语言的字母（中文、日文、韩文等），直接允许
                isValidFirstChar = true;
            }
        }
        else
        {
            // 其他字符（如标点符号等），允许
            isValidFirstChar = true;
        }

        if (!isValidFirstChar)
        {
            RealNameError = _localizationManager.GetString("Identity.User.Validation.RealNameInvalid");
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(realNameToValidate, @"^[\p{L}\p{N}. ]+$", System.Text.RegularExpressions.RegexOptions.None))
        {
            RealNameError = _localizationManager.GetString("Identity.User.Validation.RealNameInvalid") ?? "只能包含字母、数字、点和空格";
        }
        else
        {
            RealNameError = string.Empty; // 验证通过，清除错误
        }
    }

    /// <summary>
    /// 验证昵称（实时验证）
    /// </summary>
    public void ValidateNickname(string? nickname = null)
    {
        var nicknameToValidate = nickname ?? Nickname;
        if (string.IsNullOrWhiteSpace(nicknameToValidate))
        {
            NicknameError = string.Empty; // 空值时不清除错误，等待提交时验证
            return;
        }

        if (nicknameToValidate.Length > 40)
        {
            NicknameError = _localizationManager.GetString("Identity.User.Validation.NicknameMaxLength") ?? "昵称长度不能超过40个字符";
            return;
        }

        // 不允许数字、点号、空格开头
        // 如果首字符是英文字母，必须是大写
        // 如果首字符是其他语言的字母（中文、日文等），直接允许
        // 后续字符可以是：任何语言的字母、数字、点号、空格
        if (nicknameToValidate.Length == 0)
        {
            NicknameError = string.Empty;
            return;
        }

        var firstChar = nicknameToValidate[0];
        bool isValidFirstChar = false;
        
        // 检查首字符：不允许数字、点号、空格开头
        if (char.IsDigit(firstChar) || firstChar == '.' || char.IsWhiteSpace(firstChar))
        {
            // 首字符不能是数字、点号、空格
            isValidFirstChar = false;
        }
        else if (char.IsLetter(firstChar))
        {
            // 首字符是字母
            if (firstChar >= 'A' && firstChar <= 'Z')
            {
                // 英文字母，必须是大写
                isValidFirstChar = true;
            }
            else if (firstChar >= 'a' && firstChar <= 'z')
            {
                // 英文字母，但是小写，不符合要求
                isValidFirstChar = false;
            }
            else
            {
                // 其他语言的字母（中文、日文、韩文等），直接允许
                isValidFirstChar = true;
            }
        }
        else
        {
            // 其他字符（如标点符号等），允许
            isValidFirstChar = true;
        }

        if (!isValidFirstChar)
        {
            NicknameError = _localizationManager.GetString("Identity.User.Validation.NicknameInvalid") ?? "昵称不允许数字、点号、空格开头，如果首字符是英文字母则必须是大写，允许字母、数字、点和空格，支持中文、日文、韩文、越南文等";
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(nicknameToValidate, @"^[\p{L}\p{N}. ]+$", System.Text.RegularExpressions.RegexOptions.None))
        {
            NicknameError = _localizationManager.GetString("Identity.User.Validation.NicknameInvalid") ?? "昵称只能包含字母、数字、点和空格";
        }
        else
        {
            NicknameError = string.Empty; // 验证通过，清除错误
        }
    }

    /// <summary>
    /// 验证邮箱（实时验证）
    /// </summary>
    public void ValidateEmail(string? email = null)
    {
        var emailToValidate = email ?? Email;
        if (string.IsNullOrWhiteSpace(emailToValidate))
        {
            EmailError = string.Empty; // 空值时不清除错误，等待提交时验证
            return;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(emailToValidate, @"^[a-zA-Z0-9]([a-zA-Z0-9._-]*[a-zA-Z0-9])?@[a-zA-Z0-9]([a-zA-Z0-9.-]*[a-zA-Z0-9])?\.[a-zA-Z]{2,}$"))
        {
            EmailError = _localizationManager.GetString("Identity.User.Validation.EmailInvalid") ?? "邮箱格式不正确";
        }
        else
        {
            EmailError = string.Empty; // 验证通过，清除错误
        }
    }

    /// <summary>
    /// 验证手机号（实时验证）
    /// </summary>
    public void ValidatePhone(string? phone = null)
    {
        var phoneToValidate = phone ?? Phone;
        if (string.IsNullOrWhiteSpace(phoneToValidate))
        {
            PhoneError = string.Empty; // 空值时不清除错误，等待提交时验证
            return;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(phoneToValidate, @"^1[3-9]\d{9}$"))
        {
            PhoneError = _localizationManager.GetString("Identity.User.Validation.PhoneInvalid") ?? "手机号格式不正确，必须是11位数字，以1开头，第二位为3-9";
        }
        else
        {
            PhoneError = string.Empty; // 验证通过，清除错误
        }
    }

    /// <summary>
    /// 验证头像（实时验证）
    /// </summary>
    public void ValidateAvatar(string? avatar = null)
    {
        var avatarToValidate = avatar ?? Avatar;
        if (string.IsNullOrWhiteSpace(avatarToValidate))
        {
            AvatarError = string.Empty; // 空值时不清除错误，等待提交时验证
            return;
        }

        var avatarTrimmed = avatarToValidate.Trim();
        
        // 检查是否为绝对路径（Windows盘符或Unix根路径）
        if (System.IO.Path.IsPathRooted(avatarTrimmed) || 
            avatarTrimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
            avatarTrimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            avatarTrimmed.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
        {
            AvatarError = _localizationManager.GetString("Identity.User.Validation.AvatarMustBeRelativePath") ?? "头像必须是相对路径，不能使用绝对路径或URL";
        }
        // 检查路径长度（数据库字段最大256字符）
        else if (avatarTrimmed.Length > 256)
        {
            AvatarError = _localizationManager.GetString("Identity.User.Validation.AvatarPathTooLong") ?? "头像路径长度不能超过256个字符";
        }
        else
        {
            AvatarError = string.Empty; // 验证通过，清除错误
        }
    }

    /// <summary>
    /// 验证密码（实时验证）
    /// </summary>
    public void ValidatePassword(string password, string passwordConfirm)
    {
        if (!IsCreate) return; // 更新模式不验证密码

        if (string.IsNullOrWhiteSpace(password))
        {
            PasswordError = string.Empty; // 空值时不清除错误，等待提交时验证
        }
        else if (password.Length < 6)
        {
            PasswordError = _localizationManager.GetString("Identity.User.Validation.PasswordMinLength") ?? "密码长度不能少于6位";
        }
        else
        {
            PasswordError = string.Empty; // 验证通过，清除错误
        }

        // 同时验证确认密码
        ValidatePasswordConfirm(password, passwordConfirm);
    }

    /// <summary>
    /// 验证确认密码（实时验证）
    /// </summary>
    public void ValidatePasswordConfirm(string password, string passwordConfirm)
    {
        if (!IsCreate) return; // 更新模式不验证密码

        if (string.IsNullOrWhiteSpace(passwordConfirm))
        {
            PasswordConfirmError = string.Empty; // 空值时不清除错误，等待提交时验证
        }
        else if (!string.IsNullOrWhiteSpace(password) && password != passwordConfirm)
        {
            PasswordConfirmError = _localizationManager.GetString("Identity.User.Validation.PasswordMismatch") ?? "两次输入的密码不一致";
        }
        else
        {
            PasswordConfirmError = string.Empty; // 验证通过，清除错误
        }
    }

    /// <summary>
    /// 验证所有必填字段
    /// </summary>
    private bool ValidateFields(string? password = null, string? passwordConfirm = null)
    {
        // 清除所有错误，重新验证
        ClearAllErrors();
        bool isValid = true;

        // 验证用户名（必填）
        if (string.IsNullOrWhiteSpace(Username))
        {
            UsernameError = _localizationManager.GetString("Identity.User.Validation.UsernameRequired") ?? "用户名不能为空";
            isValid = false;
        }
        else if (Username.Length < 4)
        {
            UsernameError = _localizationManager.GetString("Identity.User.Validation.UsernameMinLength") ?? "用户名长度不能少于4位";
            isValid = false;
        }
        else if (Username.Length > 10)
        {
            UsernameError = _localizationManager.GetString("Identity.User.Validation.UsernameMaxLength") ?? "用户名长度不能超过10位";
            isValid = false;
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(Username, @"^[a-z][a-z0-9]{3,9}$"))
        {
            UsernameError = _localizationManager.GetString("Identity.User.Validation.UsernameInvalid");
            isValid = false;
        }

        // 验证真实姓名（必填）
        if (string.IsNullOrWhiteSpace(RealName))
        {
            RealNameError = _localizationManager.GetString("Identity.User.Validation.RealNameRequired") ?? "真实姓名不能为空";
            isValid = false;
        }
        else
        {
            // 不允许数字、点号、空格开头
            // 如果首字符是英文字母，必须是大写
            var firstChar = RealName[0];
            bool isValidFirstChar = false;
            
            if (char.IsDigit(firstChar) || firstChar == '.' || char.IsWhiteSpace(firstChar))
            {
                // 首字符不能是数字、点号、空格
                isValidFirstChar = false;
            }
            else if (char.IsLetter(firstChar))
            {
                // 首字符是字母
                if (firstChar >= 'A' && firstChar <= 'Z')
                {
                    isValidFirstChar = true;
                }
                else if (firstChar >= 'a' && firstChar <= 'z')
                {
                    isValidFirstChar = false;
                }
                else
                {
                    // 其他语言的字母（中文、日文、韩文等），直接允许
                    isValidFirstChar = true;
                }
            }
            else
            {
                // 其他字符（如标点符号等），允许
                isValidFirstChar = true;
            }

            if (!isValidFirstChar)
            {
                RealNameError = _localizationManager.GetString("Identity.User.Validation.RealNameInvalid");
                isValid = false;
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(RealName, @"^[\p{L}\p{N}. ]+$", System.Text.RegularExpressions.RegexOptions.None))
            {
                RealNameError = _localizationManager.GetString("Identity.User.Validation.RealNameInvalid") ?? "只能包含字母、数字、点和空格";
                isValid = false;
            }
        }

        // 验证昵称（必填）
        if (string.IsNullOrWhiteSpace(Nickname))
        {
            NicknameError = _localizationManager.GetString("Identity.User.Validation.NicknameRequired") ?? "昵称不能为空";
            isValid = false;
        }
        else if (Nickname.Length > 40)
        {
            NicknameError = _localizationManager.GetString("Identity.User.Validation.NicknameMaxLength") ?? "昵称长度不能超过40个字符";
            isValid = false;
        }
        else
        {
            // 不允许数字、点号、空格开头
            // 如果首字符是英文字母，必须是大写
            var firstChar = Nickname[0];
            bool isValidFirstChar = false;
            
            if (char.IsDigit(firstChar) || firstChar == '.' || char.IsWhiteSpace(firstChar))
            {
                // 首字符不能是数字、点号、空格
                isValidFirstChar = false;
            }
            else if (char.IsLetter(firstChar))
            {
                // 首字符是字母
                if (firstChar >= 'A' && firstChar <= 'Z')
                {
                    isValidFirstChar = true;
                }
                else if (firstChar >= 'a' && firstChar <= 'z')
                {
                    isValidFirstChar = false;
                }
                else
                {
                    // 其他语言的字母（中文、日文、韩文等），直接允许
                    isValidFirstChar = true;
                }
            }
            else
            {
                // 其他字符（如标点符号等），允许
                isValidFirstChar = true;
            }

            if (!isValidFirstChar)
            {
                NicknameError = _localizationManager.GetString("Identity.User.Validation.NicknameInvalid") ?? "昵称不允许数字、点号、空格开头，如果首字符是英文字母则必须是大写，允许字母、数字、点和空格，支持中文、日文、韩文、越南文等";
                isValid = false;
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(Nickname, @"^[\p{L}\p{N}. ]+$", System.Text.RegularExpressions.RegexOptions.None))
            {
                NicknameError = _localizationManager.GetString("Identity.User.Validation.NicknameInvalid") ?? "昵称只能包含字母、数字、点和空格";
                isValid = false;
            }
        }

        // 验证邮箱（必填）
        if (string.IsNullOrWhiteSpace(Email))
        {
            EmailError = _localizationManager.GetString("Identity.User.Validation.EmailRequired") ?? "邮箱不能为空";
            isValid = false;
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(Email, @"^[a-zA-Z0-9]([a-zA-Z0-9._-]*[a-zA-Z0-9])?@[a-zA-Z0-9]([a-zA-Z0-9.-]*[a-zA-Z0-9])?\.[a-zA-Z]{2,}$"))
        {
            EmailError = _localizationManager.GetString("Identity.User.Validation.EmailInvalid") ?? "邮箱格式不正确";
            isValid = false;
        }

        // 验证手机号（必填）
        if (string.IsNullOrWhiteSpace(Phone))
        {
            PhoneError = _localizationManager.GetString("Identity.User.Validation.PhoneRequired") ?? "手机号不能为空";
            isValid = false;
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(Phone, @"^1[3-9]\d{9}$"))
        {
            PhoneError = _localizationManager.GetString("Identity.User.Validation.PhoneInvalid") ?? "手机号格式不正确，必须是11位数字，以1开头，第二位为3-9";
            isValid = false;
        }

        // 验证头像（必填，且必须是相对路径）
        if (string.IsNullOrWhiteSpace(Avatar))
        {
            AvatarError = _localizationManager.GetString("Identity.User.Validation.AvatarRequired") ?? "头像不能为空";
            isValid = false;
        }
        else
        {
            // 验证是否为相对路径（不能是绝对路径或URL）
            var avatar = Avatar.Trim();
            
            // 检查是否为绝对路径（Windows盘符或Unix根路径）
            if (System.IO.Path.IsPathRooted(avatar) || 
                avatar.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                avatar.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                avatar.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                AvatarError = _localizationManager.GetString("Identity.User.Validation.AvatarMustBeRelativePath") ?? "头像必须是相对路径，不能使用绝对路径或URL";
                isValid = false;
            }
            // 检查路径长度（数据库字段最大256字符）
            else if (avatar.Length > 256)
            {
                AvatarError = _localizationManager.GetString("Identity.User.Validation.AvatarPathTooLong") ?? "头像路径长度不能超过256个字符";
                isValid = false;
            }
        }

        // 验证密码（创建时必填）
        if (IsCreate)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                PasswordError = _localizationManager.GetString("Identity.User.Validation.PasswordRequired") ?? "密码不能为空";
                isValid = false;
            }
            else if (password.Length < 6)
            {
                PasswordError = _localizationManager.GetString("Identity.User.Validation.PasswordMinLength") ?? "密码长度不能少于6位";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(passwordConfirm))
            {
                PasswordConfirmError = _localizationManager.GetString("Identity.User.Validation.PasswordConfirmRequired") ?? "确认密码不能为空";
                isValid = false;
            }
            else if (!string.IsNullOrWhiteSpace(password) && password != passwordConfirm)
            {
                PasswordConfirmError = _localizationManager.GetString("Identity.User.Validation.PasswordMismatch") ?? "两次输入的密码不一致";
                isValid = false;
            }
        }

        return isValid;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            // 先从控件读取所有值（类似密码的方式）
            string username = string.Empty;
            string realName = string.Empty;
            string nickname = string.Empty;
            string email = string.Empty;
            string phone = string.Empty;
            string avatar = string.Empty;
            string? password = null;
            string? passwordConfirm = null;
            
            if (_textFieldsAccessor != null)
            {
                var (u, rn, n, e, p, a, r) = _textFieldsAccessor.Invoke();
                username = u ?? string.Empty;
                realName = rn ?? string.Empty;
                nickname = n ?? string.Empty;
                email = e ?? string.Empty;
                phone = p ?? string.Empty;
                avatar = a ?? string.Empty;
                var remarksValue = r ?? string.Empty;
                
                // 更新 ViewModel 属性
                Username = username;
                RealName = realName;
                Nickname = nickname;
                Email = email;
                Phone = phone;
                Avatar = avatar;
                Remarks = remarksValue;
            }
            
            if (IsCreate)
            {
                var (pwd, confirm) = _passwordAccessor?.Invoke() ?? (string.Empty, string.Empty);
                password = pwd;
                passwordConfirm = confirm;
            }

            // 验证所有字段（清除所有错误并重新验证）
            if (!ValidateFields(password, passwordConfirm))
            {
                return;
            }

            if (IsCreate)
            {
                var dto = new UserCreateDto
                {
                    Username = Username,
                    Password = password!,
                    RealName = RealName,
                    Nickname = Nickname,
                    Email = Email,
                    Phone = Phone,
                    Avatar = Avatar,
                    UserType = (Takt.Common.Enums.UserTypeEnum)UserType,
                    UserGender = (Takt.Common.Enums.UserGenderEnum)UserGender,
                    UserStatus = (Takt.Common.Enums.StatusEnum)UserStatus,
                    Remarks = Remarks
                };
                var result = await _userService.CreateAsync(dto);
                if (!result.Success)
                {
                    var entityName = _localizationManager.GetString("Identity.User.Keyword") ?? "用户";
                    var errorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.create") ?? "{0}创建失败", entityName);
                    Error = errorMessage;
                    TaktMessageManager.ShowError(errorMessage);
                    return;
                }
                
                // 创建成功，显示成功消息
                var entityNameSuccess = _localizationManager.GetString("Identity.User.Keyword") ?? "用户";
                var successMessage = string.Format(_localizationManager.GetString("common.success.create") ?? "{0}创建成功", entityNameSuccess);
                TaktMessageManager.ShowSuccess(successMessage);
            }
            else
            {
                var dto = new UserUpdateDto
                {
                    Id = Id,
                    Username = Username,
                    RealName = RealName,
                    Nickname = Nickname,
                    Email = Email,
                    Phone = Phone,
                    Avatar = Avatar,
                    UserType = (Takt.Common.Enums.UserTypeEnum)UserType,
                    UserGender = (Takt.Common.Enums.UserGenderEnum)UserGender,
                    UserStatus = (Takt.Common.Enums.StatusEnum)UserStatus,
                    Remarks = Remarks,
                    Password = "" // 更新不改密码
                };
                var result = await _userService.UpdateAsync(dto);
                if (!result.Success)
                {
                    var entityName = _localizationManager.GetString("Identity.User.Keyword") ?? "用户";
                    var errorMessage = result.Message ?? string.Format(_localizationManager.GetString("common.failed.update") ?? "{0}更新失败", entityName);
                    Error = errorMessage;
                    TaktMessageManager.ShowError(errorMessage);
                    return;
                }
                
                // 更新成功，显示成功消息
                var entityNameSuccess = _localizationManager.GetString("Identity.User.Keyword") ?? "用户";
                var successMessage = string.Format(_localizationManager.GetString("common.success.update") ?? "{0}更新成功", entityNameSuccess);
                TaktMessageManager.ShowSuccess(successMessage);
            }

            // 保存成功，触发回调关闭窗口
            SaveSuccessCallback?.Invoke();
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;
            Error = errorMessage;
            TaktMessageManager.ShowError(errorMessage);
        }
    }
}


