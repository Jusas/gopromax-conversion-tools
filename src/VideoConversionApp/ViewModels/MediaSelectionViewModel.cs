using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VideoConversionApp.Abstractions;
using VideoConversionApp.Config;
using VideoConversionApp.Models;
using VideoConversionApp.Utils;
using VideoConversionApp.ViewModels.Components;

namespace VideoConversionApp.ViewModels;

public partial class MediaSelectionViewModel : ViewModelBase
{
    private readonly IVideoInfoService _videoInfoService;
    private readonly IStorageServiceProvider _storageServiceProvider;
    private readonly IVideoPreviewService _videoPreviewService;
    private readonly IVideoPoolManager _videoPoolManager;
    private readonly IBitmapCache _bitmapCache;
    private readonly ConversionPreviewViewModel _conversionPreviewViewModel;
    private readonly IConfigManager _configManager;

    [ObservableProperty]
    public partial bool SortDescending { get; set; }
    
    public string[] SortOptions =>
    [
        "Date",
        "Filename",
        "Duration"
    ];
    [ObservableProperty]
    public partial string SelectedSort { get; set; }
    
    [ObservableProperty]
    public partial VideoThumbViewModel? SelectedVideoThumbViewModel { get; set; }
    
    public SortableObservableCollection<VideoThumbViewModel> VideoList
    {
        get => field;
        set
        {
            SetProperty(ref field, value);
        }
    } = new ();
    
    public MediaSelectionViewModel(
        IConfigManager configManager,
        IVideoInfoService videoInfoService, 
        IStorageServiceProvider storageServiceProvider,
        IVideoPreviewService videoPreviewService,
        IVideoPoolManager videoPoolManager,
        IBitmapCache bitmapCache,
        ConversionPreviewViewModel conversionPreviewViewModel)
    {
        _configManager = configManager;
        _videoInfoService = videoInfoService;
        _storageServiceProvider = storageServiceProvider;
        _videoPreviewService = videoPreviewService;
        _videoPoolManager = videoPoolManager;
        _bitmapCache = bitmapCache;
        _conversionPreviewViewModel = conversionPreviewViewModel;
        SelectedSort = SortOptions.First();

        if (Design.IsDesignMode)
        {
            VideoList.Add(new VideoThumbViewModel());
            VideoList.Add(new VideoThumbViewModel());
            return;
        }
        
        _videoPoolManager.VideoRemovedFromPool += VideoPoolManagerOnVideoRemovedFromPool;
        
    }

    private void VideoPoolManagerOnVideoRemovedFromPool(object? sender, IConvertableVideo video)
    {
        var match = VideoList.FirstOrDefault(x => x.LinkedVideo == video);
        if (match != null)
        {
            VideoList.Remove(match);
            match.PropertyChanged -= VideoOnPropertiesChanged;
        }
    }

    partial void OnSortDescendingChanged(bool value)
    {
        VideoList.Sort(VideoListSortComparison);
    }

    partial void OnSelectedSortChanged(string value)
    {
        VideoList.Sort(VideoListSortComparison);
    }

    partial void OnSelectedVideoThumbViewModelChanged(VideoThumbViewModel? value)
    {
        _ = _conversionPreviewViewModel.SetPreviewedVideo(value?.LinkedVideo);
    }


    [RelayCommand]
    private async Task AddFiles()
    {
        var previewSettings = _configManager.GetConfig<PreviewsConfig>()!;
        var videoType = new FilePickerFileType("GoPro MAX .360");
        videoType.Patterns = [".360"];

        var selectedFiles = await _storageServiceProvider!.GetStorageProvider().OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            AllowMultiple = true,
            FileTypeFilter = [videoType]
        });

        var thumbGenerationJobs = new List<(VideoThumbViewModel thumbViewModel, IInputVideoInfo inputVideoInfo)>();
        foreach (var selectedFile in selectedFiles)
        {
            var fullFilename = selectedFile!.TryGetLocalPath();
            if (VideoList.Any(v => v.FullFileName == fullFilename))
                continue;
            
            var videoInfo = await _videoInfoService.ParseMediaAsync(fullFilename!);
            IConvertableVideo? video = null;
            if (videoInfo.IsValidVideo && videoInfo.IsGoProMaxFormat)
            {
                video = _videoPoolManager.AddVideoToPool(videoInfo);
                video.PropertyChanged += VideoOnPropertiesChanged;
            }

            var thumbViewModel = new VideoThumbViewModel
            {
                FullFileName = fullFilename!,
                PreviewFileName = Path.GetFileName(fullFilename!),
                FileSize = videoInfo.SizeBytes,
                VideoDateTime = videoInfo.CreatedDateTime,
                VideoLengthSeconds = (double)videoInfo.DurationInSeconds,
                ShowAsSelectedForConversion = false,
                LinkedVideo = video,
                HasProblems = !videoInfo.IsGoProMaxFormat || !videoInfo.IsValidVideo,
                ToolTipMessage = videoInfo.ValidationIssues is { Length: > 0 } 
                    ? string.Join("\n", new[] {"Video has issues, cannot use:"}.Concat(videoInfo.ValidationIssues)) 
                    : null!
            };

            thumbViewModel.OnCloseClickCommand = new RelayCommand(() =>
            {
                VideoList.Remove(thumbViewModel);
                if (thumbViewModel.LinkedVideo != null)
                {
                    _videoPoolManager.RemoveVideoFromPool(thumbViewModel.LinkedVideo);
                }
            });
            
            thumbViewModel.OnVideoCheckedChangedCommand = new RelayCommand<bool>((isChecked) =>
            {
                if (thumbViewModel.LinkedVideo != null)
                    thumbViewModel.LinkedVideo.IsEnabledForConversion = isChecked;
            });
            thumbGenerationJobs.Add((thumbViewModel, videoInfo));
            VideoList.Add(thumbViewModel);
        }
        
        VideoList.Sort(VideoListSortComparison);

        for (var i = 0; i < thumbGenerationJobs.Count; i++)
        {
            var item = thumbGenerationJobs[i];
            var existingThumb = _bitmapCache.Get(item.inputVideoInfo.Filename);
            if (existingThumb != null)
            {
                item.thumbViewModel.ThumbnailImage = existingThumb;
                item.thumbViewModel.HasLoadingThumbnail = false;
                continue;
            }

            var i1 = i;
            var thumbTimePositionMs = previewSettings.ThumbnailTimePositionPcnt / 100.0 * (long)(item.inputVideoInfo.DurationInSeconds * 1000);
            _ = _videoPreviewService.QueueThumbnailGenerationAsync(item.inputVideoInfo, (long)thumbTimePositionMs)
                .ContinueWith(task =>
                {
                    if (task.Result != null)
                    {
                        var bitmap = _bitmapCache.Add(item.inputVideoInfo.Filename, task.Result);
                        thumbGenerationJobs[i1].thumbViewModel.ThumbnailImage = bitmap;
                        thumbGenerationJobs[i1].thumbViewModel.HasLoadingThumbnail = false;
                    }
                });
        }

    }

    private void VideoOnIsEnabledForConversionUpdated(object? sender, bool isEnabledForConversion)
    {
        var video = sender as IConvertableVideo;
        var videoThumbViewModel = VideoList.FirstOrDefault(x => x.LinkedVideo == video);
        if (videoThumbViewModel == null)
            return;

        videoThumbViewModel.ShowAsSelectedForConversion = isEnabledForConversion;
    }


    private int VideoListSortComparison(VideoThumbViewModel x, VideoThumbViewModel y)
    {
        if (SelectedSort == "Date")
        {
            if (SortDescending)
                return x.VideoDateTime > y.VideoDateTime ? -1 : 1;
            else
                return x.VideoDateTime < y.VideoDateTime ? -1 : 1;
        }

        if (SelectedSort == "Filename")
        {
            if (SortDescending)
                return String.CompareOrdinal(y.PreviewFileName, x.PreviewFileName);
            else
                return String.CompareOrdinal(x.PreviewFileName, y.PreviewFileName);
        }

        if (SelectedSort == "Duration")
        {
            if (SortDescending)
                return x.VideoLengthSeconds > y.VideoLengthSeconds ? -1 : 1;
            else
                return x.VideoLengthSeconds < y.VideoLengthSeconds ? -1 : 1;
        }

        return 0;
    }

    private void VideoOnPropertiesChanged(object? sender, EventArgs e)
    {
        var video = sender as IConvertableVideo;
        var videoThumbViewModel = VideoList.FirstOrDefault(x => x.LinkedVideo == video);
        if (videoThumbViewModel == null)
            return;

        videoThumbViewModel.HasConversionSettingsModified = video!.HasNonDefaultSettings;
        videoThumbViewModel.ShowAsSelectedForConversion = video!.IsEnabledForConversion;
    }

    [RelayCommand]
    private void ClearAllFiles()
    {
        foreach (var videoThumbViewModel in VideoList)
        {
            var video = videoThumbViewModel.LinkedVideo;
            if (video != null)
            {
                _videoPoolManager.RemoveVideoFromPool(video);
            }
        }
        VideoList.Clear();
    }

    [RelayCommand]
    private void SelectAllFiles()
    {
        foreach (var videoThumbViewModel in VideoList)
        {
            var video = videoThumbViewModel.LinkedVideo;
            if (video == null) 
                continue;
            
            video.IsEnabledForConversion = true;
            videoThumbViewModel.ShowAsSelectedForConversion = true;
        }
    }

    [RelayCommand]
    private void UnselectAllFiles()
    {
        foreach (var videoThumbViewModel in VideoList)
        {
            if (videoThumbViewModel.LinkedVideo == null) 
                continue;
            
            videoThumbViewModel.LinkedVideo.IsEnabledForConversion = false;
            videoThumbViewModel.ShowAsSelectedForConversion = false;
        }
    }
    
    
}