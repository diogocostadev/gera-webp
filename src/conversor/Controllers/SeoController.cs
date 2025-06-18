using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Xml;

namespace GeraWebP.Controllers;

[Route("")]
public class SeoController : Controller
{
    [HttpGet("sitemap.xml")]
    [ResponseCache(Duration = 86400)] // Cache por 24 horas
    public IActionResult SitemapXml()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var sitemap = GenerateSitemap(baseUrl);
        
        return Content(sitemap, "application/xml", Encoding.UTF8);
    }

    [HttpGet("robots.txt")]
    [ResponseCache(Duration = 86400)] // Cache por 24 horas
    public IActionResult RobotsTxt()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var robots = GenerateRobotsTxt(baseUrl);
        
        return Content(robots, "text/plain", Encoding.UTF8);
    }

    private string GenerateSitemap(string baseUrl)
    {
        // Usar sempre o domínio de produção
        baseUrl = "https://wepper.app";
        
        var sitemap = new StringBuilder();
        sitemap.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sitemap.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        // URLs principais do wepper.app
        var urls = new[]
        {
            // Páginas principais
            new { Url = "/", Priority = "1.0", ChangeFreq = "daily" },
            new { Url = "/en/", Priority = "0.9", ChangeFreq = "daily" },
            new { Url = "/es/", Priority = "0.9", ChangeFreq = "daily" },
            
            // Ferramentas de conversão (baseado no site real)
            new { Url = "/converter-jpg-para-webp", Priority = "0.9", ChangeFreq = "weekly" },
            new { Url = "/converter-png-para-webp", Priority = "0.9", ChangeFreq = "weekly" },
            new { Url = "/converter-gif-para-webp", Priority = "0.9", ChangeFreq = "weekly" },
            new { Url = "/compressor-imagem", Priority = "0.8", ChangeFreq = "weekly" },
            
            // Páginas informativas
            new { Url = "/como-converter-webp", Priority = "0.7", ChangeFreq = "monthly" },
            new { Url = "/vantagens-formato-webp", Priority = "0.7", ChangeFreq = "monthly" },
            new { Url = "/tutorial-otimizacao-imagens", Priority = "0.6", ChangeFreq = "monthly" },
            
            // Páginas legais
            new { Url = "/privacidade", Priority = "0.4", ChangeFreq = "monthly" },
            new { Url = "/termos-uso", Priority = "0.4", ChangeFreq = "monthly" },
            new { Url = "/politica-cookies", Priority = "0.4", ChangeFreq = "monthly" },
            
            // Versões em inglês
            new { Url = "/en/jpg-to-webp-converter", Priority = "0.8", ChangeFreq = "weekly" },
            new { Url = "/en/png-to-webp-converter", Priority = "0.8", ChangeFreq = "weekly" },
            new { Url = "/en/gif-to-webp-converter", Priority = "0.8", ChangeFreq = "weekly" },
            new { Url = "/en/privacy", Priority = "0.4", ChangeFreq = "monthly" },
            new { Url = "/en/terms", Priority = "0.4", ChangeFreq = "monthly" },
            
            // Versões em espanhol
            new { Url = "/es/convertir-jpg-webp", Priority = "0.8", ChangeFreq = "weekly" },
            new { Url = "/es/convertir-png-webp", Priority = "0.8", ChangeFreq = "weekly" },
            new { Url = "/es/convertir-gif-webp", Priority = "0.8", ChangeFreq = "weekly" },
            new { Url = "/es/privacidad", Priority = "0.4", ChangeFreq = "monthly" },
            new { Url = "/es/terminos", Priority = "0.4", ChangeFreq = "monthly" }
        };

        foreach (var url in urls)
        {
            sitemap.AppendLine("  <url>");
            sitemap.AppendLine($"    <loc>{baseUrl}{url.Url}</loc>");
            sitemap.AppendLine($"    <lastmod>{DateTime.UtcNow:yyyy-MM-dd}</lastmod>");
            sitemap.AppendLine($"    <changefreq>{url.ChangeFreq}</changefreq>");
            sitemap.AppendLine($"    <priority>{url.Priority}</priority>");
            sitemap.AppendLine("  </url>");
        }

        sitemap.AppendLine("</urlset>");
        return sitemap.ToString();
    }

    private string GenerateRobotsTxt(string baseUrl)
    {
        // Usar sempre o domínio de produção
        baseUrl = "https://wepper.app";
        
        var robots = new StringBuilder();
        robots.AppendLine("User-agent: *");
        robots.AppendLine("Allow: /");
        robots.AppendLine();
        robots.AppendLine("# Crawl-delay para ser gentil com o servidor");
        robots.AppendLine("Crawl-delay: 1");
        robots.AppendLine();
        robots.AppendLine("# Bloquear arquivos temporários e sensíveis");
        robots.AppendLine("Disallow: /wwwroot/uploads/");
        robots.AppendLine("Disallow: /wwwroot/convertidos/");
        robots.AppendLine("Disallow: /temp/");
        robots.AppendLine("Disallow: /bin/");
        robots.AppendLine("Disallow: /obj/");
        robots.AppendLine("Disallow: /logs/");
        robots.AppendLine("Disallow: /*.log$");
        robots.AppendLine();
        robots.AppendLine("# Bloquear URLs de API internas");
        robots.AppendLine("Disallow: /api/");
        robots.AppendLine("Disallow: /progressHub");
        robots.AppendLine();
        robots.AppendLine("# Permitir recursos estáticos importantes");
        robots.AppendLine("Allow: /css/");
        robots.AppendLine("Allow: /js/");
        robots.AppendLine("Allow: /lib/");
        robots.AppendLine("Allow: /img/");
        robots.AppendLine("Allow: /images/");
        robots.AppendLine("Allow: /favicon.ico");
        robots.AppendLine("Allow: /robots.txt");
        robots.AppendLine("Allow: /sitemap.xml");
        robots.AppendLine();
        robots.AppendLine("# Sitemap principal");
        robots.AppendLine($"Sitemap: {baseUrl}/sitemap.xml");
        robots.AppendLine();
        robots.AppendLine("# Wepper.app - Conversor WebP Online Gratuito");
        robots.AppendLine("# Converta JPG, PNG, GIF para WebP com qualidade profissional");
        
        return robots.ToString();
    }
} 