using System.Collections.Immutable;

namespace KillerSudoku; 

public class Solver {
    private readonly string[,] _board;
    private readonly string _boardDomain;
    private readonly IReadOnlyCollection<(IReadOnlyCollection<(int col, int row)> cells, int sum)> _cages;
    private bool _doCageDomainRecalculation;
    private bool _useGuessCellPickHeuristic;
    private LinkedList<(int col, int row, string[,] board)> _guesses = null!;

    public Solver(Board board) {
        _board = board.Cells;
        _boardDomain = board.BoardDomain;
        _cages = ImmutableArray<(IReadOnlyCollection<(int col, int row)> cells, int sum)>.Empty;
    }

    public Solver(KillerBoard board) {
        _board = board.Cells;
        _boardDomain = board.BoardDomain;
        _cages = board.Cages;
    }

    public string[,]? Solve(SolverOptions? options = null) {
        if (options != null){
            SetOptions(options);
        }
        _guesses = new LinkedList<(int col, int row, string[,] board)>();
        return Solve(_board.CloneBoard());
    }

    private void SetOptions(SolverOptions options) {
        _doCageDomainRecalculation = options.DoCageDomainRecalculations;
        _useGuessCellPickHeuristic = options.UseGuessCellPickHeuristic;
    }
    
    private static bool IsComplete(string[,] board) {
        return !board.ToBoardString().Contains('.');
    }

    private bool IsValid(string[,] board) {
        var hasEmpty = board.Cast<string>().Any(string.IsNullOrEmpty);
        var cagesValid = _cages.All(cage => IsCageValid(cage.cells, cage.sum, board));
        return !hasEmpty && cagesValid;
    }

    private bool IsInvalid(string[,]? board) {
        return board == null || !IsValid(board);
    }

    private static bool IsCageValid(IReadOnlyCollection<(int col, int row)> cells, int sum, string[,] board) {
        var setCellDomains = cells.Select(pos => board[pos.col, pos.row]).Where(dom => dom.Length == 1).ToList();
        var calculatedSum = setCellDomains.Select(dom => Utils.PotentialDomain.IndexOf(dom[0]) + 1).Sum();
        if (setCellDomains.Count == cells.Count) return calculatedSum == sum;
        return calculatedSum < sum;
    }

    private string[,]? Solve(string[,]? board) {
        if (board == null || IsInvalid(board)) return null;
        if (IsComplete(board)) return board;

        var (col, row, domain) = GetMostConstrainedVariable(board);
        if (domain.Length <= 1) throw new ArgumentException("Cell at given position is not a variable and thus can't be guessed!");
        string[,]? solvedBoard = null;

        while (domain.Any()){
            _guesses.AddLast((col, row, board.CloneBoard()));
            var guess = domain[0].ToString();
            board[col, row] = guess;

            var neighbours = GetCellVariableNeighbours(col, row, board).Select(cellData => ((cellData.col, cellData.row), cellData.domSize));
            PropagateToCells(neighbours, board);
            if (_doCageDomainRecalculation){
                RecalculateDomainsForCages(board);
            }

            // If the board is valid, then continue to solve the next variable
            if (IsValid(board)){
                solvedBoard = Solve(board);
                if (solvedBoard != null && IsComplete(solvedBoard)){
                    break;
                }
            }
            
            // Wrong guess, it was not the first of the available domain.
            // Reset board to state before this guess
            board = _guesses.Last?.Value.board ?? throw new InvalidOperationException("Tried to undo more guesses than were done!");
            // Undo guess
            _guesses.RemoveLast();
            // Remove the first character and retry.
            domain = domain[1..];
        }
        return solvedBoard;
    }

    private (int col, int row, string domain) GetMostConstrainedVariable(string[,] board) {
        if (_useGuessCellPickHeuristic){
            return GetMostConstrainedVariableWithHeuristic(board);
        }
        var boardSize = board.GetBoardSize();
        return Enumerable.Range(0, boardSize)
        .SelectMany(row => Enumerable.Range(0, boardSize).Select(col => (col, row))).Select(pos => {
            var (col, row) = pos;
            var domain = board[col, row];
            return (col, row, domain);
        }).First(cellData => cellData.domain.Length > 1);
    }
    private static (int col, int row, string domain) GetMostConstrainedVariableWithHeuristic(string[,] board) {
        var boardSize = board.GetBoardSize();
        return Enumerable.Range(0, boardSize)
        .SelectMany(row => Enumerable.Range(0, boardSize).Select(col => (col, row)))
        .Select(pos => {
            var (col, row) = pos;
            var domain = board[col, row];
            return (col, row, domain);
        })
        .Where(cellData => cellData.domain.Length > 1) // Only consider variables, not set cells
        .MinBy(cellData => cellData.domain.Length);
    }

    private IEnumerable<(int col, int row, int domSize)> GetCellVariableNeighbours(int col, int row, string[,] board) {
        return GetCellNeighbourPositions(col, row, board.GetBoardSize())
        .Select(pos => (pos.col, pos.row, domSize: board[pos.col, pos.row].Length))
        .Where(tuple => tuple.domSize > 1);
    }
    
    private IEnumerable<(int col, int row)> GetCellNeighbourPositions(int col, int row, int boardSize) {
        var colNeighbours = GetColumnNeighboursOfCell(col, row, boardSize);
        var rowNeighbours = GetRowNeighboursOfCell(col, row, boardSize);
        var blockNeighbours = GetBlockNeighboursOfCell((col, row), boardSize);
        var cageNeighbours = GetCageNeighboursOfCell(col, row);
        var neighbours = colNeighbours.Union(rowNeighbours).Union(blockNeighbours).Union(cageNeighbours);
        return neighbours;
    }

    private static IEnumerable<(int col, int row)> GetColumnNeighboursOfCell(int col, int row, int boardSize) {
        return Enumerable.Range(0, boardSize)
        .Where(neighbourRow => neighbourRow != row) // Remove the cell itself
        .Select(neighbourRow => (col, row: neighbourRow));
    }

    private static IEnumerable<(int col, int row)> GetRowNeighboursOfCell(int col, int row, int boardSize) {
        return Enumerable.Range(0, boardSize)
        .Where(neighbourCol => neighbourCol != col) // Remove the cell itself
        .Select(neighbourCol => (col: neighbourCol, row));
    }

    private static IEnumerable<(int col, int row)> GetBlockNeighboursOfCell((int col, int row) cell, int boardSize) {
        var blockSize = Utils.BoardSizeToBlockSize(boardSize);
        var blockPos = Utils.PosToBlockPos(cell, blockSize);
        var indexInBlock = Utils.PosToIndexInBlock(cell, blockSize);
        return Enumerable.Range(0, boardSize)
        .Where(blockIndex => blockIndex != indexInBlock) // Remove the cell itself
        .Select(blockIndex => Utils.IndexToPos(blockIndex, blockSize))
        .Select(posInBlock => Utils.PosInBlockToPos(posInBlock, blockPos, blockSize));
    }

    private IEnumerable<(int col, int row)> GetCageNeighboursOfCell(int col, int row) {
        if (!_cages.Any()) return Enumerable.Empty<(int col, int row)>(); // This is a normal sudoku, not a killer one
        return _cages.FirstOrDefault(cage => cage.cells.Contains((col, row))).cells
            .Where(cell => cell != (col, row)); // Remove the cell itself
    }

    private void PropagateToCells(IEnumerable<((int col, int row), int)> cellsToPropagate, string[,] board) {
        var priorityQueue = new PriorityQueue<(int col, int row), int>(cellsToPropagate);

        while (priorityQueue.Count > 0){
            var (col, row) = priorityQueue.Dequeue();
            var cellIsConstrained = ConstrainCell(col, row, board);

            if (!cellIsConstrained) continue;
            
            var neighbours = GetCellVariableNeighbours(col, row, board);
            foreach (var (nCol, nRow, domSize) in neighbours){
                if (priorityQueue.UnorderedItems.Contains(((nCol, nRow), domSize))) continue;
                
                priorityQueue.Enqueue((nCol, nRow), domSize);
            }
        }
    }

    private bool ConstrainCell(int col, int row, string[,] board) {
        var currentDomain = board[col, row];
        if (currentDomain.Length == 1) return false;

        var colRestrictions = board.GetColumn(col).GetRestrictedDomain(_boardDomain);
        var rowRestrictions = board.GetRow(row).GetRestrictedDomain(_boardDomain);
        var blockRestrictions = board.GetBlock((col, row)).GetRestrictedDomain(_boardDomain);

        var restrictions = colRestrictions.Union(rowRestrictions).Union(blockRestrictions);
        var newDomain = string.Join(string.Empty, currentDomain.Except(restrictions));

        if (currentDomain == newDomain){
            return false;
        }

        board[col, row] = newDomain;
        return true;
    }

    private void RecalculateDomainsForCages(string[,] board) {
        var cagesWithDomains = _cages.Select(cage => {
            var domains = cage.cells.Select(pos => board[pos.col, pos.row]).ToImmutableList();
            return (cage.cells.ToImmutableArray(), cage.sum, domains);
        });
        foreach (var (cells, sum, domains) in cagesWithDomains){
            var newDomains = RecalculateDomainsForCage(sum, domains);
            for (var i = 0; i < cells.Length; i++){
                var cellPos = cells[i];
                var newDomain = newDomains[i];
                board[cellPos.col, cellPos.row] = newDomain;
            }
        }
    }

    private static string[] RecalculateDomainsForCage(int sum, IReadOnlyCollection<string> domains) {
        var joinedDomainsWithRightSum = Utils.JoinDomainsWithSum(domains, sum).ToImmutableList();
        var newDomains = new string[domains.Count];
        for (var i = 0; i < domains.Count; i++){
            var newDomain = joinedDomainsWithRightSum.Select(domain => domain[i].ToString()).Distinct().OrderBy(s => s).Aggregate(string.Empty, string.Concat);
            newDomains[i] = newDomain;
        }
        return newDomains;
    }
}
