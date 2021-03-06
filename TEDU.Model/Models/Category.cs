﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TEDU.Model
{
    [Table("Categories")]
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { set; get; }

        [Column(TypeName = "nvarchar")]
        [StringLength(250)]
        public string Name { set; get; }

        [Column(TypeName = "varchar")]
        [StringLength(250)]
        public string Alias { set; get; }

        public int? ParentID { set; get; }

        public DateTime? CreatedDate { set; get; }

        public string CreatedBy { set; get; }

        public string LastModifiedBy { set; get; }

        public DateTime? LastModifiedDate { set; get; }

        public bool Status { set; get; }

        public virtual ICollection<Post> Posts { set; get; }

        [Column(TypeName = "nvarchar")]
        [StringLength(250)]
        public string MetaKeyword { set; get; }

        [Column(TypeName = "nvarchar")]
        [StringLength(250)]
        public string MetaDescription { set; get; }

        public bool? ShowHome { set; get; }
    }
}