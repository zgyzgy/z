﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using z.Results;

namespace z.Extensions
{
    public static class EnumExtension
    {
        /// <summary>
        /// 拆解枚举
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <returns></returns>
        public static List<dynamic> EnumToAsync<E>() where E : struct
        {
            List<dynamic> result = new List<dynamic>();
            foreach (int item in Enum.GetValues(typeof(E)))
            {
                result.Add(new
                {
                    ID = item,
                    VALUE = Enum.GetName(typeof(E), item)
                });
            }
            return result;
        }

        /// <summary>
        /// 变为字典
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="e"></param>
        /// <returns></returns>
        public static Dictionary<int, string> EnumToDictionary<E>() where E : struct
        {
            Dictionary<int, string> result = new Dictionary<int, string>();
            foreach (int item in Enum.GetValues(typeof(E)))
            {
                result.Add(item, Enum.GetName(typeof(E), item));
            }
            return result;
        }


        /// <summary>
        /// 枚举下拉
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<SelectItem> EnumToSelectItem<T>() where T : struct
        {
            List<SelectItem> res = new List<SelectItem>();
            foreach (var item in Enum.GetValues(typeof(T)))
            {
                res.Add(new SelectItem(((int)item).ToString(), item.ToString()));
            }
            return res;
        }

        /// <summary>
        /// 枚举下拉
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static List<SelectItem> EnumToSelectItem(Type t)
        {
            List<SelectItem> res = new List<SelectItem>();
            foreach (var item in Enum.GetValues(t))
            {
                res.Add(new SelectItem(((int)item).ToString(), item.ToString()));
            }
            return res;
        }

    }
}
