using BymlLibrary;
using BymlLibrary.Nodes.Containers;
using CommunityToolkit.HighPerformance.Helpers;
using ImGuiNET;
using NativeFileDialogSharp;
using Raylib_cs;
using RayLight.NintendoFormats;
using rlImGui_cs;
using SharpYaml.Model;
using System.Numerics;
using System.Reflection;
using System.Runtime;
using System.Xml.Linq;

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
                    BymlMap map = SelectedByaml.byml.GetMap();
                    Dictionary<string, Byml> replacements = new();
                    foreach (var entry in map)
                    {
                        replacements[entry.Key] = RenderNode(new string[] { "" }, entry.Key, entry.Value);
                    }
                    foreach (var replacement in replacements)
                    {
                        map[replacement.Key] = replacement.Value;
                    }
                    SelectedByaml.byml = new Byml(map);
                }
            }
            ImGui.End();
        }


        /// <summary>
        /// Recursively renders a byaml node and its children as a tree view in ImGui. The path parameter is used to keep track of the current node's path in the byaml hierarchy.
        /// </summary>
        /// <param name="path">Current Route Taken (For Flagging)</param>
        /// <param name="node"></param>
        private static Byml RenderNode(string[] path, string Name, Byml node)
        {
            string nodePath = string.Join("/", path);
            Byml Outnode = node;
            switch (node.Type)
            {

                case BymlLibrary.BymlNodeType.String:
                    ImGui.Text($"{Name}:");
                    ImGui.SameLine();
                    string StrValue = (string)node.GetString();
                    ImGui.InputText($"##{nodePath}/{Name}", ref StrValue, 1024);
                    Outnode = new Byml(StrValue);

                    
                    break;
                case BymlNodeType.Int:
                    ImGui.Text($"{Name}:");
                    ImGui.SameLine();
                    int IntValue = (int)node.GetInt();
                    ImGui.InputInt($"##{nodePath}/{Name}", ref IntValue);
                    Outnode = new Byml(IntValue);
                    break;
                
                case BymlNodeType.Float:
                    ImGui.Text($"{Name}:");
                    ImGui.SameLine();
                    float FloatValue = (float)node.GetFloat();
                    ImGui.InputFloat($"##{nodePath}/{Name}", ref FloatValue);
                    Outnode = new Byml(FloatValue);
                    break;

                case BymlNodeType.Bool:
                    ImGui.Text($"{Name}:");
                    ImGui.SameLine();
                    bool BoolValue = (bool)node.GetBool();
                    ImGui.Checkbox($"##{nodePath}/{Name}", ref BoolValue);
                    Outnode = new Byml(BoolValue);
                    break;

                case BymlLibrary.BymlNodeType.Array:
                    if (ImGui.TreeNode($"{Name} ##{nodePath}"))
                    {
                        BymlArray array = (BymlArray)node.Value;
                        for (int i = 0; i < array.Count(); i++)
                        {
                            array[i] = RenderNode(path.Append(Name).ToArray(), $"[{i}]", array[i]);
                        }
                        ImGui.TreePop();
                        Outnode = new Byml(array);
                    }
                    
                    break;
                case BymlLibrary.BymlNodeType.Map:
                    if (ImGui.TreeNode($"{Name} ##{nodePath}"))                    
                    {
                        BymlMap map = (BymlMap)node.Value;
                        Dictionary<string, Byml> replacements = new();
                        foreach (var entry in map)
                        {
                            replacements[entry.Key] = RenderNode(path.Append(Name).ToArray(), entry.Key, entry.Value);
                        }
                        foreach (var replacement in replacements)
                        {
                            map[replacement.Key] = replacement.Value;
                        }
                        ImGui.TreePop();
                        Outnode = new Byml(map);
                    }
                    break;
                    
                default:
                    ImGui.Text($"{Name}: {node.Type.ToString()}");
                    break;

                 

                        
                        
             
            }
            return Outnode;
        }
            
    }
}