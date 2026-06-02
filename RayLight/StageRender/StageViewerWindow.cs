    
using RayLight.NintendoFormats;
using ImGuiNET;
using RayLight.SceneView;
using Raylib_cs;
using System.Numerics;
using Nintendo.Bfres.WiiU;
using Binary_Stream;


namespace RayLight.Windows
{
    internal class StageViewer
    {

        private static Dictionary<string, GenericLoader> LoaderClasses = new Dictionary<string, GenericLoader>
        {
            {"Generic",new GenericLoader()},
            {"Generic2",new GenericLoader()}
        };


        public static void RenderEditor(WindowState windowState)
        {
            SceneManager EditorState = windowState.SceneManager;

            if (ImGui.Begin("Stage Viewer", ref EditorState.LoaderWindow, ImGuiWindowFlags.MenuBar))
            {
                GenericLoader loader = LoaderClasses[EditorState.SceneLoaderType];

                if (ImGui.BeginCombo("Select Item", EditorState.SceneLoaderType))
                {
                    foreach (var loader1 in LoaderClasses)
                    {
                        if (ImGui.Selectable(loader1.Key))
                        {
                            EditorState.SceneLoaderType = loader1.Key;
                            loader = LoaderClasses[loader1.Key];
                            loader.OnSwitch(EditorState);
                        }
                        if (loader1.Key == EditorState.SceneLoaderType)
                        {
                            ImGui.SetItemDefaultFocus();
                        }
                    }
                    ImGui.EndCombo();
                }
                
                loader.WindowContent(EditorState.LoaderArgs);

                if (ImGui.Button("Render"))
                {
                    EditorState.Scene = loader.Render(EditorState.Scene);
                }

                ImGui.End();
            }
        }
    }

    internal class GenericLoader
    {
        
        public static Dictionary<string, string> default_args = new Dictionary<string, string>
        {
            { "World", "Hello" },
        };

        public virtual void OnSwitch(SceneManager EditorState)
        {
            EditorState.LoaderArgs = default_args;
        }

        public virtual void WindowContent(Dictionary<string,string> args)
        {
            string world = args["World"];
            ImGui.InputText("World", ref world, 255 );
            args["World"] = world;
        }

        public unsafe virtual SceneObject[] Render(SceneObject[] scene)
        {
            /* This code lives as a cautionary tale of what NOT to do.
            This will unload every texture, INCLUDING THE DEFAULT WHITE. */
            foreach (SceneObject obj in scene)
            {
                for (int i = 0; i < obj.Renderer.MaterialCount; i++)
                {
                    Texture2D tex = obj.Renderer.Materials[i].Maps[0].Texture;
                    if (tex.Id > 1 )
                    {
                        Raylib.UnloadTexture(tex);
                    }
                }
            }
            

            return new SceneObject[]{};
        }
    }

}