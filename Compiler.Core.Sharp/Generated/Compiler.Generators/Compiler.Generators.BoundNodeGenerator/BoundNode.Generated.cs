namespace Compiler.Core.Sharp;

internal partial class BoundVisitor
{
public virtual BoundNode VisitBoundNamespace(BoundNamespace node)
    {
        return DefaultVisit(node);
    }
}
