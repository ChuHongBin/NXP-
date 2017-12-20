using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using CameraSimulation.Properties;
using System.IO;
using CameraSimulation;
using System.Runtime.InteropServices;

namespace Camerasimulation
{
    public partial class FormMain : Form
    {
        /// <summary>
        /// 图片真实尺寸
        /// </summary>
        Size RealSize = new Size(80, 60);

        /// <summary>
        /// 图片数据集合
        /// </summary>
        List<Bitmap> CarImages = new List<Bitmap>();
        /// <summary>
        /// 当前图片的位置
        /// </summary>
        int curCarImageIndex = -1;
        /// <summary>
        /// 图层显示颜色
        /// </summary>
        struct CarImageColor
        {
            /// <summary>
            /// 底层
            /// </summary>
            public Color Back;
            /// <summary>
            /// 顶层
            /// </summary>
            public Color Fore;
            /// <summary>
            /// 法线
            /// </summary>
            public Color NormalLine;
            /// <summary>
            /// 左边线
            /// </summary>
            public Color LeftLine;
            /// <summary>
            /// 右边线
            /// </summary>
            public Color RightLine;
            /// <summary>
            /// 拟合中线
            /// </summary>
            public Color MiddleLine;
        };
        /// <summary>
        /// 显示图片颜色
        /// </summary>
        CarImageColor ShowImageColor;

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            //允许图片控件拖放
            pictureBox_Preview.AllowDrop = true;

            //显示选项复选框全部勾
            for (int n = 0; n < checkedListBox_ShownOptions.Items.Count; n++)
            {
                checkedListBox_ShownOptions.SetItemChecked(n, true);
            }

            //初始化显示颜色
            ShowImageColor.Back = Color.White;
            ShowImageColor.Fore = Color.Black;
            ShowImageColor.LeftLine = Color.Yellow;
            ShowImageColor.RightLine = Color.Yellow;
            ShowImageColor.MiddleLine = Color.Red;
            ShowImageColor.NormalLine = Color.Green;
        }

        private void FormMain_Layout(object sender, LayoutEventArgs e)
        {

        }

        private void FormMain_Shown(object sender, EventArgs e)
        {

        }

        private void FormMain_Activated(object sender, EventArgs e)
        {

        }

        private void FormMain_Resize(object sender, EventArgs e)
        {

        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void ToolStripMenu_Open_Click(object sender, EventArgs e)
        {
            openFileDialog.Title = "打开摄像头文件";
            openFileDialog.FileName = "";
            openFileDialog.Filter = "二进制文件|*.bin|图片文件|*.bmp;*.jpg;*.jepg;*.png";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (Path.GetExtension(openFileDialog.FileName).ToLower() == ".bin")
                {
                    OpenBin(openFileDialog.FileName);
                }
                else
                {
                    if (OpenImage(openFileDialog.FileName) == false)
                    {
                        MessageBox.Show("Error File");
                    }
                }
            }
        }

        private void ToolStripMenu_Clear_Click(object sender, EventArgs e)
        {
            //清空图片
            CarImages.Clear();
            //清空列表框
            listBox_ImageList.Items.Clear();
            //显示空图片
            pictureBox_Preview.Image = null;
        }

        private void ToolStripMenuItem_SaveAll_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.ShowNewFolderButton = true;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                StripProgressBar.Minimum = 0;
                StripProgressBar.Maximum = CarImages.Count;
                StripProgressBar.Value = 0;

                for (int n = 0; n < CarImages.Count; n++)
                {
                    CarImages[n].Save(folderBrowserDialog.SelectedPath + "\\" + (n + 1).ToString() + ".bmp", ImageFormat.Bmp);
                    StripProgressBar.Value = n;
                    Application.DoEvents();
                }

                StripProgressBar.Value = 0;
            }
        }

        private void ToolStripMenu_Exit_Click(object sender, EventArgs e)
        {
            //退出所有线程
            //this.Close();
            System.Environment.Exit(0);
        }

        private void ToolStrip_Verion_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Author\t" + Application.CompanyName + "\n" +
                "Verison\t" + Application.ProductVersion, 
                "版本说明");
        }

        private void ToolStripMenu_Help_Click(object sender, EventArgs e)
        {
            MessageBox.Show("请咨询作者", "帮助");
        }

        private void button_Play_Click(object sender, EventArgs e)
        {
            if (button_Play.Text == ResourcesControlText.Play)
            {
                button_Play.Text = ResourcesControlText.Stop;

                timer_Play.Interval = 6;
                timer_Play.Enabled = true;
            }
            else
            {
                button_Play.Text = ResourcesControlText.Play;

                timer_Play.Enabled = false;
            }
        }

        private void button_PreOne_Click(object sender, EventArgs e)
        {
            if (listBox_ImageList.SelectedIndex > 0)
            {
                ShowImage(listBox_ImageList.SelectedIndex - 1);
            }
        }

        private void button_NextOne_Click(object sender, EventArgs e)
        {
            if (listBox_ImageList.SelectedIndex < CarImages.Count - 1)
            {
                ShowImage(listBox_ImageList.SelectedIndex + 1);
            }
        }

        private void button_Save_Click(object sender, EventArgs e)
        {
            saveFileDialog.Title = "保存图片";
            saveFileDialog.Filter = "图片文件|*.bmp|二进制文件|*.bin";
            saveFileDialog.FileName = (listBox_ImageList.SelectedIndex + 1).ToString();
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog.FilterIndex == 1)
                {
                    CarImages[listBox_ImageList.SelectedIndex].Save(saveFileDialog.FileName, ImageFormat.Bmp);
                }
                else
                {
                    FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create);
                    BinaryWriter bw = new BinaryWriter(fs);
                    byte[] Byte2Write = new byte[1021];
                    Bitmap tempImage = (Bitmap)CarImages[listBox_ImageList.SelectedIndex].Clone();
                    Image2Data(tempImage).CopyTo(Byte2Write, 0);
                    for(int n = 0; n < 600; n++)
                    {
                        Byte2Write[n] = (byte)(~Byte2Write[n]);
                    }
                    bw.Write((byte)0xff);
                    bw.Write((byte)0xfe);
                    bw.Write((byte)0xff);
                    bw.Write(Byte2Write);
                    bw.Close();
                    fs.Close();
                }
            }
        }

        private void checkedListBox_ShownOptions_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowImage(curCarImageIndex);
        }

        private void listBox_ImageList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowImage(listBox_ImageList.SelectedIndex);
        }

        private void pictureBox_Preview_MouseEnter(object sender, EventArgs e)
        {

        }

        private void pictureBox_Preview_MouseMove(object sender, MouseEventArgs e)
        {
            //鼠标在PictureBox中的坐标
            Point CursorPos = pictureBox_Preview.PointToClient(MousePosition);

            //转换为图片分辨率下的坐标
            CursorPos.X = (int)(CursorPos.X * RealSize.Width / pictureBox_Preview.Width);
            CursorPos.Y = RealSize.Height - 1 - (int)(CursorPos.Y * RealSize.Height / pictureBox_Preview.Height);

            //显示坐标信息
            StripStatusLabel_StandPoint.Text = "标准坐标:" + CursorPos.ToString();
        }

        private void pictureBox_Preview_Click(object sender, EventArgs e)
        {

        }

        private Bitmap ImageHandle(Bitmap srcImage)
        {
            Bitmap newImage;
            newImage = srcImage.Clone(new Rectangle(0, 0, srcImage.Width, srcImage.Height), PixelFormat.Format1bppIndexed);

            return newImage;

        }

        private void pictureBox_Preview_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Link;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void pictureBox_Preview_DragDrop(object sender, DragEventArgs e)
        {
            //获取文件路径
            string filename = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString().ToLower();

            //打开图片
            if (Path.GetExtension(filename) == ".bin")
            {
                OpenBin(filename);
            }
            else
            {
                if (OpenImage(filename) == false)
                {
                    MessageBox.Show("Error File");
                }
            }
        }

        private void pictureBox_Preview_Paint(object sender, PaintEventArgs e)
        {
            //显示网格
            if (checkedListBox_ShownOptions.GetItemChecked(2) == true)
            {
                Graphics g = e.Graphics;
                for (int x = 1; x < RealSize.Width; x++)
                {
                    g.DrawLine(
                        new Pen(Brushes.Black),
                        new Point(x * pictureBox_Preview.Width / RealSize.Width, 0),
                        new Point(x * pictureBox_Preview.Width / RealSize.Width, pictureBox_Preview.Height));
                }
                for (int y = 1; y < RealSize.Width; y++)
                {
                    g.DrawLine(
                        new Pen(Brushes.Black),
                        new Point(0, y * pictureBox_Preview.Height / RealSize.Height),
                        new Point(pictureBox_Preview.Width, y * pictureBox_Preview.Height / RealSize.Height));
                }
            }
            //显示法线
            if (checkedListBox_ShownOptions.GetItemChecked(3) == true)
            {
                Graphics g = e.Graphics;
                g.DrawLine(
                    new Pen(ShowImageColor.NormalLine),
                    new Point(pictureBox_Preview.Width / 2, 0),
                    new Point(pictureBox_Preview.Width / 2, pictureBox_Preview.Height));
                if (pictureBox_Preview.Width % 2 == 0)
                {
                    g.DrawLine(
                        new Pen(ShowImageColor.NormalLine),
                        new Point(pictureBox_Preview.Width / 2 - 1, 0),
                        new Point(pictureBox_Preview.Width / 2 - 1, pictureBox_Preview.Height));
                }
            }

        }

        /// <summary>
        /// 图片列表添加一个新图片
        /// </summary>
        /// <param name="newImage">图片文件</param>
        private void AddNewImage(Bitmap newImage)
        {
            //添加图片
            CarImages.Add(newImage);
            //更新列表
            listBox_ImageList.Items.Add(CarImages.Count);
        }

        /// <summary>
        /// 显示图片
        /// </summary>
        /// <param name="Index"></param>
        private void ShowImage(int Index)
        {
            //当无图片则返回
            if (CarImages.Count == 0)
            {
                return;
            }

            //笔刷
            Graphics g;

            //载入图片
            Bitmap tempImage = new Bitmap(CarImages[Index]);

            //载入处理图片数据
            ImageProc tempImageProc;
            if (checkedListBox_ShownOptions.GetItemChecked(7))
            {
                tempImageProc = new ImageProc(tempImage);
            }
            else
            {
                tempImageProc = new ImageProc();
            }

            //显示底图
            if (checkedListBox_ShownOptions.GetItemChecked(0) == false)
            {
                g = Graphics.FromImage(tempImage);
                g.Clear(ShowImageColor.Back);
            }
            //显示左边线
            if (checkedListBox_ShownOptions.GetItemChecked(4) == true)
            {
                if (tempImageProc.LeftLine != null)
                {
                    for (int n = 0; n < tempImageProc.LeftLineCnt; n++)
                    {
                        tempImage.SetPixel(
                            tempImageProc.LeftLine[n], RealSize.Height - 1 - n,
                            ShowImageColor.LeftLine);
                    }
                }
            }
            //显示右边线
            if (checkedListBox_ShownOptions.GetItemChecked(5) == true)
            {
                if (tempImageProc.RightLine != null)
                {
                    for (int n = 0; n < tempImageProc.RightLineCnt; n++)
                    {
                        tempImage.SetPixel(
                            tempImageProc.RightLine[n], RealSize.Height - 1 - n,
                            ShowImageColor.RightLine);
                    }
                }
            }
            //显示中线
            if (checkedListBox_ShownOptions.GetItemChecked(6) == true)
            {
                if (tempImageProc.MiddleLine != null)
                {
                    for (int n = 0; n < tempImageProc.MiddleLineCnt; n++)
                    {
                        tempImage.SetPixel(
                            tempImageProc.MiddleLine[n], RealSize.Height - 1 - n,
                            ShowImageColor.MiddleLine);
                    }
                }
            }

            //显示图片矢量放大
            Bitmap ImageToShow = new Bitmap(
                pictureBox_Preview.Width,
                pictureBox_Preview.Height);
            g = Graphics.FromImage(ImageToShow);
            //显示图片对应捕获图片一个像素尺寸
            Size PixelSize = new Size(
                pictureBox_Preview.Width / RealSize.Width,
                pictureBox_Preview.Height / RealSize.Height);
            //填充色块
            for (int y = 0; y < RealSize.Height; y++)
            {
                for (int x = 0; x < RealSize.Width; x++)
                {
                    g.FillRectangle(
                        new SolidBrush(tempImage.GetPixel(x, y)),
                        new Rectangle(
                            new Point(x * PixelSize.Width, y * PixelSize.Height),
                            PixelSize));
                }
            }

            label_CarRodeType.Text = "赛道类型：" + tempImageProc.RodeType.ToString();

            //更新序列
            curCarImageIndex = Index;
            //更新显示图片
            pictureBox_Preview.Image = ImageToShow;
            //更新列表
            listBox_ImageList.SelectedIndex = curCarImageIndex;

            //内存回收
            GC.Collect();
        }

        /// <summary>
        /// 打开图片或采集信息文件
        /// </summary>
        /// <param name="filename">文件路径</param>
        /// <returns></returns>
        private bool OpenImage(string filename)
        {
            //返回值
            bool ret = false;

            //判断文件类型
            switch (Path.GetExtension(filename).ToLower())
            {
                case ".bmp":
                case ".jpg":
                case ".jepg":
                case ".png":
                    //图片列表添加新图片
                    Bitmap tempImage = new Bitmap(filename);
                    tempImage = (Bitmap)tempImage.Clone();
                    AddNewImage(tempImage);
                    
                    ret = true;
                    break;
                default:
                    ret = false;
                    break;
            }

            //图片正确显示添加的最后一张图片
            if (ret == true)
            {
                ShowImage(CarImages.Count - 1);
            }

            return ret;
        }

        private void OpenBin(string filename)
        {
            ToolStripMenu_Clear_Click(null, null);

            FileStream fs = new FileStream(filename, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            byte[] buff = new byte[1024];
            byte[] data = new byte[600];

            StripProgressBar.Maximum = (int)(fs.Length / buff.Length);
            for (long n = 0; n < fs.Length / buff.Length; n++)
            {
                buff = br.ReadBytes(buff.Length);
                for(int t = 0; t < 600; t++)
                {
                    data[t] = (byte)(~buff[t + 3]);
                }
                AddNewImage((Bitmap)Data2Image(data).Clone());
                StripProgressBar.Value = (int)n;
            }
            StripProgressBar.Value = 0;
            br.Close();
            fs.Close();
            ShowImage(0);
        }

        private void timer_Play_Tick(object sender, EventArgs e)
        {
            if (curCarImageIndex < CarImages.Count - 1)
            {
                ShowImage(++curCarImageIndex);
            }
            else
            {
                timer_Play.Enabled = false;
                button_Play.Text = ResourcesControlText.Play;
            }
        }
        public byte[] Image2Data(Bitmap srcImage)
        {
            byte[] ImageDataLine;
            BitmapData ImageData = srcImage.LockBits(
                new Rectangle(0, 0, srcImage.Width, srcImage.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format1bppIndexed);
            IntPtr ImagePtr = ImageData.Scan0;
            int ImageDataLength = ImageData.Stride * ImageData.Height;
            byte[] ImageByteData = new byte[ImageDataLength];
            Marshal.Copy(ImagePtr, ImageByteData, 0, ImageDataLength);
            //一维图像数据初始化赋值
            ImageDataLine = new byte[ImageData.Height * ImageData.Width / 8];
            for (int y = 0; y < ImageData.Height; y++)
            {
                for (int x = 0; x < ImageData.Width / 8; x++)
                {
                    ImageDataLine[y * ImageData.Width / 8 + x] =
                        ImageByteData[y * ImageData.Stride + x];
                }
            }
            srcImage.UnlockBits(ImageData);

            return ImageDataLine;
        }
        private Bitmap Data2Image(byte[] srcData)
        {
            Bitmap dstImage = new Bitmap(RealSize.Width, RealSize.Height);
            //锁定图片数据
            BitmapData ImageData = dstImage.LockBits(
                new Rectangle(0, 0, dstImage.Width, dstImage.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);
            //图片数据头位置
            IntPtr ImagePtr = ImageData.Scan0;
            //数据长度
            int ImageDataLength = ImageData.Stride * ImageData.Height;

            byte[] dstData = new byte[ImageDataLength];
            for (int y = 0; y < ImageData.Height; y++)
            {
                for (int x = 0; x < ImageData.Width / 8; x++)
                {
                    dstData[y * ImageData.Stride + x] = srcData[y * ImageData.Width / 8 + x];
                }
            }
            //写入图片字节数据到缓冲区
            Marshal.Copy(dstData, 0, ImagePtr, ImageDataLength);
            //解锁图片数据
            dstImage.UnlockBits(ImageData);

            return dstImage;
        }

    }
}
