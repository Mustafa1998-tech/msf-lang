using System.Globalization;

namespace MsfLang.Core.Runtime;

public abstract record MsfValue
{
    public static readonly MsfValue Nil = new MsfNil();
    public static readonly MsfValue True = new MsfBoolean(true);
    public static readonly MsfValue False = new MsfBoolean(false);

    public abstract string TypeName { get; }

    public static MsfValue FromBool(bool value) => value ? True : False;
}

public sealed record MsfNumber(double Value) : MsfValue
{
    public override string TypeName => "number";
}

public sealed record MsfString(string Value) : MsfValue
{
    public override string TypeName => "string";
}

public sealed record MsfBoolean(bool Value) : MsfValue
{
    public override string TypeName => "boolean";
}

public sealed record MsfNil : MsfValue
{
    public override string TypeName => "nil";
}

internal abstract record CallableValue : MsfValue;

internal sealed record UserFunction(Syntax.FunctionDecl Declaration) : CallableValue
{
    public override string TypeName => "function";
}

internal sealed record PrintBuiltin : CallableValue
{
    public override string TypeName => "builtin";
}
