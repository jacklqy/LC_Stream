﻿using Common.CustomAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class AttributeHelper
    {
        /// <summary>
        /// 获取映射表名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTName(this Type type)
        {
            string tableName = "";
            var attrs = type.GetCustomAttributes(false);//获取此类型上所有自定义特性
            foreach (var attr in attrs)
            {
                if (attr is TableAttribute)
                {
                    TableAttribute tableAttr = attr as TableAttribute;
                    tableName = tableAttr.Name;
                    break;
                }
            }
            if (string.IsNullOrEmpty(tableName))
                tableName = type.Name;
            return tableName;
        }

        /// <summary>
        /// 获取映射列名
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static string GetColName(this PropertyInfo property)
        {
            string colName = "";
            var attrs = property.GetCustomAttributes(false);//获取此属性上所有自定义特性
            foreach (var attr in attrs)
            {
                if (attr is ColumnAttribute)
                {
                    ColumnAttribute colAttr = attr as ColumnAttribute;
                    colName = colAttr.ColName;
                    break;
                }
            }
            if (string.IsNullOrEmpty(colName))
                colName = property.Name;
            return colName;
        }

        /// <summary>
        /// 获取主键名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetPrimaryName(this Type type)
        {
            string priName = "";
            var attrs = type.GetCustomAttributes(false);//获取此类型上所有自定义特性
            foreach (var attr in attrs)
            {
                if (attr is PrimaryKeyAttribute)
                {
                    PrimaryKeyAttribute priAttr = attr as PrimaryKeyAttribute;
                    priName = priAttr.Name;
                    break;
                }
            }
            return priName;
        }

        /// <summary>
        /// 判断指定属性是否为主键
        /// </summary>
        /// <param name="type"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static bool IsPrimaryKey(this PropertyInfo property)
        {
            Type type = property.DeclaringType;
            string primaryKey = type.GetPrimaryName();//获取该类型的主键名
            string colName = property.GetColName();//获取该属性的映射列名
            return (primaryKey == colName);
        }

        /// <summary>
        /// 判断主键是否自增
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsAutoIncrement(this Type type)
        {
            var attrs = type.GetCustomAttributes(false);//获取此类型上所有自定义特性
            foreach (var attr in attrs)
            {
                if (attr is PrimaryKeyAttribute)
                {
                    PrimaryKeyAttribute priAttr = attr as PrimaryKeyAttribute;
                    return priAttr.autoIncrement;
                }
            }
            return false;
        }
    }
}
