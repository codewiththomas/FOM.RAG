using CsvHelper;
using FOM.RAG.CrawlerOrchestrator;
using FOM.RAG.CrawlerOrchestrator.Abstractions.Contracts;
using FOM.RAG.Demonstrator.Configuration;
using FOM.RAG.Demonstrator.Helpers;
using FOM.RAG.Demonstrator.Models;
using FOM.RAG.Demonstrator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenAI.Embeddings;
using System.Globalization;

// Demonstrator for a RAG project
// Goal is to pull all Articles from Knowledge Sources (at the moment Zendesk Helpdesk Articles) and
// then create a solution to answer questions based on the articles.

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddCrawlers(context.Configuration);
        services.Configure<DemonstratorConfiguration>(context.Configuration.GetSection("DemonstratorConfiguration"));
        services.Configure<OpenAiConfiguration>(context.Configuration.GetSection("OpenAi"));
        services.AddSingleton<InMemoryVectorStore>();
    })
    .Build();

var options = host.Services.GetRequiredService<IOptions<DemonstratorConfiguration>>().Value;
var openAiConfig = host.Services.GetRequiredService<IOptions<OpenAiConfiguration>>().Value;

string baseFolder = options.BaseFolder;
string documentsFolder = Path.Combine(baseFolder, "Documents");

bool isCrawlingEnabled = options.IsCrawlingActivated;
bool isChunkingEnabled = options.IsCleaningActivated;
bool isCreateEmbeddingEnabled = options.IsEmbeddingActivated;

#region Crawl

// Get all documents from the configured crawlers and save them as HTML and CSV files.
// The files are saved in a folder structure according to the categories of the documents.
// e.g. Documents\{Category1}\{Category2}\{Category3}\{DocumentId}.csv
//      Documents\{Category1}\{Category2}\{Category3}\{DocumentId}.html
// CSV contains Metadata of the document (ID, URL, Categories, etc.), HTML contains the body of the document.

if (isCrawlingEnabled)
{
    var crawlerOrchestratorService = host.Services.GetRequiredService<CrawlerOrchestratorService>();
    var documents = await crawlerOrchestratorService.CrawlAllAsync();

    foreach (var document in documents)
    {
        string documentPath = documentsFolder;
        foreach (var category in document.Categories)
        {
            string folderName = category;
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                folderName = folderName.Replace(c, '_');
            }
            documentPath = Path.Combine(documentPath, folderName);
        }
        Directory.CreateDirectory(documentPath);

        string htmlFilePath = Path.Combine(documentPath, $"{document.DocumentId}.html");
        File.WriteAllText(htmlFilePath, document.HtmlBody);

        string csvFilePath = Path.Combine(documentPath, $"{document.DocumentId}.csv");
        using (var writer = new StreamWriter(csvFilePath))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteHeader<IDocument>();
            csv.NextRecord();
            csv.WriteRecord(document);
            csv.NextRecord();
        }
    }
}

#endregion


#region Clean & Chunk

// Clean the HTML-Files (to plain text) and split them into sections.
// The sections are saved as plain text, Tables are marked with a separator line and the content is tab-separated.
// Filename ist {originalfilename}_section_x_y_z.txt where x,y,z is the hierarchy of the section.
// If document has no sections, the whole content is saved as one section as {originalfilename}_section_0.txt

if (isChunkingEnabled)
{
    Console.WriteLine("DATA CLEANING STARTED");
    var htmlFiles = Directory.GetFiles(documentsFolder, "*.html", SearchOption.AllDirectories);
    foreach (var htmlFile in htmlFiles)
    {
        try
        {
            string htmlContent = File.ReadAllText(htmlFile);
            var chunks = HtmlChunkExtractor.ExtractSectionChunks(
                html: htmlContent, 
                splitByHeaders: false);

            string fileDirectory = Path.GetDirectoryName(htmlFile);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(htmlFile);
            foreach (var chunk in chunks)
            {
                // <OriginalName>_section_<hierarchy>.txt
                string chunkFileName = $"{fileNameWithoutExt}_section_{chunk.HierarchyNumber}.txt";
                string chunkFilePath = Path.Combine(fileDirectory, chunkFileName);
                File.WriteAllText(chunkFilePath, $"Title: {chunk.Title}{Environment.NewLine}{Environment.NewLine}{chunk.PlainTextContent}");
                Console.WriteLine($"  Created chunk file: {chunkFileName}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing file {htmlFile}: {ex.Message}");
        }
    }
}

#endregion


#region Create Embeddings

//Create embedding for each chunk text file
//Store the embedding vector as {OriginalFileName}.txt but replace _section_ with _embedding_
//e.g.: {DocumentId}_section_0.txt -> {DocumentId}_embedding_0.txt

if (isCreateEmbeddingEnabled)
{
    var chunkFiles = Directory.GetFiles(documentsFolder, "*_section_*.txt", SearchOption.AllDirectories);

    string embeddingModel = openAiConfig.EmbeddingModel;
    string openAiApiKey = openAiConfig.ApiKey;
    var openAiClient = new EmbeddingClient(embeddingModel, openAiApiKey);

    foreach (var chunkFile in chunkFiles)
    {
        string newEmbeddingFileName = chunkFile.Replace("_section_", "_embedding_");
        Console.Write("  Create embedding for " + Path.GetFileName(newEmbeddingFileName) + "...");

        if (File.Exists(newEmbeddingFileName))
        {
            Console.WriteLine("Already exits.");
            continue;
        }

        string chunkContent = File.ReadAllText(chunkFile);
        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(chunkFile);
        string originalDocumentId = fileNameWithoutExt.Split('_')[0];

        OpenAIEmbedding chunkEmbedding = openAiClient.GenerateEmbedding(chunkContent);
        ReadOnlyMemory<float> chunkEmbeddingVector = chunkEmbedding.ToFloats();

        File.WriteAllLines(newEmbeddingFileName, chunkEmbeddingVector.ToArray().Select(f => f.ToString(CultureInfo.InvariantCulture)));
        Console.WriteLine("Done.");
    }
}

#endregion


#region Create In-Memory Vector Store

// Load all embeddings from the files and store them in memory.
// The InMemoryVectorStore is used to retrieve the embedding vector for a given chunk id.

var inMemoryVectorStore = host.Services.GetRequiredService<InMemoryVectorStore>();

var vectorFiles = Directory.GetFiles(documentsFolder, "*_embedding_*.txt", SearchOption.AllDirectories);

foreach (var vectorFile in vectorFiles)
{
    string sectionFile = vectorFile.Replace("_embedding_", "_section_");

    if (!File.Exists(sectionFile))
    {
        Console.WriteLine($"Warning: Section file not found for embedding {sectionFile}");
        continue;
    }

    var documentEmbedding = new DocumentEmbedding
    {
        DocumentId = Guid.Parse(Path.GetFileName(vectorFile).Split('_')[0]),
        FilePath = sectionFile,
        Content = File.ReadAllText(sectionFile),
        Vector = new ReadOnlyMemory<float>(File.ReadAllLines(vectorFile).Select(float.Parse).ToArray())
    };
    inMemoryVectorStore.AddEmbedding(documentEmbedding);
}

#endregion

#region Demonstration / Information retrieval (ab hier in Deutsch)

Console.WriteLine();
Console.Write("Frage eingeben: ");
string question = Console.ReadLine() ?? string.Empty;

if (string.IsNullOrWhiteSpace(question))
{
    Console.WriteLine("Es wurde nicht eingegeben. Beende...");
    return;
}

var ragService = new RagService(inMemoryVectorStore, openAiConfig);
string answer = await ragService.AnswerQuestion(question);
Console.WriteLine(answer);

#endregion

