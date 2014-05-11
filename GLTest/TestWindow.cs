using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLTest
{
    class TestWindow : GameWindow
    {
        Camera myCamera = new Camera(new Vector3(3, 2, 0), new Vector3(0, 1, 0));
        Mesh myMesh = new Cube(1,1,1);

        Matrix4 projection = Matrix4.Identity;
        float Theta = 0;
        bool perspective = false;


        public TestWindow() : base(800, 600)
        {
            // Set up window
            VSync = VSyncMode.On;
            this.WindowBorder = OpenTK.WindowBorder.Fixed;
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.AlphaTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Enable(EnableCap.CullFace);

            // Set up projection
            SwapProjection();

            KeyUp += (sender, e) => { if(e.Key == Key.Space) SwapProjection(); };
        }

        protected override void OnResize(EventArgs e)
        {
            //GL.Viewport(0, 0, Width, Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            var x = myCamera.Eye.X;
            var z = myCamera.Eye.Z;
            float eTime = (float)e.Time;

            Theta += (float)e.Time;

            myCamera.Eye.X = (float)(2 * Math.Cos(Theta) - 2 * Math.Sin(Theta));
            myCamera.Eye.Z = (float)(2 * Math.Sin(Theta) + 2 * Math.Cos(Theta));
            myCamera.Eye.Y = (float)(Math.Sin(Theta))*2;
            myCamera.LookAt(new Vector3(0, 0, 0));


            if (Keyboard[Key.Enter])
            {
                myMesh.Scale += new Vector3(eTime, eTime, eTime);
            }
            if (Keyboard[Key.ShiftRight])
            {
                myMesh.Scale -= new Vector3(eTime, eTime, eTime);
            }
        }

        private void SwapProjection()
        {
            float aspect = Width / (float)Height;
            if(perspective)
                projection = Matrix4.CreateOrthographic(5, 5 / aspect, 0.1f, 10);
            else
                projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, aspect, 0.01f, 100f);
            
            perspective = !perspective;

            LoadProjection();
        }

        private void LoadProjection()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.LoadMatrix(ref projection);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(Color.Black);
           
            myCamera.Apply();
           
            //GL.MatrixMode(MatrixMode.Modelview);
            //var m = Matrix4.LookAt(0, 3, 0, 0, 0, 0, 0, 0, 1);
            //GL.LoadMatrix(ref m);

            
            myMesh.Draw();


            GL.Begin(PrimitiveType.Lines);;
            GL.Color3(Color.Red);
            GL.Vertex3(00, 0, 0);
            GL.Vertex3(100, 0, 0);

            GL.Color3(Color.Green);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 100, 0);

            GL.Color3(Color.Blue);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, 100);
            GL.End();

            SwapBuffers();
        }

    }
}
