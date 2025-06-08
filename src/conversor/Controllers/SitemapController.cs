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
                    Url = "https://webpconverter.com.br/", 
                    Priority = "1.0", 
                    ChangeFreq = "weekly" 
                },
                
                // Páginas secundárias em português
                new SitemapUrl 
                { 
                    Url = "https://webpconverter.com.br/privacidade", 
                    Priority = "0.5", 
                    ChangeFreq = "monthly" 
                },
                new SitemapUrl 
                { 
                    Url = "https://webpconverter.com.br/converter-jpg-para-webp", 
                    Priority = "0.8", 
                    ChangeFreq = "weekly" 
                },
                new SitemapUrl 
                { 
                    Url = "https://webpconverter.com.br/converter-png-para-webp", 
                    Priority = "0.8", 
                    ChangeFreq = "weekly" 
                },
                new SitemapUrl 
                { 
                    Url = "https://webpconverter.com.br/converter-gif-para-webp", 
                    Priority = "0.8", 
                    ChangeFreq = "weekly" 
                },
                new SitemapUrl 
                { 
                    Url = "https://webpconverter.com.br/compressor-imagem", 
                    Priority = "0.8", 
                    ChangeFreq = "weekly" 
                },
                
                // Versões internacionais
                new SitemapUrl 
                { 
                    Url = "https://webpconverter.com.br/en/", 
                    Priority = "0.9", 
                    ChangeFreq = "weekly" 
                },
                new SitemapUrl 
                { 
                    Url = "https://webpconverter.com.br/es/", 
                    Priority = "0.9", 
                    ChangeFreq = "weekly" 
                },
                new SitemapUrl 
                { 
                    Url = "https://webpconverter.com.br/en/privacy", 
                    Priority = "0.4", 
                    ChangeFreq = "monthly" 
                },
                new SitemapUrl 
                { 
                    Url = "https://webpconverter.com.br/es/privacidad", 
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