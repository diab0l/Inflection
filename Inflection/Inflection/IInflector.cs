namespace Inflection
{
    using System;

    using Immutable.TypeSystem;

    public interface IInflector
    {
        IImmutableType<TDeclaring> Inflect<TDeclaring>();

        IImmutableType Inflect(Type tDeclaring);
    }
}
