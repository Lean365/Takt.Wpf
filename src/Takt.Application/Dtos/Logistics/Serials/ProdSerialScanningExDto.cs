// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Dtos.Logistics.Serials
// 文件名称：ProdSerialScanningExDto.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：产品序列号扫描异常记录数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Logistics.Serials;

/// <summary>
/// 产品序列号扫描异常记录数据传输对象
/// </summary>
public class ProdSerialScanningExDto
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 入库完整序列号
    /// </summary>
    public string? InboundFullSerialNumber { get; set; }

    /// <summary>
    /// 入库日期
    /// </summary>
    public DateTime? InboundDate { get; set; }

    /// <summary>
    /// 入库用户
    /// </summary>
    public string? InboundClient { get; set; }

    /// <summary>
    /// 入库IP
    /// </summary>
    public string? InboundIp { get; set; }

    /// <summary>
    /// 入库机器名称
    /// </summary>
    public string? InboundMachineName { get; set; }

    /// <summary>
    /// 入库地点
    /// </summary>
    public string? InboundLocation { get; set; }

    /// <summary>
    /// 入库OS
    /// </summary>
    public string? InboundOs { get; set; }

    /// <summary>
    /// 入库异常描述
    /// </summary>
    public string? InboundDesc { get; set; }

    /// <summary>
    /// 出库单号
    /// </summary>
    public string? OutboundNo { get; set; }

    /// <summary>
    /// 仕向编码
    /// </summary>
    public string? DestCode { get; set; }

    /// <summary>
    /// 目的地港口
    /// </summary>
    public string? DestPort { get; set; }

    /// <summary>
    /// 出库日期
    /// </summary>
    public DateTime? OutboundDate { get; set; }

    /// <summary>
    /// 出库完整序列号
    /// </summary>
    public string? OutboundFullSerialNumber { get; set; }

    /// <summary>
    /// 出库用户
    /// </summary>
    public string? OutboundClient { get; set; }

    /// <summary>
    /// 出库IP
    /// </summary>
    public string? OutboundIp { get; set; }

    /// <summary>
    /// 出库机器名称
    /// </summary>
    public string? OutboundMachineName { get; set; }

    /// <summary>
    /// 出库地点
    /// </summary>
    public string? OutboundLocation { get; set; }

    /// <summary>
    /// 出库OS
    /// </summary>
    public string? OutboundOs { get; set; }

    /// <summary>
    /// 出库异常描述
    /// </summary>
    public string? OutboundDesc { get; set; }

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
/// 产品序列号扫描异常记录查询数据传输对象
/// </summary>
public class ProdSerialScanningExQueryDto : Takt.Common.Results.PagedQuery
{
    /// <summary>
    /// 搜索关键词（支持在入库/出库完整序列号、异常描述中搜索）
    /// </summary>
    public string? Keywords { get; set; }

    /// <summary>
    /// 物料代码（通过完整序列号解析）
    /// </summary>
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 入库完整序列号
    /// </summary>
    public string? InboundFullSerialNumber { get; set; }

    /// <summary>
    /// 出库完整序列号
    /// </summary>
    public string? OutboundFullSerialNumber { get; set; }

    /// <summary>
    /// 入库日期（起始）
    /// </summary>
    public DateTime? InboundDateFrom { get; set; }

    /// <summary>
    /// 入库日期（结束）
    /// </summary>
    public DateTime? InboundDateTo { get; set; }

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
/// 入库扫描异常记录创建数据传输对象
/// </summary>
public class ProdSerialInboundScanningExCreateDto
{
    /// <summary>
    /// 入库完整序列号
    /// </summary>
    public string InboundFullSerialNumber { get; set; } = string.Empty;

    /// <summary>
    /// 入库日期
    /// </summary>
    public DateTime? InboundDate { get; set; }

    /// <summary>
    /// 入库用户
    /// </summary>
    public string? InboundClient { get; set; }

    /// <summary>
    /// 入库IP
    /// </summary>
    public string? InboundIp { get; set; }

    /// <summary>
    /// 入库机器名称
    /// </summary>
    public string? InboundMachineName { get; set; }

    /// <summary>
    /// 入库地点
    /// </summary>
    public string? InboundLocation { get; set; }

    /// <summary>
    /// 入库OS
    /// </summary>
    public string? InboundOs { get; set; }

    /// <summary>
    /// 入库异常描述
    /// </summary>
    public string? InboundDesc { get; set; }
}

/// <summary>
/// 出库扫描异常记录创建数据传输对象
/// </summary>
public class ProdSerialOutboundScanningExCreateDto
{
    /// <summary>
    /// 出库单号
    /// </summary>
    public string? OutboundNo { get; set; }

    /// <summary>
    /// 仕向编码
    /// </summary>
    public string? DestCode { get; set; }

    /// <summary>
    /// 目的地港口
    /// </summary>
    public string? DestPort { get; set; }

    /// <summary>
    /// 出库完整序列号
    /// </summary>
    public string OutboundFullSerialNumber { get; set; } = string.Empty;

    /// <summary>
    /// 出库日期
    /// </summary>
    public DateTime? OutboundDate { get; set; }

    /// <summary>
    /// 出库用户
    /// </summary>
    public string? OutboundClient { get; set; }

    /// <summary>
    /// 出库IP
    /// </summary>
    public string? OutboundIp { get; set; }

    /// <summary>
    /// 出库机器名称
    /// </summary>
    public string? OutboundMachineName { get; set; }

    /// <summary>
    /// 出库地点
    /// </summary>
    public string? OutboundLocation { get; set; }

    /// <summary>
    /// 出库OS
    /// </summary>
    public string? OutboundOs { get; set; }

    /// <summary>
    /// 出库异常描述
    /// </summary>
    public string? OutboundDesc { get; set; }
}

/// <summary>
/// 产品序列号扫描异常记录更新数据传输对象
/// </summary>
public class ProdSerialScanningExUpdateDto
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 入库完整序列号
    /// </summary>
    public string? InboundFullSerialNumber { get; set; }

    /// <summary>
    /// 入库日期
    /// </summary>
    public DateTime? InboundDate { get; set; }

    /// <summary>
    /// 入库用户
    /// </summary>
    public string? InboundClient { get; set; }

    /// <summary>
    /// 入库IP
    /// </summary>
    public string? InboundIp { get; set; }

    /// <summary>
    /// 入库机器名称
    /// </summary>
    public string? InboundMachineName { get; set; }

    /// <summary>
    /// 入库地点
    /// </summary>
    public string? InboundLocation { get; set; }

    /// <summary>
    /// 入库OS
    /// </summary>
    public string? InboundOs { get; set; }

    /// <summary>
    /// 入库异常描述
    /// </summary>
    public string? InboundDesc { get; set; }

    /// <summary>
    /// 出库单号
    /// </summary>
    public string? OutboundNo { get; set; }

    /// <summary>
    /// 仕向编码
    /// </summary>
    public string? DestCode { get; set; }

    /// <summary>
    /// 目的地港口
    /// </summary>
    public string? DestPort { get; set; }

    /// <summary>
    /// 出库日期
    /// </summary>
    public DateTime? OutboundDate { get; set; }

    /// <summary>
    /// 出库完整序列号
    /// </summary>
    public string? OutboundFullSerialNumber { get; set; }

    /// <summary>
    /// 出库用户
    /// </summary>
    public string? OutboundClient { get; set; }

    /// <summary>
    /// 出库IP
    /// </summary>
    public string? OutboundIp { get; set; }

    /// <summary>
    /// 出库机器名称
    /// </summary>
    public string? OutboundMachineName { get; set; }

    /// <summary>
    /// 出库地点
    /// </summary>
    public string? OutboundLocation { get; set; }

    /// <summary>
    /// 出库OS
    /// </summary>
    public string? OutboundOs { get; set; }

    /// <summary>
    /// 出库异常描述
    /// </summary>
    public string? OutboundDesc { get; set; }
}

/// <summary>
/// 产品序列号扫描异常记录导出数据传输对象
/// </summary>
public class ProdSerialScanningExExportDto
{
    /// <summary>
    /// 入库完整序列号
    /// </summary>
    public string? InboundFullSerialNumber { get; set; }

    /// <summary>
    /// 入库日期
    /// </summary>
    public DateTime? InboundDate { get; set; }

    /// <summary>
    /// 入库用户
    /// </summary>
    public string? InboundClient { get; set; }

    /// <summary>
    /// 入库IP
    /// </summary>
    public string? InboundIp { get; set; }

    /// <summary>
    /// 入库机器名称
    /// </summary>
    public string? InboundMachineName { get; set; }

    /// <summary>
    /// 入库地点
    /// </summary>
    public string? InboundLocation { get; set; }

    /// <summary>
    /// 入库OS
    /// </summary>
    public string? InboundOs { get; set; }

    /// <summary>
    /// 入库异常描述
    /// </summary>
    public string? InboundDesc { get; set; }

    /// <summary>
    /// 出库单号
    /// </summary>
    public string? OutboundNo { get; set; }

    /// <summary>
    /// 仕向编码
    /// </summary>
    public string? DestCode { get; set; }

    /// <summary>
    /// 目的地港口
    /// </summary>
    public string? DestPort { get; set; }

    /// <summary>
    /// 出库日期
    /// </summary>
    public DateTime? OutboundDate { get; set; }

    /// <summary>
    /// 出库完整序列号
    /// </summary>
    public string? OutboundFullSerialNumber { get; set; }

    /// <summary>
    /// 出库用户
    /// </summary>
    public string? OutboundClient { get; set; }

    /// <summary>
    /// 出库IP
    /// </summary>
    public string? OutboundIp { get; set; }

    /// <summary>
    /// 出库机器名称
    /// </summary>
    public string? OutboundMachineName { get; set; }

    /// <summary>
    /// 出库地点
    /// </summary>
    public string? OutboundLocation { get; set; }

    /// <summary>
    /// 出库OS
    /// </summary>
    public string? OutboundOs { get; set; }

    /// <summary>
    /// 出库异常描述
    /// </summary>
    public string? OutboundDesc { get; set; }

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
}

