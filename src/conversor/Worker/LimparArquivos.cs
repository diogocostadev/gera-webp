namespace GeraWebP.Worker;

public class LimparArquivos : BackgroundService
{
    private const string PastaRaiz = "wwwroot";
    private const string PastaConvertidos = "convertidos";
    private const string PastaUploads = "uploads";
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var directoryInfo = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), PastaRaiz, PastaUploads));
            foreach (var directory in directoryInfo.GetDirectories())
            {
                if (directory.CreationTime < DateTime.Now.AddMinutes(-1)) 
                {
                    directory.Delete(true); 
                }
            }

            directoryInfo = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), PastaRaiz, PastaConvertidos));
            foreach (var directory in directoryInfo.GetDirectories())
            {
                if (directory.CreationTime < DateTime.Now.AddMinutes(-30)) 
                {
                    directory.Delete(true); 
                }
            }
            
            await Task.Delay(new TimeSpan(0, 30, 0), stoppingToken);
        }
    }
}