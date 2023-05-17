namespace KillerSudoku; 

public static class Utils {
    public const string PotentialDomain = "123456789ABCDEFGHIJKLMNOP";

    public static (int col, int row) PosToBlockPos((int col, int row) pos, int blockSize) {
        var blockCol = pos.col / blockSize;
        var blockRow = pos.row / blockSize;
        return (blockCol, blockRow);
    }

    public static int PosToIndexInBlock((int col, int row) pos, int blockSize) {
        var posInBlock = PosToPosInBlock(pos, blockSize);
        return PosToIndex(posInBlock, blockSize);
    }

    public static (int col, int row) PosInBlockToPos((int col, int row) posInBlock, (int col, int row) blockPos, int blockSize) {
        var col = posInBlock.col + blockPos.col * blockSize;
        var row = posInBlock.row + blockPos.row * blockSize;
        return (col, row);
    }

    public static bool IsPerfectSquare(int boardSize) {
        var blockSize = BoardSizeToBlockSize(boardSize);
        return blockSize * blockSize == boardSize;
    }

    public static int BoardSizeToBlockSize(int size) {
        return (int) Math.Sqrt(size);
    }

    public static string GetDomainForSize(int size) {
        return PotentialDomain[..size];
    }

    // TODO 
    public static IEnumerable<string> JoinDomainsWithSum(IEnumerable<string> domains, int sum) {
        using var domainsEnumerator = domains.GetEnumerator();
        domainsEnumerator.MoveNext();
        var joinedDomains = domainsEnumerator.Current.ToCharArray().Select(char.ToString);
        while (domainsEnumerator.MoveNext()){
            var dom = domainsEnumerator.Current.ToCharArray();
            joinedDomains = joinedDomains.Join(dom, _ => true, _ => true, (s, c) => s + c);
        }
        return joinedDomains.Where(joinedDomain => GetSumForDomain(joinedDomain) == sum);
    }

    // Same irrespective of block or board
    public static (int col, int row) IndexToPos(int index, int size) {
        var col = index % size;
        var row = index / size;
        return (col, row);
    }

    private static int PosToIndex((int col, int row) pos, int size) {
        return PosToIndex(pos.col, pos.row, size);
    }

    private static int PosToIndex(int col, int row, int size) {
        return col + row * size;
    }

    private static int GetSumForDomain(string domain) {
        return domain.ToCharArray().Select(GetValueForDomainChar).Sum();
    }

    private static int GetValueForDomainChar(char domainChar) {
        return PotentialDomain.IndexOf(domainChar) + 1;
    }

    private static (int col, int row) PosToPosInBlock((int col, int row) pos, int blockSize) {
        return (pos.col % blockSize, pos.row % blockSize);
    }
}
