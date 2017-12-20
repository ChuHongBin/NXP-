using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;


namespace CameraSimulation
{
    using int16_t = Int32;
    using Point_t = Point;
    using Pointf_t = PointF;

    class ImageProc
    {

        const int16_t LINES_FIRSTCHECK_CNT = 6;
        const int16_t IMAGE_WIDTH = 80;
        const int16_t IMAGE_HEIGHT = 60;
        public enum RodeColor
        {
            Side = 0,
            Rode = 1,
        };

        public enum RodeTypes
        {
            Unknown,
            Straight,
            Left,
            Right,
            Cross,
            LittleS,
            BigS,
            Ramp,
            Annulus,
        }

        public Bitmap srcImage { get; set; }
        public byte[] ImageData { get; set; }
        public byte[,] ImageDataRect { get; set; }

        public int16_t[] LeftLine { get; set; }
        public int16_t[] RightLine { get; set; }
        public int16_t[] MiddleLine { get; set; }
        public int16_t LeftLineCnt = 0;
        public int16_t RightLineCnt = 0;
        public int16_t MiddleLineCnt = 0;

        public RodeTypes RodeType { get; set; }

        public ImageProc() { }

        public ImageProc(Bitmap srcImage)
        {
            this.srcImage = srcImage.Clone(
                new Rectangle(0, 0, IMAGE_WIDTH, IMAGE_HEIGHT),
                PixelFormat.Format1bppIndexed);

            //生成图像数据
            Image2Data();

            //UpdateLines();
        }
        public ImageProc(byte[] srcData)
        {
            srcData.CopyTo(ImageData, 0);

            //生成图像数据
            Data2Image();

            //UpdateLines();
        }

        public void Image2Data()
        {
            BitmapData ImageData = srcImage.LockBits(
                new Rectangle(0, 0, IMAGE_WIDTH, IMAGE_HEIGHT),
                ImageLockMode.ReadOnly, PixelFormat.Format1bppIndexed);
            IntPtr ImagePtr = ImageData.Scan0;
            int16_t ImageDataLength = ImageData.Stride * ImageData.Height;
            byte[] ImageByteData = new byte[ImageDataLength];
            Marshal.Copy(ImagePtr, ImageByteData, 0, ImageDataLength);

            //二维矩阵数据初始化赋值
            ImageDataRect = new byte[ImageData.Height, ImageData.Width / 8];
            for (int16_t y = 0; y < ImageData.Height; y++)
            {
                for (int16_t x = 0; x < ImageData.Width / 8; x++)
                {
                    ImageDataRect[IMAGE_HEIGHT - 1 - y, x] =
                        ImageByteData[y * ImageData.Stride + x];
                }
            }

            //一维图像数据初始化赋值
            this.ImageData = new byte[ImageData.Height * ImageData.Width / 8];
            for (int16_t y = 0; y < ImageData.Height; y++)
            {
                for (int16_t x = 0; x < ImageData.Width / 8; x++)
                {
                    this.ImageData[y * ImageData.Width / 8 + x] =
                        ImageDataRect[y, x];
                }
            }

            srcImage.UnlockBits(ImageData);
        }
        public void Data2Image()
        {
            //锁定图片数据
            BitmapData ImageData = srcImage.LockBits(
                new Rectangle(0, 0, IMAGE_WIDTH, IMAGE_HEIGHT),
                ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);
            //图片数据头位置
            IntPtr ImagePtr = ImageData.Scan0;
            //数据长度
            int16_t ImageDataLength = ImageData.Stride * ImageData.Height;
            //写入图片字节数据到缓冲区
            Marshal.Copy(this.ImageData, 0, ImagePtr, ImageDataLength);
            //解锁图片数据
            srcImage.UnlockBits(ImageData);
        }

    }

}


