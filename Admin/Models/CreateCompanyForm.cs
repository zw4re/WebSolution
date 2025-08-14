namespace Admin.Models
{
    // Dashboard formundan gelen alanlar
    public class CreateCompanyForm
    {
        public string Name { get; set; }       // Şirket Adı  → KapMemberTitle
        public string StockCode { get; set; }  // Hisse Kodu  → StockCode
        public string City { get; set; }       // Şehir       → CityName
        public string Type { get; set; }       // Tür         → KapMemberType
    }
}
