//dotnet add package AssimpNet
using Assimp;

namespace OpentkGraphics
{
    public class OBJModel
    {
        public Vector3D[] Vertices { get; set; }
        public int VertexCount { get; set; }

        public static OBJModel LoadObjFile(string filePath)
        {
            OBJModel model = new OBJModel();
            AssimpContext context = new AssimpContext();
            Scene scene = context.ImportFile(filePath, PostProcessSteps.Triangulate);

            if (scene == null || scene.MeshCount == 0)
            {
                Console.WriteLine("Failed to load OBJ file " + filePath);
                return model;
            }

            Mesh mesh = scene.Meshes[0];
            model.VertexCount = mesh.VertexCount;

            if (mesh.HasVertices)
            {
                model.Vertices = new Vector3D[model.VertexCount];
                Array.Copy(mesh.Vertices.ToArray(), model.Vertices, model.VertexCount);
            }

            Console.WriteLine("Loaded " + filePath);
            return model;
        }
        public static float[] VecToFloat(OBJModel model)
        {
            List<float> v = new List<float>();
            foreach (var VARIABLE in model.Vertices)
            {
                v.Add(VARIABLE.X);
                v.Add(VARIABLE.Y);
                v.Add(VARIABLE.Z);
            }
            return v.ToArray();
        }
        public static float[] VecToFloat(List<OpenTK.Mathematics.Vector3> model)
        {
            List<float> v = new List<float>();
            foreach (var VARIABLE in model)
            {
                v.Add(VARIABLE.X);
                v.Add(VARIABLE.Y);
                v.Add(VARIABLE.Z);
            }
            return v.ToArray();
        }
    }
}
