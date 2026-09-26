using System.Net;
using LaLiga.WebUI.Services;
using LaLiga.WebUI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace LaLiga.WebUI.Filters
{
    // Bir sayfa çalışırken API'den hata (ApiException) gelirse kullanıcıya anlaşılır bir hata sayfası gösterir.
    public class ApiExceptionFilter : IExceptionFilter
    {
        private readonly IModelMetadataProvider _metadata;

        public ApiExceptionFilter(IModelMetadataProvider metadata)
        {
            _metadata = metadata;
        }

        public void OnException(ExceptionContext context)
        {
            if (context.Exception is not ApiException ex)
                return;

            bool notFound = ex.StatusCode == HttpStatusCode.NotFound;
            int status = notFound ? StatusCodes.Status404NotFound : StatusCodes.Status503ServiceUnavailable;

            var model = new ApiErrorViewModel
            {
                StatusCode = status,
                Title = notFound ? "Kayıt bulunamadı" : "Şu an bu sayfayı gösteremiyoruz",
                Message = ex.Message
            };

            context.Result = new ViewResult
            {
                ViewName = "ApiError",
                StatusCode = status,
                ViewData = new ViewDataDictionary(_metadata, context.ModelState) { Model = model }
            };

            context.ExceptionHandled = true;
        }
    }
}
