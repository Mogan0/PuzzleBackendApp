namespace puzzlerealtimeapp.model
{
    public class PuzzlePieceModel
    {
        public int Id { get; set; }
        public int CurrentX { get; set; } 
        public int CurrentY { get; set; } 
        public int CorrectX { get; set; }
        public int CorrectY { get; set; } 
        public bool IsPlacedCorrectly { get; set; } 
    }
}