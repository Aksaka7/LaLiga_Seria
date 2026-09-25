namespace LaLiga.WebUI.ViewModels
{
    // API'nin tüm hata yanıtları bu biçimde: { "message": "...", "errors": { "alan": ["..."] } }
    public class ApiErrorResponse
    {
        public string? Message { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}
