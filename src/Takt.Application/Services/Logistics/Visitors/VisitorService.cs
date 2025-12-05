// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Application.Services.Logistics.Visitors
// 文件名称：VisitorService.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：访客服务实现
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System.IO;
using System.Linq.Expressions;
using Mapster;
using Newtonsoft.Json;
using Takt.Application.Dtos.Logistics.Visitors;
using Takt.Common.Helpers;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Entities.Logistics.Visitors;
using Takt.Domain.Repositories;

namespace Takt.Application.Services.Logistics.Visitors;

/// <summary>
/// 访客服务实现
/// </summary>
public class VisitorService : IVisitorService
{
    private readonly IBaseRepository<Visitor> _visitorRepository;
    private readonly IBaseRepository<VisitorDetail> _visitorDetailRepository;
    private readonly AppLogManager _appLog;
    private readonly OperLogManager? _operLog;

    public VisitorService(
        IBaseRepository<Visitor> visitorRepository,
        IBaseRepository<VisitorDetail> visitorDetailRepository,
        AppLogManager appLog,
        OperLogManager? operLog = null)
    {
        _visitorRepository = visitorRepository;
        _visitorDetailRepository = visitorDetailRepository;
        _appLog = appLog;
        _operLog = operLog;
    }

    /// <summary>
    /// 查询访客列表（支持关键字和字段查询）
    /// </summary>
    /// <param name="query">查询条件对象，包含分页参数、关键字、公司名称等筛选条件</param>
    /// <returns>分页访客列表</returns>
    public async Task<Result<PagedResult<VisitorDto>>> GetListAsync(VisitorQueryDto query)
    {
        _appLog.Information("开始查询访客列表，参数: pageIndex={PageIndex}, pageSize={PageSize}, keyword='{Keyword}'",
            query.PageIndex, query.PageSize, query.Keywords ?? string.Empty);

        try
        {
            // 构建查询条件
            var whereExpression = QueryExpression(query);
            
            // 构建排序表达式
            System.Linq.Expressions.Expression<Func<Visitor, object>>? orderByExpression = null;
            SqlSugar.OrderByType orderByType = SqlSugar.OrderByType.Desc;
            
            if (!string.IsNullOrEmpty(query.OrderBy))
            {
                switch (query.OrderBy.ToLower())
                {
                    case "companyname":
                        orderByExpression = v => v.CompanyName;
                        break;
                    case "starttime":
                        orderByExpression = v => v.StartTime;
                        break;
                    case "createdtime":
                        orderByExpression = v => v.CreatedTime;
                        break;
                    default:
                        orderByExpression = v => v.CreatedTime;
                        break;
                }
            }
            else
            {
                orderByExpression = v => v.CreatedTime; // 默认按创建时间倒序
            }
            
            if (!string.IsNullOrEmpty(query.OrderDirection) && query.OrderDirection.ToLower() == "asc")
            {
                orderByType = SqlSugar.OrderByType.Asc;
            }
            
            // 使用真实的数据库查询
            var result = await _visitorRepository.GetListAsync(whereExpression, query.PageIndex, query.PageSize, orderByExpression, orderByType);
            var visitorDtos = result.Items.Adapt<List<VisitorDto>>();

            var pagedResult = new PagedResult<VisitorDto>
            {
                Items = visitorDtos,
                TotalNum = result.TotalNum,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            return Result<PagedResult<VisitorDto>>.Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "高级查询访客数据失败");
            return Result<PagedResult<VisitorDto>>.Fail($"高级查询访客数据失败: {ex.Message}");
        }
    }

    public async Task<Result<VisitorDto>> GetByIdAsync(long id)
    {
        try
        {
            var visitor = await _visitorRepository.GetByIdAsync(id);
            if (visitor == null)
                return Result<VisitorDto>.Fail("访客不存在");

            var visitorDto = visitor.Adapt<VisitorDto>();
            return Result<VisitorDto>.Ok(visitorDto);
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "获取访客失败");
            return Result<VisitorDto>.Fail($"获取访客失败: {ex.Message}");
        }
    }

    public async Task<Result<long>> CreateAsync(VisitorCreateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // 检查同一公司同一时段是否已存在
            var exists = await _visitorRepository.GetFirstAsync(
                v => v.CompanyName == dto.CompanyName && 
                     v.StartTime == dto.StartTime && 
                     v.IsDeleted == 0);
            if (exists != null)
                return Result<long>.Fail($"公司 {dto.CompanyName} 在 {dto.StartTime:yyyy-MM-dd HH:mm:ss} 时段已存在访客记录");

            var visitor = dto.Adapt<Visitor>();
            var result = await _visitorRepository.CreateAsync(visitor);
            Result<long> response = result > 0 
                ? Result<long>.Ok(visitor.Id) 
                : Result<long>.Fail("创建访客失败");
            
            _operLog?.LogCreate("Visitor", visitor.Id.ToString(), "Logistics.Visitors.VisitorView", 
                dto, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "创建访客失败");
            return Result<long>.Fail($"创建访客失败: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(VisitorUpdateDto dto)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var visitor = await _visitorRepository.GetByIdAsync(dto.Id);
            if (visitor == null)
                return Result.Fail("访客不存在");

            // 保存旧值用于记录变更（完整对象）
            var oldVisitor = visitor.Adapt<VisitorUpdateDto>();

            // 检查同一公司同一时段是否被其他记录使用
            if (visitor.CompanyName != dto.CompanyName || visitor.StartTime != dto.StartTime)
            {
                var exists = await _visitorRepository.GetFirstAsync(
                    v => v.CompanyName == dto.CompanyName && 
                         v.StartTime == dto.StartTime && 
                         v.Id != dto.Id && 
                         v.IsDeleted == 0);
                if (exists != null)
                    return Result.Fail($"公司 {dto.CompanyName} 在 {dto.StartTime:yyyy-MM-dd HH:mm:ss} 时段已被其他访客记录使用");
            }

            dto.Adapt(visitor);
            var result = await _visitorRepository.UpdateAsync(visitor);

            // 构建完整的变更信息，包含所有变更的字段
            var changeList = new List<string>();
            if (oldVisitor.CompanyName != dto.CompanyName) changeList.Add($"CompanyName: {oldVisitor.CompanyName} -> {dto.CompanyName}");
            if (oldVisitor.StartTime != dto.StartTime) changeList.Add($"StartTime: {oldVisitor.StartTime} -> {dto.StartTime}");
            if (oldVisitor.EndTime != dto.EndTime) changeList.Add($"EndTime: {oldVisitor.EndTime} -> {dto.EndTime}");

            var changes = changeList.Count > 0 ? string.Join(", ", changeList) : "无变更";
            Result response = result > 0 ? Result.Ok() : Result.Fail("更新访客失败");
            
            _operLog?.LogUpdate("Visitor", dto.Id.ToString(), "Logistics.Visitors.VisitorView", changes, dto, oldVisitor, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "更新访客失败");
            return Result.Fail($"更新访客失败: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(long id)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var visitor = await _visitorRepository.GetByIdAsync(id);
            if (visitor == null)
                return Result.Fail("访客不存在");

            // 删除关联的访客详情
            var details = await _visitorDetailRepository.GetListAsync(
                vd => vd.VisitorId == id && vd.IsDeleted == 0,
                1,
                int.MaxValue
            );

            foreach (var detail in details.Items)
            {
                await _visitorDetailRepository.DeleteAsync(detail);
            }

            // 删除访客
            var result = await _visitorRepository.DeleteAsync(visitor);
            Result response = result > 0 ? Result.Ok() : Result.Fail("删除访客失败");
            
            _operLog?.LogDelete("Visitor", id.ToString(), "Logistics.Visitors.VisitorView", 
                new { VisitorId = id }, response, stopwatch);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "删除访客失败");
            return Result.Fail($"删除访客失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 批量删除访客（同时删除关联的访客详情）
    /// </summary>
    /// <param name="ids">访客ID列表</param>
    /// <returns>操作结果</returns>
    public async Task<Result> DeleteBatchAsync(List<long> ids)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            if (ids == null || !ids.Any())
                return Result.Fail("ID列表为空");

            int successCount = 0;
            int failCount = 0;

            foreach (var id in ids)
            {
                var visitor = await _visitorRepository.GetByIdAsync(id);
                if (visitor == null)
                {
                    failCount++;
                    continue;
                }

                // 删除关联的访客详情
                var details = await _visitorDetailRepository.GetListAsync(
                    vd => vd.VisitorId == id && vd.IsDeleted == 0,
                    1,
                    int.MaxValue
                );

                foreach (var detail in details.Items)
                {
                    await _visitorDetailRepository.DeleteAsync(detail);
                }

                var result = await _visitorRepository.DeleteAsync(visitor);
                if (result > 0)
                {
                    successCount++;
                    _operLog?.LogDelete("Visitor", id.ToString(), "Logistics.Visitors.VisitorView", 
                        new { VisitorId = id }, Result.Ok(), stopwatch);
                }
                else
                {
                    failCount++;
                }
            }

            var response = Result.Ok($"删除完成：成功 {successCount} 个，失败 {failCount} 个");
            _appLog.Information("批量删除访客完成：成功 {SuccessCount} 个，失败 {FailCount} 个", successCount, failCount);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "批量删除访客失败");
            return Result.Fail($"批量删除访客失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出访客到Excel（支持条件查询导出）
    /// </summary>
    /// <param name="query">查询条件对象，可选，用于筛选要导出的访客</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> ExportAsync(VisitorQueryDto? query = null, string? sheetName = null, string? fileName = null)
    {
        try
        {
            var where = query != null ? QueryExpression(query) : SqlSugar.Expressionable.Create<Visitor>().And(x => x.IsDeleted == 0).ToExpression();
            var visitors = await _visitorRepository.AsQueryable().Where(where).OrderBy(v => v.CreatedTime, SqlSugar.OrderByType.Desc).ToListAsync();
            var dtos = visitors.Adapt<List<VisitorDto>>();
            sheetName ??= "Visitors";
            fileName ??= $"访客导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = ExcelHelper.ExportToExcel(dtos, sheetName);
            return Result<(string fileName, byte[] content)>.Ok((fileName, bytes), $"共导出 {dtos.Count} 条记录");
        }
        catch (Exception ex)
        {
            _appLog.Error(ex, "导出访客Excel失败");
            return Result<(string fileName, byte[] content)>.Fail($"导出失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 导出访客 Excel 模板
    /// </summary>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <param name="fileName">文件名，可选</param>
    /// <returns>包含文件名和文件内容的元组</returns>
    public async Task<Result<(string fileName, byte[] content)>> GetTemplateAsync(string? sheetName = null, string? fileName = null)
    {
        sheetName ??= "Visitors";
        fileName ??= $"访客导入模板_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        var bytes = ExcelHelper.ExportTemplate<VisitorDto>(sheetName);
        return await Task.FromResult(Result<(string fileName, byte[] content)>.Ok((fileName, bytes), "模板导出成功"));
    }

    /// <summary>
    /// 从 Excel 导入访客
    /// </summary>
    /// <param name="fileStream">Excel文件流</param>
    /// <param name="sheetName">工作表名称，可选</param>
    /// <returns>包含成功和失败数量的元组</returns>
    public async Task<Result<(int success, int fail)>> ImportAsync(Stream fileStream, string? sheetName = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            sheetName ??= "Visitors";
            var dtos = ExcelHelper.ImportFromExcel<VisitorDto>(fileStream, sheetName);
            if (dtos == null || !dtos.Any()) 
                return Result<(int success, int fail)>.Fail("Excel内容为空");

            int success = 0;
            int fail = 0;

            foreach (var dto in dtos)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(dto.CompanyName)) { fail++; continue; }
                    
                    // 检查同一公司同一时段是否已存在
                    var existing = await _visitorRepository.GetFirstAsync(
                        v => v.CompanyName == dto.CompanyName && 
                             v.StartTime == dto.StartTime && 
                             v.IsDeleted == 0);
                    if (existing == null)
                    {
                        var entity = dto.Adapt<Visitor>();
                        await _visitorRepository.CreateAsync(entity);
                        success++;
                    }
                    else
                    {
                        dto.Adapt(existing);
                        await _visitorRepository.UpdateAsync(existing);
                        success++;
                    }
                }
                catch
                {
                    fail++;
                }
            }

            var importResponse = Result<(int success, int fail)>.Ok((success, fail), $"导入完成：成功 {success} 条，失败 {fail} 条");
            
            _operLog?.LogImport("Visitor", success, "Logistics.Visitors.VisitorView", 
                new { Total = dtos.Count, Success = success, Fail = fail, Items = dtos }, importResponse, stopwatch);

            return importResponse;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _appLog.Error(ex, "导入访客Excel失败");
            return Result<(int success, int fail)>.Fail($"导入失败：{ex.Message}");
        }
    }

    private Expression<Func<Visitor, bool>> QueryExpression(VisitorQueryDto query)
    {
        return SqlSugar.Expressionable.Create<Visitor>()
            .And(v => v.IsDeleted == 0)
            .AndIF(!string.IsNullOrEmpty(query.Keywords), v => v.CompanyName.Contains(query.Keywords!))
            .AndIF(!string.IsNullOrEmpty(query.CompanyName), v => v.CompanyName.Contains(query.CompanyName!))
            .AndIF(query.StartTimeFrom.HasValue, v => v.StartTime >= query.StartTimeFrom!.Value)
            .AndIF(query.StartTimeTo.HasValue, v => v.StartTime <= query.StartTimeTo!.Value)
            .ToExpression();
    }
}

