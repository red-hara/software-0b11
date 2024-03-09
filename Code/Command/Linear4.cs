using System;
using Godot;

public class Linear4<Gen> : ICommand<IMechanism<Gen, Pose4>>
    where Gen : struct
{
    private Pose4 start;
    private readonly Pose4 target;
    private readonly(float linear, float angular) velocity;
    private float progress;
    private readonly Pose4? tool;
    private readonly Pose4? part;

    public Linear4(Pose4 target, (float linear, float angular)velocity, Pose4? tool = null, Pose4? part = null)
    {
        if (velocity.linear <= 0 || velocity.angular <= 0)
        {
            throw new ArgumentException("Velocity can't be less or equal to zero");
        }
        this.target = target;
        this.velocity = velocity;

        this.tool = tool;
        this.part = part;
    }

    public void Init(Controller<IMechanism<Gen, Pose4>> controller)
    {
        start = controller.Mechanism.CurrentPosition();
        if (tool.HasValue)
        {
            start = start.Combine(tool.Value);
        }
        if (part.HasValue)
        {
            start = part.Value.Inverse().Combine(start);
        }
    }

    public Progress Step(Controller<IMechanism<Gen, Pose4>> controller, float delta)
    {
        var deltaPose = Pose4.Delta(start, target);
        var deltaT = deltaPose.translation;
        var deltaR = deltaPose.rotation;
        if (deltaT.Length() <= 0 && Mathf.Abs(deltaR) <= 0)
        {
            return Progress.Done;
        }

        var timeT = deltaT.Length() / velocity.linear;
        var timeR = Mathf.Abs(deltaR) / velocity.angular;
        var maximum = Mathf.Max(timeT, timeR);

        progress += delta / maximum;

        if (progress >= 1)
        {
            var flangeTarget = target;
            if (tool.HasValue)
            {
                flangeTarget = flangeTarget.Combine(tool.Value.Inverse());
            }
            if (part.HasValue)
            {
                flangeTarget = part.Value.Combine(flangeTarget);
            }

            if (controller.Mechanism.SetInverse(flangeTarget) is null)
            {
                throw new Exception("Could not move robot to flange position " + flangeTarget.ToString());
            }
            return Progress.Done;
        }

        var current = start.Lerp(target, progress);
        var currentTarget = current;
        if (tool.HasValue)
        {
            currentTarget = currentTarget.Combine(tool.Value.Inverse());
        }
        if (part.HasValue)
        {
            currentTarget = part.Value.Combine(currentTarget);
        }
        if (controller.Mechanism.SetInverse(currentTarget) is null)
        {
            throw new Exception("Could not move robot to flange position " + currentTarget.ToString());
        }
        return Progress.Ongoing;
    }
}