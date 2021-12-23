using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ApExcel = Microsoft.Office.Interop.Excel;

namespace WinformIOAndExcel
{
    public  class ExcelHelper
    {
        /// <summary>
        /// Office组件 导入Excel数据到DataTable中
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public static DataTable AppExcelToDataTable(string fileName, string sheetName)
        {
            DataTable dt = new DataTable();
            ApExcel.Application exApp = new ApExcel.Application();
            ApExcel.Workbooks wbs = exApp.Workbooks;//Excel工作簿集合
            ApExcel._Workbook _wbk = null;//_Workbook--一个Excel工作簿文件
            try
            {
                //打开一个已有Excel文件
                _wbk = wbs.Add(fileName);
                ApExcel.Worksheet sheet = null;
                if (!string.IsNullOrEmpty(sheetName))
                    sheet = _wbk.Sheets[sheetName];
                else
                    sheet = _wbk.Sheets["sheet1"];
                int rcount = sheet.UsedRange.Rows.Count;//行数
                int colcount = sheet.UsedRange.Columns.Count;//列数

                //获取列  Excel工作表的第一行为列名 索引从1开始
                for (int i = 1; i <= colcount; i++)
                {
                    dt.Columns.Add(((ApExcel.Range)sheet.Cells[1, i]).Value);//添加列
                }
                for (int i = 2; i <= rcount; i++)//数据从第二行开始
                {
                    DataRow dr = dt.NewRow();
                    for (int j = 1; j <= colcount; j++)
                    {
                        //dt的索引从0开始
                        dr[j - 1] = ((ApExcel.Range)sheet.Cells[i, j]).Value;
                    }
                    dt.Rows.Add(dr);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally

            {
                _wbk.Close();
                wbs.Close();
                exApp.Quit();
            }
            return dt;
        }

        /// <summary>
        /// Office组件 将DataTable导出到Excel
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="saveFilePath"></param>
        /// <param name="sheetName"></param>
        public static void AppDataTableToExcel(DataTable dt, string saveFilePath, string sheetName)
        {
            ApExcel.Application exApp = new ApExcel.Application();
            ApExcel.Workbooks wbs = exApp.Workbooks;//Excel工作簿集合
            ApExcel._Workbook _wbk = null;//_Workbook--一个Excel工作簿文件
            try
            {
                //新建一个工作簿 就是新建一个Excel文件
                _wbk = wbs.Add(true);
                //取得第一个工作表
                ApExcel.Worksheet sheet = _wbk.Sheets[1];
                if (!string.IsNullOrEmpty(sheetName))
                    sheet.Name = sheetName;
                else
                    sheet.Name = "sheet1";
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    ApExcel.Range r = sheet.Cells[1, i + 1];
                    r.Value = dt.Columns[i].ColumnName;
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        ApExcel.Range r = sheet.Cells[i + 2, j + 1];

                        r.Value = dt.Rows[i][j].ToString();
                    }
                }
                exApp.DisplayAlerts = false;
                _wbk.Saved = true;
                _wbk.SaveCopyAs(saveFilePath);
                MessageBox.Show("导出完毕！");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _wbk.Close();
                wbs.Close();
                exApp.Quit();
            }
        }

        /// <summary>
        /// Oledb提供程序将Excel加载到DataTable中
        /// </summary>
        /// <param name="excelPath"></param>
        /// <returns></returns>
        public static DataTable OledbExcelToDataTable(string excelPath,bool hasColumnName)
        {
            string isYes = "YES";
            isYes = hasColumnName ? "YES" : "NO";
            string strConn = "";
            string ext = Path.GetExtension(excelPath);
            //IMEX=0 表示 Excel只能用作写入  1 只能作读取  2 读写都可
            //HDR =Yes 第一行是标题，No 第一行是数据，不是标题
            if (ext == ".xls")
            {
                //如果是.xls 即07以下的版本，连接字符串
                strConn = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + excelPath + ";Extended Properties='Excel 8.0;HDR="+isYes+";IMEX=1'";
            }
            else
            {
                //如果是.xlsx 07即以上的版本
                strConn = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + excelPath + ";Extended Properties='Excel 12.0;HDR=" + isYes + ";IMEX=1'";
            }
            DataTable dtNew = new DataTable();
            //以下就是读取Excel数据的方式
            using (OleDbConnection conn = new OleDbConnection(strConn))
            {
                conn.Open();
                ////得到所有Sheet的名字
                DataTable dtNames = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "Table" });
                string shName = dtNames.Rows[0][2].ToString();//获取第一个Sheet的名字
                string sql = "select * from [" + shName + "]";//查询工作表的数据
                OleDbDataAdapter da = new OleDbDataAdapter(sql, conn);
                da.Fill(dtNew);
            }
            return dtNew;
        }

        /// <summary>
        /// StreamWriter实现将DataTable数据写入Excel文件
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="fileName"></param>
        public static void SWDataTableToExcel(DataTable dt, string fileName)
        {
            //写入流
            using (StreamWriter sw = new StreamWriter(fileName, false, Encoding.GetEncoding("gb2312")))
            {
                StringBuilder sb = new StringBuilder();//可变长字符串 这个Append方式 比字符串拼接效率高
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    sb.Append(dt.Columns[i].ColumnName + "\t");// \t 相当于tab键 不能漏掉
                }
                sb.Append(Environment.NewLine);//换行
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        sb.Append(dt.Rows[i][j].ToString() + "\t");
                    }
                    sb.Append(Environment.NewLine);//换行
                }
                sw.Write(sb.ToString());//将字符串写入当前流
                sw.Flush();//写入文件
            }
            MessageBox.Show("导出完毕！");
        }

        /// <summary>
        /// 基于NPOI Excel导入到DataTable里
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sheetName"></param>
        /// <param name="isColumnName"></param>
        /// <returns></returns>
        public static DataTable NPOIExcelToDataTable(string fileName, string sheetName, bool isColumnName)
        {
            DataTable dtNpoi = new DataTable();
            IWorkbook workBook;
            string fileExt = Path.GetExtension(fileName).ToLower();//获取扩展名
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                //创建工作簿
                //XSSFWorkbook 适用xlsx格式 HSSFWorkbook 适用xls格式
                if (fileExt == ".xlsx")
                {
                    workBook = new XSSFWorkbook(fs);
                }
                else if (fileExt == ".xls")
                {
                    workBook = new HSSFWorkbook(fs);
                }
                else
                {
                    workBook = null;
                }

                //实例化sheet
                ISheet sheet = null;
                if (sheetName != null && sheetName != "")
                {
                    sheet = workBook.GetSheet(sheetName);//获取指定sheet名称的工作表
                    if (sheet == null)
                        sheet = workBook.GetSheetAt(0);//获取第一个工作表 索引从0开始
                }
                else
                {
                    sheet = workBook.GetSheetAt(0);//获取第一个工作表
                }

                //获取表头 FirstRowNum 第一行索引 0
                IRow header = sheet.GetRow(sheet.FirstRowNum);//获取第一行
                int startRow = 0;//数据的第一行索引
                if (isColumnName)//表示第一行是列名信息
                {
                    startRow = sheet.FirstRowNum + 1;
                    //遍历第一行的单元格   列名 0                      4 一行有4个单元格 
                    for (int i = header.FirstCellNum; i < header.LastCellNum; i++)
                    {
                        //获取指定索引的单元格
                        ICell cell = header.GetCell(i);
                        if (cell != null)
                        {
                            //获取列名的值
                            string cellValue = cell.ToString();

                            if (cellValue != null)
                            {
                                DataColumn col = new DataColumn(cellValue);
                                dtNpoi.Columns.Add(col);
                            }
                            else
                            {
                                DataColumn col = new DataColumn();
                                dtNpoi.Columns.Add(col);
                            }
                        }

                    }
                }

                //数据    LastRowNum 最后一行的索引 如第九行---索引 8
                for (int i = startRow; i <= sheet.LastRowNum; i++)
                {

                    IRow row = sheet.GetRow(i);//获取第i行
                    if (row == null)
                    {
                        continue;
                    }
                    DataRow dr = dtNpoi.NewRow();
                    //遍历每行的单元格
                    for (int j = row.FirstCellNum; j < row.LastCellNum; j++)
                    {
                        if (row.GetCell(j) != null)
                            dr[j] = row.GetCell(j).ToString();
                    }
                    dtNpoi.Rows.Add(dr);
                }

            }
            return dtNpoi;
        }

        /// <summary>
        /// 基于NPOI DataTable导出到Excel
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="fileName"></param>
        /// <param name="sheetName"></param>
        public static void NPOIDataTableToExcel(DataTable dt, string fileName, string sheetName)
        {
            //创建一个工作簿对象
            IWorkbook wb = new HSSFWorkbook();
            //创建一个工作表实例
            ISheet sheet = string.IsNullOrEmpty(sheetName) ? wb.CreateSheet("sheet1") : wb.CreateSheet(sheetName);
            int rowIndex = 0;
            if (dt.Columns.Count > 0)
            {
                IRow header = sheet.CreateRow(rowIndex);//创建第一行
                //header.Height = 20;//设置行高
                //设置列名
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    ICell cell = header.CreateCell(i);//创建单元格
                    cell.SetCellValue(dt.Columns[i].ColumnName);//设置单元格的值
                }
            }
            //添加数据
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    rowIndex++;
                    IRow row = sheet.CreateRow(rowIndex);
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        ICell cell = row.CreateCell(j);//创建单元格
                        cell.SetCellValue(dt.Rows[i][j].ToString());//设置值
                    }
                }
            }

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                sheet.AutoSizeColumn(i);//自适应单元格大小
            }
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                wb.Write(fs);//将工作簿写入流
            }

            MessageBox.Show("导出成功！");
        }

        /// <summary>
        /// 将List<T>列表数据导出到Excel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="fileName"></param>
        /// <param name="sheetName"></param>
        /// <param name="colNames"></param>
        /// <returns></returns>
        public static int ListToExcel<T>(List<T> list, string fileName, string sheetName, Dictionary<string, string> colNames)
        {
            int i = 0;
            int j = 0;
            int count = 0;
            ISheet sheet = null;
            IWorkbook workbook = null;
            Type type = typeof(T);
            string ext = Path.GetExtension(fileName);
            if (ext == ".xlsx") // 2007版本
                workbook = new XSSFWorkbook();
            else if (ext == ".xls") // 2003版本
                workbook = new HSSFWorkbook();
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    if (workbook != null)
                    {
                        sheet = workbook.CreateSheet(sheetName);
                    }
                    else
                    {
                        return -1;
                    }
                    List<string> keys = new List<string>(colNames.Keys);
                    if (colNames.Count > 0) //写入列名
                    {
                        IRow row = sheet.CreateRow(0);
                        for (j = 0; j < keys.Count; ++j)
                        {
                            row.CreateCell(j).SetCellValue(colNames[keys[j]]);
                        }
                        count = 1;
                    }
                    else
                    {
                        count = 0;
                    }
                    IRow firstRow = sheet.GetRow(0);
                    int cellCount = firstRow.LastCellNum;
                    PropertyInfo[] props = type.GetProperties();
                    for (i = 0; i < list.Count; i++)
                    {
                        IRow row = sheet.CreateRow(count);
                        for (j = firstRow.FirstCellNum; j < cellCount; j++)
                        {
                            var p = type.GetProperty(keys[j]);
                            object val = p.GetValue(list[i]);
                            if (val == null)
                                val = "";
                            row.CreateCell(j).SetCellValue(val.ToString());
                        }
                       count ++;
                    }
                    workbook.Write(fs); //写入到excel
                    return count;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("导出异常: " + ex.Message);
                return -1;
            }

        }
    }
}
