// ========================================
// 项目名称：Takt.Wpf
// 文件名称：LocExtension.cs
// 创建时间：2025-10-30
// 创建人：Hbt365(Cursor AI)
// 功能描述：XAML 本地化标记扩展：{local:Loc Key=Login.Welcome}
// ========================================

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using MaterialDesignThemes.Wpf;
using Takt.Fluent.Services;

namespace Takt.Fluent.Localization;

/// <summary>
/// 本地化标记扩展：在 XAML 中直接使用键
/// 例如：Text="{local:Loc Key=Login.Welcome}"
/// </summary>
public class LocExtension : MarkupExtension
{
    public string Key { get; set; } = string.Empty;
    public string? Param0 { get; set; }
    public string? Param1 { get; set; }
    public string? Param0Key { get; set; }
    public string? Param1Key { get; set; }

    public LocExtension() { }

    public LocExtension(string key)
    {
        Key = key;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        // 如果 Key 为空，直接返回空字符串（设计时或编译时）
        var keyValue = Key ?? string.Empty;
        if (string.IsNullOrWhiteSpace(keyValue))
        {
            return string.Empty;
        }

        // 通过 DI 获取本地化适配器（编译时可能为 null）
        var adapter = App.Services?.GetService(typeof(LocalizationAdapter)) as LocalizationAdapter;

        // 检查目标属性类型，避免绑定到 Name 等只读属性，或 HintAssist.Hint（MaterialDesign 内部需要读取值）
        if (serviceProvider != null)
        {
            try
            {
                var targetProperty = serviceProvider.GetService(typeof(System.Windows.Markup.IProvideValueTarget)) as System.Windows.Markup.IProvideValueTarget;
                if (targetProperty != null)
                {
                    // 检查目标对象类型
                    var targetObject = targetProperty.TargetObject;
                    if (targetObject is System.Windows.FrameworkElement)
                    {
                        // 检查目标属性
                        var targetPropertyInfo = targetProperty.TargetProperty as System.Reflection.PropertyInfo;
                        var targetDependencyProperty = targetProperty.TargetProperty as System.Windows.DependencyProperty;
                        
                        // 如果目标是 Name 属性（无论是 PropertyInfo 还是 DependencyProperty），直接返回字符串
                        if ((targetPropertyInfo != null && targetPropertyInfo.Name == "Name") ||
                            (targetDependencyProperty != null && targetDependencyProperty.Name == "Name"))
                        {
                            // 如果目标是 FrameworkElement.Name（只读属性），直接返回字符串，不使用绑定
                            return keyValue;
                        }
                        
                        // 如果目标是 HintAssist.Hint，直接返回当前翻译值（MaterialDesign 内部样式需要读取值）
                        // MaterialDesign 的 AutomationProperties.Name 绑定需要读取 HintAssist.Hint 的值，不能是 Binding 对象
                        // 参照 MaterialDesign 源码：HintProperty 是 typeof(object) 的附加属性
                        if (targetDependencyProperty != null)
                        {
                            // 使用多种方式识别 HintAssist.HintProperty，确保兼容性
                            bool isHintProperty = ReferenceEquals(targetDependencyProperty, HintAssist.HintProperty) ||
                                                   targetDependencyProperty.Equals(HintAssist.HintProperty) ||
                                                   (targetDependencyProperty.Name == "Hint" && 
                                                    targetDependencyProperty.OwnerType != null &&
                                                    (targetDependencyProperty.OwnerType == typeof(HintAssist) ||
                                                     targetDependencyProperty.OwnerType.FullName == typeof(HintAssist).FullName));
                            
                            if (isHintProperty)
                            {
                                // 获取当前翻译值，直接返回字符串而不是 Binding
                                // MaterialDesign 的 AutomationPropertiesNameConverter 只处理 string 类型
                                if (adapter != null)
                                {
                                    var translation = adapter.GetTranslation(keyValue, keyValue);
                                    // 如果没有翻译（返回的是键本身），返回键值
                                    return string.IsNullOrWhiteSpace(translation) || translation == keyValue ? keyValue : translation;
                                }
                                return keyValue;
                            }
                        }
                    }
                }
            }
            catch
            {
                // 如果检查失败，继续使用绑定
            }
        }
        if (adapter == null)
        {
            // 回退为静态文本：显示键
            return keyValue;
        }

        // 绑定到语言代码变化（INotifyPropertyChanged），使用转换器输出翻译
        try
        {
            // 再次检查目标属性，确保不是 Name 属性或 HintAssist.Hint
            if (serviceProvider != null)
            {
                var targetProperty = serviceProvider.GetService(typeof(System.Windows.Markup.IProvideValueTarget)) as System.Windows.Markup.IProvideValueTarget;
                if (targetProperty != null)
                {
                    var targetPropertyInfo = targetProperty.TargetProperty as System.Reflection.PropertyInfo;
                    var targetDependencyProperty = targetProperty.TargetProperty as System.Windows.DependencyProperty;
                    
                    // 如果目标是 Name 属性，直接返回字符串
                    if ((targetPropertyInfo != null && targetPropertyInfo.Name == "Name") ||
                        (targetDependencyProperty != null && targetDependencyProperty.Name == "Name"))
                    {
                        return keyValue;
                    }
                    
                    // 如果目标是 HintAssist.Hint，直接返回当前翻译值
                    // MaterialDesign 的 AutomationProperties.Name 绑定需要读取 HintAssist.Hint 的值，不能是 Binding 对象
                    // 参照 MaterialDesign 源码：HintProperty 是 typeof(object) 的附加属性
                    if (targetDependencyProperty != null)
                    {
                        // 使用多种方式识别 HintAssist.HintProperty，确保兼容性
                        bool isHintProperty = ReferenceEquals(targetDependencyProperty, HintAssist.HintProperty) ||
                                               targetDependencyProperty.Equals(HintAssist.HintProperty) ||
                                               (targetDependencyProperty.Name == "Hint" && 
                                                targetDependencyProperty.OwnerType != null &&
                                                (targetDependencyProperty.OwnerType == typeof(HintAssist) ||
                                                 targetDependencyProperty.OwnerType.FullName == typeof(HintAssist).FullName));
                        
                        if (isHintProperty)
                        {
                            // MaterialDesign 的 AutomationPropertiesNameConverter 只处理 string 类型
                            var translation = adapter.GetTranslation(keyValue, keyValue);
                            // 如果没有翻译（返回的是键本身），返回键值
                            return string.IsNullOrWhiteSpace(translation) || translation == keyValue ? keyValue : translation;
                        }
                    }
                }
            }
            
            var binding = new Binding("CurrentLanguageCode")
            {
                Source = adapter,
                Mode = BindingMode.OneWay,
                Converter = new LocValueConverter(adapter, keyValue, Param0, Param1, Param0Key, Param1Key),
                FallbackValue = keyValue,  // 设置回退值，避免编译时返回 null
                TargetNullValue = keyValue  // 设置目标 null 值，确保即使转换器返回 null 也有回退值
            };
            return binding.ProvideValue(serviceProvider);
        }
        catch
        {
            // 如果绑定失败（编译时），返回键本身
            return keyValue;
        }
    }
}

internal class LocValueConverter : IValueConverter
{
    private readonly LocalizationAdapter _localizationAdapter;
    private readonly string _key;
    private readonly string? _param0;
    private readonly string? _param1;
    private readonly string? _param0Key;
    private readonly string? _param1Key;

    public LocValueConverter(LocalizationAdapter localizationAdapter, string key, string? param0, string? param1, string? param0Key, string? param1Key)
    {
        _localizationAdapter = localizationAdapter;
        _key = key ?? string.Empty;
        _param0 = param0;
        _param1 = param1;
        _param0Key = param0Key;
        _param1Key = param1Key;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(_key))
        {
            return string.Empty;
        }
        if (_localizationAdapter == null)
        {
            return _key;
        }
        
        // 确保 baseText 不为 null
        var baseText = _localizationAdapter.GetTranslation(_key, _key) ?? _key;

        // 解析参数：优先使用 *Key 再使用字面值
        string? p0 = null;
        string? p1 = null;
        if (!string.IsNullOrWhiteSpace(_param0Key))
        {
            p0 = _localizationAdapter.GetTranslation(_param0Key!, _param0Key) ?? _param0Key;
        }
        else if (!string.IsNullOrWhiteSpace(_param0))
        {
            p0 = _param0;
        }

        if (!string.IsNullOrWhiteSpace(_param1Key))
        {
            p1 = _localizationAdapter.GetTranslation(_param1Key!, _param1Key) ?? _param1Key;
        }
        else if (!string.IsNullOrWhiteSpace(_param1))
        {
            p1 = _param1;
        }

        try
        {
            if (p0 != null && p1 != null)
            {
                return string.Format(baseText, p0, p1);
            }
            if (p0 != null)
            {
                return string.Format(baseText, p0);
            }
            return baseText;
        }
        catch
        {
            // 确保即使格式化失败也返回非 null 值
            return baseText ?? _key ?? string.Empty;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
