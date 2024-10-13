using Microsoft.Extensions.Options;
using StellarEmpires.Domain.Services;
using StellarEmpires.Infrastructure;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers().AddJsonOptions(options =>
{
	var enumConverter = new JsonStringEnumConverter();
	options.JsonSerializerOptions.Converters.Add(enumConverter);
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddHostedService<ResourcesBackgroundService>();
builder.Services.AddSingleton<IMinesService, MinesService>();
builder.Services.AddSingleton<IResourcesService, ResourcesService>();

builder.Services.Configure<AzureBlobStorageConfig>(builder.Configuration.GetSection("AzureBlobStorage"));
builder.Services.AddSingleton<IEventStore>(sp =>
{
	var settings = sp.GetRequiredService<IOptions<AzureBlobStorageConfig>>().Value;
	return new AzureBlobEventStore(settings);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();