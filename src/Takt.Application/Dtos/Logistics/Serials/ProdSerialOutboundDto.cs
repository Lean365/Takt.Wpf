// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Dtos.Logistics.Serials
// 文件名称：ProdSerialOutboundDto.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：产品序列号出库记录数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Logistics.Serials;

/// <summary>
/// 产品序列号出库记录数据传输对象
/// </summary>
public class ProdSerialOutboundDto
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 完整序列号
    /// </summary>
    public string FullSerialNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// 物料代码
    /// </summary>
    public string MaterialCode { get; set; } = string.Empty;
    
    /// <summary>
    /// 序列号
    /// </summary>
    public string SerialNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// 数量
    /// </summary>
    public decimal Quantity { get; set; }
    
    /// <summary>
    /// 出库单号
    /// </summary>
    public string OutboundNo { get; set; } = string.Empty;
    
    /// <summary>
    /// 出库日期
    /// </summary>
    public DateTime OutboundDate { get; set; }
    
    /// <summary>
    /// 目标代码
    /// </summary>
    public string? DestCode { get; set; }
    
    /// <summary>
    /// 目的港
    /// </summary>
    public string? DestPort { get; set; }
    
    /// <summary>
    /// 重量
    /// 出库货物的重量（单位：千克）
    /// </summary>
    public decimal? Weight { get; set; }
    
    /// <summary>
    /// 体积
    /// 出库货物的体积（单位：立方米）
    /// </summary>
    public decimal? Volume { get; set; }
    
    /// <summary>
    /// 箱数
    /// 出库货物的箱数
    /// </summary>
    public int? CarQuantity { get; set; }
    
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
/// 产品序列号出库记录查询数据传输对象
/// </summary>
public class ProdSerialOutboundQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 搜索关键词（支持在出库单号、序列号、完整序列号中搜索）
    /// </summary>
    public string? Keywords { get; set; }
    
    /// <summary>
    /// 物料代码
    /// </summary>
    public string? MaterialCode { get; set; }
    
    /// <summary>
    /// 出库单号
    /// </summary>
    public string? OutboundNo { get; set; }
    
    /// <summary>
    /// 序列号
    /// </summary>
    public string? SerialNumber { get; set; }
    
    /// <summary>
    /// 出库日期（起始）
    /// </summary>
    public DateTime? OutboundDateFrom { get; set; }
    
    /// <summary>
    /// 出库日期（结束）
    /// </summary>
    public DateTime? OutboundDateTo { get; set; }
}

/// <summary>
/// 创建产品序列号出库记录数据传输对象
/// </summary>
public class ProdSerialOutboundCreateDto
{
    /// <summary>
    /// 完整序列号
    /// </summary>
    public string FullSerialNumber { get; set; } = string.Empty;    

    /// <summary>
    /// 仕向编码
    /// </summary>
    public string? DestCode { get; set; }
    
    /// <summary>
    /// 出库单号
    /// </summary>
    public string OutboundNo { get; set; } = string.Empty;
    
    /// <summary>
    /// 出库日期
    /// </summary>
    public DateTime OutboundDate { get; set; }   
    
    /// <summary>
    /// 目的港
    /// </summary>
    public string? DestPort { get; set; }

}

/// <summary>
/// 更新产品序列号出库记录数据传输对象
/// </summary>
public class ProdSerialOutboundUpdateDto : ProdSerialOutboundCreateDto
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }
}

