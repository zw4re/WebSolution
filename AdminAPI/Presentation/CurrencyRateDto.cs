namespace AdminAPI.Presentation
{
    public class CurrencyRateDto
    {
        public string CurrencyCode { get; set; } // Örn: USD
        public decimal? Buy { get; set; }        // Alış
        public decimal? Sell { get; set; }       // Satış
    }
}
