namespace Inflection.TypeNode.TypeNode
{
    using System;

    using Immutable.Monads;

    public interface IValuePath<TFrom, TTo> : ITypePath<TFrom, TTo>, IValue<TTo>
    {
        IValuePath<TFrom, T> Wrap<T>(Func<TTo, T> vGet, IMaybe<Func<TTo, T, TTo>> mvSet, T value);
    }
}