using Microsoft.AspNetCore.SignalR; 
using puzzlerealtimeapp.model;  
using System.Collections.Concurrent; 


/// Rappresenta l'Hub centrale per la gestione della logica del puzzle collaborativo in tempo reale.
/// Le chiamate da e verso i client Flutter passano attraverso i metodi definiti in questa classe.
public class PuzzleHub : Hub
{
    // Stato globale del puzzle. Essendo 'static', è condiviso tra tutte le connessioni client
    private static PuzzleState puzzleState = new PuzzleState();


    private static readonly int _gridSize = 10;

    // Un dizionario thread-safe per tenere traccia degli utenti attualmente connessi.
    private static ConcurrentDictionary<string, User> _connectedUsers = new ConcurrentDictionary<string, User>();

    /// Costruttore statico per inizializzare lo stato del puzzle una sola volta all'avvio dell'applicazione.
    static PuzzleHub()
    {
        InitializePuzzlePieces();
    }

   
    /// Inizializza la griglia del puzzle con i pezzi nella loro posizione corretta.
    /// Ogni pezzo riceve un ID, le coordinate iniziali (corrette) e viene marcato come correttamente posizionato.
    private static void InitializePuzzlePieces()
    {
        puzzleState.Pieces = Enumerable.Range(0, _gridSize) 
            .SelectMany(oy => Enumerable.Range(0, _gridSize) 
                .Select(ox => new PuzzlePieceModel
                {
                    Id = oy * _gridSize + ox, // ID univoco per ogni pezzo
                    CurrentX = ox,            
                    CurrentY = oy,           
                    CorrectX = ox,           
                    CorrectY = oy,         
                    IsPlacedCorrectly = true  
                })
            ).ToList();

        // Dopo aver creato i pezzi, mescola lo stato iniziale del puzzle
        ShufflePuzzleState();
    }

    /// Mescola lo stato attuale del puzzle scambiando casualmente le posizioni dei pezzi.
    /// Vengono aggiornate le coordinate 'CurrentX' e 'CurrentY' e lo stato 'IsPlacedCorrectly'.
    private static void ShufflePuzzleState()
    {
        var random = new Random();

        var currentPositions = puzzleState.Pieces.Select(p => new { p.CurrentX, p.CurrentY }).ToList();

        currentPositions = currentPositions.OrderBy(a => random.Next()).ToList();

        // Assegna le nuove posizioni mescolate ai pezzi originali.
        for (int i = 0; i < puzzleState.Pieces.Count; i++)
        {
            puzzleState.Pieces[i].CurrentX = currentPositions[i].CurrentX;
            puzzleState.Pieces[i].CurrentY = currentPositions[i].CurrentY;
            // Aggiorna lo stato 'IsPlacedCorrectly' per ogni pezzo dopo lo shuffle.
            puzzleState.Pieces[i].IsPlacedCorrectly =
                (puzzleState.Pieces[i].CurrentX == puzzleState.Pieces[i].CorrectX &&
                 puzzleState.Pieces[i].CurrentY == puzzleState.Pieces[i].CorrectY);
        }
    }

    /// <summary>
    /// Chiamato da un client quando un utente imposta il proprio username.
    /// Registra l'utente connesso e invia lo stato iniziale del puzzle e la lista utenti aggiornata.
    /// </summary>
    /// <param name="username">L'username scelto dall'utente.</param>
    public async Task SetUsername(string username)
    {
        // Ottiene l'ID di connessione univoco assegnato da SignalR a questo client.
        var connectionId = Context.ConnectionId;
        var newUser = new User { ConnectionId = connectionId, Username = username };

        // Tenta di aggiungere il nuovo utente al dizionario di utenti connessi in modo thread-safe.
        if (_connectedUsers.TryAdd(connectionId, newUser))
        {
            Console.WriteLine($"Utente connesso: {username} ({connectionId})");

            await Clients.Caller.SendAsync("ReceivePuzzleState", puzzleState.Pieces);
            
            // Invia la lista aggiornata di tutti gli utenti connessi a tutti i client.
            await Clients.All.SendAsync("ReceiveUserList", _connectedUsers.Values.ToList());
            Console.WriteLine($"Lista utenti aggiornata diffusa a tutti.");
        }
        else
        {
            Console.WriteLine($"Errore: ID di connessione {connectionId} già presente nel dizionario utenti.");
        }
    }

    /// <summary>
    /// Override del metodo OnDisconnectedAsync chiamato quando un client si disconnette dall'Hub.
    /// Rimuove l'utente dal dizionario e aggiorna la lista utenti per tutti i client.
    /// </summary>
    /// <param name="exception">Eccezione che ha causato la disconnessione, se presente.</param>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        // Tenta di rimuovere l'utente dal dizionario in modo thread-safe.
        if (_connectedUsers.TryRemove(connectionId, out var disconnectedUser))
        {
            Console.WriteLine($"Utente disconnesso: {disconnectedUser.Username} ({connectionId})");
            await Clients.All.SendAsync("ReceiveUserList", _connectedUsers.Values.ToList());
            Console.WriteLine($"Lista utenti aggiornata diffusa a tutti dopo disconnessione.");
        }
        else
        {
            Console.WriteLine($"Errore: ID di connessione {connectionId} non trovato nel dizionario utenti alla disconnessione.");
        }
        // Chiama l'implementazione base per completare il processo di disconnessione.
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Gestisce il movimento di un pezzo del puzzle da parte di un utente.
    /// Scambia la posizione del pezzo trascinato con quello nella posizione di destinazione.
    /// </summary>
    /// <param name="draggedPieceId">ID del pezzo trascinato.</param>
    /// <param name="targetX">Coordinata X di destinazione.</param>
    /// <param name="targetY">Coordinata Y di destinazione.</param>
    public async Task MovePiece(int draggedPieceId, int targetX, int targetY)
    {
        Console.WriteLine($"MovePiece ricevuto: dragged ID {draggedPieceId} a ({targetX}, {targetY})");

        // Trova il pezzo trascinato e il pezzo di destinazione.
        var draggedPiece = puzzleState.Pieces.FirstOrDefault(p => p.Id == draggedPieceId);
        if (draggedPiece == null) { return; } // Se il pezzo non esiste, esci.

        var targetPiece = puzzleState.Pieces.FirstOrDefault(p => p.CurrentX == targetX && p.CurrentY == targetY);
        if (targetPiece == null) { return; } // Se non c'è un pezzo nella posizione target, esci.

        // Scambia le coordinate attuali dei due pezzi.
        int tempX = draggedPiece.CurrentX;
        int tempY = draggedPiece.CurrentY;

        draggedPiece.CurrentX = targetPiece.CurrentX;
        draggedPiece.CurrentY = targetPiece.CurrentY;
        targetPiece.CurrentX = tempX;
        targetPiece.CurrentY = tempY;

        // Aggiorna lo stato di correttezza per entrambi i pezzi dopo lo scambio.
        draggedPiece.IsPlacedCorrectly =
            draggedPiece.CurrentX == draggedPiece.CorrectX && draggedPiece.CurrentY == draggedPiece.CorrectY;
        targetPiece.IsPlacedCorrectly =
            (targetPiece.CurrentX == targetPiece.CorrectX && targetPiece.CurrentY == targetPiece.CorrectY);

        Console.WriteLine($"Pezzi scambiati: ID {draggedPiece.Id} e ID {targetPiece.Id}");

        // Controlla se il puzzle è stato risolto dopo il movimento.
        bool allPiecesCorrect = puzzleState.Pieces.All(p => p.IsPlacedCorrectly);
        if (allPiecesCorrect) { Console.WriteLine("PUZZLE RISOLTO!"); }

        // Invia lo stato aggiornato del puzzle a tutti i client connessi,
        // garantendo che tutti vedano lo stesso stato in tempo reale.
        await Clients.All.SendAsync("ReceivePuzzleState", puzzleState.Pieces);
        Console.WriteLine($"Stato puzzle aggiornato diffuso a tutti i client.");
    }


    public async Task ShufflePuzzle()
    {
        Console.WriteLine("Richiesta di mescolare il puzzle ricevuta.");
        ShufflePuzzleState();
        await Clients.All.SendAsync("ReceivePuzzleState", puzzleState.Pieces);
        Console.WriteLine("Puzzle mescolato e stato diffuso a tutti i client.");
    }


    public async Task ResetPuzzle()
    {
        Console.WriteLine("Richiesta di reset del puzzle ricevuta.");
        foreach (var piece in puzzleState.Pieces)
        {
            piece.CurrentX = piece.CorrectX;
            piece.CurrentY = piece.CorrectY;
            piece.IsPlacedCorrectly = true;
        }
        await Clients.All.SendAsync("ReceivePuzzleState", puzzleState.Pieces);
        Console.WriteLine("Puzzle resettato e stato diffuso a tutti i client.");
    }
}