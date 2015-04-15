namespace Inflection
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Graph;
    using Graph.Nodes;

    using Inflection;

    public class Program
    {
        public static void Main(string[] args)
        {
            var mutableInflector = new ReflectingMutableTypeInflector();

            var graphRoot = ObjectGraph.Create<Style>(mutableInflector);

            var style = new Style();

            graphRoot.GetDescendants<StyleInfo>().Aggregate(style, (x, y) => y.Set(x, new StyleInfo()));
            graphRoot.GetDescendants<ToolbarStyle>().Aggregate(style, (x, y) => y.Set(x, new ToolbarStyle()));
            graphRoot.GetDescendants<ButtonStyle>().Aggregate(style, (x, y) => y.Set(x, new ButtonStyle()));
            graphRoot.GetDescendants<ControlPart>().Aggregate(style, (x, y) => y.Set(x, new ControlPart()));
            graphRoot.GetDescendants<Texture>().Aggregate(style, (x, y) => y.Set(x, new Texture()));
            graphRoot.GetDescendants<TextStyle>().Aggregate(style, (x, y) => y.Set(x, new TextStyle()));
            graphRoot.GetDescendants<uint>().Aggregate(style, (x, y) => y.Set(x, 0x1337BEEF));

            var graph = ObjectGraph.Create<Style>(new ReflectingMutableTypeInflector());

            Test("Reflection        ", FetchColorsViaReflection, style);
            Test("Methods 1         ", FetchColorsViaMethods, style);
            Test("ObjectGraph 1     ", x => FetchColorsViaOg(x, graph), style);
            Test("Methods 2         ", FetchColorsViaMethods, style);
            Test("ObjectGraph 2     ", x => FetchColorsViaOg(x, graph), style);

            var cache = graph.GetDescendants<uint>().ToList();
            Test("ObjectGraph Cached", x => FetchColorsViaOgCached(x, cache), style);
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

        private static void FetchColorsViaOg(Style style, ObjectGraph<Style> graph)
        {
            var colors = graph.GetDescendants<uint>().Select(x => x.Get(style)).ToList();
        }

        private static void FetchColorsViaOgCached(Style style, List<IDescendant<Style, uint>> descendants)
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