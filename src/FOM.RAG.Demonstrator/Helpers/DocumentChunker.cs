using FOM.RAG.Demonstrator.Models;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace FOM.RAG.Demonstrator.Helpers;

public class DocumentChunker
{
    public static List<SectionChunk> ExtractSectionChunksByHeader1orLength(string documentTitle, string htmlContent, int maxPlainTextChars = 20000)
    {
        var chunks = new List<SectionChunk>();

        // Load the HTML content
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);

        // Find all H1 tags
        var h1Nodes = htmlDoc.DocumentNode.SelectNodes("//h1");

        // If no H1 tags found, create a single chunk
        if (h1Nodes == null || h1Nodes.Count == 0)
        {
            var plainText = HtmlToPlainText(htmlContent);

            // If the content is too large, split it
            if (plainText.Length > maxPlainTextChars)
            {
                return SplitByLength(documentTitle, plainText, maxPlainTextChars);
            }

            chunks.Add(new SectionChunk
            {
                DocumentTitle = documentTitle,
                ParentSectionTitles = [],
                SectionTitle = documentTitle, // Use document title as section title when no H1
                HierarchyNumber = "0",
                PlainTextContent = plainText
            });

            return chunks;
        }

        // Prepare to process chunks between H1 headers
        var bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//body");
        if (bodyNode == null)
        {
            bodyNode = htmlDoc.DocumentNode;
        }

        // Clone the body to work with
        var clonedBody = bodyNode.CloneNode(true);

        // Process each H1 section
        for (int i = 0; i < h1Nodes.Count; i++)
        {
            // Get the current H1 node from the original document
            var h1Node = h1Nodes[i];
            string sectionTitle = h1Node.InnerText.Trim();

            // Find this H1 in the cloned body
            var h1InClonedBody = clonedBody.SelectSingleNode($"//h1[{i + 1}]");
            if (h1InClonedBody == null) continue;

            // Get all nodes between this H1 and the next H1 (or end)
            var sectionHtml = new HtmlDocument();
            sectionHtml.LoadHtml("");
            var sectionRoot = sectionHtml.DocumentNode;

            // Add the H1 itself
            sectionRoot.AppendChild(h1InClonedBody.CloneNode(true));

            // Get all following nodes until next H1
            var currentNode = h1InClonedBody.NextSibling;
            while (currentNode != null)
            {
                // Stop if we hit another H1
                if (currentNode.Name == "h1")
                    break;

                // Clone and add the node
                sectionRoot.AppendChild(currentNode.CloneNode(true));
                currentNode = currentNode.NextSibling;
            }

            // Convert the section to plain text
            var plainText = HtmlToPlainText(sectionRoot.OuterHtml);

            // If the content is too large, split it
            if (plainText.Length > maxPlainTextChars)
            {
                var subChunks = SplitByLength(documentTitle, plainText, maxPlainTextChars, sectionTitle, i.ToString());
                chunks.AddRange(subChunks);
            }
            else
            {
                chunks.Add(new SectionChunk
                {
                    DocumentTitle = documentTitle,
                    ParentSectionTitles = [],
                    SectionTitle = sectionTitle,
                    HierarchyNumber = i.ToString(),
                    PlainTextContent = plainText
                });
            }
        }

        return chunks;
    }

    private static string HtmlToPlainText(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Remove script and style elements
        var nodeToRemove = doc.DocumentNode.SelectNodes("//script|//style");
        if (nodeToRemove != null)
        {
            foreach (var node in nodeToRemove)
            {
                node.Remove();
            }
        }

        // Start with an empty string builder
        var markdownBuilder = new System.Text.StringBuilder();
        ConvertNodeToMarkdown(doc.DocumentNode, markdownBuilder, 0);

        // Get the resulting markdown text
        string markdownText = markdownBuilder.ToString();

        // Normalize line breaks and remove excessive whitespace
        markdownText = Regex.Replace(markdownText, @"\r\n", "\n");
        markdownText = Regex.Replace(markdownText, @"\n{3,}", "\n\n");
        markdownText = markdownText.Trim();

        return markdownText;
    }

    private static void ConvertNodeToMarkdown(HtmlNode node, System.Text.StringBuilder markdownBuilder, int listLevel)
    {
        if (node == null) return;

        switch (node.NodeType)
        {
            case HtmlNodeType.Text:
                if (node.ParentNode.Name != "script" && node.ParentNode.Name != "style")
                {
                    string text = node.InnerText.Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        // Clean up whitespace in text nodes but preserve important spaces
                        text = Regex.Replace(text, @"\s+", " ");
                        markdownBuilder.Append(text);

                        // Add space if this isn't the end of a block element and next node is text
                        if (node.NextSibling != null && node.NextSibling.NodeType == HtmlNodeType.Text &&
                           !IsBlockElement(node.ParentNode.Name))
                        {
                            markdownBuilder.Append(" ");
                        }
                    }
                }
                break;

            case HtmlNodeType.Element:
                switch (node.Name.ToLower())
                {
                    // Headers
                    case "h1":
                    case "h2":
                    case "h3":
                    case "h4":
                    case "h5":
                    case "h6":
                        int headerLevel = int.Parse(node.Name.Substring(1));
                        markdownBuilder.Append("\n\n");
                        markdownBuilder.Append(new string('#', headerLevel));
                        markdownBuilder.Append(" ");
                        ProcessChildNodes(node, markdownBuilder, listLevel);
                        markdownBuilder.Append("\n\n");
                        return;

                    // Paragraphs and divs
                    case "p":
                    case "div":
                        markdownBuilder.Append("\n\n");
                        ProcessChildNodes(node, markdownBuilder, listLevel);
                        markdownBuilder.Append("\n\n");
                        return;

                    // Line breaks
                    case "br":
                        markdownBuilder.Append("\n");
                        break;

                    // Horizontal rule
                    case "hr":
                        markdownBuilder.Append("\n\n---\n\n");
                        break;

                    // Text formatting
                    case "strong":
                    case "b":
                        markdownBuilder.Append("**");
                        ProcessChildNodes(node, markdownBuilder, listLevel);
                        markdownBuilder.Append("**");
                        return;

                    case "em":
                    case "i":
                        markdownBuilder.Append("*");
                        ProcessChildNodes(node, markdownBuilder, listLevel);
                        markdownBuilder.Append("*");
                        return;

                    case "code":
                        markdownBuilder.Append("`");
                        ProcessChildNodes(node, markdownBuilder, listLevel);
                        markdownBuilder.Append("`");
                        return;

                    case "pre":
                        markdownBuilder.Append("\n\n```\n");
                        ProcessChildNodes(node, markdownBuilder, listLevel);
                        markdownBuilder.Append("\n```\n\n");
                        return;

                    // Lists
                    case "ul":
                        markdownBuilder.Append("\n\n");
                        ProcessChildNodes(node, markdownBuilder, listLevel + 1);
                        markdownBuilder.Append("\n\n");
                        return;

                    case "ol":
                        markdownBuilder.Append("\n\n");
                        int itemNumber = 1;
                        foreach (var child in node.ChildNodes)
                        {
                            if (child.Name.ToLower() == "li")
                            {
                                markdownBuilder.Append(new string(' ', listLevel * 2));
                                markdownBuilder.Append($"{itemNumber}. ");
                                ProcessChildNodes(child, markdownBuilder, listLevel);
                                markdownBuilder.Append("\n");
                                itemNumber++;
                            }
                            else
                            {
                                ConvertNodeToMarkdown(child, markdownBuilder, listLevel);
                            }
                        }
                        return;

                    case "li":
                        if (node.ParentNode.Name.ToLower() != "ol")
                        {
                            markdownBuilder.Append(new string(' ', listLevel * 2));
                            markdownBuilder.Append("* ");
                            ProcessChildNodes(node, markdownBuilder, listLevel);
                            markdownBuilder.Append("\n");
                        }
                        return;

                    // Links
                    case "a":
                        string href = node.GetAttributeValue("href", "");
                        markdownBuilder.Append("[");
                        ProcessChildNodes(node, markdownBuilder, listLevel);
                        markdownBuilder.Append("](");
                        markdownBuilder.Append(href);
                        markdownBuilder.Append(")");
                        return;

                    // Images
                    case "img":
                        string src = node.GetAttributeValue("src", "");
                        string alt = node.GetAttributeValue("alt", "");
                        markdownBuilder.Append("![");
                        markdownBuilder.Append(alt);
                        markdownBuilder.Append("](");
                        markdownBuilder.Append(src);
                        markdownBuilder.Append(")");
                        return;

                    // Tables
                    case "table":
                        markdownBuilder.Append("\n\n");
                        ProcessTable(node, markdownBuilder);
                        markdownBuilder.Append("\n\n");
                        return;

                    // Default case for other elements
                    default:
                        ProcessChildNodes(node, markdownBuilder, listLevel);
                        return;
                }
                break;
        }

        // Process child nodes for any other node types
        ProcessChildNodes(node, markdownBuilder, listLevel);
    }

    private static void ProcessChildNodes(HtmlNode node, System.Text.StringBuilder markdownBuilder, int listLevel)
    {
        foreach (var childNode in node.ChildNodes)
        {
            ConvertNodeToMarkdown(childNode, markdownBuilder, listLevel);
        }
    }

    private static void ProcessTable(HtmlNode tableNode, System.Text.StringBuilder markdownBuilder)
    {
        // Find table header row
        var thead = tableNode.SelectSingleNode(".//thead");
        var headerRow = thead != null
            ? thead.SelectSingleNode(".//tr")
            : tableNode.SelectSingleNode(".//tr");

        // If no header row found, try first row
        if (headerRow == null)
        {
            var rows = tableNode.SelectNodes(".//tr");
            if (rows != null && rows.Count > 0)
            {
                headerRow = rows[0];
            }
            else
            {
                // No rows found, exit
                return;
            }
        }

        // Process header cells
        var headerCells = headerRow.SelectNodes(".//th|.//td");
        if (headerCells == null || headerCells.Count == 0) return;

        List<string> columnAlignments = new List<string>();
        List<string> headerTexts = new List<string>();

        foreach (var cell in headerCells)
        {
            var cellBuilder = new System.Text.StringBuilder();
            ProcessChildNodes(cell, cellBuilder, 0);
            string cellText = cellBuilder.ToString().Trim();
            headerTexts.Add(cellText);

            // Default to left align
            columnAlignments.Add("---");
        }

        // Write header row
        markdownBuilder.Append("| ");
        markdownBuilder.Append(string.Join(" | ", headerTexts));
        markdownBuilder.Append(" |");
        markdownBuilder.Append("\n");

        // Write alignment row
        markdownBuilder.Append("| ");
        markdownBuilder.Append(string.Join(" | ", columnAlignments));
        markdownBuilder.Append(" |");
        markdownBuilder.Append("\n");

        // Process data rows (skip header if in thead)
        var tbody = tableNode.SelectSingleNode(".//tbody");
        var rowNodes = tbody != null
            ? tbody.SelectNodes(".//tr")
            : tableNode.SelectNodes(".//tr");

        if (rowNodes != null)
        {
            int startIndex = thead != null ? 0 : 1; // Skip header row if it's not in thead

            for (int i = startIndex; i < rowNodes.Count; i++)
            {
                var row = rowNodes[i];
                var cells = row.SelectNodes(".//td|.//th");
                if (cells == null) continue;

                List<string> cellTexts = new List<string>();

                foreach (var cell in cells)
                {
                    var cellBuilder = new System.Text.StringBuilder();
                    ProcessChildNodes(cell, cellBuilder, 0);
                    string cellText = cellBuilder.ToString().Trim();
                    cellTexts.Add(cellText);
                }

                // Ensure we have the same number of cells as headers
                while (cellTexts.Count < headerTexts.Count)
                {
                    cellTexts.Add("");
                }

                // Write data row
                markdownBuilder.Append("| ");
                markdownBuilder.Append(string.Join(" | ", cellTexts));
                markdownBuilder.Append(" |");
                markdownBuilder.Append("\n");
            }
        }
    }

    private static bool IsBlockElement(string tagName)
    {
        var blockElements = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "div", "p", "h1", "h2", "h3", "h4", "h5", "h6",
            "ul", "ol", "li", "table", "tr", "pre", "hr", "br"
        };

        return blockElements.Contains(tagName);
    }

    private static List<SectionChunk> SplitByLength(string documentTitle, string content, int maxLength, string sectionTitle = "", string baseHierarchy = "")
    {
        var chunks = new List<SectionChunk>();

        // If no section title provided, use document title
        if (string.IsNullOrEmpty(sectionTitle))
        {
            sectionTitle = documentTitle;
        }

        // Calculate how many chunks we'll need
        int chunkCount = (int)Math.Ceiling((double)content.Length / maxLength);

        for (int i = 0; i < chunkCount; i++)
        {
            int startIndex = i * maxLength;
            int length = Math.Min(maxLength, content.Length - startIndex);

            // Find a good breaking point (sentence end or space)
            if (i < chunkCount - 1 && startIndex + length < content.Length)
            {
                int adjustedLength = FindBreakingPoint(content, startIndex, length);
                length = adjustedLength > 0 ? adjustedLength : length;
            }

            string chunkContent = content.Substring(startIndex, length);

            chunks.Add(new SectionChunk
            {
                DocumentTitle = documentTitle,
                ParentSectionTitles = [],
                SectionTitle = $"{sectionTitle} (Part {i + 1}/{chunkCount})",
                HierarchyNumber = string.IsNullOrEmpty(baseHierarchy) ? i.ToString() : $"{baseHierarchy}.{i}",
                PlainTextContent = chunkContent
            });
        }

        return chunks;
    }

    private static int FindBreakingPoint(string content, int startIndex, int approximateLength)
    {
        // Try to find sentence end within the last 20% of the chunk
        int searchStart = startIndex + (int)(approximateLength * 0.8);
        int searchEnd = startIndex + approximateLength;

        // Look for period, question mark, or exclamation mark followed by space or newline
        for (int i = searchEnd; i >= searchStart; i--)
        {
            if (i < content.Length && (content[i] == '.' || content[i] == '?' || content[i] == '!'))
            {
                if (i + 1 < content.Length && (char.IsWhiteSpace(content[i + 1])))
                {
                    return i - startIndex + 1;
                }
            }
        }

        // If no sentence break found, try to find a space
        for (int i = searchEnd; i >= searchStart; i--)
        {
            if (i < content.Length && char.IsWhiteSpace(content[i]))
            {
                return i - startIndex;
            }
        }

        // If no good breaking point found, return the original length
        return -1;
    }
}
