using Users.Api.Endpoints;
using Users.Application.DependencyInjection;  // Application layer DI
// No explicit using for Infrastructure DI needed because of the shared namespace

var builder = WebApplication.CreateBuilder(args);

// --- Add services to the container ---

// New minimal OpenAPI document generation (MS template)
builder.Services.AddOpenApi();

// Classic Swagger (for UI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register layered architecture services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// --- Configure the HTTP request pipeline ---

if (app.Environment.IsDevelopment())
{
    // New style OpenAPI JSON
    app.MapOpenApi();

    // Swagger UI + JSON
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Register Minimal API endpoints for users
app.MapUsersEndpoints();

// Template feature: Weather endpoint
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild",
    "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Landing Page with helpful links
app.MapGet("/", () =>
    Results.Content("""
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            <title>RESTful API Training 🚀</title>
            <style>
                body {
                    font-family: Arial, sans-serif;
                    margin: 40px;
                    line-height: 1.6;
                }
                h1 {
                    color: #0d6efd;
                }
                ul {
                    padding-left: 20px;
                }
                a {
                    font-size: 18px;
                    text-decoration: none;
                    color: #1a73e8;
                }
                a:hover {
                    text-decoration: underline;
                }
                footer {
                    margin-top: 30px;
                    font-size: 14px;
                    color: #666;
                }
            </style>
        </head>
        <body>
            <h1>RESTful API Training project is running 🚀</h1>
            <p>Available endpoints for testing:</p>

            <ul>
                <li><a href="/weatherforecast" target="_blank">Weather Forecast Sample</a></li>
                <li><a href="/openapi/v1.json" target="_blank">OpenAPI JSON Document</a></li>
                <li><a href="/swagger" target="_blank">Swagger UI</a></li>
            </ul>

            <footer>
                Built using .NET 10 Minimal APIs • Training Mode Enabled 🧠
            </footer>
        </body>
        </html>
        """, "text/html")
);

app.Run();

// Record type for sample endpoint
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
