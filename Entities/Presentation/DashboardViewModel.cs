using System;
using System.Collections.Generic;
using Entities.DbModels;

namespace Entities.Presentation
{
    public class DashboardViewModel
    {
        public DateTime? RatesDate { get; set; }
        public List<TcmbExchangeRate> ExchangeRates { get; set; }  // Döviz kuru verisi
        public List<RecurringJobInfo> RecurringJobs { get; set; }  // Job listesi
        public int CompanyCount { get; set; }  // Şirket sayısı
        public string LastJobTime { get; set; }  // Son job zamanı 
        
    }
}
