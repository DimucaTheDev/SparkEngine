using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace SparkEngine
{
    internal class PlayerController
    {
        private static Vector2 lastPos;
        public static float Speed = Time.Delta * 1;
        public static Transform transform = new Transform();
        public static void Look(GameWindow window)
        {
            if (window.CursorState == CursorState.Grabbed && window.IsFocused)
            {
                #region lookin'

                MouseState mouse = window.MouseState;
                float deltaX = mouse.X - lastPos.X;
                float deltaY = mouse.Y - lastPos.Y;
                lastPos = new Vector2(mouse.X, mouse.Y);
                Camera.Yaw += deltaX * Camera.Sensitivity;
                Camera.Pitch -= deltaY * Camera.Sensitivity;
                if (Camera.Pitch > 89.9f)
                    Camera.Pitch = 89.9f;
                else if (Camera.Pitch < -89.9f)
                    Camera.Pitch = -89.9f;
                else
                    Camera.Pitch -= deltaY * Camera.Sensitivity;

                Camera.Front.X = (float)Math.Cos(MathHelper.DegreesToRadians(Camera.Pitch)) *
                                 (float)Math.Cos(MathHelper.DegreesToRadians(Camera.Yaw));
                Camera.Front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(Camera.Pitch));
                Camera.Front.Z = (float)Math.Cos(MathHelper.DegreesToRadians(Camera.Pitch)) *
                                 (float)Math.Sin(MathHelper.DegreesToRadians(Camera.Yaw));
                Camera.Front = Vector3.Normalize(Camera.Front);

                #endregion
            }
        }

        public static unsafe void Walk(Program window)
        {
            KeyboardState input = window.KeyboardState;

            if (input.IsKeyDown(Keys.W))
                PlayerController.transform.Position2 += Camera.Front * Speed; //
            if (input.IsKeyDown(Keys.S))
                PlayerController.transform.Position2 -= Camera.Front * Speed; //
            if (input.IsKeyDown(Keys.A))
                PlayerController.transform.Position2 -= Vector3.Normalize(Vector3.Cross(Camera.Front, Camera.Up)) * Speed; //
            if (input.IsKeyDown(Keys.D))
                PlayerController.transform.Position2 += Vector3.Normalize(Vector3.Cross(Camera.Front, Camera.Up)) * Speed; //
            if (input.IsKeyDown(Keys.Space))
                PlayerController.transform.Position2 += Camera.Up * Speed; //
            if (input.IsKeyDown(Keys.LeftShift))
                PlayerController.transform.Position2 -= Camera.Up * Speed; //Down
        }
    }
}
