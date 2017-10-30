﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using z.Extensions;

namespace z.Exceptions
{
    public class DataBaseException : Exception
    {
        public DataBaseException(string msg)
            : base(msg)
        {

        }

        public DataBaseException(string msg, string sql) : base(msg + "\r\n相关语句为：\r\n" + sql)
        {

        }

        public DataBaseException(string msg, string sql, object prams) : base(msg + "\r\n相关语句为：\r\n" + sql + "\r\n相关参数为：\r\n" + prams.ToJson())
        {

        }
    }
}
