using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace z.Extensions
{
    /// <summary>
    /// 配置文件帮助类
    /// </summary>
    public static class ConfigExtension
    {
        /// <summary>
        /// 取配置节点
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public static string GetConfig(string key, string defaultValue = "")
        {
            var a = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(a))
            {
                return defaultValue;
            }
            else
                return a;
        }

        /// <summary>
        /// 测试模式
        /// </summary>
        public static bool TestModel
        {
            get
            {
                return GetConfig("TestModel") == "true";
            }
        }
    }
}
