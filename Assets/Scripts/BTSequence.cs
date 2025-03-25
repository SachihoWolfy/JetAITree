using System.Collections.Generic;

public class BTSequence : BTNode
{
    private List<BTNode> children = new List<BTNode>();

    public void AddChild(BTNode node) => children.Add(node);

    public override bool Execute()
    {
        foreach (var node in children)
        {
            if (!node.Execute()) return false; // Fail if any child fails
        }
        return true;
    }
}
