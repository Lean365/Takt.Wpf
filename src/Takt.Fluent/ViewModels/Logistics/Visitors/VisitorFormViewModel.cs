// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.ViewModels.Logistics.Visitors
// 文件名称：VisitorFormViewModel.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：访客表单视图模型（新建/编辑访客）
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Takt.Application.Dtos.Logistics.Visitors;
using Takt.Application.Services.Logistics.Visitors;
using Takt.Common.Logging;
using Takt.Domain.Interfaces;

namespace Takt.Fluent.ViewModels.Logistics.Visitors;

/// <summary>
/// 访客表单视图模型（新建/编辑访客）
/// </summary>
public partial class VisitorFormViewModel : ObservableObject
{
    private readonly IVisitorService _visitorService;
    private readonly IVisitorDetailService _visitorDetailService;
    private readonly ILocalizationManager _localizationManager;
    private readonly OperLogManager? _operLog;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private bool _isCreate = true;

    [ObservableProperty]
    private long _id;

    [ObservableProperty]
    private string _companyName = string.Empty;

    [ObservableProperty]
    private DateTime _startTime = DateTime.Now;

    [ObservableProperty]
    private DateTime _endTime = DateTime.Now.AddHours(1);

    [ObservableProperty]
    private string? _remarks;

    [ObservableProperty]
    private string _error = string.Empty;

    // 错误消息属性
    [ObservableProperty]
    private string _companyNameError = string.Empty;

    [ObservableProperty]
    private string _startTimeError = string.Empty;

    [ObservableProperty]
    private string _endTimeError = string.Empty;

    [ObservableProperty]
    private string _remarksError = string.Empty;

    // 子表数据
    public ObservableCollection<VisitorDetailDto> VisitorDetails { get; } = new();

    [ObservableProperty]
    private VisitorDetailDto? _selectedVisitorDetail;

    [ObservableProperty]
    private VisitorDetailDto? _editingVisitorDetail;

    // Hint 提示属性
    public string CompanyNameHint => _localizationManager.GetString("Logistics.Visitors.Validation.CompanyNameInvalid") ?? "公司名称不能为空，长度不能超过200个字符";

    public string StartTimeHint => _localizationManager.GetString("Logistics.Visitors.Validation.StartTimeInvalid") ?? "开始时间不能为空，且必须早于结束时间";

    public string EndTimeHint => _localizationManager.GetString("Logistics.Visitors.Validation.EndTimeInvalid") ?? "结束时间不能为空，且必须晚于开始时间";

    /// <summary>
    /// 保存成功后的回调，用于关闭窗口
    /// </summary>
    public Action? SaveSuccessCallback { get; set; }

    public VisitorFormViewModel(
        IVisitorService visitorService,
        IVisitorDetailService visitorDetailService,
        ILocalizationManager localizationManager,
        OperLogManager? operLog = null)
    {
        _visitorService = visitorService ?? throw new ArgumentNullException(nameof(visitorService));
        _visitorDetailService = visitorDetailService ?? throw new ArgumentNullException(nameof(visitorDetailService));
        _localizationManager = localizationManager ?? throw new ArgumentNullException(nameof(localizationManager));
        _operLog = operLog;
    }

    public void ForCreate()
    {
        ClearAllErrors();

        IsCreate = true;
        Title = _localizationManager.GetString("Logistics.Visitors.Create") ?? "新建访客";
        CompanyName = string.Empty;
        StartTime = DateTime.Now;
        EndTime = DateTime.Now.AddHours(1);
        Remarks = null;

        // 清空子表数据
        VisitorDetails.Clear();
        SelectedVisitorDetail = null;
        EditingVisitorDetail = null;
    }

    public void ForUpdate(VisitorDto dto)
    {
        ClearAllErrors();

        IsCreate = false;
        Title = _localizationManager.GetString("Logistics.Visitors.Update") ?? "编辑访客";
        Id = dto.Id;
        CompanyName = dto.CompanyName ?? string.Empty;
        StartTime = dto.StartTime;
        EndTime = dto.EndTime;
        Remarks = dto.Remarks;

        // 清空子表数据
        VisitorDetails.Clear();
        SelectedVisitorDetail = null;
        EditingVisitorDetail = null;

        // 异步加载子表数据
        _ = LoadVisitorDetailsAsync();
    }

    /// <summary>
    /// 加载访客详情
    /// </summary>
    private async Task LoadVisitorDetailsAsync()
    {
        if (Id <= 0)
        {
            return;
        }

        try
        {
            var query = new VisitorDetailQueryDto
            {
                VisitorId = Id,
                PageIndex = 1,
                PageSize = 1000 // 加载所有详情
            };

            var result = await _visitorDetailService.GetListAsync(query);

            if (result.Success && result.Data != null)
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    VisitorDetails.Clear();
                    foreach (var detail in result.Data.Items)
                    {
                        VisitorDetails.Add(detail);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[VisitorForm] 加载访客详情失败");
        }
    }

    /// <summary>
    /// 清除所有错误消息
    /// </summary>
    private void ClearAllErrors()
    {
        CompanyNameError = string.Empty;
        StartTimeError = string.Empty;
        EndTimeError = string.Empty;
        RemarksError = string.Empty;
        Error = string.Empty;
    }

    // 属性变更时进行实时验证
    partial void OnCompanyNameChanged(string value)
    {
        ValidateCompanyName();
    }

    partial void OnStartTimeChanged(DateTime value)
    {
        ValidateTimeRange();
    }

    partial void OnEndTimeChanged(DateTime value)
    {
        ValidateTimeRange();
    }

    /// <summary>
    /// 验证公司名称（实时验证）
    /// </summary>
    private void ValidateCompanyName()
    {
        if (string.IsNullOrWhiteSpace(CompanyName))
        {
            CompanyNameError = string.Empty; // 空值时不清除错误，等待提交时验证
            return;
        }

        if (CompanyName.Length > 200)
        {
            CompanyNameError = _localizationManager.GetString("Logistics.Visitors.Validation.CompanyNameMaxLength") ?? "公司名称长度不能超过200个字符";
        }
        else
        {
            CompanyNameError = string.Empty; // 验证通过，清除错误
        }
    }

    /// <summary>
    /// 验证时间范围（实时验证）
    /// </summary>
    private void ValidateTimeRange()
    {
        StartTimeError = string.Empty;
        EndTimeError = string.Empty;

        if (StartTime >= EndTime)
        {
            StartTimeError = _localizationManager.GetString("Logistics.Visitors.Validation.StartTimeMustBeforeEndTime") ?? "开始时间必须早于结束时间";
            EndTimeError = _localizationManager.GetString("Logistics.Visitors.Validation.EndTimeMustAfterStartTime") ?? "结束时间必须晚于开始时间";
        }
    }

    /// <summary>
    /// 验证所有字段
    /// </summary>
    private bool ValidateFields()
    {
        bool isValid = true;

        // 验证公司名称
        if (string.IsNullOrWhiteSpace(CompanyName))
        {
            CompanyNameError = _localizationManager.GetString("Logistics.Visitors.Validation.CompanyNameRequired") ?? "公司名称不能为空";
            isValid = false;
        }
        else if (CompanyName.Length > 200)
        {
            CompanyNameError = _localizationManager.GetString("Logistics.Visitors.Validation.CompanyNameMaxLength") ?? "公司名称长度不能超过200个字符";
            isValid = false;
        }
        else
        {
            CompanyNameError = string.Empty;
        }

        // 验证时间范围
        if (StartTime >= EndTime)
        {
            StartTimeError = _localizationManager.GetString("Logistics.Visitors.Validation.StartTimeMustBeforeEndTime") ?? "开始时间必须早于结束时间";
            EndTimeError = _localizationManager.GetString("Logistics.Visitors.Validation.EndTimeMustAfterStartTime") ?? "结束时间必须晚于开始时间";
            isValid = false;
        }
        else
        {
            StartTimeError = string.Empty;
            EndTimeError = string.Empty;
        }

        return isValid;
    }

    /// <summary>
    /// 保存访客信息
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            // 验证字段
            if (!ValidateFields())
            {
                return;
            }

            // 如果有正在编辑的子表项，提示先保存或取消
            if (EditingVisitorDetail != null)
            {
                Error = _localizationManager.GetString("Logistics.Visitors.PleaseSaveOrCancelDetail") ?? "请先保存或取消正在编辑的访客详情";
                return;
            }

            long visitorId;

            if (IsCreate)
            {
                var dto = new VisitorCreateDto
                {
                    CompanyName = CompanyName.Trim(),
                    StartTime = StartTime,
                    EndTime = EndTime,
                    Remarks = string.IsNullOrWhiteSpace(Remarks) ? null : Remarks.Trim()
                };

                var result = await _visitorService.CreateAsync(dto);
                if (!result.Success)
                {
                    Error = result.Message ?? _localizationManager.GetString("Logistics.Visitors.CreateFailed") ?? "创建访客失败";
                    return;
                }

                visitorId = result.Data;
                Id = visitorId; // 更新 Id，用于后续保存子表

                _operLog?.Information("[VisitorForm] 创建访客成功，Id={Id}, 公司名称={CompanyName}", visitorId, CompanyName);
            }
            else
            {
                var dto = new VisitorUpdateDto
                {
                    Id = Id,
                    CompanyName = CompanyName.Trim(),
                    StartTime = StartTime,
                    EndTime = EndTime,
                    Remarks = string.IsNullOrWhiteSpace(Remarks) ? null : Remarks.Trim()
                };

                var result = await _visitorService.UpdateAsync(dto);
                if (!result.Success)
                {
                    Error = result.Message ?? _localizationManager.GetString("Logistics.Visitors.UpdateFailed") ?? "更新访客失败";
                    return;
                }

                visitorId = Id;

                _operLog?.Information("[VisitorForm] 更新访客成功，Id={Id}, 公司名称={CompanyName}", Id, CompanyName);
            }

            // 保存子表数据
            await SaveVisitorDetailsAsync(visitorId);

            SaveSuccessCallback?.Invoke();
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            _operLog?.Error(ex, "[VisitorForm] 保存访客失败");
        }
    }

    /// <summary>
    /// 保存访客详情
    /// </summary>
    private async Task SaveVisitorDetailsAsync(long visitorId)
    {
        // 获取需要新增和更新的详情项
        var newDetails = VisitorDetails.Where(d => d.Id == 0).ToList();
        var updatedDetails = VisitorDetails.Where(d => d.Id > 0).ToList();

        // 获取数据库中已存在的详情ID
        var existingDetails = await _visitorDetailService.GetListAsync(new VisitorDetailQueryDto
        {
            VisitorId = visitorId,
            PageIndex = 1,
            PageSize = 1000
        });

        var existingIds = existingDetails.Success && existingDetails.Data != null
            ? existingDetails.Data.Items.Select(d => d.Id).ToList()
            : new List<long>();

        // 删除已从列表中移除的详情（如果存在）
        var deletedIds = existingIds.Except(updatedDetails.Select(d => d.Id)).ToList();
        foreach (var deletedId in deletedIds)
        {
            await _visitorDetailService.DeleteAsync(deletedId);
        }

        // 新增详情
        foreach (var detail in newDetails)
        {
            if (string.IsNullOrWhiteSpace(detail.Department) || 
                string.IsNullOrWhiteSpace(detail.Name) || 
                string.IsNullOrWhiteSpace(detail.Position))
            {
                continue; // 跳过未填写的项
            }

            var createDto = new VisitorDetailCreateDto
            {
                VisitorId = visitorId,
                Department = detail.Department.Trim(),
                Name = detail.Name.Trim(),
                Position = detail.Position.Trim(),
                Remarks = string.IsNullOrWhiteSpace(detail.Remarks) ? null : detail.Remarks.Trim()
            };

            var result = await _visitorDetailService.CreateAsync(createDto);
            if (result.Success && result.Data > 0)
            {
                detail.Id = result.Data;
            }
        }

        // 更新详情
        foreach (var detail in updatedDetails)
        {
            if (string.IsNullOrWhiteSpace(detail.Department) || 
                string.IsNullOrWhiteSpace(detail.Name) || 
                string.IsNullOrWhiteSpace(detail.Position))
            {
                continue; // 跳过未填写的项
            }

            var updateDto = new VisitorDetailUpdateDto
            {
                Id = detail.Id,
                VisitorId = visitorId,
                Department = detail.Department.Trim(),
                Name = detail.Name.Trim(),
                Position = detail.Position.Trim(),
                Remarks = string.IsNullOrWhiteSpace(detail.Remarks) ? null : detail.Remarks.Trim()
            };

            await _visitorDetailService.UpdateAsync(updateDto);
        }
    }

    /// <summary>
    /// 取消操作
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        // 关闭窗口由窗口本身处理
    }

    #region 子表命令

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

    [RelayCommand(CanExecute = nameof(CanCreateDetailInline))]
    private void CreateDetailInline()
    {
        // 创建新的访客详情对象
        var newDetail = new VisitorDetailDto
        {
            VisitorId = Id > 0 ? Id : 0, // 如果是新建，Id 为 0，保存主表后会更新
            Department = string.Empty,
            Name = string.Empty,
            Position = string.Empty,
            CreatedTime = DateTime.Now,
            UpdatedTime = DateTime.Now
        };

        // 添加到列表（在 UI 线程上）
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            VisitorDetails.Add(newDetail);
        });

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

        _operLog?.Information("[VisitorForm] 新增访客详情行");
    }

    private bool CanCreateDetailInline()
    {
        return EditingVisitorDetail == null;
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

        _operLog?.Information("[VisitorForm] 进入编辑访客详情状态，详情Id={Id}", detail.Id);
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
    private void SaveDetailInline(VisitorDetailDto? detail)
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
        if (string.IsNullOrWhiteSpace(detail.Department) || 
            string.IsNullOrWhiteSpace(detail.Name) || 
            string.IsNullOrWhiteSpace(detail.Position))
        {
            Error = _localizationManager.GetString("Logistics.Visitors.DetailFieldsRequired") ?? "部门、姓名、职务不能为空";
            return;
        }

        // 清除编辑状态（实际保存会在保存主表时进行）
        EditingVisitorDetail = null;

        // 通知命令重新评估 CanExecute
        SaveDetailInlineCommand.NotifyCanExecuteChanged();
        CancelDetailInlineCommand.NotifyCanExecuteChanged();

        _operLog?.Information("[VisitorForm] 访客详情保存成功（待主表保存时提交）");
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
    private void CancelDetailInline()
    {
        if (EditingVisitorDetail != null)
        {
            var detail = EditingVisitorDetail;

            // 如果是新添加的项（Id=0），从列表中移除
            if (detail.Id == 0)
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    VisitorDetails.Remove(detail);
                });
            }

            EditingVisitorDetail = null;
            SelectedVisitorDetail = null;

            // 通知命令重新评估 CanExecute
            SaveDetailInlineCommand.NotifyCanExecuteChanged();
            CancelDetailInlineCommand.NotifyCanExecuteChanged();

            _operLog?.Information("[VisitorForm] 取消编辑访客详情");
        }
    }

    private bool CanCancelDetailInline()
    {
        return EditingVisitorDetail != null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteDetail))]
    private void DeleteDetail(VisitorDetailDto? detail)
    {
        if (detail == null)
        {
            detail = SelectedVisitorDetail;
        }

        if (detail == null)
        {
            return;
        }

        // 从列表中移除（如果是新添加的项，直接移除；如果是已存在的项，标记为删除，在保存时处理）
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            VisitorDetails.Remove(detail);
        });

        SelectedVisitorDetail = null;

        _operLog?.Information("[VisitorForm] 删除访客详情，详情Id={Id}", detail.Id);
    }

    private bool CanDeleteDetail(VisitorDetailDto? detail)
    {
        return EditingVisitorDetail == null; // 编辑状态下不能删除
    }

    #endregion
}

