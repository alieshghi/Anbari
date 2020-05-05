using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using Anbarii.Models;

namespace Anbarii.Models
{
    public static class DataValidation
    {
        private static readonly AnbariiEntities db = new AnbariiEntities();
        public static List<string> Validate(this User Value)
        {
            List<string> Result = new List<string>();
            if (Value == null)
                return Result;
            if (string.IsNullOrEmpty(Value.Name_Company))
                Result.Add("نام شرکت/انبار وارد نشده است.");
            if (string.IsNullOrEmpty(Value.Mobile))
                Value.Mobile = "-";
            if (string.IsNullOrEmpty(Value.Phone))
                Value.Phone = "-";
            if (string.IsNullOrEmpty(Value.Code))
                Value.Code = "-";
            if (string.IsNullOrEmpty(Value.Economic_Code))
                Value.Economic_Code = "-";
            if (string.IsNullOrEmpty(Value.Adress))
                Value.Adress = "-";
            if (string.IsNullOrEmpty(Value.Code_Posti))
                Value.Code_Posti = "-";
            if (string.IsNullOrWhiteSpace(Value.Name))
                Result.Add("نام وارد نشده است.");

            return Result;
        }
        public static List<string> Validate(this Row Value)
        {
            List<string> Result = new List<string>();
            if (Value == null)
                return Result;

            return Result;
        }
        public static List<string> Validate(this Notification Value)
        {
            List<string> Result = new List<string>();
            if (Value == null)
                return Result;

            return Result;
        }
        public static List<string> Validate(this List<ReceiptDetail> Value)
        {
            List<string> Result = new List<string>();
            if (Value == null)
                return Result;
            return Result;
        }
        public static List<string> Validate(this Receipt Value)
        {
            List<string> Result = new List<string>();
            if (Value == null)
                return Result;
            if (string.IsNullOrWhiteSpace(Value.Name))
                Result.Add("نام وارد نشده است.");
            return Result;
        }
        public static List<string> ValidateDestination(this long Value)
        {
            List<string> Result = new List<string>();
            if (!db.Users.Where(i => i.ID.Equals(Value)).Any())
                Result.Add("تاجر وارد نشده است.");
            return Result;
        }
        public static List<string> Validate(this IEnumerable<RowInput> Value)
        {
            List<string> Result = new List<string>();
            if (Value == null)
                return Result;
            foreach (var item in Value)
            {
                if (item.ID < 0)
                    Result.Add("کمتر از صفر قاب قبول نیست.");
                if (item.Count < 0)
                    Result.Add("کمتر از صفر قاب قبول نیست.");
            }
            return Result;
        }
        public static List<string> Validate(this Load Value)
        {
            List<string> Result = new List<string>();
            if (Value == null)
                return Result;
            if (string.IsNullOrWhiteSpace(Value.Name))
                Result.Add("نام بار وارد نشده است.");
            if (Value.OwnerID <= 0)
                Result.Add("تاجر انتخاب نشده است.");
            if (Value.Count < 0)
                Result.Add("تعداد بار کمتر از صفر است.");
            if (Value.Load_TypeID <= 0)
                Result.Add("نوع بار انتخاب نشده است.");
            if (Value.Weight < 0)
                Result.Add("وزن واحد بار کمتر از صفر است.");
            if (string.IsNullOrWhiteSpace(Value.Weight_Type))
                Result.Add("نوع وزن وارد نشده است.");
            if (Value.Inventory_price < 0)
                Result.Add("هزینه انبارداری کمتر از صفر است.");
            if (Value.Packaging_Price < 0)
                Result.Add("هزینه بارچینی کمتر از صفر است.");
            return Result;
        }
        public static List<string> Validate(this Driver Value)
        {
            List<string> Result = new List<string>();
            if (Value == null)
                return Result;
            if (string.IsNullOrWhiteSpace(Value.ID))
                Result.Add("پلاک وارد نشده است.");
            if (string.IsNullOrWhiteSpace(Value.Name))
                Result.Add("نام وارد نشده است.");
            if (string.IsNullOrWhiteSpace(Value.Mobile))
                Result.Add("موبایل وارد نشده است.");
            if (string.IsNullOrWhiteSpace(Value.Code))
                Result.Add("کدملی وارد نشده است.");
            if (string.IsNullOrWhiteSpace(Value.Company_Name))
                Result.Add("نام شرکت وارد نشده است.");
            return Result;
        }

        public static string ValidateImageFiles(this HttpPostedFileBase[] Value)
        {
            string Result = null;
            if (Value == null)
                return Result;
            int MaxBytes = 1000000;
            string[] Types = { ".jpeg", ".png" };
            foreach (var item in Value)
            {
                if (item != null)
                {
                    string extension = Path.GetExtension(item.FileName);
                    if (item.ContentLength >= MaxBytes)
                        Result += " فایل از 1 MB بیشتر است.";
                    if (!Types.Contains(value: extension.ToLower(CultureInfo.CurrentCulture)))
                        Result += " فایل jpeg یا png قابل قبول است.";
                    if (!string.IsNullOrEmpty(Result))
                        break;
                }
            }
            return Result;
        }
    }
}