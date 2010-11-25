using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace AppleBitmap
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
                switch (args[0])
                {
                    case "-d":
                        DecodeAppleBitmaps(args);
                        break;
                    case "-e":
                        EncodeAppleBitmaps(args);
                        break;
                    default:
                        Console.WriteLine("Correct formatting should be\n-d [file(s) to decode]\nor\n-e [file(s) to encode]");
                        break;
                }
        }

        static int ReadInt(ref Stream s)
        {
            int output = 0;
            for (int i = 0; i < 4; i++)
                output |= ((int)(s.ReadByte() & 0xFF)) << (i * 8);
            return output;
        }
        static int ReadInt(ref MemoryStream ms)
        {
            int output = 0;
            for (int i = 0; i < 4; i++)
                output |= ((int)(ms.ReadByte() & 0xFF)) << (i * 8);
            return output;
        }

        static void DecodeAppleBitmaps(string[] paths)
        {
            foreach (string path in paths)
                if (File.Exists(path))
                {
                    byte[] allbytes = File.ReadAllBytes(path);
                    MemoryStream ms = new MemoryStream(allbytes);
                    ms.Position = 0;
                    int HeaderLength = ReadInt(ref ms);
                    int ImageWidth = ReadInt(ref ms);
                    int ImageHeight = ReadInt(ref ms);
                    while (ms.Position < HeaderLength)
                        ms.ReadByte();
                    Bitmap bmp = new Bitmap(ImageWidth, ImageHeight);
                    for (int i = 0; i < ImageWidth * ImageHeight; i++)
                    {
                        byte B = (byte)(ms.ReadByte() & 0xFF);
                        byte G = (byte)(ms.ReadByte() & 0xFF);
                        byte R = (byte)(ms.ReadByte() & 0xFF);
                        byte A = (byte)(ms.ReadByte() & 0xFF);
                        bmp.SetPixel(i % ImageWidth, i / ImageWidth, Color.FromArgb(A, R, G, B));
                    }
                    bmp.Save(path + ".png");
                }
        }

        static void EncodeAppleBitmaps(string[] paths)
        {
            foreach (string path in paths)
                if (File.Exists(path))
                {
                    Image img = Image.FromFile(path);
                    Bitmap bmp = new Bitmap(img);
                    img.Dispose();
                    FileStream fs = new FileStream(path + ".applebitmap", FileMode.CreateNew, FileAccess.Write);
                    WriteInt(ref fs, 128);          // Constant, size of header in bytes.
                    WriteInt(ref fs, bmp.Width);    // Width of image in pixels.
                    WriteInt(ref fs, bmp.Height);   // Height of image in pixels.
                    WriteInt(ref fs, bmp.Width * 4);// Width of image in pixels, multiplied by 4. Unknown purpose; channels per row, perhaps?
                    WriteInt(ref fs, 8);            // Constant 8. Unknown purpose; bits per channel, perhaps?
                    WriteInt(ref fs, 32);           // Constant 32. Unknown purpose; bits per pixel, perhaps?
                    WriteInt(ref fs, 8194);         // Constant 8194. Absolutely no fecking idea.
                    WriteInt(ref fs, 0);
                    while (fs.Position < 128)
                        WriteInt(ref fs, 0);        // Padding to hit the 128-byte header length.
                    for (int i = 0; i < bmp.Width * bmp.Height; i++)
                    {
                        // Write the data, BGRA ordered.
                        int color = bmp.GetPixel(i % bmp.Width, i / bmp.Width).ToArgb();
                        for (int j = 0; j < 4; j++)
                            fs.WriteByte((byte)((color >> (j * 8)) & 0xFF));
                    }
                    fs.Close();
                }
        }

        static void WriteInt(ref FileStream fs, int integer)
        {
            for (int i = 0; i < 4; i++)
                fs.WriteByte((byte)((integer >> (i * 8)) & 0xFF));
        }
        static void WriteInt(ref Stream s, int integer)
        {
            for (int i = 0; i < 4; i++)
                s.WriteByte((byte)((integer >> (i * 8)) & 0xFF));
        }
    }
}
