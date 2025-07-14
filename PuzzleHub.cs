using Microsoft.AspNetCore.SignalR;
using puzzlerealtimeapp.model;

public class PuzzleHub : Hub
{


    private static PuzzleState puzzleState = new PuzzleState();

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("InitPuzzle", puzzleState);
        await base.OnConnectedAsync();
    }

    public async Task MovePiece(int pieceId, int newX, int newY)
    {

        var piece = puzzleState.Pieces.FirstOrDefault(p => p.Id == pieceId);

        if (piece != null)
        {
            piece.X = newX;
            piece.Y = newY;
        }
        else
        {
            piece = new PuzzlePieceModel {Id = pieceId+1,X =  newX, Y = newY };
            puzzleState.Pieces.Add(piece);
        }
        // Invia il movimento a tutti tranne al chiamante
        await Clients.All.SendAsync("ReceiveMove", pieceId, newX, newY);
    }
}
