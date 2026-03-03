using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using NativeFileDialogSharp;
using System.Reflection;
using RayLight.NintendoFormats;
using System.Xml.Linq;
using System.Runtime;

namespace RayLight.Windows
{
    internal class ByamlEditorState
    {
        public bool Open = true;

        public List<ByamlContainer> loadedByaml = new List<ByamlContainer>();
        public int selectedByamlIndex = 0;
        public List<object> ByamlNodePath = new List<object>();
        
    }

    internal class ByamlEditor
    {
        public static void RenderEditor(WindowState windowState)
        {
            ByamlEditorState state = windowState.BymlEditorState;
            if (state.loadedByaml.Count == 0)
                return;

            state.selectedByamlIndex = state.selectedByamlIndex % state.loadedByaml.Count;

            ByamlContainer SelectedByaml = state.loadedByaml[state.selectedByamlIndex];


            if (ImGui.Begin("BYAML Editor", ref state.Open, ImGuiWindowFlags.MenuBar)) { 

                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.BeginMenu("File"))
                    {
                        if (ImGui.MenuItem("Save"))
                        {
                            SelectedByaml.Save();
                        }
                        
                        if (ImGui.MenuItem("Reload"))
                        {
                            SelectedByaml.Reload();
                        }
                        if (ImGui.MenuItem("Close"))
                        {
                            windowState.BymlEditorState.loadedByaml.Remove(SelectedByaml);
                            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);

                        }
                        ImGui.EndMenu();
                    }

                    ImGui.EndMenuBar();
                }
            }
            ImGui.End();
        }
    }
}