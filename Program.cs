using Avalonia;
using System;
using DotNetEnv;
using ReactiveUI; // Ensure this is imported
using Avalonia.ReactiveUI; // Ensure this is imported for UseReactiveUI

namespace shoppro;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Env.Load(); // Load .env file before anything else

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace() // REMOVE THE SEMICOLON HERE!
            .UseReactiveUI(); // This will now correctly be chained and executed
}