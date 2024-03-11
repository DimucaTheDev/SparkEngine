using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SparkEngine;
using SparkEngine.Configuration;

namespace SparkEngine;
public static class Camera
{
    public static double Sensitivity = 0.15f;
    public static double Yaw = 0;
    public static double Pitch = 0;
    public static float Fov = 60;
    public static Vector3 Position = new(0);
    public static Vector3 Front = new Vector3(0.0f, 0.0f, -1.0f);
    public static Vector3 Up = new Vector3(0.0f, 1.0f, 0.0f);
}