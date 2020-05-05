using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Globalization;
using Anbarii.Models;
using Anbarii.Classes;
using Anbarii.Classes.Values;
using System.Data.Entity.Validation;
using System.Diagnostics;
using Com.Azari.Components;

namespace Anbarii.Controllers
{
    public class PortalController : Controller
    {
        public const string SController = "Portal";
        public const string SIndex = "Index";
        public const string SProducts = "Products";
        public const string SProduct = "Product";
        public const string SAddProduct = "AddProduct";
        public const string SExport = "Export";
        public const string SExportPrint = "ExportPrint";
        public const string SRow = "Row";
        public const string SRowIn = "RowIn";
        public const string SRowPrint = "RowPrint";
        public const string SReceiptPrint = "ReceiptPrint";
        public const string SPeople = "People";
        public const string SPerson = "Person";
        public const string SContact = "Contact";
        public const string SProfile = "Profile";
        public const string SChangePassword = "ChangePassword";
        public const string SLogOut = "LogOut";
        public const string SPayment = "Payment";
        public const string SPaymentResult = "PaymentResult";
        public const string SMassages = "Massages";
        public const string SMassage = "Massage";
        public const string SMessageCategories = "MessageCategories";
        public const string STypes = "Types";
        public const string SUsers = "Users";
        public const string SAddUser = "AddUser";
        public const string STickets = "Tickets";
        public const string SSendNotification = "SendNotification";
        public const string SSaveToken = "SaveToken";
        public const string SSaveYear = "SaveYear";

        private AnbariiEntities db = new AnbariiEntities();
        private ActionResult Return()
        {
            return RedirectToAction(HomeController.SIndex, HomeController.SController);
        }
        private bool Permission(int[] RoleArray)
        {
            var result = Members.Login(RoleArray);
            if (!result)
            {
                return false;
            }
            return true;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveToken(string Token)
        {
            if (!string.IsNullOrEmpty(Token))
            {
                Token token = db.Tokens.Where(i => i.NotificatioToken.Equals(Token, StringComparison.Ordinal)).FirstOrDefault();
                if (token == null)
                {
                    token = new Token()
                    {
                        NotificatioToken = Token,
                        UserID = Members.User.ID
                    };
                    db.Tokens.Add(token);
                    db.SaveChanges();
                    return Json(true);
                }
                else
                {
                    if (!token.UserID.Equals(Members.User.ID))
                    {
                        token.UserID = Members.User.ID;
                        db.SaveChanges();
                    }
                    return Json(true);
                }
            }
            return Json(false);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveYear(int Year)
        {
            if (Year > 0 && Year < 9999)
            {
                Members.Year = Year;
            }
            return Json(false);
        }
        [HttpGet]
        public ActionResult Index(long ID = 0)
        {
            if (!Permission(RolesInt.all))
            {
                return Return();
            }
            var montadmin = db.Rows.Where(i => i.Year.Equals(Members.Year)).OrderByDescending(i => i.Date);
            if (Members.User.RoleID.Equals(RolesInt.Anbar))
            {
                var mont = db.Rows.Where(i => i.Year.Equals(Members.Year) && i.WherHouseID.Equals(Members.User.ID)).OrderByDescending(i => i.Date);
                return View(SIndex + "_" + RolesString.Anbar, mont.ToList());
            }
            if (Members.User.RoleID.Equals(RolesInt.Tajer))
            {
                IQueryable<Load> mont = db.Loads.Where(i => i.OwnerID.Equals(Members.User.ID) && i.Count > 0).OrderByDescending(i => i.Name);
                if (ID > 0)
                {
                    mont = mont.Where(i => i.WherhouseID.Equals(ID));
                }
                return View(SIndex + "_" + RolesString.Tajer, mont.ToList());
            }
            return View(montadmin.ToList());
        }
        [HttpGet]
        public ActionResult Products(string search, long ID = 0)
        {
            if (!Permission(RolesInt.all))
            {
                return Return();
            }
            IQueryable<Load> mont = db.Loads;
            if (ID > 0)
                mont = mont.Where(i => i.WherhouseID == ID || i.OwnerID == ID);
            if (Members.User.RoleID.Equals(RolesInt.Anbar))
                mont = mont.Where(i => i.WherhouseID.Equals(Members.User.ID));
            if (Members.User.RoleID.Equals(RolesInt.Tajer))
                mont = mont.Where(i => i.OwnerID.Equals(Members.User.ID));
            if (!string.IsNullOrEmpty(search))
                mont = mont.Where(i => i.Name.Contains(search) || i.Owner.Name_Company.Contains(search));
            return View(mont);
        }
        [HttpGet]
        public ActionResult Product(long ID = 0)
        {
            if (!Permission(RolesInt.all))
            {
                return Return();
            }
            IQueryable<Load> mont = db.Loads.Where(i => i.ID.Equals(ID));
            if (Members.User.RoleID.Equals(RolesInt.Anbar))
                mont = mont.Where(i => i.WherhouseID.Equals(Members.User.ID));
            if (Members.User.RoleID.Equals(RolesInt.Tajer))
                mont = mont.Where(i => i.OwnerID.Equals(Members.User.ID));
            if (mont.Any())
            {
                Load OutPut = mont.FirstOrDefault();
                var loaddetails = db.RowDetails;
                var outload = loaddetails.Where(i => (!i.LoadID_Destination.Equals(OutPut.ID) && i.LoadID_Source.Equals(OutPut.ID)) || (i.LoadID_Destination.Equals(OutPut.ID) && i.LoadID_Source.Equals(OutPut.ID))).ToList();
                List<LoadReport> LoadReports = outload.Select(s => new LoadReport()
                {
                    LinkType = false,
                    SellerName = s.LoadS.Owner.Name_Company,
                    CustomerName = s.LoadD.Owner.Name_Company,
                    IDIn = "-",
                    IDOut = s.ID.ToString(CultureInfo.CurrentCulture),
                    Date = s.Row.Date,
                    WherHouseID = s.Row.WherHouseID,
                    CountIn = "-",
                    CountOut = s.Count.ToString(CultureInfo.CurrentCulture) + " " + s.Type.Name,
                    Name = s.Details,
                    WeightIn = "-",
                    WeightOut = (s.Weight * s.Count).ToString(CultureInfo.CurrentCulture) + " " + s.WeightType,
                }).ToList();
                var inload = loaddetails.Where(i => i.LoadID_Destination.Equals(OutPut.ID) && !i.LoadID_Source.Equals(OutPut.ID)).ToList();
                LoadReports.AddRange(inload.Select(s => new LoadReport()
                {
                    LinkType = false,
                    SellerName = s.LoadS.Owner.Name_Company,
                    CustomerName = s.LoadD.Owner.Name_Company,
                    IDIn = s.ID.ToString(CultureInfo.CurrentCulture),
                    IDOut = "-",
                    Date = s.Row.Date,
                    WherHouseID = s.Row.WherHouseID,
                    CountIn = s.Count.ToString(CultureInfo.CurrentCulture) + " " + s.Type.Name,
                    CountOut = "-",
                    Name = s.Details,
                    WeightIn = (s.Weight* s.Count).ToString(CultureInfo.CurrentCulture) + " " + s.WeightType,
                    WeightOut = "-",
                }).ToList());
                LoadReports.AddRange(db.ReceiptDetails
                                  .Where(i => i.LoadID.Equals(OutPut.ID))
                                  .ToList().Select(s => new LoadReport()
                                  {
                                      SellerName = "-",
                                      CustomerName = "-",
                                      LinkType = true,
                                      IDOut = "-",
                                      IDIn = s.ID.ToString(CultureInfo.CurrentCulture),
                                      WherHouseID = s.Receipt.WherHouseID,
                                      Date = s.Receipt.Date,
                                      CountOut = "-",
                                      CountIn = s.Count.ToString(CultureInfo.CurrentCulture) + " " + s.Type.Name,
                                      Name = s.Receipt.Name,
                                      WeightOut = "-",
                                      WeightIn = (s.Weight * s.Count).ToString(CultureInfo.CurrentCulture) + " " + s.WeightType,
                                  }).ToList());


                return View(new LoadReportMOdel { Model1 = OutPut, Model2 = LoadReports });
            }
            else
            {
                return Return();
            }
        }
        [HttpGet]
        public ActionResult AddProduct()
        {
            if (!Permission(new int[] { RolesInt.Anbar }))
            {
                return Return();
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public ActionResult AddProduct(Receipt receipt, long OwnerID)
        {
            if (!Permission(new int[] { RolesInt.Admin, RolesInt.Anbar }))
                return Return();
            if (receipt == null || OwnerID <= 0)
            {
                ModelState.AddModelError(string.Empty, "مقادیر اجباری فرم را وارد نمایید.");
                return View(new Load());
            }
            else
            {
                List<string> Errorlist = receipt.Validate();
                Errorlist.Union(receipt.Driver.Validate());
                Errorlist.Union(OwnerID.ValidateDestination());
                if (!Errorlist.Any())
                {
                    receipt.DriverID = receipt.Driver.ID;
                    if (!db.Drivers.Where(i => i.ID.Equals(receipt.Driver.ID, StringComparison.OrdinalIgnoreCase)).Any())
                    {
                        receipt.Driver.Date = DateTime.Now;
                    }
                    else
                    {
                        receipt.Driver = null;
                    }
                    receipt.ID = (db.Receipts.Where(i => i.WherHouseID.Equals(Members.User.ID)).Max(i => (long?)i.ID) ?? default(long)) + 1;
                    receipt.WherHouseID = Members.User.ID;
                    receipt.Date = DateTime.Now;
                    receipt.Year = Members.Year;
                    foreach (var item in receipt.ReceiptDetails)
                    {
                        Load load = db.Loads
                            .Where(i => i.Name.Equals(item.Details, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        if (load == null)
                        {
                            load = new Load()
                            {
                                Change_date = DateTime.Now,
                                Weight = item.Weight,
                                Count = item.Count,
                                Add_Date = DateTime.Now,
                                Details = item.Details,
                                ID = receipt.ID,
                                Input_Name = item.Details,
                                Inventory_price = 0,
                                Load_TypeID = item.Load_TypeID,
                                Name = item.Details,
                                OwnerID = OwnerID,
                                Packaging_Price = 0,
                                Pic1 = Strings.ImageEmpty,
                                Pic2 = Strings.ImageEmpty,
                                Pic3 = Strings.ImageEmpty,
                                Pic4 = Strings.ImageEmpty,
                                Pic5 = Strings.ImageEmpty,
                                Weight_Type = item.WeightType,
                                WherhouseID = receipt.WherHouseID
                            };
                            db.Loads.Add(load);

                        }
                        else
                        {
                            load.Count += item.Count;
                            load.Weight = item.Weight;
                            load.Change_date = DateTime.Now;
                        }
                        item.Load = load;
                        item.ID = receipt.ID;
                        item.WherHouseID = receipt.WherHouseID;

                    }
                    db.Receipts.Add(receipt);
                    db.SaveChanges();
                    db.Dispose();
                    db = new AnbariiEntities();
                    receipt = db.Receipts.Where(i => i.ID.Equals(receipt.ID) && i.WherHouseID.Equals(receipt.WherHouseID)).FirstOrDefault();
                    return View(SReceiptPrint, receipt);
                }
                else
                    foreach (string item in Errorlist)
                        ModelState.AddModelError(item, item);
                return View(receipt);
            }
        }
        [HttpGet]
        public ActionResult ReceiptPrint(long ID, long WherHouseID)
        {
            if (!Permission(RolesInt.all))
            {
                return Return();
            }
            Receipt mont = db.Receipts.Where(i => i.ID.Equals(ID) && i.WherHouseID.Equals(WherHouseID)).FirstOrDefault();
            if (mont != null)
            {
                return View(mont);
            }
            else
            {
                return Return();
            }
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        //public ActionResult AddProduct(ReceiptDetail receiptDetail, HttpPostedFileBase[] files)
        //{
        //    if (!Permission(new int[] { RolesInt.Admin, RolesInt.Anbar }))
        //        return Return();
        //    if (Input == null)
        //    {
        //        ModelState.AddModelError(string.Empty, "مقادیر اجباری فرم را وارد نمایید.");
        //        return View(new Load());
        //    }
        //    else
        //    {
        //        List<string> Errorlist = Input.Validate();
        //        if (files != null)
        //        {
        //            var res = files.ValidateImageFiles();
        //            if (res != null)
        //            {
        //                Errorlist.Add(res);
        //            }
        //        }
        //        if (!Errorlist.Any())
        //        {
        //            if (files != null)
        //            {
        //                int Identify = 0;
        //                foreach (HttpPostedFileBase file in files)
        //                {

        //                    Identify++;
        //                    try
        //                    {
        //                        string InputFileName = "";
        //                        if (file != null)
        //                        {
        //                            InputFileName = DateTime.Now.ToStringTimestamp() + Path.GetExtension(file.FileName);
        //                            string ServerSavePath = Path.Combine(Server.MapPath(Strings.ImagePath) + InputFileName);
        //                            //Save file to server folder  
        //                            file.SaveAs(ServerSavePath);
        //                        }
        //                        if (Identify.Equals(1))
        //                            Input.Pic1 = InputFileName;
        //                        if (Identify.Equals(2))
        //                            Input.Pic2 = InputFileName;
        //                        if (Identify.Equals(3))
        //                            Input.Pic3 = InputFileName;
        //                        if (Identify.Equals(4))
        //                            Input.Pic4 = InputFileName;
        //                        if (Identify.Equals(5))
        //                            Input.Pic5 = InputFileName;
        //                    }
        //                    catch (Exception ex)
        //                    {

        //                        ModelState.AddModelError(string.Empty, ex.Message);
        //                    }

        //                }
        //            }
        //            long ID = db.Load_Users.Where(i => i.LoadS.WherhouseID.Equals(Members.User.ID)).Max(i => i.ID) + 1;
        //            Input.Add_Date = DateTime.Now;
        //            Input.Change_date = DateTime.Now;
        //            Input.WherhouseID = Members.User.ID;
        //            Input.Input_Name = string.Empty;
        //            try
        //            {
        //                db.Loads.Add(Input);
        //                db.Load_Users.Add(new Load_Users()
        //                {
        //                    Count = Input.Count,
        //                    Date = Input.Add_Date,
        //                    Details = Input.Details,
        //                    DriverID = "",
        //                    ID = ID,
        //                    LoadID_Destination = Input.ID,
        //                    LoadID_Source = Input.ID,
        //                    Load_TypeID = Input.Load_TypeID,
        //                    Weight = Input.Weight,
        //                    WeightType = Input.Weight_Type
        //                });
        //                db.SaveChanges();
        //                Input = db.Loads.Where(i => i.ID.Equals(Input.ID)).FirstOrDefault();
        //            }
        //            catch (DbEntityValidationException e)
        //            {
        //                foreach (var eve in e.EntityValidationErrors)
        //                {
        //                    Debug.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
        //                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
        //                    foreach (var ve in eve.ValidationErrors)
        //                    {
        //                        Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
        //                            ve.PropertyName, ve.ErrorMessage);
        //                    }
        //                }
        //            }
        //        }
        //        else
        //            foreach (string item in Errorlist)
        //                ModelState.AddModelError(item, item);

        //        return View(Input);
        //    }
        //}
        [HttpGet]
        public ActionResult RowPrint(long ID, long WherHouseID)
        {
            if (!Permission(RolesInt.all))
            {
                return Return();
            }
            Row mont = db.Rows.Where(i => i.RowID.Equals(ID) && i.WherHouseID.Equals(WherHouseID)).FirstOrDefault();
            if (mont != null)
            {
                return View(mont);
            }
            else
            {
                return Return();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
        public ActionResult RowIn(int[] Count, long[] ID)
        {
            if (!Permission(new int[] { RolesInt.Tajer }))
                return Return();
            if (Count == null && ID == null)
            {
                ModelState.AddModelError(string.Empty, "مقادیر اجباری فرم را وارد نمایید.");
                return View();
            }
            else
            {
                List<RowInput> Input = new List<RowInput>();
                if (Count != null && ID != null)
                    for (var Identifier = 0; Identifier < Count?.Length; Identifier++)
                    {
                        if (Count[Identifier] > 0)
                            Input.Add(new RowInput { Count = Count[Identifier], ID = ID[Identifier] });
                    }
                if (Input.Count.Equals(0))
                {
                    return Return();
                }
                List<string> Errorlist = Input.Validate();
                if (!Errorlist.Any())
                {
                    Session["RowInput"] = Input;
                    var whereid = Input.First().ID;
                    TempData["WherHouseID"] = db.Loads.Where(i => i.ID.Equals(whereid)).First().WherhouseID;
                    return View(SRow, new Row
                    {
                        WherHouseID = db.Loads.Where(i => i.ID.Equals(whereid)).First().WherhouseID,
                    });
                }
                else
                    foreach (string item in Errorlist)
                        ModelState.AddModelError(item, item);

                return View();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
        public ActionResult Row(Row Input, long DestinationID)
        {
            if (!Permission(new int[] { RolesInt.Tajer }))
                return Return();
            if (Input == null && DestinationID < 0)
            {
                ModelState.AddModelError(string.Empty, "مقادیر اجباری فرم را وارد نمایید.");
                return View();
            }
            if (DestinationID.Equals(0))
            {
                DestinationID = Members.User.ID;
            }

            List<string> Errorlist = Input.Validate();
            Errorlist.Union(DestinationID.ValidateDestination());
            if (!Errorlist.Any())
            {
                List<RowInput> rowInputs = (List<RowInput>)Session["RowInput"];
                var WID = (long)TempData["WherHouseID"];
                Input.RowID = (db.Rows.Where(i => i.WherHouseID.Equals(WID)).Max(i => (long?)i.RowID) ?? default(long)) + 1;
                Input.WherHouseID = WID;
                Input.Date = DateTime.Now;
                Input.invoiceID = 0;
                Input.Year = Members.Year;
                if (string.IsNullOrEmpty(Input.Name))
                    Input.Name = string.Empty;
                db.Rows.Add(Input);
                foreach (var item in rowInputs)
                {
                    Load Source = db.Loads.Where(i => i.ID.Equals(item.ID)).First();
                    Load Destination = Source;
                    if (!DestinationID.Equals(Members.User.ID))
                    {
                        Members.UpdateUser();
                        Destination = db.Loads
                            .Where(i => i.Name.Equals(Source.Name, StringComparison.OrdinalIgnoreCase) && i.Load_TypeID.Equals(Source.Load_TypeID) && i.OwnerID.Equals(DestinationID)).FirstOrDefault();
                        if (Destination == null)
                        {
                            Destination = new Load
                            {
                                Add_Date = DateTime.Now,
                                Change_date = DateTime.Now,
                                Count = item.Count,
                                Input_Name = Source.Name,
                                Name = Source.Name,
                                Details = Source.Details,
                                Inventory_price = 0,
                                Load_TypeID = Source.Load_TypeID,
                                OwnerID = DestinationID,
                                Packaging_Price = 0,
                                Pic1 = Source.Pic1,
                                Pic2 = Source.Pic2,
                                Pic3 = Source.Pic3,
                                Pic4 = Source.Pic4,
                                Pic5 = Source.Pic5,
                                Weight = Source.Weight,
                                Weight_Type = Source.Weight_Type,
                                WherhouseID = Input.WherHouseID,

                            };
                            db.Loads.Add(Destination);
                        }
                        else
                        {
                            Destination.Change_date = DateTime.Now;
                            Destination.Count += item.Count;
                        }
                    }
                    else
                    {
                        Input.invoiceID = db.Rows.Where(i => i.WherHouseID.Equals(Input.WherHouseID)).Max(i => i.invoiceID) + 1;
                    }
                    Source.Count -= item.Count;
                    db.SaveChanges();
                    db.RowDetails.Add(new RowDetail
                    {
                        Count = item.Count,
                        Details = Input.Name,
                        ID = Input.RowID,
                        WherHouseID = Input.WherHouseID,
                        LoadID_Destination = Destination.ID,
                        LoadID_Source = Source.ID,
                        Load_TypeID = Source.Load_TypeID,
                        Weight = Source.Weight,
                        WeightType = Source.Weight_Type,
                    });
                    db.SaveChanges();

                }
                db.Dispose();
                db = new AnbariiEntities();
                Input = db.Rows.Where(i => i.RowID.Equals(Input.RowID) && i.WherHouseID.Equals(Input.WherHouseID)).FirstOrDefault();
                return View(SRowPrint, Input);

            }
            else
                foreach (string item in Errorlist)
                    ModelState.AddModelError(item, item);

            return View(Input);

        }
        [HttpGet]
        public ActionResult People()
        {
            if (!Permission(RolesInt.all))
            {
                return Return();
            }
            IQueryable<User> mont = db.Users;
            if (Members.User.RoleID.Equals(RolesInt.Anbar) || Members.User.RoleID.Equals(RolesInt.Tajer))
                mont = Members.User.UsersR.AsQueryable();
            return View(mont);
        }
        [HttpGet]
        public ActionResult Massages(long ID = 0)
        {
            if (!Permission(RolesInt.all))
            {
                return Return();
            }
            if (ID.Equals(Members.User.ID) && ID > 0)
                return Return();
            IQueryable<Notification> mont = db.Notifications;
            if (ID > 0)
                mont = mont.Where(i => i.UserId.Equals(ID));
            else if (Members.User.RoleID.Equals(RolesInt.Anbar))
            {
                List<Notification> mont1 = new List<Notification>();
                foreach (var item in Members.User.Users.Select(i => i.Notifications.ToList()))
                {
                    mont1.AddRange(item);
                }
                mont = mont.AsQueryable();
            }
            else if (Members.User.RoleID.Equals(RolesInt.Tajer))
                mont = mont.Where(i => i.UserId.Equals(Members.User.ID));
            return View(mont.OrderBy(i => i.Date).ToList());
        }
        [HttpGet]
        public ActionResult Massage(long ID)
        {
            if (!Permission(RolesInt.all))
            {
                return Return();
            }
            IQueryable<Notification> mont = db.Notifications.Where(i => i.ID.Equals(ID));
            if (Members.User.RoleID.Equals(RolesInt.Anbar) || Members.User.RoleID.Equals(RolesInt.Tajer))
                mont = mont.Where(i => i.UserId.Equals(Members.User.ID));
            if (mont.Any())
            {
                var notif = mont.FirstOrDefault();
                notif.Seen = true;
                db.SaveChanges();
                return View(notif);
            }
            else
                return Return();
        }
        [HttpGet]
        public ActionResult SendNotification(bool PodCast = false)
        {
            if (!Permission(new int[] { RolesInt.Admin, RolesInt.Anbar }))
            {
                return Return();
            }
            return View(PodCast);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendNotification(Notification notif)
        {
            if (!Permission(new int[] { RolesInt.Admin, RolesInt.Anbar }))
            {
                return Return();
            }
            if (notif == null)
            {
                return Return();
            }
            bool PodCast = false;
            List<string> Errorlist = notif.Validate();
            if (!Errorlist.Any())
            {
                Dictionary<string, string> tokenlinks = new Dictionary<string, string>();
                if (notif.CategoryID.Equals(1))
                {
                    var users = db.Users.AsQueryable();
                    if (Members.User.RoleID.Equals(RolesInt.Anbar))
                        users = Members.User.Users.AsQueryable();
                    foreach (var item in users)
                    {
                        var notification = new Notification()
                        {
                            Body = notif.Body,
                            CategoryID = notif.CategoryID,
                            Date = DateTime.Now,
                            Pariority = 0,
                            Seen = false,
                            Subject = notif.Subject,
                            UserId = item.ID
                        };
                        db.Notifications.Add(notification);
                        db.SaveChanges();
                        string url = Url.Action(SMassage, SController, new { notification.ID });
                        foreach (var tok in item.Tokens)
                        {
                            tokenlinks.Add(tok.NotificatioToken, url);
                        }
                    }
                    PodCast = true;
                }
                else
                {
                    var mont = db.Users.Where(i => i.ID.Equals(notif.UserId));
                    if (Members.User.RoleID.Equals(RolesInt.Anbar))
                        mont = mont.Where(i => i.Users.Where(j => j.ID.Equals(Members.User.ID)).Any());
                    var usermot = mont.FirstOrDefault();
                    if (usermot.Equals(null))
                        return Return();
                    var notification = new Notification()
                    {
                        Body = notif.Body,
                        CategoryID = notif.CategoryID,
                        Date = DateTime.Now,
                        Pariority = 0,
                        Seen = false,
                        Subject = notif.Subject,
                        UserId = usermot.ID
                    };
                    db.Notifications.Add(notification);
                    db.SaveChanges();
                    string url = Url.Action(SMassage, SController, new { notification.ID });
                    foreach (var tok in usermot.Tokens)
                    {
                        tokenlinks.Add(tok.NotificatioToken, url);
                    }

                    PodCast = true;
                }
                TempData["ChangePass"] = NotificationHandler.MultiCast(tokenlinks, notif.Subject, notif.Body);
            }
            else
                foreach (string item in Errorlist)
                    ModelState.AddModelError(item, item);
            return View(PodCast);

        }
        [HttpGet]
        public ActionResult Users()
        {
            if (!Permission(new int[] { RolesInt.Admin }))
            {
                return Return();
            }
            IQueryable<User> mont = db.Users;
            return View(mont);
        }
        [HttpGet]
        public ActionResult AddUser()
        {
            if (!Permission(new int[] { RolesInt.Admin }))
            {
                return Return();
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUser(User user)
        {
            if (!Permission(RolesInt.all))
            {
                return Return();
            }
            if (user == null)
                return Return();
            List<string> Errorlist = user.Validate();
            if (!Errorlist.Any())
            {
                var Editeuser = db.Users.Where(i => i.ID.Equals(user.ID)).FirstOrDefault();
                if (Editeuser.Equals(null))
                {
                    if (!Permission(new int[] { RolesInt.Admin }))
                    {
                        return Return();
                    }
                    user.Password = user.Password.GetMd5Hash();
                    user.Token = "";
                    user.TryCount = 0;
                    user.LastLoginDate = DateTime.Now;
                    user.Edite = false;
                    user.Validate = false;
                    db.Users.Add(user);
                    db.SaveChanges();
                    user.Token = user.ID.ToString(CultureInfo.CurrentCulture).GetMd5Hash();
                    db.SaveChanges();
                    return View(SPerson, user);
                }
                else
                {
                    if (user.RoleID > 0 && Permission(new int[] { RolesInt.Admin }))
                        Editeuser.RoleID = user.RoleID;
                    Editeuser.Name_Company = user.Name_Company;
                    Editeuser.Name = user.Name;
                    Editeuser.Mobile = user.Mobile;
                    Editeuser.Phone = user.Phone;
                    Editeuser.Code = user.Code;
                    Editeuser.Economic_Code = user.Economic_Code;
                    Editeuser.Adress = user.Adress;
                    Editeuser.Code_Posti = user.Code_Posti;
                    if (!Permission(new int[] { RolesInt.Admin }))
                    {
                        Editeuser.Edite = false;
                    }
                    db.SaveChanges();
                    if (user.RoleID > 0 && Permission(new int[] { RolesInt.Admin }))
                    {
                        Members.UpdateUser();
                        return View(SPerson, user);
                    }
                    else
                        return View(SProfile, user);
                }
            }
            else
                foreach (string item in Errorlist)
                    ModelState.AddModelError(item, item);

            return View();
        }
        [HttpGet]
        public ActionResult Person(long ID = 0)
        {
            if (!Permission(new int[] { RolesInt.Admin }))
            {
                return Return();
            }
            IQueryable<User> mont = db.Users.Where(i => i.ID.Equals(ID));
            if (mont.Any())
            {
                return View(mont.FirstOrDefault());
            }
            else
            {
                return Return();
            }
        }
        [HttpGet]
        public new ActionResult Profile()
        {
            if (!Permission(new int[] { RolesInt.Admin, RolesInt.Anbar, RolesInt.Tajer }))
            {
                return Return();
            }
            return View(db.Users.Where(i => i.ID.Equals(Members.User.ID)).FirstOrDefault());
        }

        [HttpGet]
        public ActionResult Contact()
        {
            if (!Permission(RolesInt.all))
            {
                return Return();
            }
            return View();

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(string Token)
        {
            if (!Permission(RolesInt.all))
                return Return();
            if (string.IsNullOrWhiteSpace(Token))
            {
                ModelState.AddModelError(string.Empty, "مقدار  توکت را وارد نمایید.");
                return View();
            }

            User user = db.Users.Where(i => i.Token.Equals(Token, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (user != null)
            {
                if (!Members.User.Users.Contains(user))
                {
                    try
                    {
                        User owner = db.Users.Where(i => i.ID.Equals(Members.User.ID)).FirstOrDefault();
                        owner.Users.Add(user);
                        owner.UsersR.Add(user);
                        db.SaveChanges();
                        Members.UpdateUser();
                        return View(user);
                    }
                    catch (DbEntityValidationException e)
                    {
                        foreach (var eve in e.EntityValidationErrors)
                        {
                            Debug.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                eve.Entry.Entity.GetType().Name, eve.Entry.State);
                            foreach (var ve in eve.ValidationErrors)
                            {
                                Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                    ve.PropertyName, ve.ErrorMessage);
                            }
                        }
                        throw;
                    }
                }
                else
                    ModelState.AddModelError(string.Empty, "توکن معتبر نیست.");

            }
            else
                ModelState.AddModelError(string.Empty, "توکن معتبر نیست.");

            return View();

        }
        [HttpGet]
        public ActionResult Export(long ID)
        {
            if (!Permission(new int[] { RolesInt.Anbar }))
            {
                return Return();
            }
            var mont = db.Rows.Where(i => i.RowID.Equals(ID) && i.WherHouseID.Equals(Members.User.ID)).FirstOrDefault();
            if (mont == null)
                return Return();
            return View(mont);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
        public ActionResult Export(long RowID, Driver driver, int Packaging_Price = 0, int Inventory_price = 0)
        {
            if (!Permission(new int[] { RolesInt.Anbar }))
            {
                return Return();
            }
            if (RowID < 0 || driver == null)
            {
                ModelState.AddModelError(string.Empty, "مقدار  توکت را وارد نمایید.");
                return View();
            }
            var row = db.Rows.Where(i => i.RowID.Equals(RowID) && i.WherHouseID.Equals(Members.User.ID) && i.invoiceID > 0).FirstOrDefault();
            if (row == null)
                return Return();
            List<string> Errorlist = driver.Validate();
            if (!Errorlist.Any())
            {
                if (!db.Drivers.Any(i => i.ID.Equals(driver.ID, StringComparison.OrdinalIgnoreCase)))
                {
                    driver.Date = DateTime.Now;
                    db.Drivers.Add(driver);
                    db.SaveChanges();

                }
                row.Packaging_Price = Packaging_Price;
                row.Inventory_price = Inventory_price;
                row.DriverID = driver.ID;
                db.SaveChanges();
                db.Dispose();
                db = new AnbariiEntities();
                row = db.Rows.Where(i => i.RowID.Equals(row.RowID) && i.WherHouseID.Equals(row.WherHouseID)).FirstOrDefault();
                return View(SExportPrint, row);
            }
            else
                foreach (string item in Errorlist)
                    ModelState.AddModelError(item, item);
            return View();

        }
        [HttpGet]
        public ActionResult ExportPrint(long ID, long WherHouseID)
        {
            if (!Permission(new int[] { RolesInt.Anbar }))
            {
                return Return();
            }
            var mont = db.Rows.Where(i => i.RowID.Equals(ID) && i.WherHouseID.Equals(WherHouseID)).FirstOrDefault();
            if (mont == null)
                return Return();
            return View(mont);

        }
        [HttpGet]
        public ActionResult Driver(string ID)
        {
            if (!Permission(RolesInt.all))
            {
                return Json(null);
            }
            var mont = db.Drivers.Where(i => i.ID.Contains(ID)).Select(s => new
            {
                s.ID,
                s.Name,
                s.Mobile,
                s.Code,
                s.Company_Name
            }).FirstOrDefault();
            if (mont == null)
                return Json(null);
            return Json(mont, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult Types()
        {
            if (!Permission(new int[] { RolesInt.Admin }))
            {
                return Return();
            }
            return View(db.Types.ToList());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Types(string name)
        {
            if (!Permission(new int[] { RolesInt.Admin }))
            {
                return Return();
            }
            Models.Type type = new Models.Type()
            {
                Name = name,
                Price = 0,
            };
            db.Types.Add(type);
            db.SaveChanges();
            TempData["ChangePass"] = true;
            return View(db.Types.ToList());
        }
        [HttpGet]
        public ActionResult MessageCategories()
        {
            if (!Permission(new int[] { RolesInt.Admin }))
            {
                return Return();
            }
            return View(db.NotificationCategories.ToList());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MessageCategories(string name)
        {
            if (!Permission(new int[] { RolesInt.Admin }))
            {
                return Return();
            }
            NotificationCategory type = new Models.NotificationCategory()
            {
                Name = name,
            };
            db.NotificationCategories.Add(type);
            db.SaveChanges();
            TempData["ChangePass"] = true;
            return View(db.NotificationCategories.ToList());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Payment()
        {
            var result = Payments.Pay(SController, SPaymentResult, "Mellat", new Payment(), 100, this);
            return View(result);
        }
        [HttpPost]
        public ActionResult PaymentResult(string RefId, string ResCode, long saleOrderId, long SaleReferenceId)
        {
            var result = Payments.PaymentResult(RefId, ResCode, saleOrderId, SaleReferenceId);
            return View(result);
        }
        /// <summary>
        ///  تغییر رمز
        /// </summary>
        /// <param name="oldpass">رمزعبور قدیم</param>
        /// <param name="newpass">رمزعبور جدید</param>
        /// <param name="confirm">تایید رمزعبور</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string oldpass = "", string newpass = "", string confirm = "")
        {

            if (!Permission(new int[] { RolesInt.Admin, RolesInt.Anbar, RolesInt.Tajer }))
            {
                return Return();
            }
            if (!string.IsNullOrEmpty(oldpass) && !string.IsNullOrEmpty(newpass) && !string.IsNullOrEmpty(confirm))
            {
                TempData["ChangePass"] = Members.Changepass(oldpass, newpass, confirm);
                return View(SProfile, Members.User);
            }
            else
            {
                return View(SProfile, Members.User);
            }
        }
        /// <summary>
        /// خروج
        /// </summary>
        [HttpGet]
        public ActionResult LogOut()
        {
            Members.Logout();
            return RedirectToAction(HomeController.SIndex, HomeController.SController);
        }
    }
}