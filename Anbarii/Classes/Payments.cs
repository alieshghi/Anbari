using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Anbarii.Models;

namespace Com.Azari.Components
{
    public static class Payments
    {
        //string RedirectPage = Url.Action("Success", "Home");
        private static AnbariiEntities db = new AnbariiEntities();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        public static RetunrvalueType Pay(string Controller, string Action, string Gateway, Payment Payment, int Price, Controller ContextController)
        {
            string RedirectPage = ContextController?.Url.Action(Action, Controller);
            RetunrvalueType Result = new RetunrvalueType();
            switch (Gateway)
            {
                case "Mellat":
                    Result = MellatPayment(Payment, Price, RedirectPage);
                    break;
                default:
                    Result.Message = "هیچ درگاهی انتخاب نشده";
                    break;
            }
            return Result;
        }
        public static RetunrvalueType PaymentResult(string RefId, string ResCode, long saleOrderId, long SaleReferenceId)
        {
            RetunrvalueType retunrvalueType = new RetunrvalueType();
            if (SaleReferenceId > 0)
            {
                try
                {
                    var payment = db.Payments.Where(i => i.PaymentId.Equals(saleOrderId)).FirstOrDefault();
                    switch (payment.Card.name)
                    {
                        case "Mellat":
                            retunrvalueType = MellatPaymentResult( RefId,  ResCode,  payment,  SaleReferenceId);
                            break;
                        default:
                            retunrvalueType.Message = "هیچ درگاهی انتخاب نشده";
                            break;
                    }
                }
                catch (Exception ex)
                {
                    retunrvalueType.Message = ex + "<br>" + " وضعیت:مشکلی در پرداخت بوجود آمده ، در صورتی که وجه پرداختی از حساب بانکی شما کسر شده است آن مبلغ به صورت خودکار برگشت داده خواهد شد ";
                    retunrvalueType.Write = "**************";
                    return retunrvalueType;
                }
            }
            else
            {
                //ResCode=StatusPayment
                if (!string.IsNullOrEmpty(ResCode))
                {
                    retunrvalueType.Message = PaymentResultclass.MellatResult(ResCode);
                    retunrvalueType.Write = "**************";
                }
                else
                {
                    retunrvalueType.Message = "رسید قابل قبول نیست";
                    retunrvalueType.Write = "**************";
                }
            }
            return retunrvalueType;
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        public static RetunrvalueType MellatPayment(Payment Payment, int Price, string RedirectPage)
        {
            BypassCertificateError();
            RetunrvalueType retunrvalueType = new RetunrvalueType();
            var card = db.Cards.Where(i => i.name.Equals("Mellat", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            var payment = new Anbarii.ir.shaparak.bpm.PaymentGatewayImplService();
            string result = payment.bpPayRequest(
                long.Parse(card?.TerminalID, CultureInfo.CurrentCulture),
                card?.UserName,
               card?.Password,
                (Payment?.PaymentId) ?? 0,
                Price,
                GetDate(),
                GetTime(),
                "خرید از سایت تحلیل داده",
                RedirectPage,
               0
            );
            if (result != null)
            {
                // 45zm24554654,0 
                string[] ResultArray = result.Split(',');
                if (ResultArray[0].ToString(CultureInfo.CurrentCulture) == "0")
                {
                    UpdatePayment(Payment, "-100", 0, ResultArray[1], 0);
                    NameValueCollection collection = new NameValueCollection();
                    collection.Add("RefId", ResultArray[1]);
                    retunrvalueType.Write = Helper.PreparePOSTForm("https://bpm.shaparak.ir/pgwchannel/startpay.mellat", collection);
                }
                else
                {
                    UpdatePayment(Payment, ResultArray[0].ToString(CultureInfo.CurrentCulture), 0, null, 0);
                    retunrvalueType.Message = PaymentResultclass.MellatResult(ResultArray[0]);
                }
            }
            else
            {
                retunrvalueType.Message = "امکان اتصال به درگاه بانک وجود ندارد";
            }
            payment.Dispose();
            return retunrvalueType;
        }
        public static RetunrvalueType MellatPaymentResult(string RefId, string ResCode, Payment payment, long SaleReferenceId)
        {
            RetunrvalueType retunrvalue = new RetunrvalueType();
            BypassCertificateError();

            try
            {

                var bpService = new Anbarii.ir.shaparak.bpm.PaymentGatewayImplService();
                string Result;
                Result = bpService.bpVerifyRequest(long.Parse(payment.Card.TerminalID, CultureInfo.CurrentCulture), payment.Card.UserName, payment.Card.Password, payment.PaymentId, payment.PaymentId, SaleReferenceId);
                if (!string.IsNullOrEmpty(Result))
                {
                    if (Result == "0")
                    {
                        string IQresult;
                        IQresult = bpService.bpInquiryRequest(long.Parse(payment.Card.TerminalID, CultureInfo.CurrentCulture), payment.Card.UserName, payment.Card.Password, payment.PaymentId, payment.PaymentId, SaleReferenceId);
                        if (IQresult == "0")
                        {
                            UpdatePayment(payment, Result, SaleReferenceId, RefId, 1);
                            retunrvalue.Message = "پرداخت با موفقیت انجام شد.";
                            retunrvalue.Write = SaleReferenceId.ToString(CultureInfo.CurrentCulture);
                            // پرداخت نهایی
                            string Sresult;
                            // تایید پرداخت
                            Sresult = bpService.bpSettleRequest(long.Parse(payment.Card.TerminalID, CultureInfo.CurrentCulture), payment.Card.UserName, payment.Card.Password, payment.PaymentId, payment.PaymentId, SaleReferenceId);
                            if (Sresult != null)
                            {
                                if (Sresult == "0" || Sresult == "45")
                                {
                                    //تراکنش تایید و ستل شده است 
                                }
                                else
                                {
                                    //تراکنش تایید شده ولی ستل نشده است
                                }
                            }
                        }
                        else
                        {
                            string Rvresult;
                            //عملیات برگشت دادن مبلغ
                            Rvresult = bpService.bpReversalRequest(long.Parse(payment.Card.TerminalID, CultureInfo.CurrentCulture), payment.Card.UserName, payment.Card.Password, payment.PaymentId, payment.PaymentId, SaleReferenceId);
                            retunrvalue.Message = "تراکنش بازگشت داده شد";
                            retunrvalue.Write = "**************";
                            UpdatePayment(payment, IQresult, SaleReferenceId, RefId, 0);
                        }
                    }
                    else
                    {
                        retunrvalue.Message = PaymentResultclass.MellatResult(Result);
                        retunrvalue.Write = "**************";
                        UpdatePayment(payment, Result, SaleReferenceId, RefId, 0);
                    }
                }
                else
                {
                    retunrvalue.Message = "شماره رسید قابل قبول نیست";
                    retunrvalue.Write = "**************";
                }
            }
            catch (Exception ex)
            {
                string errors = ex.Message;
                retunrvalue.Message = "مشکلی در پرداخت به وجود آمده است ، در صورتیکه وجه پرداختی از حساب بانکی شما کسر شده است آن مبلغ به صورت خودکار برگشت داده خواهد شد";
                retunrvalue.Write = "**************";
            }

            return retunrvalue;

        }
        private static void UpdatePayment(Payment Payment, string StatusPayment, long SaleRefrenceID, string RefID,
            int StatePortal = 0)
        {
            var p = db.Payments.Find(Payment);
            p.StatusPayment = StatusPayment;
            p.SaleReferenceId = SaleRefrenceID;
            p.StatePortal = StatePortal;
            if (RefID != null)
            {
                p.ReferenceNumber = RefID;
            }
            db.SaveChanges();
            db.Dispose();
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5359:Do Not Disable Certificate Validation", Justification = "<Pending>")]
        private static void BypassCertificateError()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                delegate (
                    object sender1,
                    X509Certificate certificate,
                    X509Chain chain,
                    SslPolicyErrors sslPolicyErrors)
                {
                    return true;
                };
        }
        private static string GetDate()
        {
            return DateTime.Now.Year.ToString(CultureInfo.CurrentCulture) + DateTime.Now.Month.ToString(CultureInfo.CurrentCulture).PadLeft(2, '0') +
                   DateTime.Now.Day.ToString(CultureInfo.CurrentCulture).PadLeft(2, '0');
        }
        private static string GetTime()
        {
            return DateTime.Now.Hour.ToString(CultureInfo.CurrentCulture).PadLeft(2, '0') + DateTime.Now.Minute.ToString(CultureInfo.CurrentCulture).PadLeft(2, '0') +
                   DateTime.Now.Second.ToString(CultureInfo.CurrentCulture).PadLeft(2, '0');
        }
    }
    public class RetunrvalueType
    {
        public string Message { get; set; }
        public string Write { get; set; }
    }
    public static class PaymentResultclass
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "<Pending>")]
        public static string MellatResult(string ID)
        {
            string result;
            switch (ID)
            {
                case "-100":
                    result = "پرداخت لغو شده";
                    break;
                case "0":
                    result = "تراكنش با موفقيت انجام شد";
                    break;
                case "11":
                    result = "شماره كارت نامعتبر است ";
                    break;
                case "12":
                    result = "موجودي كافي نيست ";
                    break;
                case "13":
                    result = "رمز نادرست است ";
                    break;
                case "14":
                    result = "تعداد دفعات وارد كردن رمز بيش از حد مجاز است ";
                    break;
                case "15":
                    result = "كارت نامعتبر است ";
                    break;
                case "16":
                    result = "دفعات برداشت وجه بيش از حد مجاز است ";
                    break;
                case "17":
                    result = "كاربر از انجام تراكنش منصرف شده است ";
                    break;
                case "18":
                    result = "تاريخ انقضاي كارت گذشته است ";
                    break;
                case "19":
                    result = "مبلغ برداشت وجه بيش از حد مجاز است ";
                    break;
                case "111":
                    result = "صادر كننده كارت نامعتبر است ";
                    break;
                case "112":
                    result = "خطاي سوييچ صادر كننده كارت ";
                    break;
                case "113":
                    result = "پاسخي از صادر كننده كارت دريافت نشد ";
                    break;
                case "114":
                    result = "دارنده كارت مجاز به انجام اين تراكنش نيست";
                    break;
                case "21":
                    result = "پذيرنده نامعتبر است ";
                    break;
                case "23":
                    result = "خطاي امنيتي رخ داده است ";
                    break;
                case "24":
                    result = "اطلاعات كاربري پذيرنده نامعتبر است ";
                    break;
                case "25":
                    result = "مبلغ نامعتبر است ";
                    break;
                case "31":
                    result = "پاسخ نامعتبر است ";
                    break;
                case "32":
                    result = "فرمت اطلاعات وارد شده صحيح نمي باشد ";
                    break;
                case "33":
                    result = "حساب نامعتبر است ";
                    break;
                case "34":
                    result = "خطاي سيستمي ";
                    break;
                case "35":
                    result = "تاريخ نامعتبر است ";
                    break;
                case "41":
                    result = "شماره درخواست تكراري است ، دوباره تلاش کنید";
                    break;
                case "42":
                    result = "يافت نشد  Sale تراكنش";
                    break;
                case "43":
                    result = "داده شده است  Verify قبلا درخواست";
                    break;
                case "44":
                    result = "يافت نشد  Verfiy درخواست";
                    break;
                case "45":
                    result = "شده است  Settle تراكنش";
                    break;
                case "46":
                    result = "نشده است  Settle تراكنش";
                    break;
                case "47":
                    result = "يافت نشد  Settle تراكنش";
                    break;
                case "48":
                    result = "شده است  Reverse تراكنش";
                    break;
                case "49":
                    result = "يافت نشد  Refund تراكنش";
                    break;
                case "412":
                    result = "شناسه قبض نادرست است ";
                    break;
                case "413":
                    result = "شناسه پرداخت نادرست است ";
                    break;
                case "414":
                    result = "سازمان صادر كننده قبض نامعتبر است ";
                    break;
                case "415":
                    result = "زمان جلسه كاري به پايان رسيده است ";
                    break;
                case "416":
                    result = "خطا در ثبت اطلاعات ";
                    break;
                case "417":
                    result = "شناسه پرداخت كننده نامعتبر است ";
                    break;
                case "418":
                    result = "اشكال در تعريف اطلاعات مشتري ";
                    break;
                case "419":
                    result = "تعداد دفعات ورود اطلاعات از حد مجاز گذشته است ";
                    break;
                case "421":
                    result = "نامعتبر است  IP";
                    break;
                case "51":
                    result = "تراكنش تكراري است ";
                    break;
                case "54":
                    result = "تراكنش مرجع موجود نيست ";
                    break;
                case "55":
                    result = "تراكنش نامعتبر است ";
                    break;
                case "61":
                    result = "خطا در واريز ";
                    break;
                default:
                    result = string.Empty;
                    break;
            }
            return result;
        }
    }
    public static class Helper
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "<Pending>")]
        public static string PreparePOSTForm(string url, NameValueCollection data)
        {
            string formID = "PostForm";
            StringBuilder strForm = new StringBuilder();
            strForm.Append("<form id=\"" + formID + "\" name=\"" + formID + "\" action=\"" + url + "\" method=\"POST\">");
            foreach (string key in data)
            {
                strForm.Append("<input type=\"hidden\" name=\"" + key + "\" value=\"" + data[key] + "\">");
            }
            strForm.Append("< /form >");
            StringBuilder strScript = new StringBuilder();
            strScript.Append("<script language='javascript'>");
            strScript.Append("var v" + formID + " = document." + formID + ";");
            strScript.Append("v" + formID + ".submit();");
            strScript.Append("</script>");
            return strForm.ToString() + strScript.ToString();
        }
    }

}