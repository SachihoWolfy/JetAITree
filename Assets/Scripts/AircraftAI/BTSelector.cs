using System.Collections.Generic;

public class BTSelector : BTNode
{
    private List<BTNode> children = new List<BTNode>();

    public void AddChild(BTNode node) => children.Add(node);

    public override bool Execute()
    {
        foreach (var node in children)
        {
            if (node.Execute()) return true;
        }
        return false;
    }
}
