namespace GeraWebP.Models
{
    /// <summary>
    /// Configurações da aplicação definidas no appsettings.json
    /// </summary>
    public class ApplicationSettings
    {
        /// <summary>
        /// Nome da aplicação
        /// </summary>
        public string Name { get; set; } = "Wepper";

        /// <summary>
        /// Versão da aplicação
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Data do build
        /// </summary>
        public string BuildDate { get; set; } = DateTime.Now.ToString("yyyy.MM.dd");

        /// <summary>
        /// Descrição da aplicação
        /// </summary>
        public string Description { get; set; } = "Conversor WebP Online Gratuito";

        /// <summary>
        /// Versão completa formatada para exibição
        /// </summary>
        public string DisplayVersion => $"{Name} v{Version}";

        /// <summary>
        /// Informações completas para tooltip
        /// </summary>
        public string FullInfo => $"Versão: {Version} | Build: {BuildDate}";

        /// <summary>
        /// Valor inicial do contador global quando o arquivo contador.json não existir ou estiver em 0
        /// </summary>
        public int ContadorInicial { get; set; } = 0;
    }
}
