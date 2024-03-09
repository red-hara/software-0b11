using Godot;

public struct Pose4 : Frame<Pose4>
{
    public Vector3 translation;
    public float rotation;

    public Pose4(Vector3 translation, float rotation)
    {
        this.translation = translation;
        this.rotation = rotation;
    }

    public Pose4 Lerp(Pose4 to, float weight)
    {
        Vector3 position = translation.Lerp(to.translation, weight);
        float deltaAngle = Mathf.Wrap(to.rotation - rotation, -180, 180);
        return new Pose4(position, Mathf.Wrap(rotation + deltaAngle * weight, -180, 180));
    }

    public Pose4 Combine(Pose4 other)
    {
        return this * other;
    }

    public Pose4 Inverse()
    {
        return new Pose4(-translation.Rotated(new Vector3(0, 0, 1), -Mathf.DegToRad(rotation)), -rotation);
    }

    public static Pose4 operator *(Pose4 left, Pose4 right)
    {
        return new Pose4(left.translation +
                             right.translation.Rotated(new Vector3(0, 0, 1), Mathf.DegToRad(left.rotation)),
                         Mathf.Wrap(left.rotation + right.rotation, -180, 180));
    }

    public static Pose4 Delta(Pose4 from, Pose4 to)
    {
        return from.Inverse() * to;
    }

    public override readonly string ToString()
    {
        return "Pose4 {" + translation + ", " + rotation + "}";
    }
}
