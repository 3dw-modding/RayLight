using ImGuiNET;
using Raylib_cs;
using RayLight;
using RayLight.Windows;
using rlImGui_cs;

Console.WriteLine(
    System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString()
);

if (System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.Arm64)
{
    Console.WriteLine("WARNING, THIS IS RUNNING AS ARM");
    Console.WriteLine("ARM IS UNTESTED ON WINDOWS AND BROKEN UNDER MACOS");
}

Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.VSyncHint | ConfigFlags.ResizableWindow);
Raylib.InitWindow(1280, 800, "RayLight");

int displayRefreshRate = Raylib.GetMonitorRefreshRate(Raylib.GetCurrentMonitor());
Raylib.SetTargetFPS(displayRefreshRate);
Raylib.SetWindowTitle($"RayLight - {displayRefreshRate} FPS");

rlImGui.Setup(true);

BaseUIWindows baseUI = new BaseUIWindows();
WindowState windowState = new WindowState();

float FPS = 0;
float SmoothedDeltaTime = 0;

while (!Raylib.WindowShouldClose())
{
    SmoothedDeltaTime = SmoothedDeltaTime * 0.99f + Raylib.GetFrameTime() * 0.01f;
    FPS = 1.0f / SmoothedDeltaTime;

    Raylib.BeginDrawing();
    Raylib.ClearBackground(new Color(20,20,20));

    rlImGui.Begin();
    //ImGui.ShowDemoWindow();

    Raylib.SetWindowTitle($"RayLight - {FPS:F2} FPS");

    
    baseUI.RenderMenuBar(windowState);
    if(windowState.FileWindowOpen)baseUI.RenderFileBrowser(windowState);
    if (windowState.AboutWindowOpen) baseUI.RenderAboutMenu(windowState);

    if (windowState.SZSEditorState.Open && windowState.SZSEditorState.loadedSZS.Count() > 0) SZSEditor.RenderEditor(windowState);
    if (windowState.MSBTEditorState.Open && windowState.MSBTEditorState.loadedMSBTs.Count() > 0) MsbtEditor.RenderEditor(windowState);

    rlImGui.End();
    Raylib.EndDrawing();
}

rlImGui.Shutdown();
Raylib.CloseWindow();
