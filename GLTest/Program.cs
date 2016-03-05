using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLTest
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (var game = new AsyncTextureTestWindow())
            {
                game.Run(60);
            }
        }
    }
}
