using MsfLang.Core.Modules;
using MsfLang.Core.Syntax;

namespace MsfLang.Core.Semantics;

/// <summary>
/// Maps name-bearing AST nodes to resolved bindings.
/// </summary>
public sealed class SemanticModel
{
    private readonly Dictionary<NameExpr, Binding> _nameBindings = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<MemberExpr, Binding> _memberBindings = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<QualifiedCallExpr, Binding> _qualifiedCallBindings = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<AssignStmt, Binding> _assignBindings = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<CallExpr, Binding> _callBindings = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<FunctionDecl, Binding> _functionBindings = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<VarDecl, Binding> _varBindings = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<FunctionDecl, IReadOnlyList<Binding>> _parameterBindings = new(ReferenceEqualityComparer.Instance);
    private readonly List<ModuleUnit> _modules = new();

    public void Set(NameExpr node, Binding binding) => _nameBindings[node] = binding;
    public void Set(MemberExpr node, Binding binding) => _memberBindings[node] = binding;
    public void Set(QualifiedCallExpr node, Binding binding) => _qualifiedCallBindings[node] = binding;
    public void Set(AssignStmt node, Binding binding) => _assignBindings[node] = binding;
    public void Set(CallExpr node, Binding binding) => _callBindings[node] = binding;
    public void Set(FunctionDecl node, Binding binding) => _functionBindings[node] = binding;
    public void Set(VarDecl node, Binding binding) => _varBindings[node] = binding;
    public void SetParameters(FunctionDecl node, IReadOnlyList<Binding> bindings) => _parameterBindings[node] = bindings;

    public Binding Get(NameExpr node) => _nameBindings[node];
    public Binding Get(MemberExpr node) => _memberBindings[node];
    public Binding Get(QualifiedCallExpr node) => _qualifiedCallBindings[node];
    public Binding Get(AssignStmt node) => _assignBindings[node];
    public Binding Get(CallExpr node) => _callBindings[node];
    public Binding Get(FunctionDecl node) => _functionBindings[node];
    public Binding Get(VarDecl node) => _varBindings[node];
    public IReadOnlyList<Binding> GetParameters(FunctionDecl node) => _parameterBindings[node];

    public bool TryGet(NameExpr node, out Binding binding) => _nameBindings.TryGetValue(node, out binding!);

    /// <summary>Modules reachable from this program, dependencies first, de-duplicated by symbol id.</summary>
    public IReadOnlyList<ModuleUnit> Modules => _modules;

    public void AddModule(ModuleUnit unit)
    {
        if (!_modules.Exists(m => m.Symbol.Id == unit.Symbol.Id))
        {
            _modules.Add(unit);
        }
    }

    public void MergeFrom(SemanticModel other)
    {
        foreach (var kv in other._nameBindings) _nameBindings[kv.Key] = kv.Value;
        foreach (var kv in other._memberBindings) _memberBindings[kv.Key] = kv.Value;
        foreach (var kv in other._qualifiedCallBindings) _qualifiedCallBindings[kv.Key] = kv.Value;
        foreach (var kv in other._assignBindings) _assignBindings[kv.Key] = kv.Value;
        foreach (var kv in other._callBindings) _callBindings[kv.Key] = kv.Value;
        foreach (var kv in other._functionBindings) _functionBindings[kv.Key] = kv.Value;
        foreach (var kv in other._varBindings) _varBindings[kv.Key] = kv.Value;
        foreach (var kv in other._parameterBindings) _parameterBindings[kv.Key] = kv.Value;
        foreach (var unit in other._modules) AddModule(unit);
    }

    public SemanticModel Clone()
    {
        var clone = new SemanticModel();
        clone.MergeFrom(this);
        return clone;
    }
}
