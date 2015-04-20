namespace Inflection.Immutable.Graph
{
    public interface IObjectGraph<TRoot> 
        : IObjectDescendant<TRoot, TRoot>
    { }
}
