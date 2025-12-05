// ========================================
// 项目名称：节拍(Takt)中小企业平台 · Takt SMEs Platform
// 命名空间：Takt.Fluent.Views.Logistics.Visitors
// 文件名称：DigitalSignageView.xaml.cs
// 创建时间：2025-01-20
// 创建人：Takt365(Cursor AI)
// 功能描述：数字标牌视图代码后台
//
// 版权信息：Copyright (c) 2025 Takt All rights reserved.
// 免责声明：此软件使用 MIT License，作者不承担任何使用风险。
// ========================================

using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LibVLCSharp.Shared;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;
using Takt.Common.Logging;
using Takt.Fluent.ViewModels.Logistics.Visitors;

namespace Takt.Fluent.Views.Logistics.Visitors;

public partial class DigitalSignageView : UserControl
{
    public DigitalSignageViewModel ViewModel { get; }
    private readonly OperLogManager? _operLog;
    private LibVLC? _libVLC;
    private MediaPlayer? _mediaPlayer;
    private Media? _currentMedia;

    public DigitalSignageView(DigitalSignageViewModel viewModel, OperLogManager? operLog = null)
    {
        InitializeComponent();
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _operLog = operLog;
        DataContext = ViewModel;

        Loaded += DigitalSignageView_Loaded;
        Unloaded += DigitalSignageView_Unloaded;

        // 监听 ShowVisitorInfo、CurrentVisitor、CurrentVisitorDetails、AdVideoPath 属性变化，控制视频播放
        ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.ShowVisitorInfo) ||
                e.PropertyName == nameof(ViewModel.CurrentVisitor) ||
                e.PropertyName == nameof(ViewModel.CurrentVisitorDetails) ||
                e.PropertyName == nameof(ViewModel.AdVideoPath))
            {
                UpdateVideoPlayback();
            }
        };
    }

    private void DigitalSignageView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        try
        {
            // 初始化 LibVLC（VideoLAN.LibVLC.Windows 包会自动处理本地库路径）
            // Core.Initialize() 可以安全地多次调用，如果已初始化则不会重复初始化
            Core.Initialize();
            
            // 创建 LibVLC 实例
            _libVLC = new LibVLC(enableDebugLogs: false);
            
            // 创建 MediaPlayer
            _mediaPlayer = new MediaPlayer(_libVLC);
            
            // 绑定 MediaPlayer 到 VideoView
            AdVideoPlayer.MediaPlayer = _mediaPlayer;
            
            // 订阅事件
            _mediaPlayer.EndReached += MediaPlayer_EndReached;
            _mediaPlayer.EncounteredError += MediaPlayer_EncounteredError;
            
            _operLog?.Information("[DigitalSignageView] LibVLC 初始化成功 - 版本: {Version}", _libVLC.Version);
            
            // 视图加载时，确保视频播放器状态正确
            UpdateVideoPlayback();
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[DigitalSignageView] LibVLC 初始化失败: {Message}", ex.Message);
            
            // 如果初始化失败，禁用视频播放功能，但不影响其他功能
            _libVLC?.Dispose();
            _libVLC = null;
            _mediaPlayer?.Dispose();
            _mediaPlayer = null;
        }
    }

    private void DigitalSignageView_Unloaded(object sender, System.Windows.RoutedEventArgs e)
    {
        try
        {
            // 停止播放
            _mediaPlayer?.Stop();
            
            // 释放 Media
            _currentMedia?.Dispose();
            _currentMedia = null;
            
            // 取消事件订阅
            if (_mediaPlayer != null)
            {
                _mediaPlayer.EndReached -= MediaPlayer_EndReached;
                _mediaPlayer.EncounteredError -= MediaPlayer_EncounteredError;
            }
            
            // 释放 MediaPlayer
            _mediaPlayer?.Dispose();
            _mediaPlayer = null;
            
            // 释放 LibVLC
            _libVLC?.Dispose();
            _libVLC = null;
            
            _operLog?.Information("[DigitalSignageView] LibVLC 资源已释放");
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[DigitalSignageView] 释放 LibVLC 资源失败");
        }

        // 释放 ViewModel 资源
        ViewModel?.Dispose();
    }

    /// <summary>
    /// 更新视频播放状态
    /// </summary>
    private void UpdateVideoPlayback()
    {
        if (_mediaPlayer == null || _libVLC == null)
        {
            _operLog?.Warning("[DigitalSignageView] MediaPlayer 或 LibVLC 未初始化，无法更新播放状态");
            return;
        }

        // 检查是否有数据：CurrentVisitor 不为 null 且 CurrentVisitorDetails 有数据
        bool hasData = ViewModel.CurrentVisitor != null && 
                       ViewModel.CurrentVisitorDetails != null && 
                       ViewModel.CurrentVisitorDetails.Any();

        if (!hasData || !ViewModel.ShowVisitorInfo)
        {
            // 没有数据或不应该显示访客信息，显示广告视频，开始播放
            if (string.IsNullOrEmpty(ViewModel.AdVideoPath))
            {
                _operLog?.Warning("[DigitalSignageView] 视频路径为空，无法播放");
                return;
            }

            // 获取视频路径（LibVLC 支持文件路径和 URI）
            string? videoPath = GetVideoPath(ViewModel.AdVideoPath);
            if (string.IsNullOrEmpty(videoPath))
            {
                _operLog?.Warning("[DigitalSignageView] 无法获取视频路径: {VideoPath}", ViewModel.AdVideoPath);
                return;
            }

            _operLog?.Information("[DigitalSignageView] 准备播放视频 - 路径: {VideoPath}", videoPath);

            try
            {
                // 如果路径改变，重新加载 Media
                string? currentMrl = _currentMedia?.Mrl;
                if (_currentMedia == null || currentMrl != videoPath)
                {
                    // 释放旧的 Media
                    _currentMedia?.Dispose();
                    
                    // 创建新的 Media（LibVLC 支持文件路径和 HTTP/HTTPS URI）
                    if (videoPath.StartsWith("http://") || videoPath.StartsWith("https://"))
                    {
                        // HTTP/HTTPS URI 格式
                        _currentMedia = new Media(_libVLC, videoPath, FromType.FromLocation);
                    }
                    else
                    {
                        // 文件路径格式（包括本地文件和临时文件）
                        _currentMedia = new Media(_libVLC, videoPath, FromType.FromPath);
                    }
                    
                    _mediaPlayer.Media = _currentMedia;
                    
                    _operLog?.Information("[DigitalSignageView] 设置视频源: {VideoPath}", videoPath);
                }
                
                // 开始播放
                if (_mediaPlayer.State != VLCState.Playing)
                {
                    _mediaPlayer.Play();
                    _operLog?.Information("[DigitalSignageView] 开始播放广告视频: {VideoPath}", videoPath);
                }
            }
            catch (Exception ex)
            {
                _operLog?.Error(ex, "[DigitalSignageView] 播放视频失败 - 路径: {VideoPath}", videoPath);
            }
        }
        else
        {
            // 有数据且应该显示访客信息，停止视频
            try
            {
                if (_mediaPlayer.State == VLCState.Playing || _mediaPlayer.State == VLCState.Paused)
                {
                    _mediaPlayer.Stop();
                    _operLog?.Information("[DigitalSignageView] 停止播放广告视频，显示访客信息 - 公司: {CompanyName}, 访客数量: {Count}",
                        ViewModel.CurrentVisitor?.CompanyName ?? "未知",
                        ViewModel.CurrentVisitorDetails?.Count ?? 0);
                }
            }
            catch (Exception ex)
            {
                _operLog?.Error(ex, "[DigitalSignageView] 停止视频失败");
            }
        }
    }

    /// <summary>
    /// 获取视频路径（LibVLC 支持文件路径和 URI）
    /// </summary>
    private string? GetVideoPath(string? videoPath)
    {
        if (string.IsNullOrEmpty(videoPath))
        {
            _operLog?.Warning("[DigitalSignageView] 视频路径为空");
            return null;
        }

        try
        {
            // 如果是绝对路径，直接使用
            if (Path.IsPathRooted(videoPath))
            {
                if (File.Exists(videoPath))
                {
                    _operLog?.Information("[DigitalSignageView] 使用绝对路径: {Path}", videoPath);
                    return videoPath;
                }
                else
                {
                    _operLog?.Warning("[DigitalSignageView] 绝对路径文件不存在: {Path}", videoPath);
                }
            }
            else
            {
                // 相对路径：先尝试从应用程序目录查找
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string fullPath = Path.Combine(appDirectory, videoPath);
                
                if (File.Exists(fullPath))
                {
                    _operLog?.Information("[DigitalSignageView] 使用应用程序目录路径: {Path}", fullPath);
                    return fullPath;
                }
                else
                {
                    _operLog?.Warning("[DigitalSignageView] 应用程序目录文件不存在: {Path}，尝试从资源流提取", fullPath);
                    
                    // 如果文件不存在，尝试从资源流中提取到临时文件
                    // LibVLC 不支持 pack:// URI，需要文件路径
                    try
                    {
                        var normalizedPath = videoPath.Replace('\\', '/');
                        if (!normalizedPath.StartsWith("/"))
                        {
                            normalizedPath = "/" + normalizedPath;
                        }
                        
                        // 确保路径首字母大写（Assets 而不是 assets）
                        var parts = normalizedPath.Split('/');
                        if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
                        {
                            parts[1] = char.ToUpperInvariant(parts[1][0]) + (parts[1].Length > 1 ? parts[1].Substring(1) : string.Empty);
                            normalizedPath = string.Join("/", parts);
                        }
                        
                        var packUri = new Uri($"pack://application:,,,{normalizedPath}", UriKind.Absolute);
                        var resourceStream = System.Windows.Application.GetResourceStream(packUri);
                        
                        if (resourceStream != null)
                        {
                            // 创建临时文件
                            string tempDir = Path.Combine(Path.GetTempPath(), "TaktDigitalSignage");
                            Directory.CreateDirectory(tempDir);
                            
                            string fileName = Path.GetFileName(videoPath);
                            string tempFilePath = Path.Combine(tempDir, fileName);
                            
                            // 如果文件已存在且较新，直接使用
                            if (!File.Exists(tempFilePath) || File.GetLastWriteTime(tempFilePath) < DateTime.Now.AddHours(-1))
                            {
                                using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
                                {
                                    resourceStream.Stream.CopyTo(fileStream);
                                }
                                _operLog?.Information("[DigitalSignageView] 从资源流提取文件到临时目录: {Path}", tempFilePath);
                            }
                            
                            return tempFilePath;
                        }
                    }
                    catch (Exception ex)
                    {
                        _operLog?.Warning("[DigitalSignageView] 从资源流提取文件失败: {Path}, 错误: {Error}", videoPath, ex.Message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _operLog?.Error(ex, "[DigitalSignageView] 获取视频路径失败: {Path}", videoPath);
        }

        return null;
    }

    /// <summary>
    /// 视频播放结束事件处理（循环播放）
    /// </summary>
    private void MediaPlayer_EndReached(object? sender, EventArgs e)
    {
        if (_mediaPlayer == null)
            return;

        // 检查是否有数据：CurrentVisitor 不为 null 且 CurrentVisitorDetails 有数据
        bool hasData = ViewModel.CurrentVisitor != null && 
                       ViewModel.CurrentVisitorDetails != null && 
                       ViewModel.CurrentVisitorDetails.Any();

        // 如果没有数据或不应该显示访客信息，循环播放视频
        if (!hasData || !ViewModel.ShowVisitorInfo)
        {
            try
            {
                if (_currentMedia != null && _mediaPlayer != null)
                {
                    // 重新设置 Media 并播放（实现循环）
                    _mediaPlayer.Media = _currentMedia;
                    _mediaPlayer.Play();
                    _operLog?.Information("[DigitalSignageView] 视频播放结束，重新开始播放 - 路径: {VideoPath}", 
                        ViewModel.AdVideoPath ?? "未知");
                }
            }
            catch (Exception ex)
            {
                _operLog?.Error(ex, "[DigitalSignageView] 循环播放视频失败");
            }
        }
    }

    /// <summary>
    /// 视频播放错误事件处理
    /// </summary>
    private void MediaPlayer_EncounteredError(object? sender, EventArgs e)
    {
        _operLog?.Error("[DigitalSignageView] 视频播放错误 - 路径: {VideoPath}", 
            ViewModel.AdVideoPath ?? "未知");
    }
}

