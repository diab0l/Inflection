namespace Inflection.Immutable.Graph
{
    public interface IObjectGraph<TRoot>
        : IObjectDescendant<TRoot, TRoot>
    {
        new ITypeGraph<TRoot> Open();
    }
}
