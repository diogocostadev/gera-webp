namespace GeraWebP.Worker;

public class LimparArquivos : BackgroundService
{
    private readonly ILogger<LimparArquivos> _logger;
    private const string PastaRaiz = "wwwroot";
    private const string PastaConvertidos = "convertidos";
    private const string PastaUploads = "uploads";
    private const int MinutosParaLimpeza = 5; // 5 minutos para sistema gratuito
    
    public LimparArquivos(ILogger<LimparArquivos> logger)
    {
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🧹 Serviço de Limpeza Automática iniciado - Intervalo: {MinutosParaLimpeza} minutos", MinutosParaLimpeza);
        
        // Garantir que as pastas existem
        EnsureDirectoriesExist();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                LimparArquivosAntigos();
                
                // Executa a limpeza a cada 2 minutos para ser mais eficiente
                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro durante a limpeza automática de arquivos");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Retry em 1 minuto se houver erro
            }
        }
    }

    private void EnsureDirectoriesExist()
    {
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), PastaRaiz, PastaUploads);
        var convertidosPath = Path.Combine(Directory.GetCurrentDirectory(), PastaRaiz, PastaConvertidos);
        
        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
            _logger.LogInformation("📁 Pasta de uploads criada: {Path}", uploadsPath);
        }
        
        if (!Directory.Exists(convertidosPath))
        {
            Directory.CreateDirectory(convertidosPath);
            _logger.LogInformation("📁 Pasta de convertidos criada: {Path}", convertidosPath);
        }
    }

    private void LimparArquivosAntigos()
    {
        var timestampLimpeza = DateTime.Now.AddMinutes(-MinutosParaLimpeza);
        int totalPastasRemovidas = 0;
        long totalBytesLiberados = 0;

        // Limpar uploads (arquivos originais)
        var resultadoUploads = LimparPasta(PastaUploads, timestampLimpeza);
        totalPastasRemovidas += resultadoUploads.PastasRemovidas;
        totalBytesLiberados += resultadoUploads.BytesLiberados;
        
        // Limpar convertidos (arquivos processados)
        var resultadoConvertidos = LimparPasta(PastaConvertidos, timestampLimpeza);
        totalPastasRemovidas += resultadoConvertidos.PastasRemovidas;
        totalBytesLiberados += resultadoConvertidos.BytesLiberados;

        // Limpar arquivos ZIP antigos na pasta convertidos
        var bytesZipLiberados = LimparArquivosZip(timestampLimpeza);
        totalBytesLiberados += bytesZipLiberados;

        if (totalPastasRemovidas > 0 || totalBytesLiberados > 0)
        {
            var megabytesLiberados = totalBytesLiberados / (1024.0 * 1024.0);
            _logger.LogInformation("🧹 Limpeza concluída: {PastasRemovidas} pastas removidas, {MB:F2} MB liberados", 
                totalPastasRemovidas, megabytesLiberados);
        }
    }

    private (int PastasRemovidas, long BytesLiberados) LimparPasta(string nomePasta, DateTime timestampLimpeza)
    {
        var pastaPath = Path.Combine(Directory.GetCurrentDirectory(), PastaRaiz, nomePasta);
        var directoryInfo = new DirectoryInfo(pastaPath);
        int pastasRemovidas = 0;
        long bytesLiberados = 0;

        if (!directoryInfo.Exists) 
            return (0, 0);

        foreach (var directory in directoryInfo.GetDirectories())
        {
            try
            {
                // Usar LastWriteTime ao invés de CreationTime para ser mais preciso
                if (directory.LastWriteTime < timestampLimpeza)
                {
                    // Calcular tamanho antes de deletar
                    var tamanhoBytes = CalcularTamanhoPasta(directory);
                    
                    directory.Delete(true);
                    pastasRemovidas++;
                    bytesLiberados += tamanhoBytes;
                    
                    _logger.LogDebug("🗑️ Pasta removida: {PastaPath} ({Bytes} bytes)", directory.FullName, tamanhoBytes);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Erro ao remover pasta: {PastaPath}", directory.FullName);
            }
        }

        return (pastasRemovidas, bytesLiberados);
    }

    private long LimparArquivosZip(DateTime timestampLimpeza)
    {
        var convertidosPath = Path.Combine(Directory.GetCurrentDirectory(), PastaRaiz, PastaConvertidos);
        var directoryInfo = new DirectoryInfo(convertidosPath);
        long bytesLiberados = 0;

        if (!directoryInfo.Exists) 
            return 0;

        foreach (var file in directoryInfo.GetFiles("*.zip"))
        {
            try
            {
                if (file.LastWriteTime < timestampLimpeza)
                {
                    var tamanhoBytes = file.Length;
                    file.Delete();
                    bytesLiberados += tamanhoBytes;
                    
                    _logger.LogDebug("🗑️ Arquivo ZIP removido: {ArquivoPath} ({Bytes} bytes)", file.FullName, tamanhoBytes);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Erro ao remover arquivo ZIP: {ArquivoPath}", file.FullName);
            }
        }

        return bytesLiberados;
    }

    private static long CalcularTamanhoPasta(DirectoryInfo directory)
    {
        long tamanho = 0;
        try
        {
            // Calcular tamanho de todos os arquivos na pasta
            foreach (var file in directory.GetFiles("*", SearchOption.AllDirectories))
            {
                tamanho += file.Length;
            }
        }
        catch (Exception)
        {
            // Se não conseguir calcular, retorna 0
        }
        return tamanho;
    }
}