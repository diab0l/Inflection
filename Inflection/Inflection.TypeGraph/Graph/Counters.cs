namespace Inflection.TypeGraph.Graph
{
    using System.Collections.Generic;

    using Immutable.Extensions;

    public static class Counters
    {
        private static readonly Dictionary<string, int> Foo = new Dictionary<string, int>();

        public static void Increment(string name)
        {
            Foo.AddOrUpdate(name, 1, x => x + 1);
        }
    }
}