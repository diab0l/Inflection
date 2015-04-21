namespace Inflection.Sample
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;

    using Immutable;
    using Immutable.Graph;
    using Immutable.Monads;
    
    /* TODO: Write a benchmarking suite
     * Outline: Create a range of randomly nested data-structures with certain properties (lazy, memoizing, infinite, non-nullable)
     *          and see how
     *          - straight-forward reflection
     *          - the TypeGraph,
     *          - the ObjectGraph and
     *          - "handwritten" methods (generated typed-out code)
     *          perform memory and cpu wise on
     *          - querying the data-structures for certain elements in the hierarchy
     *          - querying the data-structures for all elements of a kind
     */
    public class Program
    {
        public static void Main(string[] args)
        {
            var mutableInflector = new ReflectingMutableTypeInflector();

            var graphRoot = TypeGraph.Create<Style>(mutableInflector);

            var style = new Style();

            InitializeMutable(graphRoot, style, () => new StyleInfo());
            InitializeMutable(graphRoot, style, () => new ToolbarStyle());
            InitializeMutable(graphRoot, style, () => new ButtonStyle());
            InitializeMutable(graphRoot, style, () => new ControlPart());
            InitializeMutable(graphRoot, style, () => new Texture());
            InitializeMutable(graphRoot, style, () => new TextStyle());
            InitializeMutable(graphRoot, style, () => (uint)0x1337BEEF);

            var obj = ObjectGraph.Create(new ReflectingMutableTypeInflector(), style);

            var graph = TypeGraph.Create<Style>(new ReflectingMutableTypeInflector());

            Test("Reflection          ", FetchColorsViaReflection, style);
            Test("Handwritten Methods ", FetchColorsViaMethods, style);
            //Test("TypeGraph           ", x => FetchColorsViaTg(x, graph), style);
            Test("ObjectGraph         ", x => FetchColorsViaOg(x, obj), style);

            //var cache = graph.GetDescendants<uint>().ToList();
            //Test("TypeGraph Cached    ", x => FetchColorsViaTgCached(x, cache), style);
        }

        private static void InitializeMutable<T>(TypeGraph<Style> graphRoot, Style style, Func<T> value)
        {
            graphRoot.GetDescendants<T>().Aggregate(Maybe.Return(style), (mx, y) => mx.Bind(x => y.Set.FMap(z => z(x, value()))));
        }

        private static void Test(string name, Action<Style> foo, Style style)
        {
            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < 10000; i++)
            {
                foo(style);
            }

            sw.Stop();

            Console.WriteLine("{0}: {1}s", name, sw.Elapsed.TotalSeconds);
        }

        private static void FetchColorsViaMethods(Style style)
        {
            var colors = GetAllColors(style).ToList();
        }

        private static void FetchColorsViaTg(Style style, TypeGraph<Style> typeGraph)
        {
            var colors = typeGraph.GetDescendants<uint>().Select(x => x.Get(style)).ToList();
        }

        private static void FetchColorsViaOg(Style style, ObjectGraph<Style> objGraph)
        {
            var colors = objGraph.GetDescendants<uint>().Select(x => x.Value).ToList();
        }

        private static void FetchColorsViaTgCached(Style style, List<ITypeDescendant<Style, uint>> descendants)
        {
            var colors = descendants.Select(x => x.Get(style)).ToList();
        }

        private static void FetchColorsViaReflection(Style style)
        {
            var colors = GetAllColorsViaReflection(style).ToList();
        }

        private static IEnumerable<uint> GetAllColorsViaReflection(Style style)
        {
            var stack = new Stack<object>();

            stack.Push(style);

            while (stack.Count > 0)
            {
                var p = stack.Pop();
                var t = p.GetType();

                foreach (var prop in t.GetProperties().Where(x => x.CanRead))
                {
                    var v = prop.GetValue(p);

                    if (v == null)
                    {
                        continue;
                    }

                    if (v is uint)
                    {
                        yield return (uint)v;
                    }

                    stack.Push(v);
                }
            }
        }

        private static IEnumerable<Tuple<uint, string>> GetAllColors(Style style, string p = "x => x")
        {
            if (style == null)
            {
                yield break;
            }

            foreach (var c in GetAllColors(style.Toolbar, p + ".Toolbar"))
            {
                yield return c;
            }

            foreach (var c in GetAllColors(style.Info, p + ".Info"))
            {
                yield return c;
            }
        }

        private static IEnumerable<Tuple<uint, string>> GetAllColors(ToolbarStyle toolbar, string p)
        {
            if (toolbar == null)
            {
                yield break;
            }

            foreach (var c in GetAllColors(toolbar.Button, p + ".Button"))
            {
                yield return c;
            }

            foreach (var c in GetAllColors(toolbar.Clock, p + ".Clock"))
            {
                yield return c;
            }

            foreach (var c in GetAllColors(toolbar.Label, p + ".Label"))
            {
                yield return c;
            }

            foreach (var c in GetAllColors(toolbar.WindowLabel, p + ".WindowLabel"))
            {
                yield return c;
            }
        }

        private static IEnumerable<Tuple<uint, string>> GetAllColors(StyleInfo info, string p)
        {
            if (info == null)
            {
                yield break;
            }

            foreach (var c in GetAllColors(info.Author, p))
            {
                yield return c;
            }

            foreach (var c in GetAllColors(info.Comments, p))
            {
                yield return c;
            }

            foreach (var c in GetAllColors(info.Date, p))
            {
                yield return c;
            }

            foreach (var c in GetAllColors(info.Name, p))
            {
                yield return c;
            }
        }

        private static IEnumerable<Tuple<uint, string>> GetAllColors(string s, string p)
        {
            if (s == null)
            {
                yield break;
            }
        }

        private static IEnumerable<Tuple<uint, string>> GetAllColors(ButtonStyle button, string p)
        {
            if (button == null)
            {
                yield break;
            }

            foreach (var c in GetAllColors((ControlPart)button, p))
            {
                yield return c;
            }

            foreach (var c in GetAllColors(button.Pressed, p + ".Pressed"))
            {
                yield return c;
            }
        }

        private static IEnumerable<Tuple<uint, string>> GetAllColors(ControlPart control, string p)
        {
            if (control == null)
            {
                yield break;
            }

            foreach (var c in GetAllColors(control.Text, p + ".Text"))
            {
                yield return c;
            }

            foreach (var c in GetAllColors(control.Background, p + ".Background"))
            {
                yield return c;
            }
        }

        private static IEnumerable<Tuple<uint, string>> GetAllColors(TextStyle text, string p)
        {
            if (text == null)
            {
                yield break;
            }

            yield return Tuple.Create(text.Color, p + ".Color");
        }

        private static IEnumerable<Tuple<uint, string>> GetAllColors(Texture texture, string p)
        {
            if (texture == null)
            {
                yield break;
            }

            yield return Tuple.Create(texture.Color1, p + ".Color1");
            yield return Tuple.Create(texture.Color2, p + ".Color2");
            yield return Tuple.Create(texture.Color3, p + ".Color3");
            yield return Tuple.Create(texture.Color4, p + ".Color4");
            yield return Tuple.Create(texture.Color5, p + ".Color5");
        }
    }

    public class Style
    {
        public StyleInfo Info { get; set; }

        public ToolbarStyle Toolbar { get; set; }
    }

    public class ToolbarStyle
    {
        public ControlPart Label { get; set; }

        public ControlPart WindowLabel { get; set; }

        public ControlPart Clock { get; set; }

        public ButtonStyle Button { get; set; }
    }

    public class ButtonStyle : ControlPart
    {
        public ControlPart Pressed { get; set; }
    }

    public class ControlPart
    {
        public Texture Background { get; set; }

        public TextStyle Text { get; set; }
    }

    public class TextStyle
    {
        public string Font { get; set; }

        public uint Color { get; set; }
    }

    public class Texture
    {
        public uint Color1 { get; set; }

        public uint Color2 { get; set; }

        public uint Color3 { get; set; }

        public uint Color4 { get; set; }

        public uint Color5 { get; set; }
    }

    public class StyleInfo
    {
        public string Author { get; set; }

        public string Comments { get; set; }

        public string Date { get; set; }

        public string Name { get; set; }
    }
}