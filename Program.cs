using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpentkGraphics;
using StbImageSharp;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using ClickableTransparentOverlay;
using ImGuiNET;
using SharpGen.Runtime;
using static OpenTK.Graphics.OpenGL.GL;
using Color4 = OpenTK.Mathematics.Color4;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;
using Timer = System.Timers.Timer;

namespace RotatingCube
{
    class ConfigOverlay : Overlay
    {
        /// <inheritdoc />
        public override Task Run()
        {
            base.Position = Point.Empty;
            return base.Run();
        }
        /// <inheritdoc />
        protected override void Render()
        {
            base.Position = new Point(Program.Instance.Location.X, Program.Instance.Location.Y);
            ImGui.Begin(Program.WINDOW_TITLE);
            if (ImGui.Button("close")) Environment.Exit(0);

            ImGui.Text($"FPS: {Program.fps:#.0}");
            ImGui.SameLine();
            ImGui.Spacing();
            ImGui.SameLine();
            ImGui.Text("fps cap");
            ImGui.SameLine();
            ImGui.DragFloat("", ref Program.fpscap, 0.5f, 0, 1000);

            ImGui.DragFloat3("obj pos", ref Program.op);

            if (ImGui.Button("GC.Collect")) GC.Collect();
        }
    }
    class Program : GameWindow
    {
        public static System.Numerics.Vector3 op = new();
        public static string WINDOW_TITLE;
        public static float fps = 0;
        public static float time = 0;
        private Timer s;
        public static Program Instance;
        public static IntPtr handle;
        public static Dictionary<string, int> BadShaders = new();
        private int vao;
        private int vbo;
        private int tVbo;
        private int ebo;
        public int fr = 0;
        public static int TextureSize = 0;
        private const int UNBIND = 0;
        private int prog;
        private Dictionary<string, int> textures = new();
        public static float fpscap = 0;
        [STAThread]
        static void Main(string[] args_) => new Program().Run();

        /// <inheritdoc />
        public Program() : base(new()
        {
            //UpdateFrequency = 4
        }, new() { Title = "title", WindowState = WindowState.Normal, })
        {
            WINDOW_TITLE = "Overlay";
            s = new();
            s.AutoReset = true;
            s.Interval = 1000;
            s.Elapsed += delegate
            {
                fps = fr / time;// / 1000;
                time = fr = 0;

            };
            s.Start();
            Instance = this;
        }
        private int loc, loc2, loc3, loc4, loc5;
        private double timer = 0;

        /// <inheritdoc />
        public override unsafe void Run()
        {
            new Thread(() => new ConfigOverlay().Start()).Start();
            GLFW.HideWindow(WindowPtr);
            base.Run();
        }

        /// <inheritdoc />
        protected override void OnUnload()
        {
            base.OnUnload();
            DeleteVertexArray(vao);
            DeleteBuffer(vbo);
            DeleteBuffer(ebo);
            foreach (var VARIABLE in textures) DeleteTexture(VARIABLE.Value);
            DeleteProgram(prog);
        }
        private List<uint> indices = new();
        private List<Vector3> vert = new();
        private List<int> texCoords = new();
        /// <inheritdoc />
        protected override unsafe void OnLoad()
        {
            base.OnLoad();
            Focus();
            handle = Context.WindowPtr;
            #region OpenGL 
            //Упаси боже ты здесь что то поменяешь 
            vao = GenVertexArray();
            BindVertexArray(vao);
            vbo = GenBuffer();
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";
            List<string> lines = File.ReadAllLines(@"model.obj").Where(l => l.StartsWith("v") || l.StartsWith("f")).ToList();
            bool textured = false;
            foreach (var line in lines)
            {
                if (line.Split(" ")[0] == "v")
                    vert.Add(new(
                        float.Parse(line.Split(" ")[1].ReplaceLineEndings("0"), NumberStyles.Any, ci),
                        float.Parse(line.Split(" ")[2].ReplaceLineEndings("0"), NumberStyles.Any, ci),
                        float.Parse(line.Split(" ")[3].ReplaceLineEndings("0"), NumberStyles.Any, ci)));

                if (line.StartsWith("f"))
                {
                    if (line.Contains("/")) //new
                    {
                        textured = true;
                        indices.Add(uint.Parse(line.Split(" ")[1].Split("/")[0]) - 1);
                        indices.Add(uint.Parse(line.Split(" ")[2].Split("/")[0]) - 1);
                        indices.Add(uint.Parse(line.Split(" ")[3].Split("/")[0]) - 1);

                        texCoords.Add(int.Parse(line.Split(" ")[1].Split("/")[1]) - 1);
                        texCoords.Add(int.Parse(line.Split(" ")[2].Split("/")[1]) - 1);
                        texCoords.Add(int.Parse(line.Split(" ")[3].Split("/")[1]) - 1);
                    }
                    else
                    {
                        indices.Add(uint.Parse(line.Split(" ")[1]) - 1);
                        indices.Add(uint.Parse(line.Split(" ")[2]) - 1);
                        indices.Add(uint.Parse(line.Split(" ")[3]) - 1);
                    }
                }
            }

            BindBuffer(BufferTarget.ArrayBuffer, vbo);
            BufferData(BufferTarget.ArrayBuffer, vert.Count * Vector3.SizeInBytes, vert.ToArray(), BufferUsageHint.StaticDraw);
            //BindBuffer(BufferTarget.ArrayBuffer, UNBIND);

            VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            EnableVertexArrayAttrib(vao, 0);
            BindBuffer(BufferTarget.ArrayBuffer, UNBIND);

            tVbo = GenBuffer();
            BindBuffer(BufferTarget.ArrayBuffer, tVbo);
            BufferData(BufferTarget.ArrayBuffer, texCoords.Count * sizeof(int), texCoords.ToArray(), BufferUsageHint.StaticDraw);


            VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);
            EnableVertexArrayAttrib(vao, 1);

            BindBuffer(BufferTarget.ArrayBuffer, UNBIND);
            BindVertexArray(UNBIND);


            ebo = GenBuffer();
            BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint), indices.ToArray(), BufferUsageHint.StaticDraw);
            BindBuffer(BufferTarget.ElementArrayBuffer, UNBIND);
            int errors = 0;
            prog = CreateProgram();
            foreach (var name in Directory.GetFiles("Shader"))
            {
                int shader = CreateShader(name.EndsWith("vert") ? ShaderType.VertexShader : ShaderType.FragmentShader);
                ShaderSource(shader, FromFile(name));
                CompileShader(shader);
                string log = GetShaderInfoLog(shader);
                // Log("Compiled shader " + name);
                // Log(string.Join("", log.Split("\n").Select(s => s.Insert(0, "\n\t"))));
                int bad = 0;
                errors += bad = Math.Max(log.Split('\n').Count(l => l.Contains(": error")), 0);
                BadShaders.Add(name, bad);
                AttachShader(prog, shader);
                DeleteShader(shader);
            }
            LinkProgram(prog);
            StbImage.stbi_set_flip_vertically_on_load(1);

            foreach (var VARIABLE in Directory.GetFiles("Textures"))
            {
                int id = 0;
                textures.Add(Path.GetFileName(VARIABLE).Split('.')[0], id = GenTexture());
                ActiveTexture(TextureUnit.Texture0);
                BindTexture(TextureTarget.Texture2D, id);

                TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
                TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);


                ImageResult tex = ImageResult.FromStream(File.OpenRead(VARIABLE), ColorComponents.RedGreenBlueAlpha);
                TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, tex.Width, tex.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, tex.Data);
                BindTexture(TextureTarget.Texture2D, UNBIND);
                //Log($"Loaded texture(id:{id:D3}): {VARIABLE}");
                TextureSize += tex.Data.Length * sizeof(byte);
            }
            Enable(EnableCap.DepthTest);
            string pil = GetProgramInfoLog(prog);
            CenterWindow(new(550, 500));
            GLFW.ShowWindow(WindowPtr);
            #endregion
            GC.Collect(); //?
        }

        /// <inheritdoc />
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            Viewport(0, 0, e.Width, e.Height);
        }
        private float r = 5;// { get => (float)Random.Shared.NextDouble(); }
        private float xrot = 0, yrot = 0;
        /// <inheritdoc />
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            fr++;

            //WindowState = fs ? WindowState.Fullscreen : WindowState.Normal;
            time += (float)args.Time;
            UpdateFrequency = fpscap;
            ClearColor(Color4.Gray);
            Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            UseProgram(prog);
            BindTexture(TextureTarget.Texture2D, textures["side"]);

            BindVertexArray(vao);
            BindBuffer(BufferTarget.ElementArrayBuffer, ebo);

            int w = Size.X;
            int h = Size.Y;
            Matrix4 model = Matrix4.Identity;
            Matrix4 view = Matrix4.Identity;
            Matrix4 projecion = Matrix4.Identity;
            model = Matrix4.Identity;
            view = Matrix4.Identity;
            try
            {
                projecion = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60), (float)((w == 0 || h == 0) ? (1.1f) : (w / h)), 0.1f, 100f);
            }
            catch (Exception e) when (e is not DivideByZeroException)
            {

            }

            model = Matrix4.CreateRotationY(-90);
            //model = Matrix4.CreateRotationY(yrot >= 365 ? yrot = 0 : yrot += 0.5f * (float)args.Time)
            //* Matrix4.CreateRotationX(yrot >= 365 ? yrot = 0 : yrot += 0.5f * (float)args.Time);

            Matrix4 translation = Matrix4.CreateTranslation((new Vector3(op.X, op.Y = 20f * MathF.Sin((float)Math.Abs(yrot += (float)args.Time * 2)), -op.Z) / 30)) *
                                  Matrix4.CreateTranslation((new Vector3(op.X = 35f * MathF.Sin((float)Math.Abs(xrot += (float)args.Time * 1)), op.Y, -op.Z) / 30));
            model *= translation;

            int modelLoc = GetUniformLocation(prog, "model");
            int modelView = GetUniformLocation(prog, "view");
            int modelProj = GetUniformLocation(prog, "projection");

            UniformMatrix4(modelLoc, true, ref model);
            UniformMatrix4(modelView, true, ref view);
            UniformMatrix4(modelProj, true, ref projecion);

            DrawElements(BeginMode.Triangles, indices.Count, DrawElementsType.UnsignedInt, 0);
            //Update(args);
            Context.SwapBuffers();
            base.OnRenderFrame(args);
        }
#pragma warning disable
        #region Reflections and other siht
        string FromFile(string path) => File.ReadAllText(path);

        void Update(FrameEventArgs a) => Assembly.GetExecutingAssembly().GetTypes()
            .Where(c => c.GetCustomAttributes<Behaviour>().Any(b => b.CallUpdate)).ToList().ForEach(t =>
                t.GetMethods().Where(m => m.Name == "Update").ToList().ForEach(m =>
                    m.Invoke(t.GetConstructor(Array.Empty<Type>()).Invoke([]),
                        [KeyboardState, MouseState, a])));  //  здесь нет ошибок здесь нет ошибок здесь нет ошибок здесь нет ошибок здесь нет ошибок здесь нет ошибок здесь нет ошибок https://learn.microsoft.com/ru-ru/dotnet/csharp/language-reference/operators/collection-expressions
        public static bool DEBUG =
#if DEBUG
            true
#else
            false
#endif
            ;
        /*
         
⠀           ⠀⢀⣀⣄⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣴⠾⠛⠛⠷⣦⡀⠀⠀⠀⠀⠀⠀
           ⢠⣶⠛⠋⠉⡙⢷⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣾⣿⠐⡡⢂⠢⠈⠻⣦⡀⠀⠀⠀⠀
           ⣾⠃⠠⡀⠥⡐⡙⣧⣰⣤⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢿⡹⡜⢄⠣⢤⣩⣦⣸⣧⠀⠀⠀⠀
           ⣿⡀⢢⠑⠢⣵⡿⠛⠉⠉⠉⣷⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢿⣜⢬⣿⠛⠉⠉⠉⠻⣧⡀⠀⠀
           ⣹⣇⠢⣉⣾⡏⠀⠠⠀⢆⠡⣘⣷⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⡾⠟⠋⢉⠛⢷⣄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢻⣿⡆⠱⡈⠔⠠⠄⠈⢷⡄⠀
           ⠀⢿⣦⢡⣿⠀⠌⡐⠩⡄⢊⢵⣇⣠⣀⣀⡀⠀⠀⠀⠀⣼⠟⢁⢀⠂⠆⡌⢢⢿⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣤⡀⠀⠀⠀⠀⣾⣅⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢻⡏⢷⡣⠜⣈⠆⡡⠂⠌⣷⠀
           ⠀⠀⢹⡞⣧⠈⡆⢡⠃⣼⣾⡟⠛⠉⠉⠉⠛⣷⡄⠀⢸⡏⠐⢨⡄⡍⠒⣬⢡⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣴⡟⠁⠀⠀⠀⠀⠑⢻⣶⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⣴⣾⡖⣶⣦⠀⠀⠀⠀⠀⠀⠀⢸⡏⡜⣷⢱⢨⡆⢱⠈⡆⣿⠀
           ⠀⠀⠀⠽⣇⠎⡰⣩⡼⡟⠁⠄⡀⠠⠀⠀⠀⠈⢿⡄⡿⢄⢃⠖⡰⣉⠖⣡⡿⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣷⡿⠁⣀⣠⣀⣤⣤⣤⣼⣿⣷⣦⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣰⢯⠋⡀⢀⠀⠉⢿⣆⠀⠀⠀⠀⠀⣸⢗⢡⣿⣂⣖⣨⡱⢊⡔⣿⠀
           ⠀⠀⠀⠀⣯⠒⠥⡾⢇⠰⡉⠔⡠⠃⡌⢐⠡⠀⣼⡟⡓⢌⢒⢪⠑⣌⡾⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⣴⣿⣿⣿⣻⠭⠿⠛⠒⠓⠚⠛⠛⠿⣿⣿⣿⣿⣳⡶⢦⣄⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣾⡚⠤⣁⠢⠐⣀⠀⣿⡀⠀⠀⠀⢈⡟⣸⢟⠉⠁⠀⠉⠙⢷⣴⠇⠀
           ⠀⠀⠀⠀⢿⣩⢲⣟⢌⡒⡱⢊⠴⢡⢘⣄⣢⣽⠞⡑⢌⠂⢎⠤⢋⡞⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣠⣶⣿⣿⣿⠿⠋⡀⢀⠠⠀⠄⠠⠂⠄⠄⡠⠀⠄⡈⠉⠛⠛⠛⡙⠺⣭⡗⣦⣄⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⢿⣰⠂⡅⢣⠐⡠⢽⡇⠀⠀⢀⡾⡅⣯⢄⠊⠤⢁⠂⠄⠀⠙⣧⡀
           ⠀⠀⠀⠀⠺⣇⢾⢭⢢⠱⣡⠋⣔⣷⠋⡍⠰⢀⠊⠰⢈⠜⡠⢊⣽⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⡶⣿⡿⠛⠛⡉⡁⢄⠂⡔⢠⠂⡅⢊⢡⠘⡐⢌⣠⡑⠢⢐⠡⢊⠔⡡⢂⠅⣂⠙⡳⣎⡟⣶⣄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠹⣯⠰⢃⡜⢠⢺⡇⠀⢰⡾⠅⠃⢿⣜⠌⡒⢄⢊⡐⡁⢂⠘⣧
           ⠀⠀⠀⠀⠀⢻⣺⡇⢎⡱⢄⡓⣾⠄⢣⠈⠅⡂⠡⠑⡈⢢⠑⢢⡟⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣴⡾⣫⠗⡅⣢⣥⣧⢽⠶⠟⣶⢶⣿⠆⡜⡐⠦⠱⢌⠢⡜⢏⣿⠛⣛⠳⢾⣤⡣⡜⣠⠓⡤⢩⢳⡎⡝⡷⣄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⡟⡰⢈⠆⡹⣇⣰⠿⡀⢌⠒⠤⠙⢷⣼⡠⢆⡔⢡⠂⠔⣻
           ⠀⠀⠀⠀⠀⠐⢻⣏⠦⣑⢊⠔⣿⠈⢆⡑⠂⡌⢠⠑⡈⠤⡉⢼⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣴⢻⢣⠞⣣⢵⣾⡿⢋⠃⢆⠬⣹⠗⡬⡑⢎⠴⣉⠎⣕⢪⡑⢎⠲⡸⢯⣅⡚⠤⡘⡙⠿⣶⣍⡒⠧⢎⠼⣑⢣⢏⢷⣆⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⡿⡐⠥⠚⡄⡙⠓⠤⡑⢌⡘⠤⡉⣼⢌⣷⠢⠜⢢⠉⢆⣿
           ⠀⠀⠀⠀⠀⠀⠐⣯⣚⠤⡋⡜⢫⠩⢄⠢⡑⡠⢃⠰⡁⢆⠱⣈⡧⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⣾⠏⣎⢣⣾⣿⡋⢍⡰⢌⡚⣌⣾⢋⠳⡰⣉⢎⠲⣡⠚⡤⠣⡜⣌⢣⠱⣩⠙⠷⣧⠵⡨⠜⡨⠻⣿⣇⠮⣑⢎⢣⠞⣬⡙⣯⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣸⢻⡌⡱⢃⡜⣨⡕⢢⠑⣢⠘⢤⣹⢏⡜⣠⢣⠙⢦⡙⢦⠇
           ⠀⠀⠀⠀⠀⠀⠀⠽⣎⠖⡱⢌⠥⢊⠖⠓⠒⠿⣮⡔⡡⢎⠰⢂⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣼⡛⣆⡛⣴⣿⡟⠰⣌⠲⡌⢶⢞⡋⢦⡉⠖⣑⣢⣮⣵⣶⣷⣶⣷⣶⣶⣥⣧⣢⡙⢢⡑⢎⡡⡙⠴⣛⠛⡦⢓⢬⠚⣌⡓⢦⡹⢜⡻⣆⠀⠀⠀⠀⠀⠀⠀⠀⢰⣿⢜⡢⢱⣡⡿⠛⠛⢒⠳⢤⢋⠴⣛⠣⡔⢢⢎⡙⢦⣱⠟⠀
           ⠀⠀⠀⠀⠀⠀⠀⠀⢻⣝⡰⣉⠖⣡⠚⣈⠁⠄⠈⢻⣶⡨⢡⢃⡿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⡾⢳⠍⣦⠱⣊⠏⡽⣉⢆⠳⢌⠣⢆⡙⣤⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⣼⡠⢃⠝⡢⢅⠫⡔⡍⢦⠹⢤⡙⢦⠱⣋⡜⡻⣆⠀⠀⠀⠀⠀⠀⠀⠈⢻⣜⠲⣱⣿⠀⠂⡍⠰⣈⠦⡉⢖⡡⢓⡌⢣⠎⣜⣶⠏⠀⠀
           ⠀⠀⠀⠀⠀⠀⠀⠀⠠⢻⣖⡡⠞⣄⠓⡄⠣⢐⠀⠀⢻⣿⣥⡾⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⡾⣍⢧⢫⠔⡫⠴⣉⠖⡡⢎⡱⢊⡱⣼⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⣎⡑⢎⡱⡘⡜⢢⠝⣢⡙⣌⢳⡑⢮⠱⣹⣆⠀⠀⠀⠀⠀⠀⠀⠈⢿⡱⣿⣿⠀⢃⠌⡱⢠⢒⡉⢦⡑⢣⡜⢣⣾⠞⠁⠀⠀⠀
           ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠻⣼⠱⣌⠓⡬⠑⡌⠠⠁⢸⣿⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣾⢑⠎⡖⣩⢎⡱⢣⠜⡬⡑⢎⠔⣣⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣮⡢⡑⡜⢌⡱⢪⠔⡱⢊⢦⠹⣌⠏⣄⢻⡆⠀⠀⠀⠀⠀⠀⠀⠁⠙⢿⣿⡌⡐⢌⠰⠡⢎⠜⣢⠙⣦⡽⠟⠁⠀⠀⠀⠀⠀
           ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠻⣦⣝⡰⢩⢌⠱⣈⣾⠏⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣸⠇⣎⠹⡬⣑⠎⣔⠣⣍⠒⡭⢌⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⡕⡘⠦⣡⢃⢎⡱⣉⢦⢋⡜⡎⢥⠊⣿⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠹⣿⣔⣈⠒⣍⣢⣽⡴⠟⠉⠀⠀⠀⠀⠀⠀⠀⠀
           ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠉⠙⠛⠚⠛⠉⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⡿⠐⣌⢓⠲⣉⠞⡤⢓⠬⡑⢆⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣎⢒⡡⠎⢦⠱⡌⠦⡍⠖⣭⠒⡌⢸⣇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠉⠙⠛⠋⠉⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
           ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢼⡇⢢⠙⡜⢦⢋⡴⢡⠚⡤⢓⣼⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡌⢣⠔⡌⢦⠱⢢⠕⡲⣉⠖⣁⠚⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
           ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣾⠁⢢⠹⣌⠳⣌⠲⣡⢋⠴⣹⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡐⢎⡜⢢⡙⢆⢫⠱⣌⠳⣀⠂⣿⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
           ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠠⣿⠐⢂⠳⣌⠳⣌⠳⣄⢋⣴⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡰⢌⠣⡜⣌⠣⡝⢤⠳⢄⠂⢹⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
           ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⣿⠀⢣⡙⣔⠣⡜⠲⡌⢦⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣧⢊⠵⡘⢤⡓⢬⢣⡙⠢⠌⢸⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
           ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠸⣿⠈⠴⡱⢌⡳⢌⠳⡘⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣎⠲⣉⠦⡙⣆⢣⠚⡅⠊⣿⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
           ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠘⣿⡈⡱⣘⢣⠜⡬⢣⢹⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡰⡡⢎⡱⡌⡖⣍⠒⡡⣿⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
           ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⡇⡒⣍⠮⡜⢆⢣⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠟⡋⠍⡠⠄⡠⢀⠂⡍⢙⠻⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣧⡑⢮⠰⡱⢜⡢⠍⢤⣿⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
           ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣺⣇⣱⢎⢲⣉⠮⢼⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⢟⡋⠔⣡⠘⠤⡑⢨⠐⡁⢎⠠⢃⠌⡐⡙⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡜⣌⢣⠕⣎⡱⣩⢘⡿⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
           ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠩⣷⡐⣏⠦⣃⠞⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡟⡑⠢⠜⡰⢠⢉⠒⡌⢄⠣⡘⠄⠣⢌⠢⡑⢌⠢⡙⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣯⠔⣣⢚⡴⣑⡃⢾⠇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
           ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⣷⠸⣜⡰⢻⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠋⡴⢉⠜⢢⠑⠢⢌⠒⡌⢢⠑⠤⣉⠲⡈⢆⠱⢌⠢⢡⠃⡽⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⢜⢢⠣⢖⡱⢌⡿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
           ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⢻⣷⢆⡇⣻⣿⣿⣿⣿⣿⣿⣿⣿⡿⣁⠳⢨⠜⡨⢆⣉⠣⣊⠜⣈⠆⣙⡐⢢⠡⣑⢊⠒⡌⢣⢑⡊⡔⣊⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡎⡖⣹⢊⡵⣿⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
           ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⣿⢼⡘⣽⣿⣿⣿⣿⣿⣿⣿⢏⠦⣡⢋⠲⢌⡑⠦⢌⡱⡐⠎⡤⠩⢔⠨⡅⢃⠲⢌⡱⢌⡱⢢⠱⡘⢤⣉⠻⣿⣿⣿⣿⣿⣿⣿⣿⡗⣍⠖⣯⣼⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
           ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⢺⡱⣚⣿⣿⣿⣿⣿⠟⡕⢎⠲⡡⢎⡱⢊⡜⡘⢆⠲⢡⠓⣌⠓⣌⠓⣌⢃⠳⣈⠲⢌⡒⣡⢣⡙⠦⣌⠣⡍⢿⣿⣿⣿⣿⣿⣿⡹⡰⣋⢿⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
           ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢿⡟⡠⢇⠼⣻⠿⣟⢣⠟⡸⢜⢣⠣⡜⡠⢇⡸⢣⠜⢣⠇⡛⣄⢛⡀⢟⡀⠟⡤⢃⠻⡄⢣⢄⢣⡘⢇⡄⢧⠛⣤⢘⡿⣿⣿⣿⢟⡣⢣⠇⣿⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
           ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠚⣧⡑⡌⠒⡡⠚⢤⣳⡾⣱⠪⣅⠳⢬⠱⣊⠴⣃⠮⡑⢎⡱⢌⠦⣙⢢⡙⡜⡰⣉⠖⣩⠲⡌⢦⡙⠦⡜⢢⠛⡤⢓⡜⣆⠳⡜⢪⡑⢣⠋⣾⢁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
           ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠙⣷⣄⣁⣠⣽⡟⢧⡱⢆⡳⣌⢓⡎⡱⣌⠳⣌⢲⣉⠖⡱⢊⠖⣡⢒⡱⣌⡱⡘⣜⢢⢓⡜⣢⡙⣜⡘⣣⢝⡸⣛⢾⣌⡳⢈⠥⠘⠠⢡⡿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
           ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠙⠛⠋⠉⠛⢧⡝⣎⠵⣌⠧⡜⡱⣌⠳⣌⠶⡌⢞⡡⢏⠼⣡⢎⡱⢢⠵⡱⣌⢎⠦⣱⢡⠞⣤⠛⡴⢪⠵⣩⢞⣼⠿⢶⣤⣥⣤⡿⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
           ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠙⠳⣽⡘⢮⡱⢳⣌⠳⣜⢢⡝⣢⢝⡸⢲⢡⠞⣰⠣⡞⡱⣌⢎⢞⡰⢣⠞⣔⡫⣜⢣⣿⠶⠋⠀⠀⠀⠈⠉⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
           ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠉⠓⠯⣧⣎⠳⣬⢓⡬⡱⢎⡵⣋⡬⣛⠴⣋⠶⡱⢎⡞⡬⢳⡍⣞⣦⠷⠛⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
           ⠄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢈⡙⠓⠻⠶⠽⢮⣶⣥⣷⣭⣾⣥⣯⡵⠯⠼⠗⠛⠋⣉⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀


         */
        public class Behaviour : Attribute
        {
            public bool CallUpdate { get; set; }
        }
        #endregion
#pragma warning restore
    }
}