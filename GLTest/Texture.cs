using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GLTest
{
    public class Texture : IDisposable
    {
        private static int BoundTexture = -1;

        public TextureTarget Type { get; private set; }
        public PixelInternalFormat Format { get; private set; }
        public int ID { get; private set; }
        public int Number { get; private set; }

        public Texture(Image img, TextureTarget type, PixelInternalFormat format, int num)
        {
            ID = GL.GenTexture();

            Type = type;
            Format = format;
            Number = num;

            Activate();

            using(Bitmap bmp = new Bitmap(img)) {
                BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(Type, 0, Format, bmp.Width, bmp.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                bmp.UnlockBits(data);
            }
        }
        public Texture(int width, int height, TextureTarget type, PixelInternalFormat format, OpenTK.Graphics.OpenGL.PixelFormat format2, int num)
        {
            ID = GL.GenTexture();

            Type = type;
            Format = format;
            Number = num;

            Activate();

            GL.TexImage2D(Type, 0, Format, width, height, 0, format2, PixelType.UnsignedByte, IntPtr.Zero);
        }

        protected Texture(TextureTarget type, PixelInternalFormat format, OpenTK.Graphics.OpenGL.PixelFormat format2, int num) {
            ID = GL.GenTexture();

            Type = type;
            Format = format;
            Number = num;

            Activate();

        }

        public void Activate()
        {
            if (BoundTexture != ID)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + Number);
                GL.BindTexture(Type, ID);
                BoundTexture = ID;
            }
            OnActivate();

        }

        protected virtual void OnActivate()
        {

        }

        public virtual void Dispose()
        {
            GL.DeleteTexture(ID);
        }

        //
        // Summary:
        //     [requires: v1.0] Set texture parameters
        //
        // Parameters:
        //   target:
        //     Specifies the target texture, which must be either Texture1D, Texture2D,
        //     Texture3D, Texture1DArray, Texture2DArray, TextureRectangle, or TextureCubeMap.
        //
        //   pname:
        //     Specifies the symbolic name of a single-valued texture parameter. pname can
        //     be one of the following: DepthStencilTextureMode, TextureBaseLevel, TextureCompareFunc,
        //     TextureCompareMode, TextureLodBias, TextureMinFilter, TextureMagFilter, TextureMinLod,
        //     TextureMaxLod, TextureMaxLevel, TextureSwizzleR, TextureSwizzleG, TextureSwizzleB,
        //     TextureSwizzleA, TextureWrapS, TextureWrapT, or TextureWrapR. For the vector
        //     commands (glTexParameter*v), pname can also be one of TextureBorderColor
        //     or TextureSwizzleRgba.
        //
        //   param:
        //     For the scalar commands, specifies the value of pname.
        public void TexParameter(TextureParameterName pname, float param)
        {
            Activate();
            GL.TexParameter(Type, pname, param);
        }


        //
        // Summary:
        //     [requires: v1.0] Set texture parameters
        //
        // Parameters:
        //   target:
        //     Specifies the target texture, which must be either Texture1D, Texture2D,
        //     Texture3D, Texture1DArray, Texture2DArray, TextureRectangle, or TextureCubeMap.
        //
        //   pname:
        //     Specifies the symbolic name of a single-valued texture parameter. pname can
        //     be one of the following: DepthStencilTextureMode, TextureBaseLevel, TextureCompareFunc,
        //     TextureCompareMode, TextureLodBias, TextureMinFilter, TextureMagFilter, TextureMinLod,
        //     TextureMaxLod, TextureMaxLevel, TextureSwizzleR, TextureSwizzleG, TextureSwizzleB,
        //     TextureSwizzleA, TextureWrapS, TextureWrapT, or TextureWrapR. For the vector
        //     commands (glTexParameter*v), pname can also be one of TextureBorderColor
        //     or TextureSwizzleRgba.
        //
        //   param:
        //     For the scalar commands, specifies the value of pname.
        public void TexParameter(TextureTarget target, TextureParameterName pname, int param)
        {
            Activate();
            GL.TexParameter(Type, pname, param);
        }

        // Summary:
        //     [requires: v1.0] Set texture parameters
        //
        // Parameters:
        //   target:
        //     Specifies the target texture, which must be either Texture1D, Texture2D,
        //     Texture3D, Texture1DArray, Texture2DArray, TextureRectangle, or TextureCubeMap.
        //
        //   pname:
        //     Specifies the symbolic name of a single-valued texture parameter. pname can
        //     be one of the following: DepthStencilTextureMode, TextureBaseLevel, TextureCompareFunc,
        //     TextureCompareMode, TextureLodBias, TextureMinFilter, TextureMagFilter, TextureMinLod,
        //     TextureMaxLod, TextureMaxLevel, TextureSwizzleR, TextureSwizzleG, TextureSwizzleB,
        //     TextureSwizzleA, TextureWrapS, TextureWrapT, or TextureWrapR. For the vector
        //     commands (glTexParameter*v), pname can also be one of TextureBorderColor
        //     or TextureSwizzleRgba.
        //
        //   @params:
        //     [length: pname] For the scalar commands, specifies the value of pname.
        public void TexParameter(TextureTarget target, TextureParameterName pname, int[] @args)
        {
            Activate();
            GL.TexParameter(Type, pname, args);
        }

        //
        // Summary:
        //     [requires: v3.0]
        //
        // Parameters:
        //   target:
        //
        //   pname:
        //
        //   @params:
        //     [length: pname]
        public void TexParameterI(TextureTarget target, TextureParameterName pname, int[] @args)
        {
            Activate();
            GL.TexParameter(Type, pname, args);
        }

        //
        // Summary:
        //     [requires: v3.0]
        //
        // Parameters:
        //   target:
        //
        //   pname:
        //
        //   @params:
        //     [length: pname]
        public void TexParameterI(TextureTarget target, TextureParameterName pname, ref int @args)
        {
            Activate();
            GL.TexParameter(Type, pname, args);
        }
        //
        // Summary:
        //     [requires: v3.0]
        //
        // Parameters:
        //   target:
        //
        //   pname:
        //
        //   @params:
        //     [length: pname]
        public void TexParameterI(TextureTarget target, TextureParameterName pname, ref uint @params)
        {
            Activate();
            GL.TexParameter(Type, pname,  @params);
        }

    }

    public class AsyncTexture : Texture {
        public int BufferID { get; private set; }
        public string Path { get; private set; }
        object BUFFER_CREATED = new Object();
        private IntPtr BufferAddress = IntPtr.Zero;

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        private uint? _Size = null;
        private bool _FinishedLoading = false;
        private bool _Mapped = false;

        private int Width;
        private int Height;

        public AsyncTexture(string path) : base(TextureTarget.Texture2D, PixelInternalFormat.Rgba, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, 0)
        {
            Path = path;
            TexParameter(TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            TexParameter(TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            TexParameter(TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            TexParameter(TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            LoadIntoBufferAsync();


        }
        public override void Dispose()
        {
            base.Dispose();
            GL.DeleteBuffer(BufferID);
        }

        protected override void OnActivate()
        {
            if (_Size != null && BufferAddress == IntPtr.Zero)
            {
                Console.WriteLine("Create buffer...");
                BufferID = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.PixelUnpackBuffer, BufferID);
                GL.BufferData(BufferTarget.PixelUnpackBuffer, new IntPtr(_Size.Value), IntPtr.Zero, BufferUsageHint.StaticDraw);

                BufferAddress = GL.MapBuffer(BufferTarget.PixelUnpackBuffer, BufferAccess.WriteOnly);

                lock (BUFFER_CREATED)
                {
                    Monitor.PulseAll(BUFFER_CREATED);
                }
            }

            if (_FinishedLoading && !_Mapped)
            {
                _Mapped = true;
                Console.WriteLine("Mapping texture!");

                GL.BindBuffer(BufferTarget.PixelUnpackBuffer, BufferID);
                GL.UnmapBuffer(BufferTarget.PixelUnpackBuffer);
                GL.TexImage2D(Type, 0, Format, Width, Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);


                Console.WriteLine("Mapped texture!");
            }
        }

        private async void LoadIntoBufferAsync()
        {
            await Task.Run(delegate
            {
                Console.WriteLine("Loading {0}...", Path);
                   
                // Load image
                using (var bmp = new Bitmap(Path))
                {
                    BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    Width = bmp.Width;
                    Height = bmp.Height;
                   _Size = (uint)(data.Width * data.Height * 4);
                   
                    // Go to sleep until buffer created
                   lock (BUFFER_CREATED)
                   {
                       Monitor.Wait(BUFFER_CREATED);
                   }

                   //byte[] buffer = new byte[_Size.Value];
                   //Marshal.Copy(data.Scan0, buffer, 0, (int)_Size.Value);
                   //Marshal.Copy(buffer, 0, BufferAddress, (int)_Size.Value);

                    Console.WriteLine("BUFFER CREATED!");
                    CopyMemory(BufferAddress, data.Scan0, _Size.Value);
                    
                    //GL.TexImage2D(Type, 0, Format, bmp.Width, bmp.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                   
                    //GL.MapBuffer(BufferTarget.PixelUnpackBuffer, BufferAccess.WriteOnly
                    bmp.UnlockBits(data);
                }

                Console.WriteLine("Finished Loading {0}...", Path);
                _FinishedLoading = true;
                //GL.BufferData(BufferTarget.PixelUnpackBuffer, 
            });
        }
    }
}
