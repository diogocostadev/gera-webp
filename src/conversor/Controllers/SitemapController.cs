using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Xml;

namespace GeraWebP.Controllers
{
    public class SitemapController : Controller
    {
        [HttpGet("sitemap.xml")]
        public IActionResult Sitemap()
        {
            var urls = new List<SitemapUrl>
            {
                // Página principal em português (padrão)
                new SitemapUrl 
                { 
                    Url = "https://wepper.vip4.link/", 
                    Priority = "1.0", 
                    ChangeFreq = "weekly" 
                },
                
                // Páginas secundárias em português
                new SitemapUrl 
                { 
                    Url = "https://wepper.vip4.link/privacidade", 
                    Priority = "0.5", 
                    ChangeFreq = "monthly" 
                },
                new SitemapUrl 
                { 
                    Url = "https://wepper.vip4.link/converter-jpg-para-webp", 
                    Priority = "0.8", 
                    ChangeFreq = "weekly" 
                },
                new SitemapUrl 
                { 
                    Url = "https://wepper.vip4.link/converter-png-para-webp", 
                    Priority = "0.8", 
                    ChangeFreq = "weekly" 
                },
                new SitemapUrl 
                { 
                    Url = "https://wepper.vip4.link/converter-gif-para-webp", 
                    Priority = "0.8", 
                    ChangeFreq = "weekly" 
                },
                new SitemapUrl 
                { 
                    Url = "https://wepper.vip4.link/compressor-imagem", 
                    Priority = "0.8", 
                    ChangeFreq = "weekly" 
                },
                
                // Versões internacionais
                new SitemapUrl 
                { 
                    Url = "https://wepper.vip4.link/en/", 
                    Priority = "0.9", 
                    ChangeFreq = "weekly" 
                },
                new SitemapUrl 
                { 
                    Url = "https://wepper.vip4.link/es/", 
                    Priority = "0.9", 
                    ChangeFreq = "weekly" 
                },
                new SitemapUrl 
                { 
                    Url = "https://wepper.vip4.link/en/privacy", 
                    Priority = "0.4", 
                    ChangeFreq = "monthly" 
                },
                new SitemapUrl 
                { 
                    Url = "https://wepper.vip4.link/es/privacidad", 
                    Priority = "0.4", 
                    ChangeFreq = "monthly" 
                }
            };

            var xml = GenerateSitemapXml(urls);
            return Content(xml, "application/xml", Encoding.UTF8);
        }

        private static string GenerateSitemapXml(List<SitemapUrl> urls)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8
            };

            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, settings);
            
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

            foreach (var url in urls)
            {
                xmlWriter.WriteStartElement("url");
                xmlWriter.WriteElementString("loc", url.Url);
                xmlWriter.WriteElementString("lastmod", DateTime.Now.ToString("yyyy-MM-dd"));
                xmlWriter.WriteElementString("changefreq", url.ChangeFreq);
                xmlWriter.WriteElementString("priority", url.Priority);
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();

            return stringWriter.ToString();
        }
    }

    public class SitemapUrl
    {
        public string Url { get; set; } = string.Empty;
        public string Priority { get; set; } = "0.5";
        public string ChangeFreq { get; set; } = "monthly";
    }
} 