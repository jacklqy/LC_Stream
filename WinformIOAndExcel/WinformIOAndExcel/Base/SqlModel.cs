using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Base
{
     public class SqlModel
    {
        public string Sql { get; set; }
        public SqlParameter[] Paras { get; set; }
    }
}
