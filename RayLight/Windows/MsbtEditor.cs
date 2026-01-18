using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using NativeFileDialogSharp;
using System.Reflection;
using RayLight.NintendoFormats;
using System.Xml.Linq;
using System.Runtime;
using MsbtLib;

namespace RayLight.Windows
{
    internal class MsbtEditorState
    {
        public bool Open = false;

        public List<MSBTFile> loadedMSBTs = new List<MSBTFile>();
        public int selectedMSBTIndex = 0;
        public string SelectedMSBTKey = null;
    }

    internal class MsbtEditor
    {
        public static void RenderEditor(WindowState windowState)
        {
            MsbtEditorState state = windowState.MSBTEditorState;
            if (state.loadedMSBTs.Count == 0)
                return;

            state.selectedMSBTIndex = state.selectedMSBTIndex % state.loadedMSBTs.Count;

            MSBTFile SelectedMSBT = state.loadedMSBTs[state.selectedMSBTIndex];
            Dictionary<string, MsbtEntry> texts = SelectedMSBT.msbt.GetTexts();


            if (ImGui.Begin("MSBT Editor", ref state.Open, ImGuiWindowFlags.MenuBar)) { 

                

                // Calculate half width
                float half = ImGui.GetContentRegionAvail().X * 0.5f;

                // LEFT PANEL
                ImGui.BeginChild("LeftPanel", new Vector2(half, 0));

                // Your button list
                foreach (var item in texts)
                {
                    if (ImGui.Button(item.Key))
                        state.SelectedMSBTKey = item.Key;
                }

                ImGui.EndChild();

                // RIGHT PANEL
                ImGui.SameLine();

                ImGui.BeginChild("RightPanel", new Vector2(0, 0));

                string blank = "";

                // Multi-line text editor for selected item
                if (state.SelectedMSBTKey != null)
                {
                    ImGui.InputTextMultiline(
                        "##editor",
                        ref blank,
                        4096,
                        new Vector2(-1, -1)   // fill remaining space
                    );
                }

                ImGui.EndChild();

                ImGui.End();
            }
        }
    }
}
