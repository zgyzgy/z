using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace z.Results
{
    public class SelectResult : UIResult
    {
        public SelectResult(List<SelectItem> selectItemList)
        {
            Obj = selectItemList;
        }

        /// <summary>
        /// 隐式转换
        /// </summary>
        /// <param name="d"></param>
        public static implicit operator SelectResult(List<SelectItem> d)
        {
            return new SelectResult(d);
        }
    }
}
