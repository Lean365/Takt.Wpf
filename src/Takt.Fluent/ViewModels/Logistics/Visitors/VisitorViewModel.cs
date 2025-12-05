// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Logistics.Visitors
// 文件名称：VisitorViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：访客视图模型（主子表视图）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Takt.Application.Dtos.Logistics.Visitors;
using Takt.Application.Services.Logistics.Visitors;
using Takt.Common.Context;
using Takt.Common.Logging;
using Takt.Common.Results;
using Takt.Domain.Interfaces;
using QueryContext = Takt.Fluent.Controls.QueryContext;
using PageRequest = Takt.Fluent.Controls.PageRequest;

namespace Takt.Fluent.ViewModels.Logistics.Visitors;

/// <summary>
/// 访客视图模型（主子表视图）
/// 主表：Visitor（访客）
/// 子表：VisitorDetail（访客详情）
/// </summary>
public partial class VisitorViewModel : ObservableObject
{
    private readonly IVisitorService _visitorService;
    private readonly IVisitorDetailService _visitorDetailService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILocalizationManager _localizationManager;
    private readonly OperLogManager? _operLog;

    // 主表数据
    public ObservableCollection<VisitorDto> Visitors { get; } = new();

    [ObservableProperty]
    private VisitorDto? _selectedVisitor;

    // 子表数据
    public ObservableCollection<VisitorDetailDto> VisitorDetails { get; } = new();

    [ObservableProperty]
    private VisitorDetailDto? _selectedVisitorDetail;

    [ObservableProperty]
    private VisitorDetailDto? _editingVisitorDetail;

    // 主表查询相关
    [ObservableProperty]
    private string _visitorKeyword = string.Empty;

    [ObservableProperty]
    private int _visitorPageIndex = 1;

    [ObservableProperty]
    private int _visitorPageSize = 20;

    [ObservableProperty]
    private int _visitorTotalCount;

    // 子表查询相关（TaktInlineEditDataGrid 不支持分页，改为一次性加载所有数据）
    [ObservableProperty]
    private string _detailKeyword = string.Empty;

    [ObservableProperty]
    private bool _isLoadingVisitors;

    [ObservableProperty]
    private bool _isLoadingDetails;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    public VisitorViewModel(
        IVisitorService visitorService,
        IVisitorDetailService visitorDetailService,
        IServiceProvider serviceProvider,
        ILocalizationManager localizationManager,
        OperLogManager? operLog = null)
    {
        _visitorService = visitorService ?? throw new ArgumentNullException(nameof(visitorService));
        _visitorDetailService = visitorDetailService ?? throw new ArgumentNullException(nameof(visitorDetailService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _operLog = operLog;

        _ = LoadVisitorsAsync();
    }

    partial void OnSelectedVisitorChanged(VisitorDto? value)
    {
        // 主表选中项改变时，重置子表状态并加载子表数据
        if (value == null)
        {
            VisitorDetails.Clear();
            SelectedVisitorDetail = null;
            EditingVisitorDetail = null;
            DetailKeyword = string.Empty;
        }
        else
        {
            // 重置子表查询条件
            DetailKeyword = string.Empty;
            EditingVisitorDetail = null;
            _ = LoadDetailsAsync();
        }
    }

    partial void OnEditingVisitorDetailChanged(VisitorDetailDto? value)
    {
        // 通知所有相关命令重新评估 CanExecute
        CreateDetailInlineCommand.NotifyCanExecuteChanged();
        UpdateDetailInlineCommand.NotifyCanExecuteChanged();
        SaveDetailInlineCommand.NotifyCanExecuteChanged();
        CancelDetailInlineCommand.NotifyCanExecuteChanged();
        DeleteDetailCommand.NotifyCanExecuteChanged();
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
    }

    private async Task LoadVisitorsAsync()
    {
        if (IsLoadingVisitors) return;

        IsLoadingVisitors = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            _operLog?.Information("[VisitorView] Load visitors: pageIndex={PageIndex}, pageSize={PageSize}, keyword={Keyword}",
                VisitorPageIndex, VisitorPageSize, VisitorKeyword);

            // 构建查询DTO
            var query = new VisitorQueryDto
            {
                PageIndex = VisitorPageIndex,
                PageSize = VisitorPageSize,
                Keywords = string.IsNullOrWhiteSpace(VisitorKeyword) ? null : VisitorKeyword.Trim()
            };

            var result = await _visitorService.GetListAsync(query);

            if (!result.Success || result.Data == null)
            {
                Visitors.Clear();
                VisitorTotalCount = 0;
                ErrorMessage = result.Message ?? _localizationManager.GetString("Logistics.Visitors.LoadFailed") ?? "加载访客数据失败";
                return;
            }

            Visitors.Clear();
            foreach (var visitor in result.Data.Items)
            {
                Visitors.Add(visitor);
            }

            VisitorTotalCount = result.Data.TotalNum;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[VisitorView] 加载访客列表失败");
        }
        finally
        {
            IsLoadingVisitors = false;
        }
    }

    private async Task LoadDetailsAsync()
    {
        if (SelectedVisitor == null)
        {
            VisitorDetails.Clear();
            return;
        }

        if (IsLoadingDetails) return;

        IsLoadingDetails = true;
        ErrorMessage = null;

        try
        {
            _operLog?.Information("[VisitorView] Load visitor details: visitorId={VisitorId}, keyword={Keyword}",
                SelectedVisitor.Id, DetailKeyword);

            // TaktInlineEditDataGrid 不支持分页，一次性加载所有数据
            var query = new VisitorDetailQueryDto
            {
                VisitorId = SelectedVisitor.Id,
                Name = string.IsNullOrWhiteSpace(DetailKeyword) ? null : DetailKeyword.Trim(),
                Department = string.IsNullOrWhiteSpace(DetailKeyword) ? null : DetailKeyword.Trim(),
                PageIndex = 1,
                PageSize = 1000 // 加载足够多的数据
            };

            var result = await _visitorDetailService.GetListAsync(query);

            if (result.Success && result.Data != null)
            {
                VisitorDetails.Clear();
                foreach (var detail in result.Data.Items)
                {
                    VisitorDetails.Add(detail);
                }
            }
            else
            {
                VisitorDetails.Clear();
                ErrorMessage = result.Message ?? _localizationManager.GetString("Logistics.Visitors.LoadDetailsFailed") ?? "加载访客详情失败";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[VisitorView] 加载访客详情失败");
        }
        finally
        {
            IsLoadingDetails = false;
        }
    }

    [RelayCommand]
    private async Task QueryVisitorsAsync(QueryContext context)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[VisitorView] 执行查询访客操作，操作人={Operator}, 关键词={Keyword}, 页码={PageIndex}, 每页数量={PageSize}", 
            operatorName, context.Keyword ?? string.Empty, context.PageIndex, context.PageSize);
        
        VisitorKeyword = context.Keyword ?? string.Empty;
        if (VisitorPageIndex != context.PageIndex)
        {
            VisitorPageIndex = context.PageIndex <= 0 ? 1 : context.PageIndex;
        }

        if (VisitorPageSize != context.PageSize)
        {
            VisitorPageSize = context.PageSize <= 0 ? 20 : context.PageSize;
        }

        await LoadVisitorsAsync();
    }

    [RelayCommand]
    private async Task ResetVisitorsAsync()
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[VisitorView] 执行重置访客操作，操作人={Operator}", operatorName);
        
        VisitorKeyword = string.Empty;
        VisitorPageIndex = 1;
        VisitorPageSize = 20;
        await LoadVisitorsAsync();
    }

    [RelayCommand]
    private async Task PageChangedVisitorsAsync(PageRequest request)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[VisitorView] 访客分页变化，操作人={Operator}, 页码={PageIndex}, 每页数量={PageSize}", 
            operatorName, request.PageIndex, request.PageSize);
        
        VisitorPageIndex = request.PageIndex;
        VisitorPageSize = request.PageSize;
        await LoadVisitorsAsync();
    }

    [RelayCommand]
    private async Task QueryDetailsAsync(Takt.Fluent.Controls.QueryContext context)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[VisitorView] 执行查询访客详情操作，操作人={Operator}, 关键词={Keyword}", 
            operatorName, context.Keyword ?? string.Empty);
        
        DetailKeyword = context.Keyword ?? string.Empty;
        await LoadDetailsAsync();
    }

    [RelayCommand]
    private async Task ResetDetailsAsync(Takt.Fluent.Controls.QueryContext context)
    {
        var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
        _operLog?.Information("[VisitorView] 执行重置访客详情操作，操作人={Operator}", operatorName);
        
        DetailKeyword = string.Empty;
        await LoadDetailsAsync();
    }

    [RelayCommand]
    private void CreateVisitor()
    {
        ShowVisitorForm(null);
    }

    [RelayCommand]
    private void UpdateVisitor(VisitorDto? visitor)
    {
        if (visitor == null)
        {
            visitor = SelectedVisitor;
        }

        if (visitor == null)
        {
            return;
        }

        ShowVisitorForm(visitor);
    }

    /// <summary>
    /// 打开访客表单窗口
    /// </summary>
    /// <param name="visitor">要编辑的访客，null 表示新建</param>
    private void ShowVisitorForm(VisitorDto? visitor)
    {
        try
        {
            var window = _serviceProvider.GetRequiredService<Takt.Fluent.Views.Logistics.Visitors.VisitorComponent.VisitorForm>();
            if (window.DataContext is not VisitorFormViewModel formViewModel)
            {
                throw new InvalidOperationException("VisitorForm DataContext 不是 VisitorFormViewModel");
            }

            if (visitor == null)
            {
                formViewModel.ForCreate();
            }
            else
            {
                formViewModel.ForUpdate(visitor);
            }

            formViewModel.SaveSuccessCallback = async () =>
            {
                window.Close();
                await LoadVisitorsAsync();
            };

            window.Owner = System.Windows.Application.Current?.MainWindow;
            window.ShowDialog();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[VisitorView] 打开访客表单窗口失败");
        }
    }

    [RelayCommand]
    private async Task DeleteVisitorAsync(VisitorDto? visitor)
    {
        if (visitor == null)
        {
            visitor = SelectedVisitor;
        }

        if (visitor == null)
        {
            return;
        }

        try
        {
            var result = await _visitorService.DeleteAsync(visitor.Id);
            if (!result.Success)
            {
                ErrorMessage = result.Message ?? _localizationManager.GetString("common.deleteFailed") ?? "删除失败";
                return;
            }

            var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
            SuccessMessage = _localizationManager.GetString("common.deleteSuccess") ?? "删除成功";
            _operLog?.Information("[VisitorView] 删除访客成功，操作人={Operator}, 访客Id={Id}", operatorName, visitor.Id);
            await LoadVisitorsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[VisitorView] 删除访客失败");
        }
    }

    [RelayCommand(CanExecute = nameof(CanCreateDetailInline))]
    private void CreateDetailInline()
    {
        if (SelectedVisitor == null)
        {
            ErrorMessage = "请先选择访客";
            return;
        }

        // 创建新的访客详情对象
        var newDetail = new VisitorDetailDto
        {
            VisitorId = SelectedVisitor.Id,
            Department = string.Empty,
            Name = string.Empty,
            Position = string.Empty
        };

        // 添加到列表
        VisitorDetails.Add(newDetail);

        // 设置正在编辑的项，让 TaktInlineEditDataGrid 自动进入编辑状态
        EditingVisitorDetail = newDetail;
        SelectedVisitorDetail = newDetail;

        // 通知命令重新评估 CanExecute
        SaveDetailInlineCommand.NotifyCanExecuteChanged();
        CancelDetailInlineCommand.NotifyCanExecuteChanged();
        UpdateDetailInlineCommand.NotifyCanExecuteChanged();

        // 延迟触发编辑状态，确保 UI 已更新
        System.Windows.Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (UpdateDetailInlineCommand.CanExecute(newDetail))
            {
                UpdateDetailInlineCommand.Execute(newDetail);
            }
        }), System.Windows.Threading.DispatcherPriority.Loaded);

        _operLog?.Information("[VisitorView] 新增访客详情行，访客Id={VisitorId}", SelectedVisitor.Id);
    }

    private bool CanCreateDetailInline()
    {
        return SelectedVisitor is not null && EditingVisitorDetail == null;
    }

    [RelayCommand(CanExecute = nameof(CanUpdateDetailInline))]
    private void UpdateDetailInline(VisitorDetailDto? detail)
    {
        if (detail == null)
        {
            detail = SelectedVisitorDetail;
        }

        if (detail == null)
        {
            return;
        }

        EditingVisitorDetail = detail;

        // 通知命令重新评估 CanExecute
        SaveDetailInlineCommand.NotifyCanExecuteChanged();
        CancelDetailInlineCommand.NotifyCanExecuteChanged();

        _operLog?.Information("[VisitorView] 进入编辑访客详情状态，详情Id={Id}", detail.Id);
    }

    private bool CanUpdateDetailInline(VisitorDetailDto? detail)
    {
        if (detail == null)
        {
            return SelectedVisitorDetail is not null && EditingVisitorDetail == null;
        }
        return detail is not null && EditingVisitorDetail == null;
    }

    [RelayCommand(CanExecute = nameof(CanSaveDetailInline))]
    private async Task SaveDetailInlineAsync(VisitorDetailDto? detail)
    {
        if (detail == null)
        {
            detail = EditingVisitorDetail;
        }

        if (detail == null || EditingVisitorDetail != detail)
        {
            return;
        }

        // 验证必填字段
        if (string.IsNullOrWhiteSpace(detail.Department))
        {
            ErrorMessage = "部门不能为空";
            return;
        }

        if (string.IsNullOrWhiteSpace(detail.Name))
        {
            ErrorMessage = "姓名不能为空";
            return;
        }

        if (string.IsNullOrWhiteSpace(detail.Position))
        {
            ErrorMessage = "职务不能为空";
            return;
        }

        try
        {
            if (detail.Id == 0)
            {
                // 新增
                var createDto = new VisitorDetailCreateDto
                {
                    VisitorId = detail.VisitorId,
                    Department = detail.Department,
                    Name = detail.Name,
                    Position = detail.Position,
                    Remarks = detail.Remarks
                };

                var result = await _visitorDetailService.CreateAsync(createDto);
                if (!result.Success || result.Data <= 0)
                {
                    ErrorMessage = result.Message ?? "创建失败";
                    return;
                }

                detail.Id = result.Data;
                EditingVisitorDetail = null;

                // 通知命令重新评估 CanExecute
                SaveDetailInlineCommand.NotifyCanExecuteChanged();
                CancelDetailInlineCommand.NotifyCanExecuteChanged();

                SuccessMessage = "创建成功";
                _operLog?.Information("[VisitorView] 访客详情创建成功，详情Id={Id}", detail.Id);

                await LoadDetailsAsync();
            }
            else
            {
                // 更新
                var updateDto = new VisitorDetailUpdateDto
                {
                    Id = detail.Id,
                    VisitorId = detail.VisitorId,
                    Department = detail.Department,
                    Name = detail.Name,
                    Position = detail.Position,
                    Remarks = detail.Remarks
                };

                var result = await _visitorDetailService.UpdateAsync(updateDto);
                if (!result.Success)
                {
                    ErrorMessage = result.Message ?? "更新失败";
                    return;
                }

                EditingVisitorDetail = null;

                // 通知命令重新评估 CanExecute
                SaveDetailInlineCommand.NotifyCanExecuteChanged();
                CancelDetailInlineCommand.NotifyCanExecuteChanged();

                SuccessMessage = "更新成功";
                _operLog?.Information("[VisitorView] 访客详情更新成功，详情Id={Id}", detail.Id);

                await LoadDetailsAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[VisitorView] 访客详情保存失败");
        }
    }

    private bool CanSaveDetailInline(VisitorDetailDto? detail)
    {
        if (detail == null)
        {
            return EditingVisitorDetail is not null;
        }
        return EditingVisitorDetail != null && EditingVisitorDetail == detail;
    }

    [RelayCommand(CanExecute = nameof(CanCancelDetailInline))]
    private async Task CancelDetailInlineAsync()
    {
        if (EditingVisitorDetail != null)
        {
            EditingVisitorDetail = null;

            // 通知命令重新评估 CanExecute
            SaveDetailInlineCommand.NotifyCanExecuteChanged();
            CancelDetailInlineCommand.NotifyCanExecuteChanged();

            await LoadDetailsAsync(); // 重新加载以恢复原始数据
        }
    }

    private bool CanCancelDetailInline()
    {
        return EditingVisitorDetail != null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteDetail))]
    private async Task DeleteDetailAsync(VisitorDetailDto? detail)
    {
        if (detail == null)
        {
            detail = SelectedVisitorDetail;
        }

        if (detail == null)
        {
            return;
        }

        try
        {
            var result = await _visitorDetailService.DeleteAsync(detail.Id);
            if (!result.Success)
            {
                ErrorMessage = result.Message ?? _localizationManager.GetString("common.deleteFailed") ?? "删除失败";
                return;
            }

            var operatorName = UserContext.Current?.IsAuthenticated == true ? UserContext.Current.Username ?? "Takt365" : "Takt365";
            SuccessMessage = _localizationManager.GetString("common.deleteSuccess") ?? "删除成功";
            _operLog?.Information("[VisitorView] 删除访客详情成功，操作人={Operator}, 详情Id={Id}", operatorName, detail.Id);
            await LoadDetailsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _operLog?.Error(ex, "[VisitorView] 删除访客详情失败");
        }
    }

    private bool CanDeleteDetail(VisitorDetailDto? detail)
    {
        // 编辑状态下不能删除
        if (EditingVisitorDetail != null)
        {
            return false;
        }

        if (detail == null)
        {
            return SelectedVisitorDetail is not null;
        }
        return detail is not null;
    }
}

