public interface IMechanism<Gen, Pos>
    where Gen : struct
    where Pos : struct {
    public Pos CurrentPosition();
    public Pos? SolveForward(Gen generalized);
    public Pos? SetForward(Gen generalized);

    public Gen CurrentGeneralized();
    public Gen? SolveInverse(Pos position);
    public Gen? SetInverse(Pos position);

    public Gen DriveSpeed();
}
