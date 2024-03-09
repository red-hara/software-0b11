using Godot;
using System;

public partial class BoxSource : Node3D
{
    [Export]
    private Node3D boxAnchor;

    [Export]
    private PackedScene boxTemplate;

    [Export]
    private float spawnDelay = 10;

    private float counter = 0;

    private Node3D currentBox = null;

    public Pose4 Origin
    {
        get => cachedOrigin;
    }
    private Pose4 cachedOrigin;

    public Pose4 LocalBoxOrigin
    {
        get => cachedBoxOrigin;
    }
    private Pose4 cachedBoxOrigin;

    public bool BoxIsReady
    {
        get => currentBox is not null;
    }

    public override void _Ready()
    {
        cachedOrigin = new Pose4(Transform.Origin, Mathf.RadToDeg(Rotation.Z));
        cachedBoxOrigin = new Pose4(boxAnchor.Transform.Origin, Mathf.RadToDeg(boxAnchor.Rotation.Z));
    }

    public override void _Process(double delta)
    {

        counter += (float)delta;
        if (counter >= spawnDelay)
        {
            if (currentBox is null)
            {
                counter = 0;
                SpawnBox();
            }
        }
    }

    public Node3D TakeBox()
    {
        var box = currentBox;
        currentBox = null;
        return box;
    }

    private void SpawnBox()
    {
        currentBox = boxTemplate.Instantiate<Node3D>();
        AddChild(currentBox);
        currentBox.Transform = boxAnchor.Transform;
    }
}
