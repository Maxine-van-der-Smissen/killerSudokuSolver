namespace KillerSudoku; 

public record SolverOptions(bool DoCageDomainRecalculations, bool UseGuessCellPickHeuristic) {
    public static SolverOptions WithAllOptimizations() {
        return new SolverOptions(true, true);
    }

    public static SolverOptions WithNoOptimizations() {
        return new SolverOptions(false, false);
    }
}
