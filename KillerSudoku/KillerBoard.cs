namespace KillerSudoku; 

public class KillerBoard {
    public readonly string BoardDomain;
    private readonly int _boardSize;
    public readonly IReadOnlyCollection<(IReadOnlyCollection<(int col, int row)> cells, int sum)> Cages;
    public string[,] Cells { get; }

    private bool IsComplete => !ToString().Contains('.');

    public KillerBoard(int boardSize, IReadOnlyCollection<(IReadOnlyCollection<(int col, int row)> cells, int sum)> cages) {
        if (boardSize is < 4 or > 25 || !Utils.IsPerfectSquare(boardSize)) {
            throw new ArgumentOutOfRangeException(nameof(boardSize), boardSize, "The size of a sudoku board must be between 4 and 25 (both inclusive).");
        }

        _boardSize = boardSize;
        Cells = new string[_boardSize,_boardSize];
        BoardDomain = Utils.GetDomainForSize(_boardSize);
        Cages = cages;
        var cellsWithDomain = cages.SelectMany(cage => AssignMinimizedDomainToCellsInCage(cage.cells, cage.sum));
        foreach (var (col, row, domain) in cellsWithDomain){
            Cells[col, row] = domain;
        }

        var boardChanged = true;
        while (boardChanged && !IsComplete){
            boardChanged = DoForwardChecking();
        }
    }

    private IEnumerable<(int col, int row, string domain)> AssignMinimizedDomainToCellsInCage(IReadOnlyCollection<(int col, int row)> cells, int sum) {
        var minimizedDomain = MinimizeDomainForCage(cells, sum);
        return cells.Select(cell => (cell.col, cell.row, minimizedDomain));
    }

    private string MinimizeDomainForCage(IReadOnlyCollection<(int col, int row)> cells, int sum) {
        var maxIndex = CageMaxDomainIndex(cells, sum);
        var minIndex = CageMinDomainIndex(cells, sum);
        return BoardDomain[minIndex..maxIndex];
    }

    private int CageMaxDomainIndex(IReadOnlyCollection<(int col, int row)> cells, int sum) {
        var cellAmount = cells.Count;
        var maxDomainValue = BoardDomain.Length;
        var minSumExceptOne = BoardDomain[..(cellAmount - 1)].Select((_, i) => i + 1).Sum();
        int calcSum;
        do{
            calcSum = minSumExceptOne + maxDomainValue--;
        } while (calcSum > sum);
        return maxDomainValue + 1;
    }

    private int CageMinDomainIndex(IReadOnlyCollection<(int col, int row)> cells, int sum) {
        var cellAmount = cells.Count;
        var minDomainValue = 0;
        var maxSumExceptOne = BoardDomain.Select((_, i) => i + 1).Reverse().Take(cellAmount - 1).Sum();
        int calcSum;
        do{
            calcSum = maxSumExceptOne + ++minDomainValue;
        } while (calcSum < sum);
        return minDomainValue - 1;
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
        var cageRestrictions = GetCage(col, row).Select(pos => Cells[pos.col, pos.row]).GetRestrictedDomain(BoardDomain);

        var restrictions = colRestrictions.Union(rowRestrictions).Union(blockRestrictions).Union(cageRestrictions);
        var newDomain = string.Join(string.Empty, currentDomain.Except(restrictions));

        if (currentDomain == newDomain){
            return false;
        }

        Cells[col, row] = newDomain;
        return true;
    }

    private IEnumerable<(int col, int row)> GetCage(int col, int row) {
        return Cages.FirstOrDefault(cage => cage.cells.Contains((col, row))).cells.Where(cell => cell != (col, row));
    }
}
