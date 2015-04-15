namespace Inflection.Inflection
{
    using System;

    using Immutable;

    public interface IInflector
    {
        ITypeDescriptor<TDeclaring> Inflect<TDeclaring>();

        ITypeDescriptor Inflect(Type tDeclaring);
    }
}
