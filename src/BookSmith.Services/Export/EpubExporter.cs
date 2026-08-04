using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
            Directory.CreateDirectory(directory);

        if (File.Exists(options.OutputPath))
            File.Delete(options.OutputPath);

        // Split content into chapters if chapter info is available
        var chapters = BuildChapters(options);

        // Check if cover image is provided and exists
        bool hasCover = !string.IsNullOrWhiteSpace(options.CoverImagePath) && File.Exists(options.CoverImagePath);
        string coverExt = hasCover ? Path.GetExtension(options.CoverImagePath!).ToLowerInvariant() : "";
        string coverMime = coverExt == ".png" ? "image/png" : "image/jpeg";
        string coverFileName = coverExt == ".png" ? "cover.png" : "cover.jpg";

        using var fileStream = new FileStream(options.OutputPath, FileMode.Create, FileAccess.Write);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Create);

        // 1. mimetype (Must be uncompressed first entry)
        WriteEntry(archive, "mimetype", "application/epub+zip", CompressionLevel.NoCompression);

        // 2. META-INF/container.xml
        WriteEntry(archive, "META-INF/container.xml", @"<?xml version=""1.0"" encoding=""UTF-8""?>
<container version=""1.0"" xmlns=""urn:oasis:names:tc:opendocument:xmlns:container"">
  <rootfiles>
    <rootfile full-path=""OEBPS/content.opf"" media-type=""application/oebps-package+xml""/>
  </rootfiles>
</container>");

        string bookId = "urn:uuid:" + Guid.NewGuid().ToString();
        string title = HttpUtility.HtmlEncode(options.Title ?? "Untitled Book");
        string author = HttpUtility.HtmlEncode(options.Author ?? "Unknown Author");
        string language = HttpUtility.HtmlEncode(options.Language ?? "tr");

        // 3. Add Cover Image & Page if available
        if (hasCover)
        {
            byte[] imageBytes = File.ReadAllBytes(options.CoverImagePath!);
            WriteBinaryEntry(archive, $"OEBPS/{coverFileName}", imageBytes);
            WriteEntry(archive, "OEBPS/cover.xhtml", BuildCoverXhtml(title, coverFileName));
        }

        // 4. OEBPS/content.opf (dynamic manifest/spine per chapter & cover)
        WriteEntry(archive, "OEBPS/content.opf", BuildOpf(bookId, title, author, language, chapters, hasCover, coverFileName, coverMime));

        // 5. OEBPS/toc.ncx (real chapter nav points)
        WriteEntry(archive, "OEBPS/toc.ncx", BuildNcx(bookId, title, chapters, hasCover));

        // 6. OEBPS/style.css (with user typography settings)
        int fontSize = options.FontSizePt > 0 ? options.FontSizePt : 12;
        double lineHeight = options.LineHeight > 0 ? options.LineHeight : 1.6;
        string lineHeightStr = lineHeight.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);

        WriteEntry(archive, "OEBPS/style.css", $@"body {{
    font-family: Georgia, 'Times New Roman', serif;
    font-size: {fontSize}pt;
    line-height: {lineHeightStr};
    margin: 5%;
}}
h1 {{
    font-size: 1.4em;
    font-weight: bold;
    text-align: center;
    margin: 2em 0 1em 0;
    page-break-before: always;
}}
p {{
    text-indent: 1.5em;
    margin-top: 0;
    margin-bottom: 0.5em;
}}
.cover-image {{
    max-width: 100%;
    max-height: 100vh;
    height: auto;
    display: block;
    margin: 0 auto;
}}");

        // 7. Chapter XHTML files
        for (int i = 0; i < chapters.Count; i++)
        {
            string fileName = $"OEBPS/chapter{i + 1}.xhtml";
            WriteEntry(archive, fileName, BuildChapterXhtml(chapters[i].Title, chapters[i].Content, title));
        }
    }

    // ──────────────────────────────────────────────────────────────────
    // Chapter splitting

    private record ChapterContent(string Title, string Content);

    private List<ChapterContent> BuildChapters(EpubExportOptions options)
    {
        string fullText = options.ContentText ?? string.Empty;
        var chapterInfos = options.Chapters;

        // No chapter info: single chapter
        if (chapterInfos == null || chapterInfos.Count == 0)
        {
            return new List<ChapterContent>
            {
                new ChapterContent(options.Title ?? "Book", fullText)
            };
        }

        // Split text at chapter offsets
        var result = new List<ChapterContent>();
        for (int i = 0; i < chapterInfos.Count; i++)
        {
            int start = chapterInfos[i].CharOffset;
            int end = (i + 1 < chapterInfos.Count) ? chapterInfos[i + 1].CharOffset : fullText.Length;
            string content = start < fullText.Length
                ? fullText.Substring(start, Math.Max(0, end - start))
                : string.Empty;
            result.Add(new ChapterContent(chapterInfos[i].Title, content));
        }

        // If there's content before the first chapter, prepend as a preface
        if (chapterInfos.Count > 0 && chapterInfos[0].CharOffset > 0)
        {
            string preface = fullText.Substring(0, chapterInfos[0].CharOffset).Trim();
            if (!string.IsNullOrWhiteSpace(preface))
                result.Insert(0, new ChapterContent("Preface", preface));
        }

        return result;
    }

    // ──────────────────────────────────────────────────────────────────
    // Content builders

    private static string BuildCoverXhtml(string title, string coverFileName)
    {
        return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.1//EN"" ""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
  <title>{title} - Cover</title>
  <link rel=""stylesheet"" type=""text/css"" href=""style.css""/>
  <style type=""text/css"">
    body {{ margin: 0; padding: 0; text-align: center; }}
    div {{ height: 100vh; display: flex; align-items: center; justify-content: center; }}
  </style>
</head>
<body>
  <div>
    <img src=""{coverFileName}"" alt=""Cover Image"" class=""cover-image"" />
  </div>
</body>
</html>";
    }

    private static string BuildOpf(string bookId, string title, string author, string language,
        List<ChapterContent> chapters, bool hasCover, string coverFileName, string coverMime)
    {
        var manifest = new StringBuilder();
        var spine = new StringBuilder();

        if (hasCover)
        {
            manifest.AppendLine($"    <item id=\"cover-image\" href=\"{coverFileName}\" media-type=\"{coverMime}\"/>");
            manifest.AppendLine("    <item id=\"cover\" href=\"cover.xhtml\" media-type=\"application/xhtml+xml\"/>");
            spine.AppendLine("    <itemref idref=\"cover\"/>");
        }

        for (int i = 0; i < chapters.Count; i++)
        {
            string id = $"chapter{i + 1}";
            manifest.AppendLine($"    <item id=\"{id}\" href=\"{id}.xhtml\" media-type=\"application/xhtml+xml\"/>");
            spine.AppendLine($"    <itemref idref=\"{id}\"/>");
        }

        string metaCover = hasCover ? "    <meta name=\"cover\" content=\"cover-image\"/>" : "";

        return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<package xmlns=""http://www.idpf.org/2007/opf"" unique-identifier=""BookId"" version=""2.0"">
  <metadata xmlns:dc=""http://purl.org/dc/elements/1.1/"" xmlns:opf=""http://www.idpf.org/2007/opf"">
    <dc:title>{title}</dc:title>
    <dc:creator opf:role=""aut"">{author}</dc:creator>
    <dc:language>{language}</dc:language>
    <dc:identifier id=""BookId"">{bookId}</dc:identifier>
{metaCover}  </metadata>
  <manifest>
    <item id=""ncx"" href=""toc.ncx"" media-type=""application/x-dtbncx+xml""/>
    <item id=""style"" href=""style.css"" media-type=""text/css""/>
{manifest}  </manifest>
  <spine toc=""ncx"">
{spine}  </spine>
</package>";
    }

    private static string BuildNcx(string bookId, string title, List<ChapterContent> chapters, bool hasCover)
    {
        var navPoints = new StringBuilder();
        int playOrder = 1;

        if (hasCover)
        {
            navPoints.AppendLine($@"    <navPoint id=""navPoint-{playOrder}"" playOrder=""{playOrder}"">
      <navLabel><text>Cover</text></navLabel>
      <content src=""cover.xhtml""/>
    </navPoint>");
            playOrder++;
        }

        for (int i = 0; i < chapters.Count; i++)
        {
            string chapterTitle = HttpUtility.HtmlEncode(chapters[i].Title);
            navPoints.AppendLine($@"    <navPoint id=""navPoint-{playOrder}"" playOrder=""{playOrder}"">
      <navLabel><text>{chapterTitle}</text></navLabel>
      <content src=""chapter{i + 1}.xhtml""/>
    </navPoint>");
            playOrder++;
        }

        return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<ncx xmlns=""http://www.daisy.org/z3986/2005/ncx/"" version=""2005-1"">
  <head>
    <meta name=""dtb:uid"" content=""{bookId}""/>
    <meta name=""dtb:depth"" content=""1""/>
    <meta name=""dtb:totalPageCount"" content=""0""/>
    <meta name=""dtb:maxPageNumber"" content=""0""/>
  </head>
  <docTitle><text>{title}</text></docTitle>
  <navMap>
{navPoints}  </navMap>
</ncx>";
    }

    private static string BuildChapterXhtml(string chapterTitle, string content, string bookTitle)
    {
        var body = new StringBuilder();
        body.AppendLine($"  <h1>{HttpUtility.HtmlEncode(chapterTitle)}</h1>");

        string[] paragraphs = content.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string p in paragraphs)
        {
            string trimmed = p.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                // Skip the chapter heading line itself if it matches
                if (trimmed.Equals(chapterTitle, StringComparison.OrdinalIgnoreCase))
                    continue;
                string encoded = HttpUtility.HtmlEncode(trimmed).Replace("\n", "<br/>");
                body.AppendLine($"  <p>{encoded}</p>");
            }
        }

        return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.1//EN"" ""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
  <title>{HttpUtility.HtmlEncode(bookTitle)}</title>
  <link rel=""stylesheet"" type=""text/css"" href=""style.css""/>
</head>
<body>
{body}</body>
</html>";
    }

    // ──────────────────────────────────────────────────────────────────
    // Helpers

    private static void WriteEntry(ZipArchive archive, string entryName, string content,
        CompressionLevel level = CompressionLevel.Optimal)
    {
        var entry = archive.CreateEntry(entryName, level);
        using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
        writer.Write(content);
    }

    private static void WriteBinaryEntry(ZipArchive archive, string entryName, byte[] data,
        CompressionLevel level = CompressionLevel.Optimal)
    {
        var entry = archive.CreateEntry(entryName, level);
        using var stream = entry.Open();
        stream.Write(data, 0, data.Length);
    }
}
