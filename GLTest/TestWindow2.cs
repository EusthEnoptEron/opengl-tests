using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace GLTest
{
    class TestWindow2 : GameWindow
    {
        private List<int> vbos = new List<int>();
        private List<int> vaos = new List<int>();
        private List<int> shaders = new List<int>();
        private List<int> programs = new List<int>();

        int timeUniform = 0;

        float elapsed = 0;
        private Matrix4 ModelMatrix = Matrix4.Identity;
        private Matrix4 ViewMatrix = Matrix4.Identity;
        private Matrix4 ProjectionMatrix = Matrix4.Identity;

        public TestWindow2()
            : base(800, 600)
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            VSync = VSyncMode.On;
            this.WindowBorder = OpenTK.WindowBorder.Fixed;
            LoadShaders();
            PrepareScene();
            InitMatrices();

            timeUniform = GL.GetUniformLocation(programs[0], "time");

        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            foreach (int i in programs) GL.DeleteProgram(i);
            foreach (int i in shaders) GL.DeleteShader(i);
            foreach (int i in vbos) GL.DeleteBuffer(i);
            foreach (int i in vaos) GL.DeleteVertexArray(i);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            
            float multiplier = 1;
            if (Keyboard[Key.Space]) multiplier = 3;

            GL.Uniform1(timeUniform, elapsed);

            ModelMatrix = Matrix4.CreateRotationZ( elapsed );
            GL.UniformMatrix4(GL.GetUniformLocation(programs[0], "model"), false, ref ModelMatrix);

            elapsed += multiplier * (float)e.Time;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            //GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
         
            SwapBuffers();
        }


        private void PrepareScene()
        {
            float[] vertices = new float[] {
                -0.5f,  0.5f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, // Top-left
                 0.5f,  0.5f, 0.0f, 1.0f, 0.0f, 2.0f, 0.0f, // Top-right
                 0.5f, -0.5f, 0.0f, 0.0f, 1.0f, 2.0f, 2.0f, // Bottom-right
                -0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 0.0f, 2.0f  // Bottom-left
            };

            uint[] elements = new uint[] { 0, 1, 2,    2, 3, 0 };
   
            int vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            int vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            int ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(elements.Length * sizeof(uint)), elements, BufferUsageHint.StaticDraw);


            int posAttr = GL.GetAttribLocation(programs[0], "position");
            int colAttr = GL.GetAttribLocation(programs[0], "color");
            int texAttr = GL.GetAttribLocation(programs[0], "texcoord");
            GL.EnableVertexAttribArray(posAttr);
            // Tell the program how to interpret the input values (2 values of the type float will be interpreted as a vertex)
            // Stride: bytes between each position in the array
            // Offset: ...offset
            // IMPORTANT: this will also store the current VBO!
            GL.VertexAttribPointer(posAttr, 2, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);

            GL.EnableVertexAttribArray(colAttr);
            GL.VertexAttribPointer(colAttr, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 2 * sizeof(float));

            GL.EnableVertexAttribArray(texAttr);
            GL.VertexAttribPointer(texAttr, 2, VertexAttribPointerType.Float, false, 7 * sizeof(float), 5 * sizeof(float));


            LoadPicture("Assets/info.png", 0, TextureUnit.Texture0, "tex1");
            LoadPicture("Assets/play.png", 1, TextureUnit.Texture1, "tex2");

            vbos.Add(vbo);
            vbos.Add(ebo);
            vaos.Add(vao);
        }

        private void InitMatrices() {
            GL.UniformMatrix4(GL.GetUniformLocation(programs[0], "model"), false, ref ModelMatrix);

            ViewMatrix = Matrix4.LookAt(new Vector3(1.2f, 1.2f, 1.2f), new Vector3(0, 0, 0), new Vector3(0, 0, 1));
            GL.UniformMatrix4(GL.GetUniformLocation(programs[0], "view"), false, ref ViewMatrix);

            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(3.41f / 4, (float)Width / Height, 1.0f, 10.0f);
            GL.UniformMatrix4(GL.GetUniformLocation(programs[0], "projection"), false, ref ProjectionMatrix);

        }

        private void LoadPicture(string resName, int number, TextureUnit unit, string uniformName)
        {
            int tex = GL.GenTexture();
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, tex);
            
            using (Bitmap img = new Bitmap(GetResourceStream(resName)))
            {
                BitmapData data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, img.Width, img.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                img.UnlockBits(data);
            }


            // x,y,z => s,t,r
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            
            // How to resize the texture
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
           
            // Create mipmap (pre-rendered thumb)
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);


            GL.Uniform1(GL.GetUniformLocation(programs[0], uniformName), number);
        }

        private void LoadShaders()
        {

            // Create vert and frag shaders
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);

            // Load them
            GL.ShaderSource(vertexShader, LoadShader("Shaders/basic.vert"));
            GL.ShaderSource(fragmentShader, LoadShader("Shaders/basic.frag"));

            // Compile them
            GL.CompileShader(vertexShader);
            GL.CompileShader(fragmentShader);

            var log1 = GL.GetShaderInfoLog(vertexShader);
            var log2 = GL.GetShaderInfoLog(fragmentShader);


            // Add them to a program
            int shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);

            // Unnecessary
            //GL.BindAttribLocation(shaderProgram, 0, "position");
            //GL.BindFragDataLocation(shaderProgram, 0, "outColor");

            // Link and user program
            GL.LinkProgram(shaderProgram);
            var log3 = GL.GetProgramInfoLog(shaderProgram);
            GL.UseProgram(shaderProgram);

            shaders.Add(vertexShader);
            shaders.Add(fragmentShader);
            programs.Add(shaderProgram);

            Console.WriteLine("Compiled shaders. Log:\n");
            Console.Write(log1);
            Console.Write(log2);
            Console.Write(log3);
            Console.WriteLine("---------------------------");
        }


        private string LoadShader(string name)
        {
            using(StreamReader reader = new StreamReader(GetResourceStream(name))) {
                return reader.ReadToEnd();
            }
        }

        private static UnmanagedMemoryStream GetResourceStream(string resName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var strResources = assembly.GetName().Name + ".g.resources";
            var rStream = assembly.GetManifestResourceStream(strResources);
            var resourceReader = new ResourceReader(rStream);
            var items = resourceReader.OfType<DictionaryEntry>();
            var stream = items.First(x => (x.Key as string) == resName.ToLower()).Value;
            return (UnmanagedMemoryStream)stream;
        }

    }
}
