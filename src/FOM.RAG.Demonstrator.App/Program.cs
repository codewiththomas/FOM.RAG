using FOM.RAG.Demonstrator.App.Components;
using FOM.RAG.Demonstrator.App.Configuration;
using FOM.RAG.Demonstrator.App.Contracts;
using FOM.RAG.Demonstrator.App.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<DemonstratorConfiguration>(builder.Configuration.GetSection("DemonstratorConfiguration"));
builder.Services.Configure<OpenAiConfiguration>(builder.Configuration.GetSection("OpenAi"));

builder.Services.AddSingleton<InMemoryVectorStore>();
builder.Services.AddScoped<VectorStoreInitializer>();
builder.Services.AddScoped<RagService>();
builder.Services.AddScoped<IChatService, RagChatService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<VectorStoreInitializer>();
    await initializer.InitializeAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
