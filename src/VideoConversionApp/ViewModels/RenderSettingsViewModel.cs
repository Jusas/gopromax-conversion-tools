using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VideoConversionApp.Abstractions;
using VideoConversionApp.Config;
using VideoConversionApp.Models;

namespace VideoConversionApp.ViewModels;

public partial class RenderSettingsViewModel : ViewModelBase
{
    private readonly IVideoPoolManager _videoPoolManager;
    private readonly IVideoConverterService _converterService;
    private readonly IStorageServiceProvider _storageServiceProvider;
    private readonly IConfigManager _configManager;

    [ObservableProperty]
    public partial string SelectedOutputDirectoryMethod { get; set; }
    [ObservableProperty]
    public partial string SelectedOutputDirectory { get; set; }
    [ObservableProperty]
    public partial string SelectedOutputDirectoryIssues { get; set; }
    [ObservableProperty]
    public partial string SelectedVideoCodecTab { get; set; }
    [ObservableProperty]
    public partial string SelectedAudioCodecTab { get; set; }
    [ObservableProperty]
    public partial bool IsOutputToSelectedDirSelected { get; set; } = true;
    [ObservableProperty]
    public partial bool IsOtherVideoCodecSelected { get; set; }
    [ObservableProperty]
    public partial bool IsOtherAudioCodecSelected { get; set; }
    [ObservableProperty]
    public partial string CustomVideoCodecName { get; set; }
    [ObservableProperty]
    public partial string CustomAudioCodecName { get; set; }
    [ObservableProperty]
    public partial string CustomContainerName { get; set; }
    [ObservableProperty]
    public partial uint CustomResolutionWidth { get; set; }
    [ObservableProperty]
    public partial uint CustomResolutionHeight { get; set; }
    [ObservableProperty]
    public partial string FilenamePattern { get; set; } = "%o";
    [ObservableProperty]
    public partial string FilenamePatternIssues { get; set; }
    [ObservableProperty]
    public partial string FilenamePreview { get; set; }
    [ObservableProperty]
    public partial bool AllowOverwrite { get; set; }
    [ObservableProperty]
    public partial ObservableCollection<CodecEntry>? AudioCodecs { get; set; }
    [ObservableProperty]
    public partial ObservableCollection<CodecEntry>? VideoCodecs { get; set; }
    [ObservableProperty]
    public partial ObservableCollection<CodecEntry>? Containers { get; set; }

    private bool _eventsEnabled = true;
    
    public RenderSettingsViewModel(IVideoPoolManager videoPoolManager,
        IVideoConverterService converterService,
        IStorageServiceProvider storageServiceProvider,
        IConfigManager configManager)
    {
        _videoPoolManager = videoPoolManager;
        _converterService = converterService;
        _storageServiceProvider = storageServiceProvider;
        _configManager = configManager;

        if (Design.IsDesignMode)
            return;
        
        PopulateFromConfig(_configManager.GetConfig<ConversionConfig>()!);
        _configManager.NewConfigLoaded += ConfigManagerOnNewConfigLoaded;
        
    }

    private void ConfigManagerOnNewConfigLoaded(object? sender, EventArgs e)
    {
        PopulateFromConfig(_configManager.GetConfig<ConversionConfig>()!);
    }

    partial void OnSelectedOutputDirectoryMethodChanged(string value)
    {
        if (!_eventsEnabled) return; 
        
        IsOutputToSelectedDirSelected = value == "OutputToSelectedDir";
        _configManager.GetConfig<ConversionConfig>()!.OutputBesideOriginals = !IsOutputToSelectedDirSelected;
    }

    partial void OnSelectedOutputDirectoryChanged(string value)
    {
        if (!_eventsEnabled) return;
        
        var exists = Directory.Exists(value);
        SelectedOutputDirectoryIssues = exists ? string.Empty : "Directory does not exist";
        _configManager.GetConfig<ConversionConfig>()!.OutputDirectory = value;
    }

    

    partial void OnSelectedVideoCodecTabChanged(string value)
    {
        if (!_eventsEnabled) return;
        
        IsOtherVideoCodecSelected = value == "VcOther";
        if (IsOtherVideoCodecSelected && VideoCodecs == null)
            RefreshCodecLists();
        
        _configManager.GetConfig<ConversionConfig>()!.UseCustomEncodingSettings = IsOtherVideoCodecSelected;
        _configManager.GetConfig<ConversionConfig>()!.CodecVideo = value switch
        {
            "VcProres" => "prores", // MOV container
            "VcCineform" => "cfhd", // MOV container
            _ => CustomVideoCodecName 
        };
    }

    partial void OnSelectedAudioCodecTabChanged(string value)
    {
        if (!_eventsEnabled) return;
        
        IsOtherAudioCodecSelected = value == "AcOther";
        if (IsOtherAudioCodecSelected && AudioCodecs == null)
            RefreshCodecLists();
        
        _configManager.GetConfig<ConversionConfig>()!.CodecAudio = value switch
        {
            "AcPcms16le" => "pcm_s16le",
            "AcPcms32le" => "pcm_s32le",
            "AcNoAudio" => "",
            _ => CustomAudioCodecName
        };
        _configManager.GetConfig<ConversionConfig>()!.OutputAudio = value != "AcNoAudio";
    }

    partial void OnCustomVideoCodecNameChanged(string value)
    {
        if (!_eventsEnabled) return;
        
        if (IsOtherVideoCodecSelected)
            _configManager.GetConfig<ConversionConfig>()!.CodecVideo = value;
    }

    partial void OnCustomAudioCodecNameChanged(string value)
    {
        if (!_eventsEnabled) return;
        
        if (IsOtherAudioCodecSelected)
            _configManager.GetConfig<ConversionConfig>()!.CodecAudio = value;
    }

    partial void OnFilenamePatternChanged(string value)
    {
        if (!_eventsEnabled) return;
        
        var dummyVideo = _videoPoolManager.GetDummyVideo();
        FilenamePreview = _converterService.GetFilenameFromPattern(dummyVideo, value);
        
        _configManager.GetConfig<ConversionConfig>()!.OutputFilenamePattern = value;

        FilenamePatternIssues = string.IsNullOrWhiteSpace(FilenamePattern) ? "Filename pattern is empty" : "";
    }

    partial void OnCustomContainerNameChanged(string value)
    {
        if (!_eventsEnabled) return;
        
        _configManager.GetConfig<ConversionConfig>()!.CustomContainerName = value;
    }

    partial void OnCustomResolutionHeightChanged(uint value)
    {
        if (!_eventsEnabled) return;
        
        _configManager.GetConfig<ConversionConfig>()!.CustomResolutionHeight = value;
    }

    partial void OnCustomResolutionWidthChanged(uint value)
    {
        if (!_eventsEnabled) return;
        
        _configManager.GetConfig<ConversionConfig>()!.CustomResolutionWidth = value;
    }

    partial void OnAllowOverwriteChanged(bool value)
    {
        if (!_eventsEnabled) return;
        
        _configManager.GetConfig<ConversionConfig>()!.AllowOverwrite = value;
    }

    private void PopulateFromConfig(ConversionConfig config)
    {
        _eventsEnabled = false;
        
        if (VideoCodecs == null || AudioCodecs == null)
            RefreshCodecLists();

        CustomVideoCodecName = config.CodecVideo;
        CustomAudioCodecName = config.CodecAudio;
        CustomContainerName = config.CustomContainerName;
        CustomResolutionWidth = config.CustomResolutionWidth;
        CustomResolutionHeight = config.CustomResolutionHeight;
        
        SelectedVideoCodecTab = config.CodecVideo switch
        {
            "prores" => "VcProres",
            "cfhd" => "VcCineform",
            _ => "VcOther"
        };
        SelectedAudioCodecTab = config.CodecAudio switch
        {
            "pcm_s16le" => "AcPcms16le",
            "pcm_s32le" => "AcPcms32le",
            "" => "AcNoAudio",
            _ => "AcOther"
        };
        IsOtherVideoCodecSelected = SelectedVideoCodecTab == "VcOther" || config.UseCustomEncodingSettings;
        IsOtherAudioCodecSelected = SelectedAudioCodecTab == "AcOther";
        
        if (config.UseCustomEncodingSettings)
            SelectedVideoCodecTab = "VcOther";
        
        // Enable events since there are some checks there
        _eventsEnabled = true;
        IsOutputToSelectedDirSelected = !config.OutputBesideOriginals;
        SelectedOutputDirectoryMethod = config.OutputBesideOriginals ? "OutputToSameDir" : "OutputToSelectedDir";
        SelectedOutputDirectory = config.OutputDirectory;
        FilenamePattern = config.OutputFilenamePattern;
        AllowOverwrite = config.AllowOverwrite;
    }

    public void RefreshCodecLists()
    {
        VideoCodecs = new ObservableCollection<CodecEntry>(_converterService.GetAvailableVideoCodecs().Where(x => x.EncodingSupported));
        AudioCodecs = new ObservableCollection<CodecEntry>(_converterService.GetAvailableAudioCodecs().Where(x => x.EncodingSupported));
        Containers = new ObservableCollection<CodecEntry>(_converterService.GetAvailableContainers().Where(x => x.EncodingSupported));
    }

    [RelayCommand]
    private async Task BrowseOutputDirectory()
    {
        var firstInputVideo = _videoPoolManager.VideoPool.FirstOrDefault();
        var suggestedStartLocation = !string.IsNullOrEmpty(SelectedOutputDirectory) && Directory.Exists(SelectedOutputDirectory)
            ? SelectedOutputDirectory
            : firstInputVideo != null
                ? Path.GetDirectoryName(firstInputVideo.InputVideoInfo.Filename)
                : null;

        var storageProvider = _storageServiceProvider!.GetStorageProvider();
        var selectedDirectory = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            AllowMultiple = false,
            SuggestedStartLocation = suggestedStartLocation != null
                ? await storageProvider.TryGetFolderFromPathAsync(new Uri(suggestedStartLocation))
                : await storageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Videos),
            Title = "Select Output Directory"
        });
        
        if (selectedDirectory.Count > 0)
            SelectedOutputDirectory = selectedDirectory[0].TryGetLocalPath()!;
        
    }

    [RelayCommand]
    private void OpenOutputDirectory()
    {
        try
        {
            if (Directory.Exists(SelectedOutputDirectory))
            {
                var launcher = _storageServiceProvider!.GetLauncher();
                launcher.LaunchUriAsync(new Uri(SelectedOutputDirectory));
            }
        }
        catch (Exception)
        {
            
        }
        
        
    }
}