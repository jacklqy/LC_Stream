using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WinformIOAndExcel
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string folderPath = fbd.SelectedPath;//文件夹路径
                string[] folders=  Directory.GetDirectories(folderPath);//子目录列表
                listBox1.Items.Clear();
                foreach (string path in folders)
                {
                    DirectoryInfo di = new DirectoryInfo(path);
                    listBox1.Items.Add(di.FullName);
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string folderPath = listBox1.SelectedItem.ToString();//选择文件夹的路径
            DirectoryInfo di = new DirectoryInfo(folderPath);
            FileInfo[] files=  di.GetFiles();//获取指定文件夹下的文件列表
            listBox2.Items.Clear();
            foreach (FileInfo f in files)
            {
                listBox2.Items.Add(f.Name);
            }
        }
        /// <summary>
        /// 遍历图片文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            string path = @"D:\工作\工作文档\壁纸";
            string fileName = "05.jpg";
            string fileNameNew = Path.Combine(path, fileName);//D:\工作\工作文档\壁纸\05.jpg
            if (Directory.Exists(path))//判断目录存在
            {
                string[] files = Directory.GetFiles(path);//获取文件列表
                int i = 0;
                foreach (string fPath in files)
                {
                    string ext = Path.GetExtension(fPath).ToLower();//扩展名
                    if(ext ==".jpg"||ext ==".png")
                    {
                        Image img = Image.FromFile(fPath);//加载图片
                        imageList1.Images.Add(img);
                        ListViewItem li = new ListViewItem();
                        li.Text = Path.GetFileName(fPath);
                        li.ImageIndex = i;
                        listView1.Items.Add(li);
                        i++;
                    }
                }
            }
        }
    }
}
