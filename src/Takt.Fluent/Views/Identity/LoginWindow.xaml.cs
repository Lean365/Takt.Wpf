//===================================================================
// 项目名 : Takt.Wpf
// 文件名 : LoginWindow.xaml.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 版本号 : 0.0.1
// 描述    : 登录窗口代码后台（参照WPFGallery实现）
//===================================================================

#pragma warning disable WPF0001 // ThemeMode 是实验性 API，但在 .NET 9 中可用
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using FontAwesome.Sharp;
using Takt.Fluent.Services;
using Takt.Common.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Takt.Domain.Interfaces;
using Takt.Application.Services.Routine;
using Takt.Application.Dtos.Routine;
using Takt.Fluent.Controls;

namespace Takt.Fluent.Views.Identity;

/// <summary>
/// 登录窗口
/// </summary>
public partial class LoginWindow : Window
{
    private readonly ThemeService _themeService;
    private readonly ILocalizationManager _localizationManager;
    private readonly ILanguageService _languageService;
    private List<LanguageOptionDto> _availableLanguages = new();

    public LoginWindow()
    {
        InitializeComponent();
        
        // 检查 Services 是否可用
        if (App.Services == null)
        {
            throw new InvalidOperationException("App.Services 为 null，无法获取服务。请确保 Host 已正确启动。");
        }
        
        try
        {
            // 获取主题服务和本地化管理器
            _themeService = App.Services.GetRequiredService<ThemeService>();
            _localizationManager = App.Services.GetRequiredService<ILocalizationManager>();
            _languageService = App.Services.GetRequiredService<ILanguageService>();
            
            // 订阅主题变化事件
            _themeService.ThemeChanged += OnThemeChanged;
            
            // 订阅语言切换事件
            _localizationManager.LanguageChanged += OnLanguageChanged;
            
            // 初始化主题选择
            InitializeThemeComboBox();
            
            // 通过依赖注入获取ViewModel
            var viewModel = App.Services.GetRequiredService<ViewModels.Identity.LoginViewModel>();
            DataContext = viewModel;
            
            // 初始化语言选择（使用默认值，不等待数据库）
            UpdateLanguageIcon();
            
            // 初始化语言菜单（不等待数据，数据加载完成后会自动更新）
            InitializeLanguageMenu();
            
            // 窗口加载完成后的初始化
            Loaded += OnWindowLoaded;

        Closed += LoginWindow_Closed;
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[登录] LoginWindow 初始化失败");
            throw;
        }
    }

    /// <summary>
    /// 窗口加载完成事件处理
    /// </summary>
    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as ViewModels.Identity.LoginViewModel;
        
        // 订阅 ShowProgressBar 属性变化事件
        if (viewModel != null)
        {
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
        
        // 调整输入框 Padding
                AdjustInputPadding();
                
        // ViewModel 提示文本已通过 XAML 的 local:Loc 绑定，无需在代码中设置
                if (viewModel != null)
                {
                    
            // 同步密码框的值并订阅事件，同时清除错误的 AutomationProperties.Name 绑定
            Dispatcher.BeginInvoke(() =>
                    {
                // 清除 TextBox 的错误绑定
                        var usernameTextBox = FindName("UsernameTextBox") as TextBox;
                        if (usernameTextBox != null)
                        {
                            try
                            {
                        var binding = BindingOperations.GetBinding(usernameTextBox, AutomationProperties.NameProperty);
                        if (binding != null && binding.Path.Path == "(0)")
                        {
                            BindingOperations.ClearBinding(usernameTextBox, AutomationProperties.NameProperty);
                        }
                            }
                    catch { /* 忽略清除绑定时的错误 */ }
                        }
                        
                // 清除 PasswordBox 的错误绑定
                        if (FindName("PasswordBox") is PasswordBox passwordBox)
                        {
                            try
                            {
                        var binding = BindingOperations.GetBinding(passwordBox, AutomationProperties.NameProperty);
                        if (binding != null && binding.Path.Path == "(0)")
                        {
                            BindingOperations.ClearBinding(passwordBox, AutomationProperties.NameProperty);
                            }
                            }
                    catch { /* 忽略清除绑定时的错误 */ }
                            
                    if (!string.IsNullOrWhiteSpace(viewModel.Password))
                            {
                                passwordBox.Password = viewModel.Password;
                            }
                            passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
                        }
                
                // 设置焦点到用户名输入框
                usernameTextBox?.Focus();
            }, System.Windows.Threading.DispatcherPriority.Loaded);
                }
            
        // 初始化主题
        InitializeTheme();
        
        // 初始化语言
        InitializeLanguage();
        
        // 更新翻译文本
        UpdateTranslations();
        
        // 后台任务：检查数据库连接和加载语言列表
        _ = InitializeBackgroundTasksAsync();
    }

    /// <summary>
    /// ViewModel 属性变化事件处理
    /// </summary>
    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModels.Identity.LoginViewModel.ShowProgressBar))
        {
            if (DataContext is not ViewModels.Identity.LoginViewModel viewModel) return;

            // PropertyChanged 事件已经在 UI 线程，直接执行即可
            // 不需要额外的 Dispatcher 调用
            var loginFormBorder = FindName("LoginFormBorder") as FrameworkElement;
            var progressBarLayer = FindName("ProgressBarLayer") as FrameworkElement;
            var topButtonsPanel = FindName("TopButtonsPanel") as FrameworkElement;

            if (viewModel.ShowProgressBar)
                {
                    // 先确保进度条层可见但透明
                    if (progressBarLayer != null)
                    {
                        progressBarLayer.Opacity = 0.0;
                        progressBarLayer.Visibility = Visibility.Visible;
                    }

                    // 使用 Storyboard 实现平滑的过渡动画
                    var storyboard = new Storyboard();
                    
                    // 创建缓动函数，使动画更平滑
                    var easeOut = new CubicEase { EasingMode = EasingMode.EaseOut };
                    var easeIn = new CubicEase { EasingMode = EasingMode.EaseIn };

                    if (loginFormBorder != null)
                    {
                        // 表单淡出动画（200ms，使用缓动）
                        var formFadeOut = new DoubleAnimation
                        {
                            From = 1.0,
                            To = 0.0,
                            Duration = TimeSpan.FromMilliseconds(200),
                            EasingFunction = easeOut
                        };
                        Storyboard.SetTarget(formFadeOut, loginFormBorder);
                        Storyboard.SetTargetProperty(formFadeOut, new PropertyPath(UIElement.OpacityProperty));
                        storyboard.Children.Add(formFadeOut);

                        // 表单上移动画（200ms）
                        if (loginFormBorder.RenderTransform is not TranslateTransform transform)
                        {
                            transform = new TranslateTransform();
                            loginFormBorder.RenderTransform = transform;
                        }
                        
                        var formTranslate = new DoubleAnimation
                        {
                            From = 0,
                            To = -30,
                            Duration = TimeSpan.FromMilliseconds(200),
                            EasingFunction = easeOut
                        };
                        Storyboard.SetTarget(formTranslate, transform);
                        Storyboard.SetTargetProperty(formTranslate, new PropertyPath(TranslateTransform.YProperty));
                        storyboard.Children.Add(formTranslate);
                    }

                    if (topButtonsPanel != null)
                    {
                        // 顶部按钮淡出动画（200ms）
                        var buttonsFadeOut = new DoubleAnimation
                        {
                            From = 1.0,
                            To = 0.0,
                            Duration = TimeSpan.FromMilliseconds(200),
                            EasingFunction = easeOut
                        };
                        Storyboard.SetTarget(buttonsFadeOut, topButtonsPanel);
                        Storyboard.SetTargetProperty(buttonsFadeOut, new PropertyPath(UIElement.OpacityProperty));
                        storyboard.Children.Add(buttonsFadeOut);
                    }

                    // 进度条淡入动画（200ms，延迟50ms开始，与表单淡出有重叠，更平滑）
                    if (progressBarLayer != null)
                    {
                        var progressFadeIn = new DoubleAnimation
                        {
                            From = 0.0,
                            To = 1.0,
                            Duration = TimeSpan.FromMilliseconds(200),
                            BeginTime = TimeSpan.FromMilliseconds(50), // 延迟50ms，与表单淡出重叠
                            EasingFunction = easeIn
                        };
                        Storyboard.SetTarget(progressFadeIn, progressBarLayer);
                        Storyboard.SetTargetProperty(progressFadeIn, new PropertyPath(UIElement.OpacityProperty));
                        storyboard.Children.Add(progressFadeIn);
                    }

                    // 开始动画（使用 FillBehavior.HoldEnd 保持最终状态）
                    storyboard.FillBehavior = FillBehavior.HoldEnd;
                    storyboard.Begin();
                }
                else
                {
                    // 隐藏进度条：进度条淡出，表单淡入（登录失败时）
                    var storyboard = new Storyboard();
                    
                    var easeOut = new CubicEase { EasingMode = EasingMode.EaseOut };
                    var easeIn = new CubicEase { EasingMode = EasingMode.EaseIn };

                    if (progressBarLayer != null)
                    {
                        var progressFadeOut = new DoubleAnimation
                        {
                            From = 1.0,
                            To = 0.0,
                            Duration = TimeSpan.FromMilliseconds(200),
                            EasingFunction = easeOut
                        };
                        Storyboard.SetTarget(progressFadeOut, progressBarLayer);
                        Storyboard.SetTargetProperty(progressFadeOut, new PropertyPath(UIElement.OpacityProperty));
                        storyboard.Children.Add(progressFadeOut);
                    }

                    if (loginFormBorder != null)
                    {
                        var formFadeIn = new DoubleAnimation
                        {
                            From = 0.0,
                            To = 1.0,
                            Duration = TimeSpan.FromMilliseconds(300),
                            BeginTime = TimeSpan.FromMilliseconds(100), // 延迟100ms，等进度条开始淡出后再淡入
                            EasingFunction = easeIn
                        };
                        Storyboard.SetTarget(formFadeIn, loginFormBorder);
                        Storyboard.SetTargetProperty(formFadeIn, new PropertyPath(UIElement.OpacityProperty));
                        storyboard.Children.Add(formFadeIn);

                        if (loginFormBorder.RenderTransform is not TranslateTransform transform)
                        {
                            transform = new TranslateTransform();
                            loginFormBorder.RenderTransform = transform;
                        }

                        var formTranslate = new DoubleAnimation
                        {
                            From = -50,
                            To = 0,
                            Duration = TimeSpan.FromMilliseconds(300),
                            BeginTime = TimeSpan.FromMilliseconds(100),
                            EasingFunction = easeIn
                        };
                        Storyboard.SetTarget(formTranslate, transform);
                        Storyboard.SetTargetProperty(formTranslate, new PropertyPath(TranslateTransform.YProperty));
                        storyboard.Children.Add(formTranslate);
                    }

                    if (topButtonsPanel != null)
                    {
                        var buttonsFadeIn = new DoubleAnimation
                        {
                            From = 0.0,
                            To = 1.0,
                            Duration = TimeSpan.FromMilliseconds(300),
                            BeginTime = TimeSpan.FromMilliseconds(100),
                            EasingFunction = easeIn
                        };
                        Storyboard.SetTarget(buttonsFadeIn, topButtonsPanel);
                        Storyboard.SetTargetProperty(buttonsFadeIn, new PropertyPath(UIElement.OpacityProperty));
                        storyboard.Children.Add(buttonsFadeIn);
                    }

                    storyboard.Begin();
                }
        }
    }
    
    /// <summary>
    /// 初始化主题设置
    /// </summary>
    private void InitializeTheme()
    {
                var savedTheme = AppSettingsHelper.GetTheme();
                var themeMode = ParseTheme(savedTheme);
                _themeService.SetTheme(themeMode);
        
                var appliedTheme = _themeService.GetAppliedThemeMode();
                UpdateThemeIcon(appliedTheme);
                UpdateBrandAreaBackground(appliedTheme);
    }
    
    /// <summary>
    /// 初始化语言设置
    /// </summary>
    private void InitializeLanguage()
    {
                var savedLang = AppSettingsHelper.GetLanguage();
                var systemLang = SystemInfoHelper.GetSystemLanguageCode();
                var initLang = string.IsNullOrWhiteSpace(savedLang) ? MapSystemLanguage(systemLang) : savedLang;
        
                if (!string.IsNullOrWhiteSpace(initLang) && _localizationManager.CurrentLanguage != initLang)
                {
                    _localizationManager.ChangeLanguage(initLang);
                }

                UpdateLanguageIcon();
    }
                
    /// <summary>
    /// 初始化后台任务（数据库检查和语言列表加载）
    /// </summary>
    private async Task InitializeBackgroundTasksAsync()
    {
                // 检查数据库连接状态
                _ = CheckDatabaseConnectionAsync();
                
        // 加载语言列表
                    try
                    {
                        await LoadAvailableLanguagesAsync().ConfigureAwait(false);
                        
                        Dispatcher.Invoke(() =>
                        {
                            UpdateLanguageMenuItems();
                            UpdateLanguageIcon();
                        });
                    }
                    catch (Exception ex)
                    {
                        var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
                        operLog?.Error(ex, "[登录] 后台加载语言列表失败");
        }
    }

    private void LoginWindow_Closed(object? sender, EventArgs e)
    {
        _themeService.ThemeChanged -= OnThemeChanged;
        _localizationManager.LanguageChanged -= OnLanguageChanged;
        
        // 取消订阅 ViewModel 属性变化事件
        if (DataContext is ViewModels.Identity.LoginViewModel viewModel)
        {
            viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }
    }
    
    /// <summary>
    /// 加载可用语言列表
    /// </summary>
    private async Task LoadAvailableLanguagesAsync()
    {
        try
        {
            var result = await _languageService.OptionAsync(false);
            if (result.Success && result.Data != null)
            {
                _availableLanguages = result.Data;
                Dispatcher.Invoke(() =>
                {
                    UpdateLanguageMenuItems();
                });
            }
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[登录] 加载可用语言列表失败");
        }
    }

    /// <summary>
    /// 初始化主题图标
    /// </summary>
    private void InitializeThemeComboBox()
    {
        UpdateThemeIcon(_themeService.GetAppliedThemeMode());
    }

    /// <summary>
    /// 更新主题图标显示
    /// </summary>
    private void UpdateThemeIcon(System.Windows.ThemeMode currentTheme)
    {
        if (ThemeIcon == null || ThemeButton == null) return;
        
        // 确定下一个主题
        System.Windows.ThemeMode nextTheme;
        if (currentTheme == System.Windows.ThemeMode.Light)
        {
            nextTheme = System.Windows.ThemeMode.Dark;
        }
        else if (currentTheme == System.Windows.ThemeMode.Dark)
        {
            nextTheme = System.Windows.ThemeMode.System;
        }
        else
        {
            nextTheme = System.Windows.ThemeMode.Light;
        }
        
        // 获取主题名称和切换提示文本
        var currentThemeName = GetThemeName(currentTheme);
        var nextThemeName = GetThemeName(nextTheme);
        var clickToSwitch = _localizationManager.GetString("common.clickToSwitch");
        
        // 动态拼接 ToolTip 文本
        var toolTipText = $"{currentThemeName}（{clickToSwitch}{nextThemeName}）";
        ThemeButton.ToolTip = toolTipText;
        
        // 使用 FontAwesome 图标：Sun - 浅色主题, Moon - 深色主题, Palette - 跟随系统
        if (currentTheme == System.Windows.ThemeMode.Light)
        {
            ThemeIcon.Icon = IconChar.Sun;
        }
        else if (currentTheme == System.Windows.ThemeMode.Dark)
        {
            ThemeIcon.Icon = IconChar.Moon;
        }
        else
        {
            ThemeIcon.Icon = IconChar.Palette;
        }
    }
    
    /// <summary>
    /// 获取主题的本地化名称
    /// </summary>
    private string GetThemeName(System.Windows.ThemeMode theme)
    {
        if (theme == System.Windows.ThemeMode.Light)
        {
            return _localizationManager.GetString("common.theme.light");
        }
        else if (theme == System.Windows.ThemeMode.Dark)
        {
            return _localizationManager.GetString("common.theme.dark");
        }
        else if (theme == System.Windows.ThemeMode.System)
        {
            return _localizationManager.GetString("common.theme.system");
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 主题变化事件处理
    /// </summary>
    private void OnThemeChanged(object? sender, System.Windows.ThemeMode e)
    {
        Dispatcher.Invoke(() =>
        {
            UpdateBrandAreaBackground(e);
            UpdateThemeIcon(e);
        });
    }

    /// <summary>
    /// 主题按钮点击事件 - 循环切换主题
    /// </summary>
    private void ThemeButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var currentTheme = _themeService.GetCurrentTheme();
            
            // 如果当前主题是 None，默认使用 System
            if (currentTheme == System.Windows.ThemeMode.None)
            {
                currentTheme = System.Windows.ThemeMode.System;
            }
            
            System.Windows.ThemeMode nextTheme;
            
            if (currentTheme == System.Windows.ThemeMode.Light)
            {
                nextTheme = System.Windows.ThemeMode.Dark;
            }
            else if (currentTheme == System.Windows.ThemeMode.Dark)
            {
                nextTheme = System.Windows.ThemeMode.System;
            }
            else
            {
                nextTheme = System.Windows.ThemeMode.Light;
            }

            // 设置主题（ThemeService.SetTheme 已经包含了保存逻辑，不需要再调用 LocalConfigHelper）
            _themeService.SetTheme(nextTheme);
            
            // 立即更新 UI（不等待，确保立即响应）
            var appliedTheme = _themeService.GetAppliedThemeMode();
            UpdateThemeIcon(appliedTheme);
            UpdateBrandAreaBackground(appliedTheme);
            
            // 更新主题提示文本
            if (DataContext is ViewModels.Identity.LoginViewModel viewModel)
            {
                viewModel.ThemeToolTip = _localizationManager.GetString("common.button.changeTheme");
            }
        }
        catch (Exception ex)
        {
            var operLog = App.Services?.GetService<Takt.Common.Logging.OperLogManager>();
            operLog?.Error(ex, "[登录] 主题切换失败");
        }
    }

    /// <summary>
    /// 更新品牌展示区和登录表单背景色（根据主题）
    /// </summary>
    private void UpdateBrandAreaBackground(System.Windows.ThemeMode _)
    {
        var layerBrush = TryFindResource("LayerFillColorDefaultBrush") as SolidColorBrush ?? new SolidColorBrush(Colors.Transparent);

        if (BrandAreaBorder != null)
        {
            BrandAreaBorder.Background = layerBrush;
        }

        if (LoginFormBorder != null)
        {
            LoginFormBorder.Background = layerBrush;
        }
    }

    private static string MapSystemLanguage(string systemCode)
    {
        if (string.IsNullOrWhiteSpace(systemCode)) return "zh-CN";
        var s = systemCode.ToLowerInvariant();
        if (s.StartsWith("zh")) return "zh-CN";
        if (s.StartsWith("ja")) return "ja-JP";
        return "en-US";
    }

    private static System.Windows.ThemeMode ParseTheme(string? stored)
    {
        return stored switch
        {
            "Light" => System.Windows.ThemeMode.Light,
            "Takt365" => System.Windows.ThemeMode.System,
            _ => System.Windows.ThemeMode.Dark
        };
    }


    /// <summary>
    /// 初始化语言菜单（不等待数据加载，数据加载完成后会自动更新）
    /// </summary>
    private void InitializeLanguageMenu()
    {
        // 如果数据已加载，立即更新菜单
        if (_availableLanguages.Count > 0)
        {
            UpdateLanguageMenuItems();
        }
        // 否则等待后台加载完成后自动更新（在 Loaded 事件中处理）
    }
    
    private void LanguageButton_Click(object sender, RoutedEventArgs e)
    {
        if (LanguageContextMenu == null) return;
        
        if (sender is Button button)
        {
            LanguageContextMenu.PlacementTarget = button;
        }
        LanguageContextMenu.Placement = PlacementMode.Bottom;
        LanguageContextMenu.IsOpen = true;
    }
    
    /// <summary>
    /// 更新语言菜单项（仅在首次加载或数据变化时调用）
    /// </summary>
    private void UpdateLanguageMenuItems()
    {
        if (LanguageContextMenu == null) return;
        
        // 只在 ItemsSource 为空或数据实际变化时更新，避免频繁刷新导致卡顿
        if (LanguageContextMenu.ItemsSource == null || LanguageContextMenu.ItemsSource != _availableLanguages)
        {
            LanguageContextMenu.ItemsSource = _availableLanguages;
        }
        
        // 延迟更新选中状态，等待容器生成完成
        Dispatcher.BeginInvoke(new Action(() =>
        {
            UpdateMenuItemsCheckedState();
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }
    
    /// <summary>
    /// 更新菜单项的选中状态
    /// </summary>
    private void UpdateMenuItemsCheckedState()
    {
        if (LanguageContextMenu == null) return;
        
        var currentLang = _localizationManager.CurrentLanguage;
        
        foreach (var language in _availableLanguages)
        {
            if (LanguageContextMenu.ItemContainerGenerator.ContainerFromItem(language) is System.Windows.Controls.MenuItem menuItem)
            {
                // 只更新选中状态，不触发其他更新
                var shouldBeChecked = language.Code == currentLang;
                if (menuItem.IsChecked != shouldBeChecked)
                {
                    menuItem.IsChecked = shouldBeChecked;
                }
            }
        }
    }
    
    /// <summary>
    /// 语言菜单子菜单打开事件
    /// </summary>
    private async void LanguageContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (_availableLanguages.Count == 0)
        {
            await LoadAvailableLanguagesAsync();
        }
        
        // 只在菜单首次打开或数据变化时更新，避免频繁刷新导致卡顿
        UpdateLanguageMenuItems();
    }
    
    /// <summary>
    /// 语言菜单项点击事件
    /// </summary>
    private void LanguageMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.MenuItem menuItem && menuItem.DataContext is Takt.Application.Dtos.Routine.LanguageOptionDto language)
        {
            _localizationManager.ChangeLanguage(language.Code);
            AppSettingsHelper.SaveLanguage(language.Code);
            UpdateLanguageIcon();
            UpdateTranslations();
            // 只更新选中状态，不需要重新设置 ItemsSource
            UpdateMenuItemsCheckedState();
            if (LanguageContextMenu != null)
            {
                LanguageContextMenu.IsOpen = false;
            }
        }
    }

    /// <summary>
    /// 更新语言图标显示
    /// </summary>
    private void UpdateLanguageIcon()
    {
        if (LanguageIcon == null || LanguageButton == null) return;
        
        var currentLang = _localizationManager.CurrentLanguage;
        // 语言图标使用 FontAwesome 的 Globe 图标（地球图标，表示国际化/语言切换）
        LanguageIcon.Icon = IconChar.Globe;
        LanguageIcon.Visibility = Visibility.Visible;
        
        // 更新工具提示
        var toolTip = _localizationManager.GetString("common.button.changeLanguage");
        var currentLanguage = _availableLanguages.FirstOrDefault(l => l.Code == currentLang);
        string toolTipText;
        if (currentLanguage != null)
        {
            toolTipText = $"{toolTip} ({currentLanguage.Name})";
        }
        else
        {
            toolTipText = $"{toolTip} ({currentLang})";
        }
        LanguageButton.ToolTip = toolTipText;
        
        // 只更新菜单项的选中状态，不需要重新设置 ItemsSource
        UpdateMenuItemsCheckedState();
        
        if (DataContext is ViewModels.Identity.LoginViewModel viewModel)
        {
            viewModel.LanguageToolTip = toolTip;
        }
    }

    /// <summary>
    /// 语言切换事件处理
    /// </summary>
    private void OnLanguageChanged(object? sender, string languageCode)
    {
        Dispatcher.Invoke(() =>
        {
            UpdateLanguageIcon();
            UpdateTranslations();
        });
    }

    /// <summary>
    /// 更新所有翻译文本
    /// </summary>
    private void UpdateTranslations()
    {
        // 窗口标题（其余文本已用 XAML 标记扩展绑定）
        this.Title = _localizationManager.GetString("Login.Title") ?? string.Empty;

        // 更新 ViewModel 中的 ToolTip 属性（Hint 已通过 XAML 的 local:Loc 绑定）
        if (DataContext is ViewModels.Identity.LoginViewModel viewModel)
        {
            viewModel.LanguageToolTip = _localizationManager.GetString("common.button.changeLanguage");
            viewModel.ThemeToolTip = _localizationManager.GetString("common.button.changeTheme");
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        // 原生 PasswordBox 需要手动同步到 ViewModel
        if (DataContext is ViewModels.Identity.LoginViewModel viewModel && sender is PasswordBox passwordBox)
        {
            viewModel.Password = passwordBox.Password;
        }
    }

    /// <summary>
    /// 调整输入框的 Padding，为左侧图标留出空间
    /// </summary>
    private void AdjustInputPadding()
    {
        const double iconLeftMargin = 12;
        const double iconWidth = 16;
        const double iconRightSpacing = 12;
        const double leftPadding = iconLeftMargin + iconWidth + iconRightSpacing;
        
        if (FindName("UsernameTextBox") is TextBox usernameTextBox)
        {
            usernameTextBox.Padding = new Thickness(leftPadding, 10, 12, 10);
        }

        if (FindName("PasswordBox") is PasswordBox passwordBox)
        {
            passwordBox.Padding = new Thickness(leftPadding, 10, 12, 10);
        }
    }

    /// <summary>
    /// 检查数据库连接状态
    /// </summary>
    private async Task CheckDatabaseConnectionAsync()
    {
        try
        {
            // 获取数据库上下文
            var dbContext = App.Services?.GetService<Takt.Infrastructure.Data.DbContext>();
            if (dbContext == null)
            {
                Dispatcher.Invoke(() =>
                {
                    TaktMessageBox.Error(
                        _localizationManager.GetString("Database.ConnectionError.ServiceNotFound") ?? "无法获取数据库服务",
                        _localizationManager.GetString("Common.Error") ?? "错误",
                        this);
                });
                return;
            }

            // 异步检查数据库连接（设置超时时间 5 秒）
            var checkTask = dbContext.CheckConnectionAsync();
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));
            var completedTask = await Task.WhenAny(checkTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                // 超时
                Dispatcher.Invoke(() =>
                {
                    TaktMessageBox.Warning(
                        _localizationManager.GetString("Database.ConnectionError.Timeout") ?? "数据库连接超时，请检查网络连接和数据库服务器状态",
                        _localizationManager.GetString("Common.Error") ?? "错误",
                        this);
                });
                return;
            }

            var isConnected = await checkTask;
            if (!isConnected)
            {
                // 连接失败
                Dispatcher.Invoke(() =>
                {
                    TaktMessageBox.Error(
                        _localizationManager.GetString("Database.ConnectionError.Failed") ?? "无法连接到数据库，请检查数据库连接配置和服务器状态",
                        _localizationManager.GetString("Common.Error") ?? "错误",
                        this);
                });
            }
        }
        catch (Exception ex)
        {
            // 异常处理
            Dispatcher.Invoke(() =>
            {
                var errorMessage = _localizationManager.GetString("Database.ConnectionError.Exception") 
                    ?? "数据库连接检查时发生异常";
                TaktMessageBox.Error(
                    $"{errorMessage}\n\n{ex.Message}",
                    _localizationManager.GetString("Common.Error") ?? "错误",
                    this);
            });
        }
    }
}
#pragma warning restore WPF0001