using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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


        float elapsed = 0;

        public TestWindow2()
            : base(800, 600)
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            VSync = VSyncMode.On;
            this.WindowBorder = OpenTK.WindowBorder.Fixed;
            PrepareScene();
            LoadShaders();

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
            elapsed += (float)e.Time;
            float red = (float)(Math.Sin(elapsed * Math.PI) + 1) / 2f;
            GL.Uniform4( GL.GetUniformLocation(programs[0], "color"), red,0f,0f,1f);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {

            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
         
            SwapBuffers();
        }


        private void PrepareScene()
        {
            float[] vertices = new float[] {
                0.0f,  0.5f, // Vertex 1 (X, Y)
                 0.5f, -0.5f, // Vertex 2 (X, Y)
                -0.5f, -0.5f  // Vertex 3 (X, Y)
            };
   
            int vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            int vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            vbos.Add(vbo);
            vaos.Add(vao);
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
            GL.BindAttribLocation(shaderProgram, 0, "position");
            //GL.BindFragDataLocation(shaderProgram, 0, "outColor");

            // Link and user program
            GL.LinkProgram(shaderProgram);
            var log3 = GL.GetProgramInfoLog(shaderProgram);
            GL.UseProgram(shaderProgram);


            GL.EnableVertexAttribArray(0);
            // Tell the program how to interpret the input values (2 values of the type float will be interpreted as a vertex)
            // Stride: bytes between each position in the array
            // Offset: ...offset
            // IMPORTANT: this will also store the current VBO!
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, 0);

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
