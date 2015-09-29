namespace Inflection.TypeExtensions.Extension
{
    using System;

    using Immutable.Monads;

    public interface IQueryTransform<out TId>
    {
        TId Id { get; }
    }

    public interface IQueryTransform<TRoot, T, out TId>
        : IQuery<TRoot, T, TId>, IMaybe<Func<TRoot, T, TRoot>>
    {
    }
}
