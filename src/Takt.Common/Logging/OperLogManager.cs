// ========================================
// 项目名称：Takt.Wpf
// 文件名称：OperLogManager.cs
// 创建时间：2025-10-20
// 创建人：Hbt365(Cursor AI)
// 功能描述：操作日志管理器
// ========================================

using System.Diagnostics;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using Takt.Common.Context;
using Takt.Common.Results;

namespace Takt.Common.Logging;

/// <summary>
/// 操作日志管理器
/// 专门用于记录用户操作日志（登录、创建、更新、删除等）
/// 同时记录到文件（Serilog）和数据库（通过 ILogDatabaseWriter 接口）
/// </summary>
public class OperLogManager : ILogManager
{
    private readonly ILogger _operLogger;
    private readonly ILogDatabaseWriter? _logDatabaseWriter;
    private static ILogDatabaseWriter? _staticLogDatabaseWriter;

    public OperLogManager(ILogger logger, ILogDatabaseWriter? logDatabaseWriter = null)
    {
        // 使用符合 Windows 规范的日志目录（AppData\Local）
        var logsDir = Takt.Common.Helpers.PathHelper.GetLogDirectory();

        // 创建独立的操作日志器
        // 设置最小日志级别为 Debug，确保所有调试日志都能被记录
        _operLogger = new LoggerConfiguration()
            .MinimumLevel.Debug()  // 设置最小日志级别为 Debug
            .WriteTo.File(
                path: Path.Combine(logsDir, "oper-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                fileSizeLimitBytes: 8 * 1024 * 1024,  // 单个文件最大 8MB
                rollOnFileSizeLimit: true,  // 达到文件大小限制时自动滚动
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                encoding: System.Text.Encoding.UTF8,
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug)  // 文件输出也接受 Debug 级别
            .CreateLogger();

        _logDatabaseWriter = logDatabaseWriter;
        
        // 如果提供了 logDatabaseWriter，同时设置静态引用（用于延迟解析）
        if (logDatabaseWriter != null)
        {
            _staticLogDatabaseWriter = logDatabaseWriter;
            System.Diagnostics.Debug.WriteLine($"🟢 [OperLogManager] 构造函数: logDatabaseWriter 不为 null，已设置到静态引用: {logDatabaseWriter.GetType().Name}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ [OperLogManager] 构造函数: logDatabaseWriter 为 null，将依赖后续的 SetLogDatabaseWriter 调用");
        }
    }

    /// <summary>
    /// 设置静态日志数据库写入器（由依赖注入容器调用）
    /// </summary>
    public static void SetLogDatabaseWriter(ILogDatabaseWriter logDatabaseWriter)
    {
        _staticLogDatabaseWriter = logDatabaseWriter ?? throw new ArgumentNullException(nameof(logDatabaseWriter));
        
        // 记录诊断信息（使用 System.Diagnostics.Debug 避免依赖 Serilog）
        System.Diagnostics.Debug.WriteLine($"🟢 [OperLogManager] SetLogDatabaseWriter 已调用，_staticLogDatabaseWriter 已设置: {logDatabaseWriter.GetType().Name}");
        
        // 同时使用 Serilog 记录（如果可能，通过静态日志器）
        try
        {
            // 尝试获取全局日志器（如果可用）
            if (Log.Logger != null)
            {
                Log.Information("[OperLogManager] SetLogDatabaseWriter 已调用，_staticLogDatabaseWriter 已设置: {WriterType}", logDatabaseWriter.GetType().Name);
            }
        }
        catch
        {
            // 忽略，可能 Serilog 还未初始化
        }
    }

    /// <summary>
    /// 获取日志数据库写入器（优先使用实例字段，如果为 null 则使用静态引用）
    /// </summary>
    private ILogDatabaseWriter? GetLogDatabaseWriter()
    {
        // 优先使用注入的实例
        if (_logDatabaseWriter != null)
        {
            _operLogger.Debug("[OperLogManager] 使用实例注入的 ILogDatabaseWriter");
            return _logDatabaseWriter;
        }
        
        // 如果实例为 null，尝试使用静态引用（用于延迟解析场景）
        if (_staticLogDatabaseWriter != null)
        {
            _operLogger.Debug("[OperLogManager] 使用静态引用的 ILogDatabaseWriter");
            return _staticLogDatabaseWriter;
        }
        
        // 如果静态引用也为 null，直接返回 null
        // 注意：不能在这里尝试从服务容器获取，因为 Takt.Common 不应该依赖 Takt.Fluent
        // 静态引用会在 AutofacModule 的 OnActivated 回调中设置
        _operLogger.Warning("[OperLogManager] ⚠️ ILogDatabaseWriter 不可用: _logDatabaseWriter={IsNull1}, _staticLogDatabaseWriter={IsNull2}", 
            _logDatabaseWriter == null ? "null" : "not null", 
            _staticLogDatabaseWriter == null ? "null" : "not null");
        return null;
    }

    /// <summary>
    /// 记录操作信息
    /// </summary>
    public void Information(string message, params object[] args)
    {
        _operLogger.Information("[操作] " + message, args);
    }

    /// <summary>
    /// 记录操作警告
    /// </summary>
    public void Warning(string message, params object[] args)
    {
        _operLogger.Warning("[操作] " + message, args);
    }

    /// <summary>
    /// 记录操作错误
    /// </summary>
    public void Error(string message, params object[] args)
    {
        _operLogger.Error("[操作] " + message, args);
    }

    /// <summary>
    /// 记录操作错误（带异常）
    /// </summary>
    public void Error(Exception exception, string message, params object[] args)
    {
        // 记录到文件
        _operLogger.Error(exception, "[操作] " + message, args);
    }

    /// <summary>
    /// 获取当前IP地址
    /// </summary>
    private string GetCurrentIpAddress()
    {
        try
        {
            var ip = Takt.Common.Helpers.SystemInfoHelper.GetLocalIpAddress();
            return string.IsNullOrWhiteSpace(ip) ? "127.0.0.1" : ip;
        }
        catch
        {
            return "127.0.0.1";
        }
    }

    /// <summary>
    /// 获取操作系统信息
    /// </summary>
    private string GetOsInfo()
    {
        try
        {
            var osType = Takt.Common.Helpers.SystemInfoHelper.GetOsType();
            var osVersion = Takt.Common.Helpers.SystemInfoHelper.GetOsVersion();
            var osInfo = $"{osType} {osVersion}";
            return string.IsNullOrWhiteSpace(osInfo) ? "Unknown OS" : osInfo;
        }
        catch
        {
            return "Unknown OS";
        }
    }

    /// <summary>
    /// 获取客户端信息（用户代理）
    /// </summary>
    private string GetUserAgent()
    {
        try
        {
            var userAgent = Takt.Common.Helpers.SystemInfoHelper.GetClientName();
            return string.IsNullOrWhiteSpace(userAgent) ? "WPF Desktop App" : userAgent;
        }
        catch
        {
            return "WPF Desktop App";
        }
    }

    /// <summary>
    /// 获取客户端类型（浏览器字段）
    /// </summary>
    private string GetBrowser()
    {
        try
        {
            var browser = Takt.Common.Helpers.SystemInfoHelper.GetClientType();
            return string.IsNullOrWhiteSpace(browser) ? "Desktop" : browser;
        }
        catch
        {
            return "Desktop";
        }
    }

    /// <summary>
    /// 记录操作调试信息
    /// </summary>
    public void Debug(string message, params object[] args)
    {
        _operLogger.Debug("[操作] " + message, args);
    }

    /// <summary>
    /// 记录用户登录
    /// </summary>
    public void Login(string username, string realName, bool success, string ip = "")
    {
        if (success)
            _operLogger.Information("[操作-登录] 用户登录成功：{Username} ({RealName}) IP:{IP}", username, realName, ip);
        else
            _operLogger.Warning("[操作-登录] 用户登录失败：{Username} IP:{IP}", username, ip);
    }

    /// <summary>
    /// 记录用户登出
    /// </summary>
    public void Logout(string username, string realName)
    {
        _operLogger.Information("[操作-登出] 用户登出：{Username} ({RealName})", username, realName);
    }

    /// <summary>
    /// 记录创建操作
    /// </summary>
    /// <param name="entityName">实体名称</param>
    /// <param name="entityId">实体ID</param>
    /// <param name="operatorName">操作人</param>
    /// <param name="requestPath">请求路径（视图路径，如 "Identity.UserView"）</param>
    /// <param name="requestParams">请求参数（JSON格式）</param>
    /// <param name="responseResult">响应结果（JSON格式）</param>
    /// <param name="elapsedTime">执行耗时（毫秒）</param>
    public void Create(string entityName, string entityId, string operatorName, string? requestPath = null, string? requestParams = null, string? responseResult = null, int elapsedTime = 0)
    {
        // 记录到文件
        _operLogger.Information("[操作-创建] {Operator} 创建了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);

        // 保存到数据库（异步，不阻塞）
        // 使用完整的 JSON 序列化选项，确保所有信息都被记录
        var jsonOptions = new JsonSerializerSettings 
        { 
            Formatting = Formatting.None,  // 不缩进，节省空间但保持完整
            StringEscapeHandling = StringEscapeHandling.Default  // 允许更多字符
        };
        var requestParamsJson = requestParams ?? JsonConvert.SerializeObject(new { EntityId = entityId }, jsonOptions);
        _ = SaveOperLogAsync(
            operationType: "Create", 
            operationModule: entityName, 
            operatorName: operatorName, 
            operationDesc: $"创建了 {entityName} (ID:{entityId})", 
            operationResult: "Success",
            requestPath: requestPath, 
            requestParams: requestParamsJson, 
            responseResult: responseResult, 
            elapsedTime: elapsedTime);
    }

    /// <summary>
    /// 记录更新操作
    /// </summary>
    /// <param name="entityName">实体名称</param>
    /// <param name="entityId">实体ID</param>
    /// <param name="operatorName">操作人</param>
    /// <param name="changes">变更内容</param>
    /// <param name="requestPath">请求路径（视图路径，如 "Identity.UserView"）</param>
    /// <param name="requestParams">请求参数（JSON格式）</param>
    /// <param name="responseResult">响应结果（JSON格式）</param>
    /// <param name="elapsedTime">执行耗时（毫秒）</param>
    public void Update(string entityName, string entityId, string operatorName, string changes = "", string? requestPath = null, string? requestParams = null, string? responseResult = null, int elapsedTime = 0)
    {
        // 记录到文件
        _operLogger.Information("[操作-更新] {Operator} 更新了 {EntityName} (ID:{EntityId}) 变更:{Changes}", 
            operatorName, entityName, entityId, changes);

        // 保存到数据库（异步，不阻塞）
        // 完整记录变更信息，不简化
        var desc = string.IsNullOrEmpty(changes) 
            ? $"更新了 {entityName} (ID:{entityId})" 
            : $"更新了 {entityName} (ID:{entityId}) 变更:{changes}";
        // 使用完整的 JSON 序列化选项，确保所有变更信息都被记录
        var jsonSettings = new JsonSerializerSettings 
        { 
            Formatting = Formatting.None,  // 不缩进，节省空间但保持完整
            StringEscapeHandling = StringEscapeHandling.Default  // 使用默认转义
        };
        var requestParamsJson = requestParams ?? JsonConvert.SerializeObject(new { EntityId = entityId, Changes = changes }, jsonSettings);
        _ = SaveOperLogAsync(
            operationType: "Update", 
            operationModule: entityName, 
            operatorName: operatorName, 
            operationDesc: desc, 
            operationResult: "Success",
            requestPath: requestPath, 
            requestParams: requestParamsJson, 
            responseResult: responseResult, 
            elapsedTime: elapsedTime);
    }

    /// <summary>
    /// 记录删除操作
    /// </summary>
    /// <param name="entityName">实体名称</param>
    /// <param name="entityId">实体ID</param>
    /// <param name="operatorName">操作人</param>
    /// <param name="requestPath">请求路径（视图路径，如 "Identity.UserView"）</param>
    /// <param name="requestParams">请求参数（JSON格式）</param>
    /// <param name="responseResult">响应结果（JSON格式）</param>
    /// <param name="elapsedTime">执行耗时（毫秒）</param>
    public void Delete(string entityName, string entityId, string operatorName, string? requestPath = null, string? requestParams = null, string? responseResult = null, int elapsedTime = 0)
    {
        // 记录到文件
        _operLogger.Information("[操作-删除] {Operator} 删除了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);

        // 保存到数据库（异步，不阻塞）
        // 使用完整的 JSON 序列化选项，确保所有信息都被记录
        var jsonOptions = new JsonSerializerSettings 
        { 
            Formatting = Formatting.None,  // 不缩进，节省空间但保持完整
            StringEscapeHandling = StringEscapeHandling.Default  // 允许更多字符
        };
        var requestParamsJson = requestParams ?? JsonConvert.SerializeObject(new { EntityId = entityId }, jsonOptions);
        _ = SaveOperLogAsync(
            operationType: "Delete", 
            operationModule: entityName, 
            operatorName: operatorName, 
            operationDesc: $"删除了 {entityName} (ID:{entityId})", 
            operationResult: "Success",
            requestPath: requestPath, 
            requestParams: requestParamsJson, 
            responseResult: responseResult, 
            elapsedTime: elapsedTime);
    }

    /// <summary>
    /// 异步保存操作日志到数据库
    /// </summary>
    private async Task SaveOperLogAsync(
        string operationType, 
        string operationModule, 
        string operatorName, 
        string operationDesc,
        string operationResult = "Success",
        string? requestPath = null,
        string? requestParams = null,
        string? responseResult = null,
        int elapsedTime = 0)
    {
        // 使用延迟解析机制获取 LogDatabaseWriter
        _operLogger.Debug("[OperLogManager] SaveOperLogAsync 开始: 操作类型={OperationType}, 操作模块={OperationModule}, 操作人={OperatorName}", 
            operationType, operationModule, operatorName);
        
        var logDatabaseWriter = GetLogDatabaseWriter();
        if (logDatabaseWriter == null)
        {
            _operLogger.Warning("[OperLogManager] ⚠️ ILogDatabaseWriter 不可用，操作日志仅保存到文件，不保存到数据库: 操作类型={OperationType}, 操作模块={OperationModule}", 
                operationType, operationModule);
            return;
        }

        _operLogger.Debug("[OperLogManager] ILogDatabaseWriter 可用，开始保存操作日志到数据库: 操作类型={OperationType}, 操作模块={OperationModule}", 
            operationType, operationModule);

        try
        {
            // 确保必填字段有值
            if (string.IsNullOrWhiteSpace(operatorName))
            {
                operatorName = "Takt365";
            }

            if (string.IsNullOrWhiteSpace(operationType))
            {
                operationType = "Unknown";
            }

            if (string.IsNullOrWhiteSpace(operationModule))
            {
                operationModule = "Unknown";
            }

            if (string.IsNullOrWhiteSpace(operationResult))
            {
                operationResult = "Success";
            }

            // 完整记录操作描述，不截断（数据库字段支持 nvarchar(max)）
            var desc = string.IsNullOrWhiteSpace(operationDesc) ? string.Empty : operationDesc;

            // 获取系统信息（确保非 null）
            var ipAddress = GetCurrentIpAddress();
            var userAgent = GetUserAgent();
            var osInfo = GetOsInfo();
            var browser = GetBrowser();

            // 根据操作类型确定请求方法
            var requestMethod = operationType switch
            {
                "Create" => "POST",
                "Update" => "PUT",
                "Delete" => "DELETE",
                "Query" => "GET",
                "Export" => "GET",
                "Import" => "POST",
                _ => operationType // 其他情况使用操作类型本身
            };

            _operLogger.Debug("[OperLogManager] 准备调用 logDatabaseWriter.SaveOperLogAsync: 操作类型={OperationType}, 操作模块={OperationModule}, 操作人={OperatorName}", 
                operationType, operationModule, operatorName);

            await logDatabaseWriter.SaveOperLogAsync(
                operatorName,
                operationType,
                operationModule,
                desc,
                operationResult,
                ipAddress,
                requestPath,
                requestMethod,
                requestParams,
                responseResult,
                elapsedTime,
                userAgent,
                osInfo,
                browser
            );

            _operLogger.Debug("[OperLogManager] ✅ 操作日志保存成功: 操作类型={OperationType}, 操作模块={OperationModule}, 操作人={OperatorName}", 
                operationType, operationModule, operatorName);
        }
        catch (Exception ex)
        {
            // 如果保存到数据库失败，只记录到文件，不抛出异常
            _operLogger.Error(ex, "[OperLogManager] ❌ 保存操作日志到数据库失败: 操作类型={OperationType}, 操作模块={OperationModule}, 操作人={OperatorName}, 错误={ErrorMessage}", 
                operationType, operationModule, operatorName, ex.Message);
        }
    }

    /// <summary>
    /// 记录查询操作
    /// </summary>
    public void Query(string entityName, string operatorName, string condition = "")
    {
        _operLogger.Information("[操作-查询] {Operator} 查询了 {EntityName} 条件:{Condition}", 
            operatorName, entityName, condition);
    }

    /// <summary>
    /// 记录导出操作
    /// </summary>
    public void Export(string entityName, int count, string operatorName)
    {
        _operLogger.Information("[操作-导出] {Operator} 导出了 {Count} 条 {EntityName} 数据", 
            operatorName, count, entityName);
    }

    /// <summary>
    /// 记录导入操作
    /// </summary>
    /// <param name="entityName">实体名称</param>
    /// <param name="count">导入数量</param>
    /// <param name="operatorName">操作人</param>
    /// <param name="requestPath">请求路径（视图路径，如 "Identity.UserView"）</param>
    /// <param name="requestParams">请求参数（JSON格式）</param>
    /// <param name="responseResult">响应结果（JSON格式）</param>
    /// <param name="elapsedTime">执行耗时（毫秒）</param>
    public void Import(string entityName, int count, string operatorName, string? requestPath = null, string? requestParams = null, string? responseResult = null, int elapsedTime = 0)
    {
        // 记录到文件
        _operLogger.Information("[操作-导入] {Operator} 导入了 {Count} 条 {EntityName} 数据", 
            operatorName, count, entityName);

        // 保存到数据库（异步，不阻塞）
        // 使用完整的 JSON 序列化选项，确保所有信息都被记录
        var jsonSettings = new JsonSerializerSettings 
        { 
            Formatting = Formatting.None,  // 不缩进，节省空间但保持完整
            StringEscapeHandling = StringEscapeHandling.Default  // 使用默认转义
        };
        var requestParamsJson = requestParams ?? JsonConvert.SerializeObject(new { Count = count }, jsonSettings);
        _ = SaveOperLogAsync(
            operationType: "Import", 
            operationModule: entityName, 
            operatorName: operatorName, 
            operationDesc: $"导入了 {count} 条 {entityName} 数据", 
            operationResult: "Success",
            requestPath: requestPath, 
            requestParams: requestParamsJson, 
            responseResult: responseResult, 
            elapsedTime: elapsedTime);
    }

    /// <summary>
    /// 记录批量创建操作
    /// </summary>
    public void BatchCreate(string entityName, int count, string operatorName)
    {
        _operLogger.Information("[操作-批量创建] {Operator} 批量创建了 {Count} 条 {EntityName} 数据", 
            operatorName, count, entityName);
    }

    /// <summary>
    /// 记录批量更新操作
    /// </summary>
    public void BatchUpdate(string entityName, int count, string operatorName, string condition = "")
    {
        _operLogger.Information("[操作-批量更新] {Operator} 批量更新了 {Count} 条 {EntityName} 数据 条件:{Condition}", 
            operatorName, count, entityName, condition);
    }

    /// <summary>
    /// 记录批量删除操作
    /// </summary>
    public void BatchDelete(string entityName, int count, string operatorName, string condition = "")
    {
        _operLogger.Information("[操作-批量删除] {Operator} 批量删除了 {Count} 条 {EntityName} 数据 条件:{Condition}", 
            operatorName, count, entityName, condition);
    }

    /// <summary>
    /// 记录复制操作
    /// </summary>
    public void Copy(string entityName, string sourceId, string targetId, string operatorName)
    {
        _operLogger.Information("[操作-复制] {Operator} 复制了 {EntityName} 从 {SourceId} 到 {TargetId}", 
            operatorName, entityName, sourceId, targetId);
    }

    /// <summary>
    /// 记录移动操作
    /// </summary>
    public void Move(string entityName, string sourceId, string targetId, string operatorName)
    {
        _operLogger.Information("[操作-移动] {Operator} 移动了 {EntityName} 从 {SourceId} 到 {TargetId}", 
            operatorName, entityName, sourceId, targetId);
    }

    /// <summary>
    /// 记录打印操作
    /// </summary>
    public void Print(string entityName, string entityId, string operatorName, string printType = "")
    {
        _operLogger.Information("[操作-打印] {Operator} 打印了 {EntityName} (ID:{EntityId}) 类型:{PrintType}", 
            operatorName, entityName, entityId, printType);
    }

    /// <summary>
    /// 记录预览操作
    /// </summary>
    public void Preview(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-预览] {Operator} 预览了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录详情查看操作
    /// </summary>
    public void Detail(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-详情] {Operator} 查看了 {EntityName} (ID:{EntityId}) 详情", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录锁定操作
    /// </summary>
    public void Lock(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-锁定] {Operator} 锁定了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录解锁操作
    /// </summary>
    public void Unlock(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-解锁] {Operator} 解锁了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录启用操作
    /// </summary>
    public void Enable(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-启用] {Operator} 启用了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录禁用操作
    /// </summary>
    public void Disable(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-禁用] {Operator} 禁用了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录提交操作
    /// </summary>
    public void Submit(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-提交] {Operator} 提交了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录撤回操作
    /// </summary>
    public void Recall(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-撤回] {Operator} 撤回了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录审核操作
    /// </summary>
    public void Approve(string entityName, string entityId, string operatorName, bool approved, string reason = "")
    {
        var status = approved ? "通过" : "拒绝";
        _operLogger.Information("[操作-审核] {Operator} {Status}了 {EntityName} (ID:{EntityId}) 原因:{Reason}", 
            operatorName, status, entityName, entityId, reason);
    }

    /// <summary>
    /// 记录发送操作
    /// </summary>
    public void Send(string entityName, string entityId, string operatorName, string recipient = "")
    {
        _operLogger.Information("[操作-发送] {Operator} 发送了 {EntityName} (ID:{EntityId}) 接收方:{Recipient}", 
            operatorName, entityName, entityId, recipient);
    }

    /// <summary>
    /// 记录启动操作
    /// </summary>
    public void Start(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-启动] {Operator} 启动了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录停止操作
    /// </summary>
    public void Stop(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-停止] {Operator} 停止了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录重启操作
    /// </summary>
    public void Restart(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-重启] {Operator} 重启了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录收藏操作
    /// </summary>
    public void Favorite(string entityName, string entityId, string operatorName, bool favorited)
    {
        var action = favorited ? "收藏" : "取消收藏";
        _operLogger.Information("[操作-{Action}] {Operator} {Action}了 {EntityName} (ID:{EntityId})", 
            action, operatorName, action, entityName, entityId);
    }

    /// <summary>
    /// 记录点赞操作
    /// </summary>
    public void Like(string entityName, string entityId, string operatorName, bool liked)
    {
        var action = liked ? "点赞" : "取消点赞";
        _operLogger.Information("[操作-{Action}] {Operator} {Action}了 {EntityName} (ID:{EntityId})", 
            action, operatorName, action, entityName, entityId);
    }

    /// <summary>
    /// 记录评论操作
    /// </summary>
    public void Comment(string entityName, string entityId, string operatorName, string comment = "")
    {
        _operLogger.Information("[操作-评论] {Operator} 评论了 {EntityName} (ID:{EntityId}) 内容:{Comment}", 
            operatorName, entityName, entityId, comment);
    }

    /// <summary>
    /// 记录分享操作
    /// </summary>
    public void Share(string entityName, string entityId, string operatorName, string platform = "")
    {
        _operLogger.Information("[操作-分享] {Operator} 分享了 {EntityName} (ID:{EntityId}) 平台:{Platform}", 
            operatorName, entityName, entityId, platform);
    }

    /// <summary>
    /// 记录订阅操作
    /// </summary>
    public void Subscribe(string entityName, string entityId, string operatorName, bool subscribed)
    {
        var action = subscribed ? "订阅" : "取消订阅";
        _operLogger.Information("[操作-{Action}] {Operator} {Action}了 {EntityName} (ID:{EntityId})", 
            action, operatorName, action, entityName, entityId);
    }

    /// <summary>
    /// 记录刷新操作
    /// </summary>
    public void Refresh(string entityName, string operatorName, string condition = "")
    {
        _operLogger.Information("[操作-刷新] {Operator} 刷新了 {EntityName} 条件:{Condition}", 
            operatorName, entityName, condition);
    }

    /// <summary>
    /// 记录归档操作
    /// </summary>
    public void Archive(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-归档] {Operator} 归档了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录恢复操作
    /// </summary>
    public void Restore(string entityName, string entityId, string operatorName)
    {
        _operLogger.Information("[操作-恢复] {Operator} 恢复了 {EntityName} (ID:{EntityId})", 
            operatorName, entityName, entityId);
    }

    /// <summary>
    /// 记录通知操作
    /// </summary>
    public void Notify(string entityName, string entityId, string operatorName, string message = "")
    {
        _operLogger.Information("[操作-通知] {Operator} 通知了 {EntityName} (ID:{EntityId}) 消息:{Message}", 
            operatorName, entityName, entityId, message);
    }

    /// <summary>
    /// 记录附件操作
    /// </summary>
    public void Attach(string entityName, string entityId, string operatorName, string fileName)
    {
        _operLogger.Information("[操作-附件] {Operator} 为 {EntityName} (ID:{EntityId}) 添加了附件 {FileName}", 
            operatorName, entityName, entityId, fileName);
    }

    #region 便捷方法 - 统一处理日志记录的通用逻辑

    /// <summary>
    /// 记录创建操作（便捷方法，自动处理序列化和耗时计算）
    /// </summary>
    /// <typeparam name="TResponse">响应类型</typeparam>
    /// <param name="entityName">实体名称</param>
    /// <param name="entityId">实体ID</param>
    /// <param name="requestPath">请求路径（视图路径，如 "Identity.UserView"）</param>
    /// <param name="requestParams">请求参数（可以是 DTO 对象或已序列化的 JSON 字符串）</param>
    /// <param name="response">响应结果</param>
    /// <param name="stopwatch">用于计算执行耗时的计时器（必须已启动）</param>
    public void LogCreate<TResponse>(
        string entityName,
        string entityId,
        string requestPath,
        object? requestParams = null,
        TResponse? response = default,
        Stopwatch? stopwatch = null)
    {
        stopwatch?.Stop();
        var operatorName = UserContext.Current.IsAuthenticated ? UserContext.Current.Username : "Takt365";
        var requestParamsJson = SerializeRequestParams(requestParams, new { EntityId = entityId });
        var responseResult = SerializeResponse(response);
        var elapsedTime = CalculateElapsedTime(stopwatch);

        Create(entityName, entityId, operatorName, requestPath, requestParamsJson, responseResult, elapsedTime);
    }

    /// <summary>
    /// 记录更新操作（便捷方法，自动处理序列化和耗时计算）
    /// </summary>
    /// <typeparam name="TResponse">响应类型</typeparam>
    /// <param name="entityName">实体名称</param>
    /// <param name="entityId">实体ID</param>
    /// <param name="requestPath">请求路径（视图路径，如 "Identity.UserView"）</param>
    /// <param name="changes">变更内容（字段变更说明，如 "Field1: oldValue -> newValue"）</param>
    /// <param name="requestParams">请求参数（新值，可以是 DTO 对象或已序列化的 JSON 字符串）</param>
    /// <param name="oldValue">旧值对象（修改前的完整对象，用于审计）</param>
    /// <param name="response">响应结果</param>
    /// <param name="stopwatch">用于计算执行耗时的计时器（必须已启动）</param>
    public void LogUpdate<TResponse>(
        string entityName,
        string entityId,
        string requestPath,
        string changes = "",
        object? requestParams = null,
        object? oldValue = null,
        TResponse? response = default,
        Stopwatch? stopwatch = null)
    {
        stopwatch?.Stop();
        var operatorName = UserContext.Current.IsAuthenticated ? UserContext.Current.Username : "Takt365";
        
        // 构建包含完整修改前后信息的请求参数
        var requestParamsObj = new
        {
            EntityId = entityId,
            Changes = changes,
            OldValue = oldValue,  // 修改前的完整对象
            NewValue = requestParams  // 修改后的完整对象（新值）
        };
        var requestParamsJson = SerializeRequestParams(requestParamsObj, new { EntityId = entityId, Changes = changes });
        var responseResult = SerializeResponse(response);
        var elapsedTime = CalculateElapsedTime(stopwatch);

        Update(entityName, entityId, operatorName, changes, requestPath, requestParamsJson, responseResult, elapsedTime);
    }

    /// <summary>
    /// 记录删除操作（便捷方法，自动处理序列化和耗时计算）
    /// </summary>
    /// <typeparam name="TResponse">响应类型</typeparam>
    /// <param name="entityName">实体名称</param>
    /// <param name="entityId">实体ID</param>
    /// <param name="requestPath">请求路径（视图路径，如 "Identity.UserView"）</param>
    /// <param name="requestParams">请求参数（可以是 DTO 对象或已序列化的 JSON 字符串）</param>
    /// <param name="response">响应结果</param>
    /// <param name="stopwatch">用于计算执行耗时的计时器（必须已启动）</param>
    public void LogDelete<TResponse>(
        string entityName,
        string entityId,
        string requestPath,
        object? requestParams = null,
        TResponse? response = default,
        Stopwatch? stopwatch = null)
    {
        stopwatch?.Stop();
        var operatorName = UserContext.Current.IsAuthenticated ? UserContext.Current.Username : "Takt365";
        var requestParamsJson = SerializeRequestParams(requestParams, new { EntityId = entityId });
        var responseResult = SerializeResponse(response);
        var elapsedTime = CalculateElapsedTime(stopwatch);

        Delete(entityName, entityId, operatorName, requestPath, requestParamsJson, responseResult, elapsedTime);
    }

    /// <summary>
    /// 记录导入操作（便捷方法，自动处理序列化和耗时计算）
    /// </summary>
    /// <typeparam name="TResponse">响应类型</typeparam>
    /// <param name="entityName">实体名称</param>
    /// <param name="successCount">成功数量</param>
    /// <param name="requestPath">请求路径（视图路径，如 "Identity.UserView"）</param>
    /// <param name="requestParams">请求参数（可以是对象或已序列化的 JSON 字符串）</param>
    /// <param name="response">响应结果</param>
    /// <param name="stopwatch">用于计算执行耗时的计时器（必须已启动）</param>
    public void LogImport<TResponse>(
        string entityName,
        int successCount,
        string requestPath,
        object? requestParams = null,
        TResponse? response = default,
        Stopwatch? stopwatch = null)
    {
        stopwatch?.Stop();
        var operatorName = UserContext.Current.IsAuthenticated ? UserContext.Current.Username : "Takt365";
        var requestParamsJson = SerializeRequestParams(requestParams, new { Success = successCount });
        var responseResult = SerializeResponse(response);
        var elapsedTime = CalculateElapsedTime(stopwatch);

        // 先调用 Import 方法（简单记录）
        Import(entityName, successCount, operatorName, requestPath, requestParamsJson, responseResult, elapsedTime);

        // 再调用 Update 方法记录详细信息（如果需要）
        if (response is Result<(int success, int fail)> importResult && importResult.Success)
        {
            var (success, fail) = importResult.Data;
            var desc = $"导入了 {success} 条 {entityName} 数据，失败 {fail} 条";
            Update(entityName, "Import", operatorName, desc, requestPath, requestParamsJson, responseResult, elapsedTime);
        }
    }

    /// <summary>
    /// 序列化请求参数（私有辅助方法）
    /// </summary>
    private static string? SerializeRequestParams(object? requestParams, object defaultParams)
    {
        var jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            StringEscapeHandling = StringEscapeHandling.Default
        };

        if (requestParams == null)
            return JsonConvert.SerializeObject(defaultParams, jsonSettings);

        // 如果已经是字符串，直接返回
        if (requestParams is string str)
            return str;

        // 否则序列化为 JSON
        return JsonConvert.SerializeObject(requestParams, jsonSettings);
    }

    /// <summary>
    /// 序列化响应结果（私有辅助方法）
    /// </summary>
    private static string? SerializeResponse<TResponse>(TResponse? response)
    {
        if (response == null)
            return null;

        var jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            StringEscapeHandling = StringEscapeHandling.Default
        };

        // 如果是 Result 类型，提取关键信息
        if (response is Result result)
        {
            var resultObj = new
            {
                Success = result.Success,
                Message = result.Message,
                Code = result.Code,
                Data = result.GetType().GetProperty("Data")?.GetValue(result)
            };
            return JsonConvert.SerializeObject(resultObj, jsonSettings);
        }

        // 否则直接序列化
        return JsonConvert.SerializeObject(response, jsonSettings);
    }

    /// <summary>
    /// 计算执行耗时（毫秒）（私有辅助方法）
    /// </summary>
    private static int CalculateElapsedTime(Stopwatch? stopwatch)
    {
        if (stopwatch == null || !stopwatch.IsRunning)
            return 0;

        var elapsedMs = stopwatch.ElapsedMilliseconds;
        return elapsedMs > int.MaxValue ? int.MaxValue : (int)elapsedMs;
    }

    #endregion
}

