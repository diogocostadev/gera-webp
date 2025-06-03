using System.Diagnostics;
using System.IO.Compression;
using conversor.Models;
using GeraWebP.Hub;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

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
        public async Task<IActionResult> Converter(List<IFormFile>? arquivos, int qualidade = 75, int maxWidth = 1920, int maxHeight = 1080, bool manterProporção = true)
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

                        byte[] webPImage = await ConvertToWebP(arquivo, qualidade, maxWidth, maxHeight, manterProporção);
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

        private static async Task<byte[]> ConvertToWebP(IFormFile file, int qualidade, int maxWidth = 1920, int maxHeight = 1080, bool manterProporção = true)
        {
            await using var imageStream = file.OpenReadStream();
            using var image = await Image.LoadAsync(imageStream);
            
            // Otimização automática baseada no tamanho do arquivo
            var tamanhoOriginalMB = file.Length / (1024.0 * 1024.0);
            var perfilOtimizacao = DeterminarPerfilOtimizacao(tamanhoOriginalMB, qualidade);
            
            // Redimensionar a imagem se necessário
            if (image.Width > maxWidth || image.Height > maxHeight)
            {
                var resizeOptions = new ResizeOptions
                {
                    Size = new Size(maxWidth, maxHeight),
                    Mode = manterProporção ? ResizeMode.Max : ResizeMode.Stretch,
                    Sampler = KnownResamplers.Lanczos3 // Melhor qualidade de redimensionamento
                };
                
                image.Mutate(x => x.Resize(resizeOptions));
            }
            
            // Aplicar filtros adicionais para reduzir ruído se a imagem for muito grande
            if (tamanhoOriginalMB > 5)
            {
                image.Mutate(x => x.GaussianSharpen(0.5f)); // Leve sharpening para compensar compressão
            }
            
            using var output = new MemoryStream();
            
            // Configurações avançadas do WebP encoder baseadas no perfil
            var encoder = new WebpEncoder
            {
                Quality = perfilOtimizacao.Quality,
                Method = perfilOtimizacao.Method,
                FileFormat = perfilOtimizacao.FileFormat,
                FilterStrength = perfilOtimizacao.FilterStrength,
                SpatialNoiseShaping = perfilOtimizacao.SpatialNoiseShaping,
                NearLossless = perfilOtimizacao.NearLossless,
                TransparentColorMode = WebpTransparentColorMode.Clear
            };
            
            await image.SaveAsWebpAsync(output, encoder);
            return output.ToArray();
        }

        private static WebPOptimizationProfile DeterminarPerfilOtimizacao(double tamanhoMB, int qualidadeBase)
        {
            return tamanhoMB switch
            {
                // Arquivos muito grandes (>10MB) - Compressão máxima
                > 10 => new WebPOptimizationProfile
                {
                    Quality = Math.Max(qualidadeBase - 15, 40),
                    Method = WebpEncodingMethod.BestQuality,
                    FileFormat = WebpFileFormatType.Lossy,
                    FilterStrength = 80,
                    SpatialNoiseShaping = 70,
                    NearLossless = false
                },
                // Arquivos grandes (5-10MB) - Compressão alta
                > 5 => new WebPOptimizationProfile
                {
                    Quality = Math.Max(qualidadeBase - 10, 50),
                    Method = WebpEncodingMethod.BestQuality,
                    FileFormat = WebpFileFormatType.Lossy,
                    FilterStrength = 70,
                    SpatialNoiseShaping = 60,
                    NearLossless = false
                },
                // Arquivos médios (1-5MB) - Compressão balanceada
                > 1 => new WebPOptimizationProfile
                {
                    Quality = Math.Max(qualidadeBase - 5, 60),
                    Method = WebpEncodingMethod.BestQuality,
                    FileFormat = WebpFileFormatType.Lossy,
                    FilterStrength = 60,
                    SpatialNoiseShaping = 50,
                    NearLossless = false
                },
                // Arquivos pequenos (<1MB) - Compressão suave
                _ => new WebPOptimizationProfile
                {
                    Quality = qualidadeBase,
                    Method = WebpEncodingMethod.BestQuality,
                    FileFormat = WebpFileFormatType.Lossy,
                    FilterStrength = 40,
                    SpatialNoiseShaping = 30,
                    NearLossless = false
                }
            };
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

    // Classe para definir perfis de otimização
    public class WebPOptimizationProfile
    {
        public int Quality { get; set; }
        public WebpEncodingMethod Method { get; set; }
        public WebpFileFormatType FileFormat { get; set; }
        public int FilterStrength { get; set; }
        public int SpatialNoiseShaping { get; set; }
        public bool NearLossless { get; set; }
    }
}
