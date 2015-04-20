namespace Inflection
{
    using System;

    using global::Inflection.Immutable;

    using Immutable.TypeSystem;

    public interface IInflector
    {
        IImmutableType<TDeclaring> Inflect<TDeclaring>();

        IImmutableType Inflect(Type tDeclaring);
    }
}
