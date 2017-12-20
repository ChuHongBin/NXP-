using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

//
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

        public enum SpeedRode
        {
            MIX = 0,
            MID = 1,
            MIN = 2
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
            Ring,
            OutRing,
            Annulus,
        }

        public Bitmap srcImage { get; set; }
        public byte[] ImageData { get; set; }
        public byte[,] ImageDataRect { get; set; }
        //定义图像处理数组
        public int16_t[] LeftLine { get; set; }
        public int16_t[] RightLine { get; set; }
        public int16_t[] MiddleLine { get; set; }
        // //            
        public int16_t[] Widths { get; set; }
        //画点的长度
        public int16_t LeftLineCnt = 0;
        public int16_t RightLineCnt = 0;
        public int16_t MiddleLineCnt = 0;
      
        //判断说明 赋值
        public string retcond = "初始化";

        public int16_t IMAGE_HEIGHT_BAND = 59;
        public int16_t IMAGE_WIDTH_BAND = 79;      
        //图像黑白
        public byte IMAGE_BLACK = (byte)0x00;
        public byte IMAGE_WHITE = (byte)0xff;
        public RodeTypes RodeType { get; set; }
        public byte[,] imagebuff { get; set; }

        public ImageProc() { }

        public ImageProc(Bitmap srcImage)
        {
            this.srcImage = srcImage.Clone(
                new Rectangle(0, 0, IMAGE_WIDTH, IMAGE_HEIGHT),
                PixelFormat.Format1bppIndexed);

            //生成图像数据
            Image2Data();
            //图像解压
            //imagebuff[y,x];
            img_extract();
            //边线处理
            UpdateLines();

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
            srcImage.UnlockBits(ImageData);
        }

        //数组解压
        public void img_extract()
        {
            int16_t bug;
            imagebuff = new byte[IMAGE_HEIGHT, IMAGE_WIDTH];
            for (byte y = 0; y < 60; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    for (int n = 0; n < 8; n++)
                    {
                        bug = ImageDataRect[y, x];
                        bug = bug >> n;
                        bug = bug & 0x01;
                        if (bug == 1)
                        {
                            imagebuff[y, (x * 8 + 7 - n)] = IMAGE_WHITE;
                        }
                        else
                        {
                            imagebuff[y, (x * 8 + 7 - n)] = IMAGE_BLACK;
                        }
                    }
                }


            }
        }



        //图像处理函数
        //输入：imagebuff
        //输出：【左边线 LeftLine[]】    【右边线RightLine[]】    【中线MiddleLine[]】
        // LeftLineCnt  RightLineCnt   MiddleLineCnt
        public void UpdateLines()
        {

        }

        ///功能:从左向右查找最宽
        //输入：行，
        public int16_t Guide_MaxWidthBorder(int16_t tline)
        {
            int16_t tpixel, tempwidth, width = 0;
            int16_t left = 0, right = 0;

            //循环一整行左右的信息
            tpixel = 0; tempwidth = 0;
            while (tpixel < IMAGE_WIDTH)
            {
                //滤出左侧的黑色
                while (tpixel <= IMAGE_WIDTH_BAND && imagebuff[tline, tpixel] != IMAGE_WHITE)
                {
                    tpixel++;
                }
                //白色部分左侧坐标
                left = tpixel;
                //开始记录宽度，从第一个白色点开始记录
                while (tpixel <= IMAGE_WIDTH_BAND && imagebuff[tline, tpixel] == IMAGE_WHITE)
                {
                    tpixel++;
                    tempwidth++;
                }
                //白色部分右侧坐标
                right = left + tempwidth - 1;
                //坐标越界判断，防止内存溢出
                if (left < 0)
                {
                    left = 0;
                }
                if (right > IMAGE_WIDTH_BAND)
                {
                    right = IMAGE_WIDTH_BAND;
                }
                //判断该白色区域是否为整行里最大白色宽度
                //不满足上一个判定则记录数据执行下一次循环
                if (tempwidth > width)
                {
                    width = tempwidth;
                }
                //若检查剩下的区域长度不大于已找到最大白色区域，判定找到最大宽度，记录数据并提前退出循环
                if (IMAGE_WIDTH - tpixel < width)
                {
                    width = tempwidth;
                    break;
                }
                //进入下一次判断坐标
                tempwidth = 0;
                tpixel++;
            }
            RightLine[tline] = right;
            LeftLine[tline] = left;
            return width;

        }
        //功能：从中间向两边
        //输入： 行，中点
        //返回： 
        public int16_t Guide_Center2Board(int16_t tline, int16_t Center)
        {
            int left, right, center;
            center = Center;

            //向左运算
            left = center;
            while (left >= 0 && imagebuff[tline, left] == IMAGE_WHITE)//找黑边
            {
                left--;
            }
            while (left >= 0 && imagebuff[tline, left] != IMAGE_WHITE)//判断是否是黑边
            {                                             //左边界为0跳出
                left--;
            }
            LeftLine[tline] = left + 1;
            //向右运算
            right = center;
            while (right <= IMAGE_WIDTH_BAND && imagebuff[tline, right] == IMAGE_WHITE)//找黑边
            {
                right++;
            }

            while (right <= IMAGE_WIDTH_BAND && imagebuff[tline, right] != IMAGE_WHITE)//判断是否是黑边
            {                                                           //右边界为最大跳出
                right++;
            }
            RightLine[tline] = right - 1;

            center = (left + right);

            return center;  //   

        }
        //功能：检查一行图像在指定范围是否存在全部为指定颜色；
        //输入：[tline]图像行；[Left]左边界;[Right]右边界；[Color]颜色；
        //输出：无；
        //返回：0：判断失败;1:判断成功;
        public bool Guide_CheckLine(int16_t tline, int16_t Left, int16_t Right, int16_t Color)
        {
            int16_t tpixel;

            //判断
            for (tpixel = Left; tpixel <= Right; tpixel++)
            {
                if (imagebuff[tline, tpixel] != Color)
                {
                    return false;
                }
            }
            return true;


        }

        //功能：最小二乘法拟合斜率
        //输入：[begin]数组开始；[end]数组结束；[*p]数组首地址
        //输出：无
        //返回：（float）斜率；
        float Slope_Calculate(int16_t begin, int16_t end, int16_t[] p)    //最小二乘法拟合斜率
        {
            float xsum = 0, ysum = 0, xysum = 0, x2sum = 0;
            int16_t i = 0;
            float result = 0;
            float resultlast = 0;

            for (i = begin; i < end; i++)
            {
                xsum += i;
                ysum += p[i];
                xysum += i * p[i];
                x2sum += i * i;
            }
            if (((end - begin) * x2sum - xsum * xsum) != 0) //判断除数是否为零 
            {
                result = ((end - begin) * xysum - xsum * ysum) / ((end - begin) * x2sum - xsum * xsum);
                resultlast = result;
            }
            else
            {
                result = resultlast;
            }
            return result;
        }


        //泰勒展开式
        private float TaylorLn(float x)
        {
            float ret = 0.0f;

            if (x < 100.0f)
            {
                float y = (x - 1.0f) / (x + 1.0f);
                float y2 = y * y;
                float power = 1.0f;
                float sum = 1.0f;
                int16_t time;
                int16_t time_max = (int16_t)x;
                time_max = time_max / 10 + 1;
                time_max = time_max * 10;
                for (time = 0; time < time_max; time++)
                {
                    power = power * y2;
                    sum = sum + (power / (2 * time + 3));
                }
                ret = 2 * y * sum;
            }
            else
            {
                ret = (float)Math.Log(x);
            }

            return ret;
        }
        private float TwoPointDistance(Point_t p1, Point_t p2)
        {
            int16_t errX = p1.X - p2.X;
            int16_t errY = p1.Y - p2.Y;

            float dis = FastSqrt((float)(errX * errX + errY * errY));

            return dis;
        }
        unsafe private float FastSqrt(float srcNum)
        {
            int i;
            float x, y;
            const float f = 1.5f;

            x = srcNum * 0.5f;
            y = srcNum;
            i = *(int*)&y;
            i = 0x5f3759df - (i >> 1);
            y = *(float*)&i;
            y = y * (f - (x * y * y));
            y = y * (f - (x * y * y));
            y = y * (f - (x * y * y));

            return srcNum * y;
        }

        //线平均误差
        private float LineAvgErr(int16_t[] Line, int16_t Start, int16_t End)
        {
            if (Start > IMAGE_HEIGHT - 1 || End > IMAGE_HEIGHT - 1 ||
                End - Start + 1 == 0)
            { return 0; }

            float errAvg = 0.0f;
            float slope = CalcSlope(Line, Start, End - Start + 1);

            float tempX = (float)Line[Start];

            for (int16_t n = Start; n <= End; n++)
            {
                errAvg = errAvg + (tempX - (float)Line[n]);
                tempX = tempX + slope;
            }
            errAvg = errAvg / (float)(End - Start + 1);

            return errAvg;
        }
        //线方差
        private float LineVariance(int16_t[] Line, int16_t Start, int16_t Length)
        {
            int16_t End = Start + Length - 1;

            if (Length == 0)
            {
                return 0.0f;
            }

            float variance = 0.0f;
            float slope = CalcSlope(Line, Start, Length);
            float tempx = (float)Line[Start];

            //方差
            for (int16_t n = Start; n <= End; n++)
            {
                variance = variance + (((float)Line[n] - tempx) * ((float)Line[n] - tempx));
                tempx = tempx + slope;
            }
            variance = variance / (float)Length;

            return variance;
        }
        //平均方差
        private float AvgVariance(int16_t[] Points, int16_t Start, int16_t Length)
        {
            int16_t End = Start + Length - 1;

            float variance = 0.0f;
            float avg = 0.0f;

            //平均数
            for (int16_t n = Start; n <= End; n++)
            {
                avg = avg + (float)Points[n];
            }
            avg /= (End - Start);
            //方差
            for (int16_t n = Start; n <= End; n++)
            {
                variance = variance + (((float)Points[n] - avg) * ((float)Points[n] - avg));
            }
            variance = variance / (float)Length;

            return variance;
        }
        //两点的距离
        private float VectorCos(Point_t V1, Point_t V2)
        {
            float mCos = (float)(V1.X * V2.X + V1.Y * V2.Y) /
                (float)(FastSqrt((float)(V1.X * V1.X + V1.Y * V1.Y)) *
                        FastSqrt((float)(V2.X * V2.X + V2.Y * V2.Y)));

            return mCos;
        }
        //回归方程
        private float CalcSlope(int16_t[] slopePoints, int16_t Start, int16_t Length)
        {
            //XY需要置换计算斜率
            //x = n
            //y = slopePoints
            float b = 0.0f, b1 = 0.0f, b2 = 0.0f;
            float avgn = (float)(0 + Length - 1) / 2.0f;
            float avgSlope = 0.0f;

            if (Length == 0) { return 0; }
            for (int16_t n = 0; n < Length; n++)
            {
                avgSlope = avgSlope + (float)slopePoints[Start + n];
            }
            avgSlope = avgSlope / (float)Length;

            for (int16_t n = 0; n < Length; n++)
            {
                b1 = b1 + (float)(n * slopePoints[Start + n]);
                b2 = b2 + (float)(n * n);
            }
            b1 = b1 - (float)(Length * avgn * avgSlope);
            b2 = b2 - (float)(Length * avgn * avgn);

            if (b2 != 0)
            {
                b = b1 / b2;
            }
            else
            {
                b = 0;
            }
            //b = b * (float)Length;

            return b;
        }

        //Cos值
        private float RectCos(int16_t[] Points)
        {
            //精确计算三点向量角度
            Point_t V1 = new Point_t(Points[2] - Points[1], 2);
            Point_t V2 = new Point_t(Points[1] - Points[0], 2);
            float cs = VectorCos(V1, V2);

            return cs;
        }

    }    
}

          
