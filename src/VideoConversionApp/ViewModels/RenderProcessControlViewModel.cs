using System;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VideoConversionApp.Abstractions;
using VideoConversionApp.Models;

namespace VideoConversionApp.ViewModels;

public partial class RenderProcessControlViewModel : ObservableObject
{
    private readonly IVideoConverterService _converterService;
    private readonly RenderQueueViewModel _renderQueueViewModel;

    [ObservableProperty]
    public partial bool IsRendering { get; set; }
    [ObservableProperty]
    public partial TimeSpan Elapsed { get; set; }
    [ObservableProperty]
    public partial uint ProcessedCount { get; set; }
    [ObservableProperty]
    public partial uint ProcessingNum { get; set; }
    [ObservableProperty]
    public partial uint FailedCount { get; set; }
    [ObservableProperty]
    public partial uint SucceededCount { get; set; }
    [ObservableProperty]
    public partial uint QueueLength { get; set; }

    private DateTime _renderStartedTime;
    private Timer? _timer = null;
    
    public RenderProcessControlViewModel(IVideoConverterService converterService,
        RenderQueueViewModel renderQueueViewModel)
    {
        _converterService = converterService;
        _renderQueueViewModel = renderQueueViewModel;
        
        if (Design.IsDesignMode)
        {
            IsRendering = true;
            Elapsed = TimeSpan.FromSeconds(95);
            ProcessedCount = 0;
            ProcessingNum = 1;
            FailedCount = 0;
            SucceededCount = 1;
            QueueLength = 4;
            return;
        }
        
        _converterService.RenderingQueueProcessingStarted += OnRenderingQueueProcessingStarted;
        _converterService.RenderingQueueProcessingFinished += OnRenderingQueueProcessingFinished;
        _converterService.RenderingCanceled += OnRenderingCanceled;
        _converterService.RenderingFailed += OnRenderingFailed;
        _converterService.RenderingSucceeded += OnRenderingSucceeded;
        
        _renderQueueViewModel.RenderQueue.CollectionChanged += RenderQueueOnCollectionChanged;
    }

    private void RenderQueueOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        QueueLength = (uint)_renderQueueViewModel.RenderQueue.Count;
    }

    private void ResetQueueStats()
    {
        if(_timer != null)
            _timer.Dispose();
        Elapsed = TimeSpan.Zero;
        ProcessedCount = 0;
        ProcessingNum = 1;
        FailedCount = 0;
        SucceededCount = 0;
    }

    private void OnRenderingSucceeded(object? sender, VideoRenderQueueEntry e)
    {
        SucceededCount++;
        ProcessingNum++;
        ProcessedCount++;
    }

    private void OnRenderingFailed(object? sender, VideoRenderQueueEntry e)
    {
        FailedCount++;
        ProcessedCount++;
        ProcessingNum++;
    }

    private void OnRenderingCanceled(object? sender, VideoRenderQueueEntry e)
    {
        FailedCount++;
        ProcessedCount++;
        ProcessingNum++;
    }

    private void OnRenderingQueueProcessingFinished(object? sender, EventArgs e)
    {
        IsRendering = false;
        _timer?.Dispose();
        _timer = null;
    }

    private void OnRenderingQueueProcessingStarted(object? sender, EventArgs e)
    {
        IsRendering = true;
        ResetQueueStats();
        _renderStartedTime = DateTime.Now;
        _timer = new Timer(OnTimerTick, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(200));
    }

    private void OnTimerTick(object? stateInfo)
    {
        Elapsed = TimeSpan.FromSeconds((int)(DateTime.Now - _renderStartedTime).TotalSeconds);
    }

    [RelayCommand]
    private async Task StartRendering()
    {
        if (_renderQueueViewModel.RenderQueue.Count == 0)
            return;
        
        var allSucceeded = await _converterService.ConvertVideosAsync(_renderQueueViewModel.RenderQueue, false);
    }
    
    [RelayCommand]
    private void StopRendering()
    {
        _converterService.SignalCancellation();
    }
    
    [RelayCommand]
    private void ResetQueuedVideoStatuses()
    {
        foreach (var queueEntry in _renderQueueViewModel.RenderQueue)
        {
            queueEntry.ResetStatus();
        }
    }
}