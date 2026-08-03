using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Web;
using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;

namespace BookSmith.Services.Export;

public class EpubExporter : IEpubExporter
{
    public void Export(EpubExportOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(options.OutputPath))
            throw new ArgumentException("Output path cannot be null or empty.", nameof(options));

        string directory = Path.GetDirectoryName(options.OutputPath) ?? string.Empty;
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(options.OutputPath))
        {
            File.Delete(options.OutputPath);
        }

        using (var fileStream = new FileStream(options.OutputPath, FileMode.Create, FileAccess.Write))
        using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
        {
            // 1. mimetype (Must be uncompressed first entry)
            var mimeEntry = archive.CreateEntry("mimetype", CompressionLevel.NoCompression);
            using (var writer = new StreamWriter(mimeEntry.Open(), Encoding.ASCII))
            {
                writer.Write("application/epub+zip");
            }

            // 2. META-INF/container.xml
            var containerEntry = archive.CreateEntry("META-INF/container.xml", CompressionLevel.Optimal);
            using (var writer = new StreamWriter(containerEntry.Open(), Encoding.UTF8))
            {
                writer.Write(@"<?xml me=""1.0"" encoding=""UTF-8""?>
<container version=""1.0"" xmlns=""urn:oasis:names:tc:opendocument:xmlns:container"">
  <rootfiles>
    <rootfile full-path=""OEBPS/content.opf"" media-type=""application/oebps-package+xml""/>
  </rootfiles>
</container>");
            }

            string bookId = "urn:uuid:" + Guid.NewGuid().ToString();
            string title = HttpUtility.HtmlEncode(options.Title ?? "Untitled Book");
            string author = HttpUtility.HtmlEncode(options.Author ?? "Unknown Author");
            string language = HttpUtility.HtmlEncode(options.Language ?? "tr");

            // 3. OEBPS/content.opf
            var opfEntry = archive.CreateEntry("OEBPS/content.opf", CompressionLevel.Optimal);
            using (var writer = new StreamWriter(opfEntry.Open(), Encoding.UTF8))
            {
                writer.Write($@"<?xml version=""1.0"" encoding=""UTF-8""?>
<package xmlns=""http://www.idpf.org/2007/opf"" unique-identifier=""BookId"" version=""2.0"">
  <metadata xmlns:dc=""http://purl.org/dc/elements/1.1/"" xmlns:opf=""http://www.idpf.org/2007/opf"">
    <dc:title>{title}</dc:title>
    <dc:creator opf:role=""aut"">{author}</dc:creator>
    <dc:language>{language}</dc:language>
    <dc:identifier id=""BookId"">{bookId}</dc:identifier>
  </metadata>
  <manifest>
    <item id=""ncx"" href=""toc.ncx"" media-type=""application/x-dtbncx+xml""/>
    <item id=""style"" href=""style.css"" media-type=""text/css""/>
    <item id=""chapter1"" href=""chapter1.xhtml"" media-type=""application/xhtml+xml""/>
  </manifest>
  <spine toc=""ncx"">
    <itemref idref=""chapter1""/>
  </spine>
</package>");
            }

            // 4. OEBPS/toc.ncx
            var ncxEntry = archive.CreateEntry("OEBPS/toc.ncx", CompressionLevel.Optimal);
            using (var writer = new StreamWriter(ncxEntry.Open(), Encoding.UTF8))
            {
                writer.Write($@"<?xml version=""1.0"" encoding=""UTF-8""?>
<ncx xmlns=""http://www.daisy.org/z3986/2005/ncx/"" version=""2005-1"">
  <head>
    <meta name=""dtb:uid"" content=""{bookId}""/>
    <meta name=""dtb:depth"" content=""1""/>
    <meta name=""dtb:totalPageCount"" content=""0""/>
    <meta name=""dtb:maxPageNumber"" content=""0""/>
  </head>
  <docTitle>
    <text>{title}</text>
  </docTitle>
  <navMap>
    <navPoint id=""navPoint-1"" playOrder=""1"">
      <navLabel>
        <text>Start Reading</text>
      </navLabel>
      <content src=""chapter1.xhtml""/>
    </navPoint>
  </navMap>
</ncx>");
            }

            // 5. OEBPS/style.css
            var cssEntry = archive.CreateEntry("OEBPS/style.css", CompressionLevel.Optimal);
            using (var writer = new StreamWriter(cssEntry.Open(), Encoding.UTF8))
            {
                writer.Write(@"body {
    font-family: Georgia, 'Times New Roman', serif;
    margin: 5%;
    line-height: 1.6;
}
p {
    text-indent: 1.5em;
    margin-top: 0;
    margin-bottom: 0.5em;
}");
            }

            // 6. OEBPS/chapter1.xhtml
            var chapterEntry = archive.CreateEntry("OEBPS/chapter1.xhtml", CompressionLevel.Optimal);
            using (var writer = new StreamWriter(chapterEntry.Open(), Encoding.UTF8))
            {
                var htmlBuilder = new StringBuilder();
                htmlBuilder.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
                htmlBuilder.AppendLine(@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.1//EN"" ""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"">");
                htmlBuilder.AppendLine(@"<html xmlns=""http://www.w3.org/1999/xhtml"">");
                htmlBuilder.AppendLine(@"<head>");
                htmlBuilder.AppendLine($@"  <title>{title}</title>");
                htmlBuilder.AppendLine(@"  <link rel=""stylesheet"" type=""text/css"" href=""style.css""/>");
                htmlBuilder.AppendLine(@"</head>");
                htmlBuilder.AppendLine(@"<body>");

                string rawText = options.ContentText ?? string.Empty;
                string[] paragraphs = rawText.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string p in paragraphs)
                {
                    string trimmed = p.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        string encoded = HttpUtility.HtmlEncode(trimmed).Replace("\n", "<br/>");
                        htmlBuilder.AppendLine($"  <p>{encoded}</p>");
                    }
                }

                htmlBuilder.AppendLine(@"</body>");
                htmlBuilder.AppendLine(@"</html>");

                writer.Write(htmlBuilder.ToString());
            }
        }
    }
}
