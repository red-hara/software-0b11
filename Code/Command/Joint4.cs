using System;
using Godot;

public class Joint4<Pos> : ICommand<IMechanism<(float a, float b, float c, float d), Pos>>
    where Pos : struct, Frame<Pos>
{
    private Pos start;
    private readonly Pos target;
    private readonly float speed;
    private float progress;
    private readonly Pos? tool;
    private readonly Pos? part;

    public Joint4(Pos target, float speed, Pos? tool = null, Pos? part = null)
    {
        if (speed <= 0 || speed > 1)
        {
            throw new ArgumentException("Speed can't be less or equal to zero or greater than 1");
        }
        this.target = target;
        this.speed = speed;

        this.tool = tool;
        this.part = part;
    }

    public void Init(Controller<IMechanism<(float a, float b, float c, float d), Pos>> controller)
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

    public Progress Step(Controller<IMechanism<(float a, float b, float c, float d), Pos>> controller, float delta)
    {
        var flangeStart = start;
        if (tool.HasValue)
        {
            flangeStart = flangeStart.Combine(tool.Value.Inverse());
        }
        if (part.HasValue)
        {
            flangeStart = part.Value.Combine(flangeStart);
        }

        var flangeTarget = target;
        if (tool.HasValue)
        {
            flangeTarget = flangeTarget.Combine(tool.Value.Inverse());
        }
        if (part.HasValue)
        {
            flangeTarget = part.Value.Combine(flangeTarget);
        }

        var solutionStart = controller.Mechanism.SolveInverse(flangeStart);
        var solutionTarget = controller.Mechanism.SolveInverse(flangeTarget);

        if (solutionStart is null)
        {
            throw new Exception("Could not solve inverse kinematics problem for flange pose " + flangeStart);
        }
        if (solutionTarget is null)
        {
            throw new Exception("Could not solve inverse kinematics problem for flange pose " + flangeTarget);
        }

        var (a, b, c, d) = controller.Mechanism.DriveSpeed();

        var deltaA = solutionTarget.Value.a - solutionStart.Value.a;
        var deltaB = solutionTarget.Value.b - solutionStart.Value.b;
        var deltaC = solutionTarget.Value.c - solutionStart.Value.c;
        var deltaD = solutionTarget.Value.d - solutionStart.Value.d;
        var timeA = Math.Abs(deltaA) / a / speed;
        var timeB = Math.Abs(deltaB) / b / speed;
        var timeC = Math.Abs(deltaC) / c / speed;
        var timeD = Math.Abs(deltaD) / d / speed;
        var maximum = Mathf.Max(Mathf.Max(timeA, timeB), Mathf.Max(timeC, timeD));

        if (maximum <= 0)
        {
            return Progress.Done;
        }

        progress += delta / maximum;

        if (progress >= 1)
        {
            if (controller.Mechanism.SetForward(solutionTarget.Value) is null)
            {
                throw new Exception("Could not move robot to generalized pose " + solutionTarget.Value);
            }
            return Progress.Done;
        }

        var currentA = solutionStart.Value.a + deltaA * progress;
        var currentB = solutionStart.Value.b + deltaB * progress;
        var currentC = solutionStart.Value.c + deltaC * progress;
        var currentD = solutionStart.Value.d + deltaD * progress;
        var current = (currentA, currentB, currentC, currentD);

        if (controller.Mechanism.SetForward(current) is null)
        {
            throw new Exception("Could not move robot to generalized pose " + current);
        }
        return Progress.Ongoing;
    }
}