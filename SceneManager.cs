using Newtonsoft.Json;
using GL = OpenTK.Graphics.OpenGL.GL;
using static OpenTK.Graphics.OpenGL.GL;
using OpenTK.Mathematics;
using SparkEngine.Configuration;
using System.Reflection;
using OpenTK.Graphics.OpenGL;

namespace SparkEngine
{
    internal class SceneManager
    {
        private static Scene _currentScene = new();
        public static Scene CurrentScene
        {
            get => _currentScene;
            set
            {

                _currentScene = value;
                PlayerController.transform.Position2 = value.StartPosition;
            }
        }

        public static List<Scene> AllScenes = new();
        //   ????
        public static List<Scene> ReloadScenes()
        {
            AllScenes.Clear();
            List<Scene> scenes = new List<Scene>();
            Directory.GetDirectories("scenes").ToList().ForEach(dir =>
            {
                var combine = Path.Combine(dir, "levelinfo.json");
                var deserializeObject = Load(combine);
                deserializeObject.Objects.ForEach(m =>
                {
                    Program.Instance.GenModel(m);
                });
                scenes.Add(deserializeObject);
                AllScenes.Add(deserializeObject);
            });
            return scenes;
        }

        public static void Save(Scene scene, string path)
        {
            var str = JsonConvert.SerializeObject(scene);
            File.WriteAllText(path, str);
        }
        public static Scene Load(string path)
        {
            Program.ReloadRequired = true;
            var readAllText = File.ReadAllText(path);
            var scene = JsonConvert.DeserializeObject<Scene>(readAllText);
            Program.ReloadModel.AddRange(scene.Objects);
            return scene;
        }
    }

    public class Scene
    {
        public string LevelName { get; set; } = "Level";
        [JsonProperty("StartPosition")]
        public V3 __IGNORE__
        {
            get => new() { X = StartPosition.X, Y = StartPosition.Y, Z = StartPosition.Z };
            set => StartPosition = new(value.X, value.Y, value.Z);
        }

        [JsonIgnore] public Vector3 StartPosition;
        public List<GameObject> Objects { get; set; } = new();
    }
}
