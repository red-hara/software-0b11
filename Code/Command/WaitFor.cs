public class WaitFor<Gen, Pos> : ICommand<IMechanism<Gen, Pos>>
    where Gen : struct
    where Pos : struct
{
    private readonly WaitForCondition condition;

    public WaitFor(WaitForCondition condition)
    {
        this.condition = condition;
    }

    public void Init(Controller<IMechanism<Gen, Pos>> controller)
    {
    }

    public Progress Step(Controller<IMechanism<Gen, Pos>> controller, float delta)
    {
        if (condition())
        {
            return Progress.Done;
        }
        return Progress.Ongoing;
    }
}

public delegate bool WaitForCondition();