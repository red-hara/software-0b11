using System;
using System.Threading.Tasks;
using Godot;

public partial class TaskController : Node
{
    private Controller<IMechanism<(float a, float b, float c, float d), Pose4>> controller;

    [Export]
    public TaskRobot robot;

    [Export]
    public Gripper gripper;

    [Export]
    public BoxSource boxSource;

    [Export]
    public BoxSource boxSource2;

    private Task run;

    public override void _Ready()
    {
        controller = new(robot);

        run = Run();
    }

    private async Task Run()
    {
        var interPosition = new Pose4(new Vector3(2, 0, 1.5f), -60);

        {
            var source = boxSource;
            await Joint(interPosition, 0.75f);
            await WaitFor(() => source.BoxIsReady);
            var currentBox = await PickBox(source);
            await Joint(interPosition, 0.75f);
            await PlaceBox(new Pose4(new Vector3(1, -2, 0), 45), currentBox);
        }
        {
            var source = boxSource2;
            await Joint(interPosition, 0.75f);
            await WaitFor(() => source.BoxIsReady);
            var currentBox = await PickBox(source);
            await Joint(interPosition, 0.75f);
            await PlaceBox(new Pose4(new Vector3(1, -2, 0.2f), 45), currentBox);
        }
    }

    private async Task<Node3D> PickBox(BoxSource source)
    {
        var tcp = Gripper.GetToolCenterPoint();
        var superior = source.LocalBoxOrigin * new Pose4(new Vector3(0, 0, 0.3f), 0);
        await Joint(superior, 0.75f, tcp, source.Origin);
        await Linear(source.LocalBoxOrigin, (0.25f, 90), tcp, source.Origin);
        gripper.Close();
        await WaitFor(() => gripper.IsClosed);
        var currentBox = AttachBox(source.TakeBox());
        await Linear(superior, (0.25f, 90), tcp, source.Origin);
        return currentBox;
    }

    private async Task PlaceBox(Pose4 target, Node3D box)
    {
        var tcp = Gripper.GetToolCenterPoint();
        var superior = target * new Pose4(new Vector3(0, 0, 0.3f), 0);
        await Joint(superior, 0.75f, tcp);
        await Linear(target, (0.25f, 90), tcp);
        gripper.Open();
        await WaitFor(() => gripper.IsOpen);
        DetachBox(box);
        await Linear(superior, (0.25f, 90), tcp);
    }

    private Node3D AttachBox(Node3D box)
    {
        Transform3D global = box.GlobalTransform;
        box.GetParent().RemoveChild(box);
        gripper.AddChild(box);
        box.GlobalTransform = global;
        return box;
    }

    private void DetachBox(Node3D box)
    {
        Transform3D global = box.GlobalTransform;
        box.GetParent().RemoveChild(box);
        robot.AddChild(box);
        box.GlobalTransform = global;
    }

    private async Task Joint(Pose4 target, float speed, Pose4? tool = null, Pose4? part = null)
    {
        await SpawnCommand(new Joint4<Pose4>(target, speed, tool, part));
    }

    private async Task Linear(Pose4 target, (float linear, float angular)velocity, Pose4? tool = null,
                              Pose4? part = null)
    {
        await SpawnCommand(new Linear4<(float, float, float, float)>(target, velocity, tool, part));
    }

    private async Task PositionHold(float duration, Pose4? tool = null, Pose4? part = null)
    {
        await SpawnCommand(new PositionHold<(float, float, float, float), Pose4>(duration, tool, part));
    }

    private async Task WaitFor(WaitForCondition condition)
    {
        await SpawnCommand(new WaitFor<(float, float, float, float), Pose4>(condition));
    }

    private async Task SpawnCommand(ICommand<IMechanism<(float a, float b, float c, float d), Pose4>> command)
    {
        var ex = await controller.EnqueueCommand(command);
        if (ex is not null)
        {
            GD.Print("Got exception " + ex);
            throw ex;
        }
    }

    public override void _Process(double delta)
    {

        try
        {
            run.WaitAsync(new TimeSpan(10));
        }
        catch (TimeoutException)
        {
        }

        if (controller is not null)
        {
            var status = controller.Step((float)delta);
            if (status == Status.Error)
            {
                GD.Print("Encountered error!");
                controller = null;
            }
        }
    }
}