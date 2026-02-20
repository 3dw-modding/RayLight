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
        public bool Open = true;

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

                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.BeginMenu("File"))
                    {
                        if (ImGui.MenuItem("Save"))
                        {
                            SelectedMSBT.Save();
                        }
                        if (ImGui.MenuItem("Reload"))
                        {
                            SelectedMSBT.Reload();
                        }
                        if (ImGui.MenuItem("Close"))
                        {
                            windowState.MSBTEditorState.loadedMSBTs.Remove(SelectedMSBT);
                            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);

                        }
                        ImGui.EndMenu();
                    }

                    ImGui.EndMenuBar();
                }

                if (ImGui.BeginTabBar("Loaded_SZS_TAB_BAR"))
                {
                    for (int i = 0; i < windowState.MSBTEditorState.loadedMSBTs.Count(); i++)
                    {
                        if (ImGui.BeginTabItem(windowState.MSBTEditorState.loadedMSBTs[i].GetName()))
                        {
                            windowState.MSBTEditorState.selectedMSBTIndex = i;
                            //EditorState.ExportFile = null;  //Close export window.
                            ImGui.EndTabItem();
                        }
                    }

                    ImGui.EndTabBar();
                }

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

                string EditingText = "";
                MsbtEntry EditingValue;
                if(state.SelectedMSBTKey != null){
                    if(texts.TryGetValue(state.SelectedMSBTKey,out EditingValue))
                    {
                        EditingText = EditingValue.Value;
                    }
                }

                

                // Multi-line text editor for selected item
                if (state.SelectedMSBTKey != null)
                {
                    ImGui.InputTextMultiline(
                        "##editor",
                        ref EditingText,
                        4096,
                        new Vector2(-1, -1)   // fill remaining space
                    );
                }

                if(state.SelectedMSBTKey != null){
                    if(texts.TryGetValue(state.SelectedMSBTKey,out EditingValue))
                    {
                        if (EditingText != EditingValue.Value)
                        {
                            //Console.WriteLine($"replaceing {EditingValue.Value} with {EditingText}");
                            
                            texts[state.SelectedMSBTKey] = new MsbtEntry(texts[state.SelectedMSBTKey].Attribute,EditingText);
                            SelectedMSBT.msbt.SetTexts(texts);
                        }
                    }
                }

                ImGui.EndChild();

                ImGui.End();
            }
        }
    }
}
