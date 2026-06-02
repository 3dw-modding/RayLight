using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using RayLight.Windows;

namespace RayLight.SceneView
{

    internal class SceneObject
    {
        public Raylib_cs.Model Renderer;
        public Vector3 position;
        public SceneObject(Model model,Vector3 position, Vector3 rotation, Vector3 scale)
        {
            float deg2rad = float.Pi/180;
            this.position = position;
            Matrix4x4 rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(rotation.Y*deg2rad, rotation.X*deg2rad, rotation.Z*deg2rad);
            Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(scale);
            Renderer = model;
            Renderer.Transform = rotationMatrix * scaleMatrix;
        }
    }
    internal class SceneManager
    {
        public Freecam Camera = new Freecam();

        public bool LoaderWindow = true;
        public string SceneLoaderType = "Generic";
        public Dictionary<string, string> LoaderArgs = new Dictionary<string, string>{};

        public SceneObject[] Scene;

        public SceneManager()
        {
            Mesh cubeMesh = Raylib.GenMeshCube(1.0f, 1.0f, 1.0f);
            Raylib_cs.Model modelData = Raylib.LoadModelFromMesh(cubeMesh);
            SceneObject obj = new SceneObject (modelData, new Vector3(0,0,0), new Vector3(0,45,0), new Vector3(2,1,1));
            Scene = [obj];
            LoaderArgs = GenericLoader.default_args;
        }

            
        

        public void Update()
        {
            Camera.Update();
            Render();
        }

        public void Render()
        {
            Raylib.BeginMode3D(Camera.Camera);

            bool DrawGrid = true;

            if (DrawGrid)
            {
                int GridSize = 10;
                //Render grid
                for (int i = -GridSize; i <= GridSize; i++)
                {
                    Raylib.DrawLine3D(new Vector3(i, 0, -GridSize), new Vector3(i, 0, GridSize), Color.Gray);
                    Raylib.DrawLine3D(new Vector3(-GridSize, 0, i), new Vector3(GridSize, 0, i), Color.Gray);
                }
            }

            foreach (SceneObject obj in Scene)
            {
                Raylib.DrawModel(obj.Renderer, obj.position, 1.0f, Color.White);
            }
            

            Raylib.EndMode3D();
        }
    }

    internal class Freecam
    {
        public Camera3D Camera;
        public float Speed = 3f;
        public float FastSpeed = 5f;

        public Freecam()
        {
            Camera = new Camera3D();
            Camera.FovY = 60;
            Camera.Projection = CameraProjection.Perspective;
            Camera.Position = new Vector3(0, 5, 10);
            Camera.Target = Vector3.Zero;
            Camera.Up = Vector3.UnitY;
        }

        public void Update()
        {
            //Initalise Motion and Rotation
            Vector3 Movement = Vector3.Zero;
            Vector3 Rotation = Vector3.Zero;

            float MovementSpeed = Raylib.IsKeyDown(KeyboardKey.LeftShift) ? FastSpeed : Speed;

            //Dont update if the user is using the UI
            if (ImGui.GetIO().WantCaptureMouse) return;
            if (ImGui.GetIO().WantCaptureKeyboard) return;


            //Movement input 
            Vector3 MovementInput = Vector3.Zero;
            if (Raylib.IsKeyDown(KeyboardKey.W)) MovementInput.Y += 1;
            if (Raylib.IsKeyDown(KeyboardKey.S)) MovementInput.Y -= 1;
            if (Raylib.IsKeyDown(KeyboardKey.A)) MovementInput.X -= 1;
            if (Raylib.IsKeyDown(KeyboardKey.D)) MovementInput.X += 1;
            if (Raylib.IsKeyDown(KeyboardKey.Q)) MovementInput.Z -= 1;
            if (Raylib.IsKeyDown(KeyboardKey.E)) MovementInput.Z += 1;

            Movement = new Vector3(MovementInput.Y, MovementInput.X, MovementInput.Z) * MovementSpeed * Raylib.GetFrameTime();

            Vector2 MouseDelta = Raylib.GetMouseDelta();
            if (Raylib.IsMouseButtonDown(MouseButton.Right))
            {
                Rotation.X += MouseDelta.X * 0.2f;
                Rotation.Y += MouseDelta.Y * 0.2f;
            }

            Raylib.UpdateCameraPro(ref Camera, Movement, Rotation, 0.0f);
        }
    }
}