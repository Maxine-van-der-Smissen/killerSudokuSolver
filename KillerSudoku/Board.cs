namespace KillerSudoku; 

public class Board {
    public readonly string BoardDomain;
    private readonly int _boardSize;
    public string[,] Cells { get; }

    private bool IsComplete => !ToString().Contains('.');

    public Board(int boardSize, string start) {
        if (boardSize is < 4 or > 25 || !Utils.IsPerfectSquare(boardSize)) {
            throw new ArgumentOutOfRangeException(nameof(boardSize), boardSize, "The size of a sudoku board must be between 4 and 25 (both inclusive).");
        }

        _boardSize = boardSize;
        Cells = new string[_boardSize,_boardSize];
        BoardDomain = Utils.GetDomainForSize(_boardSize);
        for (var i = 0; i < start.Length; i++) {
            var c = start[i];
            var (col, row) = Utils.IndexToPos(i, _boardSize);
            var cell = c == '.' ? BoardDomain : c.ToString();

            Cells[col, row] = cell;
        }

        var boardChanged = true;
        while (boardChanged && !IsComplete){
            boardChanged = DoForwardChecking();
        }
    }

    private bool DoForwardChecking() {
        var boardChanged = false;
        for (var row = 0; row < _boardSize; row++){
            for (var col = 0; col < _boardSize; col++){
                if (ConstrainCell(col, row)){
                    boardChanged = true;
                }
            }
        }
        return boardChanged;
    }

    public override string ToString() {
        return Cells.ToBoardString();
    }

    private bool ConstrainCell(int col, int row) {
        var currentDomain = Cells[col, row];
        if (currentDomain.Length == 1) return false;

        var colRestrictions = Cells.GetColumn(col).GetRestrictedDomain(BoardDomain);
        var rowRestrictions = Cells.GetRow(row).GetRestrictedDomain(BoardDomain);
        var blockRestrictions = Cells.GetBlock((col, row)).GetRestrictedDomain(BoardDomain);

        var restrictions = colRestrictions.Union(rowRestrictions).Union(blockRestrictions);
        var newDomain = string.Join(string.Empty, currentDomain.Except(restrictions));

        if (currentDomain == newDomain){
            return false;
        }

        Cells[col, row] = newDomain;
        return true;
    }
}
