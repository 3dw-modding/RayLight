
using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using NativeFileDialogSharp;
using System.Reflection;
using RayLight.Windows;

namespace RayLight
{
    internal class BaseUIWindows
    {

        //Vars
        private string FileBrowserPath = Directory.GetCurrentDirectory();
        private string UserFileBrowserPath = Directory.GetCurrentDirectory();
        private string UserSearch = "";

        //Windows
        public void RenderMenuBar(WindowState windowState)
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Exit"))
                    {
                        rlImGui.Shutdown();
                        Raylib.CloseWindow();
                        Environment.Exit(0);
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("View" ))
                {
                    if (ImGui.MenuItem(windowState.FileWindowOpen ? "File Browser " + IconFonts.FontAwesome6.Check : "File Browser "))
                    {
                        windowState.FileWindowOpen = !windowState.FileWindowOpen;
                    }
                    if (ImGui.MenuItem(windowState.SZSViewerOpen ? "SZS Editor " + IconFonts.FontAwesome6.Check : "SZS Editor "))
                    {
                        windowState.SZSViewerOpen = !windowState.SZSViewerOpen;
                    }
                    if (ImGui.MenuItem(windowState.AboutWindowOpen ? "About " + IconFonts.FontAwesome6.Check : "About " ))
                    {
                        windowState.AboutWindowOpen = !windowState.AboutWindowOpen;
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }
        }

        public void RenderAboutMenu(WindowState windowState)
        {
            if (ImGui.Begin("About", ref windowState.AboutWindowOpen))
            {
                
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));
                ImGui.SetWindowFontScale(1.5f);   // 1.0 = normal, 1.5 = 150%

                ImGui.Text("RayLight!\n");

                ImGui.SetWindowFontScale(1.0f);   // reset
                ImGui.PopStyleVar();

                string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
                ImGui.Text($"Version: {version}");
                ImGui.TextWrapped($"An experemental level editor for Mario 3D World and Bowser's Fury");

                ImGui.End();
            }
        }

        public void RenderFileBrowser(WindowState windowState)
        {
            if (ImGui.Begin("Files",ref windowState.FileWindowOpen ,ImGuiWindowFlags.MenuBar))
            {

                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.BeginMenu("File"))
                    {
                        if (ImGui.MenuItem("Open Directory"))
                        {
                            Console.WriteLine("Open clicked");
                            var result = NativeFileDialogSharp.Dialog.FolderPicker(FileBrowserPath);
                            if (result.IsOk)
                            {
                                FileBrowserPath = result.Path;
                                UserFileBrowserPath = result.Path;
                            }
                        }
                        ImGui.EndMenu();
                    }

                    ImGui.EndMenuBar();
                }


                // Path entry
                ImGui.Text("Path:");
                ImGui.SameLine();
                if (ImGui.InputText("##Path", ref UserFileBrowserPath, 260, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    if (Directory.Exists(UserFileBrowserPath))
                        FileBrowserPath = UserFileBrowserPath;

                    UserFileBrowserPath = FileBrowserPath;
                }

                ImGui.Spacing();
                ImGui.Text("Files:");

                ImGui.SameLine();
                ImGui.InputText("##FileFilter", ref UserSearch, 260);


                // Up directory
                var parent = Directory.GetParent(FileBrowserPath);
                if (parent != null && ImGui.Button(".."))
                {
                    FileBrowserPath = parent.FullName;
                    UserFileBrowserPath = parent.FullName;
                }

                ImGui.BeginChild("FileList", new Vector2(0, 0));

                // Directories
                foreach (string dir in Directory.GetDirectories(FileBrowserPath))
                {
                    string name = Path.GetFileName(dir);
                    if (ImGui.Button(name))
                    {
                        FileBrowserPath = dir;
                        UserFileBrowserPath = dir;
                    }
                }

                // Files
                foreach (string file in Directory.GetFiles(FileBrowserPath))
                {
                    string name = Path.GetFileName(file);

                    if(!name.ToLower().Contains(UserSearch.ToLower())) continue;

                    bool isSupported = SupportedFormats.isSupported(Path.GetExtension(file));

                    if (!isSupported) StyleData.ColourButtonRed();
                    else StyleData.ColourButtonGreen();

                    if (ImGui.Button(name))
                    {
                        if (isSupported)
                        {
                            SupportedFormats.HandleFile(file,windowState);
                        }
                    }

                    
                    ImGui.PopStyleColor(3);

                    ImGui.SameLine();
                    
                    if (ImGui.Button(IconFonts.FontAwesome6.Clipboard + $"##{name}"))
                    {
                        ImGui.SetClipboardText(file);
                    }
                    if (ImGui.BeginItemTooltip())
                    {
                        ImGui.Text("Copy Path");
                        ImGui.EndTooltip();
                    }
                }

                ImGui.EndChild();
                
            }
            ImGui.End();
        }
    }
}
