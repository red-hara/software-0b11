using Godot;
using System;

[Tool]
public partial class TaskRobot : Node3D, IMechanism<(float a, float b, float c, float d), Pose4>
{
    [Export]
    public float A
    {
        get => generalized.a;
        set => generalized.a = value;
    }
    [Export]
    public float B
    {
        get => generalized.b;
        set => generalized.b = value;
    }
    [Export]
    public float C
    {
        get => generalized.c;
        set => generalized.c = value;
    }
    [Export]
    public float D
    {
        get => generalized.d;
        set => generalized.d = value;
    }

    public (float a, float b, float c, float d) generalized = (0, 0, 0, 0);
    [Export]
    public Node3D column;

    [Export]
    public Node3D shoulder;

    [Export]
    public Node3D forearm;

    [Export]
    public Node3D wrist;

    [Export]
    public Node3D flange;

    [Export]
    public Node3D connector;

    [Export]
    public Node3D wristConnector;

    [Export]
    public Node3D columnConnector;

    [Export]
    public Node3D mover;

    [Export]
    public Node3D forearmConnector;

    public override void _Process(double delta)
    {
        column.RotationDegrees = new Vector3(0, 0, generalized.a);
        shoulder.RotationDegrees = new Vector3(0, generalized.b, 0);
        forearm.RotationDegrees = new Vector3(0, generalized.c - generalized.b, 0);
        wrist.RotationDegrees = new Vector3(0, -generalized.c, 0);
        flange.RotationDegrees = new Vector3(0, 0, generalized.d);
        connector.RotationDegrees = new Vector3(0, -generalized.b, 0);
        wristConnector.RotationDegrees = new Vector3(0, generalized.c, 0);
        columnConnector.RotationDegrees = new Vector3(0, generalized.b, 0);
        mover.RotationDegrees = new Vector3(0, generalized.c, 0);
        forearmConnector.RotationDegrees = new Vector3(0, generalized.b - generalized.c, 0);
    }

    public Pose4 CurrentPosition()
    {
        return SolveForward(generalized).Value;
    }

    public Pose4? SolveForward((float a, float b, float c, float d)gen)
    {
        var a = Mathf.DegToRad(gen.a);
        var b = Mathf.DegToRad(gen.b);
        var c = Mathf.DegToRad(gen.c);
        var d = Mathf.DegToRad(gen.d);
        var column = new Transform3D(new Basis(new Vector3(0, 0, 1), a), Vector3.Zero);
        var shoulder = new Transform3D(new Basis(new Vector3(0, 1, 0), b), new Vector3(0.3f, 0, 0.8f));
        var elbow = new Transform3D(new Basis(new Vector3(0, 1, 0), c - b), new Vector3(0, 0, 1.28f));
        var wrist = new Transform3D(new Basis(new Vector3(0, 1, 0), -c), new Vector3(1.35f, 0, 0));
        var flange = new Transform3D(new Basis(new Vector3(0, 0, 1), d), new Vector3(0.26f, 0, -0.247f));
        return new Pose4((column * shoulder * elbow * wrist * flange).Origin, gen.a + gen.d);
    }

    public Pose4? SetForward((float a, float b, float c, float d)generalized)
    {
        this.generalized = generalized;
        return SolveForward(generalized);
    }

    public (float a, float b, float c, float d) CurrentGeneralized()
    {
        return generalized;
    }

    public (float a, float b, float c, float d)? SolveInverse(Pose4 position)
    {
        var wrist = position.translation - new Vector3(0, 0, 0.8f - 0.247f);
        var projection = Mathf.Sqrt(wrist.X * wrist.X + wrist.Y * wrist.Y) - 0.56f;
        var dSquared = projection * projection + wrist.Z * wrist.Z;
        if (dSquared > Mathf.Pow(1.35f + 1.28f, 2))
        {
            return null;
        }
        if (dSquared < Mathf.Pow(1.35f - 1.28f, 2))
        {
            return null;
        }
        var alpha = Mathf.Atan2(wrist.Y, wrist.X);
        var beta = Mathf.Atan2(projection, wrist.Z) -
                   Mathf.Acos((1.28f * 1.28f + dSquared - 1.35f * 1.35f) / (2 * Mathf.Sqrt(dSquared) * 1.28f));
        var gamma = -Mathf.Pi / 2 + Mathf.Acos((1.35f * 1.35f + 1.28f * 1.28f - dSquared) / (2 * 1.35f * 1.28f));

        return (Mathf.RadToDeg(alpha), Mathf.RadToDeg(beta), Mathf.RadToDeg(beta - gamma),
                position.rotation - Mathf.RadToDeg(alpha));
    }

    public (float a, float b, float c, float d)? SetInverse(Pose4 position)
    {
        var solution = SolveInverse(position);
        if (solution is null)
        {
            return null;
        }
        generalized = solution.Value;
        return solution;
    }

    public (float a, float b, float c, float d) DriveSpeed()
    {
        return (130, 130, 130, 300);
    }
}
