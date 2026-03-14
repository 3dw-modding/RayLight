using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using NativeFileDialogSharp;
using System.Reflection;
using RayLight.NintendoFormats;
using System.Xml.Linq;
using System.Runtime;
using BymlLibrary.Nodes.Containers;
using BymlLibrary;
using CommunityToolkit.HighPerformance.Helpers;

namespace RayLight.Windows
{
    internal class ByamlEditorState
    {
        public bool Open = true;

        public List<ByamlContainer> loadedByaml = new List<ByamlContainer>();
        public int selectedByamlIndex = 0;
        
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


                    //Render TreeView of Byaml
                    foreach (var entry in SelectedByaml.byml.GetMap())
                    {
                        RenderNode(new string[] { "" }, entry.Key, entry.Value);
                    }
                }
            }
            ImGui.End();
        }


        /// <summary>
        /// Recursively renders a byaml node and its children as a tree view in ImGui. The path parameter is used to keep track of the current node's path in the byaml hierarchy.
        /// </summary>
        /// <param name="path">Current Route Taken (For Flagging)</param>
        /// <param name="node"></param>
        private static void RenderNode(string[] path, string Name, Byml node)
        {
            string nodePath = string.Join("/", path);
            switch (node.Type)
            {

                case BymlLibrary.BymlNodeType.String:
                    ImGui.Text($"{Name}:");
                    ImGui.SameLine();
                    string StrValue = (string)node.GetString();
                    ImGui.InputText($"##{nodePath}/{Name}", ref StrValue, 1024);
                    
                    break;
                case BymlNodeType.Int:
                    ImGui.Text($"{Name}:");
                    ImGui.SameLine();
                    int IntValue = (int)node.GetInt();
                    ImGui.InputInt($"##{nodePath}/{Name}", ref IntValue);
                    break;
                
                case BymlNodeType.Float:
                    ImGui.Text($"{Name}:");
                    ImGui.SameLine();
                    float FloatValue = (float)node.GetFloat();
                    ImGui.InputFloat($"##{nodePath}/{Name}", ref FloatValue);
                    break;

                case BymlNodeType.Bool:
                    ImGui.Text($"{Name}:");
                    ImGui.SameLine();
                    bool BoolValue = (bool)node.GetBool();
                    ImGui.Checkbox($"##{nodePath}/{Name}", ref BoolValue);
                    break;

                case BymlLibrary.BymlNodeType.Array:
                    if (ImGui.TreeNode($"{Name} ##{nodePath}"))
                    {
                        BymlArray array = (BymlArray)node.Value;
                        int i = 0;
                        foreach (Byml entry in array)
                        {
                            RenderNode(path.Append(Name).ToArray(), $"[{i++}]", entry);
                        }
                        ImGui.TreePop();
                    }
                    break;
                case BymlLibrary.BymlNodeType.Map:
                    if (ImGui.TreeNode($"{Name} ##{nodePath}"))                    
                    {
                        BymlMap map = (BymlMap)node.Value;
                        foreach (var entry in map)                        
                        {
                            RenderNode(path.Append(Name).ToArray(), entry.Key, entry.Value);
                        }
                        ImGui.TreePop();
                    }
                    break;
                    
                default:
                    ImGui.Text($"{Name}: {node.Type.ToString()}");
                    break;
                


                        
                        
             
            }
           /*
            if(ImGui.TreeNode($"{Name} ##{nodePath}"))
            {
                ImGui.TreePop();
            }
                
            */
        }
            
    }
}