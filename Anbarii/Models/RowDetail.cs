//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Anbarii.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class RowDetail
    {
        public long LoadID_Source { get; set; }
        public long LoadID_Destination { get; set; }
        public long ID { get; set; }
        public int Count { get; set; }
        public double Weight { get; set; }
        public string Details { get; set; }
        public string WeightType { get; set; }
        public int Load_TypeID { get; set; }
        public long WherHouseID { get; set; }
    
        public virtual Load LoadD { get; set; }
        public virtual Load LoadS { get; set; }
        public virtual Type Type { get; set; }
        public virtual Row Row { get; set; }
        public virtual User User { get; set; }
    }
}