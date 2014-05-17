﻿using OpenTK;
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

        ShaderProgram program2;

        GLObject Cube;

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


            program2 = new ShaderProgram();
            program2.AddShader(new Shader(ShaderType.VertexShader, LoadShader("Shaders/2d.vert")));
            program2.AddShader(new Shader(ShaderType.FragmentShader, LoadShader("Shaders/2d.frag")));

            if (!program2.Compile())
            {
                Console.WriteLine(program2.Log);
            }

            Cube = PrepareCube();

            InitMatrices();
            LoadFrameBuffer();

            Prepare2DScene(program2.ID);

        }

        private void Prepare2DScene(int program)
        {
            GL.UseProgram(program);
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

            ErrorCode error4 = GL.GetError();
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

            ErrorCode error7 = GL.GetError();
            vbos.Add(vbo);
            vaos.Add(vao);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            foreach (int i in programs) GL.DeleteProgram(i);
            foreach (int i in shaders) GL.DeleteShader(i);
            foreach (int i in vbos) GL.DeleteBuffer(i);
            foreach (int i in vaos) GL.DeleteVertexArray(i);

            Cube.Dispose();
            program2.Dispose();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            
            float multiplier = 1;
            if (Keyboard[Key.Space]) multiplier = 3;

            GL.Uniform1(Cube.Program.GetUniformLocation("time"), elapsed);

            elapsed += multiplier * (float)e.Time;
        }
        protected void RenderCube()
        {

            Cube.Use();

            GL.Enable(EnableCap.DepthTest);
            ModelMatrix = Matrix4.CreateRotationZ(elapsed);
            
            Cube.Program.UniformMatrix4("model", false, ref ModelMatrix);
            Cube.Program.Uniform1("opacity", 1.0f);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            GL.Enable(EnableCap.StencilTest);
            {
                // Draw floor
                ModelMatrix = Matrix4.Identity;
                Cube.Program.UniformMatrix4("model", false, ref ModelMatrix);
                GL.DepthMask(false);
                GL.StencilFunc(StencilFunction.Always, 1, 0xFF); // Write 1 to all drawn pixels
                GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
                GL.StencilMask(0xFF);
                GL.Clear(ClearBufferMask.StencilBufferBit);

                GL.DrawArrays(PrimitiveType.Triangles, 36, 6);
                GL.DepthMask(true);

                // Draw reflection
                ModelMatrix = Matrix4.CreateRotationZ(elapsed);
                var transform = Matrix4.Mult(Matrix4.CreateScale(1, 1, -1), Matrix4.CreateTranslation(0, 0, -1));

                Matrix4.Mult(ref transform, ref ModelMatrix, out ModelMatrix);

                Cube.Program.UniformMatrix4("model", false, ref ModelMatrix);
                Cube.Program.Uniform1("opacity", .5f);
                
                GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
                GL.StencilMask(0x00);
                GL.DepthMask(true);

                GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            }
            GL.Disable(EnableCap.StencilTest);
        }

        protected void RenderRect()
        {
            GL.UseProgram(program2.ID);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, ColorBuffer);

            GL.Disable(EnableCap.DepthTest);
           
            GL.BindVertexArray(vaos[0]);
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
        private GLObject PrepareCube()
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

            // Make shader
            var program1 = new ShaderProgram();
            program1.AddShader(new Shader(ShaderType.VertexShader, LoadShader("Shaders/basic.vert")));
            program1.AddShader(new Shader(ShaderType.FragmentShader, LoadShader("Shaders/basic.frag")));



            // Make textures
            var tex1 = new Texture(new Bitmap(GetResourceStream("Assets/info.png")), TextureTarget.Texture2D, PixelInternalFormat.Rgba, 0);
            
            tex1.TexParameter(TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            tex1.TexParameter(TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            tex1.TexParameter(TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
            tex1.TexParameter(TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            // Create mipmap (pre-rendered thumb)
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            var tex2 = new Texture(new Bitmap(GetResourceStream("Assets/play.png")), TextureTarget.Texture2D, PixelInternalFormat.Rgba, 1);
    
            tex2.TexParameter(TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            tex2.TexParameter(TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            tex2.TexParameter(TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
            tex2.TexParameter(TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            // Create mipmap (pre-rendered thumb)
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);



            List<PartitionRule> rules = new List<PartitionRule>()
            {
                new PartitionRule("position", 0, 3),
                new PartitionRule("color", 3, 3),
                new PartitionRule("texcoord", 6, 2)
            };

            GLObject obj = new GLObject(vertices, rules, program1);
            obj.AddTexture("tex1", tex1);
            obj.AddTexture("tex2", tex2);

            return obj;
        }

        private void InitMatrices() {
            ViewMatrix = Matrix4.LookAt(new Vector3(2.6f, 2.6f, 1.6f), new Vector3(0, 0, 0), new Vector3(0, 0, 1));
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(3.41f / 4, (float)Width / Height, 1.0f, 10.0f);
            
            Cube.Program.UniformMatrix4("model", false, ref ModelMatrix);
            Cube.Program.UniformMatrix4("view", false, ref ViewMatrix);
            Cube.Program.UniformMatrix4("projection", false, ref ProjectionMatrix);
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
