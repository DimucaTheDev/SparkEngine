#region Header

// // OpentkGraphics -> OpentkGraphics -> Overlay.cs
// // Created By DimucaTheDev on 14:23 (10.03.2024)

#endregion

using System.Numerics;
using ImGuiNET;
using SparkEngine.Shader;
using Vortice.Direct3D11;
using static ImGuiNET.ImGui;

namespace SparkEngine.Configuration;

class Overlay : ClickableTransparentOverlay.Overlay
{
    public static Overlay? Instance { get; set; }
    public Overlay() => Instance = this;
    public string Log = "Command line interpreter.\nType '?' for help.\n";
    /// <inheritdoc />
    protected override void Render()
    {
        //main config
        Position = new System.Drawing.Point(Program.Instance.Location.X, Program.Instance.Location.Y);
        Begin("SparkEngine");
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

        Text("yaw: " + Camera.Yaw);
        Text("pitch: " + Camera.Pitch);
        End();

        Begin("Scene");
        if (CollapsingHeader("Objects"))
        {
            Indent();
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
            Unindent();
        }
        End();
        Begin("Console");
        float footer_height_to_reserve = GetStyle().ItemSpacing.Y + GetFrameHeightWithSpacing();
        if (BeginChild("ScrollingRegion", new(0, -footer_height_to_reserve), ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar))
        {
            if (BeginPopupContextWindow())
            {
                if (Selectable("Clear")) Log = "";
                EndPopup();
            }

            foreach (var line in Log.Split("\n"))
            {
                if (line.Split(" ")[0].Contains("ERR!"))
                {
                    PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
                } else if (line.Split(" ")[0].Contains("WARN"))
                {
                    PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 0, 1));
                }
                TextUnformatted($"{line}");
                PopStyleColor();
            }
            PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 1));
            SetScrollHereY(1.0f);
            PopStyleVar();
        }
        EndChild();
        Separator();

        ImGuiInputTextFlags input_text_flags = ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.EscapeClearsAll | ImGuiInputTextFlags.CallbackCompletion | ImGuiInputTextFlags.CallbackHistory;
        if (InputText("Input", ref command, 999, input_text_flags))
        {
            if (CommandLineInterpreter.Execute(command) == CommandResult.Fail) CommandLineInterpreter.Error("Command execution failed");
            command = "";
        }
        End();
    }

    private string command = "";
    static Overlay()
    {
        //   Logger.PrintLine("hi!");
    }
}