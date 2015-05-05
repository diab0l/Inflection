namespace Inflection.Immutable.Graph.Strategies
{
    public static class DefaultStrategy
    {
        public static DefaultStrategy<TRoot> Create<TRoot>(TRoot root)
        {
            return new DefaultStrategy<TRoot>(root);
        } 
    }

    public class DefaultStrategy<TRoot> : NotNullAdapter<TRoot>
    {
        public DefaultStrategy(TRoot root) : base(root, new DfsStrategy<TRoot>())
        { }
    }
}