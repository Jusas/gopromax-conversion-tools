using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace VideoConversionApp.Abstractions;

/// <summary>
/// A helper for retrieving the Avalonia StorageProvider from ViewModels.
/// </summary>
public interface IStorageServiceProvider
{
    IStorageProvider GetStorageProvider();
    ILauncher GetLauncher();
}