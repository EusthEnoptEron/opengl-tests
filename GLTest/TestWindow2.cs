using OpenTK;
using OpenTK.Graphics;
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
        private List<int> textures = new List<int>();
        int timeUniform = 0;

        float elapsed = 0;
        private Matrix4 ModelMatrix = Matrix4.Identity;
        private Matrix4 ViewMatrix = Matrix4.Identity;
        private Matrix4 ProjectionMatrix = Matrix4.Identity;

        private int Framebuffer;
        private int ColorBuffer;

        public TestWindow2()
            : base(800, 600, new OpenTK.Graphics.GraphicsMode(ColorFormat.Empty, 24, 8, 4))
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            VSync = VSyncMode.On;
            this.WindowBorder = OpenTK.WindowBorder.Fixed;
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            LoadShaders("Shaders/basic.vert", "Shaders/basic.frag");
            LoadShaders("Shaders/2d.vert", "Shaders/2d.frag");

            PrepareScene(programs[0]);

            InitMatrices();
            LoadFrameBuffer();

            Prepare2DScene(programs[1]);
            
            timeUniform = GL.GetUniformLocation(programs[0], "time");

        }

        private void Prepare2DScene(int program)
        {
            float[] vertices = new float[] {
                //X     Y    
                -0.5f, -0.5f, 0, 0,
                 0.5f, -0.5f, 1, 0,
                 0.5f,  0.5f, 1, 1,

                 0.5f,  0.5f, 1, 1,
                -0.5f,  0.5f, 0, 1,
                -0.5f, -0.5f, 0, 0
            };

            int vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

           

            int vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            int posAttr = GL.GetAttribLocation(program, "position");
            int texAttr = GL.GetAttribLocation(program, "texcoord");

            GL.EnableVertexAttribArray(posAttr);
            GL.EnableVertexAttribArray(texAttr);
            // Tell the program how to interpret the input values (2 values of the type float will be interpreted as a vertex)
            // Stride: bytes between each position in the array
            // Offset: ...offset
            // IMPORTANT: this will also store the current VBO!
            GL.VertexAttribPointer(posAttr, 2, VertexAttribPointerType.Float, false, sizeof(float) * 4, 0);
            GL.VertexAttribPointer(texAttr, 2, VertexAttribPointerType.Float, false, sizeof(float) * 4, 2 * sizeof(float));

            GL.Uniform1(GL.GetUniformLocation(program, "texFramebuffer"), 0);

            vbos.Add(vbo);
            vaos.Add(vao);
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

            GL.UseProgram(programs[0]);
            GL.Uniform1(timeUniform, elapsed);

            elapsed += multiplier * (float)e.Time;
        }
        protected void RenderCube()
        {
            GL.UseProgram(programs[0]);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textures[0]);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, textures[1]);

            GL.Enable(EnableCap.DepthTest);

            int modelUni = GL.GetUniformLocation(programs[0], "model");
            int opacity = GL.GetUniformLocation(programs[0], "opacity");

            GL.BindVertexArray(vaos[0]);
            // Draw cube

            ModelMatrix = Matrix4.CreateRotationZ(elapsed);
            GL.UniformMatrix4(modelUni, false, ref ModelMatrix);
            GL.Uniform1(opacity, 1.0f);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            GL.Enable(EnableCap.StencilTest);
            {
                // Draw floor
                ModelMatrix = Matrix4.Identity;
                GL.UniformMatrix4(modelUni, false, ref ModelMatrix);
                GL.DepthMask(false);
                GL.StencilFunc(StencilFunction.Always, 1, 0xFF); // Write 1 to all drawn pixels
                GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
                GL.StencilMask(0xFF);
                GL.Clear(ClearBufferMask.StencilBufferBit);

                GL.DrawArrays(PrimitiveType.Triangles, 36, 6);
                GL.DepthMask(true);

                // Draw reflection
                GL.UniformMatrix4(modelUni, false, ref ModelMatrix);
                ModelMatrix = Matrix4.CreateRotationZ(elapsed);
                var transform = Matrix4.Mult(Matrix4.CreateScale(1, 1, -1), Matrix4.CreateTranslation(0, 0, -1));
                //var transform = Matrix4.CreateScale(1, 1, -1);
                Matrix4.Mult(ref transform, ref ModelMatrix, out ModelMatrix);

                GL.UniformMatrix4(modelUni, false, ref ModelMatrix);
                GL.Uniform1(opacity, .5f);
                GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
                GL.StencilMask(0x00);
                GL.DepthMask(true);

                GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            }
            GL.Disable(EnableCap.StencilTest);
        }

        protected void RenderRect()
        {
            GL.UseProgram(programs[1]);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, ColorBuffer);

            GL.Disable(EnableCap.DepthTest);
           
            GL.BindVertexArray(vaos[1]);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Framebuffer);
            GL.ClearColor(Color.White);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            RenderCube();


            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.ClearColor(Color.White);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            RenderRect();
            

            //GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }

        private void LoadFrameBuffer()
        {
            int fb = Framebuffer = GL.GenFramebuffer();
            GL.BindFramebuffer( FramebufferTarget.Framebuffer, fb );

            // Create texture that will hold our colors
            ColorBuffer = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, ColorBuffer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, 800, 600, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            // Attach to framebuffer
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, ColorBuffer, 0);

            // Create a render buffer for depth & stencil
            int depthStencilBuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthStencilBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, 800, 600);

            // Attach to framebuffer
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, depthStencilBuffer);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            
        }
        private void PrepareScene(int program)
        {
            float[] vertices = new float[] {
                //X     Y      Z     R      G    B     U     V
                -0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                 0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 1.0f, 1.0f, 0.0f,
                 0.5f,  0.5f, -0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f,
                 0.5f,  0.5f, -0.5f, 0.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                -0.5f,  0.5f, -0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f,
                -0.5f, -0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f,

                -0.5f, -0.5f,  0.5f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                 0.5f, -0.5f,  0.5f, 1.0f, 0.0f, 1.0f, 1.0f, 0.0f,
                 0.5f,  0.5f,  0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f,
                 0.5f,  0.5f,  0.5f, 0.0f, 1.0f, 1.0f, 1.0f, 1.0f,
                -0.5f,  0.5f,  0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f,
                -0.5f, -0.5f,  0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f,

                -0.5f,  0.5f,  0.5f, 1.0f, 1.0f, 0.0f, 1.0f, 0.0f,
                -0.5f,  0.5f, -0.5f, 1.0f, 0.0f, 1.0f, 1.0f, 1.0f,
                -0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f,
                -0.5f, -0.5f, -0.5f, 0.0f, 1.0f, 1.0f, 0.0f, 1.0f,
                -0.5f, -0.5f,  0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f,
                -0.5f,  0.5f,  0.5f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f,

                 0.5f,  0.5f,  0.5f, 1.0f, 1.0f, 0.0f, 1.0f, 0.0f,
                 0.5f,  0.5f, -0.5f, 1.0f, 0.0f, 1.0f, 1.0f, 1.0f,
                 0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f,
                 0.5f, -0.5f, -0.5f, 0.0f, 1.0f, 1.0f, 0.0f, 1.0f,
                 0.5f, -0.5f,  0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f,
                 0.5f,  0.5f,  0.5f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f,

                -0.5f, -0.5f, -0.5f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f,
                 0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 1.0f, 1.0f, 1.0f,
                 0.5f, -0.5f,  0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f,
                 0.5f, -0.5f,  0.5f, 0.0f, 1.0f, 1.0f, 1.0f, 0.0f,
                -0.5f, -0.5f,  0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f,
                -0.5f, -0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f,

                -0.5f,  0.5f, -0.5f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f,
                 0.5f,  0.5f, -0.5f, 1.0f, 0.0f, 1.0f, 1.0f, 1.0f,
                 0.5f,  0.5f,  0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f,
                 0.5f,  0.5f,  0.5f, 0.0f, 1.0f, 1.0f, 1.0f, 0.0f,
                -0.5f,  0.5f,  0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f,
                -0.5f,  0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f,


                -1.0f, -1.0f, -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                 1.0f, -1.0f, -0.5f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f,
                 1.0f,  1.0f, -0.5f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f,
                 1.0f,  1.0f, -0.5f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f,
                -1.0f,  1.0f, -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f,
                -1.0f, -1.0f, -0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f
            };

            uint[] elements = new uint[] { 0, 1, 2,    2, 3, 0 };
   
            int vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            GL.UseProgram(program);

            int vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            int ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(elements.Length * sizeof(uint)), elements, BufferUsageHint.StaticDraw);


            int posAttr = GL.GetAttribLocation(program, "position");
            int colAttr = GL.GetAttribLocation(program, "color");
            int texAttr = GL.GetAttribLocation(program, "texcoord");
            GL.EnableVertexAttribArray(posAttr);
            // Tell the program how to interpret the input values (2 values of the type float will be interpreted as a vertex)
            // Stride: bytes between each position in the array
            // Offset: ...offset
            // IMPORTANT: this will also store the current VBO!
            GL.VertexAttribPointer(posAttr, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

            GL.EnableVertexAttribArray(colAttr);
            GL.VertexAttribPointer(colAttr, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));

            GL.EnableVertexAttribArray(texAttr);
            GL.VertexAttribPointer(texAttr, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));


            textures.Add(LoadPicture("Assets/info.png", 0, TextureUnit.Texture0, "tex1"));
            textures.Add(LoadPicture("Assets/play.png", 1, TextureUnit.Texture1, "tex2"));

            vbos.Add(vbo);
            vbos.Add(ebo);
            vaos.Add(vao);
        }

        private void InitMatrices() {
            GL.UniformMatrix4(GL.GetUniformLocation(programs[0], "model"), false, ref ModelMatrix);

            ViewMatrix = Matrix4.LookAt(new Vector3(2.6f, 2.6f, 1.6f), new Vector3(0, 0, 0), new Vector3(0, 0, 1));
            GL.UniformMatrix4(GL.GetUniformLocation(programs[0], "view"), false, ref ViewMatrix);

            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(3.41f / 4, (float)Width / Height, 1.0f, 10.0f);
            GL.UniformMatrix4(GL.GetUniformLocation(programs[0], "projection"), false, ref ProjectionMatrix);

        }

        private int LoadPicture(string resName, int number, TextureUnit unit, string uniformName)
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

            return tex;
        }

        private void LoadShaders(string vertexPath, string fragmentPath)
        {

            // Create vert and frag shaders
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);

            // Load them
            GL.ShaderSource(vertexShader, LoadShader(vertexPath));
            GL.ShaderSource(fragmentShader, LoadShader(fragmentPath));

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
