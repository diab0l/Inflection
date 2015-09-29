namespace Inflection.TypeNode.TypeNode
{
    public interface IValue<out TTo> : ITypePath, IValue
    {
        TTo Value { get; }
    }

    public interface IValue
    {
        bool IsNull { get; }
    }
}