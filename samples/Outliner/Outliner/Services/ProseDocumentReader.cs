using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using UglyToad.PdfPig;

namespace Outliner.Services
{
    public class ProseDocumentReader
    {
        public Task<string> ReadAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty", nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Prose file not found", filePath);

            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            return ext switch
            {
                ".txt" => Task.FromResult(File.ReadAllText(filePath)),
                ".docx" => Task.FromResult(ReadDocx(filePath)),
                ".pdf" => Task.FromResult(ReadPdf(filePath)),
                _ => throw new NotSupportedException($"Unsupported prose format: {ext}")
            };
        }

        private static string ReadDocx(string path)
        {
            using var doc = WordprocessingDocument.Open(path, false);
            return doc.MainDocumentPart?.Document.Body?.InnerText ?? string.Empty;
        }

        private static string ReadPdf(string path)
        {
            var sb = new StringBuilder();
            using var pdf = PdfDocument.Open(path);
            foreach (var page in pdf.GetPages())
                sb.AppendLine(page.Text);
            return sb.ToString();
        }
    }
}
