using Microsoft.AspNetCore.SignalR;
using puzzlerealtimeapp.model;

public class PuzzleHub : Hub
{


private static PuzzleState puzzleState = new PuzzleState
{
    Pieces = new List<PuzzlePieceModel>
    {
        new PuzzlePieceModel { Id = 1, X = 2, Y = 3 },
        new PuzzlePieceModel { Id = 2, X = 5, Y = 6 }
    }
};

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("InitPuzzle", puzzleState.Pieces);
        await base.OnConnectedAsync();
    }

    public async Task MovePiece(int pieceId, int newX, int newY)
    {
        Console.WriteLine($"MovePiece ricevuto: pezzo {pieceId} -> ({newX}, {newY})");


        var piece = puzzleState.Pieces.FirstOrDefault(p => p.Id == pieceId);

        if (piece != null)
        {
            piece.X = newX;
            piece.Y = newY;
        }
        else
        {
            piece = new PuzzlePieceModel { Id = pieceId, X = newX, Y = newY };
            puzzleState.Pieces.Add(piece);
        }
        // Invia il movimento a tutti tranne al chiamante
        await Clients.All.SendAsync("ReceiveMove", pieceId, newX, newY);
        Console.WriteLine($"ReceiveMove INVIATO: pezzo {pieceId} -> ({newX},{newY})");

    }
}
