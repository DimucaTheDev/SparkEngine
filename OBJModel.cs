using OpenTK.Mathematics;
using System.Globalization;

namespace OpentkGraphics
{
    public class OBJModel
    {
        private static CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        static OBJModel()  {
            ci.NumberFormat.NumberDecimalSeparator = ".";
        }
        public static void Load(string path, List<Vector3> vertices, List<uint> indices, List<int> texCoords)
        {
            List<string> lines = File.ReadAllLines(path).Where(l => l.StartsWith("v") || l.StartsWith("f")).ToList();
            bool textured = false;
            foreach (var line in lines)
            {
                if (line.Split(" ")[0] == "v")
                    vertices.Add(new(
                        float.Parse(line.Split(" ")[1].ReplaceLineEndings("0"), NumberStyles.Any, ci),
                        float.Parse(line.Split(" ")[2].ReplaceLineEndings("0"), NumberStyles.Any, ci),
                        float.Parse(line.Split(" ")[3].ReplaceLineEndings("0"), NumberStyles.Any, ci)));

                if (line.StartsWith("f"))
                {
                    if (line.Contains("/")) //new
                    {
                        textured = true;
                        indices.Add(uint.Parse(line.Split(" ")[1].Split("/")[0]) - 1);
                        indices.Add(uint.Parse(line.Split(" ")[2].Split("/")[0]) - 1);
                        indices.Add(uint.Parse(line.Split(" ")[3].Split("/")[0]) - 1);

                        texCoords.Add(int.Parse(line.Split(" ")[1].Split("/")[1]) - 1);
                        texCoords.Add(int.Parse(line.Split(" ")[2].Split("/")[1]) - 1);
                        texCoords.Add(int.Parse(line.Split(" ")[3].Split("/")[1]) - 1);
                    }
                    else
                    {
                        indices.Add(uint.Parse(line.Split(" ")[1]) - 1);
                        indices.Add(uint.Parse(line.Split(" ")[2]) - 1);
                        indices.Add(uint.Parse(line.Split(" ")[3]) - 1);
                    }
                }
            }

        }
    }

}
