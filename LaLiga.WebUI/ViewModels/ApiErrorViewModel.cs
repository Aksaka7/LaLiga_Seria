namespace LaLiga.WebUI.ViewModels
{
    // Hata sayfasında gösterilecek bilgiler
    public class ApiErrorViewModel
    {
        public int StatusCode { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
