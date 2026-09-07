    
using RayLight.NintendoFormats;
using ImGuiNET;
using RayLight.SceneView;
using Raylib_cs;
using System.Numerics;
using Nintendo.Bfres.WiiU;
using Binary_Stream;
using BymlLibrary;
using BymlLibrary.Nodes.Containers;

namespace RayLight.SceneView
{
    internal class SM3DWLoaderNX : GenericLoader {
        new public static Dictionary<string, string> default_args = new Dictionary<string, string>
        {
            {"GameDir",""},
            {"StagePath",""}
        };

        public override void WindowContent(Dictionary<string, string> args)
        {
            string GameDir = args["GameDir"];
            ImGui.Text("Game Directory: "); ImGui.SameLine();
            ImGui.InputText("##GameDir", ref GameDir, 512 );
            args["GameDir"] = GameDir;

            string StagePath = args["StagePath"];
            ImGui.Text("Stage Path: "); ImGui.SameLine();
            ImGui.InputText("##StagePath", ref StagePath, 255 );
            args["StagePath"] = StagePath;
        }


        /*
        Heres some logical flow planning, in hopes of keeping this documented.

        Assume the Game Directory and StagePath are valid.

        Open the SZS file for the Stage, to open the MSBT file for the Map.byml

        Itterate though the object, using a dynamically built byml cashe.
        */
        public override unsafe SceneObject[] Render(SceneObject[] scene, Dictionary<string,string> args)
        {
            const float UNIT_SCALE = 0.01f;


            List<SceneObject> outputscene = base.Render(scene, args).ToList<SceneObject>();

            //Generate list of valid object models.
            string GameObjectDir = Path.Join(args["GameDir"], "ObjectData");
            string[] GameModelsPath = Directory.GetFiles(GameObjectDir);
            Dictionary<string, string> GameModels = new Dictionary<string, string>{};
            foreach(string path in GameModelsPath)
            {
                GameModels[Path.GetFileNameWithoutExtension(path)] = path;
            }

            //Load the stage
            SZSArchive Stage = new SZSArchive(args["StagePath"]);
            
            string StageName = Path.GetFileNameWithoutExtension(args["StagePath"]);
            ByamlContainer Map = null;
            foreach (LoadedFile File in Stage.files)
            {
                if (File.Name.StartsWith(StageName))
                {
                    if (File.Name.EndsWith("Map.byml")){
                        Map = new ByamlContainer(File.Data, Stage,args["StagePath"]);
                    }
                }
            }
            if (Map == null) return outputscene.ToArray();

            
            BymlArray StageItems = (BymlArray)((BymlMap)(Map.byml.Value))["Objs"].Value;

            foreach (Byml StageItem in StageItems)
            {
                if (StageItem.Type == BymlNodeType.Map)
                {
                    BymlMap StageItemMap = (BymlMap)StageItem.Value;

                    Vector3 Translate = new Vector3(0,0,0);
                    BymlMap Translation = (BymlMap)StageItemMap["Translate"].Value;
                    Translate.X = (float) Translation["X"].Value * UNIT_SCALE;
                    Translate.Y = (float) Translation["Y"].Value * UNIT_SCALE;
                    Translate.Z = (float) Translation["Z"].Value * UNIT_SCALE;

                    Vector3 Rotate = new Vector3(0,0,0);
                    BymlMap Rotation = (BymlMap)StageItemMap["Rotate"].Value;
                    Rotate.X = (float) Rotation["X"].Value;
                    Rotate.Y = (float) Rotation["Y"].Value;
                    Rotate.Z = (float) Rotation["Z"].Value;

                    Vector3 Scale = new Vector3(0,0,0);
                    BymlMap ScaleMap = (BymlMap)StageItemMap["Rotate"].Value;
                    Scale.X = (float) ScaleMap["X"].Value * UNIT_SCALE;
                    Scale.Y = (float) ScaleMap["Y"].Value * UNIT_SCALE;
                    Scale.Z = (float) ScaleMap["Z"].Value * UNIT_SCALE;

                    Mesh cubeMesh = Raylib.GenMeshCube(1.0f/UNIT_SCALE, 1.0f/UNIT_SCALE, 1.0f/UNIT_SCALE);
                    Raylib_cs.Model modelData = Raylib.LoadModelFromMesh(cubeMesh);
                    SceneObject obj = new SceneObject (modelData, Translate, Rotate, Scale);
                    outputscene.Add(obj);
                }
            }
            
            

            return outputscene.ToArray();
        }

    }
}