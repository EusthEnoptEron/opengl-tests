using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLTest
{
    public class AsyncTextureTestWindow : GameWindow
    {
        GLObject _Object;
        double _Time = 0;
        private string[] bigAssPixtures = new string[] {
            //@"E:\文学\漫画\私がモテないのはどう考えてもお前らが悪い\私喪① p011.png"
            @"E:\文学\漫画\Black_Lagoon_v10b\10\001.jpg",
            @"E:\文学\漫画\Black_Lagoon_v10b\10\002.jpg",
            @"E:\文学\漫画\Black_Lagoon_v10b\10\003.jpg",
            @"E:\文学\漫画\Black_Lagoon_v10b\10\004.jpg",
            @"E:\文学\漫画\Black_Lagoon_v10b\10\005.jpg",
            @"E:\文学\漫画\Black_Lagoon_v10b\10\006.jpg"

        };
        private Random R = new Random();

        public AsyncTextureTestWindow() : base(800, 600) {
        
        }

        protected override void OnLoad(EventArgs e)
        {
            // Set up window
            VSync = VSyncMode.On;
            this.WindowBorder = OpenTK.WindowBorder.Fixed;
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.AlphaTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Enable(EnableCap.CullFace);

            _Object = MakeQuad(0.8f, 0.8f);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            _Time += e.Time;
            Matrix4 camera = Matrix4.LookAt(new Vector3((float)Math.Sin(_Time), 0, 1), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            _Object.Program.UniformMatrix4("view", false, ref camera);
        }

        protected override void OnKeyDown(OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenTK.Input.Key.Space)
            {
                _Object.ClearTextures();
                _Object.AddTexture("tex", new AsyncTexture(bigAssPixtures[R.Next(0, bigAssPixtures.Length - 1)]));
            }
            if (e.Key == OpenTK.Input.Key.Enter)
            {
                _Object.ClearTextures();
                using (var img = Image.FromFile(bigAssPixtures[R.Next(0, bigAssPixtures.Length - 1)]))
                {
                    _Object.AddTexture("tex", new Texture(img, TextureTarget.Texture2D, PixelInternalFormat.Rgba, 0));
                }
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(Color.LightBlue);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _Object.Use();
            GL.DrawArrays(PrimitiveType.Triangles, 0, _Object.Length);

            SwapBuffers();
        }

        private GLObject MakeQuad(float width, float height)
        {
            int stride = 4;
            Vector2 topLeft = new Vector2(-width/2, height/2);
            Vector2 topRight = new Vector2(width/2, height/2);
            Vector2 bottomRight = new Vector2(width/2, -height/2);
            Vector2 bottomLeft = new Vector2(-width/2, -height/2);

            Vector2[] vertices = new Vector2[] {
                topLeft, bottomLeft, topRight, // first triangle
                topRight, bottomLeft, bottomRight

            };

            float[] data = new float[vertices.Length * stride];

            for (int i = 0; i < vertices.Length; i++)
            {
                int j = i * stride;
                Vector2 v = vertices[i];

                // X / Z
                data[j] = v.X;
                data[j + 1] = v.Y;

                // U / V
                data[j + 2] = v.X / width;
                data[j + 3] = -v.Y / height;
            }

            Matrix4 model = Matrix4.Identity;//Matrix4.CreateRotationX(3.41f);
            Matrix4 camera = Matrix4.LookAt(new Vector3(0, 0, 1), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(3.41f / 4, Width / (float)Height, 0.1f, 10000);

            // Calc shader
            ShaderProgram program = new ShaderProgram();
            program.AddShader(new Shader(ShaderType.VertexShader, Helper.LoadShader("Shaders/simple.vert")));
            program.AddShader(new Shader(ShaderType.FragmentShader, Helper.LoadShader("Shaders/simple.frag")));


            if (!program.Compile())
            {
                throw new Exception(program.Log);
            }

            program.UniformMatrix4("model", false, ref model);
            program.UniformMatrix4("view", false, ref camera);
            program.UniformMatrix4("projection", false, ref proj);

            //Texture image = new Texture(img, TextureTarget.Texture2D, PixelInternalFormat.Rgb, 0);
            //image.TexParameter(TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            //image.TexParameter(TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            //image.TexParameter(TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            //image.TexParameter(TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            // Make instance
            GLObject obj = new GLObject(data, new List<PartitionRule>()
            {
                new PartitionRule("position", 0, 2),
                new PartitionRule("texcoord", 2, 2)
            }, program);

            //obj.AddTexture("image", image);
            return obj;
        }

    }
}
