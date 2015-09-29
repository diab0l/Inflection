namespace Inflection.TypeNode.TypeNode
{
    using System;

    using Immutable.Monads;

    public interface ITypePath
    { }

    public interface IGetterPath<in TFrom, out TTo>
    {
        Func<TFrom, TTo> Get { get; } 
    }

    public interface IWitherPath<TFrom, TTo>
    {
        IMaybe<Func<TFrom, TTo, TFrom>> Set { get; }

        TFrom UpdateOrDefault(TFrom style, Func<TTo, TTo> f);
    }

    public interface ITypePath<TFrom, TTo> : ITypePath, IGetterPath<TFrom, TTo>, IWitherPath<TFrom, TTo>
    {
        ITypePath<TFrom, T> Wrap<T>(Func<TTo, T> get, IMaybe<Func<TTo, T, TTo>> set);
    }
}
