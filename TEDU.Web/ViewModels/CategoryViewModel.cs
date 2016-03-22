﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEDU.Web.ViewModels
{
    [Serializable]
    public class CategoryViewModel
    {
        public int ID { set; get; }

        public string Name { set; get; }

        public string Alias { set; get; }

        public int? ParentID { set; get; }

        public DateTime? CreatedDate { set; get; }

        public string CreatedBy { set; get; }

        public string LastModifiedBy { set; get; }

        public DateTime? LastModifiedDate { set; get; }

        public bool Status { set; get; }
    }
}