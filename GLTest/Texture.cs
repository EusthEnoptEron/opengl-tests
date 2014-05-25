using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
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

            GL.TexImage2D(Type, 2, Format, width, height, 0, format2, PixelType.UnsignedByte, IntPtr.Zero);
        }

        public void Activate()
        {
            if (BoundTexture != ID)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + Number);
                GL.BindTexture(Type, ID);
                BoundTexture = ID;
            }
        }

        public void Dispose()
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
}
