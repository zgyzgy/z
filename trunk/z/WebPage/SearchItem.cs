using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace z.WebPage
{
    public class SearchItem
    {
        private Dictionary<string, string> _values;

        public Dictionary<string, string> Values
        {
            get
            {
                if (_values == null)
                    _values = new Dictionary<string, string>();
                return _values;
            }

            set
            {
                _values = value;
            }
        }

        public static SearchItem GetAllPram()
        {
            SearchItem item = new WebPage.SearchItem();
            item.Values = new Dictionary<string, string>();
            var get = HttpContext.Current.Request.QueryString;
            foreach (var k in get.AllKeys)
            {
                if (!item.Values.ContainsKey(k))
                {
                    item.Values.Add(k, get[k]);
                }
            }
            var post = HttpContext.Current.Request.Form;
            foreach (var k in post.AllKeys)
            {
                if (!item.Values.ContainsKey(k))
                {
                    item.Values.Add(k, post[k]);
                }
            }
            return item;
        }

        public bool HasKey(string key, Action<string> act = null)
        {
            if (Values.ContainsKey(key) && !string.IsNullOrWhiteSpace(Values[key]))
            {
                act?.Invoke(Values[key]);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

