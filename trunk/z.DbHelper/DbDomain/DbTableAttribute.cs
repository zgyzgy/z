using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace z.DbHelper.DbDomain
{
    /// <summary>
    /// 表的属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class DbTableAttribute : Attribute
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string Tablename
        {
            get;
        }

        /// <summary>
        /// 表的属性
        /// </summary>
        /// <param name="tablename"></param>
        public DbTableAttribute(string tablename)
        {
            Tablename = tablename;
        }
    }
}
