using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DbModels
{
    [Table("tcmb_exchange_rate")]
    public class TcmbExchangeRate
    {
        [Key, Column("date", Order = 1)]
        public DateTime Date { get; set; }

        [Key, Column("currency_code", Order = 2)]
        public string CurrencyCode { get; set; } = string.Empty;

        [Key, Column("type", Order = 3)]
        public string Type { get; set; } = string.Empty; // Buy / Sell

        [Column("value")]
        public decimal Value { get; set; }
    }
}
