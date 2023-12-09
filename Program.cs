using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpentkGraphics;
using StbImageSharp;
using static OpenTK.Graphics.OpenGL.GL;
using Color4 = OpenTK.Mathematics.Color4;
namespace RotatingCube
{
    class Program : GameWindow
    {
        private int vao;
        private int vbo;
        private int tVbo;
        private int ebo;
        private const int UNBIND = 0;
        private int prog;
        private float[] texCoords =
        {
            0,0.7f,
            0.7f,0.7f,
            0.7f,0,
            0,0
        };
        private float[] vert =
        {
            -1,-1,0,
            0,1,0,
            1,-1,0
            /*
            -0.7f,0.7f,0,
            0.7f,0.7f,0,
            0.7f,-0.7f,0,
            -0.7f,-0.7f,0
            */
        };

        private uint[] indices =
        {
            0, 1, 2
        };
        private int textureID;

        static void Main(string[] args_) => new Program().Run();
        /// <inheritdoc />
        public Program() : base(new(){UpdateFrequency = 3}, new() { Title = "title", WindowState = WindowState.Normal }) => CenterWindow(new(500, 500));

        private OBJModel model;
        private int loc,loc2,loc3;
        private double timer=0;
        /// <inheritdoc />
        protected override void OnLoad()
        {
            base.OnLoad();

            //model = OBJModel.LoadObjFile("untitled.obj");
            //vert = OBJModel.VecToFloat(model);
            #region OpenGL
            vao = GenVertexArray();
            BindVertexArray(vao);
            vbo = GenBuffer();


            BindBuffer(BufferTarget.ArrayBuffer, vbo);
            BufferData(BufferTarget.ArrayBuffer, vert.Length * sizeof(float), vert, BufferUsageHint.StaticDraw);
            //BindBuffer(BufferTarget.ArrayBuffer, UNBIND);

            VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            EnableVertexArrayAttrib(vao, 0);
            BindBuffer(BufferTarget.ArrayBuffer, UNBIND);

            tVbo = GenBuffer();
            BindBuffer(BufferTarget.ArrayBuffer, tVbo);
            BufferData(BufferTarget.ArrayBuffer, texCoords.Length * sizeof(float), texCoords, BufferUsageHint.StaticDraw);




            VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 0, 0);
            EnableVertexArrayAttrib(vao, 1);

            BindBuffer(BufferTarget.ArrayBuffer, UNBIND);
            BindVertexArray(UNBIND);


            ebo = GenBuffer();
            BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
            BindBuffer(BufferTarget.ElementArrayBuffer, UNBIND);

            prog = CreateProgram();
            foreach (var name in Directory.GetFiles("Shader"))
            {
                int shader = CreateShader(name.EndsWith("vert") ? ShaderType.VertexShader : ShaderType.FragmentShader);
                ShaderSource(shader, FromFile(name));
                CompileShader(shader);
                Console.WriteLine("Compiled shader " + name);
                Console.WriteLine(GetShaderInfoLog(shader));
                AttachShader(prog, shader);
                DeleteShader(shader);
            }
            LinkProgram(prog);


            textureID = GenTexture();
            ActiveTexture(TextureUnit.Texture0);
            BindTexture(TextureTarget.Texture2D, textureID);

            TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
            TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            StbImage.stbi_set_flip_vertically_on_load(1);
            ImageResult tex = ImageResult.FromStream(File.OpenRead("Textures/image.png"), ColorComponents.RedGreenBlueAlpha);
            TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, tex.Width, tex.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, tex.Data);
            BindTexture(TextureTarget.Texture2D, UNBIND);
            loc = GetAttribLocation(prog, "aRandom1");
            loc2 = GetAttribLocation(prog, "aRandom2");
            loc3 = GetAttribLocation(prog, "aRandom3");
            Console.WriteLine(GetProgramInfoLog(prog));
            #endregion
        }

        /// <inheritdoc />
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            Viewport(0, 0, e.Width, e.Height);
        }

        /// <inheritdoc />
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            timer += args.Time;
            ClearColor(Color4.Gray);
            Clear(ClearBufferMask.ColorBufferBit);

            UseProgram(prog);
            VertexAttrib1(loc, (float)Random.Shared.NextDouble());
            VertexAttrib1(loc2, (float)Random.Shared.NextDouble());
            VertexAttrib1(loc3, (float)Random.Shared.NextDouble());
            BindTexture(TextureTarget.Texture2D, textureID);

            BindVertexArray(vao);
            BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            DrawElements(BeginMode.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

            Context.SwapBuffers();
            base.OnRenderFrame(args);
        }

        string FromFile(string path) => File.ReadAllText(path);
    }
}