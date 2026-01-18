using ImGuiNET;
using Raylib_cs;
using RayLight;
using RayLight.Windows;
using rlImGui_cs;

Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.VSyncHint | ConfigFlags.ResizableWindow);
Raylib.InitWindow(1280, 800, "RayLight");

Raylib.SetTargetFPS(144);

rlImGui.Setup(true);

BaseUIWindows baseUI = new BaseUIWindows();
WindowState windowState = new WindowState();

while (!Raylib.WindowShouldClose())
{

    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.DarkGray);

    rlImGui.Begin();
    //ImGui.ShowDemoWindow();

    
    baseUI.RenderMenuBar(windowState);
    if(windowState.FileWindowOpen)baseUI.RenderFileBrowser(windowState);
    if (windowState.AboutWindowOpen) baseUI.RenderAboutMenu(windowState);

    if (windowState.SZSViewerOpen && windowState.SZSEditorState.loadedSZS.Count() > 0) SZSEditor.RenderEditor(windowState);

    rlImGui.End();
    Raylib.EndDrawing();
}

rlImGui.Shutdown();
Raylib.CloseWindow();
