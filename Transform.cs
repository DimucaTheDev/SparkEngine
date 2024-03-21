using System.Numerics;
using Newtonsoft.Json;

namespace SparkEngine
{
    public class Transform : ICloneable
    {
        [JsonIgnore]
        public OpenTK.Mathematics.Vector3 Position2
        {
            get => new(Position.X, Position.Y, Position.Z);
            set => Position = new(value.X, value.Y, value.Z);
        }
        public Vector3 Position = new();
        public Vector3 Rotation = new();
        [Obsolete("TODO: Add ")]
        public Vector3 Scale
        {
            get => new(1);
            set
            {
            }
        }

        /// <inheritdoc />
        public object Clone() => new Transform() { Position = this.Position, Rotation = this.Rotation, Scale = this.Scale };
    }
}
