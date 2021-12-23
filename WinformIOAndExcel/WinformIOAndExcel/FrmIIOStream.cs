using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinformIOAndExcel
{
    public partial class FrmIIOStream : Form
    {
        public FrmIIOStream()
        {
            InitializeComponent();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            string path = @"D:\NewImgs\Graphics绘图介绍.txt";
            string pathto = @"D:\NewImgs02\Graphics绘图介绍1.txt"; //StreamReader和StreamWriter没有问题 （在流上读写字符串）
            //string path = @"D:\NewImgs\c#基础.pptx";
            //string pathto = @"D:\NewImgs02\c#基础11.pptx";  //StreamReader和StreamWriter不行
            //string path = @"D:\NewImgs\37 Chart控件介绍-1.mp4";
            //string pathto = @"D:\NewImgs02\37 Chart控件介绍-11.mp4";//StreamReader和StreamWriter不行
            //string path = @"D:\NewImgs\aaaa.jpg";
            //string pathto = @"D:\NewImgs02\aaaa1.jpg"; //StreamReader和StreamWriter不行
            char[] data = new char[1024];
            using (StreamReader dr = new StreamReader(path))
            {
                using (StreamWriter sw = new StreamWriter(pathto))
                {
                    while (true)
                    {
                        int rcount = dr.Read(data, 0, data.Length);
                        if (rcount == 0) break;
                        sw.Write(data, 0, rcount);
                    }

                }
            }
        }

        string path = @"D:\NewImgs\37 Chart控件介绍-1.mp4";
        string pathto = @"D:\NewImgs02\37 Chart控件介绍-11.mp4";
        int size = 1024 * 1024 * 5;
        byte[] bytes;
        FileStream fr;
        FileStream fw;
        /// <summary>
        /// 文件流读写
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            string path = @"D:\NewImgs\37 Chart控件介绍-1.mp4";
            string pathto = @"D:\NewImgs02\37 Chart控件介绍-11.mp4";
            bytes = new byte[size];
            using(fr=new FileStream(path,FileMode.Open,FileAccess.Read))
            {
                using(fw=new FileStream(pathto,FileMode.Create,FileAccess.Write))
                {
                    int count = fr.Read(bytes, 0, size);
                    while(count>0)
                    {
                        fw.Write(bytes, 0, count);
                        count = fr.Read(bytes, 0, size);
                    }
                }
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// MemoryStream 内存流   中转  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            string path = @"D:\NewImgs\37 Chart控件介绍-1.mp4";
            string pathto = @"D:\NewImgs02\37 Chart控件介绍-11.mp4";
            bytes = new byte[size];
            MemoryStream ms = new MemoryStream(size);
            using (fr = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                int count = fr.Read(bytes, 0, size);
                while (count > 0)
                {
                    ms.Write(bytes, 0, count);
                    count = fr.Read(bytes, 0, size);
                }
            }
            //写入流
            using(fw=new FileStream(pathto,FileMode.Create,FileAccess.Write))
            {
                ms.WriteTo(fw);
            }
        }

        /// <summary>
        /// 缓冲流读写文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            string path = @"D:\NewImgs\37 Chart控件介绍-1.mp4";
            string pathto = @"D:\NewImgs02\37 Chart控件介绍-11.mp4";
            bytes = new byte[size];
            using (fr = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                BufferedStream br = new BufferedStream(fr);
                using (fw = new FileStream(pathto, FileMode.Create, FileAccess.Write))
                {
                    BufferedStream bw = new BufferedStream(fw);
                    int count = br.Read(bytes, 0, size);
                    while (count > 0)
                    {
                        bw.Write(bytes, 0, count);
                        count = br.Read(bytes, 0, size);
                    }
                    bw.Close();
                }
                br.Close();
            }

        }

        /// <summary>
        /// 二进制读写器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            string path = @"D:\NewImgs\37 Chart控件介绍-1.mp4";
            string pathto = @"D:\NewImgs02\37 Chart控件介绍-11.mp4";
            bytes = new byte[size];
            fr = new FileStream(path, FileMode.Open, FileAccess.Read);
            fw = new FileStream(pathto, FileMode.Create, FileAccess.Write);
            using(BinaryReader br=new BinaryReader(fr))
            {
                using(BinaryWriter bw=new BinaryWriter(fw))
                {
                    int count = br.Read(bytes, 0, size);
                    while (count > 0)
                    {
                        bw.Write(bytes, 0, count);
                        count = br.Read(bytes, 0, size);
                    }
                }
            }
            fr.Close();
            fw.Close();
        }

        /// <summary>
        /// 异步复制大文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void button6_Click(object sender, EventArgs e)
        {
            await ReadFileAsync02(path);
        }
        /// <summary>
        /// 推荐使用异步读取
        /// </summary>
        /// <param name="pathFName"></param>
        /// <returns></returns>
        private async Task ReadFileAsync02(string pathFName)
        {
            Console.WriteLine($"异步读:{Thread.CurrentThread.ManagedThreadId}");
            bytes = new byte[size];
            using (fr = new FileStream(pathFName, FileMode.Open, FileAccess.Read, FileShare.Read, size, true))
            {
                using (fw = new FileStream(pathto, FileMode.Append, FileAccess.Write, FileShare.None, size, true))
                {
                    int count = 0;
                    count = await fr.ReadAsync(bytes, 0, bytes.Length);
                    while (count != 0)
                    {
                        await fw.WriteAsync(bytes, 0, count);
                        count = await fr.ReadAsync(bytes, 0, bytes.Length);
                    }
                }
            }
            Console.WriteLine($"异步复制完成！");
        }

    }
}
