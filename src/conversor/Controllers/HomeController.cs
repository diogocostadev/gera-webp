using System.Diagnostics;
using System.IO.Compression;
using conversor.Models;
using GeraWebP.Hub;
using GeraWebP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.Globalization;
using System.Collections.Generic;
using System.Text.Json;

namespace GeraWebP.Controllers
{
    public class HomeController(ILogger<HomeController> logger, IHubContext<ProgressHub> progressHub, IOptions<ApplicationSettings> appSettings
        ) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;
        private readonly IHubContext<ProgressHub> _progressHub = progressHub;
        private readonly ApplicationSettings _appSettings = appSettings.Value;

        private const string PastaRaiz = "wwwroot";
        private const string PastaConvertidos = "convertidos";
        private const string PastaUploads = "uploads";
        private static readonly string ContadorPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "contador.json");
        
        // Lock para evitar problemas de concorrência no contador
        private static readonly object ContadorLock = new object();
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public ActionResult Index(string? culture = null)
        {
            // Se culture vier da rota, usá-la
            if (string.IsNullOrEmpty(culture))
            {
                culture = RouteData.Values["culture"]?.ToString();
            }
            
            // Adicionar headers de performance e anti-cache
            Response.Headers.Append("X-Content-Type-Options", "nosniff");
            Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
            Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.Headers.Append("Pragma", "no-cache");
            Response.Headers.Append("Expires", "0");
            
            SetCultureContent(culture ?? "pt");
            int contadorGlobal = LerContadorGlobal();
            ViewBag.ContadorGlobal = contadorGlobal;
            return View();
        }

        public ActionResult Privacidade(string? culture = null)
        {
            // Se culture vier da rota, usá-la
            if (string.IsNullOrEmpty(culture))
            {
                culture = RouteData.Values["culture"]?.ToString();
            }
            
            SetCultureContent(culture ?? "pt");
            return View();
        }
        
        [HttpGet("privacidade")]
        public IActionResult PrivacidadeSimples()
        {
            SetCultureContent("pt");
            return View("Privacidade");
        }

        private void SetCultureContent(string culture)
        {
            switch (culture?.ToLower())
            {
                case "en":
                            ViewData["Title"] = "Free Online Wepper - WebP Converter";
        ViewData["Description"] = "Free and fast online WebP converter. Convert JPG, PNG, GIF images to WebP format with up to 90% size reduction. Optimize your images for web in seconds!";
                    ViewData["Language"] = "en";
                    ViewData["Culture"] = "en-US";
                    ViewData["DebugLang"] = "EN_SET"; // Debug temporário
                    break;
                case "es":
                    ViewData["Title"] = "Convertidor WebP Online Gratuito";
                    ViewData["Description"] = "Convertidor WebP online gratuito y rápido. Convierte imágenes JPG, PNG, GIF a formato WebP con hasta 90% de reducción de tamaño. ¡Optimiza tus imágenes para web en segundos!";
                    ViewData["Language"] = "es";
                    ViewData["Culture"] = "es-ES";
                    ViewData["DebugLang"] = "ES_SET"; // Debug temporário
                    break;
                default:
                    ViewData["Title"] = "Conversor WebP Online Gratuito";
                    ViewData["Description"] = "Conversor WebP online gratuito e rápido. Converta imagens JPG, PNG, GIF para formato WebP com até 90% de redução no tamanho. Otimize suas imagens para web em segundos!";
                    ViewData["Language"] = "pt";
                    ViewData["Culture"] = "pt-BR";
                    ViewData["DebugLang"] = "PT_SET"; // Debug temporário
                    break;
            }
        }
        
        [HttpPost]
        [RequestSizeLimit(104857600)] // 100MB
        [RequestFormLimits(
            MultipartBodyLengthLimit = 104857600,
            ValueLengthLimit = 104857600,
            MultipartHeadersLengthLimit = 104857600)]
        public async Task<IActionResult> Converter(List<IFormFile>? arquivos, int qualidade = 75)
        {   
            if (arquivos == null || arquivos.Count == 0)
            {
                ModelState.AddModelError("files", "Por favor, selecione um ou mais arquivos.");
                return View("Index");
            }
            
            // Validar tipos de arquivo
            var tiposPermitidos = new HashSet<string> { "image/jpeg", "image/png", "image/gif" };
            foreach (var arquivo in arquivos)
            {
                if (!tiposPermitidos.Contains(arquivo.ContentType))
                {
                    ModelState.AddModelError("files", $"Tipo de arquivo não permitido: {arquivo.ContentType}");
                    return View("Index");
                }
            }
            
            // Validar dimensões das imagens antes da conversão
            const int maxWebPWidth = 16383;
            const int maxWebPHeight = 16383;
            
            foreach (var arquivo in arquivos)
            {
                try
                {
                    using var imageStream = arquivo.OpenReadStream();
                    using var image = await Image.LoadAsync(imageStream);
                    
                    if (image.Width > maxWebPWidth || image.Height > maxWebPHeight)
                    {
                        ModelState.AddModelError("files", 
                            $"A imagem '{arquivo.FileName}' ({image.Width}x{image.Height}) excede o limite máximo de {maxWebPWidth}x{maxWebPHeight} pixels suportado pelo formato WebP.");
                        return View("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("files", $"Erro ao processar a imagem '{arquivo.FileName}': {ex.Message}");
                    return View("Index");
                }
            }
            
            // Validar tamanho total dos arquivos (100MB máximo)
            const long maxTotalSize = 100 * 1024 * 1024; // 100MB
            const long maxFileSize = 100 * 1024 * 1024;  // 100MB por arquivo
            
            long totalSize = arquivos.Sum(f => f.Length);
            if (totalSize > maxTotalSize)
            {
                var totalMB = Math.Round(totalSize / (1024.0 * 1024.0), 1);
                ModelState.AddModelError("files", $"Tamanho total dos arquivos ({totalMB}MB) excede o limite de 100MB. Por favor, selecione menos arquivos ou arquivos menores.");
                return View("Index");
            }
            
            // Validar tamanho individual dos arquivos
            foreach (var arquivo in arquivos)
            {
                if (arquivo.Length > maxFileSize)
                {
                    var fileMB = Math.Round(arquivo.Length / (1024.0 * 1024.0), 1);
                    ModelState.AddModelError("files", $"Arquivo '{arquivo.FileName}' ({fileMB}MB) excede o limite de 100MB por arquivo.");
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
            
            IncrementarContadorGlobal(arquivos.Count);

            ViewBag.DownloadLink = Url.Action("DownloadFiles", new { sessionId })!;
            return View("Index");
        }

        [HttpPost("api/converter")]
        [RequestSizeLimit(104857600)] // 100MB
        [RequestFormLimits(
            MultipartBodyLengthLimit = 104857600,
            ValueLengthLimit = 104857600,
            MultipartHeadersLengthLimit = 104857600)]
        public async Task<IActionResult> ConverterApi(List<IFormFile>? arquivos, int qualidade = 75)
        {
            try
            {
                if (arquivos == null || arquivos.Count == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Por favor, selecione um ou mais arquivos."
                    });
                }

                // Validar tipos de arquivo
                var tiposPermitidos = new HashSet<string> { "image/jpeg", "image/png", "image/gif" };
                foreach (var arquivo in arquivos)
                {
                    if (!tiposPermitidos.Contains(arquivo.ContentType))
                    {
                        return Json(new
                        {
                            success = false,
                            message = $"Tipo de arquivo não permitido: {arquivo.ContentType}"
                        });
                    }
                }

                // Validar dimensões das imagens antes da conversão
                const int maxWebPWidth = 16383;
                const int maxWebPHeight = 16383;

                foreach (var arquivo in arquivos)
                {
                    try
                    {
                        using var imageStream = arquivo.OpenReadStream();
                        using var image = await Image.LoadAsync(imageStream);

                        if (image.Width > maxWebPWidth || image.Height > maxWebPHeight)
                        {
                            return Json(new
                            {
                                success = false,
                                message = $"A imagem '{arquivo.FileName}' ({image.Width}x{image.Height}) excede o limite máximo de {maxWebPWidth}x{maxWebPHeight} pixels suportado pelo formato WebP."
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        return Json(new
                        {
                            success = false,
                            message = $"Erro ao processar a imagem '{arquivo.FileName}': {ex.Message}"
                        });
                    }
                }

                // Validar tamanho total dos arquivos (100MB máximo)
                const long maxTotalSize = 100 * 1024 * 1024; // 100MB
                const long maxFileSize = 100 * 1024 * 1024;  // 100MB por arquivo

                long totalSize = arquivos.Sum(f => f.Length);
                if (totalSize > maxTotalSize)
                {
                    var totalMB = Math.Round(totalSize / (1024.0 * 1024.0), 1);
                    return Json(new
                    {
                        success = false,
                        message = $"Tamanho total dos arquivos ({totalMB}MB) excede o limite de 100MB. Por favor, selecione menos arquivos ou arquivos menores."
                    });
                }

                // Validar tamanho individual dos arquivos
                foreach (var arquivo in arquivos)
                {
                    if (arquivo.Length > maxFileSize)
                    {
                        var fileMB = Math.Round(arquivo.Length / (1024.0 * 1024.0), 1);
                        return Json(new
                        {
                            success = false,
                            message = $"Arquivo '{arquivo.FileName}' ({fileMB}MB) excede o limite de 100MB por arquivo."
                        });
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

                IncrementarContadorGlobal(arquivos.Count);

                var downloadLink = Url.Action("DownloadFiles", new { sessionId });

                return Json(new
                {
                    success = true,
                    message = "Conversão concluída com sucesso!",
                    downloadLink = downloadLink,
                    sessionId = sessionId,
                    filesCount = arquivos.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na conversão de arquivos");
                return Json(new
                {
                    success = false,
                    message = "Erro interno na conversão: " + ex.Message
                });
            }
        }

        private static async Task<byte[]> ConvertToWebP(IFormFile file, int qualidade)
        {
            await using var imageStream = file.OpenReadStream();
            using var image = await Image.LoadAsync(imageStream);
            
            // Otimização automática baseada no tamanho do arquivo
            var tamanhoOriginalMB = file.Length / (1024.0 * 1024.0);
            var perfilOtimizacao = DeterminarPerfilOtimizacao(tamanhoOriginalMB, qualidade);
            
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
            
            if (!Directory.Exists(caminhoConvertidos))
            {
                return NotFound("Arquivos não encontrados.");
            }

            var arquivos = Directory.GetFiles(caminhoConvertidos);
            
            if (arquivos.Length == 0)
            {
                return NotFound("Nenhum arquivo encontrado.");
            }
            
            // Se houver apenas um arquivo, baixar diretamente
            if (arquivos.Length == 1)
            {
                var arquivo = arquivos[0];
                var nomeArquivo = Path.GetFileName(arquivo);
                var bytes = System.IO.File.ReadAllBytes(arquivo);
                
                return File(bytes, "image/webp", nomeArquivo);
            }
            
            // Se houver múltiplos arquivos, criar zip
            var caminhoZip = Path.Combine(Directory.GetCurrentDirectory(), PastaRaiz, PastaConvertidos, sessionId + ".zip");
            
            // Remover zip existente se houver
            if (System.IO.File.Exists(caminhoZip))
            {
                System.IO.File.Delete(caminhoZip);
            }

            ZipFile.CreateFromDirectory(caminhoConvertidos, caminhoZip);

            byte[] fileBytes = System.IO.File.ReadAllBytes(caminhoZip);
            return File(fileBytes, "application/zip", sessionId + ".zip");
        }
        
        [HttpGet("converter-jpg-para-webp")]
        public IActionResult ConverterJpgParaWebp()
        {
            ViewData["Title"] = "Converter JPG para WebP Online Gratuito";
            ViewData["Description"] = "Converta imagens JPG/JPEG para WebP online gratuitamente. Reduza até 90% do tamanho mantendo a qualidade. Ferramenta rápida e segura.";
            ViewData["Keywords"] = "converter jpg para webp, jpg para webp online, jpeg para webp, converter jpeg webp gratuito";
            ViewData["FormatType"] = "JPG";
            ViewData["FormatIcon"] = "jpg";
            ViewData["FormatColor"] = "#f59e0b";
            return View("FormatSpecific");
        }

        [HttpGet("converter-png-para-webp")]
        public IActionResult ConverterPngParaWebp()
        {
            ViewData["Title"] = "Converter PNG para WebP Online Gratuito";
            ViewData["Description"] = "Converta imagens PNG para WebP online mantendo transparência. Reduza até 85% do tamanho. Ferramenta gratuita e profissional.";
            ViewData["Keywords"] = "converter png para webp, png para webp online, converter png webp transparência, png webp gratuito";
            ViewData["FormatType"] = "PNG";
            ViewData["FormatIcon"] = "png";
            ViewData["FormatColor"] = "#8b5cf6";
            return View("FormatSpecific");
        }

        [HttpGet("converter-gif-para-webp")]
        public IActionResult ConverterGifParaWebp()
        {
            ViewData["Title"] = "Converter GIF para WebP Online Gratuito";
            ViewData["Description"] = "Converta animações GIF para WebP online. Compressão máxima para GIFs animados. Ferramenta online gratuita e rápida.";
            ViewData["Keywords"] = "converter gif para webp, gif para webp online, gif animado webp, comprimir gif webp";
            ViewData["FormatType"] = "GIF";
            ViewData["FormatIcon"] = "gif";
            ViewData["FormatColor"] = "#ef4444";
            return View("FormatSpecific");
        }

        [HttpGet("compressor-imagem")]
        public IActionResult CompressorImagem()
        {
            ViewData["Title"] = "Compressor de Imagem Online Gratuito - Wepper";
            ViewData["Description"] = "Comprima e otimize suas imagens online gratuitamente. Reduza o tamanho de JPG, PNG, GIF convertendo para WebP. Ferramenta profissional.";
            ViewData["Keywords"] = "compressor imagem online, comprimir foto, otimizar imagem web, reduzir tamanho imagem";
            return View("CompressorImagem");
        }



        // Rotas específicas para idiomas
        [HttpGet("en")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult IndexEnglish()
        {
            SetCultureContent("en");
            int contadorGlobal = LerContadorGlobal();
            ViewBag.ContadorGlobal = contadorGlobal;
            return View("IndexEnglish");
        }

        [HttpGet("es")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult IndexSpanish()
        {
            SetCultureContent("es");
            int contadorGlobal = LerContadorGlobal();
            ViewBag.ContadorGlobal = contadorGlobal;
            return View("Index");
        }

        [HttpGet("en/privacy")]
        public IActionResult PrivacyEnglish()
        {
            SetCultureContent("en");
            return View("Privacy");
        }

        [HttpGet("es/privacidad")]
        public IActionResult PrivacidadSpanish()
        {
            SetCultureContent("es");
            return View("Privacidad");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private int LerContadorGlobal()
        {
            lock (ContadorLock)
            {
                try
                {
                    // Criar diretório se não existir
                    var directoryPath = Path.GetDirectoryName(ContadorPath);
                    if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    
                    if (!System.IO.File.Exists(ContadorPath))
                        return 0;
                    var json = System.IO.File.ReadAllText(ContadorPath);
                    var obj = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(json);
                    return obj != null && obj.ContainsKey("total") ? obj["total"] : 0;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao ler contador global");
                    return 0;
                }
            }
        }

        private void IncrementarContadorGlobal(int quantidade)
        {
            lock (ContadorLock)
            {
                try
                {
                    // Criar diretório se não existir
                    var directoryPath = Path.GetDirectoryName(ContadorPath);
                    if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    
                    int atual = 0;
                    if (System.IO.File.Exists(ContadorPath))
                    {
                        var json = System.IO.File.ReadAllText(ContadorPath);
                        var obj = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(json);
                        atual = obj != null && obj.ContainsKey("total") ? obj["total"] : 0;
                    }
                    
                    int novo = atual + quantidade;
                    var novoObj = new Dictionary<string, int> { { "total", novo } };
                    var novoJson = System.Text.Json.JsonSerializer.Serialize(novoObj, new JsonSerializerOptions { WriteIndented = true });
                    System.IO.File.WriteAllText(ContadorPath, novoJson);
                    
                    _logger.LogInformation("Contador incrementado: {Anterior} + {Quantidade} = {Novo}", atual, quantidade, novo);
                }
                catch (Exception ex)
                {
                    // Log erro mas não falhar a conversão por causa do contador
                    _logger.LogError(ex, "Erro ao incrementar contador global de {Anterior} para +{Quantidade}", quantidade, quantidade);
                }
            }
        }
        
        /// <summary>
        /// Endpoint para retornar informações de versão da aplicação
        /// </summary>
        [HttpGet]
        [Route("version")]
        [Route("api/version")]
        public IActionResult Version()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var assemblyBuildTime = System.IO.File.GetCreationTimeUtc(assembly.Location);
                
                var versionInfo = new
                {
                    Application = _appSettings.Name,
                    Version = _appSettings.Version,
                    ShortVersion = _appSettings.Version,
                    BuildDate = _appSettings.BuildDate,
                    BuildTime = assemblyBuildTime.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                    Description = _appSettings.Description,
                    Framework = Environment.Version.ToString(),
                    Platform = Environment.OSVersion.ToString(),
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                    DisplayVersion = _appSettings.DisplayVersion
                };
                
                return Json(versionInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter informações de versão");
                
                // Fallback usando configurações básicas
                var fallbackInfo = new
                {
                    Application = _appSettings.Name,
                    Version = _appSettings.Version,
                    Error = "Algumas informações não puderam ser obtidas"
                };
                
                return Json(fallbackInfo);
            }
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
