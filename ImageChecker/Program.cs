using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ImageChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("参数错误");
                return;
            }
            DirectoryInfo dir = new DirectoryInfo(args[0]);
            if (!dir.Exists)
            {
                Console.WriteLine("目录不存在");
                return;
            }
            var pngs = new List<FileInfo>();

            pngs.AddRange(dir.GetFiles("*.png", SearchOption.AllDirectories));
            var jpgs = dir.GetFiles("*.jpg", SearchOption.AllDirectories);
            pngs.AddRange(jpgs);

            bool should_alert = false;
            foreach (var file in jpgs)
            {
                Console.WriteLine(file.Name + "\t是jpg.");
                should_alert = true;
            }

            foreach (var file in pngs)
            {
                var bitmap = Bitmap.FromFile(file.FullName);
                if (bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb
                    && bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppPArgb
                    && bitmap.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppRgb)
                {
                    Console.WriteLine(file.Name + "\t不是32位深.\r\n");
                    should_alert = true;
                }

                if (bitmap.Width >= 1024 || bitmap.Height >= 1024)
                {
                    Console.WriteLine( file.Name + "\t大小为: " + bitmap.Width + "*" + bitmap.Height);
                    should_alert = true;
                }
            }

            if (!should_alert)
            {
                Console.WriteLine("没有发现问题");
            }

            Console.ReadLine();
        }
    }
}
