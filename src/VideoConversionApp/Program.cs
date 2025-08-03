using Avalonia;
using System;

namespace VideoConversionApp;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
            if (App.Logger != null)
            {
                App.Logger.LogInformation("Application exiting");
                App.Logger.Flush();
            }
        }
        catch (Exception e)
        {
            var message = $"Main: Unhandled exception was thrown, exiting... {e}";
            if (App.Logger != null)
            {
                App.Logger.LogError(message);
                App.Logger.Flush();
            }
            else
                Console.Error.WriteLine(message);
        }
           
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}