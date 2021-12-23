using DAL.Base;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformIOAndExcel.DAL
{
    public class StudentDAL : BaseDAL<StudentInfo>
    {
        public List<StudentInfo> GetAllStudents()
        {
            return GetModelList("", "", 0);
        }
    }
}
