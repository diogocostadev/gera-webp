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
        public async Task<IActionResult> Converter(List<IFormFile>? arquivos, int qualidade = 75, string outputFormat = "webp", string sourceView = "Index")
        {   
            try
            {
                // Monitorar recursos do sistema antes de iniciar
                var totalMemoryMB = GC.GetTotalMemory(false) / (1024.0 * 1024.0);
                var availableProcessors = Environment.ProcessorCount;
                var workingSetMB = Environment.WorkingSet / (1024.0 * 1024.0);
                
                _logger.LogInformation("Iniciando conversão de arquivos. Formato: {OutputFormat}, Quantidade: {Count}, Qualidade: {Quality}. " +
                    "Sistema - Memória: {MemoryMB:F1}MB, WorkingSet: {WorkingSetMB:F1}MB, CPUs: {CPUs}", 
                    outputFormat, arquivos?.Count ?? 0, qualidade, totalMemoryMB, workingSetMB, availableProcessors);
                
                if (arquivos == null || arquivos.Count == 0)
                {
                    _logger.LogWarning("Tentativa de conversão sem arquivos selecionados");
                    ModelState.AddModelError("files", "Por favor, selecione um ou mais arquivos.");
                    return View(sourceView);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no início do processo de conversão");
                ModelState.AddModelError("", "Erro interno no servidor. Tente novamente.");
                return View(sourceView);
            }
            
            // Validar tipos de arquivo
            if (outputFormat == "jpeg")
            {
                var tiposPermitidosJpeg = new HashSet<string> { "image/jpeg" };
                foreach (var arquivo in arquivos)
                {
                    if (!tiposPermitidosJpeg.Contains(arquivo.ContentType))
                    {
                        ModelState.AddModelError("files", $"Para compressão JPEG, apenas arquivos 'image/jpeg' são permitidos. Arquivo '{arquivo.FileName}' é do tipo '{arquivo.ContentType}'.");
                        return View(sourceView);
                    }
                }
            }
            else // webp
            {
                var tiposPermitidosWebp = new HashSet<string> { "image/jpeg", "image/png", "image/gif" };
                foreach (var arquivo in arquivos)
                {
                    if (!tiposPermitidosWebp.Contains(arquivo.ContentType))
                    {
                        ModelState.AddModelError("files", $"Tipo de arquivo não permitido para conversão WebP: {arquivo.ContentType}");
                        return View(sourceView);
                    }
                }
            }
            
            // Validar dimensões das imagens antes da conversão (apenas para WebP)
            if (outputFormat == "webp")
            {
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
                            return View(sourceView);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao validar dimensões da imagem {FileName}", arquivo.FileName);
                        ModelState.AddModelError("files", $"Erro ao processar a imagem '{arquivo.FileName}': {ex.Message}");
                        return View(sourceView);
                    }
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
                return View(sourceView);
            }
            
            // Validar tamanho individual dos arquivos
            foreach (var arquivo in arquivos)
            {
                if (arquivo.Length > maxFileSize)
                {
                    var fileMB = Math.Round(arquivo.Length / (1024.0 * 1024.0), 1);
                    ModelState.AddModelError("files", $"Arquivo '{arquivo.FileName}' ({fileMB}MB) excede o limite de 100MB por arquivo.");
                    return View(sourceView);
                }
            }
            
            var sessionId = Guid.NewGuid().ToString();
            var caminhoUpload = Path.Combine(Directory.GetCurrentDirectory(), PastaRaiz, PastaUploads, sessionId);
            var caminhoConvertido = Path.Combine(Directory.GetCurrentDirectory(), PastaRaiz, PastaConvertidos, sessionId);

            try
            {
                _logger.LogInformation("Criando diretórios para sessão {SessionId}", sessionId);
                
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
                
                var currentWorkingSetMB = Environment.WorkingSet / (1024.0 * 1024.0);
                var maxConcurrency = Environment.ProcessorCount > 2 ? 3 : 1;
                var memoryThresholdMB = 500;
                
                if (currentWorkingSetMB > memoryThresholdMB)
                {
                    maxConcurrency = 1;
                    _logger.LogWarning("🔥 Memória alta detectada ({WorkingSetMB:F1}MB > {ThresholdMB}MB). Reduzindo concorrência para 1", 
                        currentWorkingSetMB, memoryThresholdMB);
                }
                
                var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
                _logger.LogInformation("Configuração de concorrência: {MaxConcurrency} conversões simultâneas", maxConcurrency);
                
                foreach (var arquivo in arquivos)
                {
                    var task = Task.Run(async () =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            if (arquivo.Length > 0)
                            {
                                var threadStartTime = DateTime.UtcNow;
                                _logger.LogInformation("Thread {ThreadId} - Iniciando processamento do arquivo {FileName} (Tamanho: {FileSize} bytes) para o formato {OutputFormat} às {StartTime}",
                                    Thread.CurrentThread.ManagedThreadId, arquivo.FileName, arquivo.Length, outputFormat, threadStartTime);

                                var caminhoOriginal = Path.Combine(caminhoUpload, arquivo.FileName);
                                var nomeArquivo = Path.GetFileNameWithoutExtension(arquivo.FileName);
                                string caminhoCompletoConvertido;
                                byte[] outputImage;

                                using (var fileStream = new FileStream(caminhoOriginal, FileMode.Create))
                                {
                                    await arquivo.CopyToAsync(fileStream);
                                }

                                if (outputFormat == "jpeg")
                                {
                                    caminhoCompletoConvertido = Path.Combine(caminhoConvertido, nomeArquivo + ".jpeg");
                                    _logger.LogDebug("Iniciando compressão JPEG para {FileName}", arquivo.FileName);
                                    outputImage = await CompressJpeg(arquivo, qualidade);
                                }
                                else // default to webp
                                {
                                    caminhoCompletoConvertido = Path.Combine(caminhoConvertido, nomeArquivo + ".webp");
                                    _logger.LogDebug("Iniciando conversão WebP para {FileName}", arquivo.FileName);
                                    outputImage = await ConvertToWebP(arquivo, qualidade);
                                }

                                _logger.LogDebug("Salvando arquivo convertido: {CaminhoConvertido}", caminhoCompletoConvertido);
                                await System.IO.File.WriteAllBytesAsync(caminhoCompletoConvertido, outputImage);

                                Interlocked.Increment(ref arquivosProcessados);
                                int progress = (int)((float)arquivosProcessados / totalFiles * 100);

                                try 
                                {
                                    await _progressHub.Clients.All.SendAsync("ReceiveProgress", progress);
                                }
                                catch (Exception hubEx)
                                {
                                    _logger.LogWarning(hubEx, "Erro ao enviar progresso via SignalR para {FileName}", arquivo.FileName);
                                }
                                
                                var threadEndTime = DateTime.UtcNow;
                                var conversionDuration = (threadEndTime - threadStartTime).TotalMilliseconds;
                                
                                _logger.LogInformation("Thread {ThreadId} - Arquivo {FileName} processado com sucesso em {DurationMs}ms. Progresso: {Progress}%", 
                                    Thread.CurrentThread.ManagedThreadId, arquivo.FileName, conversionDuration, progress);
                                
                                if (conversionDuration > 30000) // 30 segundos
                                {
                                    _logger.LogWarning("⚠️ Processamento LENTO detectado! Arquivo {FileName} demorou {DurationMs}ms ({DurationSec:F1}s)", 
                                        arquivo.FileName, conversionDuration, conversionDuration / 1000.0);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            var isServerError = ex is OutOfMemoryException or 
                                                TimeoutException or 
                                                System.IO.IOException or 
                                                System.ComponentModel.Win32Exception;
                            
                            if (isServerError)
                            {
                                _logger.LogError(ex, "🚨 ERRO DE SERVIDOR detectado ao converter {FileName}: {ExceptionType} - {Message}", 
                                    arquivo.FileName, ex.GetType().Name, ex.Message);
                            }
                            else
                            {
                                _logger.LogError(ex, "Erro ao converter arquivo {FileName}. Detalhes: {ErrorDetails}", 
                                    arquivo.FileName, ex.ToString());
                            }
                            throw;
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });
                    tasks.Add(task);
                }

                _logger.LogInformation("Aguardando conclusão de {TaskCount} tarefas de conversão", tasks.Count);
                
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var initialMemory = GC.GetTotalMemory(false);
                
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
                    await Task.WhenAll(tasks).WaitAsync(cts.Token);
                    
                    stopwatch.Stop();
                    var finalMemory = GC.GetTotalMemory(false);
                    var memoryUsed = (finalMemory - initialMemory) / (1024.0 * 1024.0);
                    
                    _logger.LogInformation("Conversão concluída com sucesso em {ElapsedTime}ms. Memória utilizada: {MemoryMB:F2}MB", 
                        stopwatch.ElapsedMilliseconds, memoryUsed);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogError("Timeout na conversão de múltiplos arquivos após 5 minutos. Tarefas: {TaskCount}", tasks.Count);
                    throw new TimeoutException("Timeout na conversão de arquivos. Tente enviar menos arquivos por vez.");
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    _logger.LogError(ex, "Erro durante Task.WhenAll após {ElapsedTime}ms. Tarefas: {TaskCount}", 
                        stopwatch.ElapsedMilliseconds, tasks.Count);
                    throw;
                }
                
                _logger.LogInformation("Conversão concluída com sucesso. Total de arquivos: {TotalFiles}", arquivos.Count);
                IncrementarContadorGlobal(arquivos.Count);

                ViewBag.DownloadLink = Url.Action("DownloadFiles", new { sessionId })!;
                return View(sourceView);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante o processo de conversão. SessionId: {SessionId}", sessionId);
                ModelState.AddModelError("", "Erro interno durante a conversão. Tente novamente.");
                return View(sourceView);
            }
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
                _logger.LogInformation("API - Iniciando conversão de arquivos. Quantidade: {Count}, Qualidade: {Quality}", 
                    arquivos?.Count ?? 0, qualidade);
                
                if (arquivos == null || arquivos.Count == 0)
                {
                    _logger.LogWarning("API - Tentativa de conversão sem arquivos selecionados");
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

        [HttpPost("api/comprimir-jpeg")]
        [RequestSizeLimit(104857600)] // 100MB
        [RequestFormLimits(
            MultipartBodyLengthLimit = 104857600,
            ValueLengthLimit = 104857600,
            MultipartHeadersLengthLimit = 104857600)]
        public async Task<IActionResult> ComprimirJpegApi(List<IFormFile>? arquivos, int qualidade = 75)
        {
            try
            {
                _logger.LogWarning("API JPEG - Esta é a API que retorna JSON! Quantidade: {Count}, Qualidade: {Quality}, UserAgent: {UserAgent}",
                    arquivos?.Count ?? 0, qualidade, Request.Headers.UserAgent.ToString());

                if (arquivos == null || arquivos.Count == 0)
                {
                    _logger.LogWarning("API - Tentativa de compressão sem arquivos selecionados");
                    return Json(new
                    {
                        success = false,
                        message = "Por favor, selecione um ou mais arquivos."
                    });
                }

                // Validar tipos de arquivo
                var tiposPermitidos = new HashSet<string> { "image/jpeg" };
                foreach (var arquivo in arquivos)
                {
                    if (!tiposPermitidos.Contains(arquivo.ContentType))
                    {
                        return Json(new
                        {
                            success = false,
                            message = $"Tipo de arquivo não permitido: {arquivo.ContentType}. Apenas JPEG é suportado."
                        });
                    }
                }

                // Validar tamanho total dos arquivos (100MB máximo)
                const long maxTotalSize = 100 * 1024 * 1024; // 100MB
                long totalSize = arquivos.Sum(f => f.Length);
                if (totalSize > maxTotalSize)
                {
                    var totalMB = Math.Round(totalSize / (1024.0 * 1024.0), 1);
                    return Json(new
                    {
                        success = false,
                        message = $"Tamanho total dos arquivos ({totalMB}MB) excede o limite de 100MB."
                    });
                }

                var sessionId = Guid.NewGuid().ToString();
                var caminhoUpload = Path.Combine(Directory.GetCurrentDirectory(), PastaRaiz, PastaUploads, sessionId);
                var caminhoConvertido = Path.Combine(Directory.GetCurrentDirectory(), PastaRaiz, PastaConvertidos, sessionId);

                Directory.CreateDirectory(caminhoUpload);
                Directory.CreateDirectory(caminhoConvertido);

                List<Task> tasks = [];
                foreach (var arquivo in arquivos)
                {
                    var task = Task.Run(async () =>
                    {
                        if (arquivo.Length > 0)
                        {
                            var nomeArquivo = Path.GetFileNameWithoutExtension(arquivo.FileName);
                            var caminhoCompletoConvertido = Path.Combine(caminhoConvertido, nomeArquivo + ".jpeg");

                            byte[] jpegImage = await CompressJpeg(arquivo, qualidade);
                            await System.IO.File.WriteAllBytesAsync(caminhoCompletoConvertido, jpegImage);
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
                    message = "Compressão concluída com sucesso!",
                    downloadLink = downloadLink,
                    sessionId = sessionId,
                    filesCount = arquivos.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na compressão de arquivos JPEG");
                return Json(new
                {
                    success = false,
                    message = "Erro interno na compressão: " + ex.Message
                });
            }
        }

        private async Task<byte[]> ConvertToWebP(IFormFile file, int qualidade)
        {
            string threadId = Thread.CurrentThread.ManagedThreadId.ToString();
            try
            {
                _logger.LogInformation("Thread {ThreadId} - Iniciando conversão WebP do arquivo {FileName} (Tamanho: {FileSizeMB:F2}MB, Qualidade: {Quality})", 
                    threadId, file.FileName, file.Length / (1024.0 * 1024.0), qualidade);
                
                // Carregar o arquivo em memória para evitar problemas de concorrência no stream
                byte[] fileBytes;
                await using (var imageStream = file.OpenReadStream())
                {
                    using var memoryStream = new MemoryStream();
                    await imageStream.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }
                
                _logger.LogDebug("Thread {ThreadId} - Arquivo {FileName} carregado em memória ({Bytes} bytes)", 
                    threadId, file.FileName, fileBytes.Length);
                
                using var image = Image.Load(fileBytes);
                
                _logger.LogInformation("Thread {ThreadId} - Imagem carregada: {FileName} - Dimensões: {Width}x{Height}, Formato: {Format}", 
                    threadId, file.FileName, image.Width, image.Height, image.Metadata.DecodedImageFormat?.Name ?? "Desconhecido");
                
                // Otimização automática baseada no tamanho do arquivo
                var tamanhoOriginalMB = file.Length / (1024.0 * 1024.0);
                var perfilOtimizacao = DeterminarPerfilOtimizacao(tamanhoOriginalMB, qualidade);
                
                _logger.LogInformation("Thread {ThreadId} - Perfil de otimização para {FileName}: Qualidade={Quality}, Método={Method}, Formato={FileFormat}", 
                    threadId, file.FileName, perfilOtimizacao.Quality, perfilOtimizacao.Method, perfilOtimizacao.FileFormat);
            
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
                var resultBytes = output.ToArray();
                
                _logger.LogInformation("Thread {ThreadId} - Conversão WebP concluída para {FileName}. Tamanho final: {FinalSizeMB:F2}MB (Redução: {ReductionPercent:F1}%)", 
                    threadId, file.FileName, 
                    resultBytes.Length / (1024.0 * 1024.0),
                    (1 - (double)resultBytes.Length / file.Length) * 100);
                
                return resultBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Thread {ThreadId} - Erro durante conversão WebP do arquivo {FileName}: {ErrorMessage}. StackTrace: {StackTrace}", 
                    threadId, file.FileName, ex.Message, ex.StackTrace);
                throw; // Re-throw para ser capturado pelo middleware global
            }
        }

        private async Task<byte[]> CompressJpeg(IFormFile file, int quality)
        {
            using var imageStream = file.OpenReadStream();
            using var image = await Image.LoadAsync(imageStream);

            using var output = new MemoryStream();
            var encoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder
            {
                Quality = quality
            };

            await image.SaveAsJpegAsync(output, encoder);
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
            try
            {
                _logger.LogInformation("Iniciando download de arquivos para sessão {SessionId}", sessionId);
                
                var caminhoConvertidos = Path.Combine(Directory.GetCurrentDirectory(), PastaRaiz, PastaConvertidos, sessionId);
                
                if (!Directory.Exists(caminhoConvertidos))
                {
                    _logger.LogWarning("Diretório de arquivos convertidos não encontrado para sessão {SessionId}", sessionId);
                    return NotFound("Arquivos não encontrados.");
                }

                var arquivos = Directory.GetFiles(caminhoConvertidos);
                
                if (arquivos.Length == 0)
                {
                    _logger.LogWarning("Nenhum arquivo encontrado no diretório para sessão {SessionId}", sessionId);
                    return NotFound("Nenhum arquivo encontrado.");
                }
                
                // Se houver apenas um arquivo, baixar diretamente
                if (arquivos.Length == 1)
                {
                    var arquivo = arquivos[0];
                    var nomeArquivo = Path.GetFileName(arquivo);
                    var bytes = System.IO.File.ReadAllBytes(arquivo);
                    var extension = Path.GetExtension(nomeArquivo).ToLowerInvariant();
                    var contentType = extension == ".webp" ? "image/webp" : "image/jpeg";

                    var nomeDownload = $"wepper-{nomeArquivo}";
                    _logger.LogInformation("Download de arquivo único {FileName} para sessão {SessionId}", nomeArquivo, sessionId);
                    return File(bytes, contentType, nomeDownload);
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
                var nomeZipDownload = $"wepper-{sessionId}.zip";
                
                _logger.LogInformation("Download de arquivo ZIP com {FileCount} arquivos para sessão {SessionId}", 
                    arquivos.Length, sessionId);
                
                return File(fileBytes, "application/zip", nomeZipDownload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no download de arquivos para sessão {SessionId}", sessionId);
                return StatusCode(500, "Erro interno no servidor");
            }
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

        [HttpGet("comprimir-jpeg")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult ComprimirJpeg()
        {
            // Adicionar headers anti-cache específicos para evitar problemas de cache
            Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.Headers.Append("Pragma", "no-cache");
            Response.Headers.Append("Expires", "0");
            Response.Headers.Append("Content-Type", "text/html; charset=utf-8");
            
            ViewData["Title"] = "Comprimir JPEG Online Gratuito";
            ViewData["Description"] = "Comprima e otimize suas imagens JPEG online gratuitamente. Reduza o tamanho de arquivos JPG/JPEG mantendo a melhor qualidade para web.";
            ViewData["Keywords"] = "comprimir jpeg, otimizar jpeg, reduzir tamanho jpeg, compressor de imagem jpeg";
            
            _logger.LogInformation("GET - Acesso à página comprimir-jpeg");
            
            return View("CompressorJpeg");
        }

        [HttpGet("comprimir-png")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult ComprimirPng()
        {
            // Adicionar headers anti-cache específicos para evitar problemas de cache
            Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.Headers.Append("Pragma", "no-cache");
            Response.Headers.Append("Expires", "0");
            Response.Headers.Append("Content-Type", "text/html; charset=utf-8");
            
            ViewData["Title"] = "Comprimir PNG Online Gratuito";
            ViewData["Description"] = "Comprima e otimize suas imagens PNG online gratuitamente. Reduza o tamanho de arquivos PNG mantendo transparência e a melhor qualidade para web.";
            ViewData["Keywords"] = "comprimir png, otimizar png, reduzir tamanho png, compressor de imagem png, png transparência";
            
            _logger.LogInformation("GET - Acesso à página comprimir-png");
            
            return View("CompressorPng");
        }

        [HttpPost("comprimir-jpeg")]
        [RequestSizeLimit(104857600)] // 100MB
        [RequestFormLimits(
            MultipartBodyLengthLimit = 104857600,
            ValueLengthLimit = 104857600,
            MultipartHeadersLengthLimit = 104857600)]
        public async Task<IActionResult> ComprimirJpegPost(List<IFormFile>? arquivos, int qualidade = 75)
        {
            try
            {
                _logger.LogInformation("POST - Iniciando compressão de arquivos JPEG. Quantidade: {Count}, Qualidade: {Quality}",
                    arquivos?.Count ?? 0, qualidade);

                if (arquivos == null || arquivos.Count == 0)
                {
                    _logger.LogWarning("POST - Tentativa de compressão sem arquivos selecionados");
                    ModelState.AddModelError("files", "Por favor, selecione um ou mais arquivos.");
                    return View("CompressorJpeg");
                }

                // Validar tipos de arquivo
                var tiposPermitidos = new HashSet<string> { "image/jpeg" };
                foreach (var arquivo in arquivos)
                {
                    if (!tiposPermitidos.Contains(arquivo.ContentType))
                    {
                        ModelState.AddModelError("files", $"Tipo de arquivo não permitido: {arquivo.ContentType}. Apenas JPEG é suportado.");
                        return View("CompressorJpeg");
                    }
                }

                // Validar tamanho total dos arquivos (100MB máximo)
                const long maxTotalSize = 100 * 1024 * 1024; // 100MB
                long totalSize = arquivos.Sum(f => f.Length);
                if (totalSize > maxTotalSize)
                {
                    var totalMB = Math.Round(totalSize / (1024.0 * 1024.0), 1);
                    ModelState.AddModelError("files", $"Tamanho total dos arquivos ({totalMB}MB) excede o limite de 100MB.");
                    return View("CompressorJpeg");
                }

                var sessionId = Guid.NewGuid().ToString();
                var caminhoUpload = Path.Combine(Directory.GetCurrentDirectory(), PastaRaiz, PastaUploads, sessionId);
                var caminhoConvertido = Path.Combine(Directory.GetCurrentDirectory(), PastaRaiz, PastaConvertidos, sessionId);

                Directory.CreateDirectory(caminhoUpload);
                Directory.CreateDirectory(caminhoConvertido);

                int totalFiles = arquivos.Count;
                int arquivosProcessados = 0;

                List<Task> tasks = [];
                var semaphore = new SemaphoreSlim(Environment.ProcessorCount > 2 ? 3 : 1, Environment.ProcessorCount > 2 ? 3 : 1);

                foreach (var arquivo in arquivos)
                {
                    var task = Task.Run(async () =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            if (arquivo.Length > 0)
                            {
                                var nomeArquivo = Path.GetFileNameWithoutExtension(arquivo.FileName);
                                var caminhoCompletoConvertido = Path.Combine(caminhoConvertido, nomeArquivo + ".jpeg");

                                byte[] jpegImage = await CompressJpeg(arquivo, qualidade);
                                await System.IO.File.WriteAllBytesAsync(caminhoCompletoConvertido, jpegImage);

                                Interlocked.Increment(ref arquivosProcessados);
                                int progress = (int)((float)arquivosProcessados / totalFiles * 100);

                                try 
                                {
                                    await _progressHub.Clients.All.SendAsync("ReceiveProgress", progress);
                                }
                                catch (Exception hubEx)
                                {
                                    _logger.LogWarning(hubEx, "Erro ao enviar progresso via SignalR para {FileName}", arquivo.FileName);
                                }

                                _logger.LogInformation("Arquivo {FileName} comprimido com sucesso. Progresso: {Progress}%", 
                                    arquivo.FileName, progress);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Erro ao comprimir arquivo {FileName}", arquivo.FileName);
                            throw;
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);

                IncrementarContadorGlobal(arquivos.Count);

                ViewBag.DownloadLink = Url.Action("DownloadFiles", new { sessionId })!;
                ViewData["Title"] = "Compressão JPEG Concluída";
                
                _logger.LogInformation("Compressão JPEG concluída com sucesso para {TotalFiles} arquivos", arquivos.Count);
                
                return View("CompressorJpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na compressão de arquivos JPEG");
                ModelState.AddModelError("", "Erro interno na compressão: " + ex.Message);
                return View("CompressorJpeg");
            }
        }

        [HttpPost("comprimir-png")]
        [RequestSizeLimit(104857600)] // 100MB
        [RequestFormLimits(
            MultipartBodyLengthLimit = 104857600,
            ValueLengthLimit = 104857600,
            MultipartHeadersLengthLimit = 104857600)]
        public async Task<IActionResult> ComprimirPngPost(List<IFormFile>? arquivos, int qualidade = 75)
        {
            try
            {
                _logger.LogInformation("POST - Iniciando compressão de arquivos PNG. Quantidade: {Count}, Qualidade: {Quality}",
                    arquivos?.Count ?? 0, qualidade);

                if (arquivos == null || arquivos.Count == 0)
                {
                    _logger.LogWarning("POST - Tentativa de compressão sem arquivos selecionados");
                    ModelState.AddModelError("files", "Por favor, selecione um ou mais arquivos.");
                    return View("CompressorPng");
                }

                // Validar tipos de arquivo
                var tiposPermitidos = new HashSet<string> { "image/png" };
                foreach (var arquivo in arquivos)
                {
                    if (!tiposPermitidos.Contains(arquivo.ContentType))
                    {
                        ModelState.AddModelError("files", $"Tipo de arquivo não permitido: {arquivo.ContentType}. Apenas PNG é suportado.");
                        return View("CompressorPng");
                    }
                }

                // Validar tamanho total dos arquivos (100MB máximo)
                const long maxTotalSize = 100 * 1024 * 1024; // 100MB
                long totalSize = arquivos.Sum(f => f.Length);
                if (totalSize > maxTotalSize)
                {
                    var totalMB = Math.Round(totalSize / (1024.0 * 1024.0), 1);
                    ModelState.AddModelError("files", $"Tamanho total dos arquivos ({totalMB}MB) excede o limite de 100MB.");
                    return View("CompressorPng");
                }

                var sessionId = Guid.NewGuid().ToString();
                var caminhoUpload = Path.Combine(Directory.GetCurrentDirectory(), PastaRaiz, PastaUploads, sessionId);
                var caminhoConvertido = Path.Combine(Directory.GetCurrentDirectory(), PastaRaiz, PastaConvertidos, sessionId);

                Directory.CreateDirectory(caminhoUpload);
                Directory.CreateDirectory(caminhoConvertido);

                // Usar semáforo para controlar concorrência (máximo 4 processamentos simultâneos)
                using var semaphore = new SemaphoreSlim(4, 4);
                var tasks = new List<Task>();

                int progress = 0;
                int total = arquivos.Count;

                foreach (var arquivo in arquivos)
                {
                    var task = Task.Run(async () =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            using var progressScope = _logger.BeginScope(new Dictionary<string, object>
                            {
                                ["FileName"] = arquivo.FileName,
                                ["SessionId"] = sessionId
                            });

                            _logger.LogInformation("Iniciando compressão PNG do arquivo {FileName}", arquivo.FileName);

                            // Converter PNG para WebP (compressão superior mantendo transparência)
                            var imagemComprimida = await ConvertToWebP(arquivo, qualidade);
                            
                            // Salvar arquivo comprimido com extensão .webp
                            var nomeArquivo = Path.GetFileNameWithoutExtension(arquivo.FileName) + ".webp";
                            var caminhoArquivo = Path.Combine(caminhoConvertido, nomeArquivo);
                            await System.IO.File.WriteAllBytesAsync(caminhoArquivo, imagemComprimida);

                            // Calcular e reportar progresso
                            var currentProgress = Interlocked.Increment(ref progress);
                            var progressPercent = (int)((double)currentProgress / total * 100);

                            if (_progressHub != null)
                            {
                                await _progressHub.Clients.All.SendAsync("UpdateProgress", sessionId, progressPercent);
                                _logger.LogInformation("Arquivo {FileName} comprimido com sucesso. Progresso: {Progress}%", 
                                    arquivo.FileName, progress);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Erro ao comprimir arquivo {FileName}", arquivo.FileName);
                            throw;
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);

                IncrementarContadorGlobal(arquivos.Count);

                ViewBag.DownloadLink = Url.Action("DownloadFiles", new { sessionId })!;
                ViewData["Title"] = "Compressão PNG Concluída";
                
                _logger.LogInformation("Compressão PNG concluída com sucesso para {TotalFiles} arquivos", arquivos.Count);
                
                return View("CompressorPng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na compressão de arquivos PNG");
                ModelState.AddModelError("", "Erro interno na compressão: " + ex.Message);
                return View("CompressorPng");
            }
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

        // Rotas para páginas de compressão JPEG em inglês e espanhol
        [HttpGet("en/compress-jpeg")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult ComprimirJpegEnglish()
        {
            SetCultureContent("en");
            ViewData["Title"] = "Compress JPEG Online Free - Reduce Image Size";
            ViewData["Description"] = "Free JPEG compressor online. Reduce JPEG file size while maintaining excellent quality. Batch compression up to 100MB. No watermarks, fast processing.";
            ViewData["Keywords"] = "compress jpeg, optimize jpeg, reduce jpeg size, jpeg compressor, jpg optimizer, image compression";
            
            _logger.LogInformation("GET - Acesso à página compress-jpeg (English)");
            return View("CompressorJpegEnglish");
        }

        [HttpGet("es/comprimir-jpeg")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult ComprimirJpegSpanish()
        {
            SetCultureContent("es");
            ViewData["Title"] = "Comprimir JPEG Online Gratis - Reducir Tamaño de Imágenes";
            ViewData["Description"] = "Compresor JPEG gratis online. Reduce el tamaño de archivos JPEG manteniendo excelente calidad. Compresión por lotes hasta 100MB. Sin marcas de agua.";
            ViewData["Keywords"] = "comprimir jpeg, optimizar jpeg, reducir tamaño jpeg, compresor de imagen jpeg, optimizador jpg";
            
            _logger.LogInformation("GET - Acesso à página comprimir-jpeg (Spanish)");
            return View("CompressorJpegSpanish");
        }

        // Rotas para páginas de compressão PNG em inglês e espanhol
        [HttpGet("en/compress-png")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult ComprimirPngEnglish()
        {
            SetCultureContent("en");
            ViewData["Title"] = "Compress PNG Online Free - Preserve Transparency";
            ViewData["Description"] = "Free PNG compressor online. Reduce PNG file size while preserving transparency and alpha channel. Perfect for logos and icons. Batch processing available.";
            ViewData["Keywords"] = "compress png, optimize png, reduce png size, png compressor, png transparency, image compression";
            
            _logger.LogInformation("GET - Acesso à página compress-png (English)");
            return View("CompressorPngEnglish");
        }

        [HttpGet("es/comprimir-png")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult ComprimirPngSpanish()
        {
            SetCultureContent("es");
            ViewData["Title"] = "Comprimir PNG Online Gratis - Preservar Transparencia";
            ViewData["Description"] = "Compresor PNG gratis online. Reduce el tamaño de archivos PNG preservando transparencia y canal alfa. Perfecto para logos e iconos. Procesamiento por lotes.";
            ViewData["Keywords"] = "comprimir png, optimizar png, reducir tamaño png, compresor de imagen png, transparencia png";
            
            _logger.LogInformation("GET - Acesso à página comprimir-png (Spanish)");
            return View("CompressorPngSpanish");
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
                    _logger.LogDebug("Iniciando leitura do contador global. ContadorPath: {ContadorPath}", ContadorPath);
                    _logger.LogDebug("ApplicationSettings - ContadorInicial: {ContadorInicial}", _appSettings.ContadorInicial);
                    
                    // Criar diretório se não existir
                    var directoryPath = Path.GetDirectoryName(ContadorPath);
                    if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                        _logger.LogInformation("Diretório criado: {DirectoryPath}", directoryPath);
                    }
                    
                    int contador = 0;
                    
                    if (System.IO.File.Exists(ContadorPath))
                    {
                        var json = System.IO.File.ReadAllText(ContadorPath);
                        _logger.LogDebug("Conteúdo do arquivo contador.json: {Json}", json);
                        
                        var obj = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(json);
                        contador = obj != null && obj.ContainsKey("total") ? obj["total"] : 0;
                        
                        _logger.LogInformation("Contador lido do arquivo: {Contador}", contador);
                    }
                    else
                    {
                        _logger.LogWarning("Arquivo contador.json não existe: {ContadorPath}", ContadorPath);
                    }
                    
                    // Se o contador estiver em 0, inicializar com o valor do appsettings
                    if (contador == 0 && _appSettings.ContadorInicial > 0)
                    {
                        contador = _appSettings.ContadorInicial;
                        
                        // Salvar o valor inicial no arquivo
                        var obj = new Dictionary<string, int> { { "total", contador } };
                        var jsonString = System.Text.Json.JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
                        System.IO.File.WriteAllText(ContadorPath, jsonString);
                        
                        _logger.LogInformation("Contador inicializado com valor do appsettings: {ContadorInicial}", contador);
                    }
                    else if (contador == 0)
                    {
                        _logger.LogWarning("Contador em zero e ContadorInicial também é zero. AppSettings: {AppSettings}", 
                            System.Text.Json.JsonSerializer.Serialize(_appSettings));
                    }
                    
                    _logger.LogInformation("Contador final retornado: {Contador}", contador);
                    return contador;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao ler contador global. AppSettings ContadorInicial: {ContadorInicial}", 
                        _appSettings.ContadorInicial);
                    
                    // Em caso de erro, retornar o valor inicial do appsettings se configurado
                    var fallbackValue = _appSettings.ContadorInicial > 0 ? _appSettings.ContadorInicial : 0;
                    _logger.LogWarning("Retornando valor fallback: {FallbackValue}", fallbackValue);
                    return fallbackValue;
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
