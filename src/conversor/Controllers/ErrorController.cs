using Microsoft.AspNetCore.Mvc;

namespace GeraWebP.Controllers;

[Route("Error")]
public class ErrorController : Controller
{
    [Route("{statusCode:int}")]
    public IActionResult HandleError(int statusCode)
    {
        switch (statusCode)
        {
            case 404:
                return View("NotFound");
            case 500:
                return View("InternalServerError");
            default:
                return View("GeneralError");
        }
    }

    [Route("404")]
    public new IActionResult NotFound()
    {
        Response.StatusCode = 404;
        return View();
    }

    [Route("500")]
    public IActionResult InternalServerError()
    {
        Response.StatusCode = 500;
        return View();
    }
} 