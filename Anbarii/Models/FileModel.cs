using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Anbarii.Models
{
    public class FileModel
    {
        [Required(ErrorMessage = "Please select file.")]
        [DataType(DataType.Upload)]
        [Display(Name = "Browse File")]
        public HttpPostedFileBase Files { get; set; }
    }
    public class RowInput
    {
        [MinLength(0, ErrorMessage = "کمتر از صفر قاب قبول نیست")]
        public int Count { get; set; }
        [MinLength(0, ErrorMessage = "کمتر از صفر قاب قبول نیست")]
        public long ID { get; set; }
    }
}