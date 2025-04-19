
using AgentsSSO;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Hosting.AspNetCore;
using Microsoft.Agents.Samples;
using Microsoft.Agents.Storage;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddAgentAspNetAuthentication(builder.Configuration);
builder.AddAgentApplicationOptions();
builder.AddAgent<AuthAgent>();
builder.Services.AddSingleton<IStorage, MemoryStorage>();
var app = builder.Build();


app.MapPost("/api/messages", (HttpRequest request, HttpResponse response, IAgentHttpAdapter adapter, IAgent agent, CancellationToken cancellationToken) =>
    adapter.ProcessAsync(request, response, agent, cancellationToken));

app.Run();

