namespace MsfLang.Core.Runtime;

public sealed class MsEnvironment
{
    private readonly MsEnvironment? _parent;
    private readonly Dictionary<int, MsfValue> _values = new();

    public MsEnvironment(MsEnvironment? parent = null)
    {
        _parent = parent;
    }

    public MsEnvironment Global => _parent?.Global ?? this;

    public void Define(int id, MsfValue value) => _values[id] = value;

    public MsfValue Get(int id)
    {
        if (_values.TryGetValue(id, out var value))
        {
            return value;
        }

        if (_parent is not null)
        {
            return _parent.Get(id);
        }

        throw new InvalidOperationException($"Undefined binding id {id} at runtime.");
    }

    public void Assign(int id, MsfValue value)
    {
        if (_values.ContainsKey(id))
        {
            _values[id] = value;
            return;
        }

        if (_parent is not null)
        {
            _parent.Assign(id, value);
            return;
        }

        throw new InvalidOperationException($"Undefined binding id {id} at runtime.");
    }

    public MsEnvironment SnapshotShallow() => new(_parent, new Dictionary<int, MsfValue>(_values));

    private MsEnvironment(MsEnvironment? parent, Dictionary<int, MsfValue> values)
    {
        _parent = parent;
        _values = values;
    }
}
