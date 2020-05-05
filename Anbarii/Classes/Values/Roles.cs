using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Anbarii.Classes.Values
{
    public static class RolesString
    {
        public const string Admin = "1";
        public const string Anbar = "2";
        public const string Tajer = "3";

    }
    public static class RolesInt
    {
        public const int Admin = 1;
        public const int Anbar = 2;
        public const int Tajer = 3;
        public readonly static int[] all = new int[] { Admin, Anbar, Tajer };

    }

}