using Microsoft.AspNetCore.Mvc;

namespace GeraWebP.Controllers;

[Route("Error")]
public class ErrorController(ILogger<ErrorController> logger) : Controller
{
    private readonly ILogger<ErrorController> _logger = logger;

    [Route("{statusCode:int}")]
    public IActionResult HandleError(int statusCode)
    {
        var requestUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;
        var method = HttpContext.Request.Method;
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        
        _logger.LogWarning("Erro HTTP {StatusCode} capturado pelo ErrorController - URL: {RequestUrl}, Método: {Method}, UserAgent: {UserAgent}", 
            statusCode, requestUrl, method, userAgent);
        
        switch (statusCode)
        {
            case 404:
                return View("NotFound");
            case 405:
                _logger.LogWarning("Método não permitido para URL: {RequestUrl} com método: {Method}", requestUrl, method);
                // Redireciona para a página inicial ou retorna erro mais amigável
                return RedirectToAction("Index", "Home");
            case 500:
                return View("InternalServerError");
            default:
                _logger.LogError("Status code não tratado: {StatusCode} para URL: {RequestUrl}", statusCode, requestUrl);
                return View("GeneralError");
        }
    }

    [Route("404")]
    public new IActionResult NotFound()
    {
        _logger.LogWarning("Página não encontrada acessada diretamente");
        Response.StatusCode = 404;
        return View();
    }

    [Route("500")]
    public IActionResult InternalServerError()
    {
        _logger.LogError("Erro interno do servidor acessado diretamente");
        Response.StatusCode = 500;
        return View();
    }
} 