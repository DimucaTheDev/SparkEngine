#region Header
// // OpentkGraphics -> OpentkGraphics -> Overlay.cs
// // Created By DimucaTheDev on 14:23 (10.03.2024)
#endregion

using ImGuiNET;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Numerics;
using static ImGuiNET.ImGui;

namespace SparkEngine.Configuration;

internal class Overlay : ClickableTransparentOverlay.Overlay
{
    private string command = "";
    public string Log = "Command line interpreter.\nType '?' for help.\n";

    public Overlay()
    {
        Instance = this;
    }

    public static Overlay? Instance { get; set; }

    /// <inheritdoc />
    protected override void Render()
    {
        //main config
        Position = new Point(Program.Instance.Location.X, Program.Instance.Location.Y);
        Begin("SparkEngine");
        if (Button("close")) Environment.Exit(0);
        if (Button("restart")) Program.Restart = true;
        if (CollapsingHeader("Main"))
        {
            Indent();
            DragFloat("fps cap", ref Program.fpscap, 0.5f, 0, 1000);
            if (Button("GC.Collect")) GC.Collect();

            Unindent();
        }

        //fun
        Text($"FPS: {Program.fps:#.0}");
        Text("yaw: " + Camera.Yaw);
        Text("pitch: " + Camera.Pitch);
        Text("fov: " + Camera.Fov);
        Text($"frames: {Program.Frames}");
        End();

        Begin("Scene");
        if (CollapsingHeader("Scenes"))
        {
            Indent();
            foreach (var scene in Directory.GetDirectories("Scenes"))
            {
                var scene_ = (string)scene.Clone();
                if (Button($"{scene_.Split("\\")[1]}"))
                    SceneManager.CurrentScene = SceneManager.Load(scene_ + "/levelinfo.json");
            }

            Unindent();
        }

        if (CollapsingHeader("Objects"))
        {
            Indent();
            for (var i = 0; i < SceneManager.CurrentScene.Objects.Count; i++)
            {
                var _ = i;
                if (CollapsingHeader($"[{SceneManager.CurrentScene.Objects[_].Name}] - Transform"))
                {
                    BeginChild(_.ToString());
                    Indent();
                    InputText("Name", ref SceneManager.CurrentScene.Objects[_].Name, 100);
                    InputFloat3("Pos", ref SceneManager.CurrentScene.Objects[_].Transform.Position);
                    InputFloat3("Rot", ref SceneManager.CurrentScene.Objects[_].Transform.Rotation);
                    Unindent();
                    EndChild();
                    if (CollapsingHeader("Fun"))
                    {
                        Indent();
                        Checkbox("sinusoida", ref SceneManager.CurrentScene.Objects[_].Sinusoida);
                        Unindent();
                    }
                }
            }

            Unindent();
        }

        End();
        Begin("Console");
        Text($"GLES errors:{Program.Errors}");
        Text("Latest:" + GL.GetError());
        var footer_height_to_reserve = GetStyle().ItemSpacing.Y + GetFrameHeightWithSpacing();
        if (BeginChild("ScrollingRegion", new Vector2(0, -footer_height_to_reserve), ImGuiChildFlags.None,
                ImGuiWindowFlags.HorizontalScrollbar))
        {
            if (BeginPopupContextWindow())
            {
                if (Selectable("Clear")) Log = "";
                EndPopup();
            }

            foreach (var line in Log.Split("\n"))
            {
                if (line.Split(" ")[0].Contains("ERR!"))
                    PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
                else if (line.Split(" ")[0].Contains("WARN")) PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 0, 1));
                TextUnformatted($"{line}");
                PopStyleColor();
            }

            PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 1));
            SetScrollHereY(1.0f);
            PopStyleVar();
        }

        EndChild();
        Separator();

        var input_text_flags = ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.EscapeClearsAll |
                               ImGuiInputTextFlags.CallbackCompletion | ImGuiInputTextFlags.CallbackHistory;
        if (InputText("Input", ref command, 999, input_text_flags))
        {
            if (CommandLineInterpreter.Execute(command) == CommandResult.Fail)
                CommandLineInterpreter.Error("Command execution failed");
            command = "";
        }

        End();

        foreach (var UPPER in Program.Instance.JoystickStates.Where(s => s != null))
        {
            var joystick = UPPER;
            Begin($"Controller_{joystick.Id} ({joystick.Name})");
            Text($"AXIS_{0}:{joystick.GetAxis(0)}");
            Text($"AXIS_{1}:{joystick.GetAxis(1)}");
            Text($"AXIS_{2}:{joystick.GetAxis(2)}");
            Text($"AXIS_{3}:{joystick.GetAxis(3)}");
            Text($"AXIS_{4}:{joystick.GetAxis(4)}");
            Text($"AXIS_{5}:{joystick.GetAxis(5)}");
            for (var i = 0; i < joystick.ButtonCount; i++)
                Text($"b{i}:{joystick.IsButtonDown(i)}");
            End();
        }
    }
}