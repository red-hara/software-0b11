using System;

public class PositionHold<Gen, Pos> : ICommand<IMechanism<Gen, Pos>>
    where Gen : struct
    where Pos : struct, Frame<Pos>
{
    private float duration;
    private Pos position;

    private readonly Pos? tool;
    private readonly Pos? part;

    public PositionHold(float duration, Pos? tool = null, Pos? part = null)
    {
        this.duration = duration;
        this.tool = tool;
        this.part = part;
    }

    public void Init(Controller<IMechanism<Gen, Pos>> controller)
    {
        position = controller.Mechanism.CurrentPosition();
        if (tool.HasValue)
        {
            position = position.Combine(tool.Value);
        }
        if (part.HasValue)
        {
            position = part.Value.Inverse().Combine(position);
        }
    }
    public Progress Step(Controller<IMechanism<Gen, Pos>> controller, float delta)
    {
        duration -= delta;
        if (duration < 0)
        {
            return Progress.Done;
        }

        if (tool.HasValue)
        {
            position = position.Combine(tool.Value.Inverse());
        }
        if (part.HasValue)
        {
            position = part.Value.Combine(position);
        }

        if (controller.Mechanism.SetInverse(position) is null)
        {
            throw new Exception("Could not move robot to flange position " + position.ToString());
        }
        return Progress.Ongoing;
    }
}