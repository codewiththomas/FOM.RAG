using FOM.RAG.Demonstrator.Models;
using HtmlAgilityPack;
using System.Text;

namespace FOM.RAG.Demonstrator.Helpers;

public static class HtmlChunkExtractor
{
    /// <summary>
    /// Removes all HTML tags from the given HTML string and returns the plain text content.
    /// Tables are marked with a separator line and the content is tab-separated.
    /// </summary>
    /// <param name="html"></param>
    /// <returns></returns>
    private static string CleanText(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        string plainText = doc.DocumentNode.InnerText;

        // Look for table nodes.
        var tables = doc.DocumentNode.SelectNodes("//table");
        if (tables != null)
        {
            var sb = new StringBuilder();
            sb.AppendLine(plainText);
            sb.AppendLine("---- Tabelleninhalt ----");

            foreach (var table in tables)
            {
                var rows = table.SelectNodes(".//tr");
                if (rows != null)
                {
                    foreach (var row in rows)
                    {
                        var cells = row.SelectNodes(".//th|.//td");
                        if (cells != null)
                        {
                            // Combine cell texts with a tab separator.
                            string rowText = string.Join("\t", cells.Select(c => c.InnerText.Trim()));
                            sb.AppendLine(rowText);
                        }
                    }
                }
                sb.AppendLine("------------------------");
            }
            return sb.ToString();
        }
        else
        {
            return plainText;
        }
    }


    private static List<SectionChunk> ExtractSectionChunksSplittedByHeaders(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var chunks = new List<SectionChunk>();

        var headerNodes = doc.DocumentNode.SelectNodes("//h1|//h2|//h3|//h4|//h5|//h6");
        if (headerNodes == null || headerNodes.Count == 0)
        {
            var plainText = CleanText(html);
            chunks.Add(new SectionChunk
            {
                HierarchyNumber = "0",
                Title = "Full Document",
                PlainTextContent = plainText
            });
            return chunks;
        }

        int[] counters = new int[6];

        foreach (var header in headerNodes)
        {
            int level = int.Parse(header.Name.Substring(1)); // "h2" -> 2
            counters[level - 1]++;
            for (int i = level; i < 6; i++)
            {
                counters[i] = 0;
            }

            // "1", "1_2", "2_1"
            var parts = new List<string>();
            for (int i = 0; i < level; i++)
            {
                if (counters[i] > 0)
                    parts.Add(counters[i].ToString());
            }
            string hierarchyNumber = string.Join("_", parts);

            StringBuilder sb = new StringBuilder();
            var node = header.NextSibling;
            while (node != null && !(node.NodeType == HtmlNodeType.Element &&
                  (node.Name.Equals("h1", StringComparison.OrdinalIgnoreCase) ||
                   node.Name.Equals("h2", StringComparison.OrdinalIgnoreCase) ||
                   node.Name.Equals("h3", StringComparison.OrdinalIgnoreCase) ||
                   node.Name.Equals("h4", StringComparison.OrdinalIgnoreCase) ||
                   node.Name.Equals("h5", StringComparison.OrdinalIgnoreCase) ||
                   node.Name.Equals("h6", StringComparison.OrdinalIgnoreCase))))
            {
                sb.Append(node.OuterHtml);
                node = node.NextSibling;
            }

            string chunkPlainText = CleanText(sb.ToString());

            string headerText = header.InnerText.Trim();

            chunks.Add(new SectionChunk
            {
                HierarchyNumber = hierarchyNumber,
                Title = headerText,
                PlainTextContent = chunkPlainText.Trim()
            });
        }

        return chunks;
    }


    public static List<SectionChunk> ExtractSectionChunks(string html, bool splitByHeaders = false)
    {
        return splitByHeaders
            ? ExtractSectionChunksSplittedByHeaders(html)
            : new List<SectionChunk>
            {
                new SectionChunk
                {
                    HierarchyNumber = "0",
                    Title = "Full Document",
                    PlainTextContent = CleanText(html)
                }
            };
    }
}

public static class OLDHtmlChunkExtractor
{
    /// <summary>
    /// Removes all HTML tags from the given HTML string and returns the plain text content.
    /// Tables are marked with a separator line and the content is tab-separated.
    /// </summary>
    /// <param name="html"></param>
    /// <returns></returns>
    private static string CleanText(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        string plainText = doc.DocumentNode.InnerText;

        // Look for table nodes.
        var tables = doc.DocumentNode.SelectNodes("//table");
        if (tables != null)
        {
            var sb = new StringBuilder();
            sb.AppendLine(plainText);
            sb.AppendLine("---- Tabelleninhalt ----");

            foreach (var table in tables)
            {
                var rows = table.SelectNodes(".//tr");
                if (rows != null)
                {
                    foreach (var row in rows)
                    {
                        var cells = row.SelectNodes(".//th|.//td");
                        if (cells != null)
                        {
                            // Combine cell texts with a tab separator.
                            string rowText = string.Join("\t", cells.Select(c => c.InnerText.Trim()));
                            sb.AppendLine(rowText);
                        }
                    }
                }
                sb.AppendLine("------------------------");
            }
            return sb.ToString();
        }
        else
        {
            return plainText;
        }
    }


    private static List<SectionChunk> ExtractSectionChunksSplittedByHeaders(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var chunks = new List<SectionChunk>();

        var headerNodes = doc.DocumentNode.SelectNodes("//h1|//h2|//h3|//h4|//h5|//h6");
        if (headerNodes == null || headerNodes.Count == 0)
        {
            var plainText = CleanText(html);
            chunks.Add(new SectionChunk
            {
                HierarchyNumber = "0",
                Title = "Full Document",
                PlainTextContent = plainText
            });
            return chunks;
        }

        int[] counters = new int[6];

        foreach (var header in headerNodes)
        {
            int level = int.Parse(header.Name.Substring(1)); // "h2" -> 2
            counters[level - 1]++;
            for (int i = level; i < 6; i++)
            {
                counters[i] = 0;
            }

            // "1", "1_2", "2_1"
            var parts = new List<string>();
            for (int i = 0; i < level; i++)
            {
                if (counters[i] > 0)
                    parts.Add(counters[i].ToString());
            }
            string hierarchyNumber = string.Join("_", parts);

            StringBuilder sb = new StringBuilder();
            var node = header.NextSibling;
            while (node != null && !(node.NodeType == HtmlNodeType.Element &&
                  (node.Name.Equals("h1", StringComparison.OrdinalIgnoreCase) ||
                   node.Name.Equals("h2", StringComparison.OrdinalIgnoreCase) ||
                   node.Name.Equals("h3", StringComparison.OrdinalIgnoreCase) ||
                   node.Name.Equals("h4", StringComparison.OrdinalIgnoreCase) ||
                   node.Name.Equals("h5", StringComparison.OrdinalIgnoreCase) ||
                   node.Name.Equals("h6", StringComparison.OrdinalIgnoreCase))))
            {
                sb.Append(node.OuterHtml);
                node = node.NextSibling;
            }

            string chunkPlainText = CleanText(sb.ToString());

            string headerText = header.InnerText.Trim();

            chunks.Add(new SectionChunk
            {
                HierarchyNumber = hierarchyNumber,
                Title = headerText,
                PlainTextContent = chunkPlainText.Trim()
            });
        }

        return chunks;
    }


    public static List<SectionChunk> ExtractSectionChunks(string html, bool splitByHeaders = false)
    {
        return splitByHeaders
            ? ExtractSectionChunksSplittedByHeaders(html)
            : new List<SectionChunk>
            {
                new SectionChunk
                {
                    HierarchyNumber = "0",
                    Title = "Full Document",
                    PlainTextContent = CleanText(html)
                }
            };
    }
}