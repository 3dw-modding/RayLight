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
using Nintendo.Aamp;
using Aamp.Security.Cryptography;

namespace RayLight.Windows
{
    internal class AampEditorState
    {
        public bool Open = true;

        public List<AampContainer> loadedAamps = new List<AampContainer>();
        public int selectedAampIndex = 0;

        public int selectedParam = 0;

        public Dictionary<string, string> AAmpParamiterGuessedNames = new Dictionary<string, string>()
        {
        };

        public AampEditorState()
        {
            DateTime StartTime = DateTime.Now;

            string[] paramFile = File.ReadAllLines("Assets/3DWAAMPParamNames.txt");
            foreach (string line in paramFile)
            {
                string[] parts = line.Split(":");
                if (parts.Length == 1)
                {
                    string name = parts[0].Trim();
                    string hash = Crc32.Compute(name).ToString();
                    AAmpParamiterGuessedNames[hash] = name;
                }
                else
                {
                    string name = parts[0].Trim();
                    int count = int.Parse(parts[2].Trim());

                    for (int number = 0; number < count; number++) 
                    {
                        string numberedName = name + number.ToString(parts[1].Trim());
                        string hash = Crc32.Compute(numberedName).ToString();
                        AAmpParamiterGuessedNames[hash] = numberedName;
                    }
                }

                
            };
            DateTime EndTime = DateTime.Now;
            TimeSpan LoadTime = EndTime - StartTime;
            Console.WriteLine($"Loaded {AAmpParamiterGuessedNames.Count} AAMP Param in {LoadTime.TotalSeconds} seconds");
        }

        
        
    }

    internal class AampEditor
    {
        
        //Function to pritty up the param names.
        public static string FancyName (string name)
        {
            string facnyName = "";
            for(int i = 0; i < name.Length-1; i++)
            {
                string charater = name[i].ToString();
                string nextCharater = name[i+1].ToString();
                if (charater == "_")
                {
                    facnyName += " ";
                }
                //If the next charater is uppercase but the current one isnt, add a space.
                else if (name[i].ToString().ToLower() == name[i].ToString() && nextCharater.ToString().ToUpper() == nextCharater.ToString())
                {
                    facnyName += $"{charater} ";
                }
                else
                {
                    facnyName += charater;
                }
            }
            facnyName += name[name.Length - 1].ToString();
            return facnyName;
        }

        public static void RenderEditor(WindowState windowState)
        {
            AampEditorState state = windowState.AampEditorState;
            if (state.loadedAamps.Count == 0)
                return;

            state.selectedAampIndex = state.selectedAampIndex % state.loadedAamps.Count;

            AampContainer SelectedAamp = state.loadedAamps[state.selectedAampIndex];


            if (ImGui.Begin("AAMP Editor", ref state.Open, ImGuiWindowFlags.MenuBar)) { 

                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.BeginMenu("File"))
                    {
                        if (ImGui.MenuItem("Save"))
                        {
                            SelectedAamp.Save();
                        }
                        
                        if (ImGui.MenuItem("Reload"))
                        {
                            SelectedAamp.Reload();
                        }
                        if (ImGui.MenuItem("Close"))
                        {
                            windowState.AampEditorState.loadedAamps.Remove(SelectedAamp);
                            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);

                        }
                        ImGui.EndMenu();
                    }

                    ImGui.EndMenuBar();
                }

                if (ImGui.BeginTabBar("Loaded_AAMP_TAB_BAR"))
                {
                    for (int i = 0; i < windowState.AampEditorState.loadedAamps.Count(); i++)
                    {
                        if (ImGui.BeginTabItem(windowState.AampEditorState.loadedAamps[i].GetName()))
                        {
                            windowState.AampEditorState.selectedAampIndex = i;
                            //EditorState.ExportFile = null;  //Close export window.
                            ImGui.EndTabItem();
                        }
                    }

                    ImGui.EndTabBar();
                }

                // Calculate half width
                float half = ImGui.GetContentRegionAvail().X * 0.3f;

                // LEFT PANEL
                ImGui.BeginChild("LeftPanel", new Vector2(half, 0));

                for (int i = 0; i < SelectedAamp.aamp.RootNode.ParamObjects.Count(); i++)
                {
                    ParamObject param = SelectedAamp.aamp.RootNode.ParamObjects[i];

                    string Name = "";
                    foreach (ParamEntry paramObject in param.ParamEntries)
                    {
                        if (paramObject.HashString == "Name")
                        {
                            Name = ((Nintendo.Aamp.StringEntry)paramObject.Value).ToString();
                            break;
                        }
                    }
                    if (ImGui.Button($"{i.ToString()}: {Name}"))
                    {
                        state.selectedParam = i;
                    }
                }

                ImGui.EndChild();

                // RIGHT PANEL
                ImGui.SameLine();

                ImGui.BeginChild("RightPanel", new Vector2(0, 0));

                    if (state.selectedParam < SelectedAamp.aamp.RootNode.ParamObjects.Count())
                    {
                        ParamObject param = SelectedAamp.aamp.RootNode.ParamObjects[state.selectedParam];
    
                        foreach (ParamEntry paramObject in param.ParamEntries)
                        {
                            string paramName = "";

                            if (state.AAmpParamiterGuessedNames.ContainsKey(paramObject.Hash.ToString()))
                            {
                                string guessedName = state.AAmpParamiterGuessedNames[paramObject.Hash.ToString()];
                                if (guessedName == paramObject.HashString) paramName = guessedName;
                                else paramName = $"[{guessedName}]";
                            }
                            else
                            {
                                paramName = paramObject.HashString;
                            }
                            
                            paramName = FancyName(paramName);

                            ImGui.Text($"{paramName}");
                            ImGui.SameLine();

                            ParamType Type = paramObject.ParamType;
                            switch (paramObject.ParamType)
                            {
                                case ParamType.String64:
                                case ParamType.String256:
                                case ParamType.String32:
                                    string value = ((Nintendo.Aamp.StringEntry)paramObject.Value).ToString();
                                    if (ImGui.InputText($"##{paramObject.HashString}", ref value, 255))
                                    {
                                        paramObject.Value = new Nintendo.Aamp.StringEntry(value);
                                    }
                                    break;
                                case ParamType.Float:
                                    float floatValue = (float)paramObject.Value;
                                    if (ImGui.InputFloat($"##{paramObject.HashString}", ref floatValue))
                                    {
                                        paramObject.Value = floatValue;
                                    }
                                    break;
                                case ParamType.Boolean:
                                    bool boolValue = (bool)paramObject.Value;
                                    if (ImGui.Checkbox($"##{paramObject.HashString}", ref boolValue))
                                    {
                                        paramObject.Value = boolValue;
                                    }
                                    break;
                                case ParamType.Int:
                                    int intValue = (int)paramObject.Value;
                                    if (ImGui.InputInt($"##{paramObject.HashString}", ref intValue))
                                    {
                                        paramObject.Value = intValue;
                                    }
                                    break;
                                case ParamType.Color4F:
                                    Color4F colorValue = (Color4F)paramObject.Value;
                                    Vector4 colorVec = new Vector4(colorValue.R, colorValue.G, colorValue.B, colorValue.A);
                                    if (ImGui.ColorEdit4($"##{paramObject.HashString}", ref colorVec, ImGuiColorEditFlags.HDR))
                                    {
                                        paramObject.Value = new Color4F(colorVec.X, colorVec.Y, colorVec.Z, colorVec.W);
                                    }
                                    break;
                                case ParamType.Vector3F:
                                    Syroot.Maths.Vector3F vec3Value = (Syroot.Maths.Vector3F)paramObject.Value;
                                    Vector3 vec3 = new Vector3(vec3Value.X, vec3Value.Y, vec3Value.Z);
                                    if (ImGui.InputFloat3($"##{paramObject.HashString}", ref vec3))
                                    {
                                        paramObject.Value = new Syroot.Maths.Vector3F(vec3.X, vec3.Y, vec3.Z);
                                    }
                                    break;
                                case ParamType.Vector2F:
                                    Syroot.Maths.Vector2F vec2Value = (Syroot.Maths.Vector2F)paramObject.Value;
                                    Vector2 vec2 = new Vector2(vec2Value.X, vec2Value.Y);
                                    if (ImGui.InputFloat2($"##{paramObject.HashString}", ref vec2))
                                    {
                                        paramObject.Value = new Syroot.Maths.Vector2F(vec2.X, vec2.Y);
                                    }
                                    break;
                                case ParamType.Vector4F:
                                    Syroot.Maths.Vector4F vec4Value = (Syroot.Maths.Vector4F)paramObject.Value;
                                    Vector4 vec4 = new Vector4(vec4Value.X, vec4Value.Y, vec4Value.Z, vec4Value.W);
                                    if (ImGui.InputFloat4($"##{paramObject.HashString}", ref vec4))
                                    {
                                        paramObject.Value = new Syroot.Maths.Vector4F(vec4.X, vec4.Y, vec4.Z, vec4.W);
                                    }
                                    break;
                                default:
                                    ImGui.Text($"Unsupported Type {Type.ToString()}");
                                    break;
                            }
                        }
                    }

                ImGui.EndChild();

                ImGui.End();
            }
        }
    }
}