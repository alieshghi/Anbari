using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Anbarii.Classes
{
    public static class DatetimeConvertor
    {
        public static readonly CultureInfo ci = new CultureInfo("fa-ir");
        public static string ToStringPersian(this int Value, string Type) => Value.ToString(Type, ci);
        public static string ToStringTimestamp(this DateTime Value) => Value.ToString("yyyyMMddHHmmssffff", ci);

        public static string MiladiTopersianstring(this string miladi)
        {
            DateTime s = DateTime.Parse(miladi, CultureInfo.GetCultureInfo("en-US").DateTimeFormat);
            PersianCalendar pc = new PersianCalendar();
            string answer = pc.GetYear(s) + "/" + pc.GetMonth(s) + "/" + pc.GetDayOfMonth(s);
            return answer;
        }
        public static string PersianToEnglish(this string persianStr)
        {
            Dictionary<string, string> LettersDictionary = new Dictionary<string, string>
            {
                ["۰"] = "0",
                ["۱"] = "1",
                ["۲"] = "2",
                ["۳"] = "3",
                ["۴"] = "4",
                ["۵"] = "5",
                ["۶"] = "6",
                ["۷"] = "7",
                ["۸"] = "8",
                ["۹"] = "9"
            };
            return LettersDictionary.Aggregate(persianStr, (current, item) =>
                         current.Replace(item.Key, item.Value));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public static DateTime PersianToMiladiDate(this string Persian)
        {

            try
            {
                CultureInfo pr = new CultureInfo("fa-ir");
                DateTime dt = DateTime.ParseExact(Persian, "yyyy/MM/dd", pr);
                return dt;
            }
            catch (Exception)
            {
                Persian = Persian.PersianToEnglish();
                CultureInfo pr = new CultureInfo("fa-ir");
                DateTime dt = DateTime.ParseExact(Persian, "yyyy/MM/dd", pr);
                return dt;
            }
        }

        public static string MiladiTopersian(this DateTime miladi)
        {
            PersianCalendar pc = new PersianCalendar();
            string answer = pc.GetYear(miladi) + "/" + pc.GetMonth(miladi).ToString("00", CultureInfo.CurrentCulture) + "/" + pc.GetDayOfMonth(miladi).ToString("00", CultureInfo.CurrentCulture);
            return answer;
        }
        public static int MiladiTopersianYear(this DateTime miladi)
        {
            PersianCalendar pc = new PersianCalendar();
            return pc.GetYear(miladi);
        }
        public static string PersianMonth(int number)
        {

            switch (number)
            {
                case 1:
                    return "فروردین";
                case 2:
                    return "اردیبهشت";
                case 3:
                    return "خرداد";
                case 4:
                    return "تیر";
                case 5:
                    return "مرداد";
                case 6:
                    return "شهریور";
                case 7:
                    return "مهر";
                case 8:
                    return "آبان";
                case 9:
                    return "آذر";
                case 10:
                    return "دی";
                case 11:
                    return "بهمن";
                case 12:
                    return "اسفند";
                default:
                    return "";
            }
        }
        public static int Lastday(int year, int month)
        {
            if (month <= 6)
            {
                return 31;
            }
            else
            {
                if (month == 12)
                {
                    PersianCalendar PC = new PersianCalendar();
                    if (PC.IsLeapYear(year))
                        return 30;
                    else
                        return 29;
                }
                else
                {
                    return 30;
                }
            }

        }
        public static bool Isleap(int year)
        {
            Double a = 0.025;
            int b = 266;
            double leapDays0;
            double leapDays1;
            if (year > 0)
            {
                leapDays0 = ((year + 38) % 2820) * 0.24219 + a; //0.24219 ~ extra days of one year
                leapDays1 = ((year + 39) % 2820) * 0.24219 + a;// 38 days is the difference of epoch to 2820-year cycle
            }
            else if (year < 0)
            {
                leapDays0 = ((year + 39) % 2820) * 0.24219 + a;
                leapDays1 = ((year + 40) % 2820) * 0.24219 + a;
            }
            else
            {
                return false;
            }

            int frac0 = (int)(leapDays0 - (int)leapDays0) * 1000;
            int frac1 = (int)(leapDays1 - (int)leapDays1) * 1000;

            if (frac0 <= b && frac1 > b)
            {
                return true;
            }
            else
            {


                return false;
            }
        }
    }

}