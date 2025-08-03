using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using VideoConversionApp.Abstractions;
using VideoConversionApp.Views;

namespace VideoConversionApp.Services;

public class StorageServiceProvider : IStorageServiceProvider
{
    private Window? _providerWindow = null;

    public StorageServiceProvider()
    {
    }

    public void UseProviderWindow(Window window)
    {
        _providerWindow = window;
    }

    public IStorageProvider GetStorageProvider()
    {
        return TopLevel.GetTopLevel(_providerWindow)!.StorageProvider;
    }

    public ILauncher GetLauncher()
    {
        return TopLevel.GetTopLevel(_providerWindow)!.Launcher;
    }
}