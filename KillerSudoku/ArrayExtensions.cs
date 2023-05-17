using System.Text;

namespace KillerSudoku; 

public static class ArrayExtensions {
    public static int GetBoardSize<T>(this T[,] board) {
        return board.GetLength(0);
    }
    public static IEnumerable<T> GetRow<T>(this T[,] board, int rowNumber) {
        return Enumerable.Range(0, board.GetBoardSize())
            .Select(col => board[col, rowNumber]);
    }

    public static IEnumerable<T> GetColumn<T>(this T[,] board, int columnNumber) {
        return Enumerable.Range(0, board.GetBoardSize())
            .Select(row => board[columnNumber, row]);
    }

    public static IEnumerable<T> GetBlock<T>(this T[,] board, (int col, int row)pos) {
        var boardSize = board.GetBoardSize();
        var blockSize = Utils.BoardSizeToBlockSize(boardSize);
        var blockI = Utils.PosToBlockPos(pos, blockSize);
        return GetBlock(board, blockI, boardSize, blockSize);
    }

    public static T[,] CloneBoard<T>(this T[,] board) {
        return board.Clone() as T[,] ?? throw new InvalidOperationException("Can not clone 'null'.");
    }

    private static IEnumerable<T> GetBlock<T>(this T[,] board, (int col, int row) blockPos, int boardSize, int blockSize) {
        return Enumerable.Range(0, boardSize)
            .Select(index => Utils.IndexToPos(index, blockSize))
            .Select(posInBlock => Utils.PosInBlockToPos(posInBlock, blockPos, blockSize))
            .Select(pos => board[pos.Item1, pos.Item2]);
    }

    public static string ToBoardString(this string[,] board) {
        var strBuilder = new StringBuilder();
        var boardSize = board.GetBoardSize();
        for (var i = 0; i < boardSize * boardSize; i++) {
            var (col, row) = Utils.IndexToPos(i, boardSize);
            var cell = board[col, row];
            var cellValue = cell.Length == 1 ? cell[0] : '.';
            strBuilder.Append(cellValue);
            if ((i + 1) % boardSize == 0) strBuilder.AppendLine();
        }

        return strBuilder.ToString();
    }

    public static string GetRestrictedDomain(this IEnumerable<string> cellGroup, string domain) {
        var setVariablesCollection = cellGroup.Where(cell => cell.Length == 1).SelectMany(cellValue => cellValue.ToCharArray());
        var restrictedDomains = domain.Intersect(setVariablesCollection);
        return string.Join(string.Empty, restrictedDomains);
    }
}
