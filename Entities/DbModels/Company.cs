using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.DbModels
{
    [Table("companies")]
    public class Company
    {
        [Key]
        [MaxLength(50)]
        [Column("stock_code")] 
        public string StockCode { get; set; }

        [Column("mkk_member_oid")]
        public string MkkMemberOid { get; set; }

        [Column("kap_member_title")]
        public string KapMemberTitle { get; set; }

        [Column("related_member_title")]
        public string RelatedMemberTitle { get; set; }

        [Column("city_name")]
        public string CityName { get; set; }

        [Column("related_member_oid")]
        public string RelatedMemberOid { get; set; }

        [Column("kap_member_type")]
        public string KapMemberType { get; set; }
    }
}

