using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace z.Results
{
    /// <summary>
    /// 页面通用返回值
    /// </summary>
    public class UIResult
    {
        public UIResult()
        {
        }
        public UIResult(object o)
        {
            Obj = o;
        }
        public UIResult(Exception ex)
        {
            Flag = UIResultType.Exception;
            Msg = ex.Message;
        }


        public int Flag = 0;
        public object Obj;
        public string Msg;
    }
}
