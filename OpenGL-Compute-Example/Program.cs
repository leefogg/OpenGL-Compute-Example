using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;

namespace OpenGL_Compute_Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var gameWindowSettings = new GameWindowSettings()
            {
                UpdateFrequency = -1,
                RenderFrequency = -1,
            };
            var nativeWindowSettings = new NativeWindowSettings
            {
                API = ContextAPI.OpenGL,
                APIVersion = new Version(4, 3),
                Profile = ContextProfile.Core,
                Flags = ContextFlags.Debug,
                Size = new Vector2i(1920, 1080),
                IsEventDriven = false,
                Title = "Compute Example",
            };
            using var window = new Window(gameWindowSettings, nativeWindowSettings);
            //window.VSync = VSyncMode.Off;
            window.Run();
        }
    }
}
