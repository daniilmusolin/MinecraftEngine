using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using MinecraftEngine.Engine.Core;

namespace MinecraftEngine;

public static class Program {
    private static void Main() {
        Console.WriteLine("=== PROGRAM START ===");
        
        try {
            Console.WriteLine("Creating window settings...");
            var gameSettings = GameWindowSettings.Default;
            
            var nativeSettings = new NativeWindowSettings {
                ClientSize = new(1920, 1080),
                WindowState = WindowState.Normal,
                Title = "Minecraft Engine",
                APIVersion = new(4, 6),
                Profile = ContextProfile.Core,
                Flags = ContextFlags.ForwardCompatible,
                NumberOfSamples = 4
            };

            Console.WriteLine("Creating game instance...");
            using var game = new Game(gameSettings, nativeSettings);
            
            Console.WriteLine("Starting game loop...");
            game.Run();
            
            Console.WriteLine("=== PROGRAM END ===");
        } catch (Exception ex) {
            Console.WriteLine($"!!! FATAL ERROR: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Console.ReadKey();
        }
    }
}