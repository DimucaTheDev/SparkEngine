using System.Numerics;
using Newtonsoft.Json;

namespace SparkEngine;

public class Transform : ICloneable
{
    public Vector3 Position;
    public Vector3 Rotation;

    [JsonIgnore]
    public OpenTK.Mathematics.Vector3 Position2
    {
        get => new(Position.X, Position.Y, Position.Z);
        set => Position = new Vector3(value.X, value.Y, value.Z);
    }

    [Obsolete("TODO: Add ")]
    public Vector3 Scale
    {
        get => new(1);
        set { }
    }

    /// <inheritdoc />
    public object Clone()
    {
        return new Transform { Position = Position, Rotation = Rotation, Scale = Scale };
    }
}