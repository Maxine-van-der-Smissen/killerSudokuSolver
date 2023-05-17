using System.Diagnostics;
using KillerSudoku;

#region Sudoku Definitions

const string testStart = "...7....." +
                         "..2...6.." +
                         "8...13..5" +
                         ".6..745.." +
                         "4..9....." +
                         "...8....7" +
                         ".....9..." +
                         "3...45..1" +
                         ".8.....3.";

// puzzle 23739
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage0 = (new (int col, int row)[ ] {(0, 0), (0, 1), (0, 2), (1, 2), (2, 2)}, 26);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage1 = (new (int col, int row)[ ] {(1, 0), (2, 0), (1, 1)}, 10);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage2 = (new (int col, int row)[ ] {(3, 0), (4, 0), (5, 0)}, 20);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage3 = (new (int col, int row)[ ] {(6, 0), (7, 0), (7, 1)}, 10);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage4 = (new (int col, int row)[ ] {(8, 0), (8, 1), (8, 2), (7, 2), (6, 2)}, 29);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage5 = (new (int col, int row)[ ] {(2, 1), (3, 1), (4, 1), (5, 1), (6, 1)}, 31);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage6 = (new (int col, int row)[ ] {(3, 2), (3, 3), (2, 3)}, 12);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage7 = (new (int col, int row)[ ] {(4, 2), (4, 3)}, 11);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage8 = (new (int col, int row)[ ] {(5, 2), (5, 3), (6, 3)}, 9);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage9 = (new (int col, int row)[ ] {(0, 3), (1, 3)}, 14);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage10 = (new (int col, int row)[ ] {(7, 3), (8, 3)}, 8);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage11 = (new (int col, int row)[ ] {(0, 4), (1, 4)}, 9);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage12 = (new (int col, int row)[ ] {(2, 4), (2, 5)}, 6);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage13 = (new (int col, int row)[ ] {(3, 4), (4, 4), (5, 4), (3, 5), (5, 5)}, 27);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage14 = (new (int col, int row)[ ] {(6, 4), (6, 5)}, 9);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage15 = (new (int col, int row)[ ] {(7, 4), (8, 4)}, 8);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage16 = (new (int col, int row)[ ] {(0, 5), (1, 5)}, 8);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage17 = (new (int col, int row)[ ] {(4, 5), (4, 6)}, 11);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage18 = (new (int col, int row)[ ] {(7, 5), (8, 5)}, 17);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage19 = (new (int col, int row)[ ] {(0, 6), (1, 6), (2, 6)}, 19);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage20 = (new (int col, int row)[ ] {(3, 6), (5, 6), (3, 7), (4, 7), (5, 7)}, 20);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage21 = (new (int col, int row)[ ] {(6, 6), (7,6), (8, 6)}, 14);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage22 = (new (int col, int row)[ ] {(0, 7), (0, 8), (1, 8)}, 7);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage23 = (new (int col, int row)[ ] {(1, 7), (2, 7), (2, 8)}, 19);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage24 = (new (int col, int row)[ ] {(6, 7), (7, 7), (6, 8)}, 13);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage25 = (new (int col, int row)[ ] {(8, 7), (7, 8), (8, 8)}, 18);
(IReadOnlyCollection<(int col, int row)> cells, int sum) cage26 = (new (int col, int row)[ ] {(3, 8), (4, 8), (5, 8)}, 20);
IReadOnlyCollection<(IReadOnlyCollection<(int col, int row)> cells, int sum)> killerBoardCages = new List<(IReadOnlyCollection<(int col, int row)> cells, int sum)>(new[ ] { cage0, cage1, cage2, cage3, cage4, cage5, cage6, cage7, cage8, cage9, cage10, cage11, cage12, cage13, cage14, cage15, cage16, cage17, cage18, cage19, cage20, cage21, cage22, cage23, cage24, cage25, cage26 });

#endregion

void SolveBoardTimed(Solver solver, string solveText, SolverOptions? options = null) {
    var stopwatch = new Stopwatch();
    stopwatch.Start();
    var solution = solver.Solve(options)?.ToBoardString();
    stopwatch.Stop();
    Console.WriteLine($"{solveText} solved in: {stopwatch.ElapsedMilliseconds} milliseconds");
    Console.WriteLine($"{solveText} result:");
    Console.WriteLine(solution);
}

#region Normal Sudoku Stats

var testBoard = new Board(9, testStart);
var sudokuSolver = new Solver(testBoard);

// Cage domain recalculations can be ignored, because normal sudoku's don't have cages
var noCageDomainRecalculationOptionsNormal = new SolverOptions(false, true);
SolveBoardTimed(sudokuSolver, "Normal sudoku with heuristic for guesses optimization", noCageDomainRecalculationOptionsNormal);
SolveBoardTimed(sudokuSolver, "Normal sudoku with no heuristic for guesses optimization", SolverOptions.WithNoOptimizations());

#endregion

#region Killer Sudoku Stats

var killerBoard = new KillerBoard(9, killerBoardCages);
var killerSolver = new Solver(killerBoard);

SolveBoardTimed(killerSolver, "Killer board with all optimizations", SolverOptions.WithAllOptimizations());

var noCageDomainRecalculationOptionsKiller = new SolverOptions(false, true);
SolveBoardTimed(killerSolver, "Killer board with no cage domain recalculation", noCageDomainRecalculationOptionsKiller);

var noGuessCellPickHeuristicOptions = new SolverOptions(true, false);
SolveBoardTimed(killerSolver, "Killer board with no cell pick heuristic for guesses", noGuessCellPickHeuristicOptions);

SolveBoardTimed(killerSolver, "Killer board with no optimizations", SolverOptions.WithNoOptimizations());

#endregion
