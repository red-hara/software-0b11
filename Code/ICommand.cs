public interface ICommand<Mech>
{
    public void Init(Controller<Mech> controller);
    public Progress Step(Controller<Mech> controller, float delta);
}

public enum Progress
{
    Ongoing,
    Done,
}