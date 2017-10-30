using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace z.Extensions
{
    public static class ObjectExtension
    {
        #region 序列化
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJson(this object obj, bool simple = false)
        {
            return JsonConvert.SerializeObject(obj, simple ? Formatting.Indented : Formatting.None);
        }

        /// <summary>
        /// 反序列化json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static T ToObj<T>(this string str)
        {
            return JsonConvert.DeserializeObject<T>(str);
        }

        /// <summary>
        /// 深度拷贝
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T DeepClone<T>(this object obj)
        {
            return obj.ToJson().ToObj<T>();
        }

        /// <summary>
        /// 获取实体类的str形式
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToCommonString(this object obj)
        {
            return obj.ToCommonString("\r\n", "=", true, true, true);
        }

        /// <summary>
        /// 使用特定方式输出字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="endstr">分隔符</param>
        /// <param name="midstr">键值对分隔符(仅当显示键和值时有效</param>
        /// <param name="HasNull">输出空值</param>
        /// <param name="hasname">输出键名</param>
        /// <param name="hasvalue">输出值</param>
        /// <returns></returns>
        public static string ToCommonString(this object obj, string endstr, string midstr, bool HasNull, bool hasname, bool hasvalue)
        {
            List<string> strlist = new List<string>();
            PropertyInfo[] props = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo p in props)
            {
                string typename = p.Name;
                string value = p.GetValue(obj, null) == null ? "" : p.GetValue(obj, null).ToString();
                if (HasNull || !string.IsNullOrEmpty(value))
                {
                    if (hasname && hasvalue)
                    {
                        strlist.Add(typename + midstr + value);
                    }
                    else if (hasvalue)
                    {
                        strlist.Add(value);
                    }
                    else if (hasname)
                    {
                        strlist.Add(typename);
                    }
                }
            }
            return String.Join(endstr, strlist);
        }

        /// <summary>
        /// 序列化为字典集
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> ToDictionary(this object obj)
        {
            return obj.ToDictionary<string>();
        }

        /// <summary>
        /// 序列化为字典集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Dictionary<string, T> ToDictionary<T>(this object obj) where T : class
        {
            Dictionary<string, T> dic = new Dictionary<string, T>();
            List<string> strlist = new List<string>();
            PropertyInfo[] props = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo p in props)
            {
                string typename = p.Name;
                T value = p.GetValue(obj, null) == null ? default(T) : p.GetValue(obj, null) as T;
                dic.Add(typename, value);
            }
            return dic;
        }
        #endregion
    }
}
