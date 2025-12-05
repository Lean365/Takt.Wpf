// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Dtos.Logistics.Visitors
// 文件名称：VisitorDetailDto.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：访客详情数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Logistics.Visitors;

/// <summary>
/// 访客详情数据传输对象
/// </summary>
public class VisitorDetailDto
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 访客ID
    /// </summary>
    public long VisitorId { get; set; }
    
    /// <summary>
    /// 部门
    /// </summary>
    public string Department { get; set; } = string.Empty;
    
    /// <summary>
    /// 姓名
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 职位
    /// </summary>
    public string Position { get; set; } = string.Empty;
    
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
/// 访客详情查询数据传输对象
/// </summary>
public class VisitorDetailQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 搜索关键词（支持在姓名、部门、职位中搜索）
    /// </summary>
    public string? Keywords { get; set; }
    
    /// <summary>
    /// 访客ID
    /// </summary>
    public long? VisitorId { get; set; }
    
    /// <summary>
    /// 姓名
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// 部门
    /// </summary>
    public string? Department { get; set; }
}

/// <summary>
/// 创建访客详情数据传输对象
/// </summary>
public class VisitorDetailCreateDto
{
    /// <summary>
    /// 访客ID
    /// </summary>
    public long VisitorId { get; set; }
    
    /// <summary>
    /// 部门
    /// </summary>
    public string Department { get; set; } = string.Empty;
    
    /// <summary>
    /// 姓名
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 职位
    /// </summary>
    public string Position { get; set; } = string.Empty;
    
    /// <summary>
    /// 备注
    /// </summary>
    public string? Remarks { get; set; }
}

/// <summary>
/// 更新访客详情数据传输对象
/// </summary>
public class VisitorDetailUpdateDto : VisitorDetailCreateDto
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }
}

