using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Anbarii.Classes
{
    internal static class EnableExtendtion
    {
        //public static int ToInt32Enable(this bool Value)
        //{
        //    switch (Value)
        //    {
        //        case true:
        //            return 1;
        //        default:
        //            return 0;
        //    }
        //}
        //public static String ToStringEnable(this bool Value)
        //{
        //    return Value switch { true => System.Net.WebUtility.HtmlDecode("&#10004;"), _ => System.Net.WebUtility.HtmlDecode("&#10006;") };
        //}
        public static List<Enable> EnableList()
        {
            List<Enable> Enable = new List<Enable>();
            Enable.Add(new Enable(System.Net.WebUtility.HtmlDecode("&#10006;"), false));
            Enable.Add(new Enable(System.Net.WebUtility.HtmlDecode("&#10004;"), true));
            return Enable;
        }

        public class Enable
        {
            public Enable(string _name, bool _value)
            {
                Name = _name;
                Value = _value;
            }
            public String Name { get; set; }
            public bool Value { get; set; }
        }
    }

}