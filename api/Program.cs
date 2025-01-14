using api.Cache;
using API;
var builder = WebApplication.CreateBuilder(args);

var apiPolicy = "cache";

builder.Services.AddInfrastructureService();
builder.Services.AddWebAPIService();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(apiPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:3000")
       .AllowAnyMethod()
       .AllowAnyHeader()
       .AllowCredentials();
    });
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(apiPolicy);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();