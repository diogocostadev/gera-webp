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
        var sitemap = new StringBuilder();
        sitemap.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sitemap.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        // URLs principais
        var urls = new[]
        {
            new { Url = "/", Priority = "1.0", ChangeFreq = "daily" },
            new { Url = "/en/", Priority = "0.9", ChangeFreq = "daily" },
            new { Url = "/es/", Priority = "0.9", ChangeFreq = "daily" },
            new { Url = "/converter-jpg-para-webp", Priority = "0.8", ChangeFreq = "weekly" },
            new { Url = "/converter-png-para-webp", Priority = "0.8", ChangeFreq = "weekly" },
            new { Url = "/converter-gif-para-webp", Priority = "0.8", ChangeFreq = "weekly" },
            new { Url = "/compressor-imagem", Priority = "0.8", ChangeFreq = "weekly" },
            new { Url = "/privacidade", Priority = "0.5", ChangeFreq = "monthly" },
            new { Url = "/en/privacy", Priority = "0.5", ChangeFreq = "monthly" },
            new { Url = "/es/privacidad", Priority = "0.5", ChangeFreq = "monthly" }
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
        var robots = new StringBuilder();
        robots.AppendLine("User-agent: *");
        robots.AppendLine("Allow: /");
        robots.AppendLine();
        robots.AppendLine("# Bloquear arquivos temporários e uploads");
        robots.AppendLine("Disallow: /wwwroot/uploads/");
        robots.AppendLine("Disallow: /wwwroot/convertidos/");
        robots.AppendLine("Disallow: /temp/");
        robots.AppendLine();
        robots.AppendLine("# Permitir CSS e JS para renderização");
        robots.AppendLine("Allow: /css/");
        robots.AppendLine("Allow: /js/");
        robots.AppendLine("Allow: /lib/");
        robots.AppendLine("Allow: /img/");
        robots.AppendLine();
        robots.AppendLine($"# Sitemap");
        robots.AppendLine($"Sitemap: {baseUrl}/sitemap.xml");
        
        return robots.ToString();
    }
} 