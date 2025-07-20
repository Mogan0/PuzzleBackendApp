// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
          .WithOrigins(
    "http://localhost:54557",
    "http://localhost:5000",
    "http://127.0.0.1:5000",
    "http://10.0.2.2:5000",
    "http://192.168.252.1:5000",
    "http://localhost",           
    "http://host.docker.internal" 
)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


var app = builder.Build();

app.UseRouting();
app.UseCors(); 
app.UseAuthorization();
app.UseWebSockets(); 

app.MapControllers();
app.MapHub<PuzzleHub>("/puzzlehub");
app.MapGet("/", () => Results.Content("Backend Puzzle API attivo!", "text/html"));

app.Run("http://0.0.0.0:5000");