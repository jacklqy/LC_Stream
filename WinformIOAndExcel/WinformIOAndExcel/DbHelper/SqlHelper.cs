﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper
{
   public class SqlHelper
    {
        //1.连接字符串
        private static string connStr = ConfigurationManager.ConnectionStrings["connStr"].ConnectionString;

        //2.Command三个执行方法

        /// <summary>
        /// 增、删、改的通用方法
        /// 执行Sql语句或存储过程，返回受影响的行数
        /// </summary>
        /// <param name="sql">sql语句或存储过程名</param>
        /// <param name="cmdType">执行的脚本类型 1:sql语句  2:存储过程</param>
        /// <param name="parameters">参数列表</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string sql, int cmdType, params SqlParameter[] parameters)
        {
            //select @@Identity 返回上一次插入记录时自动产生的ID
            int result = 0;//返回结果
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                //执行脚本的对象cmd
                SqlCommand cmd = BuilderCommand(conn, sql, cmdType, null, parameters);
                result = cmd.ExecuteNonQuery();//执行T-SQL并返回受影响行数
                cmd.Parameters.Clear();
            }
            //using原理：类似于try finally
            return result;
        }

        /// <summary>
        /// 执行sql查询，返回第一行第一列的值
        /// </summary>
        /// <param name="sql">sql语句或存储过程</param>
        /// <param name="cmdType">执行的脚本类型 1:sql语句  2:存储过程</param>
        /// <param name="parameters">参数列表</param>
        /// <returns></returns>
        public static object ExecuteScalar(string sql, int cmdType, params SqlParameter[] parameters)
        {
            object result = null;//返回结果
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                //执行脚本的对象cmd
                SqlCommand cmd = BuilderCommand(conn, sql, cmdType, null, parameters);
                result = cmd.ExecuteScalar();//执行T-SQL并返回第一行第一列的值
                cmd.Parameters.Clear();
                if (result == null || result == DBNull.Value)
                {
                    return null;
                }
                else
                {
                    return result;
                }
            }
        }

        /// <summary>
        /// 执行sql查询,返回SqlDataReader对象
        /// </summary>
        /// <param name="sql">sql语句或存储过程</param>
        /// <param name="cmdType">执行的脚本类型 1:sql语句  2:存储过程</param>
        /// <param name="parameters">参数列表</param>
        /// <returns></returns>
        public static SqlDataReader ExecuteReader(string sql, int cmdType, params SqlParameter[] parameters)
        {
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand cmd = BuilderCommand(conn, sql, cmdType, null, parameters);
            SqlDataReader reader;
            try
            {
                reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return reader;
            }
            catch (Exception ex)
            {
                conn.Close();
                throw new Exception("创建reader对象发生异常", ex);
            }
        }

        //SqlDataAdapter两种填充、Update方法（自动配置命令）
        /// <summary>
        /// 执行查询，查询结果填充到DataTable 只针对查询一个表的情况
        /// </summary>
        /// <param name="sql">sql语句或存储过程</param>
        /// <param name="cmdType">执行的脚本类型 1:sql语句  2:存储过程</param>
        /// <param name="parameters">参数列表</param>
        /// <returns></returns>
        public static DataTable GetDataTable(string sql, int cmdType, params SqlParameter[] parameters)
        {
            DataTable dt = null;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlCommand cmd = BuilderCommand(conn, sql, cmdType, null, parameters);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                dt = new DataTable();
                da.Fill(dt);
            }
            return dt;
        }

        /// <summary>
        /// 执行查询，数据填充到DataSet
        /// </summary>
        /// <param name="sql">sql语句或存储过程</param>
        /// <param name="cmdType">执行的脚本类型 1:sql语句  2:存储过程</param>
        /// <param name="parameters">参数列表</param>
        /// <returns></returns>
        public static DataSet GetDataSet(string sql, int cmdType, params SqlParameter[] parameters)
        {
            DataSet ds = null;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlCommand cmd = BuilderCommand(conn, sql, cmdType, null, parameters);
                //数据适配器
                //conn 自动打开  断开式连接
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                ds = new DataSet();
                da.Fill(ds);
                //自动关闭conn
            }
            return ds;
        }

        /// <summary>
        /// SqlDataAdapter更新DataTable到数据库（不常用）
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static bool UpdateDataTable(DataTable dt,string sql)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                SqlCommandBuilder cmdBuilder = new SqlCommandBuilder(da);
                int count= da.Update(dt);
                if (count > 0)
                    return true;
            }
            return false;
        }

        //执行事务的方法三个

        /// <summary>
        /// 事务 执行批量sql
        /// </summary>
        /// <param name="listSql"></param>
        /// <returns></returns>
        public static bool ExecuteTrans(List<string> listSql)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                SqlCommand cmd = BuilderCommand(conn, "", 1, trans);
                try
                {
                    int count = 0;
                    for (int i = 0; i < listSql.Count; i++)
                    {
                        if (listSql[i].Length > 0)
                        {
                            cmd.CommandText = listSql[i];
                            cmd.CommandType = CommandType.Text;
                            count += cmd.ExecuteNonQuery();
                        }
                    }
                    trans.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception("执行事务出现异常", ex);
                }
            }
        }

        /// <summary>
        /// 事务 批量执行 CommandInfo 包括sql,脚本类型，参数列表
        /// </summary>
        /// <param name="comList"></param>
        /// <returns></returns>
        public static bool ExecuteTrans(List<CommandInfo> comList)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                SqlCommand cmd = BuilderCommand(conn, "", 1, trans);
                try
                {
                    int count = 0;
                    for (int i = 0; i < comList.Count; i++)
                    {
                        cmd.CommandText = comList[i].CommandText;
                        if (comList[i].IsProc)
                            cmd.CommandType = CommandType.StoredProcedure;
                        else
                            cmd.CommandType = CommandType.Text;

                        if (comList[i].Paras != null && comList[i].Paras.Length > 0)
                        {
                            cmd.Parameters.Clear();
                            foreach (var p in comList[i].Paras)
                            {
                                cmd.Parameters.Add(p);
                            }
                        }
                        count += cmd.ExecuteNonQuery();

                    }
                    trans.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception("执行事务出现异常", ex);
                }
            }
        }

        public static T ExecuteTrans<T>(Func<IDbCommand, T> action)
        {
            using (IDbConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                IDbTransaction trans = conn.BeginTransaction();
                IDbCommand cmd = conn.CreateCommand();
                cmd.Transaction = trans;
                return action(cmd);
            }
        }

        //构造Command对象方法
        /// <summary>
        /// 构建SqlCommand
        /// </summary>
        /// <param name="conn">数据库连接对象</param>
        /// <param name="sql">SQL语句或存储过程</param>
        /// <param name="comType">命令字符串的类型</param>
        /// <param name="trans">事务</param>
        /// <param name="paras">参数数组</param>
        /// <returns></returns>
        private static SqlCommand BuilderCommand(SqlConnection conn, string sql, int cmdType, SqlTransaction trans, params SqlParameter[] paras)
        {
            if (conn == null) throw new ArgumentNullException("连接对象不能为空！");
            SqlCommand command = new SqlCommand(sql, conn);
            if (cmdType == 2)
                command.CommandType = CommandType.StoredProcedure;
            if (conn.State == ConnectionState.Closed)
                conn.Open();
            if (trans != null)
                command.Transaction = trans;
            if (paras != null && paras.Length > 0)
            {
                command.Parameters.Clear();
                command.Parameters.AddRange(paras);
            }
            return command;
        }

        /// <summary>
        /// 将参数数组添加到IDbCommand的参数集合中
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public static void AddParas(IDbCommand cmd, SqlParameter[] paras)
        {
            if (paras != null && paras.Length > 0)
            {
                foreach (var p in paras)
                {
                    cmd.Parameters.Add(p);
                }
            }
        }

        /// <summary>
        /// 初始化IDbCommand的操作(每步的执行)
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="sql"></param>
        /// <param name="cmdType"></param>
        /// <param name="paras"></param>
        public static void InitIdbCommand(IDbCommand cmd, string sql, int cmdType, SqlParameter[] paras)
        {
            cmd.CommandText = sql;
            if (cmdType == 1)
                cmd.CommandType = CommandType.Text;
            else
                cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Clear();
            AddParas(cmd, paras);
        }

        /// <summary>
        /// 执行Insert \Update\Delete语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="cmdType"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public static bool Execute(string sql, int cmdType, params SqlParameter[] paras)
        {
            int count = 0;
            count = ExecuteNonQuery(sql, cmdType, paras);
            if (count > 0)
                return true;
            return false;

        }

        /// <summary>
        /// 执行查询，返回指定类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="cmdType"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public static T GetValByExecute<T>(string sql,int cmdType, params SqlParameter[] paras)
        {
            object oVal = null;
            oVal = ExecuteScalar(sql, cmdType, paras);
            if (oVal != null && oVal.ToString() != "")
                return (T)Convert.ChangeType(oVal, typeof(T));
            else
                return default(T);
        }

        /// <summary>
        /// Adapter提交DataTable  单表
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static int AdapterUpdateDt(DataTable dt,string sql)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                SqlCommandBuilder sbuilder = new SqlCommandBuilder(da);
                return da.Update(dt);
            }
        }
    }
}
