using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace SparkEngine
{
    public class Transform
    {
        public Vector3 Position=new();
        public Vector3 Rotation=new();
        [Obsolete("TODO: Add ")]
        public Vector3 Scale
        {
            get => new(1);
            set
            {
            }
        }
    }
}
