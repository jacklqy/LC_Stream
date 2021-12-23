using Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinformIOAndExcel.DAL;

namespace WinformIOAndExcel
{
    public partial class FrmExcel : Form
    {
        public FrmExcel()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string path = @"D:\NewImgs\Student信息.xls";
            DataTable dt = ExcelHelper.AppExcelToDataTable(path, "Sheet2");
            dataGridView1.DataSource = dt;
        }

        //Oledb导入
        private void button2_Click(object sender, EventArgs e)
        {
            //string path = @"D:\NewImgs\Student信息.xls";
             string path = @"D:\NewImgs\区域数据.xls";
            DataTable dt = ExcelHelper.OledbExcelToDataTable(path,true);
            dataGridView1.DataSource = dt;


        }

        //NPOI导入
        private void button3_Click(object sender, EventArgs e)
        {
            //string path = @"D:\NewImgs\Student信息.xls";
            string path = @"D:\NewImgs\区域数据.xls";
            DataTable dt = ExcelHelper.NPOIExcelToDataTable(path,"Sheet1", true);
            dataGridView1.DataSource = dt;

        }

        /// <summary>
        /// 保存到数据库 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            //批量   循环Insert  ----事务
            DataTable dt = dataGridView1.DataSource as DataTable;
            SaveStudents(dt);
        }

        /// <summary>
        /// 将dt中的数据直接保存到数据库
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private bool SaveStudents(DataTable dt)
        {
            return SqlHelper.ExecuteTrans<bool>(cmd =>
            {
                try
                {
                    foreach (DataRow  dr in dt.Rows)
                    {
                        string className = dr["班级"].ToString().Trim();
                        string gradeName= dr["年级"].ToString().Trim();
                        cmd.CommandText = $"select ClassId from ClassInfos where ClassName='{className}' and GradeId=(select GradeId from GradeInfos where GradeName='{gradeName}')";
                        object oClassId = cmd.ExecuteScalar();
                        int classId = 0;
                        if (oClassId != null)
                            classId = (int)oClassId;
                        if(classId==0)
                        {
                            cmd.CommandText = $"select GradeId from GradeInfos where GradeName='{gradeName}'";
                            int gradeId = 0;
                            object oGradeId = cmd.ExecuteScalar();
                            if (oGradeId != null)
                                gradeId = (int)oGradeId;
                            if(gradeId>0)
                            {
                                string inclasssql = $"insert  into ClassInfos (ClassName,GradeId) values('{className}',{gradeId});select @@identity";
                                cmd.CommandText = inclasssql;
                                oClassId = cmd.ExecuteScalar();
                                if (oClassId != null)
                                    classId = (int)oClassId;
                            }
                          
                        }
                        cmd.CommandText = $"insert  into StudentInfos2 (StuName,ClassId,Sex,Phone) values('{dr["姓名"].ToString().Trim()}',{classId},'{dr["性别"].ToString().Trim()}','{dr["电话"].ToString().Trim()}')";
                        cmd.ExecuteNonQuery();
                    }
                    cmd.Transaction.Commit();
                    return true;
                }
                catch(Exception ex)
                {
                    cmd.Transaction.Rollback();
                    throw ex;
                }
            });
        }

        //导出
        private void button5_Click(object sender, EventArgs e)
        {
            DataTable dt = dataGridView1.DataSource as DataTable;
            string path = @"D:\NewImgs\Student信息22.xls";

            // ExcelHelper.SWDataTableToExcel(dt, path);

            ExcelHelper.NPOIDataTableToExcel(dt, path, "Students");
        }

        /// <summary>
        /// 将数据库中的数据加载到Dgv
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            StudentDAL stuDAL = new StudentDAL();
            List<StudentInfo> stuList = stuDAL.GetAllStudents();
            dataGridView1.DataSource = stuList;
            dataGridView1.Columns[0].HeaderText = "学号";
            dataGridView1.Columns[1].HeaderText = "姓名";
            dataGridView1.Columns[2].HeaderText = "班级编号";
            dataGridView1.Columns[3].HeaderText = "性别";
            dataGridView1.Columns[4].HeaderText = "电话";
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> dicList = new Dictionary<string, string>();
            foreach (DataGridViewColumn c in dataGridView1.Columns)
            {
                dicList.Add(c.Name, c.HeaderText);
            }
            List<StudentInfo> list = dataGridView1.DataSource as List<StudentInfo>;
            string path = @"D:\NewImgs\Student信息33.xls";
            ExcelHelper.ListToExcel(list, path, "学生列表", dicList);
        }
    }
}
