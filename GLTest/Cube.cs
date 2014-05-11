using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLTest
{
    class Cube : Mesh
    {
        int Width;
        int Height;
        int Depth;
        public Cube(int width, int height, int depth) {
            Width = width;
            Height = height;
            Depth = depth;
        }

        public override void OnDraw()
        {
            float left = -Width / 2f;
            float right = Width / 2f;
            float top = Height / 2f;
            float bottom = -Height / 2f;
            float far = -Depth / 2f;
            float near = Depth / 2f;

            GL.Color4(Color.FromArgb(100, Color.Blue));
            // Font plane
            GL.Begin(PrimitiveType.TriangleStrip);

            GL.Vertex3(left, bottom, near);
            GL.Vertex3(right, bottom, near);
            GL.Vertex3(left, top, near);
            GL.Vertex3(right, top, near);

            GL.End();

            // Back plane
            GL.Begin(PrimitiveType.TriangleStrip);

            GL.Vertex3(right, bottom, far);
            GL.Vertex3(left, bottom, far);
            GL.Vertex3(right, top, far);
            GL.Vertex3(left, top, far);

            GL.End();

            // Top plane
            GL.Begin(PrimitiveType.TriangleStrip);

            GL.Vertex3(left, top, near);
            GL.Vertex3(right, top, near);
            GL.Vertex3(left, top, far);
            GL.Vertex3(right, top, far);

            GL.End();

            // Bottom plane
            GL.Begin(PrimitiveType.TriangleStrip);

            GL.Vertex3(right, bottom, near);
            GL.Vertex3(left, bottom, near);
            GL.Vertex3(right, bottom, far);
            GL.Vertex3(left, bottom, far);

            GL.End();


            // Left plane
            GL.Begin(PrimitiveType.TriangleStrip);

            GL.Vertex3(left, bottom, far);
            GL.Vertex3(left, bottom, near);
            GL.Vertex3(left, top, far);
            GL.Vertex3(left, top, near);

            GL.End();


            // Right plane
            GL.Begin(PrimitiveType.TriangleStrip);

            GL.Vertex3(right, bottom, near);
            GL.Vertex3(right, bottom, far);
            GL.Vertex3(right, top, near);
            GL.Vertex3(right, top, far);

            GL.End();



        }
    }
}
