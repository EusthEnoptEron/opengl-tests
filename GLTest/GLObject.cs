using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLTest
{
    public struct PartitionRule
    {
        /// <summary>
        /// Name to bind to
        /// </summary>
        public string AttributeName;

        /// <summary>
        /// Offset to start at
        /// </summary>
        public int Offset;

        /// <summary>
        /// Number of values to take
        /// </summary>
        public int Length;

        public PartitionRule(string attribName, int offset, int length)
        {
            AttributeName = attribName;
            Offset = offset;
            Length = length;
        }
    }

    public class GLObject : IDisposable
    {
        public delegate void OnUseHandler(GLObject sender);

        /// <summary>
        /// Gets called whenever the GLObject is "used"
        /// </summary>
        public event OnUseHandler Activate;


        public ShaderProgram Program { get; private set; }
        private List<Texture> textures_ = new List<Texture>();

        public int VAO { get; private set; }
        public int VBO { get; private set; }
        public GLObject(float[] values, List<PartitionRule> rules, ShaderProgram program)
        {
            Init(values, rules, program, VertexAttribPointerType.Float, values.Length, sizeof(float));
        }


        private void Init<T2>(T2[] values, List<PartitionRule> rules, ShaderProgram program, VertexAttribPointerType type, int length, int size) where T2 : struct {
            Program = program;

            program.Compile();
            GL.UseProgram(program.ID);

            VAO = GL.GenVertexArray();
            Bind();

            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(length * size), values, BufferUsageHint.StaticDraw);

            
            int stride = rules.Max((el) => { return el.Offset + el.Length; }) * size;
            Length = values.Length / (stride / size);
            foreach (var rule in rules)
            {
                var attr = Program.GetAttribLocation(rule.AttributeName);
                GL.EnableVertexAttribArray(attr);
                GL.VertexAttribPointer(attr, rule.Length, type, false, stride, rule.Offset * size);

                ErrorCode error = GL.GetError();
            }
        }

        public void Bind()
        {
            GL.BindVertexArray(VAO);
        }

        public void Use()
        {
            GL.BindVertexArray(VAO);
            Program.Use();
            foreach (var tex in textures_)
            {
                tex.Activate();
            }

            var hd = Activate;
            if (hd != null) hd(this);
        }

        public void AddTexture(string attribName, Texture tex)
        {
            textures_.Add(tex);

            tex.Activate();
            Program.Use();

            GL.Uniform1(Program.GetUniformLocation(attribName), tex.Number);
        }

        public void Dispose()
        {
            GL.DeleteVertexArray(VAO);
            GL.DeleteBuffer(VBO);

            Program.Dispose();
            foreach (var tex in textures_) tex.Dispose();
        }

        public int Length
        {
            get;
            private set;
        }
    }
}
