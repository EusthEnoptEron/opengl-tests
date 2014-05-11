using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLTest
{
    abstract class Mesh
    {
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }

        protected Mesh()
        {
            Scale = new Vector3(1, 1, 1);
        }

        public void Draw()
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.Translate(Position);
            GL.Scale(Scale);

            OnDraw();

            GL.PopMatrix();
        }

        public abstract void OnDraw();
    }
}
