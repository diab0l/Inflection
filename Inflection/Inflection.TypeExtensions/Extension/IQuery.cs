namespace Inflection.TypeExtensions.Extension
{
    using System;

    public interface IQuery<in TRoot, out TOut, out TId> : IQuery<TId>
    {
        Func<TRoot, TOut> Get { get; }
    }

    public interface IQuery<out TId> : IQueryTransform<TId>
    { }
}