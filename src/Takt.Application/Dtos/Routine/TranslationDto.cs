//===================================================================
// 项目名 : Takt.Wpf
// 命名空间：Takt.Application.Dtos.Routine
// 文件名 : TranslationDto.cs
// 创建者 : Takt365(Cursor AI)
// 创建时间: 2025-01-20
// 功能描述：翻译数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Routine;

/// <summary>
/// 翻译数据传输对象
/// 用于传输翻译信息
/// </summary>
public class TranslationDto
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 语言代码
    /// </summary>
    public string LanguageCode { get; set; } = string.Empty;
    
    /// <summary>
    /// 翻译键
    /// </summary>
    public string TranslationKey { get; set; } = string.Empty;
    
    /// <summary>
    /// 翻译值
    /// </summary>
    public string TranslationValue { get; set; } = string.Empty;
    
    /// <summary>
    /// 模块
    /// </summary>
    public string? Module { get; set; }
    
    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? Remarks { get; set; }
    
    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreatedBy { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }
    
    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdatedBy { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedTime { get; set; }
    
    /// <summary>
    /// 是否删除（0=否，1=是）
    /// </summary>
    public int IsDeleted { get; set; }
    
    /// <summary>
    /// 删除人
    /// </summary>
    public string? DeletedBy { get; set; }
    
    /// <summary>
    /// 删除时间
    /// </summary>
    public DateTime? DeletedTime { get; set; }
}

/// <summary>
/// 翻译查询数据传输对象
/// </summary>
public class TranslationQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 搜索关键词（支持在翻译键、翻译值、模块中搜索）
    /// </summary>
    public string? Keywords { get; set; }
    
    /// <summary>
    /// 语言ID
    /// </summary>
    public string? LanguageCode { get; set; }

    /// <summary>
    /// 翻译键
    /// </summary>
    public string? TranslationKey { get; set; }

    /// <summary>
    /// 模块
    /// </summary>
    public string? Module { get; set; }
}

/// <summary>
/// 创建翻译数据传输对象
/// </summary>
public class TranslationCreateDto
{
    /// <summary>
    /// 语言ID
    /// </summary>
    public string LanguageCode { get; set; } = string.Empty;

    /// <summary>
    /// 翻译键
    /// </summary>
    public string TranslationKey { get; set; } = string.Empty;

    /// <summary>
    /// 翻译值
    /// </summary>
    public string TranslationValue { get; set; } = string.Empty;

    /// <summary>
    /// 模块
    /// </summary>
    public string? Module { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remarks { get; set; }
}

/// <summary>
/// 更新翻译数据传输对象
/// </summary>
public class TranslationUpdateDto : TranslationCreateDto
{
    /// <summary>
    /// 翻译ID
    /// </summary>
    public long Id { get; set; }
}
