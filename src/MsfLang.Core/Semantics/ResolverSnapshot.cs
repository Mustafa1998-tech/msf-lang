namespace MsfLang.Core.Semantics;

public sealed class ResolverSnapshot
{
    internal Resolver.Scope Global { get; }
    internal int NextId { get; }
    public SemanticModel Model { get; }

    internal ResolverSnapshot(Resolver.Scope global, int nextId, SemanticModel model)
    {
        Global = global;
        NextId = nextId;
        Model = model;
    }
}
