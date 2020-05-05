using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Anbarii.Models
{
    public class LoadReport
    {
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public string SellerName { get; set; }

        public string CustomerName { get; set; }

        public string IDIn { get; set; }
        public string IDOut { get; set; }
        public string CountIn { get; set; }
        public string CountOut { get; set; }
        public string WeightIn { get; set; }
        public string WeightOut { get; set; }
        public long WherHouseID { get; set; }
        public bool LinkType { get; set; }
    }
    public class LoadReportMOdel
    {
        public Load Model1 { get; set; }
        public List<LoadReport> Model2 { get; set; }
    }
}