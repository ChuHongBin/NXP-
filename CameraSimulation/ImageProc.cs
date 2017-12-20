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

        public int16_t[] LeftLine { get; set; }
        public int16_t[] RightLine { get; set; }
        public int16_t[] MiddleLine { get; set; }
        // //            
        public int16_t[] Widths { get; set; }
        //
        public int16_t LeftLineCnt = 0;
        public int16_t RightLineCnt = 0;
        public int16_t MiddleLineCnt = 0;
        public int16_t MiddleBias = 0;
        public int16_t Useline = 0;
        public  bool   steepup = false;
        public int16_t normline = 0;
        public float Middle_K = 0.0f;
        public float Left_K = 0.0f;
        public float Right_K = 0.0f;
        //判断说明 赋值
        public string  retcond ="........"; 
 
        public int16_t IMAGE_HEIGHT_BAND = 59;
        public int16_t IMAGE_WIDTH_BAND = 79;
        //入环则为 1 ， 出环
        public static  int16_t   OutRingflag = 0;
        public static  int16_t[] MiddLast = new int16_t[60];
        //
        public byte IMAGE_BLACK = (byte)0x00;
        public byte IMAGE_WHITE = (byte)0xff;
        public RodeTypes RodeType { get; set; }
        public byte[,] imagebuff { get; set; }
        public int16_t[] standline { get; set; }
        public static int16_t online = 0;
        public SpeedRode Speed { get; set; }
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
                            imagebuff[ y, (x * 8 + 7 - n)] = IMAGE_WHITE;
                        }
                        else
                        {
                            imagebuff[ y, (x * 8 + 7 - n)] = IMAGE_BLACK;
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
            normline = 0;
            //获取边线  
             GetBorder();
            //生成赛道宽度
            Guide_GenerateWidth(LeftLineCnt);        
            ////判断赛道类型
            while (1 >= 1)
            {
                //      OutRingflag = 1;
                //环形弯
                if (Guide_CheckRing(LeftLineCnt, 0) == RodeTypes.Ring)
                {
                    RodeType = RodeTypes.Ring;
                    break;
                }
                else if (Guide_CheckOutRing(LeftLineCnt, 0) == RodeTypes.OutRing)
                {
                    RodeType = RodeTypes.OutRing;
                    break;
                }
                ////十字弯 (else if)
                else if (Guide_CheckCorss(LeftLineCnt) == RodeTypes.Cross)
                {
                    RodeType = RodeTypes.Cross;
                    break;
                }
                else if (Guide_CheckS(LeftLineCnt) == RodeTypes.LittleS)
                {
                    RodeType = RodeTypes.LittleS;
                    break;
                }
                else if (Guide_CheckStraight(LeftLineCnt) == RodeTypes.Straight)
                {
                    RodeType = RodeTypes.Straight;
                    break;
                }
               RodeType = Guide_CheckDir(LeftLineCnt);
                if (RodeType == RodeTypes.Left)
                {
                    RodeType = RodeTypes.Left;
                    break;
                }
                else if (RodeType == RodeTypes.Right)
                {
                    RodeType = RodeTypes.Right;
                    break;
                }
                else
                {
                    RodeType = RodeTypes.Unknown;
                    break;
                }
            }        
            MiddleLineCnt = LeftLineCnt<RightLineCnt?LeftLineCnt:RightLineCnt;
            online++;
            //求中线
            //标准          
            for (int n = 0; n <= MiddleLineCnt; n++)
            {
                MiddleLine[n] = (int16_t)((LeftLine[n] + RightLine[n]) / 2);              
                if (LeftLine[n] < 0) LeftLine[n] = 0;
                if (LeftLine[n] > 79) LeftLine[n] = 79; 
                if (RightLine[n] > 79) RightLine[n] = 79;
                if (RightLine[n] < 0) RightLine[n] = 0;
                if (MiddleLine[n] > 79) MiddleLine[n] = 79;
                if (MiddleLine[n] < 0) MiddleLine[n] = 0;
            }        
                for (int n = 0; n <= MiddleLineCnt; n++)
                {
                    MiddLast[n] =(RightLine[n] - LeftLine[n]) ;                   
                }         
        }
        //获取边线 
        public void GetBorder()
        {
            int16_t tline;
            int16_t linecnt;
            //      int16_t tleft, tright;
            int16_t width;
            //边线
            RightLine = new int16_t[60];
            LeftLine = new int16_t[60];
            MiddleLine = new int16_t[60];
            //获取起始左右坐标  
            //预处理（5行）
            for (tline = 0; tline < 5; tline++)
            {
                width = Guide_MaxWidthBorder(tline);
                if ((LeftLine[tline] < IMAGE_WIDTH_BAND / 2 && RightLine[tline] > IMAGE_WIDTH_BAND / 2) && width > IMAGE_WIDTH / 3)
                {
                }
                else
                {
                    if (Guide_Center2Board(tline, IMAGE_WIDTH_BAND / 2) < IMAGE_WIDTH_BAND / 3)
                    {
                        
                    }
                    else
                    {
                        LeftLine[tline] = 0;
                        RightLine[tline] = IMAGE_WIDTH_BAND;
                    }
                }
            }
            linecnt = Guide_BorderSearch(tline);
            LeftLineCnt = linecnt;
            RightLineCnt = linecnt;
        }
        //功能：寻找左右边线  并判断是否为赛道尽头
        //输入：[StarLine]起始行；
        //输出：无；
        //返回：处理行；
        public int16_t Guide_BorderSearch(int16_t StartLine)
        {
            int16_t tline, tpixel;
            int16_t tleft, tright;
            int16_t nowleft, nowright;
            ///初始化
            tline = StartLine;
            tright = RightLine[tline - 1];
            tleft = LeftLine[tline - 1];
            //边线查找方式搜索左右边线
            while (tline <= IMAGE_HEIGHT_BAND)
            {
                //判断空行横向检测停止   
                if (Guide_CheckLine(tline, tleft, tright, IMAGE_BLACK))
                {
                    break;
                }
                //查找下一个左标点
                //若上一行左边界为0且本行最左侧也是路径元素，则当前行左边界也为0
                nowleft = 0;
                if (!Guide_CheckLine(tline, 0, tleft, IMAGE_WHITE))
                {
                    for (tpixel = 0; tpixel <= IMAGE_WIDTH_BAND && nowleft == 0; tpixel++)
                    {
                        if (tleft + tpixel + 1 <= IMAGE_WIDTH_BAND && imagebuff[tline, tleft + tpixel] == IMAGE_BLACK && imagebuff[tline, tleft + tpixel + 1] == IMAGE_WHITE)
                        {
                            nowleft = tleft + tpixel + 1;
                        }
                        if (tleft - tpixel >= 0 && imagebuff[tline, tleft - tpixel] == IMAGE_BLACK && imagebuff[tline, tleft - tpixel + 1] == IMAGE_WHITE)
                        {
                            nowleft = tleft - tpixel + 1;
                        }
                    }
                }
                //判断左边界是否越界，若越界则返回
                if (nowleft >= tright)
                {                  
                    break;
                }
                //查找下一个右坐标点，方法同上
                nowright = IMAGE_WIDTH_BAND;
                if (!Guide_CheckLine(tline, tright, IMAGE_WIDTH_BAND, IMAGE_WHITE))
                {
                    for (tpixel = 0; tpixel <= IMAGE_WIDTH_BAND && nowright == IMAGE_WIDTH_BAND; tpixel++)
                    {
                        if (tright + tpixel + 1 <= IMAGE_WIDTH_BAND && imagebuff[tline, tright + tpixel] == IMAGE_WHITE && imagebuff[tline, tright + tpixel + 1] == IMAGE_BLACK)
                        {
                            nowright = tright + tpixel;
                        }
                        if (tright - tpixel >= 0 && imagebuff[tline, tright - tpixel] == IMAGE_WHITE && imagebuff[tline, tright - tpixel + 1] == IMAGE_BLACK)
                        {
                            nowright = tright - tpixel;
                        }
                    }
                }
                if (nowright <= tleft)
                {                 
                    break;
                }
                RightLine[tline] = nowright;
                LeftLine[tline] = nowleft;
                //传递父子变量
                tleft = nowleft;
                tright = nowright;
                tline++;
            }         
            return (tline-1);
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
            LeftLine[tline] =left ;
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
        //功能：生成赛道宽度
        //输入：[Length]处理行;
        //输出：[Widths]宽度数值;
        //返回：无;
        public void Guide_GenerateWidth(int16_t Length)
        {
            Widths = new int16_t[60];
            int16_t tline = Length;

            //计算宽度
            for (tline = 0; tline <= Length; tline++)
            {
                Widths[tline] = RightLine[tline] - LeftLine[tline];
            }

        }
        //功能：判断 处理赛道后的 宽度突变 扔掉
        //输入：[Widths]宽度；
        //输出：处理行（有效宽度）；
        //返回： 有效长度；
        public int16_t Guide_DealWidths()
        {
            int16_t tline = 0;
                       
            tline=0;
            while(tline<LeftLineCnt)
            {
                if (LeftLine[tline] > 77 || RightLine[tline]<2)
                    break;              
                if (!Guide_CheckLine(tline,LeftLine[tline]+2,RightLine[tline]-2,IMAGE_WHITE))
                {
                    return tline;
                }
                tline++;
            }
            return tline;
        }

        //功能：最小二乘法拟合斜率
        //输入：[begin]数组开始；[end]数组结束；[*p]数组首地址
        //输出：无
        //返回：（float）斜率；
        float Slope_Calculate(int16_t begin, int16_t end,int16_t []p)    //最小二乘法拟合斜率
        {
               float xsum=0,ysum=0,xysum=0,x2sum=0;
               int16_t i=0;
               float result=0;
               float resultlast=0;
         
               for(i=begin;i<end;i++)
               {
                 xsum+=i;
                 ysum+=p[i];
                 xysum+=i*p[i];
                 x2sum+=i*i;               
               }
              if(((end-begin)*x2sum-xsum*xsum)!=0) //判断除数是否为零 
              {
                result=((end-begin)*xysum-xsum*ysum)/((end-begin)*x2sum-xsum*xsum);
                resultlast=result;
              }
              else
              {
               result=resultlast;
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
       
        private void ReDrawLines(int16_t[] Line, int16_t Start, int16_t End)
        {
            if (End > Start)
            {
                float slopex = (float)(Line[End] - Line[Start]) / (float)(End - Start + 1);
                float tempX = (float)Line[Start];
                if (slopex > 1.0f)
                {
                    for (int16_t n = Start; n <= End; n++)
                    {
                        if (tempX < 0) { tempX = 0; }
                        if (tempX > (IMAGE_WIDTH - 1)) { tempX = (IMAGE_WIDTH - 1); }
                        tempX = tempX + slopex;
                        Line[n] = (int16_t)(tempX + 0.5f);
                    }
                }
                else if (slopex < -1.0f)
                {
                    for (int16_t n = Start; n <= End; n++)
                    {
                        if (tempX < 0) { tempX = 0; }
                        if (tempX > (IMAGE_WIDTH - 1)) { tempX = (IMAGE_WIDTH - 1); }
                        tempX = tempX + slopex;
                        Line[n] = (int16_t)(tempX + 0.5f);
                    }
                }
                else
                {
                    for (int16_t n = Start; n <= End; n++)
                    {
                        if (tempX < 0) { tempX = 0; }
                        if (tempX > (IMAGE_WIDTH - 1)) { tempX = (IMAGE_WIDTH - 1); }
                        Line[n] = (int16_t)(tempX + 0.5f);
                        tempX = tempX + slopex;
                    }
                }
            }
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

            for(int16_t n = Start; n <= End; n++)
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



       //求三点向量角度即cos值
            
        //十字弯的拐点判断
        private bool IsRectPoint(int16_t[] Points)
        {
            //精确计算三点向量角度
            Point_t V1 = new Point_t(Points[2] - Points[1], 2);
            Point_t V2 = new Point_t(Points[1] - Points[0], 2);
            float cs = VectorCos(V1, V2);

            bool ret = cs > 0.75f ? true : false;

            return ret;
        }
        //Cos值
        private float RectCos(int16_t[] Points)
        {
            //精确计算三点向量角度
            Point_t V1 = new Point_t(Points[2] - Points[1], 2);
            Point_t V2 = new Point_t(Points[1] - Points[0],2);
            float cs = VectorCos(V1, V2);

            return cs;
        }
        //竖直检测是否丢行(xline ,Length )
        //返回假则为丢边
        public bool CheckVerticlLine(int xline, int[] LINE, int Length)
        {
            int tline = 0, line = 0;
            //要在边线的里边
            if (xline < 40)
            {
                ///左边线  
                while (tline < IMAGE_HEIGHT - 20 && tline < Length)
                {
                    if (imagebuff[tline, xline] == IMAGE_WHITE && LINE[tline] <= xline)
                    {
                        line++;
                    }
                    tline++;
                }
            }
            else
            {
                //右边线
                while (tline < IMAGE_HEIGHT - 20 && tline < Length)
                {
                    if (imagebuff[tline, xline] == IMAGE_WHITE && LINE[tline] >= xline)
                    {
                        line++;
                    }
                    tline++;
                }
            }
              bool ret = line*2 < IMAGE_HEIGHT / 3 * 2 ? true : false;
              return ret;
        }
        //丢边为真
        public bool CheckRingLine(int xline, int[] LINE, int Length)
        {
            int tline = 0, line = 0;
            //要在边线的里边
            if (xline < 40)
            {
                ///左边线  
                while (tline < IMAGE_HEIGHT - 10 && tline < Length)
                {
                    if (imagebuff[tline, xline] == IMAGE_WHITE && LINE[tline] <= xline)
                    {
                        line++;
                    }
                    tline++;
                }
            }
            else
            {
                //右边线
                while (tline < IMAGE_HEIGHT - 10 && tline < Length)
                {
                    if (imagebuff[tline, xline] == IMAGE_WHITE && LINE[tline] >= xline)
                    {
                        line++;
                    }
                    tline++;
                }
            }
            bool ret = line  < 40 ? true : false;
            return ret;
        }
         //功能：检查十字路口
        //输入：[*Image]二值化图像，[Length]处理行；
        //输出：无；
        //返回：Rode_Err失败，Rode_Cross成功；
        public RodeTypes Guide_CheckCorss(int16_t Length)
        {
            int16_t tline, leftoverlength = 0, rightoverlength = 0,leftboder=0,rightboder=0;
            int16_t flag,jumpleft=0,jumpright=0,leftdata=0,rightdata=0,downleft=0,downright=0;
            float leftlop = 0.0f, righlop = 0.0f;           
            int16_t upline = 0, downline = 0;
            bool flagup = false;
            bool leftvertical = true, rightvertical = true;
            int[] point = new int16_t[3];

            tline=0;
            while(tline<10)
            {
                if (RightLine[tline] - LeftLine[tline] < 40)
                    return RodeTypes.Unknown;
                tline++;
            }
            ////判断左边界连续丢边
            tline = 0;
            leftoverlength = 0;
            while (tline < Length)
            {
                if (LeftLine[tline] >1)
                {
                    leftoverlength--;
                    if (leftoverlength < 0)
                        leftoverlength = 0;
                    break;
                }
                leftoverlength++;
                tline++;
            }
            //左连续丢线后是否还有丢边
            jumpleft = 0;
            tline=leftoverlength+1;
            while (tline < Length)
            {
                if (LeftLine[tline] <= 1)
                {
                    jumpleft = 1;
                    leftdata = tline;
                    break;
                }                
                tline++;
            }
            //右边线连续丢边
            tline = 0;
            rightoverlength = 0;
            while (tline < Length)
            {
                if (RightLine[tline] < IMAGE_WIDTH_BAND-1 )
                {
                    rightoverlength--;
                    if (rightoverlength < 0)
                        rightoverlength = 0;
                    break;
                }
                rightoverlength++;
                tline++;
            }
            //右连续丢线后是否还有丢边
            jumpright = 0;
            tline=rightoverlength+1 ;
            while (tline < Length)
            {
                if (RightLine[tline] >= IMAGE_WIDTH_BAND-1 )
                {
                    rightdata = tline;
                     jumpright = 1;
                    break;
                }             
                tline++;
            }
            leftvertical = CheckVerticlLine(2, LeftLine, Length);
            rightvertical = CheckVerticlLine(77, RightLine, Length);
            // @1  当两边丢边      
            while (leftvertical==false && rightvertical==false)
            {             
                //判断是否宽度突变
                tline = 0;
                flag = 0;
                //条件                           
                flag = 0;
                //有跳变
                while (tline < Length-10)
                {                                    
                    if (Widths[tline] > Widths[tline + 1] + 6&&Widths[tline+1]>15)
                    {
                        flag = 1;
                        break;
                    }
                    tline++;
                }
                //不能左右不能偏向一边  
                //
                tline = 20;
                while (tline < Length - 10&&tline <50)
                {
                    if(79 - RightLine[tline]>49||LeftLine[tline]>50)
                    {
                        flag = 0;
                        break;
                    }
                    tline++;
                }              
                //无宽度突变 和 宽度突变不符合要求 跳出
                if (flag != 1)
                {
                    break;
                }           
                //左边的拐点
                tline = leftboder;
                leftboder = 0;
                while(tline<=Length -10)
                {
                    float Cos = 0.0f;
                    if (LeftLine[tline] + 3 <= LeftLine[tline + 1] )
                    {
                        leftboder = tline+1;
                        point[0] = LeftLine[leftboder ];
                        point[1] = LeftLine[leftboder-2];
                        point[2] = LeftLine[leftboder + 2];
                        Cos = RectCos(point);
                        break;
                    }
                    tline++;
                }            
                if (leftboder == 0 || RectCos(point) >0.25F)
                {
                    leftboder = 0;
                }
                //右边的拐点
                tline = rightboder;
                rightboder = 0;
                while (tline <= Length - 10)
                {
                    if (RightLine[tline] >= RightLine[tline + 1] + 3 )
                    {
                        float Cos = 0.0f;
                        rightboder = tline + 1;
                        point[0] = RightLine[rightboder];
                        point[1] = RightLine[rightboder-2];
                        point[2] = RightLine[rightboder + 2];
                        Cos = RectCos(point);
                        break;
                    }
                    tline++;
                }               
                if (rightboder == 0 || RectCos(point) >0.25F)
                {
                    rightboder = 0;
                }
                //补左边的线
                //求斜率补线
                //左边线                                                              
                if (leftboder != 0)
                {
                    int leftbodererr = 0;
                    tline = leftboder;
                    while (tline < Length - 2)
                    {
                        leftbodererr = LeftLine[tline] > LeftLine[tline + 1] ? LeftLine[tline] - LeftLine[tline + 1] : LeftLine[tline + 1] - LeftLine[tline];
                        if (leftbodererr <= 1)
                        {
                            break;
                        }
                        else
                        {
                            leftboder = tline + 1;
                        }
                        tline++;
                    }
                    leftlop = 0.0f;
                    upline = leftboder;
                    if (leftboder < 40)
                    {
                        leftlop = Slope_Calculate(upline, upline + 8, LeftLine);
                        for (int tipexl = 0; tipexl <= upline; tipexl++)
                        {

                            LeftLine[tipexl] = LeftLine[upline] + (int16_t)((tipexl - upline) * leftlop);//
                            if (LeftLine[tipexl] >= 80)
                            {
                                LeftLine[tipexl] = 79;
                            }
                            if (LeftLine[tipexl] <= 0)
                            {
                                LeftLine[tipexl] = 0;
                            }
                        }
                    }
                    else 
                    {
                        downleft = 0;
                        tline = 0;
                        while (tline < leftboder / 3 * 2)
                        {
                            if (LeftLine[tline] > LeftLine[downleft])
                            {
                                downleft = tline;
                            }
                            tline++;
                        }
                        leftlop = (float)(LeftLine[upline] - LeftLine[downleft]) / (upline - downleft);

                        for (int tipexl = downleft; tipexl <= upline; tipexl++)
                        {

                            LeftLine[tipexl] = LeftLine[downleft] - (int16_t)((downleft - tipexl) * leftlop);//
                            if (LeftLine[tipexl] >= 80)
                            {
                                LeftLine[tipexl] = 79;
                            }
                            if (LeftLine[tipexl] <= 0)
                            {
                                LeftLine[tipexl] = 0;
                            }
                        }
                    }
                }
                ////求斜率补线
                ////右边线 
                if (rightboder != 0)
                {                     
                    int rightbodererr=0;                 
                    tline = rightboder;
                    while(tline<Length-2)
                    {
                        rightbodererr = RightLine[tline]>RightLine[tline+1]?RightLine[tline]-RightLine[tline+1]:RightLine[tline+1]-RightLine[tline];
                        if(rightbodererr<=1)
                        {
                            break;
                        }
                        else
                        {
                            rightboder = tline+1;
                        }
                        tline++;
                    }
                    upline = rightboder;
                    righlop = 0.0f;
                    if (rightboder < 40)
                    {
                        righlop = Slope_Calculate(upline, upline + 8, RightLine);
                        for (int tipexl = 0; tipexl <= upline; tipexl++)
                        {

                            RightLine[tipexl] = RightLine[upline] + (int16_t)((tipexl - upline) * righlop);//
                            if (RightLine[tipexl] >= 80)
                            {
                                RightLine[tipexl] = 79;
                            }
                            if (RightLine[tipexl] <= 0)
                            {
                                RightLine[tipexl] = 0;
                            }
                        }
                    }                
                else
                {                 
                    downright=0;
                    tline =0;
                    while (tline < rightboder/3*2)
                     {
                        if(RightLine[tline]<RightLine[downright])
                        {
                            downright = tline;                       
                        }
                        tline++;
                      }
                    righlop = (float)(RightLine[upline] - RightLine[downright]) / (upline - downright);
                   
                    for (int tipexl = downright; tipexl <= upline; tipexl++)
                    {

                        RightLine[tipexl] = RightLine[downright] - (int16_t)((downright - tipexl) * righlop);//
                        if (RightLine[tipexl] >= 80)
                        {
                            RightLine[tipexl] = 79;
                        }
                        if (RightLine[tipexl] <= 0)
                        {
                            RightLine[tipexl] = 0;
                        }
                    }
                }
                    
              }
                //没有拐点则跳出判断
                if (leftboder == 0 && rightboder == 0) { return RodeTypes.Unknown; }
                retcond =  "@1判断成功";
                return RodeTypes.Cross;

            }
            /////////////////////////////////////
            // @2  左边线连续丢线到一定距离(右边没有掉)
            //右边可以补线
            while (jumpleft > 0 || leftoverlength>0)          
            {
                int16_t minleft=0,leftbodererr=0;
                 int righterr = 0;                        
                 flagup = false;
                 float rightdirect = 0.0f;
                //判断左边没有跳变点        
                flagup = false;
                leftbodererr=0;
                tline=0;
                while(tline<45)
                {
                    if ( LeftLine[tline] > 10)
                    {
                        flagup = true;
                        break;
                    }
                    tline++;
                }
                if (flagup == true) break;
                   
                tline=0;
                while(tline<Length -10)
                {  
                   
                    leftbodererr = LeftLine[tline] >= LeftLine[tline + 1] ? LeftLine[tline] - LeftLine[tline + 1] : LeftLine[tline + 1] - LeftLine[tline];
                    if (leftbodererr>3)
                    {
                        flagup = true;
                        break;
                    }
                    tline++;
                }
                if (flagup) break;
              
                //右边有直角点           
                tline = 0;
                minleft = 0;
                //找右边最小值
                while(tline<Length-1)
                {
                    if (RightLine[minleft] > RightLine[tline])
                    {
                        minleft = tline;   
                    }
                    if (RightLine[minleft] < RightLine[tline] - 2)
                        break;
                    tline++;
                }
                if (tline >= Length - 6) break;
                rightdirect = Slope_Calculate(minleft, minleft + 6, LeftLine);
                for (int tipxel = minleft + 1; tipxel <= minleft + 6; tipxel++)
                {
                    righterr = righterr + RightLine[tipxel] - RightLine[minleft];
                }           
                if (righterr<=12||rightdirect>1.0f)
                {
                    retcond = "没有拐点";
                    return RodeTypes.Unknown;
                }
                else { if (RightLine[minleft] - LeftLine[minleft] < 20)return RodeTypes.Unknown; }
                //将开始检测上边界的行从新赋值
               //判断右边是否有跳变
                tline = minleft;
                upline = 0;
                while (tline < Length-1)
                {
                    if (RightLine[tline] < RightLine[minleft] - 5 && RightLine[tline]<60)
                    {
                        upline = tline;
                        break;
                    }
                    tline++;
                }
                //没有跳变的。。。
                if (upline <= minleft)
                {
                    //
                    //1.若向上有边界
                    tline = minleft;
                    flagup = false;
                    while(tline<=Length-1)
                    {                     
                        //从左往右
                        if(LeftLine[tline]>10)break;
                        for (int tipexl = LeftLine[tline]; tipexl < 45; tipexl++)
                        {
                            if (imagebuff[tline, tipexl] == IMAGE_WHITE && imagebuff[tline, tipexl + 1] == IMAGE_BLACK)
                            {
                                if (RightLine[tline] <= tipexl) break;
                                RightLine[tline] = tipexl;
                                //找到上边界
                                if (flagup == false)
                                {
                                    upline = tline;
                                    flagup = true;
                                }
                                
                                break;
                            }
                         //   if (flagup == true) break;
                        }
                        tline++;
                    }
                    //对上边界 upline 进行处理
                    if (upline < Length - 2 && RightLine[upline + 1] + 1 <= RightLine[upline])
                    {
                        int16_t rightupline = 0;
                        rightupline = upline + 1;
                        while (rightupline < Length - 2)
                        {
                            if (RightLine[rightupline + 1] + 1 >= RightLine[rightupline])
                            {
                                upline = rightupline;
                                break;
                            }
                            rightupline++;
                        }
                    }
                     //2.找一点
                    if(upline<=0)
                    { 
                        tline=minleft;
                        upline = minleft;
                        while (LeftLine[tline] < 5 && tline<55)
                        {
                            if (LeftLine[upline] >= LeftLine[tline])
                           {                             
                               upline = tline;
                           }
                         tline++;
                        }
                        RightLine[upline] = 0;
                        //改掉有效值
                        RightLineCnt = upline;
                        LeftLineCnt = upline;
                    }                 
                
                }                                                    
                //求斜率补线
                //右边线                                                              
                righlop = 0;                                      
                downline = minleft;
                for (int tipexl = 0; tipexl < 3; tipexl++)
                {
                    int16_t downdata = 0;
                    //下边界超
                    downdata = downline - tipexl;
                    if (downdata <= 0)
                    {
                        downdata = 0;
                    }
                    //斜率
                    righlop += (float)(upline - downdata) / (RightLine[upline] - RightLine[downdata]);
                }
                righlop /= 3;
                for (int tipexl = downline; tipexl <= upline; tipexl++)
                {
                    RightLine[tipexl] = (int)((tipexl - downline) / righlop) + RightLine[downline];
                    if (RightLine[tipexl] >= 80)
                    {
                        RightLine[tipexl] = 79;
                    }
                    if (RightLine[tipexl] <= 0)
                    {
                        RightLine[tipexl] = 0;
                    }
                }
                ///改有效值
                if (LeftLineCnt-2>upline)
                {
                    //有边线条线就改掉
                    tline=upline;
                    while(tline<Length)
                    {
                        if(LeftLine[tline]+5 <= LeftLine[tline+1])
                        {
                            LeftLineCnt = tline;
                            RightLineCnt = tline;
                            break;
                        }
                        tline++;
                    }
                }
                retcond = "@2  左边丢线判断成功";
                return RodeTypes.Cross;
            }
            // @3
            //右边线连续丢线到一定距离(左边没有掉)
            //左边可以补线
           while (rightoverlength > 0 || jumpright > 0)
            {
                int16_t minright=0,maxright=0;
                int16_t lefterr = 0;
                float leftdirect=0.0f;        
                flagup = false;
                tline = 0;
                while (tline <10 )  // 
                {
                    if (RightLine[tline] <IMAGE_WIDTH_BAND - 10)
                    {
                        flagup = true;
                        break;
                    }
                    tline++;
                }
                if (flagup == true) break;
                //判断右边线没有突变
                tline = 0;
                if(RightLine[tline]>=IMAGE_WIDTH_BAND -5)
                {
                    //找右边最大值且没有跳变                    
                    minright = 0;
                    while(tline<Length-2&&tline<50)
                    {                  
                        if (RightLine[tline] < IMAGE_WIDTH_BAND - 5 && tline < 30) { flagup = true; break; }
                        if (RightLine[tline] > RightLine[tline + 1] + 3 || RightLine[tline + 1] > RightLine[tline] + 3)
                        {
                            flagup = true;
                            break; 
                        }
                        if (RightLine[minright] <= RightLine[tline])
                        {
                            minright = tline;       
                        }
                        tline++;
                    }
                }
                else { break; }                      
                if (flagup) break;
                //左边有找曲率              
                tline = 0;
                minright = 0;
                //找左边极大值点
                while(tline<Length-1)
                {
                    if (LeftLine[minright] < LeftLine[tline])
                    {
                        minright = tline;   
                    }
                    if(LeftLine[minright] > LeftLine[tline]+2)
                    {
                        break;
                    }
                    if (LeftLine[minright] > 0 && LeftLine[tline + 1] <= 0)
                        break;
                    tline++;
                }
                tline = minright;
                if (tline >= Length - 6) break; 
                //向上6行判断误差
                leftdirect =Slope_Calculate(minright,minright+6,RightLine);
                lefterr = 0;               
                for (int16_t tipexl = minright+1; tipexl <= minright + 6; tipexl++)
                {
                    lefterr = lefterr + (LeftLine[minright] - LeftLine[tipexl]);
                }
                if (lefterr <= 12 || leftdirect<-0.5F)
                {   
                    retcond = "@3左边界 没有直角点";
                    return RodeTypes.Unknown;
                }
                else { if (RightLine[tline] - LeftLine[tline] < 20)return RodeTypes.Unknown; }
                //可以判
                //将开始检测上边界的行从新赋值
                 //判断左边是否有跳变
                tline = maxright;
                upline = 0;
                while (tline < Length-1)
                {
                    if (LeftLine[tline] > LeftLine[minright]-5 && LeftLine[minright]>20)
                    {
                        upline = tline;
                        break;
                    }
                    tline++;
                }
                //没有跳变的。。。
                if (upline <= minright)
                {
                    //
                    //1.若向上有边界
                    tline = minright;
                    flagup = false;
                    while(tline<= Length-1)
                    {                     
                        //从右往左
                        if(RightLine[tline]<IMAGE_WIDTH_BAND-10)break;
                        for (int tipexl = RightLine[tline]; tipexl > 35; tipexl--)
                        {
                            if (imagebuff[tline, tipexl] == IMAGE_WHITE && imagebuff[tline, tipexl - 1] == IMAGE_BLACK)
                            {
                                if (LeftLine[tline] >= tipexl) break;
                                LeftLine[tline] = tipexl;
                                //找到上边界
                                if (flagup == false)
                                {
                                    upline = tline;
                                    flagup = true;
                                }                               
                                break;
                            }
                         //   if (flagup == true) break;
                        }
                        tline++;
                    }
                    //对上边界 upline 进行处理
                    if (upline < Length - 2 && LeftLine[upline] + 1 <= LeftLine[upline+1])
                    {
                        int16_t rightupline = 0;
                        rightupline = upline + 1;
                        while (rightupline < Length - 2)
                        {
                            if (LeftLine[rightupline] + 1 <= LeftLine[rightupline+1])
                            {
                                upline = rightupline;
                                break;
                            }
                            rightupline++;
                        }
                    }
                     //2.找右边一点
                    if(upline<=10)
                    {
                        tline = minright;
                        upline = minright;
                        while (tline <= 59 && RightLine[tline] > IMAGE_WIDTH_BAND - 5)
                        {
                            if (RightLine[upline] <= RightLine[tline])
                           {                             
                               upline = tline;
                           }
                         tline++;
                        }
                        LeftLine[upline] = IMAGE_WIDTH_BAND;
                        //改掉有效值
                        RightLineCnt = upline;
                        LeftLineCnt =  upline;
                        //是否为十字弯
                        if (upline - minright + 5 < RightLine[minright] - LeftLine[minright])
                        {
                            return RodeTypes.Unknown;
                        }
                    }                 
                
                }  
                
                //求斜率补线
                //左边线                                                              
                leftlop = 0;
                downline = minright;
                for (int tipexl = 0; tipexl < 3; tipexl++)
                {
                    int16_t downdata = 0;
                    //下边界超
                    downdata = downline - tipexl;
                    if (downdata <= 0)
                    {
                        downdata = 0;
                    }
                    //斜率
                    leftlop += (float)(upline - downdata) / (LeftLine[upline] - LeftLine[downdata]);
                }
                leftlop /= 3;
                for (int tipexl = downline + 1; tipexl <= upline; tipexl++)
                {
                    LeftLine[tipexl] = (int)((tipexl - downline) / leftlop) + LeftLine[downline];
                    if (LeftLine[tipexl] >= 80)
                    {
                        LeftLine[tipexl] = 79;
                    }
                    if (LeftLine[tipexl] <= 0)
                    {
                        LeftLine[tipexl] = 0;
                    }
                }
                ///改有效值
                if (RightLineCnt - 2 > upline)
                {
                    //有边线条线就改掉
                    tline = upline;
                    while (tline < Length)
                    {
                        if (RightLine[tline+1] + 5 <= RightLine[tline])
                        {
                            LeftLineCnt = tline;
                            RightLineCnt = tline;
                            break;
                        }
                        tline++;
                    }

                }
                retcond = "@3 右边丢线 判断成功";
                return RodeTypes.Cross;
            }                 
  
            /////////////////////////////////////
            // @4  排除前几行后两边是否为边界
            //
            while (jumpleft == 1|| jumpright == 1)//&&thisok==0
            {
                int leftbodererr=0,rightbodererr=0;
                bool leftstep = false,rightstep = false;
                float leftUK = 0.0F, leftDK = 0.0F;
                //找左边的跳变点
                tline = 10;
                while(tline<Length-5)
                {
                    if (RightLine[tline + 1] - LeftLine[tline + 1] < 30 ) return RodeTypes.Unknown;
                    leftbodererr = LeftLine[tline]>LeftLine[tline+1]? LeftLine[tline]-LeftLine[tline+1]:LeftLine[tline+1]-LeftLine[tline];
                     if(leftbodererr>3)
                     {
                         leftbodererr = LeftLine[tline]>LeftLine[tline+2]? LeftLine[tline]-LeftLine[tline+2]:LeftLine[tline+2]-LeftLine[tline];
                         if(leftbodererr>3)
                         {
                             //误差成立则判断方向是否相反
                             leftUK = (float)(LeftLine[tline + 1] - LeftLine[tline - 5]) / (tline + 1 - tline - 5);
                             leftDK = (float)(LeftLine[tline ] - LeftLine[tline - 5]) / (tline  - tline - 5);
                             if (leftUK * leftDK <= 2.0f) 
                             {
                                 leftdata = tline;
                                 leftstep = true;
                                 break;
                             }
                         }
                     }
                     tline++;
                }
                //找右边的跳变点
                tline = 10;
                while (tline < Length - 5)
                {
                    rightbodererr = RightLine[tline] > RightLine[tline + 1] ? RightLine[tline] - RightLine[tline + 1] : RightLine[tline + 1] - RightLine[tline];
                    if (rightbodererr > 3)
                    {
                        rightbodererr = RightLine[tline] > RightLine[tline + 2] ? RightLine[tline] - RightLine[tline + 2] : RightLine[tline + 2] - RightLine[tline];
                        if (rightbodererr > 3)
                        {
                            //误差成立则判断方向是否相反
                            leftUK = (float)(RightLine[tline + 1] - RightLine[tline - 5]) / (tline + 1 - tline - 5);
                            leftDK = (float)(RightLine[tline] - RightLine[tline - 5]) / (tline  - tline - 5);
                            if (leftUK * leftDK <= 2.0f)
                            {
                                rightdata = tline;
                                rightstep = true;
                                break;
                            }
                        }
                    }
                    tline++;
                }
                ///判断是否为转折点
                ///有跳变点
                if(leftstep&&rightstep)
                {  
                    //左边
                    if(leftstep)
                    {
                        leftstep = false;
                        leftbodererr = 0;
                        for (int16_t tipexl = leftdata; tipexl <= leftdata + 4; tipexl++)
                        {
                            leftbodererr = leftbodererr + LeftLine[leftdata] - LeftLine[tline]; 
                        }
                        if (leftbodererr > 12 || leftbodererr< -20)
                        {
                            leftstep = true;
                        }
                    }
                    //右边
                    if(rightstep)
                    {
                        rightstep = false;
                        rightbodererr = 0;
                        for (int16_t tipexl = rightdata; tipexl <= rightdata + 4; tipexl++)
                        {
                            rightbodererr = rightbodererr + RightLine[tipexl] - RightLine[rightdata];
                        }
                        if (rightbodererr > 12 || rightbodererr<-20)
                        {
                            rightstep = true;
                        }
                    }
                    //两者一方没有直角拐点则跳出
                    if(leftstep==false&&rightstep==false)
                    {
                        retcond = "//两者没有直角拐点则跳出";
                        return RodeTypes.Unknown;
                    }
                }
                else
                {
                    retcond = "//没有跳变点";
                    return RodeTypes.Unknown;
                }         
                //求斜率并补线 左边
                if (leftdata!=0)
                {
                    int[] leftvirtul = new int16_t[3];
                    int leftcorrect = 0;
                    //若此点 大于下一个点则为下边界
                    if(LeftLine[leftdata]>=LeftLine[leftdata+1])
                    {                        
                        //向上寻边界
                        //向下找不触碰边界
                        leftboder=leftdata;
                        tline=leftdata;
                        while(tline>0)
                        {
                            if(LeftLine[tline]<2)
                            {
                                break;
                            }
                            leftboder--;
                            tline--;
                        }
                        leftlop = 0.0f;
                        //比较好的斜率                                               
                        leftlop = Slope_Calculate(leftboder, leftdata-1, LeftLine);
                        leftboder = (leftdata + leftboder) / 2;
                        for (int16_t tipexl = leftdata; tipexl < Length - 2; tipexl++)                         
                        {
                            //  LeftLine[tipexl] = LeftLine[leftdata] + (int16_t)((tipexl - leftdata) * leftlop);//
                            leftvirtul[leftcorrect] = LeftLine[leftboder] + (int16_t)((tipexl - leftboder) * leftlop);//
                            if (leftvirtul[leftcorrect] <= 0) leftvirtul[leftcorrect] = 0;
                            if (leftvirtul[leftcorrect] >= 79)leftvirtul[leftcorrect] = 79;
                            //整合左边线
                            if (leftcorrect >= 1)
                            {
                                int errone = 0, errtwo = 0;
                                leftcorrect = -1;
                                errone = leftvirtul[0] > LeftLine[tipexl - 1] ? leftvirtul[0] - LeftLine[tipexl - 1] : LeftLine[tipexl - 1] - leftvirtul[0];
                                errtwo = leftvirtul[1] > LeftLine[tipexl] ? leftvirtul[1] - LeftLine[tipexl] : LeftLine[tipexl] - leftvirtul[1];
                                if (errone <= 3 && errtwo <= 3 && tipexl > leftdata + 3)
                                {
                                    LeftLineCnt = tipexl;
                                    break;
                                }
                                else
                                {
                                    LeftLine[tipexl] = leftvirtul[1];
                                    LeftLine[tipexl - 1] = leftvirtul[0];
                                }

                            }
                            leftcorrect++;
                            }
                        
                    }
                    else  //若此点小于下一点则为上边界
                    {
                        //向下寻边界
                        //找不跳变点
                        tline=leftdata;
                        while (tline < Length - 6)
                        {
                            if(LeftLine[tline]+3>=LeftLine[tline+1]&&LeftLine[tline+1]+3>=LeftLine[tline+2])
                            {
                                leftboder =tline;
                                break;
                            }
                            tline++;
                        }
                        if(leftboder<Length-10)
                        {
                            
                            leftlop = Slope_Calculate(leftboder, leftboder + 6, LeftLine);                          
                            leftdata = leftboder;
                            for (int16_t tipexl = leftdata; tipexl >= 0; tipexl--)
                            {
                             //   LeftLine[tipexl] = LeftLine[leftboder] + (int16_t)((tipexl - leftboder) * leftlop);//
                                leftvirtul[leftcorrect] = LeftLine[leftboder] + (int16_t)((tipexl - leftboder) * leftlop);//
                                if (leftvirtul[leftcorrect] <= 0) leftvirtul[leftcorrect] = 0;
                                if (leftvirtul[leftcorrect] >= 79) leftvirtul[leftcorrect] = 79;
                                //整合左边线
                                if (leftcorrect >= 1)
                                {
                                    int errone = 0, errtwo = 0;
                                    leftcorrect = -1;
                                    errone = leftvirtul[0] > LeftLine[tipexl + 1] ? leftvirtul[0] - LeftLine[tipexl + 1] : LeftLine[tipexl + 1] - leftvirtul[0];
                                    errtwo = leftvirtul[1] > LeftLine[tipexl] ? leftvirtul[1] - LeftLine[tipexl] : LeftLine[tipexl] - leftvirtul[1];
                                    if (errone <= 2 && errtwo <= 2 && tipexl < leftboder - 2)
                                    {                                       
                                        break;
                                    }
                                    else
                                    {
                                        LeftLine[tipexl] = leftvirtul[1];
                                        LeftLine[tipexl + 1] = leftvirtul[0];
                                    }

                                }
                                leftcorrect++;
                            }
                        }
                    }                 
                }
                //求斜率并补线 右边
                if (rightdata!=0)
                {  
                    //若此点小于下一点则为下边界
                    int[] rightvirtul = new int16_t[3];
                    int rightcorrect = 0;
                    if (RightLine[rightdata] <= RightLine[rightdata + 1])
                    {
                        //向上寻边界
                        //向下找不触碰边界
                        rightboder = rightdata;
                        tline = rightdata;
                        while (tline > 0)
                        {
                            if (RightLine[tline] > IMAGE_WIDTH_BAND-1)
                            {
                                break;
                            }
                            rightboder--;
                            tline--;
                        }
                        righlop = 0.0f;
                        //比较好的斜率                                               
                        righlop = Slope_Calculate(rightboder, rightdata, RightLine);
                        rightboder = (rightdata + rightboder) / 2;
                        for (int16_t tipexl = rightdata; tipexl < Length - 2; tipexl++)
                        {
                            rightvirtul[rightcorrect] = RightLine[rightboder] + (int16_t)((tipexl - rightboder) * righlop);//RightLine[tipexl]
                            if (rightvirtul[rightcorrect] <= 0) rightvirtul[rightcorrect] = 0;
                            if (rightvirtul[rightcorrect] >= 79) rightvirtul[rightcorrect] = 79;
                            //整合左边线
                            if (rightcorrect >= 1)
                            {
                                int errone = 0, errtwo = 0;
                                rightcorrect = -1;
                                errone = rightvirtul[0] > RightLine[tipexl - 1] ? rightvirtul[0] - RightLine[tipexl - 1] : RightLine[tipexl - 1] - rightvirtul[0];
                                errtwo = rightvirtul[1] > RightLine[tipexl] ? rightvirtul[1] - RightLine[tipexl] : RightLine[tipexl] - rightvirtul[1];
                                if (errone <= 3 && errtwo <= 3 && tipexl > rightdata + 3)
                                {
                                    RightLineCnt = tipexl;
                                    break;
                                }
                                else
                                {
                                    RightLine[tipexl] = rightvirtul[1];
                                    RightLine[tipexl - 1] = rightvirtul[0];
                                }

                            }
                            rightcorrect++;
                        }

                    }
                    else  //若此点小于下一点则为上边界
                    {
                        //向下寻边界
                        //找不跳变点
                        tline = rightdata;
                        while (tline < Length - 6)
                        {
                            if (RightLine[tline]  <= RightLine[tline + 1]+3 && RightLine[tline + 1]  <= RightLine[tline + 2]+3)
                            {
                                rightboder = tline;
                                break;
                            }
                            tline++;
                        }
                        if (rightboder < Length - 10)
                        {

                            righlop = Slope_Calculate(rightboder, rightboder + 6, RightLine);
                            rightdata = rightboder;
                            for (int16_t tipexl = rightdata; tipexl >= 0; tipexl--)
                            {
                                //   LeftLine[tipexl] = LeftLine[leftboder] + (int16_t)((tipexl - leftboder) * leftlop);//
                                rightvirtul[rightcorrect] = RightLine[rightboder] + (int16_t)((tipexl - rightboder) * righlop);//
                                if (rightvirtul[rightcorrect] <= 0) rightvirtul[rightcorrect] = 0;
                                if (rightvirtul[rightcorrect] >= 79) rightvirtul[rightcorrect] = 79;
                                //整合左边线
                                if (rightcorrect >= 1)
                                {
                                    int errone = 0, errtwo = 0;
                                    rightcorrect = -1;
                                    errone = rightvirtul[0] > RightLine[tipexl + 1] ? rightvirtul[0] - RightLine[tipexl + 1] : RightLine[tipexl + 1] - rightvirtul[0];
                                    errtwo = rightvirtul[1] > RightLine[tipexl] ? rightvirtul[1] - RightLine[tipexl] : RightLine[tipexl] - rightvirtul[1];
                                    if (errone <= 2 && errtwo <= 2 && tipexl < leftboder - 2)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        RightLine[tipexl] = rightvirtul[1];
                                        RightLine[tipexl + 1] = rightvirtul[0];
                                    }

                                }
                                rightcorrect++;
                            }
                        }
                    }
                    //若此点 大于下一个点则为上边界                
                    
                }
          
                retcond = "@4 两边是边界判断成功";
                return RodeTypes.Cross;
            } 

            
             return RodeTypes.Unknown;
        }

        //功能：检查环形路口
        //输入：[*Image]二值化图像，[Length]处理行,turnlight =0左转弯；
        //输出：无；
        //返回：Rode_Err失败，Rode_Ring成功；

        public RodeTypes Guide_CheckRing(int16_t Length, int TurnLight)
        {
            int16_t tline = 0,flagleft=0,flagright=0;
            int16_t [] point = new int16_t [3];
            int16_t leftdown=0,rightdown=0;
            float leftk = 0.0f, rightk = 0.0f;
            int16_t throwendgeL = 0, throwendgeR=0;
            bool RingAddLine = false;

            //出环检测 ----
            if (OutRingflag == 1) return RodeTypes.Unknown;
            //入环
            //有效值大于
            if (Length <= 40) return RodeTypes.Unknown;         
            //判断左变是否丢边
            if (CheckVerticlLine(2, LeftLine, Length))
            {
                //没有丢边则求
                //找左边线的极大值              
                tline = 0;
                leftdown = 0;
                while (tline < Length - 10)
                {
                    if (RightLine[tline] - LeftLine[tline] < 20)
                    {
                        return RodeTypes.Unknown;
                    }
                    if (LeftLine[tline] > LeftLine[leftdown])
                    {
                        leftdown = tline;
                    }
                    if (LeftLine[leftdown] > LeftLine[tline] + 2)
                    {
                        ///有极值点                   
                        flagleft = 1;
                        break;
                    }
                    tline++;
                }
                //判断左拐点是否符合要求
                if (flagleft == 1)
                {
                    int Lefterr = 0, leftfirm = 0;
                    float Cospoint = 0.0f;
                    flagleft = 0;
                    //判断是否为硬拐
                    for (int tipexl = leftdown + 1; tipexl <= leftdown + 6; tipexl++)
                    {
                        Lefterr = Lefterr + (LeftLine[leftdown] - LeftLine[tipexl]);
                    }
                    if (Lefterr >= 15)
                    {
                        leftfirm = 1;
                    }
                    else
                    {  
                        int sameleft=leftdown;
                        tline = leftdown;
                        while (tline < leftdown+10&&tline<Length-2)
                        {
                            if (LeftLine[tline] < LeftLine[leftdown])
                            {
                                sameleft--;
                                break;
                            }
                            sameleft++;
                            tline++;
                        }
                        point[0] =LeftLine[sameleft+2];
                        point[1] =LeftLine[sameleft];
                        point[2] =LeftLine[sameleft + 4];
                        Cospoint = RectCos(point);
                    }
                    //拐点成立
                    if (leftfirm == 1 || Cospoint < -0.25f)
                    {
                        flagleft = 1;
                        //求斜率
                        //向下求边界线
                        tline = leftdown;
                        while (tline > 0)
                        {
                            if (LeftLine[tline] < 2)
                            {
                                break;
                            }
                            tline--;
                        }
                        //向上延的斜率
                        leftk = (float)(leftdown - tline) / (LeftLine[leftdown] - LeftLine[tline]);
                    }
                }
            }
            else { leftk = 0; throwendgeL = 1; }
            //判断右边是否丢边
            if (CheckVerticlLine(IMAGE_WIDTH_BAND - 2, RightLine, Length))
            {
                //没有丢边
                //求右边的极小值               
                tline = 0;
                rightdown = 0;
                while (tline < Length - 10)
                {
                    if (RightLine[tline] - LeftLine[tline] < 20)
                    {
                        return RodeTypes.Unknown;
                    }
                    if (RightLine[tline] < RightLine[rightdown])
                    {
                        rightdown = tline;
                    }
                    if (RightLine[rightdown] < RightLine[tline] - 2)
                    {
                        ///有极值点                   
                        flagright = 1;
                        break;
                    }
                    tline++;
                }
                //判断左拐点是否符合要求
                if (flagright == 1)
                {
                    float Cospoint = 1.0f;
                    int righterr = 0, rightfirm = 0;
                    flagright = 0;
                    //判断是否为硬拐
                    for (int tipexl = rightdown + 1; tipexl <= rightdown + 6; tipexl++)
                    {
                        righterr = righterr + (RightLine[tipexl] - RightLine[rightdown]);
                    }
                    if (righterr >= 15)
                    {
                        rightfirm = 1;
                    }
                    else
                    {
                        int sameright = rightdown;
                        tline = rightdown;
                        while (tline < rightdown + 10 && tline < Length - 2)
                        {
                            if (RightLine[tline] > RightLine[rightdown])
                            {
                                sameright--;
                                break;
                            }
                            sameright++;
                            tline++;
                        }
                        point[0] = RightLine[sameright + 2];
                        point[1] = RightLine[sameright];
                        point[2] = RightLine[sameright + 4];
                        Cospoint = RectCos(point);
                    }
                    //拐点成立
                    if (rightfirm == 1 || Cospoint < -0.25f)
                    {
                        flagright = 1;
                        retcond = "右拐点成立";
                        //求斜率
                        //向下求边界线
                        tline = rightdown;
                        while (tline > 0)
                        {
                            if (RightLine[tline] > IMAGE_WIDTH_BAND - 2)
                            {
                                break;
                            }
                            tline--;
                        }
                        //向上延的斜率
                        rightk = (float)(rightdown - tline) / (RightLine[rightdown] - RightLine[tline]);
                    }
                }
            }
            else { rightk = 0; throwendgeR = 1;}
            //环和十字弯的区别
            //两边都丢线
            if (CheckVerticlLine(2,LeftLine, Length) != true && CheckVerticlLine(77,RightLine, Length) != true)
            {
                int Verticalleft = 0, Verticalright = 0;
                int averageline = 0;

              ///  if (OutRingflag == 1) return RodeTypes.Unknown;
                if (LeftLine[30] > 15 || RightLine[30] < 65) return RodeTypes.Unknown;
                if (LeftLine[0] > 10 || RightLine[30] < 70) return RodeTypes.Unknown;                                            

                //前20行的最值
                tline=0;
                Verticalleft = 0;
                Verticalright = 79;
                while(tline<20)
                {
                    if (LeftLine[tline] > Verticalleft)
                    {
                        Verticalleft = LeftLine[tline];
                    }
                    if (RightLine[tline] < Verticalright)
                    {
                        Verticalright = RightLine[tline]; 
                    }
                    tline++;
                }
                Verticalleft = Verticalleft +(int16_t)(IMAGE_WIDTH_BAND*0.25F);
                Verticalright = Verticalright - (int16_t)(IMAGE_WIDTH_BAND * 0.25F);
                tline = 0;
                while (tline<55)               
                {  
                    if (Guide_CheckLine(tline, Verticalleft, Verticalright,IMAGE_BLACK))
                    {
                        averageline++;
                    }
                    tline++;
                }      
                if(averageline>5)
                {              
                    retcond = "俩边是丢边";
                    RingAddLine = true;             
                }
                //判断入环里了
                if(RingAddLine)
                {
                    int Ringinflag = 0;
                  //俩边都是边界线
                    tline = 0;
                  while(tline <= 10)
                  {
                     if(LeftLine[tline]>=2)
                     { 
                         break;
                     }
                     tline++;
                  }
                  if (tline >= 10) { Ringinflag = 1; }             
                    //左边满足
                   if(Ringinflag >=1)
                   {
                       tline = 0;
                       while (tline <= 10)
                       {
                           if (RightLine[tline] <78)
                           {
                               break;
                           }
                           tline++;
                       }
                       if (tline >= 10) { OutRingflag = 1; }    
                   }
                }
            }
            //一边丢线或不丢
            if ((flagleft == 1 && throwendgeR == 1) || (flagright == 1 && throwendgeL == 1) || (flagright == 1 && flagleft==1))
            {
                int[] leftxline = new int16_t[60];
                int[] rightxline = new int16_t[60];              
                int16_t average = 0,averageline=0;
                int Verticalleft = 0, Verticalright = 0;
                //先找左右边线起点
                //两边不丢线
                if(flagright==1&&flagleft==1)
                {
                    average = leftdown > rightdown ? leftdown : rightdown;
                    Verticalleft = (int16_t)(RightLine[rightdown] - LeftLine[leftdown]) / 4 + LeftLine[leftdown];
                    Verticalright = RightLine[rightdown] - (int16_t)(RightLine[rightdown] - LeftLine[leftdown]) / 4;
                }
                else if (flagleft == 1)
                {
                    average = leftdown;
                    Verticalleft = (int16_t)(79 - LeftLine[leftdown]) / 4 + LeftLine[leftdown];
                    Verticalright = LeftLine[leftdown] + (int16_t)(RightLine[leftdown] - LeftLine[leftdown]) / 4*3;
                }
                else
                {
                    average = rightdown;
                    Verticalleft = RightLine[rightdown] - (int16_t)(RightLine[rightdown] - LeftLine[rightdown]) / 4*3;
                    Verticalright = RightLine[rightdown] - (int16_t)(RightLine[rightdown] - LeftLine[rightdown]) / 4;
                }
                //左边的拟合边线
                //右边的拟合边线
                tline= average;               
             　 while(tline<IMAGE_HEIGHT_BAND)
               {
                   if (leftk != 0.0f)
                       leftxline[tline] = Verticalleft + (int)((tline - average) / leftk);///
                   else
                       leftxline[tline] = Verticalleft;
                   if (rightk != 0.0f)
                       rightxline[tline] = Verticalright + (int)((tline - average) / rightk);//
                   else
                       rightxline[tline] = Verticalright;
                   //越界判断
                   if (leftxline[tline] <= 0 || leftxline[tline] >= 79) break;
                   if (rightxline[tline] <= 0 || rightxline[tline] >= 79) break;
                   if (leftxline[tline] > rightxline[tline]) break;

                   //if (leftk != 0.0f)
                   //    LeftLine[tline] = Verticalleft + (int)((tline - average) / leftk);///
                   //else
                   //    LeftLine[tline] = Verticalleft;
                   //if (rightk != 0.0f)
                   //    RightLine[tline] = Verticalright + (int)((tline - average) / rightk);//
                   //else
                   //    RightLine[tline] = Verticalright;
                   ////越界判断
                   //if (LeftLine[tline] <= 0 || LeftLine[tline] >= 79) break;
                   //if (RightLine[tline] <= 0 || RightLine[tline] >= 79) break;
                   //if (LeftLine[tline] > RightLine[tline]) break;

                   tline++;                   
               }               
               //超过三行就可以判定是环
               averageline = 0;
               while ( average< tline )           
               {

                   //if (Guide_CheckLine(average, LeftLine[tline], RightLine[tline], IMAGE_BLACK))
                   //{
                   //    averageline++;
                   //}
                   if (Guide_CheckLine(average, leftxline[average], rightxline[average], IMAGE_BLACK))
                   {
                       averageline++;
                   }
                   average++;
               }
               if (averageline > 3)
               {               
                   retcond = "单边丢线且中间黑色";
                   RingAddLine = true;
              //     return RodeTypes.Ring;
               }
            //   retcond = "单边丢线且中间黑色不满足";
            }
            //判断环形成功 补线
            if (RingAddLine)
            {
                int16_t upline = 0;
                float rightlop = 0.0f,leftlop=0.0f;
                //左转弯
                if (TurnLight == 0)
                {
                    //以右边的拐点为起始点补线
                    //以左边最小值为终点
                    //找终点即左最小值
                    tline = 20;
                    upline = tline;
                    while (tline < Length - 1)
                    {
                        if (LeftLine[tline] <= LeftLine[upline])
                        {
                            upline = tline;
                        }
                        tline++;
                    }
                    if (rightdown != 0 || leftdown != 0)
                    {
                        if (rightdown == 0) rightdown = leftdown;
                        //补右边线
                        rightlop = (float)(LeftLine[upline] - RightLine[rightdown]) / (upline - rightdown);
                        RightLineCnt = upline;
                        for (int16_t tipexl = rightdown; tipexl < upline; tipexl++)
                        {
                            RightLine[tipexl] = RightLine[rightdown] + (int)((tipexl - rightdown) * rightlop);
                            if (RightLine[tipexl] <= 0) RightLine[tipexl] = 0;
                            if (RightLine[tipexl] >= 79) RightLine[tipexl] = 79;
                        }
                        return RodeTypes.Ring;
                    }
                    else
                    {
                        //向上寻找底边（右边）
                        tline = 0;
                        rightdown = 0;
                        while (tline < 25)
                        {
                            if (RightLine[tline] <= RightLine[rightdown])
                            {
                                rightdown = tline;
                            }
                            if (3 + RightLine[rightdown] <= RightLine[tline])
                            {
                                break;
                            }
                            tline++;
                        }
                        if (RightLine[rightdown] >= 78)
                        { 
                            rightdown = 0;                          
                            upline = (int16_t)upline * 2 / 3;                        
                        }
                        //补右边线
                        rightlop = (float)(LeftLine[upline] - RightLine[rightdown]) / (upline - rightdown);
                        RightLineCnt = upline;
                        for (int16_t tipexl = rightdown; tipexl < upline; tipexl++)
                        {
                            RightLine[tipexl] = RightLine[rightdown] + (int)((tipexl - rightdown) * rightlop);
                            if (RightLine[tipexl] <= 0) RightLine[tipexl] = 0;
                            if (RightLine[tipexl] >= 79) RightLine[tipexl] = 79;
                        }
                        return RodeTypes.Ring;

                    }
                }
                //右转弯TurnLight =1;
                else
                {
                    //以左边的拐点为起始点补线
                    //以右边最大值为终点                                     
                    tline = 20;
                    upline = tline;
                    while (tline < Length - 1)
                    {
                        if (RightLine[tline] >= RightLine[upline])
                        {
                            upline = tline;
                        }
                        tline++;
                    }
                    if (rightdown != 0 || leftdown != 0)
                    {
                        if (leftdown == 0) leftdown = rightdown;
                        //补左边线
                        leftlop = (float)(RightLine[upline] - LeftLine[leftdown]) / (upline - leftdown);
                        LeftLineCnt = upline;
                        for (int16_t tipexl = leftdown; tipexl < upline; tipexl++)
                        {
                            LeftLine[tipexl] = LeftLine[leftdown] + (int)((tipexl - leftdown) * leftlop);
                            if (LeftLine[tipexl] <= 0) LeftLine[tipexl] = 0;
                            if (LeftLine[tipexl] >= 79) LeftLine[tipexl] = 79;
                        }
                        return RodeTypes.Ring;
                    }
                    else
                    {
                        //向上寻找底边（左边）
                        tline = 0;
                        leftdown = 0;
                        while (tline < 25)
                        {
                            if (LeftLine[tline] >= LeftLine[leftdown])
                            {
                                leftdown = tline;
                            }
                            if ( LeftLine[leftdown] >= LeftLine[tline]+3)
                            {
                                break;
                            }
                            tline++;
                        }
                        if (LeftLine[leftdown] <= 1)
                        {                       
                            leftdown = 0; 
                            upline= (int16_t)upline*2/3;
                          

                        }
                        //补左边线
                        leftlop = (float)(RightLine[upline] - LeftLine[leftdown]) / (upline - leftdown);
                        LeftLineCnt = upline;
                        for (int16_t tipexl = leftdown; tipexl < upline; tipexl++)
                        {
                            LeftLine[tipexl] = LeftLine[leftdown] + (int)((tipexl - leftdown) * leftlop);
                            if (LeftLine[tipexl] <= 0) LeftLine[tipexl] = 0;
                            if (LeftLine[tipexl] >= 79) LeftLine[tipexl] = 79;
                        }
                        return RodeTypes.Ring;

                    }

                }
            }
            return RodeTypes.Unknown;
        }


        //功能：检查环形出路口
        //输入：[*Image]二值化图像，[Length]处理行,；
        //输出：无；
        //返回：Rode_Err失败，Rode_OutRing成功；
        public RodeTypes Guide_CheckOutRing(int16_t Length,int TurnLight)
        {

            //判断标志位入环
            if (OutRingflag == 0) return RodeTypes.Unknown;
            //环左转弯
            if (TurnLight==0)
            {
                int16_t tline = 0;
                int16_t rightdown = 0, upline = 0;
                float rightlop = 0.0f;
                //判断丢边(78,63,50)
                if (Length>57&&RightLine[0] > IMAGE_WIDTH_BAND - 5 && CheckRingLine(78, RightLine, Length) && CheckRingLine(63, RightLine, Length) && CheckRingLine(50, RightLine, Length))
                {
                    int lineceber=0, lined=0;
                    //判断...
                    tline=0;
                    //右边
                    while (tline<30)
                    {
                        if(RightLine[tline]<78)
                        {
                            lined = tline;
                            break;
                        }
                        tline++;
                    }
                    // 总中间值--40
                    while(tline<Length -10)
                    {
                        if(RightLine[tline]>=40&&RightLine[tline+1]<=40)
                        {
                            //判断谁近
                            lineceber = RightLine[tline] - 40 > 40 - RightLine[tline + 1] ? tline : tline + 1;
                            break;
                        }
                        tline++;
                    }
                    //判断斜率越小则要补
                    rightlop = (float)(RightLine[lineceber] - RightLine[lined]) / (lineceber - lined);
                    if (rightlop<-1.0f)
                    {
                        int16_t leftadd = 0;
                        //补线
                        //以右边 RightLine[0]，上 upline  ;
                        rightdown = 0;
                        //找极小值、
                        tline = 0;
                        upline=0;
                        while(tline<Length)
                        {
                            if(RightLine[tline]<RightLine[upline])
                            {
                                upline = tline;
                            }
                            if (RightLine[tline] >= RightLine[upline]+1)
                            {
                                break;
                            }
                            tline++;
                        }
                        //补右边线
                        rightlop = (float)(RightLine[upline] - RightLine[rightdown]) / (upline - rightdown);                    
                        for (int16_t tipexl = rightdown; tipexl < upline; tipexl++)
                        {
                            leftadd = RightLine[rightdown] + (int)((tipexl - rightdown) * rightlop);
                            if (leftadd <= 0) leftadd = 0;
                            if (leftadd >= 79) leftadd = 79;
                            if (leftadd < RightLine[tipexl]) RightLine[tipexl] = leftadd;
                        }
                    }
                    
                    return RodeTypes.OutRing;
                }
                //判断右边向左延有白黑
                tline = Length;
                upline = 0;
                while (tline > 10 )
                {
                    if (RightLine[tline] >=78)
                    {
                        rightdown = tline;
                        break;
                    }
                    tline--;
                }
                if (tline <= 10) return RodeTypes.OutRing;
                for (int tipexl = RightLine[rightdown]; tipexl > 40 && tipexl >= LeftLine[rightdown]; tipexl--)
                {
                     if(imagebuff[rightdown,tipexl]==IMAGE_WHITE&&imagebuff[rightdown,tipexl-1]==IMAGE_BLACK)
                     {
                         upline = tipexl;
                         break;
                     }
                }                 
                if (upline!=0)
                {
                    //向下寻10个有跳变则是
                    tline = rightdown;
                    while (tline >= rightdown-10)
                    {
                        if(upline - LeftLine[tline]>20)
                        {
                            break;
                        }
                        else { upline = LeftLine[tline]; }
                        tline--;
                    }
                    if (tline <= rightdown - 10)
                    {
                        retcond = "没有波谷";
                        return RodeTypes.OutRing;
                    }
                    tline = Length-1;
                    while(tline>0)
                    {
                        if(LeftLine[tline]<2)
                        {
                            Length = tline;
                            LeftLineCnt = tline;
                            break;
                        }
                        tline--;
                    }
                    int16_t rightadd = LeftLine[tline];
                    //从上到下找右边界                   
                    tline = Length;              
                    while (tline > Length - 15&&tline >0)
                    {
                        for (int tipexl = rightadd; tipexl < 79 && tipexl <= RightLine[tline]; tipexl++)// tipexl < rightadd + 30 &&
                        {
                            if (imagebuff[tline, tipexl] == IMAGE_WHITE && imagebuff[tline, tipexl + 1] == IMAGE_BLACK)
                            {
                                RightLine[tline] = tipexl;
                                rightadd = tipexl;
                                rightdown = tline;
                                if (RightLine[rightdown] >= 78)
                                {
                                    if (RightLine[rightdown] + 3 <= RightLine[rightdown + 1])
                                        rightdown = rightdown + 1;
                                    tline = 0;
                                    break;
                                }
                                break;
                            }

                        }
                        
                        tline--;
                    }                  
                }
                //判断是符合
                //补线
                if (rightdown != 0)
                {
                    int16_t rightalt = 0;
                    tline = rightdown;
                    upline = tline;
                    //向上找终点
                    while (tline < Length - 1)
                    {
                        if (RightLine[tline] <= 2)
                        {
                            break;
                        }
                        upline++;
                        tline++;
                    }
                    //补线
                    rightlop = Slope_Calculate(rightdown, upline, RightLine);
                    rightlop = rightlop / 2.2f;
                    for (int tipexl = rightdown; tipexl > 0; tipexl--)
                    {
                        rightalt = RightLine[rightdown+1] + (int16_t)((tipexl - rightdown) * rightlop);
                        
                        if (rightalt <= RightLine[tipexl])
                        {
                            if (rightalt >= 79) rightalt = 79;
                            if (rightalt <= 0) rightalt = 0;
                            RightLine[tipexl] = rightalt;
                        }
                        else
                        {
                            // 判断出环
                            int  Ringflag=0;
                            tline =0;
                            while(tline<5)
                            {
                                //左边
                                if(LeftLine[tline]>2)
                                {
                                    break;
                                }
                                tline++;
                            }
                            if (tline >= 5) { return RodeTypes.OutRing; }
                            
                            //左边有一个跳变点
                            tline = 5;
                            while (tline < 30)
                            {
                                if (RightLine[tline] >= RightLine[tline + 1] + 3)
                                {
                                    if(RightLine[tline+1] >= RightLine[tline + 2] + 3)
                                    {                                     
                                        return RodeTypes.Unknown;
                                    }
                                    else { Ringflag = 1; }
                                }
                                tline++;
                            }
                            if (Ringflag == 1)
                            {
                                rightlop = 0.0f;
                                //求斜率
                                rightlop = (RightLine[tline + 6] - RightLine[tline + 1]) / 5;
                                if(rightlop<-0.20f)
                                {
                                    //出环标志
                                    OutRingflag = 0;
                                }
                            }
                            return RodeTypes.OutRing;
                        }
                    }
                } 
                  

            }
            //环右拐弯
            else 
            {
                int16_t tline = 0;
                int16_t leftdown = 0,upline=0;
                float leftlop = 0.0f;
               //判断丢边(0,15,30)
                if (Length > 57 && LeftLine[0] < 5 && CheckRingLine(0, LeftLine, Length) && CheckRingLine(15, LeftLine, Length) && CheckRingLine(30, LeftLine, Length))
               {
                   int lineceber = 0, lined = 0;
                   //判断...
                   tline = 0;
                   //右边
                   while (tline < 30)
                   {
                       if (LeftLine[tline] >= 2)
                       {
                           lined = tline;
                           break;
                       }
                       tline++;
                   }
                   // 总中间值--40
                   while (tline < Length - 10)
                   {
                       if (LeftLine[tline+1] >= 40 && LeftLine[tline ] <= 40)
                       {
                           //判断谁近
                           lineceber = LeftLine[tline+1] - 40 > 40 - LeftLine[tline] ? tline : tline + 1;
                           break;
                       }
                       tline++;
                   }
                   //判断斜率越小则要补
                   leftlop = (float)(LeftLine[lineceber] - LeftLine[lined]) / (lineceber - lined);
                   if (leftlop > 1.0f)
                   {
                       int16_t rightadd = 0;
                       //补线
                       //以左边 LeftLine[0]，上 upline  ;
                       leftdown = 0;
                       //找极大值、
                       tline = 0;
                       upline = 0;
                       while (tline < Length)
                       {
                           if (LeftLine[tline] > LeftLine[upline])
                           {
                               upline = tline;
                           }
                           if (LeftLine[tline]+1 <= LeftLine[upline])
                           {
                               break;
                           }
                           tline++;
                       }                                                  
                       //补左边线
                       leftlop = (float)(LeftLine[upline] - LeftLine[leftdown]) / (upline - leftdown);                    
                       for (int16_t tipexl = leftdown; tipexl < upline; tipexl++)
                       {
                           rightadd = LeftLine[leftdown] + (int)((tipexl - leftdown) * leftlop);
                           if (rightadd <= 0) rightadd = 0;
                           if (rightadd >= 79) rightadd = 79;
                           if (rightadd > LeftLine[tipexl]) LeftLine[tipexl] = rightadd;
                       }
                   }
                  
                   return RodeTypes.OutRing;
               }
                //判断左边向右延有白黑
                tline = Length;
                upline = 0;
                while (tline > 10)
                {
                    if (LeftLine[tline] <= 2)
                    {
                        leftdown = tline;
                        break;
                    }
                    tline--;
                }
                if (tline <= 10) return RodeTypes.OutRing;
                ///向右
                for (int tipexl = LeftLine[leftdown]; tipexl < 40 && tipexl <= RightLine[leftdown]; tipexl++)
                {
                    if (imagebuff[leftdown, tipexl] == IMAGE_WHITE && imagebuff[leftdown, tipexl + 1] == IMAGE_BLACK)
                    {
                        upline = tipexl;
                        break;
                    }
                }
                if (upline != 0)
                {
                    //向下寻10个有跳变则是
                    tline = leftdown;
                    while (tline >= leftdown - 10)
                    {
                        if ( RightLine[tline] - upline > 20)
                        {
                            break;
                        }
                        else { upline = RightLine[tline]; }
                        tline--;
                    }
                    if (tline <= leftdown - 10)
                    {
                        retcond = "没有波谷";
                        return RodeTypes.OutRing;
                    }
                    tline = Length - 1;
                    while (tline > 0)
                    {
                        if (RightLine[tline] >=78)
                        {
                            Length = tline;
                            RightLineCnt = tline;
                            break;
                        }
                        tline--;
                    }
                    int16_t leftadd = RightLine[tline];
                    //从上到下找左边界                   
                    tline = Length;
                    while (tline > Length - 30 && tline > 0)
                    {
                        for (int tipexl = leftadd; tipexl> 0 && tipexl >= LeftLine[tline]; tipexl--)// tipexl < rightadd + 30 &&
                        {
                            if (imagebuff[tline, tipexl] == IMAGE_WHITE && imagebuff[tline, tipexl - 1] == IMAGE_BLACK)
                            {
                                LeftLine[tline] = tipexl;
                                leftadd = tipexl;
                                leftadd = tline;
                                if (LeftLine[leftdown] <= 2)
                                {
                                    if (LeftLine[leftdown] + 3<= LeftLine[leftdown+1])
                                        leftdown = leftdown + 1;
                                    tline = 0;
                                    break;
                                }
                                break;
                            }

                        }

                        tline--;
                    }
                }
                //判断是符合
                //补线
                if(leftdown != 0)
                {
                    int16_t leftalt = 0;
                    tline=leftdown;
                    upline = tline;
                    //向上找终点
                    while(tline<Length-1)
                    {
                        if(LeftLine[tline]>=78)
                        {
                            break;
                        }
                        upline++;
                        tline++;
                    }
                  //补线
                    leftlop = Slope_Calculate(leftdown, upline, LeftLine);
                    leftlop = leftlop / 2.2f;
                    for (int tipexl = leftdown; tipexl > 0; tipexl--)
                    {
                        leftalt = LeftLine[leftdown+2] + (int16_t)((tipexl - leftdown-2) * leftlop);                     
                        if(leftalt>=LeftLine[tipexl])
                        {
                            if (leftalt >= 79) leftalt = 79;
                            if (leftalt <= 0) leftalt = 0;  
                            LeftLine[tipexl] = leftalt;
                        }                    
                        else
                        {
                            // 判断出环
                            int  Ringflag=0;
                            tline =0;
                            while(tline<5)
                            {
                                //右边
                                if(RightLine[tline]<79-2)
                                {
                                    break;
                                }
                                tline++;
                            }
                            if (tline >= 5) { return RodeTypes.OutRing; }
                            
                            //左边有一个跳变点
                            tline = 5;
                            while (tline < 30)
                            {
                                if (LeftLine[tline+1] >= LeftLine[tline ] + 3)
                                {
                                    if (LeftLine[tline + 2] >= LeftLine[tline + 1] + 3)
                                    {                                    
                                        return RodeTypes.Unknown;
                                    }
                                    else { Ringflag = 1; }
                                }
                                tline++;
                            }
                            if (Ringflag == 1)
                            {
                                leftlop = 0.0f;
                                //求斜率
                                leftlop = (LeftLine[tline + 6] - LeftLine[tline + 1]) / 5;
                                if (leftlop > 0.5f)
                                { 
                                    //出环标志
                                    OutRingflag = 0;
                                }
                            }
                            return RodeTypes.OutRing;                       
                        }                
                    }
                } 
              
             }
            return RodeTypes.OutRing; ;
     
}

    //功能：检查S弯道
    //输入：[Length]处理行；
    //输出：无；
    //返回：Rode_Err失败，Rode_SmallS 小S弯道，Rode_BigS 大S弯道；
    public RodeTypes Guide_CheckS(int16_t Length)
{
    int16_t tline;	
	float leftslope,rightslope;
	float tleftslope,trightslope;
    int16_t flag = 0, leftstart, rightstart, leftend, rightend;
	
	//初始化变量
	
	leftstart=LeftLine[0];  //左边开始
    leftend = LeftLine[Length-2]; //左边结束
	rightstart=RightLine[0];  //右边开始
    rightend = RightLine[Length-2];//右边结束
	
	if(Length<IMAGE_HEIGHT_BAND-5)
	{
		return RodeTypes.Unknown;
	}
	//判断顶部没有跳变（5行）
    //左边 和 右边 宽度
    for (int tipexl = Length-2; tipexl > Length - 15; tipexl--)
    {
        if(LeftLine[tipexl-1] - LeftLine[tipexl] > 3 || LeftLine[tipexl] - LeftLine[tipexl-1] > 3)
        {
            retcond = "S左边有跳变";
            return RodeTypes.Unknown;
        }
        if (RightLine[tipexl - 1] - RightLine[tipexl] > 3 || RightLine[tipexl] - RightLine[tipexl - 1] > 3)
        {
            retcond = "S右边有跳变";
            return RodeTypes.Unknown;
        }
        if(Widths[tipexl] < 15 )
        {
            retcond = "S顶部宽度不符合";
            return RodeTypes.Unknown;
        }
    }
    //计算左右边界斜率
    leftslope = (float)(leftend - leftstart) / (float)(Length-2);
    rightslope = (float)(rightend - rightstart) / (float)(Length-2);//IMAGE_HEIGHT_BAND-Length
    if (leftslope == 0.0f || rightslope==0.0f)
    {
        retcond = "边界斜率为0";
        return RodeTypes.Unknown;
    }
	//计算右边每5个点的斜率
	tleftslope=0.0f;trightslope=0.0f;
	for(tline=2;tline<=Length-2;tline++)
	{
        if (RightLine[tline] - LeftLine[tline] < 15) { LeftLineCnt = tline; return RodeTypes.Unknown; }
        tleftslope = (float)(LeftLine[tline + 2] - LeftLine[tline - 2]) / 5.0f;
		//判断与左右边界斜率是否垂直，垂直则退出
		if(tleftslope*leftslope<0.0f)
		{
			flag+=1;
			break;
		}
	}
	//计算左边每5个点的斜率
	for(tline=2;tline<=Length-2;tline++)
	{
        if (RightLine[tline] - LeftLine[tline] < 15) { LeftLineCnt = tline; return RodeTypes.Unknown; }
        trightslope = (float)(RightLine[tline + 2] - RightLine[tline - 2]) / 5.0f;
		//判断与左右边界斜率是否垂直，垂直则退出
		if(trightslope*rightslope<0.0f)
		{
			flag+=1;
			break;
		}
	}
	//若有垂直斜率判定为S弯
	if(flag>=2)
	{   
        //若为S弯 则把它拉直
        ///左边 （小于ta就不成立）leftslope       
        int boderadd = 0;
        for (int tipexl = 0; tipexl < Length - 2;tipexl++ )
        {
            if (RightLine[tline] - LeftLine[tline] < 15) { LeftLineCnt = tline; return RodeTypes.Unknown; }
            boderadd = LeftLine[0] + (int)(tipexl * leftslope);
            if (boderadd < 0) boderadd = 0;
            if (boderadd > 79) boderadd = 79;
            if (boderadd > LeftLine[tipexl]) LeftLine[tipexl] = boderadd;
        }
        ///右边  （大于ta就不成立)rightslope
        for (int tipexl = 0; tipexl < Length - 2; tipexl++)
        {
            boderadd = RightLine[0] + (int)(tipexl * rightslope);
            if (boderadd < 0) boderadd = 0;
            if (boderadd > 79) boderadd = 79;
            if (boderadd < RightLine[tipexl]) RightLine[tipexl] = boderadd;
        }
        
		return RodeTypes.LittleS;
	}
	return RodeTypes.Unknown;
}
  
    //功能：检查直道；
    //输入：[Length]处理行；
    //输出：无；
    //返回：Rode_Err失败，Rode_Straight成功；
    public RodeTypes Guide_CheckStraight(int16_t Length)
    {
        int tline;      
        int standleft = 0, standright = 0;
        //直道的判定条件之一
        //远瞻2m，
        if (Length < IMAGE_HEIGHT_BAND - 3)
        {
            return RodeTypes.Unknown;
        }
        for (int tipexl = Length - 2; tipexl > Length - 6;tipexl-- )
        {
            if(Widths[tipexl]<15)
            {
                retcond = "顶部宽度不符合";
                return RodeTypes.Unknown;
            }
        }
        standleft = LeftLine[Length - 2]+2;
        standright = RightLine[Length - 2]-2;
        //底部和标准有最小距离
        if (RightLine[0] - standleft < 25 || standright - LeftLine[0]<25)
        {
            retcond = "底部和标准有最小距离不符合";
            return RodeTypes.Unknown;
        }
        //判断直道宽度一直变小        
        for (tline = Length - 3; tline>2; tline--)
        {
            if (LeftLine[tline] > standleft || RightLine[tline] < standright)
            {
                retcond = "跑偏了";
                return RodeTypes.Unknown;
            }
            if (Widths[tline] - Widths[tline + 1] >= 5 || Widths[tline] - Widths[tline+1] <= -5 || Widths[tline] < 15)
            {                                                       
                        retcond = "宽度不符合";
                        return RodeTypes.Unknown;
                    
            }
        }
       
        //判断是否中间有赛道元素   
        return RodeTypes.Straight;
    }

    //    //功能:检查左右弯道
    //    //输入：[Length]处理行；
    //    //输出：无；
    //    //返回：Rode_Err失败，成功：Rode_LeftSide--左,Rode_RightSide--右；
    public RodeTypes Guide_CheckDir(int16_t Length)
    {
        int tline = 0;
        int leftcenter = 0,rightcenter=0;
        int minerr = 0,err=0;
        //左边值靠近 40
        minerr = 40;
        while(tline<=Length-5)
        {
            if (RightLine[tline+1] - LeftLine[tline+1] <= 10) break;
            err = LeftLine[tline] > 40 ? LeftLine[tline] - 40 : 40 - LeftLine[tline];
            if (err < minerr)
            {
                 minerr = err;
                 leftcenter = tline;
            }
            tline++;
        }
        //右边值靠近 40
        minerr = 40;
        tline = 0;
        while (tline <= Length-5 )
        {
            if (RightLine[tline + 1] - LeftLine[tline + 1] <= 10) break;
            err = RightLine[tline] > 40 ? RightLine[tline] - 40 : 40 - RightLine[tline];
            if (err < minerr)
            {
                minerr = err;
                rightcenter = tline;
            }
            tline++;
        }
        ///整体向右
        if (leftcenter > rightcenter)
        {
            float rightlop = 0.0f;
            int   [] Middleline = new int16_t [60];          
            int  pole = 0;
            for(int16_t tipexl = 0;tipexl<=RightLineCnt;tipexl++)
            {
                Middleline[tipexl] = (int16_t)(LeftLine[tipexl]+RightLine[tipexl])/2;
                if(Middleline[tipexl]>79)Middleline[tipexl]=79;  
                if(Middleline[tipexl]<0)Middleline[tipexl]=0;
            }                                                          
            //判断最小值
            tline = 0;
            pole = 0;
            while(tline<Length -10)
            {
                if (Middleline[tline] <= Middleline[pole])
                {
                        pole = tline;
                 }
                tline++;
            }
            rightlop = (float)(Middleline[pole] - Middleline[0]) / 40;
             Middle_K = rightlop;
             if (rightlop < -0.2f)
             {
                 // S弯增益
                 int leftadd = 0;
                 LeftLineCnt = pole;
                 normline = pole;
                 if (RightLine[tline] > 40)
                 {
                     rightlop = rightlop / 1.0f;
                     for (int tipexl = 0; tipexl <= pole; tipexl++)
                     {
                         leftadd = 2 * (Middleline[0] + (int16_t)(tipexl * rightlop)) - RightLine[tipexl];
                         if (leftadd > 79) leftadd = 79;
                         if (leftadd < 0) leftadd = 0;
                         if (leftadd > LeftLine[tipexl]) LeftLine[tipexl] = leftadd;
                     }
                 }            
             }
             else
             {
                 if (rightlop < 0)
                 {
                     int leftadd = 0;
                     // 底边和 40 处的点的斜率拟合
                     tline = pole;
                     while (tline < Length - 10)
                     {
                         if (Middleline[tline] <= 40 && Middleline[tline + 1] >= 40)
                         {
                             pole = 40 - Middleline[tline] < Middleline[tline + 1] - 40 ? tline : tline + 1;
                             break;
                         }
                         tline++;
                     }
                     rightlop = (float)(Middleline[pole] - Middleline[0]) / pole;
                     if (rightlop < 0) rightlop = 0;
                     for (int tipexl = 0; tipexl <= pole; tipexl++)
                     {
                         leftadd = 2 * (Middleline[0] + (int16_t)(tipexl * rightlop)) - RightLine[tipexl];
                         if (leftadd > 79) leftadd = 79;
                         if (leftadd < 0) leftadd = 0;
                         if (leftadd > LeftLine[tipexl]) LeftLine[tipexl] = leftadd;
                     }
                 }
                 else
                 {
                     tline = pole;
                     while (tline < Length - 5)
                     {
                         if (Middleline[tline]  >= Middleline[pole])
                         {
                             pole = tline;
                         }
                         tline++;
                     }
                     tline = pole;
                     while (tline <= Length)
                     {
                         if (Middleline[tline] + 3 <= Middleline[pole] && Middleline[tline] >= 60)//
                         {
                             LeftLineCnt = pole;
                             break;
                         }
                         tline++;
                     }
                 }
             }   
            //丢边判断
             float Average = 0.0f;
             Average = AvgVariance(Middleline, 0, LeftLineCnt);
             Middle_K = Average;
              //补边 
              if(Average > 50.0f)
             { 
                 int lackline = 0;
                 //右边
                 // 从上到下找丢边
                 tline = LeftLineCnt;
                  //先过滤
                  while(tline>0)
                  {
                      if(RightLine[tline]>=78)
                      {
                          break;
                      }
                      tline--;
                  }
                 //找到最低的丢边行
                  while(tline>0)
                  {                    
                      if(RightLine[tline]<=78)
                      {
                          lackline = tline;
                          break;
                      }
                      tline--;
                  }
                  retcond = "以上副图像--右边补线";
                  //用上一副图像行的宽度补线
                  int16_t addline = 0;
                  if (lackline < 10)
                  {
                      float leftlop = 0.0f;
                      leftlop = (float)(LeftLine[LeftLineCnt] - LeftLine[0]) / LeftLineCnt;
                      for (int tipexl = lackline; tipexl <= LeftLineCnt; tipexl++)
                      {
                          addline =LeftLine[0] +  (int)(tipexl*leftlop);
                          if (addline > LeftLine[tipexl])
                          {
                              LeftLine[tipexl] = addline;
                          }
                      }
                  }
                  else
                  {
                      for (int tipexl = lackline; tipexl <= LeftLineCnt; tipexl++)
                      {
                          addline = (int)(MiddLast[tipexl - 10]) + LeftLine[tipexl];
                          if(addline>RightLine[tline])
                          {
                              RightLine[tipexl] = addline;
                          }
                      }
                  }
             }
            return RodeTypes.Right; 
        }
        ///整体向左
        if (rightcenter > leftcenter)
        {
            float leftlop = 0.0f;
            int[] Middleline = new int16_t[60];         
            int pole = 0;
            for (int16_t tipexl = 0; tipexl <= LeftLineCnt; tipexl++)
            {
                Middleline[tipexl] = (int16_t)(LeftLine[tipexl] + RightLine[tipexl]) / 2;
                if (Middleline[tipexl] > 79) Middleline[tipexl] = 79;
                if (Middleline[tipexl] < 0) Middleline[tipexl] = 0;
            }                     
            //2.找极值点          
            //判断最大值
            tline = 0;
            pole = 0;
            while (tline < Length - 10)
            {
                if (Middleline[tline] >= Middleline[pole])
                {
                    pole = tline;
                }
                tline++;
            }          
            leftlop = (float)(Middleline[pole] - Middleline[0]) / 40;
            Middle_K = leftlop;
            //把有效值变小
            if (leftlop > 0.2f)
            {
                // S弯增益
                int rightadd = 0;
                LeftLineCnt = pole;
                if (LeftLine[tline] < 40)
                {
                    leftlop = leftlop / 1.0f;
                    for (int tipexl = 0; tipexl <= pole; tipexl++)
                    {
                        rightadd = 2 * (Middleline[0] + (int16_t)(tipexl * leftlop)) - LeftLine[tipexl];
                        if (rightadd > 79) rightadd = 79;
                        if (rightadd < 0) rightadd = 0;
                        if (rightadd < RightLine[tipexl]) RightLine[tipexl] = rightadd;
                    }
                }
                normline = pole;
            ///    return RodeTypes.Left;
            }
            else
            {
                if (leftlop > 0)
                {
                    int rightadd = 0;
                    // 底边和 40 处的点的斜率拟合
                    tline = pole;
                    while (tline < Length - 10)
                    {
                        if (Middleline[tline] >= 39 && Middleline[tline + 1] <= 39)
                        {
                            pole = Middleline[tline] - 40 < 40 - Middleline[tline + 1] ? tline : tline + 1;
                            pole = tline + 1;
                            break;
                        }
                        tline++;
                    }
                    leftlop = (float)(Middleline[pole] - Middleline[0]) / pole;
                    if (leftlop > 0) leftlop = 0;
                    for (int tipexl = 0; tipexl <= pole; tipexl++)
                    {
                        rightadd = 2 * (Middleline[0] + (int16_t)(tipexl * leftlop)) - LeftLine[tipexl];
                        if (rightadd > 79) rightadd = 79;
                        if (rightadd < 0) rightadd = 0;
                        if (rightadd < RightLine[tipexl]) RightLine[tipexl] = rightadd;
                    }
                }
                else 
                {
                    tline = pole;
                    //极小
                    while(tline<Length - 5)
                    {
                        if (Middleline[tline] <= Middleline[pole])
                        {
                            pole = tline;
                        }
                        tline++;
                    }
                    tline = pole;
                    while(tline <= Length)
                    {
                        if (Middleline[tline] >= Middleline[pole] + 3 && Middleline[tline] <= 20)
                        {
                            RightLineCnt = pole;
                            break;
                        }
                        tline++;
                    }

                }
            }
            //丢边判断
            float Average = 0.0f;
            Average = AvgVariance(Middleline, 0,RightLineCnt);
            Middle_K = Average;
            //补边 
            if (Average > 50.0f)
            {
                int lackline = 0;
                //左边
                // 从上到下找丢边
                tline = RightLineCnt;
                //先过滤
                while (tline > 0)
                {
                    if (LeftLine[tline] <= 2)
                    {
                        break;
                    }
                    tline--;
                }
                //找到最低的丢边行
                while (tline > 0)
                {
                    if (LeftLine[tline] >=2)
                    {
                        lackline = tline;
                        break;
                    }
                    tline--;
                }
                retcond = "以上副图像--右边补线";
                //用上一副图像行的宽度补线
                int16_t addline = 0;
                if (lackline < 10)
                {
                    float rightlop = 0.0f;
                    rightlop = (float)(RightLine[RightLineCnt] - RightLine[0]) / RightLineCnt;
                    for (int tipexl = lackline; tipexl <= RightLineCnt; tipexl++)
                    {
                        addline = RightLine[0] + (int)(tipexl * rightlop);
                        if (addline < RightLine[tipexl])
                        {
                            RightLine[tipexl] = addline;
                        }
                    }
                }
                else
                {
                    for (int tipexl = lackline; tipexl <= RightLineCnt; tipexl++)
                    {
                        addline =  RightLine[tipexl] - (int)(MiddLast[tipexl - 10]) ;
                        if (addline < LeftLine[tline])
                        {
                            LeftLine[tipexl] = addline;
                        }
                    }
                }
            }
            return RodeTypes.Left; 
        }

        return RodeTypes.Unknown;
     

    }


 }  //end
}

          
