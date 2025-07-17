

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:54557", "http://127.0.0.1:5500")
            .AllowAnyHeader()   
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


var app = builder.Build();

app.UseRouting();
app.UseCors();
app.UseAuthorization();

app.MapControllers();
app.MapHub<PuzzleHub>("/puzzlehub");

app.Run();
