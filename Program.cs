var builder = WebApplication.CreateBuilder(args);

// --- Configurazione dei Servizi ---
// I servizi vengono aggiunti al contenitore di Dependency Injection, rendendoli disponibili in tutta l'applicazione.
builder.Services.AddControllers();

// Aggiunge i servizi SignalR, il cuore della comunicazione real-time.
// Questo abilita la gestione degli Hub e la negoziazione delle connessioni WebSocket/Long Polling.
builder.Services.AddSignalR();

// Configura le Cross-Origin Resource Sharing (CORS) policies.
builder.Services.AddCors(options =>
{
    // Definisce una policy CORS predefinita.
    options.AddDefaultPolicy(policy =>
    {
        policy
            // Specifica le origini consentite. Solo le richieste provenienti da questi URL saranno accettate
            .WithOrigins(
                "http://localhost:54557",        // Per lo sviluppo Flutter Web locale (non Dockerizzato)
                "http://localhost:5000",         // Accesso diretto al backend da localhost
                "http://127.0.0.1:5000",         // Alternativa per localhost
                "http://localhost",              // L'origine del frontend quando servito da Nginx su Docker Desktop (porta 80)
                "http://host.docker.internal"    // L'indirizzo da cui Nginx (nel container) vede l'host
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Abilita il routing, permettendo all'applicazione di mappare le richieste a endpoint specifici.
app.UseRouting();

// Applica le policy CORS definite in precedenza.
app.UseCors();

// Abilita l'autorizzazione - anche se non utilizzata - futura feature
app.UseAuthorization();

// Abilita il supporto per i WebSockets.
app.UseWebSockets();

// Mappa i controller API.
app.MapControllers();

// Mappa l'Hub SignalR al percorso specificato.
app.MapHub<PuzzleHub>("/puzzlehub");

// Definisce un endpoint HTTP GET per la root dell'applicazione.
app.MapGet("/", () => Results.Content("Backend Puzzle API attivo!", "text/html"));

// Avvia l'applicazione, facendola ascoltare su tutte le interfacce disponibili sulla porta 5000.
// Questo rende il backend accessibile sia localmente che, se configurato, da altri dispositivi sulla rete.
app.Run("http://0.0.0.0:5000");