using Microsoft.AspNetCore.SignalR;
using puzzlerealtimeapp.model;
using System.Collections.Concurrent;
public class PuzzleHub : Hub
{
    private static PuzzleState puzzleState = new PuzzleState();
    private static readonly int _gridSize = 10;

    private static ConcurrentDictionary<string, User> _connectedUsers = new ConcurrentDictionary<string, User>();

    static PuzzleHub()
    {
        InitializePuzzlePieces();
    }

    private static void InitializePuzzlePieces()
    {
        puzzleState.Pieces = Enumerable.Range(0, _gridSize)
            .SelectMany(oy => Enumerable.Range(0, _gridSize)
                .Select(ox => new PuzzlePieceModel
                {
                    Id = oy * _gridSize + ox,
                    CurrentX = ox,
                    CurrentY = oy,
                    CorrectX = ox,
                    CorrectY = oy,
                    IsPlacedCorrectly = true
                })
            ).ToList();
        ShufflePuzzleState();
    }

    private static void ShufflePuzzleState()
    {
        var random = new Random();
        var currentPositions = puzzleState.Pieces.Select(p => new { p.CurrentX, p.CurrentY }).ToList();
        currentPositions = currentPositions.OrderBy(a => random.Next()).ToList();

        for (int i = 0; i < puzzleState.Pieces.Count; i++)
        {
            puzzleState.Pieces[i].CurrentX = currentPositions[i].CurrentX;
            puzzleState.Pieces[i].CurrentY = currentPositions[i].CurrentY;
            puzzleState.Pieces[i].IsPlacedCorrectly =
                (puzzleState.Pieces[i].CurrentX == puzzleState.Pieces[i].CorrectX &&
                 puzzleState.Pieces[i].CurrentY == puzzleState.Pieces[i].CorrectY);
        }
    }


    public async Task SetUsername(string username)
    {
        var connectionId = Context.ConnectionId;
        var newUser = new User { ConnectionId = connectionId, Username = username };

        if (_connectedUsers.TryAdd(connectionId, newUser))
        {
            Console.WriteLine($"Utente connesso: {username} ({connectionId})");
            await Clients.Caller.SendAsync("ReceivePuzzleState", puzzleState.Pieces);
            await Clients.All.SendAsync("ReceiveUserList", _connectedUsers.Values.ToList());
            Console.WriteLine($"Lista utenti aggiornata diffusa a tutti.");
        }
        else
        {
            Console.WriteLine($"Errore: ID di connessione {connectionId} già presente nel dizionario utenti.");
        }
    }


    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
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
        await base.OnDisconnectedAsync(exception);
    }

    public async Task MovePiece(int draggedPieceId, int targetX, int targetY)
    {
        Console.WriteLine($"MovePiece ricevuto: dragged ID {draggedPieceId} a ({targetX}, {targetY})");

        var draggedPiece = puzzleState.Pieces.FirstOrDefault(p => p.Id == draggedPieceId);
        if (draggedPiece == null) {  return; }

        var targetPiece = puzzleState.Pieces.FirstOrDefault(p => p.CurrentX == targetX && p.CurrentY == targetY);
        if (targetPiece == null) {  return; }

        int tempX = draggedPiece.CurrentX;
        int tempY = draggedPiece.CurrentY;

        draggedPiece.CurrentX = targetPiece.CurrentX;
        draggedPiece.CurrentY = targetPiece.CurrentY;
        targetPiece.CurrentX = tempX;
        targetPiece.CurrentY = tempY;

        draggedPiece.IsPlacedCorrectly =
            (draggedPiece.CurrentX == draggedPiece.CorrectX && draggedPiece.CurrentY == draggedPiece.CorrectY);
        targetPiece.IsPlacedCorrectly =
            (targetPiece.CurrentX == targetPiece.CorrectX && targetPiece.CurrentY == targetPiece.CorrectY);

        Console.WriteLine($"Pezzi scambiati: ID {draggedPiece.Id} e ID {targetPiece.Id}");

        bool allPiecesCorrect = puzzleState.Pieces.All(p => p.IsPlacedCorrectly);
        if (allPiecesCorrect) { Console.WriteLine("PUZZLE RISOLTO!"); }

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