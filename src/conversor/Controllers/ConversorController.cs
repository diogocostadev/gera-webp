using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;



namespace conversor.Controllers
{
    public class ConversorController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
       
            [HttpPost]
        public IActionResult Convert(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
              
                byte[] webPImage = ConvertToWebP(file);
                
                ViewBag.WebPImage = webPImage;
            }
            else
            {
                ModelState.AddModelError("file", "Selecione um arquivo.");
            }

           
            return View("Index");
        }
    

        private byte[] ConvertToWebP(IFormFile file)
        {
            using (var imageStream = file.OpenReadStream())
            {
                using (var image = Image.FromStream(imageStream))
                {
                    using (var output = new MemoryStream())
                    {
                       
                        var encoder = ImageCodecInfo.GetImageEncoders()[1];
                        var parameters = new EncoderParameters(1);
                        parameters.Param[0] = new EncoderParameter(Encoder.Quality, 50L); // Ajuste a qualidade conforme necessário

                        
                        image.Save(output, encoder, parameters);

                        return output.ToArray();
                    }
                }
            }
        }
    }
}



