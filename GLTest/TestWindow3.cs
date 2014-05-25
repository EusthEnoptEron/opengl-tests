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
    class TestWindow3 : GameWindow
    {
        internal TestWindow3() : base(800, 600) { }

        Image image;
        GLObject myObject;
        Matrix4 camera;

        protected override void OnLoad(EventArgs e)
        {
            // Configure
            VSync = VSyncMode.On;
            WindowBorder = OpenTK.WindowBorder.Fixed;
            GL.Enable(EnableCap.DepthTest);

            // Init scene
            image = Image.FromStream(Helper.GetResourceStream("Assets/cover.jpg"));
            myObject = MakeObject(image);

            // Wire up handlers
            this.Mouse.WheelChanged += (s, e2) => {
                Matrix4 translation = Matrix4.CreateTranslation(0, 0, e2.DeltaPrecise*5);
                //Matrix4.Mult(ref camera, ref translation, out camera);
                Matrix4.Mult(ref translation, ref camera, out camera);
            };
            bool rotating = false;
            bool panning = false;
            this.Mouse.ButtonDown += (s, e2) =>
            {
                if (e2.Button == OpenTK.Input.MouseButton.Left) rotating = true;
                if (e2.Button == OpenTK.Input.MouseButton.Right) panning = true;

            };
            Mouse.ButtonUp += (s, e2) => {
                if (e2.Button == OpenTK.Input.MouseButton.Left) rotating = false;
                if (e2.Button == OpenTK.Input.MouseButton.Right) panning = false;
            };

            this.Mouse.Move += (s, e2) =>
            {
                if (rotating)
                {

                    Matrix4 rotation = Matrix4.CreateRotationY(-e2.XDelta / 30f);
                    Matrix4 rotation2 = Matrix4.CreateRotationX(-e2.YDelta / 30f);


                    Matrix4.Mult(ref camera, ref rotation, out camera);
                    Matrix4.Mult(ref camera, ref rotation2, out camera);
                }
                if (panning)
                {
                    Matrix4 translation = Matrix4.CreateTranslation(e2.XDelta, 0, e2.YDelta);
                    Matrix4.Mult(ref camera, ref translation, out camera);
                }
            };

        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // Update camera
            myObject.Program.UniformMatrix4("view", false, ref camera);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(Color.LightBlue);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            myObject.Use();
            GL.DrawArrays(PrimitiveType.Triangles, 0, myObject.Length);
            
            SwapBuffers();
        }



        private GLObject MakeObject(Image img)
        {
            int width = img.Width;
            int height = img.Height;
            int stride = 4;
            Vector2[] vertices = MakePlane(width, height, width /2, height / 2);
            float[] data = new float[vertices.Length * stride];

            for (int i = 0; i < vertices.Length; i++)
            {
                int j = i * stride;
                Vector2 v = vertices[i];

                // X / Z
                data[j] = v.X;
                data[j+1] = v.Y;

                // U / V
                data[j + 2] = v.X / width;
                data[j + 3] = -v.Y / height;
            }

            Matrix4 model = Matrix4.Identity;//Matrix4.CreateRotationX(3.41f);
            camera = Matrix4.LookAt(new Vector3(700, 700, -400), new Vector3(600, 0, -300), new Vector3(0, 1, 0));
            Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(3.41f / 4, Width / (float)Height, 0.1f, 10000);
            
            // Calc shader
            ShaderProgram program = new ShaderProgram();
            program.AddShader(new Shader(ShaderType.VertexShader, Helper.LoadShader("Shaders/picture.vert")));
            program.AddShader(new Shader(ShaderType.FragmentShader, Helper.LoadShader("Shaders/picture.frag")));


            if (!program.Compile())
            {
                throw new Exception(program.Log);
            }

            program.UniformMatrix4("model", false, ref model);
            program.UniformMatrix4("view", false, ref camera);
            program.UniformMatrix4("projection", false, ref proj);

            Texture image = new Texture(img, TextureTarget.Texture2D, PixelInternalFormat.Rgb, 0);
            image.TexParameter(TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            image.TexParameter(TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            image.TexParameter(TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            image.TexParameter(TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            // Make instance
            GLObject obj = new GLObject(data, new List<PartitionRule>()
            {
                new PartitionRule("position", 0, 2),
                new PartitionRule("texcoord", 2, 2)
            }, program);

            obj.AddTexture("image", image);
            return obj;
        }

        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="xSegments"></param>
        /// <param name="ySegments"></param>
        /// <returns></returns>
        private Vector2[] MakePlane(int width, int height, int xSegments = 1, int ySegments = 1)
        {
            /*return new Vector2[] {
                new Vector2(0, 0),
                new Vector2(0, -height),
                new Vector2(width, -height),

                new Vector2(0,0),
                new Vector2(width, -height),
                new Vector2(width, 0)
            };*/

            Vector2[] vertices = new Vector2[(xSegments+1) * (ySegments+1) * 6];

            int gridX = Math.Max(xSegments, 1);
            int gridY = Math.Max(ySegments, 1);


            float widthHalf = width / 2f;
            float heightHalf = height / 2f;

            float segmentWidth = (float)width / gridX;
            float segmentHeight = (float)height / gridY;
            int i = 0;

            for (int iy = 1; iy <= gridY; iy++) {
                float y = iy * segmentHeight; //- heightHalf;

                for (int ix = 1; ix <= gridX; ix++) {
                    var x = ix * segmentWidth;// - widthHalf;
                    vertices[i++] = new Vector2(x - segmentWidth, -y);
                    vertices[i++] = new Vector2(x - segmentWidth, -y - segmentHeight);
                    vertices[i++] = new Vector2(x, -y - segmentHeight);

                    vertices[i++] = new Vector2(x - segmentWidth, -y);
                    vertices[i++] = new Vector2(x, -y - segmentHeight);
                    vertices[i++] = new Vector2(x, -y);

                }
            }

            return vertices;
        }

    }
}
