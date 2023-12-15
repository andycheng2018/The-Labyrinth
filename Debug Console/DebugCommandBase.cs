using System;

public class DebugCommandBase
{
    private string _commandId;
    private string _commandDescription;

    public string commandId {get {return _commandId;}}
    public string commandDescription {get {return _commandDescription;}}

    public DebugCommandBase(string id, string description) {
        _commandId = id;
        _commandDescription = description;
    }
}

public class DebugCommand : DebugCommandBase {
    private Action command;
    private Action<string> commandStr;

    public DebugCommand(string id, string description, Action command) : base (id, description) {
        this.command = command;
    }

    public DebugCommand(string id, string description, Action<string> commandStr) : base (id, description) {
        this.commandStr = commandStr;
    }

    public void Invoke() {
        command.Invoke();
    }

    public void Invoke(string parameter)
    {
        commandStr.Invoke(parameter);
    }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class CommandAttribute : Attribute
{
    public string commandId { get; }
    public string commandDescription { get; }

    public CommandAttribute(string commandId, string commandDescription)
    {
        this.commandId = commandId;
        this.commandDescription = commandDescription;
    }
}