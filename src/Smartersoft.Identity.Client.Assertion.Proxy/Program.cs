using Azure.Core;
using Azure.Identity;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Reflection;
using System.Text.Json.Serialization;

var portIndex = Array.IndexOf(args, "--port");
int port = portIndex > -1 && args.Length > portIndex + 1 && int.TryParse(args[portIndex + 1], out int p) ? p : 5616;

ConsoleStyle.WriteHeader(port);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddMemoryCache();

builder.Services.AddSingleton<TokenCredential>(new DefaultAzureCredential(
    new DefaultAzureCredentialOptions
        {
            ExcludeEnvironmentCredential = true, // Don't use environment variables (even interactive is better)
            ExcludeInteractiveBrowserCredential = false,
            ExcludeManagedIdentityCredential = true, // Don't run this api in production!
        }
    ));

builder.Services.AddControllers()
    .AddJsonOptions(x =>
    {
        // serialize enums as strings in api responses (e.g. Role)
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services
    .AddValidatorsFromAssemblyContaining<Smartersoft.Identity.Client.Assertion.Proxy.Models.TokenRequest>()
    .AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(swagger =>
{
    swagger.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString() ?? "v1",
        Title = "Smartersoft KeyVault proxy",
        Description = "Sort your certificates securly in Azure and use this proxy to request a token using it.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Smartersoft B.V. repository",
            Url = new Uri("https://github.com/Smartersoft/identity-client-assertion")
        },
        License = new Microsoft.OpenApi.Models.OpenApiLicense
        {
            Name = "Open source licence GPL-3.0",
            Url = new Uri("https://github.com/Smartersoft/identity-client-assertion/blob/main/LICENSE.txt")
        },
    });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    swagger.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Environment.ContentRootPath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName)!;
builder.Environment.WebRootPath = Path.Combine(builder.Environment.ContentRootPath!, "wwwroot");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (true || app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Run($"http://localhost:{port}/");
