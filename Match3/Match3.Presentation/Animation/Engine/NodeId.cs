namespace Match3.Presentation.Animation.Engine;

public readonly record struct NodeId(Guid Value)
{
    public static NodeId New()
    {
        return new NodeId(Guid.NewGuid());
    }
}
