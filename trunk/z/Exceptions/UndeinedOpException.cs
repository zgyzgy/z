using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using z.Extensions;

namespace z.Exceptions
{
    /// <summary>
    /// 无效入口参数异常
    /// </summary>
    public class undefinedOpException : Exception
    {
        public override string Message
        {
            get
            {
                string op = HttpExtension.GetRequestParam(HttpExtension.op);
                if (string.IsNullOrEmpty(op))
                {
                    return $"请求参数字段{HttpExtension.op}必输";
                }
                return $"请求参数字段{op}对应的方法返回值必须是UIResult或其子类且不能有参数";
            }
        }
    }
}
