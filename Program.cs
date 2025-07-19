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
                "http://localhost:54557",     // Il tuo Flutter Web sta qui
                "http://localhost:5000",      // Il backend stesso se si chiama da localhost
                "http://127.0.0.1:5000",      // Alternativa per localhost
                "http://10.0.2.2:5000",       // Per emulatori Android (anche se non è il tuo caso attuale)
                "http://192.168.252.1:5000"   // L'IP specifico se dovessi testare da un dispositivo fisico esterno
                                              // Rimuovi "/" alla fine, non è necessario per le origini.
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


var app = builder.Build();

app.UseRouting();
app.UseCors(); // Assicurati che UseCors sia prima di UseAuthorization/UseEndpoints
app.UseAuthorization();

app.MapControllers();
app.MapHub<PuzzleHub>("/puzzlehub");
app.MapGet("/", () => Results.Content("Backend Puzzle API attivo!", "text/html"));

app.Run("http://0.0.0.0:5000"); // Ascolta su tutte le interfacce disponibili sulla porta 5000