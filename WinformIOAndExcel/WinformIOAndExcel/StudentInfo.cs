using Common.CustomAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformIOAndExcel
{
    [Table("StudentInfos2")]
    [PrimaryKey("StuId",autoIncrement =true)]
    public class StudentInfo
    {
        public int StuId { get; set; }
        public string StuName { get; set; }
        public int ClassId { get; set; }
        public string Sex { get; set; }
        public string Phone { get; set; }
    }
}
