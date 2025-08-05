namespace AdminAPI.Presentation
{
    public class CurrencyDto
    {
        public string CurrencyCode { get; set; } // Örn: USD, EUR
        public string Type { get; set; }         // "Buy" veya "Sell"
        public decimal Value { get; set; }       // Kur değeri
    }
}
