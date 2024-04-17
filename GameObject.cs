using System.Collections;
using System.Globalization;
using Newtonsoft.Json;
using OpenTK.Mathematics;

namespace SparkEngine;

public class GameObject
{
    public static List<GameObject> Models = new(); //todo
    [JsonProperty("Vertices")] public list __IGNORE__;
    public List<uint> Indices = new();
    public string Name = $"OBJ_{Random.Shared.Next(0, 999).ToString("000")}";
    [JsonIgnore] public bool Sinusoida;
    public List<int> TexCoords = new();
    public Transform Transform = new();
    [JsonIgnore] public int VBO, VAO, TVBO, EBO;
    [JsonIgnore] public List<Vector3> Vertices = new();

    public GameObject()
    {
        __IGNORE__ = new list { g = this };
    }

    //{
    //get => Vertices.Select(s => new V3() { X = s.X, Y = s.Y, Z = s.Z }).ToList();
    //set => Vertices = value.Select(s => new Vector3(s.X, s.Y, s.Z)).ToList();
    //}

    public GameObject LoadModelData(string objFile)
    {
        var ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        ci.NumberFormat.CurrencyDecimalSeparator = ".";
        foreach (var _line in File.ReadAllLines(objFile).Where(l => l.StartsWith("v") || l.StartsWith("f")).ToList())
        {
            var line = _line;
            if (line.Split(" ")[0] == "v")
                Vertices.Add(new Vector3(
                    float.Parse(line.Split(" ")[1].ReplaceLineEndings("0"), NumberStyles.Any, ci),
                    float.Parse(line.Split(" ")[2].ReplaceLineEndings("0"), NumberStyles.Any, ci),
                    float.Parse(line.Split(" ")[3].ReplaceLineEndings("0"), NumberStyles.Any, ci)));

            if (line.StartsWith("f"))
            {
                if (line.Contains("/")) //new
                {
                    line = line.Replace("//", "/");

                    //textured = true;
                    Indices.Add(uint.Parse(line.Split(" ")[1].Split("/")[0]) - 1);
                    Indices.Add(uint.Parse(line.Split(" ")[2].Split("/")[0]) - 1);
                    Indices.Add(uint.Parse(line.Split(" ")[3].Split("/")[0]) - 1);

                    TexCoords.Add(int.Parse(line.Split(" ")[1].Split("/")[1]) - 1);
                    TexCoords.Add(int.Parse(line.Split(" ")[2].Split("/")[1]) - 1);
                    TexCoords.Add(int.Parse(line.Split(" ")[3].Split("/")[1]) - 1);
                }
                else
                {
                    Indices.Add(uint.Parse(line.Split(" ")[1]) - 1);
                    Indices.Add(uint.Parse(line.Split(" ")[2]) - 1);
                    Indices.Add(uint.Parse(line.Split(" ")[3]) - 1);
                }
            }
        }

        return this;
    }
}
public class list : IList<V3>
{
    public GameObject g;

    /// <inheritdoc />
    public IEnumerator<V3> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc />
    public void Add(V3 item)
    {
        g.Vertices.Add(new Vector3(item.X, item.Y, item.Z));
    }

    /// <inheritdoc />
    public void Clear()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public bool Contains(V3 item)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void CopyTo(V3[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public bool Remove(V3 item)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public int Count { get; }

    /// <inheritdoc />
    public bool IsReadOnly { get; }

    /// <inheritdoc />
    public int IndexOf(V3 item)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void Insert(int index, V3 item)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void RemoveAt(int index)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public V3 this[int index]
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
}
public struct V3
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}