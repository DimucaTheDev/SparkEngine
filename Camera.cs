using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpentkGraphics
{
    [Behaviour(CallUpdate = false)]
    internal class Camera
    {
        public Matrix4 GetViewMatrix() { throw new(); }

        public Matrix4 GetProjectionMatrix()
        {
            throw new();
        }

        public Camera()
        {

        }
        public void InputController(KeyboardState inpit, MouseState mouse, FrameEventArgs e) { }
        private void UpdateVectors() { }
        public void Update(KeyboardState inpit, MouseState mouse, FrameEventArgs e)
        {
            InputController(inpit, mouse, e);
        }

    }
}
