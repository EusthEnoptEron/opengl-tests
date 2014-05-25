using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLTest
{

    public class Shader : IDisposable
    {
        private bool compiled_ = false;
        private string source_ = "";

        public int ID { get; private set; }
        public ShaderType Type { get; private set; }
        public string Log { get; private set; }

        public string Source { 
            get { return source_; } 
            set {
                if (compiled_)
                    throw new Exception("Can't change a compiled shader.");
                source_ = value;
            } 
       }

        public Shader(ShaderType type) : this(type, "")
        {
        }

        public Shader(ShaderType type, string source)
        {
            ID = GL.CreateShader(type);
            Type = type;
            Source = source;
        }


        public bool Compile()
        {
            if (!compiled_)
            {
                compiled_ = true;

                GL.ShaderSource(ID, Source);
                GL.CompileShader(ID);

                Log = GL.GetShaderInfoLog(ID);
            }

            int success = 0;
            GL.GetShader(ID, ShaderParameter.CompileStatus, out success);

            return success == 1;
        }

        public void Dispose()
        {
            GL.DeleteShader(ID);
        }
    }


    public class ShaderProgram : IDisposable
    {
        List<Shader> shaders_ = new List<Shader>();
        private Dictionary<string, int> params_ = new Dictionary<string, int>();
        private bool compiled_ = false;


        public int ID { get; private set; }
        public string Log { get; private set; }
        
        public ShaderProgram()
        {
            ID = GL.CreateProgram();
        }

        public void AddShader(Shader shader)
        {
            shaders_.Add(shader);
        }

        public bool Compile()
        {
            if (!compiled_)
            {
                foreach (var shader in shaders_)
                {
                    if (!shader.Compile())
                    {
                        Log = shader.Log;
                        return false;
                    }

                    GL.AttachShader(ID, shader.ID);
                }

                GL.LinkProgram(ID);
                compiled_ = true;
            }
            
            int success = 0;
            GL.GetProgram(ID, GetProgramParameterName.LinkStatus, out success);
            
            return success == 1;
        }

        public void Dispose()
        {
            GL.DeleteProgram(ID);

            foreach (var shader in shaders_)
            {
                shader.Dispose();
            } 
        }

        public int GetUniformLocation(string param)
        {
            if (!params_.ContainsKey(param))
            {
                int loc = GL.GetUniformLocation(ID, param);
                if (loc >= 0)
                    params_[param] = loc;
                else
                    throw new Exception("Halp! Uniform not found: " + param);
            }
            return params_[param];
        }

        public int GetAttribLocation(string param)
        {
            if (!params_.ContainsKey(param))
            {
                int loc = GL.GetAttribLocation(ID, param);
                if (loc >= 0)
                    params_[param] = loc;
                else
                    throw new Exception("Halp! Attrib not found: " + param);

            }
            return params_[param];
        }

        public void Use()
        {
            GL.UseProgram(ID);
        }


        //
        // Summary:
        //     [requires: v4.0 or ARB_gpu_shader_fp64|VERSION_4_0] Specify the value of
        //     a uniform variable for the current program object
        //
        // Parameters:
        //   location:
        //     Specifies the location of the uniform variable to be modified.
        //
        //   x:
        //     For the vector (glUniform*v) commands, specifies the number of elements that
        //     are to be modified. This should be 1 if the targeted uniform variable is
        //     not an array, and 1 or more if it is an array. For the matrix (glUniformMatrix*)
        //     commands, specifies the number of matrices that are to be modified. This
        //     should be 1 if the targeted uniform variable is not an array of matrices,
        //     and 1 or more if it is an array of matrices.
        public void Uniform1(string name, double x) {
            Use();
            GL.Uniform1(GetUniformLocation(name), x);
        }
        //
        // Summary:
        //     [requires: v2.0] Specify the value of a uniform variable for the current
        //     program object
        //
        // Parameters:
        //   location:
        //     Specifies the location of the uniform variable to be modified.
        //
        //   v0:
        //     For the scalar commands, specifies the new values to be used for the specified
        //     uniform variable.
        public void Uniform1(string name, float v0)
        {
            Use();
            GL.Uniform1(GetUniformLocation(name), v0);
        }
        //
        // Summary:
        //     [requires: v2.0] Specify the value of a uniform variable for the current
        //     program object
        //
        // Parameters:
        //   location:
        //     Specifies the location of the uniform variable to be modified.
        //
        //   v0:
        //     For the scalar commands, specifies the new values to be used for the specified
        //     uniform variable.
        public void Uniform1(string name, int v0)
        {
            Use();
            GL.Uniform1(GetUniformLocation(name), v0);
        }
        //
        // Summary:
        //     [requires: v3.0] Specify the value of a uniform variable for the current
        //     program object
        //
        // Parameters:
        //   location:
        //     Specifies the location of the uniform variable to be modified.
        //
        //   v0:
        //     For the scalar commands, specifies the new values to be used for the specified
        //     uniform variable.
        [CLSCompliant(false)]
        public void Uniform1(string name, uint v0)
        {
            Use();
            GL.Uniform1(GetUniformLocation(name), v0);
        }

        public void Uniform4(string name, Vector4 vector)
        {
            Use();
            GL.Uniform4(GetUniformLocation(name), vector);
        }
        public void Uniform3(string name, Vector3 vector)
        {
            Use();
            GL.Uniform3(GetUniformLocation(name), vector);
        }
        public void Uniform2(string name, Vector2 vector)
        {
            Use();
            GL.Uniform2(GetUniformLocation(name), vector);
        }


        public void Uniform4(string name, ref Vector4 vector)
        {
            Use();
            GL.Uniform4(GetUniformLocation(name), ref vector);
        }
        public void Uniform3(string name, ref Vector3 vector)
        {
            Use();
            GL.Uniform3(GetUniformLocation(name), ref vector);
        }
        public void Uniform2(string name, ref Vector2 vector)
        {
            Use();
            GL.Uniform2(GetUniformLocation(name), ref vector);
        }

        public void UniformMatrix4(string name, bool transpose, ref Matrix4 matrix)
        {
            Use();
            GL.UniformMatrix4(GetUniformLocation(name), transpose, ref matrix);
        }

    }
}
