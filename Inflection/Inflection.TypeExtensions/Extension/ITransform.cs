namespace Inflection.TypeExtensions.Extension
{
    using System;

    public interface ITransform<TRoot, in TIn, out TId> : ITransform<TId>
    {
        Func<TRoot, TIn, TRoot> With { get; } 
    }

    public interface ITransform<out TId> : IQueryTransform<TId>
    { }
}