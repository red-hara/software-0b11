using Godot;

public partial class Gripper : Node3D
{
    [Export]
    private Node3D mover;
    [Export]
    private float motionDuration = 0.25f;
    [Export]
    public float openPosition = 0.245f;
    [Export]
    public float closePosition = 0.1f;

    private float counter = 0;

    private TargetState? targetState = null;
    private State state = State.Open;

    public bool IsOpen
    {
        get => state == State.Open;
    }

    public bool IsClosed
    {
        get => state == State.Closed;
    }

    public override void _Process(double dt)
    {
        float delta = (float)dt;
        if (!(targetState is null))
        {
            counter += delta;
            if (counter >= 1)
            {
                switch (targetState.Value)
                {
                case TargetState.Open:
                    mover.Position = new Vector3(0, openPosition, 0);
                    state = State.Open;
                    break;
                case TargetState.Closed:
                    mover.Position = new Vector3(0, closePosition, 0);
                    state = State.Closed;
                    break;
                }
                targetState = null;
            }
            else
            {
                float current = 0;
                switch (targetState.Value)
                {
                case TargetState.Open:
                    current = closePosition + (openPosition - closePosition) * counter;
                    break;
                case TargetState.Closed:
                    current = openPosition + (closePosition - openPosition) * counter;
                    break;
                }
                mover.Position = new Vector3(0, current, 0);
            }
        }
    }

    public static Pose4 GetToolCenterPoint()
    {
        return new Pose4(new Vector3(0.25f, -0.24f, -0.51f), 90);
    }

    public void Open()
    {
        if (targetState is null && state == State.Closed)
        {
            targetState = TargetState.Open;
            state = State.Transition;
            counter = 0;
        }
    }

    public void Close()
    {
        if (targetState is null && state == State.Open)
        {
            targetState = TargetState.Closed;
            state = State.Transition;
            counter = 0;
        }
    }

    public enum State
    {
        Open,
        Closed,
        Transition,
    }

    private enum TargetState
    {
        Open,
        Closed,
    }
}
