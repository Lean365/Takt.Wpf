// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Dtos.Logistics.Serials
// 文件名称：ProdSerialOutboundStatisticDto.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：产品序列号出库统计数据传输对象
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

namespace Takt.Application.Dtos.Logistics.Serials;

/// <summary>
/// 产品序列号出库统计数据传输对象
/// </summary>
public class ProdSerialOutboundStatisticDto
{
    /// <summary>
    /// 统计维度（年或月）
    /// 格式：年统计为 "2025"，月统计为 "2025-01"
    /// </summary>
    public string Period { get; set; } = string.Empty;

    /// <summary>
    /// 仕向编码（DestCode）
    /// </summary>
    public string? DestCode { get; set; }

    /// <summary>
    /// 目的地港口（DestPort）
    /// </summary>
    public string? DestPort { get; set; }

    /// <summary>
    /// 总数量
    /// </summary>
    public decimal TotalQuantity { get; set; }

    /// <summary>
    /// 占比（百分比）
    /// </summary>
    public double Percentage { get; set; }
}

/// <summary>
/// 产品序列号出库统计查询参数
/// </summary>
public class ProdSerialOutboundStatisticQueryDto
{
    /// <summary>
    /// 统计类型：Year（按年）或 Month（按月）或 Both（同时返回按年和按月）
    /// </summary>
    public string StatisticType { get; set; } = "Year";

    /// <summary>
    /// 统计维度：DestCode（按仕向编码）或 DestPort（按目的地港口）或 Both（同时统计）
    /// </summary>
    public string Dimension { get; set; } = "DestCode";

    /// <summary>
    /// 起始日期（可选）
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束日期（可选）
    /// </summary>
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// 产品序列号出库统计结果（包含按年和按月统计清单）
/// </summary>
public class ProdSerialOutboundStatisticResultDto
{
    /// <summary>
    /// 近年统计清单（按年统计）
    /// </summary>
    public List<ProdSerialOutboundStatisticDto> YearStatistics { get; set; } = new();

    /// <summary>
    /// 按月统计清单（按月统计）
    /// </summary>
    public List<ProdSerialOutboundStatisticDto> MonthStatistics { get; set; } = new();
}

