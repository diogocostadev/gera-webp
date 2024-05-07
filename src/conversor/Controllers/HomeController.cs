using System.Diagnostics;
using System.IO.Compression;
using conversor.Models;
using GeraWebP.Hub;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace GeraWebP.Controllers
{
    public class HomeController(ILogger<HomeController> logger, IHubContext<ProgressHub> progressHub
        ) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;
        private readonly IHubContext<ProgressHub> _progressHub = progressHub;

        private const string PastaRaiz = "wwwroot";
        private const string PastaConvertidos = "convertidos";
        private const string PastaUploads = "uploads";
        
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Privacidade()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Converter(List<IFormFile>? arquivos, int qualidade = 75)
        {   
            if (arquivos == null || arquivos.Count == 0)
            {
                ModelState.AddModelError("files", "Por favor, selecione um ou mais arquivos.");
                return View("Index");
            }
            
            var tiposPermitidos = new HashSet<string> { "image/jpeg", "image/png", "image/gif" };
            foreach (var arquivo in arquivos)
            {
                if (!tiposPermitidos.Contains(arquivo.ContentType))
                {
                    ModelState.AddModelError("files", $"Tipo de arquivo não permitido: {arquivo.ContentType}");
                    return View("Index");
                }
            }
            
            var sessionId = Guid.NewGuid().ToString();
            var caminhoUpload = Path.Combine(Directory.GetCurrentDirectory(), PastaRaiz, PastaUploads, sessionId);
            var caminhoConvertido = Path.Combine(Directory.GetCurrentDirectory(), PastaRaiz, PastaConvertidos, sessionId);

            if (!Directory.Exists(caminhoUpload))
            {
                Directory.CreateDirectory(caminhoUpload);
            }

            if (!Directory.Exists(caminhoConvertido))
            {
                Directory.CreateDirectory(caminhoConvertido);
            }

            int totalFiles = arquivos.Count;
            int arquivosProcessados = 0;

            List<Task> tasks = [];
            foreach (var arquivo in arquivos)
            {
                var task = Task.Run(async () =>
                {
                    if (arquivo.Length > 0)
                    {
                        var caminhoOriginal = Path.Combine(caminhoUpload, arquivo.FileName);
                        var nomeArquivo = Path.GetFileNameWithoutExtension(arquivo.FileName);
                        
                        var caminhoCompletoConvertido = Path.Combine(caminhoConvertido, nomeArquivo + ".webp");

                        using (var fileStream = new FileStream(caminhoOriginal, FileMode.Create))
                        {
                            await arquivo.CopyToAsync(fileStream);
                        }

                        byte[] webPImage = await ConvertToWebP(arquivo, qualidade);
                        await System.IO.File.WriteAllBytesAsync(caminhoCompletoConvertido, webPImage);


                        arquivosProcessados++;
                        int progress = (int)((float)arquivosProcessados / totalFiles * 100);
                        await _progressHub.Clients.All.SendAsync("ReceiveProgress", progress);
                    }
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            ViewBag.DownloadLink = Url.Action("DownloadFiles", new { sessionId })!;

            return View("Index");
            
        }

        private static async Task<byte[]> ConvertToWebP(IFormFile file, int qualidade)
        {
            await using var imageStream = file.OpenReadStream();
            using var image = await Image.LoadAsync(imageStream);
            using var output = new MemoryStream();
            var encoder = new WebpEncoder
            {
                Quality = qualidade //nos testes nao notei diferença de qualidades 
            };
            await image.SaveAsWebpAsync(output, encoder);
            return output.ToArray();
        }

        public IActionResult DownloadFiles(string sessionId)
        {
            var caminhoConvertidos = Path.Combine(Directory.GetCurrentDirectory(), PastaRaiz, PastaConvertidos, sessionId);
            var caminhoZip = Path.Combine(Directory.GetCurrentDirectory(), PastaRaiz, PastaConvertidos, sessionId + ".zip");

            ZipFile.CreateFromDirectory(caminhoConvertidos, caminhoZip);

            byte[] fileBytes = System.IO.File.ReadAllBytes(caminhoZip);
            return File(fileBytes, "application/zip", sessionId + ".zip");
        }
        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
