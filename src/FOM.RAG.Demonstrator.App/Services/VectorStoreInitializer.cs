using FOM.RAG.Demonstrator.App.Configuration;
using FOM.RAG.Demonstrator.App.Models;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace FOM.RAG.Demonstrator.App.Services;

public class VectorStoreInitializer
{
    private readonly InMemoryVectorStore _vectorStore;
    private readonly DemonstratorConfiguration _config;
    private readonly ILogger<VectorStoreInitializer> _logger;
    private bool _isInitialized = false;


    /// <summary>
    /// Initializes a new instance of the <see cref="VectorStoreInitializer"/> class.
    /// </summary>
    /// <param name="vectorStore">The in-memory vector store to populate.</param>
    /// <param name="config">Configuration for the demonstrator application.</param>
    /// <param name="logger">Logger for capturing diagnostic information.</param>
    public VectorStoreInitializer(
        InMemoryVectorStore vectorStore,
        IOptions<DemonstratorConfiguration> config,
        ILogger<VectorStoreInitializer> logger)
    {
        _vectorStore = vectorStore;
        _config = config.Value;
        _logger = logger;
    }


    /// <summary>
    /// Indicates whether the vector store has been initialized.
    /// </summary>
    public bool IsInitialized => _isInitialized;


    /// <summary>
    /// Initializes the vector store by loading embeddings from files.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        try
        {
            _logger.LogInformation("Starting vector store initialization");

            string documentsFolder = Path.Combine(_config.BaseFolder, "Documents");
            var vectorFiles = Directory.GetFiles(documentsFolder, "*_embedding_*.txt", SearchOption.AllDirectories);

            int totalFiles = vectorFiles.Length;
            int processedFiles = 0;

            foreach (var vectorFile in vectorFiles)
            {
                string sectionFile = vectorFile.Replace("_embedding_", "_section_");

                if (!File.Exists(sectionFile))
                {
                    _logger.LogWarning("Section file not found for embedding: {SectionFile}", sectionFile);
                    continue;
                }

                try
                {
                    var documentEmbedding = new DocumentEmbedding
                    {
                        DocumentId = Guid.Parse(Path.GetFileName(vectorFile).Split('_')[0]),
                        FilePath = sectionFile,
                        Content = await File.ReadAllTextAsync(sectionFile),
                        Vector = new ReadOnlyMemory<float>(
                            (await File.ReadAllLinesAsync(vectorFile))
                            .Select(line => float.Parse(line, CultureInfo.InvariantCulture))
                            .ToArray())
                    };

                    _vectorStore.AddEmbedding(documentEmbedding);
                    processedFiles++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing embedding file: {VectorFile}", vectorFile);
                }
            }

            _logger.LogInformation("Vector store initialization completed. Processed {ProcessedFiles} of {TotalFiles} files",
                processedFiles, totalFiles);

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing vector store");
            throw;
        }
    }
}
