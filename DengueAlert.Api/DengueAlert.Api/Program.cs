using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DengueAlert.Api.Data;
using DengueAlert.Api.Repository;
using DengueAlert.Api.Services;
using Microsoft.EntityFrameworkCore;
using System;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(conn, ServerVersion.AutoDetect(conn));
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});


builder.Services.AddControllers();

builder.Services.AddScoped<IDengueRepository, DengueRepository>();
builder.Services.AddHostedService<FetchAndPersistHostedService>();

builder.Services.AddHttpClient<AlertDengueService>(client =>
{
    client.BaseAddress = new Uri("https://info.dengue.mat.br/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
