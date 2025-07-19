using Microsoft.AspNetCore.SignalR;
using puzzlerealtimeapp.model;
using System.Linq; // Per usare LINQ (Select, FirstOrDefault, ToList)
using System.Collections.Generic; // Per List
using System; // Per Random

public class PuzzleHub : Hub
{
    // Stato statico del puzzle, condiviso tra tutte le connessioni al server
    private static PuzzleState puzzleState = new PuzzleState();
    private static readonly int _gridSize = 10; // Deve corrispondere a _gridSize in Flutter

    static PuzzleHub() // Costruttore statico per inizializzare lo stato una volta sola
    {
        InitializePuzzlePieces();
    }

    // Metodo per inizializzare i pezzi del puzzle
    private static void InitializePuzzlePieces()
    {
        puzzleState.Pieces = Enumerable.Range(0, _gridSize)
            .SelectMany(oy => Enumerable.Range(0, _gridSize)
                .Select(ox => new PuzzlePieceModel
                {
                    Id = oy * _gridSize + ox, // ID univoco per ogni pezzo (0 a 99 per 10x10)
                    CurrentX = ox, // Inizialmente, la posizione corrente è la posizione corretta
                    CurrentY = oy,
                    CorrectX = ox, // La posizione finale corretta
                    CorrectY = oy,
                    IsPlacedCorrectly = true // Inizialmente tutti sono al posto giusto
                })
            ).ToList();

        // Facoltativo: mescola i pezzi all'avvio del server per non avere un puzzle già risolto
        ShufflePuzzleState();
    }

    // Metodo helper per mescolare lo stato dei pezzi (usato anche per ShufflePuzzle)
    private static void ShufflePuzzleState()
    {
        var random = new Random();
        var currentPositions = puzzleState.Pieces.Select(p => new { p.CurrentX, p.CurrentY }).ToList();
        
        // Mescola le posizioni
        currentPositions = currentPositions.OrderBy(a => random.Next()).ToList();

        // Applica le posizioni mescolate ai pezzi
        for (int i = 0; i < puzzleState.Pieces.Count; i++)
        {
            puzzleState.Pieces[i].CurrentX = currentPositions[i].CurrentX;
            puzzleState.Pieces[i].CurrentY = currentPositions[i].CurrentY;
            puzzleState.Pieces[i].IsPlacedCorrectly = 
                (puzzleState.Pieces[i].CurrentX == puzzleState.Pieces[i].CorrectX && 
                 puzzleState.Pieces[i].CurrentY == puzzleState.Pieces[i].CorrectY);
        }
    }


    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connesso: {Context.ConnectionId}");
        // Invia lo stato completo del puzzle al client che si è appena connesso
        await Clients.Caller.SendAsync("ReceivePuzzleState", puzzleState.Pieces);
        Console.WriteLine($"Stato iniziale inviato a {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    // Metodo chiamato dal client quando un pezzo viene trascinato su un altro
    public async Task MovePiece(int draggedPieceId, int targetX, int targetY)
    {
        Console.WriteLine($"MovePiece ricevuto: dragged ID {draggedPieceId} a ({targetX}, {targetY})");

        // Trova il pezzo trascinato
        var draggedPiece = puzzleState.Pieces.FirstOrDefault(p => p.Id == draggedPieceId);
        if (draggedPiece == null)
        {
            Console.WriteLine($"Errore: Pezzo trascinato con ID {draggedPieceId} non trovato.");
            return;
        }

        // Trova il pezzo che si trova nella posizione target (dove è stato rilasciato il pezzo trascinato)
        var targetPiece = puzzleState.Pieces.FirstOrDefault(p => p.CurrentX == targetX && p.CurrentY == targetY);
        if (targetPiece == null)
        {
            Console.WriteLine($"Errore: Nessun pezzo trovato nella posizione target ({targetX}, {targetY}).");
            return;
        }

        // --- Logica di SCAMBIO ---
        // Salva le posizioni del pezzo trascinato
        int tempX = draggedPiece.CurrentX;
        int tempY = draggedPiece.CurrentY;

        // Sposta il pezzo trascinato nella posizione del pezzo target
        draggedPiece.CurrentX = targetPiece.CurrentX;
        draggedPiece.CurrentY = targetPiece.CurrentY;

        // Sposta il pezzo target nella posizione originale del pezzo trascinato
        targetPiece.CurrentX = tempX;
        targetPiece.CurrentY = tempY;

        // --- Aggiorna lo stato di IsPlacedCorrectly per entrambi i pezzi ---
        draggedPiece.IsPlacedCorrectly = 
            (draggedPiece.CurrentX == draggedPiece.CorrectX && draggedPiece.CurrentY == draggedPiece.CorrectY);
        targetPiece.IsPlacedCorrectly = 
            (targetPiece.CurrentX == targetPiece.CorrectX && targetPiece.CurrentY == targetPiece.CorrectY);
        
        Console.WriteLine($"Pezzi scambiati: ID {draggedPiece.Id} e ID {targetPiece.Id}");

        // --- Verifica condizione di vittoria ---
        bool allPiecesCorrect = puzzleState.Pieces.All(p => p.IsPlacedCorrectly);
        if (allPiecesCorrect)
        {
            Console.WriteLine("PUZZLE RISOLTO!");
            // Potresti inviare un messaggio specifico di vittoria o semplicemente aggiornare lo stato
            // che verrà riflesso nella UI del client
        }

        // --- Invia lo stato aggiornato a TUTTI i client ---
        // Questo invierà la lista completa e aggiornata dei pezzi.
        await Clients.All.SendAsync("ReceivePuzzleState", puzzleState.Pieces);
        Console.WriteLine($"Stato puzzle aggiornato diffuso a tutti i client.");
    }

    // Metodo per mescolare il puzzle (chiamato dal client "Mescola")
    public async Task ShufflePuzzle()
    {
        Console.WriteLine("Richiesta di mescolare il puzzle ricevuta.");
        ShufflePuzzleState(); // Riutilizza la logica di mescolatura
        
        // Invia lo stato aggiornato a TUTTI i client
        await Clients.All.SendAsync("ReceivePuzzleState", puzzleState.Pieces);
        Console.WriteLine("Puzzle mescolato e stato diffuso a tutti i client.");
    }

    // Metodo per resettare il puzzle (chiamato dal client "Reset")
    public async Task ResetPuzzle()
    {
        Console.WriteLine("Richiesta di reset del puzzle ricevuta.");
        // Riporta ogni pezzo alla sua posizione corretta
        foreach (var piece in puzzleState.Pieces)
        {
            piece.CurrentX = piece.CorrectX;
            piece.CurrentY = piece.CorrectY;
            piece.IsPlacedCorrectly = true; // Sono tutti al posto giusto
        }
        
        // Invia lo stato aggiornato a TUTTI i client
        await Clients.All.SendAsync("ReceivePuzzleState", puzzleState.Pieces);
        Console.WriteLine("Puzzle resettato e stato diffuso a tutti i client.");
    }
}