﻿using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;
using System.Reflection;
using System.Runtime.InteropServices;
using static OpenTK.Graphics.OpenGL.GL;
using Color4 = OpenTK.Mathematics.Color4;
using Timer = System.Timers.Timer;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;
using SparkEngine.Configuration;
using Veldrid.Sdl2;
using Bitmap = System.Drawing.Bitmap;
using ErrorCode = OpenTK.Graphics.OpenGL.ErrorCode;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace SparkEngine
{
    class Program : GameWindow
    {
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

        [DllImport("user32.dll")]
        public static extern bool CloseWindow(IntPtr hWnd);
        public static Bitmap PrintWindow(IntPtr hwnd)
        {
            RECT rc;
            GetWindowRect(hwnd, out rc);
            rc.Width = Instance.Size.X;
            rc.Height = Instance.Size.Y;
            Bitmap bmp = new Bitmap(rc.Width, rc.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics gfxBmp = Graphics.FromImage(bmp);
            IntPtr hdcBitmap = gfxBmp.GetHdc();

            PrintWindow(hwnd, hdcBitmap, 0);

            gfxBmp.ReleaseHdc(hdcBitmap);
            gfxBmp.Dispose();

            return bmp;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            private int _Left;
            private int _Top;
            private int _Right;
            private int _Bottom;

            public RECT(RECT Rectangle) : this(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom)
            {
            }
            public RECT(int Left, int Top, int Right, int Bottom)
            {
                _Left = Left;
                _Top = Top;
                _Right = Right;
                _Bottom = Bottom;
            }

            public int X
            {
                get { return _Left; }
                set { _Left = value; }
            }
            public int Y
            {
                get { return _Top; }
                set { _Top = value; }
            }
            public int Left
            {
                get { return _Left; }
                set { _Left = value; }
            }
            public int Top
            {
                get { return _Top; }
                set { _Top = value; }
            }
            public int Right
            {
                get { return _Right; }
                set { _Right = value; }
            }
            public int Bottom
            {
                get { return _Bottom; }
                set { _Bottom = value; }
            }
            public int Height
            {
                get { return _Bottom - _Top; }
                set { _Bottom = value + _Top; }
            }
            public int Width
            {
                get { return _Right - _Left; }
                set { _Right = value + _Left; }
            }
            public Point Location
            {
                get { return new Point(Left, Top); }
                set
                {
                    _Left = value.X;
                    _Top = value.Y;
                }
            }
            public Size Size
            {
                get { return new Size(Width, Height); }
                set
                {
                    _Right = value.Width + _Left;
                    _Bottom = value.Height + _Top;
                }
            }

            public static implicit operator Rectangle(RECT Rectangle)
            {
                return new Rectangle(Rectangle.Left, Rectangle.Top, Rectangle.Width, Rectangle.Height);
            }
            public static implicit operator RECT(Rectangle Rectangle)
            {
                return new RECT(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom);
            }
            public static bool operator ==(RECT Rectangle1, RECT Rectangle2)
            {
                return Rectangle1.Equals(Rectangle2);
            }
            public static bool operator !=(RECT Rectangle1, RECT Rectangle2)
            {
                return !Rectangle1.Equals(Rectangle2);
            }

            public override string ToString()
            {
                return "{Left: " + _Left + "; " + "Top: " + _Top + "; Right: " + _Right + "; Bottom: " + _Bottom + "}";
            }

            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }

            public bool Equals(RECT Rectangle)
            {
                return Rectangle.Left == _Left && Rectangle.Top == _Top && Rectangle.Right == _Right && Rectangle.Bottom == _Bottom;
            }

            public override bool Equals(object Object)
            {
                if (Object is RECT)
                {
                    return Equals((RECT)Object);
                }
                else if (Object is Rectangle)
                {
                    return Equals(new RECT((Rectangle)Object));
                }

                return false;
            }
        }
        public static float fps = 0;
        public static float time = 0;
        public static int Frames = 0;
        public static int Errors = 0;
        private Timer s;
        public static Program Instance;
        public static IntPtr handle;
        public static Dictionary<string, int> BadShaders = new();
        public int fr = 0;
        public static int TextureSize = 0;
        private const int UNBIND = 0;
        private int prog;
        private Dictionary<string, int> textures = new();
        public static float fpscap = 0;
        private float xrot = 0, yrot = 0;
        float speed = 1.5f;
        public static bool ScreenshotRequired;
        private Vector2 lastPos;
        public static bool Restart = true;
        [STAThread]
        static void Main(string[] args_)
        {
            while (Restart)
            {
                SceneManager.CurrentScene = new();
                GameObject.Models = new List<GameObject>();

                fps = new();
                time = new();
                Frames = new();
                Errors = new();
                BadShaders = new();
                TextureSize = new();
                fpscap = new();
                ScreenshotRequired = new();
                Restart = false;
                new Program().Run();
                CloseWindow(handle);
            }
        }

        /// <inheritdoc />
        public Program() : base(new()
        {
            //UpdateFrequency = 4
        }, new() { Title = "SparkEngine                  visit https://spark.cmdev.pw/ for more info                  made by Dmitrii Punchenko as a personal project                  ESC to release cursor                  click Close or enter 'exit' command to stop", WindowState = WindowState.Normal, })
        {
            s = new();
            s.AutoReset = true;
            s.Interval = 1000;
            s.Elapsed += delegate
            {
                fps = fr / time;// / 1000;
                time = fr = 0;
                UpdateFrequency = fpscap;
            };
            s.Start();
            Instance = this;
        }
        private int loc, loc2, loc3, loc4, loc5;
        private double timer = 0;

        /// <inheritdoc />
        public override unsafe void Run()
        {
            new Thread(() => new Overlay().Start()).Start();
            //GLFW.HideWindow(WindowPtr);
            base.Run();
        }

        public GameObject GenModel(GameObject model)
        {
            model.VAO = GenVertexArray();
            BindVertexArray(model.VAO);
            model.VBO = GenBuffer();

            BindBuffer(BufferTarget.ArrayBuffer, model.VBO);
            BufferData(BufferTarget.ArrayBuffer, model.Vertices.Count * Vector3.SizeInBytes, model.Vertices.ToArray(),
                BufferUsageHint.StaticDraw);
            //BindBuffer(BufferTarget.ArrayBuffer, UNBIND);

            VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            EnableVertexArrayAttrib(model.VAO, 0);
            BindBuffer(BufferTarget.ArrayBuffer, UNBIND);

            model.TVBO = GenBuffer();
            BindBuffer(BufferTarget.ArrayBuffer, model.TVBO);
            BufferData(BufferTarget.ArrayBuffer, model.TexCoords.Count * sizeof(int), model.TexCoords.ToArray(),
                BufferUsageHint.StaticDraw);


            VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);
            EnableVertexArrayAttrib(model.VAO, 1);

            BindBuffer(BufferTarget.ArrayBuffer, UNBIND);
            BindVertexArray(UNBIND);


            model.EBO = GenBuffer();
            BindBuffer(BufferTarget.ElementArrayBuffer, model.EBO);
            BufferData(BufferTarget.ElementArrayBuffer, model.Indices.Count * sizeof(uint), model.Indices.ToArray(),
                BufferUsageHint.StaticDraw);
            BindBuffer(BufferTarget.ElementArrayBuffer, UNBIND);
            return model;
        }
        /// <inheritdoc />
        protected override void OnUnload()
        {

            base.OnUnload();
            foreach (var obj in SceneManager.CurrentScene.Objects)
            {
                DeleteVertexArray(obj.VAO);
                DeleteBuffer(obj.VBO);
                DeleteBuffer(obj.EBO);
            }
            foreach (var VARIABLE in textures) DeleteTexture(VARIABLE.Value);
            DeleteProgram(prog);
        }

        /// <inheritdoc />
        protected override unsafe void OnLoad()
        {
            base.OnLoad();

            //base.ClientLocation = new(SystemParameters.FullPrimaryScreenHeight / 2 - Size.X / 2, SystemInformation.VirtualScreen.Y/2-Size.Y/2);

            handle = Context.WindowPtr;
            CursorState = CursorState.Grabbed;

            #region OpenGL


            //Упаси боже ты здесь что то поменяешь 
            //foreach (var files in Directory.GetFiles(".", "*.obj"))
            //{
            //    var model = GenModel(new GameObject().LoadModelData(files));
            //    SceneManager.CurrentScene.Objects.Add(model);
            //    //GameObject.Models.Add(model);
            //}

            SceneManager.CurrentScene = SceneManager.Load("Scenes/level1/levelinfo.json");
            int errors = 0;
            prog = CreateProgram();
            foreach (var name in Directory.GetFiles("Shader"))
            {
                int shader = CreateShader(name.EndsWith("vert") ? ShaderType.VertexShader : ShaderType.FragmentShader);
                ShaderSource(shader, FromFile(name));
                CompileShader(shader);
                string log = GetShaderInfoLog(shader);
                Logger.PrintLine("Compiled shader " + name);
                //Logger.PrintLine( string.Join("", log.Split("\n").Select(s => s.Insert(0, "\n\t"))));
                int bad = 0;
                errors += bad = Math.Max(log.Split('\n').Count(l => l.Contains(": error")), 0);
                BadShaders.Add(name, bad);
                AttachShader(prog, shader);
                DeleteShader(shader);
            }
            BadShaders.Where(s => s.Value > 0).ToList().ForEach(f => Logger.PrintLine($"Bad shader:{f.Key} {f.Value} errors", LogLevel.Error));
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
                Logger.PrintLine($"Loaded texture(id:{id:D3}): {VARIABLE}");
                TextureSize += tex.Data.Length * sizeof(byte);
            }
            Enable(EnableCap.DepthTest);

            string pil = GetProgramInfoLog(prog);
            CenterWindow(new(550, 500));
            GLFW.ShowWindow(WindowPtr);
            #endregion

            Camera.Yaw = 180;
            Size = new(1280, 720);
            GC.Collect(); //?
        }

        /// <inheritdoc />
        protected override void OnFocusedChanged(FocusedChangedEventArgs e)
        {
            if (e.IsFocused)
            {
                Logger.PrintLine("focus");
                CursorState = CursorState.Grabbed;
            }
            base.OnFocusedChanged(e);
        }

        /// <inheritdoc />
        public override unsafe void Close() => CloseWindow(handle);//Environment.Exit(0);

        /// <inheritdoc />
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            CursorState = CursorState.Grabbed;
        }

        /// <inheritdoc />
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            Viewport(0, 0, Size.X, Size.Y);
        }

        public static bool ReloadRequired = false;
        public static List<GameObject> ReloadModel = new();

        void ReloadAll()
        {
            if (ReloadRequired)
            {
                ReloadRequired = false;
                foreach (var obj in SceneManager.CurrentScene.Objects)
                {
                    DeleteVertexArray(obj.VAO);
                    DeleteBuffer(obj.VBO);
                    DeleteBuffer(obj.EBO);
                }
                SceneManager.CurrentScene.Objects.ForEach(model =>
                {

                    model.VAO = GenVertexArray();
                    BindVertexArray(model.VAO);
                    model.VBO = GenBuffer();

                    BindBuffer(BufferTarget.ArrayBuffer, model.VBO);
                    BufferData(BufferTarget.ArrayBuffer, model.Vertices.Count * Vector3.SizeInBytes, model.Vertices.ToArray(), BufferUsageHint.StaticDraw);
                    VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
                    EnableVertexArrayAttrib(model.VAO, 0);
                    BindBuffer(BufferTarget.ArrayBuffer, 0);

                    model.TVBO = GenBuffer();
                    BindBuffer(BufferTarget.ArrayBuffer, model.TVBO);
                    BufferData(BufferTarget.ArrayBuffer, model.TexCoords.Count * sizeof(int), model.TexCoords.ToArray(),
                        BufferUsageHint.StaticDraw);


                    VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);
                    EnableVertexArrayAttrib(model.VAO, 1);

                    BindBuffer(BufferTarget.ArrayBuffer, 0);
                    BindVertexArray(0);


                    model.EBO = GenBuffer();
                    BindBuffer(BufferTarget.ElementArrayBuffer, model.EBO);
                    BufferData(BufferTarget.ElementArrayBuffer, model.Indices.Count * sizeof(uint),
                        model.Indices.ToArray(),
                        BufferUsageHint.StaticDraw);
                    BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                });
            }

        }

        /// <inheritdoc />
        protected override void OnJoystickConnected(JoystickEventArgs e)
        {
            base.OnJoystickConnected(e);
            Logger.PrintLine(JoystickStates[0].Name);
        }
        public int idJoystick = -1;
        /// <inheritdoc />
        protected override unsafe void OnRenderFrame(FrameEventArgs args)
        {

            if (ReloadRequired) ReloadAll();
            if (JoystickStates[0] != null)
            {
                var joystickState = JoystickStates.First();
                PlayerController.transform.Position2 += Camera.Front * speed * -(Math.Abs(Gamepad.Axis(Axis.LY)) > 0.1f ? Gamepad.Axis(Axis.LY) : 0); //W
                PlayerController.transform.Position2 -= Vector3.Normalize(Vector3.Cross(Camera.Front, Camera.Up)) * speed * -(Math.Abs(Gamepad.Axis(Axis.LX)) > 0.1f ? Gamepad.Axis(Axis.LX) : 0); //A
                if (joystickState.IsButtonDown(11)) PlayerController.transform.Position2 -= Camera.Up * speed; //Down
                if (joystickState.IsButtonDown(1)) PlayerController.transform.Position2 += Camera.Up * speed; //Down


                Camera.Yaw += (Math.Abs(Gamepad.Axis(Axis.RX)) > 0.2f ? Gamepad.Axis(Axis.RX) : 0) * 0.5f * Camera.Sensitivity;
                Camera.Pitch -= (Math.Abs(Gamepad.Axis(Axis.RY)) > 0.2f ? Gamepad.Axis(Axis.RY) : 0) * 0.5f * Camera.Sensitivity;
                if (Camera.Pitch > 89.9f)
                    Camera.Pitch = 89.9f;
                else if (Camera.Pitch < -89.9f)
                    Camera.Pitch = -89.9f;
                //else
                //Camera.Pitch -= -(Math.Abs(Gamepad.Axis(5)) > 0.2f ? Gamepad.Axis(5) : 0) * 0.5f * Camera.Sensitivity;

                Camera.Front.X = (float)Math.Cos(MathHelper.DegreesToRadians(Camera.Pitch)) *
                                 (float)Math.Cos(MathHelper.DegreesToRadians(Camera.Yaw));
                Camera.Front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(Camera.Pitch));
                Camera.Front.Z = (float)Math.Cos(MathHelper.DegreesToRadians(Camera.Pitch)) *
                                 (float)Math.Sin(MathHelper.DegreesToRadians(Camera.Yaw));
                Camera.Front = Vector3.Normalize(Camera.Front);


                Camera.Fov += (Gamepad.Axis(Axis.R2) + 1) * 36 * (float)args.Time;
                Camera.Fov -= (Gamepad.Axis(Axis.L2) + 1) * 36 * (float)args.Time;
            }
            if (KeyboardState.IsKeyDown(Keys.Escape)) CursorState = CursorState.Normal;
            speed = (float)args.Time * 2;
            fr++;
            time += (float)args.Time;
            ClearColor(Color4.Black);
            Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            UseProgram(prog);
            //BindTexture(TextureTarget.Texture2D, textures["side"]);
            foreach (var go in SceneManager.CurrentScene.Objects)
            {

                BindVertexArray(go.VAO);
                BindBuffer(BufferTarget.ElementArrayBuffer, go.EBO);

                int w = Size.X;
                int h = Size.Y;
                Matrix4 model = Matrix4.Identity;
                Matrix4 view = Matrix4.Identity;
                Matrix4 projecion = Matrix4.Identity;
                view = Matrix4.LookAt(PlayerController.transform.Position2, PlayerController.transform.Position2 + Camera.Front, Camera.Up);

                projecion = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Camera.Fov), w / (float)h, 0.01f, 200f);
                model = Matrix4.CreateRotationX(float.DegreesToRadians(go.Transform.Rotation.X)) *
                        Matrix4.CreateRotationY(float.DegreesToRadians(go.Transform.Rotation.Y)) *
                        Matrix4.CreateRotationZ(float.DegreesToRadians(go.Transform.Rotation.Z));

                Matrix4 translation = Matrix4.CreateTranslation(new Vector3(go.Transform.Position.X, go.Transform.Position.Y, -go.Transform.Position.Z) / 30);

                #region fun
                if (go.Sinusoida)
                    translation = Matrix4.CreateTranslation((new Vector3(go.Transform.Position.X,
                                                                 go.Transform.Position.Y = 20f * MathF.Sin(
                                                                     (float)Math.Abs(yrot += (float)args.Time * 2)),
                                                                 -go.Transform.Position.Z) /
                                                             30)) *
                                  Matrix4.CreateTranslation((new Vector3(
                                      go.Transform.Position.X = 35f * MathF.Sin((float)Math.Abs(xrot += (float)args.Time * 1)), go.Transform.Position.Y,
                                      -go.Transform.Position.Z) / 30));
                #endregion

                model *= translation;

                int modelLoc = GetUniformLocation(prog, "model");
                int modelView = GetUniformLocation(prog, "view");
                int modelProj = GetUniformLocation(prog, "projection");

                UniformMatrix4(modelLoc, true, ref model);
                UniformMatrix4(modelView, true, ref view);
                UniformMatrix4(modelProj, true, ref projecion);

                DrawElements(BeginMode.Triangles, go.Indices.Count, DrawElementsType.UnsignedInt, 0);
            }
            PlayerController.Look(this);
            KeyboardState input = KeyboardState;

            if (input.IsKeyDown(Keys.W))
                PlayerController.transform.Position2 += Camera.Front * speed; //W
            if (input.IsKeyDown(Keys.S))
                PlayerController.transform.Position2 -= Camera.Front * speed; //S
            if (input.IsKeyDown(Keys.A))
                PlayerController.transform.Position2 -= Vector3.Normalize(Vector3.Cross(Camera.Front, Camera.Up)) * speed; //A
            if (input.IsKeyDown(Keys.D))
                PlayerController.transform.Position2 += Vector3.Normalize(Vector3.Cross(Camera.Front, Camera.Up)) * speed; //D
            if (input.IsKeyDown(Keys.Space))
                PlayerController.transform.Position2 += Camera.Up * speed; //"  "
            if (input.IsKeyDown(Keys.LeftShift))
                PlayerController.transform.Position2 -= Camera.Up * speed; //Down

            if (ScreenshotRequired || KeyboardState.IsKeyPressed(Keys.F2))
            {
                ScreenshotRequired = false;
                var filename = $"screenshots/{DateTime.Now.ToString("G").Replace(" ", "_").Replace(":", "-")}.png";
                Finish();
                Flush();
                SwapBuffers();
                OnRefresh();
                TakeScreenshot().Save("aaa.png");
                Logger.PrintLine("Saved as " + filename);
            }

            if (GetError() != ErrorCode.NoError) Errors++;
            Frames++;
            if (Restart)
            {
                SceneManager.CurrentScene = new();
                foreach (var obj in SceneManager.CurrentScene.Objects)
                {
                    DeleteVertexArray(obj.VAO);
                    DeleteBuffer(obj.VBO);
                    DeleteBuffer(obj.EBO);
                }
                foreach (var VARIABLE in textures) DeleteTexture(VARIABLE.Value);
                DeleteProgram(prog);
                Overlay.Instance!.Close();
                Overlay.Instance.Dispose();
                Overlay.Instance = null;
                Instance = null;
                Close();
            }
            Context.SwapBuffers();
            base.OnRenderFrame(args);
            //Update(args);
        }
        public Bitmap TakeScreenshot()
        {
            int w = ClientSize.X;
            int h = ClientSize.Y;
            Bitmap bmp = new Bitmap(w, h);
            BitmapData data = bmp.LockBits(new(0, 0, w, h), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            ReadPixels(0, 0, w, h, PixelFormat.Rgb, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return bmp;
        } //todo: fix colors
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