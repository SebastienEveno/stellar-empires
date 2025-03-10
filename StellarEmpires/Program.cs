using StellarEmpires.Application.Commands;
using StellarEmpires.Domain.Services;
using StellarEmpires.Infrastructure;
using StellarEmpires.Infrastructure.EventStore;
using StellarEmpires.Infrastructure.PlanetStore;
using System.IO.Abstractions;
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

builder.Services.AddScoped<IFileSystem, FileSystem>();
builder.Services.AddScoped<IEventStore, FileEventStore>();
builder.Services.AddScoped<IPlanetStore, FilePlanetStore>();
builder.Services.AddScoped<IPlanetStateRetriever, PlanetStateRetriever>();
builder.Services.AddScoped<IColonizePlanetCommandHandler, ColonizePlanetCommandHandler>();
builder.Services.AddScoped<IRenamePlanetCommandHandler, RenamePlanetCommandHandler>();
//builder.Services.AddHostedService<ResourcesBackgroundService>();
//builder.Services.AddSingleton<IMinesService, MinesService>();
//builder.Services.AddSingleton<IResourcesService, ResourcesService>();

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