using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using z;
using z.Extensions;

namespace z.DbHelper.DbDomain
{
    /// <summary>
    /// 所有数据操作类的基类
    /// </summary>
    public class DomainBase
    {
        /// <summary>
        /// 获取表的名字
        /// </summary>
        /// <returns></returns>
        public string GetTableName()
        {
            return this.GetAttribute<DbTableAttribute>()?.Tablename;
        }

        /// <summary>
        /// 获取主键
        /// </summary>
        /// <returns></returns>
        public PropertyInfo[] GetPrimaryKey()
        {
            return GetType().GetProperties()
                .Where(a => a.GetAttribute<PrimaryKeyAttribute>() != null)
                .ToArray();
        }

        /// <summary>
        /// 获取所有字段
        /// </summary>
        /// <returns></returns>
        public PropertyInfo[] GetAllField()
        {
            return GetType().GetProperties().ToArray();
        }

        /// <summary>
        /// 获取所有不是主键的字段
        /// </summary>
        /// <returns></returns>
        public PropertyInfo[] GetFieldWithoutPrimaryKey()
        {
            return GetType().GetProperties()
                .Where(a => a.GetAttribute<PrimaryKeyAttribute>() == null)
                .ToArray();
        }
    }
}
