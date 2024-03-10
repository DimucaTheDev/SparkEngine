using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using static System.Windows.Forms.LinkLabel;

namespace SparkEngine
{
    internal class GameObject
    {
        public static List<GameObject> Models=new(); //todo

        public string Name = $"OBJ_{Random.Shared.Next(0,999).ToString("000")}";
        public Transform Transform = new();
        public int VBO, VAO, TVBO, EBO;
        public List<uint> indices = new();
        public List<Vector3> vert = new();
        public List<int> texCoords = new();
        public bool Sinusoida;

        public GameObject LoadModelData(string objFile)
        {
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";
            foreach (var _line in File.ReadAllLines(objFile).Where(l => l.StartsWith("v") || l.StartsWith("f")).ToList())
            {
                var line = _line;
                if (line.Split(" ")[0] == "v")
                    vert.Add(new(
                        float.Parse(line.Split(" ")[1].ReplaceLineEndings("0"), NumberStyles.Any, ci),
                        float.Parse(line.Split(" ")[2].ReplaceLineEndings("0"), NumberStyles.Any, ci),
                        float.Parse(line.Split(" ")[3].ReplaceLineEndings("0"), NumberStyles.Any, ci)));

                if (line.StartsWith("f"))
                {
                    if (line.Contains("/")) //new
                    {
                        line = line.Replace("//", "/");
                        //textured = true;
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

            return this;
        }
    }
}
