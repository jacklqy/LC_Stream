using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class PropertyHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cols">列名字符串，以逗号隔开</param>
        /// <returns></returns>
        public static PropertyInfo[] GetProperties<T>(string cols)
        {
            Type type = typeof(T);//获取Type对象
            PropertyInfo[] properties = type.GetProperties();//获取所有公有属性
            if (!string.IsNullOrEmpty(cols))
            {
                List<string> listCols = cols.GetStrList(',', true);//转换成List<string>，并转换成小写
                properties = properties.Where(p => listCols.Contains(p.GetColName().ToLower())).ToArray();
            }
            return properties;
        }

        /// <summary>
        /// 指定不包含列名 ，返回需要的列名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="notHasCols"></param>
        /// <returns></returns>
        public static string GetColNames<T>(string notHasCols)
        {
            string cols = "";
            Type type = typeof(T);//获取Type对象
            PropertyInfo[] properties = type.GetProperties();//获取所有公有属性
            if (!string.IsNullOrEmpty(notHasCols))
            {
                List<string> listCols = notHasCols.GetStrList(',', true);//转换成List<string>，并转换成小写
                properties= properties.Where(p => !listCols.Contains(p.GetColName().ToLower())).ToArray();
            }
            cols = string.Join(",", properties.Select(p => p.GetColName()));
            return cols;
        }
    }
}
