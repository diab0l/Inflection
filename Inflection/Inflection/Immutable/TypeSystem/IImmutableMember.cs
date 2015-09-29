namespace Inflection.Immutable.TypeSystem
{
    using System.Reflection;

    public interface IImmutableMember
    {
        MemberInfo ClrMember { get; }

        IImmutableType DeclaringType { get; }
    }

    public interface IImmutableMember<TDeclaring> : IImmutableMember
    {
        new IImmutableType<TDeclaring> DeclaringType { get; }
    }
}
