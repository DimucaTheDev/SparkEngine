#region Header

// // OpentkGraphics -> OpentkGraphics -> Overlay.cs
// // Created By DimucaTheDev on 14:23 (10.03.2024)

#endregion

using ImGuiNET;
using SparkEngine;
using static ImGuiNET.ImGui;

namespace SparkEngine.Configuration;

class Overlay : ClickableTransparentOverlay.Overlay
{
    /// <inheritdoc />
    protected override void Render()
    {
        //main config
        Position = new Point(Program.Instance.Location.X, Program.Instance.Location.Y);
        Begin("overlay");
        if (Button("close")) Environment.Exit(0);
        if (CollapsingHeader("Main"))
        {
            Indent();

            Text($"FPS: {Program.fps:#.0}");
            DragFloat("fps cap", ref Program.fpscap, 0.5f, 0, 1000);
            if (Button("GC.Collect")) GC.Collect();

            Unindent();
        }
        //fun

        Text("yaw: " + Program.camera.Yaw);
        Text("pitch: " + Program.camera.Pitch);
        End();

        Begin("Scene");
        for (int i = 0; i < GameObject.Models.Count; i++)
        {
            var _ = i;
            if (CollapsingHeader($"[{GameObject.Models[_].Name}] - Transform"))
            {
                BeginChild(_.ToString());
                Indent();
                InputText("Name", ref GameObject.Models[_].Name, 100);
                InputFloat3("Pos", ref GameObject.Models[_].Transform.Position);
                InputFloat3("Rot", ref GameObject.Models[_].Transform.Rotation);
                Unindent();
                EndChild();
                if (CollapsingHeader("Fun"))
                {
                    Indent();
                    Checkbox("sinusoida", ref GameObject.Models[_].Sinusoida);
                    Unindent();
                }
            }
        }
    }
}