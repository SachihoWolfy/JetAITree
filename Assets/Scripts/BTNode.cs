public abstract class BTNode
{
    public abstract bool Execute();
}

// These System things are cool, I need to use them more often. Having seperate files for seperate things SUCKS.
public class BTCondition : BTNode
{
    private System.Func<bool> condition;

    public BTCondition(System.Func<bool> condition)
    {
        this.condition = condition;
    }

    public override bool Execute()
    {
        return condition();
    }
}

public class BTAction : BTNode
{
    private System.Action action;

    public BTAction(System.Action action)
    {
        this.action = action;
    }

    public override bool Execute()
    {
        action();
        return true;
    }
}