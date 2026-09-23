using Avalonia;
using Avalonia.Media;
using System;
using System.Linq;

namespace KULMS.Local;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) 
    {
        if (OperatingSystem.IsLinux() && !args.Contains("--avalonia-use-x11"))
        {
            args = args.Append("--avalonia-use-x11").ToArray();
        }
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new X11PlatformOptions{})
            .WithInterFont()
            .With(new FontManagerOptions
            {
                FontFallbacks =
            [
                new FontFallback { FontFamily = new FontFamily("Noto Sans CJK JP") },
                new FontFallback { FontFamily = new FontFamily("Segoe UI") },
                new FontFallback { FontFamily = new FontFamily("Hiragino Sans") },
                new FontFallback { FontFamily = new FontFamily("Ubuntu") },
                new FontFallback { FontFamily = new FontFamily("Meiryo") },
            ]
            })
            .LogToTrace();
}
