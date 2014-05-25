using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLTest
{
    class Camera
    {

        private Matrix4 _matrix = Matrix4.Identity;

        public Vector3 Eye = new Vector3();
        public Vector3 Up { get; set; }
        public Quaternion Quaternion { get; private set; }
        private Vector3 Translation { get; set; }
        private Vector3 Target { get; set; }
        public float Scale = 1f;

        //public Matrix4 Modelview
        //{
        //    get
        //    {

        //    }
        //}

        public Camera(Vector3 eye, Vector3 target, Vector3 up)
        {
            Eye = eye;
            Up = up;
            LookAt(target);
        }

        public Camera(Vector3 eye, Vector3 up)
        {
            Eye = eye;
            Up = up;
        }

        public void LookAt(Vector3 target) {
            //Quaternion = Matrix4.LookAt(Eye, target, Up).ExtractRotation();
            //Translation = Matrix4.LookAt(Eye, target, Up).ExtractTranslation();

            //_matrix = Matrix4.CreateFromQuaternion(Quaternion);
            Target = target - Eye;
        }

        public Matrix4 GetMatrix()
        {
            Matrix4 m = Matrix4.LookAt(Eye, Eye + Target, Up);
            Matrix4 s = Matrix4.CreateScale(Scale);
            Matrix4.Mult(ref s, ref m, out m);

            return m;
        }

        public void Apply()
        {
            var m = Matrix4.LookAt(Eye, Eye + Target, Up);
            
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.LoadMatrix(ref m);
        }

    }
}
