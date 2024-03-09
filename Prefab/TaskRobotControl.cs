using Godot;

[Tool]
public partial class TaskRobotControl : Node
{

    [Export]
    public TaskRobot taskRobot;
    [Export]
    public Node3D hold;

    public override void _Process(double delta)
    {
        if (taskRobot is not null && hold is not null)
        {
            var position = taskRobot.CurrentPosition();
            hold.Transform = new Transform3D(new Basis(new Vector3(0, 0, 1), Mathf.DegToRad(position.rotation)),
                                             position.translation);
        }
    }
}
