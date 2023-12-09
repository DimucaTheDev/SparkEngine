using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpentkGraphics;
using StbImageSharp;
using System.Drawing;
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
        private float[] _vert =
        {
            -0.5f,0.5f,0,
            0.5f,0.5f,0,
            -0.5f,-0.5f,0,
            0.5f,0.5f,0,
            0.5f,-0.5f,0,
            -0.5f,-0.5f,0
            /*
            -0.7f,0.7f,0,
            0.7f,0.7f,0,
            0.7f,-0.7f,0,
            -0.7f,-0.7f,0
            */
        };

        private List<Vector3> vert = new()
        {
            new(-0.5f,0.5f,-0.5f),
            new(0.5f,0.5f,-0.5f),
            new(0.5f,-0.5f,-0.5f),
            new(-0.5f,-0.5f,-0.5f)
        };

        private uint[] indices =
        {
            1,2,3,4,5,6
        };
        private int textureID;

        static void Main(string[] args_) => new Program().Run();
        /// <inheritdoc />
        public Program() : base(new() {  }, new() { Title = "title", WindowState = WindowState.Normal }) => CenterWindow(new(700, 500));

        private OBJModel model;
        private int loc, loc2, loc3;
        private double timer = 0;
        /// <inheritdoc />
        protected override void OnLoad()
        {
            base.OnLoad();

            model = OBJModel.LoadObjFile("untitled.obj");
            //vert = OBJModel.VecToFloat(model);
            #region OpenGL
            vao = GenVertexArray();
            BindVertexArray(vao);
            vbo = GenBuffer();


            BindBuffer(BufferTarget.ArrayBuffer, vbo);
            BufferData(BufferTarget.ArrayBuffer, vert.Count * sizeof(float), OBJModel.VecToFloat(vert), BufferUsageHint.StaticDraw);
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
            BufferData(BufferTarget.ElementArrayBuffer, indices.Length * Vector3.SizeInBytes, indices, BufferUsageHint.StaticDraw);
            BindBuffer(BufferTarget.ElementArrayBuffer, UNBIND);

            prog = CreateProgram();
            foreach (var name in Directory.GetFiles("Shader"))
            {
                int shader = CreateShader(name.EndsWith("vert") ? ShaderType.VertexShader : ShaderType.FragmentShader);
                ShaderSource(shader, FromFile(name));
                CompileShader(shader);
                Console.WriteLine("Compiled shader " + name);
                string log = "";
                if((log = GetShaderInfoLog(shader)) != "") Console.WriteLine(log);
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

        private float r = 5;// { get => (float)Random.Shared.NextDouble(); }
        private float yrot = 0;
        /// <inheritdoc />
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            timer += args.Time;
            ClearColor(Color4.Gray);
            Clear(ClearBufferMask.ColorBufferBit);

            UseProgram(prog);
            VertexAttrib1(loc, r);
            VertexAttrib1(loc2, r);
            VertexAttrib1(loc3, r);
            BindTexture(TextureTarget.Texture2D, textureID);

            BindVertexArray(vao);
            BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            int w = Size.X;
            int h = Size.Y;
            Matrix4 model = Matrix4.Identity;
            Matrix4 view = Matrix4.Identity;
            Matrix4 projecion = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60), w / h, 0.1f, 100f);

            model = Matrix4.CreateRotationY(yrot>=365?yrot=0:yrot+=1*(float)args.Time);

            Matrix4 translation = Matrix4.CreateTranslation(0, 0, -3f);
            model *= translation;

            int modelLoc = GetUniformLocation(prog, "model");
            int modelView = GetUniformLocation(prog, "view");
            int modelProj = GetUniformLocation(prog, "projection");

            UniformMatrix4(modelLoc, true, ref model);
            UniformMatrix4(modelView, true, ref view);
            UniformMatrix4(modelProj, true, ref projecion);

            DrawElements(BeginMode.TriangleFan, indices.Length, DrawElementsType.UnsignedInt, 0);

            Context.SwapBuffers();
            base.OnRenderFrame(args);
        }

        string FromFile(string path) => File.ReadAllText(path);
    }
}