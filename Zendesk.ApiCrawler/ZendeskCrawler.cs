using FOM.RAG.CrawlerOrchestrator.Abstractions.Contracts;
using FOM.RAG.CrawlerOrchestrator.Abstractions.Dtos;
using Zendesk.ApiClient.Abstractions;
using Zendesk.ApiClient.Endpoints.HelpCenter.Sections.GetList;

namespace Zendesk.ApiCrawler;

public class ZendeskCrawler(IZendeskClient zendeskClient) : ICrawler
{
    public async Task<IEnumerable<IDocument>> CrawlAsync()
    {
        var allArticles = await zendeskClient.HelpCenter.Articles.GetListAsync();
        var allCategories = await zendeskClient.HelpCenter.Categories.GetListAsync();
        var allSections = await zendeskClient.HelpCenter.Sections.GetListAsync();

        var documents = new List<IDocument>();

        foreach (var article in allArticles)
        {
            var categories = new List<string>();

            // Sections     
            var sectionHierarchy = new List<SectionAsListItemDto>();
            long sectionId = article.SectionId;
            do
            {
                var section = allSections.First(s => s.Id == sectionId);

                if (section is not SectionAsListItemDto)
                {
                    break;
                }

                sectionHierarchy.Add(section);

                if (section.ParentSectionId is long)
                {
                    sectionId = section.ParentSectionId.Value;
                }
                else
                {
                    break;
                }

            } while (true);
            sectionHierarchy.Reverse();

            var firstSection = sectionHierarchy.FirstOrDefault();
            if (firstSection is SectionAsListItemDto)
            {
                var categoryId = firstSection.CategoryId;
                var category = allCategories.FirstOrDefault(x => x.Id == categoryId);

                categories.Add(category?.Name ?? string.Empty);
            }

            foreach (var section in sectionHierarchy)
            {
                categories.Add(section.Name ?? string.Empty);
            }

            var document = new Document
            {
                DocumentId = Guid.NewGuid(),
                Origin = "ZENDESK",
                IdInOrigin = article.Id.ToString(),
                Location = article.HtmlUrl ?? article.Url,
                Title = article.Title ?? string.Empty,
                Categories = categories.ToArray(),
                HtmlBody = article.Body ?? string.Empty
            };
            documents.Add(document);
        }

        return documents;
    }

}
