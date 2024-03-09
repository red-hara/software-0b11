using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Controller<Mech>
{
    public Mech Mechanism { get; }
    private ICommand<Mech> currentCommand;
    private readonly Queue<(ICommand<Mech> command, TaskCompletionSource<Exception> completion)> commands;
    private TaskCompletionSource<Exception> taskCompletionSource;

    public Controller(Mech mechanism)
    {
        Mechanism = mechanism;
        commands = new();
    }

    public Status Step(float delta)
    {
        if (currentCommand is not null)
        {
            try
            {
                Progress progress = currentCommand.Step(this, delta);
                switch (progress)
                {
                case Progress.Ongoing:
                    break;
                case Progress.Done:
                    currentCommand = null;
                    taskCompletionSource.SetResult(null);
                    StartNextCommand();
                    break;
                }
            }
            catch (Exception ex)
            {
                taskCompletionSource.SetResult(ex);
                commands.Clear();
                return Status.Error;
            }
        }
        else
        {
            StartNextCommand();
        }
        return Status.Ok;
    }

    private void StartNextCommand()
    {
        if (commands.Count > 0)
        {
            var (command, completion) = commands.Dequeue();
            currentCommand = command;
            taskCompletionSource = completion;
            try
            {
                currentCommand.Init(this);
            }
            catch (Exception ex)
            {
                taskCompletionSource.SetResult(ex);
            }
        }
    }

    public Task<Exception> EnqueueCommand(ICommand<Mech> command)
    {
        TaskCompletionSource<Exception> source = new();
        commands.Enqueue((command, source));
        return source.Task;
    }
}

public enum Status
{
    Ok,
    Error,
}
