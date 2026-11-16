using MsfLang.Core.Source;

namespace MsfLang.Core.Semantics;

public enum BindingKind
{
    Variable,
    Function,
    Builtin
}

public sealed class Binding
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required BindingKind Kind { get; init; }
    public required bool IsMutable { get; init; }
    public int? Arity { get; init; }
    public TextSpan DeclarationSpan { get; init; }
}
