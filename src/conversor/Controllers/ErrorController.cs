using Microsoft.AspNetCore.Mvc;

namespace GeraWebP.Controllers;

[Route("Error")]
public class ErrorController(ILogger<ErrorController> logger) : Controller
{
    private readonly ILogger<ErrorController> _logger = logger;

    [Route("{statusCode:int}")]
    public IActionResult HandleError(int statusCode)
    {
        _logger.LogWarning("Erro HTTP {StatusCode} capturado pelo ErrorController", statusCode);
        
        switch (statusCode)
        {
            case 404:
                return View("NotFound");
            case 500:
                return View("InternalServerError");
            default:
                _logger.LogError("Status code não tratado: {StatusCode}", statusCode);
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