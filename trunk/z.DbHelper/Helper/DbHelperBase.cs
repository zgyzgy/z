﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using z;
using z.Exceptions;
using z.DBHelper.Connection;
using z.DBHelper.Info;
using z.DbHelper.DbDomain;
using z.Extensions;

namespace z.DBHelper.Helper
{
    /// <summary>
    /// 数据操作类
    /// </summary>
    public abstract class DbHelperBase
    {
        #region 私有变量
        /// <summary>
        /// 配置文件字符串
        /// </summary>
        protected string _dbConnectionInfoStr;
        /// <summary>
        /// 配置文件类
        /// </summary>
        protected IDbConnectionInfo _dbConnectionInfo;
        /// <summary>
        /// 数据库链接
        /// </summary>
        protected DbConnection _dbConnection;
        /// <summary>
        /// 数据库命令对象
        /// </summary>
        protected DbCommand _dbCommand;

        string _insert = "INSERT INTO {0}({1}) VALUES({2})";
        string _update = "UPDATE {0} SET {1} WHERE {2}";
        string _delete = "DELETE {0} WHERE {1}";
        string _selectcount = "SELECT COUNT(1) from {0} WHERE {1}";
        #endregion
        #region 构造

        public DbHelperBase(string ConnectionStr)
        {
            _dbConnectionInfoStr = ConnectionStr;
            Open();
        }

        public DbHelperBase(IDbConnectionInfo conn)
            : this(conn.ToConnectionString())
        {
            _dbConnectionInfo = conn;
        }


        ~DbHelperBase()
        {
            Close();
        }
        #endregion
        #region 数据操作
        #region 查表

        public DataTable ExecuteTable(string sql)
        {
            return ExecuteTable(sql, 0, 0);
        }

        public DataTable ExecuteTable(string sql, PageInfo pageinfo)
        {
            return ExecuteTable(sql, pageinfo.PageSize, pageinfo.PageIndex);
        }

        public DataTable ExecuteTable(string sql, PageInfo pageinfo, out int allCount)
        {
            return ExecuteTable(sql, pageinfo.PageSize, pageinfo.PageIndex, out allCount);
        }

        /// <summary>
        /// 查
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public DataTable ExecuteTable(string sql, int pageSize, int pageIndex)
        {
            DataTable dt = new DataTable();
            try
            {
                if (_dbCommand.Connection.State != ConnectionState.Open)
                    Open();
                _dbCommand.CommandText = GetPageSql(sql, pageSize, pageIndex);
                IDataReader reader = _dbCommand.ExecuteReader();
                dt = this.ReaderToTable(reader);
            }
            catch (Exception ex)
            {
                throw new DataBaseException(ex.Message, sql);
            }
            return dt;
        }

        /// <summary>
        /// 查
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="allCount"></param>
        /// <returns></returns>
        public DataTable ExecuteTable(string sql, int pageSize, int pageIndex, out int allCount)
        {
            DataTable dt = ExecuteTable(sql, pageSize, pageIndex);
            try
            {
                if (_dbCommand.Connection.State != ConnectionState.Open)
                    Open();
                _dbCommand.CommandText = GetCountSql(sql);
                IDataReader reader = _dbCommand.ExecuteReader();
                DataTable dtcount = this.ReaderToTable(reader);
                if (dtcount.IsOneLine())
                {
                    int.TryParse(dtcount.Rows[0][0].ToString(), out allCount);
                }
                else
                {
                    allCount = 0;
                }
            }
            catch (Exception ex)
            {
                throw new DataBaseException(ex.Message, sql);
            }
            return dt;
        }

        /// <summary>
        /// 取总数sql
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        protected abstract string GetCountSql(string sql);

        /// <summary>
        /// 取分页sql
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        protected abstract string GetPageSql(string sql, int pageSize = 0, int pageIndex = 0);
        #endregion
        #region 增删改
        /// <summary>
        /// 增删改
        /// </summary>
        /// <param name="sql">sql</param>
        /// <param name="fun">大量sql时每执行一条sql的回调(当前行索引,当前行发生的数据变更,总数据变更)</param>
        /// <returns></returns>
        public virtual int ExecuteNonQuery(List<string> sql, Action<int, int, int> fun = null)
        {
            if (sql == null)
                return 0;
            string tmpStr = "";
            int influenceRowCount = 0;
            try
            {
                if (_dbCommand.Connection.State != ConnectionState.Open)
                    Open();
                for (int i = 0; i < sql.Count; i++)
                {
                    tmpStr = sql[i];
                    _dbCommand.CommandText = tmpStr;
                    int cntnow = _dbCommand.ExecuteNonQuery();
                    influenceRowCount += cntnow;
                    if (fun != null)
                    {
                        fun(i, cntnow, influenceRowCount);
                    }
                }
                return influenceRowCount;
            }
            catch (Exception ex)
            {
                throw new DataBaseException(ex.Message, _dbCommand.CommandText);
            }
        }

        /// <summary>
        /// 增删改
        /// </summary>
        /// <param name="sql">执行多条sql</param>
        /// <returns></returns>
        public int ExecuteNonQuery(params string[] sql)
        {
            return ExecuteNonQuery(sql.ToList());
        }

        /// <summary>
        /// 增删改
        /// </summary>
        /// <param name="sql">执行多条sql</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sql)
        {
            return ExecuteNonQuery(new List<string>() { sql });
        }
        #endregion
        #region 对象增删改
        public int Save(DomainBase info)
        {
            if (_dbCommand.Connection.State != ConnectionState.Open)
                Open();
            IDbDataParameter[] dbprams = info.GetPrimaryKey().Select(a =>
           {
               if (a.GetAttribute<PrimaryKeyAttribute>() != null)
               {
                   if (string.IsNullOrEmpty(a.GetValue(info, null).ToString()))
                   {
                       throw new DataBaseException("字段:" + a.Name + "是主键,在保存时不能为空");
                   }
               }
               IDbDataParameter p = GetDbDataParameter(a, info);
               object value = a.GetValue(info, null);
               p.Value = (value == null || string.IsNullOrEmpty(value.ToString()) ? DBNull.Value : value);
               return p;
           }).ToArray();
            _dbCommand.Parameters.Clear();
            _dbCommand.Parameters.AddRange(dbprams);
            string tablename = info.GetTableName();
            string where = string.Join(" and ", info.GetPrimaryKey().Select(a => a.Name + "=" + GetPramCols(a.Name)));
            _dbCommand.CommandText = string.Format(_selectcount, tablename, where);
            IDataReader reader = _dbCommand.ExecuteReader();
            int i = 0;
            if (reader.Read())
                i = reader.GetValue(0).ToString().ToInt();
            if (i == 0)
                return Insert(info);
            else
                return Update(info);
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public int Insert(DomainBase info)
        {
            if (_dbCommand.Connection.State != ConnectionState.Open)
                Open();
            IDbDataParameter[] dbprams = info.GetAllField().Select(a =>
            {
                IDbDataParameter p = GetDbDataParameter(a, info);
                return p;
            }).ToArray();
            _dbCommand.Parameters.Clear();
            _dbCommand.Parameters.AddRange(dbprams);
            _dbCommand.CommandText = string.Format(_insert, info.GetTableName(),
                string.Join(",", dbprams.Select(a => a.ParameterName)),
                string.Join(",", dbprams.Select(a => ":" + a.ParameterName))
                );
            try
            {
                return _dbCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new DataBaseException(ex.Message, _dbCommand.CommandText, info);
            }
        }

        /// <summary>
        /// 更新，按照主键进行更新
        /// </summary>
        /// <exception cref="DataBaseException"></exception>
        /// 没有主键
        /// 主键不是全部都有值
        /// <param name="info"></param>
        /// <returns></returns>
        public int Update(DomainBase info)
        {
            if (_dbCommand.Connection.State != ConnectionState.Open)
                Open();
            IDbDataParameter[] dbprams = info.GetFieldWithoutPrimaryKey().Select(a =>
           {
               if (a.GetAttribute<PrimaryKeyAttribute>() != null)
               {
                   if (string.IsNullOrEmpty(a.GetValue(info, null).ToString()))
                   {
                       throw new DataBaseException("字段:" + a.Name + "是主键,在更新时不能为空");
                   }
               }
               IDbDataParameter p = GetDbDataParameter(a, info);
               return p;
           }).ToArray().Concat(info.GetPrimaryKey().Select(a =>
          {
              IDbDataParameter p = GetDbDataParameter(a, info);
              return p;
          })).ToArray();
            _dbCommand.Parameters.Clear();
            _dbCommand.Parameters.AddRange(dbprams);
            string tablename = info.GetTableName();
            string set = string.Join(",", info.GetFieldWithoutPrimaryKey().Select(a => a.Name + "=" + GetPramCols(a.Name)));
            string where = string.Join(" and ", info.GetPrimaryKey().Select(a => a.Name + "=" + GetPramCols(a.Name)));
            if (string.IsNullOrEmpty(where))
            {
                throw new DataBaseException("没有主键,不能更新");
            }
            _dbCommand.CommandText = string.Format(_update, tablename, set, where);
            try
            {
                return _dbCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new DataBaseException(ex.Message, _dbCommand.CommandText, info);
            }
        }

        /// <summary>
        /// 删除,按照主键进行删除
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public int Delete(DomainBase info)
        {
            IDbDataParameter[] dbprams = info.GetPrimaryKey().Select(a =>
            {
                if (a.GetAttribute<PrimaryKeyAttribute>() != null)
                {
                    if (string.IsNullOrEmpty(a.GetValue(info, null).ToString()))
                    {
                        throw new DataBaseException("字段:" + a.Name + "是主键,在删除时不能为空");
                    }
                }
                IDbDataParameter p = GetDbDataParameter(a, info);
                return p;
            }).ToArray();
            string tablename = info.GetTableName();
            string where = string.Join(" and ", info.GetPrimaryKey().Select(a => a.Name + "=" + GetPramCols(a.Name)));
            if (string.IsNullOrEmpty(where))
            {
                throw new DataBaseException("没有主键,不能删除");
            }
            _dbCommand.Parameters.Clear();
            _dbCommand.Parameters.AddRange(dbprams);
            _dbCommand.CommandText = string.Format(_delete, tablename, where);
            try
            {
                return _dbCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new DataBaseException(ex.Message, _dbCommand.CommandText, info);
            }
        }

        /// <summary>
        /// 删除，按照非null的进行删除，全null抛出异常
        /// </summary>
        /// <exception cref="DataBaseException"></exception>
        /// 没有删除条件
        /// <param name="info"></param>
        /// <returns></returns>
        public int DeleteList(DomainBase info)
        {
            IDbDataParameter[] dbprams = info.GetPrimaryKey().Where(a =>
               {
                   return a.GetValue(info, null) != null;
               }).Select(a =>
               {
                   IDbDataParameter p = GetDbDataParameter(a, info);
                   return p;
               }).ToArray();
            string tablename = info.GetTableName();
            string where = string.Join(" and ", info.GetPrimaryKey().Select(a => a.Name + "=" + GetPramCols(a.Name)));
            if (string.IsNullOrEmpty(where))
            {
                throw new DataBaseException("没有删除条件，不能删除");
            }
            _dbCommand.Parameters.Clear();
            _dbCommand.Parameters.AddRange(dbprams);
            _dbCommand.CommandText = string.Format(_delete, tablename, where);
            try
            {
                return _dbCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new DataBaseException(ex.Message, _dbCommand.CommandText, info);
            }
        }

        /// <summary>
        /// 获取字段名对应的参数表示字符串
        /// </summary>
        /// <param name="cols"></param>
        /// <returns></returns>
        protected abstract string GetPramCols(string cols);
        #endregion
        #endregion
        #region sql方法
        public abstract string GetMergerSql(MergeInfo info);
        #endregion
        #region 事物操作

        /// <summary>
        /// 判断是否在事物中
        /// </summary>
        /// <returns></returns>
        public virtual bool HasTransaction()
        {
            return _dbCommand != null && _dbCommand.Transaction != null;
        }

        /// <summary>
        /// 开启事物
        /// </summary>
        public virtual DbTransaction BeginTransaction()
        {
            if (_dbCommand == null || _dbCommand.Transaction == null)
            {
                if (_dbConnection == null || _dbConnection.State != ConnectionState.Open)
                    Open();
                _dbCommand.Transaction = _dbConnection.BeginTransaction();
            }
            return _dbCommand.Transaction;
        }


        #endregion
        #region 链接操作
        public virtual void Open()
        {
            //连接数据库
            if (_dbConnection == null || _dbConnection.State != ConnectionState.Open)
            {
                _dbConnection = GetDbConnection(_dbConnectionInfoStr);
                _dbConnection.Open();
            }
            if (_dbConnection.State == ConnectionState.Closed)
            {
                _dbConnection.Open();
            }
            else if (_dbConnection.State == ConnectionState.Broken)
            {
                _dbConnection.Close();
                _dbConnection.Open();
            }
            _dbCommand = GetDbCommand(_dbConnection);
        }

        /// <summary>
        /// 取数据库链接
        /// </summary>
        /// <param name="_dbConnectionInfoStr"></param>
        /// <returns></returns>
        protected abstract DbConnection GetDbConnection(string _dbConnectionInfoStr);

        /// <summary>
        /// 取数据库操作对象
        /// </summary>
        /// <param name="dbconnection"></param>
        /// <returns></returns>
        protected abstract DbCommand GetDbCommand(DbConnection dbconnection);


        public virtual void Close()
        {
            try
            {
                if (_dbConnection != null)
                {
                    if (_dbConnection.State != ConnectionState.Closed)
                    {
                        _dbConnection.Close();
                        _dbCommand.Dispose();
                    }
                }
            }
            catch
            {

            }
        }
        #endregion
        #region 辅助方法
        /// <summary>
        /// 获取参数列表
        /// </summary>
        /// <param name="p"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        protected abstract IDbDataParameter GetDbDataParameter(PropertyInfo p, DomainBase info);

        /// <summary>
        /// 读取的内容转换为datatable
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        /// 
        private DataTable ReaderToTable(IDataReader reader)
        {

            DataTable dt = new DataTable();
            int fieldCount = reader.FieldCount;

            for (int count = 0; count < fieldCount; count++)
            {
                dt.Columns.Add(reader.GetName(count).ToUpper(), reader.GetFieldType(count));
            }

            while (reader.Read())
            {
                DataRow row = dt.NewRow();
                for (int i = 0; i < fieldCount; i++)
                {
                    row[i] = reader.GetValue(i);
                }
                dt.Rows.Add(row);
            }
            reader.Close();
            return dt;
        }

        public override string ToString()
        {
            return _dbConnectionInfoStr.ToString();
        }
        #endregion
    }
}
