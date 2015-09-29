namespace Inflection.Sample
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    using Immutable.Monads;
    using Immutable.TypeSystem;

    using TypeExtensions.Extension;

    using TypeGraph.Graph;
    using TypeGraph.Graph.Strategies;

    using TypeNode.TypeNode;

    /* TODO: Write a benchmarking suite
     * Outline: Create a range of randomly nested data-structures with certain properties (lazy, memoizing, infinite, non-nullable)
     *          and see how
     *          - straight-forward reflection
     *          - ImmutableTypeExtensions
     *          - "handwritten" methods (generated typed-out code)
     *          perform memory and cpu wise on
     *          - querying the data-structures for certain elements in the hierarchy
     *          - querying the data-structures for all elements of a kind
     *          - transforming the data-structures for all elements of a kind
     */
    public class Program
    {
        public static void Main(string[] args)
        {
            var mutableInflector = new ReflectingMutableTypeInflector();

            var t = mutableInflector.Inflect<Style>();

            var graphRoot = TypeGraph.Create<Style>(mutableInflector);

            var style = new Style();

            InitializeMutable(graphRoot, style, () => new StyleInfo());
            InitializeMutable(graphRoot, style, () => new ToolbarStyle());
            InitializeMutable(graphRoot, style, () => new ButtonStyle());
            InitializeMutable(graphRoot, style, () => new ControlPart());
            InitializeMutable(graphRoot, style, () => new Texture());
            InitializeMutable(graphRoot, style, () => new TextStyle());

            var s = (uint)0x1337BEEF;
            InitializeMutable(graphRoot, style, () => s++);

            var tg = TypeGraph.Create<Style>(new ReflectingMutableTypeInflector());
            var tn = TypeNode.Create<Style>(new ReflectingMutableTypeInflector());

            var _c = tg.GetDescendants<uint>().First(x => x.Get(style) > 5).Update(style, x => x + 1);
            var _d = tn.GetPaths<uint>().First(x => x.Get(style) > 5).UpdateOrDefault(style, x => x + 1);

            Console.WriteLine("Select");
            var tHw = Test("Handwritten ", FetchColorsViaMethods, style);
            var tRe = Test("Reflection  ", FetchColorsViaReflection, style);
            var tTg = Test("TypeGraph   ", x => FetchColorsViaTg(x, tg), style);

            var cache = DfsStrategy.Create<Style>().WithCache();
            //var tTgC = Test("TypeGraph Cached  ", x => FetchColorsViaTgCached(x, tg, cache), style);
            var tTn = Test("TypeNode    ", x => FetchColorsViaTn(x, tn), style);
            var pExt = Test("Extensions  ", x => FetchColorsViaPathExtension(x, mutableInflector), style);

            Console.WriteLine();
            Console.WriteLine("Reflection / Handwritten : {0:F2}%", tRe / tHw * 100);
            Console.WriteLine("TypeGraph  / Handwritten : {0:F2}%", tTg / tHw * 100);
            //Console.WriteLine("TypeGraph Cached / Handwritten : {0:F2}%", tTgC / tHw * 100);
            Console.WriteLine("TypeNode   / Handwritten : {0:F2}%", tTn / tHw * 100);
            Console.WriteLine("Extensions / Handwritten : {0:F2}%", pExt / tHw * 100);

            Console.WriteLine();
            Console.WriteLine("GMap");
            
            var tgHw = Test("Handwritten ", x => GMap(x, (uint a) => a), style);
            var tgRf = Test("Reflection  ", x => GMapViaReflection(x, (uint a) => a), style);
            //var tgTg = Test("TypeGraph   ", x => tg.GMap<TypeGraph<Style>, uint, uint>(x, (uint a) => a), style);
            var tgTn = Test("TypeNode    ", x => tn.GMap(x, (uint a) => a), style);
            var tgXt = Test("Extension   ", x => x.GMap((uint a) => a, mutableInflector), style);
            Console.WriteLine();

            Console.WriteLine("Reflection / Handwritten : {0:F2}%", tgRf / tgHw * 100);
            //Console.WriteLine("TypeGraph  / Handwritten : {0:F2}%", tgTg / tgHw * 100);
            Console.WriteLine("TypeNode   / Handwritten : {0:F2}%", tgTn / tgHw * 100);
            Console.WriteLine("Extension  / Handwritten : {0:F2}%", tgXt / tgHw * 100);

            Console.WriteLine();
            //Console.WriteLine("GFold");

            //var tfHw = Test("Handwritten ", x => GFold(x, 0, (int cnt, uint col) => cnt + 1), style);
            //var tfRf = Test("Reflection  ", x => GFoldViaReflection(x, 0, (int cnt, uint col) => cnt + 1), style);
            //Console.WriteLine();

            //Console.WriteLine("Reflection / Handwritten : {0:F2}%", tfRf / tfHw * 100);

            Counters.Increment("");

            Console.ReadLine();
        }

        private static void InitializeMutable<T>(TypeGraph<Style> graphRoot, Style style, Func<T> value)
        {
            graphRoot.GetDescendants<T>().Aggregate(Maybe.Return(style), (mx, y) => mx.Bind(x => y.Set.FMap(z => z(x, value()))));
        }

        private static double Test(string name, Action<Style> foo, Style style)
        {
            var sw = new Stopwatch();
            sw.Start();

            for (var i = 0; i < 10000; i++)
            {
                foo(style);
            }

            sw.Stop();

            Console.WriteLine("{0}: {1}s", name, sw.Elapsed.TotalSeconds);

            return sw.Elapsed.TotalSeconds;
        }

        private static void FetchColorsViaMethods(Style style)
        {
            var colors = GSelect<uint>(style).ToList();
        }

        private static void FetchColorsViaTg(Style style, TypeGraph<Style> typeGraph)
        {
            var colors = new List<uint>();
            foreach (var d in typeGraph.GetDescendants<uint>(style))
            {
                colors.Add(d.Get(style));
            }
        }

        private static void FetchColorsViaTgCached(Style style, TypeGraph<Style> typeGraph, IDescendingStrategy<Style> strategy)
        {
            strategy = new NotNullAdapter<Style>(style, strategy);

            var colors = new List<uint>();
            foreach (var d in typeGraph.GetDescendants<uint>(strategy))
            {
                colors.Add(d.Get(style));
            }
        }

        private static void FetchColorsViaReflection(Style style)
        {
            var colors = GSelectViaReflection<uint>(style).ToList();
        }

        private static void FetchColorsViaTn(Style style, TypeNode<Style, Style> tn)
        {
            var colors = new List<uint>();
            foreach (var p in tn.GetValuePaths<uint>(style))
            {
                colors.Add(p.Value);
            }
        }

        private static void FetchColorsViaPathExtension(Style style, IInflector inflector)
        {
            var colors = new List<Tuple<uint, string>>();
            foreach (var v in style.GetValues<Style, uint>(inflector))
            {
                colors.Add(v);
            }
        }

        private static Dictionary<Type, PropertyInfo[]> propertyCache = new Dictionary<Type, PropertyInfo[]>();

        private static void CmpReflectionTm(Style style, IInflector inflector)
        {
            var n = 100000;

            var stRef = new Stack<Tuple<object>>();
            var stIt = new Stack<Tuple<object, IImmutableType>>();

            var sw = new Stopwatch();

            sw.Start();
            for (int i = 0; i < n; i++)
            {
                stRef.Clear();
                stRef.Push(new Tuple<object>(style));
            }
            sw.Stop();
            var tRef = sw.Elapsed.TotalSeconds;

            sw.Restart();
            for (int i = 0; i < n; i++)
            {
                stIt.Clear();
                stIt.Push(new Tuple<object, IImmutableType>(style, inflector.Inflect<Style>()));
            }
            sw.Stop();
            var tIt = sw.Elapsed.TotalSeconds;

            sw.Restart();
            for (int i = 0; i < n; i++)
            {
                var p = stRef.Peek();
                var t = p.GetType();
            }
            sw.Stop();
            tRef = sw.Elapsed.TotalSeconds;

            sw.Restart();
            for (int i = 0; i < n; i++)
            {
                var p = stIt.Peek();
                var t = p.Item2;
            }
            sw.Stop();
            tIt = sw.Elapsed.TotalSeconds;

            {
                var p = stRef.Peek();
                var t = p.Item1.GetType();
                sw.Restart();
                for (int i = 0; i < n; i++)
                {
                    var props = t.GetProperties().Where(x => x.CanRead).Where(x => x.GetIndexParameters().Length == 0).ToList();
                }
                sw.Stop();
            }
            tRef = sw.Elapsed.TotalSeconds;
            List<IImmutableProperty> properties = null;

            {
                var p = stIt.Peek();
                var t = p.Item2;
                sw.Restart();
                for (int i = 0; i < n; i++)
                {
                    var props = properties ?? (properties = t.GetProperties().ToList());
                }
                sw.Stop();
            }
            tIt = sw.Elapsed.TotalSeconds;

            {
                var p = stRef.Peek();
                var t = p.Item1.GetType();
                var props = t.GetProperties().Where(x => x.CanRead).Where(x => x.GetIndexParameters().Length == 0).ToList();
                sw.Restart();
                for (int i = 0; i < n; i++)
                {
                    var values = props.Select(x => x.GetValue(style)).ToList();
                }
                sw.Stop();
            }
            tRef = sw.Elapsed.TotalSeconds;

            {
                var p = stIt.Peek();
                var t = p.Item2;
                var props = properties ?? (properties = t.GetProperties().ToList());
                sw.Restart();
                for (int i = 0; i < n; i++)
                {
                    var values = props.Select(x => x.GetValue(style)).ToList();
                }
                sw.Stop();
            }
            tIt = sw.Elapsed.TotalSeconds;
        }

        private static IEnumerable<Tuple<T, string>> GSelectViaReflection<T>(Style style)
        {
            var stack = new Stack<Tuple<object, string>>();

            stack.Push(Tuple.Create((object)style, "x => x"));

            while (stack.Count > 0)
            {
                var p = stack.Pop();
                var t = p.Item1.GetType();

                PropertyInfo[] props;
                if (!propertyCache.TryGetValue(t, out props))
                {
                    props = t.GetProperties().Where(x => x.CanRead && x.CanWrite).Where(x => x.GetIndexParameters().Length == 0).ToArray();
                    propertyCache[t] = props;
                }
                
                foreach (var prop in props)
                {
                    var v = prop.GetValue(p.Item1);

                    if (v == null)
                    {
                        continue;
                    }

                    var tpl = Tuple.Create(v, p.Item2 + "." + prop.Name);

                    if (v is T)
                    {
                        yield return Tuple.Create((T)tpl.Item1, tpl.Item2);
                    }

                    stack.Push(tpl);
                }
            }
        }

        private static T GMapViaReflection<T, TA, TB>(T v, Func<TA, TB> f)
        {
            return (T)GMapViaReflection(v, f, typeof(T));
        }

        private static object GMapViaReflection<TA, TB>(object v, Func<TA, TB> f, Type t)
        {
            var seed = (v is TA && t.IsAssignableFrom(typeof(TB)))
                ? f((TA)v)
                : v;

            if (seed == null)
            {
                return null;
            }

            var agg = seed;
            PropertyInfo[] props;
            if (!propertyCache.TryGetValue(t, out props))
            {
                props = t.GetProperties().Where(x => x.CanRead && x.CanWrite).Where(x => x.GetIndexParameters().Length == 0).ToArray();
                propertyCache[t] = props;
            }

            foreach (var p in props)
            {
                var pv = p.GetValue(agg);
                var gv = GMapViaReflection(pv, f, p.PropertyType);

                p.SetValue(agg, gv);
            }

            return agg;
        }

        private static TAggregate GFoldViaReflection<TAggregate, T, TA>(T root, TAggregate seed, Func<TAggregate, TA, TAggregate> f)
        {
            return GFoldViaReflection(root, typeof(T), seed, f);
        }

        private static TAggregate GFoldViaReflection<TAggregate, TA>(object node, Type tNode, TAggregate seed, Func<TAggregate, TA, TAggregate> f)
        {
            if (node is TA)
            {
                seed = f(seed, (TA)node);
            }

            PropertyInfo[] props;
            if (!propertyCache.TryGetValue(tNode, out props))
            {
                props = tNode.GetProperties().Where(x => x.CanRead && x.CanWrite).Where(x => x.GetIndexParameters().Length == 0).ToArray();
                propertyCache[tNode] = props;
            }


            foreach (var p in props)
            {
                var pv = p.GetValue(node);

                seed = GFoldViaReflection(pv, p.PropertyType, seed, f);
            }

            return seed;
        }

        private static IEnumerable<Tuple<T, string>> GSelect2<T>(Style style, string p = "Style")
        {
            return GFold<IEnumerable<Tuple<T, string>>, T>(style, Enumerable.Empty<Tuple<T, string>>(), (xs, x) => xs.Concat(new[] { Tuple.Create(x, "Folded") }));
        }

        private static IEnumerable<Tuple<T, string>> GSelect<T>(Style style, string p = "Style")
        {
            if (style == null)
            {
                yield break;
            }

            if (style is T)
            {
                yield return Tuple.Create((T)(object)style, p);
            }

            foreach (var c in GSelect<T>(style.Toolbar, p + ".Toolbar"))
            {
                yield return c;
            }

            foreach (var c in GSelect<T>(style.Info, p + ".Info"))
            {
                yield return c;
            }
        }

        private static IEnumerable<Tuple<T, string>> GSelect<T>(ToolbarStyle toolbar, string p = "ToolbarStyle")
        {
            if (toolbar == null)
            {
                yield break;
            }

            if (toolbar is T)
            {
                yield return Tuple.Create((T)(object)toolbar, p);
            }

            foreach (var c in GSelect<T>(toolbar.Button, p + ".Button"))
            {
                yield return c;
            }

            foreach (var c in GSelect<T>(toolbar.Clock, p + ".Clock"))
            {
                yield return c;
            }

            foreach (var c in GSelect<T>(toolbar.Label, p + ".Label"))
            {
                yield return c;
            }

            foreach (var c in GSelect<T>(toolbar.WindowLabel, p + ".WindowLabel"))
            {
                yield return c;
            }
        }

        private static IEnumerable<Tuple<T, string>> GSelect<T>(StyleInfo info, string p = "StyleInfo")
        {
            if (info == null)
            {
                yield break;
            }

            if (info is T)
            {
                yield return Tuple.Create((T)(object)info, p);
            }


            foreach (var c in GSelect<T>(info.Author, p))
            {
                yield return c;
            }

            foreach (var c in GSelect<T>(info.Comments, p))
            {
                yield return c;
            }

            foreach (var c in GSelect<T>(info.Date, p))
            {
                yield return c;
            }

            foreach (var c in GSelect<T>(info.Name, p))
            {
                yield return c;
            }
        }

        private static IEnumerable<Tuple<T, string>> GSelect<T>(string s, string p = "String")
        {
            if (s == null)
            {
                yield break;
            }

            if (s is T)
            {
                yield return Tuple.Create((T)(object)s, p);
            }
        }

        private static IEnumerable<Tuple<T, string>> GSelect<T>(ButtonStyle button, string p = "ButtonStyle")
        {
            if (button == null)
            {
                yield break;
            }

            if (button is T)
            {
                yield return Tuple.Create((T)(object)button, p);
            }

            foreach (var c in GSelect<T>((ControlPart)button, p))
            {
                yield return c;
            }

            foreach (var c in GSelect<T>(button.Pressed, p + ".Pressed"))
            {
                yield return c;
            }
        }

        private static IEnumerable<Tuple<T, string>> GSelect<T>(ControlPart control, string p = "ControlPart")
        {
            if (control == null)
            {
                yield break;
            }

            if (control is T)
            {
                yield return Tuple.Create((T)(object)control, p);
            }

            foreach (var c in GSelect<T>(control.Text, p + ".Text"))
            {
                yield return c;
            }

            foreach (var c in GSelect<T>(control.Background, p + ".Background"))
            {
                yield return c;
            }
        }

        private static IEnumerable<Tuple<T, string>> GSelect<T>(TextStyle text, string p = "TextStyle")
        {
            if (text == null)
            {
                yield break;
            }

            if (text is T)
            {
                yield return Tuple.Create((T)(object)text, p);
            }

            foreach (var c in GSelect<T>(text.Font, p + ".Font"))
            {
                yield return c;
            }

            foreach (var c in GSelect<T>(text.Color, p + ".Color"))
            {
                yield return c;
            }
        }

        private static IEnumerable<Tuple<T, string>> GSelect<T>(Texture texture, string p = "Texture")
        {
            if (texture == null)
            {
                yield break;
            }

            if (texture is T)
            {
                yield return Tuple.Create((T)(object)texture, p);
            }

            foreach (var x in GSelect<T>(texture.Color1, p + ".Color1"))
            {
                yield return x;
            }

            foreach (var x in GSelect<T>(texture.Color2, p + ".Color2"))
            {
                yield return x;
            }

            foreach (var x in GSelect<T>(texture.Color3, p + ".Color3"))
            {
                yield return x;
            }

            foreach (var x in GSelect<T>(texture.Color4, p + ".Color4"))
            {
                yield return x;
            }

            foreach (var x in GSelect<T>(texture.Color5, p + ".Color5"))
            {
                yield return x;
            }
        }

        private static IEnumerable<Tuple<T, string>> GSelect<T>(uint num, string p)
        {
            if (num is T)
            {
                yield return Tuple.Create((T)(object)num, p);
            }
        }
        
        private static T MapIfCompatible<T, TA, TB>(T v, Func<TA, TB> f)
        {
            return (v is TA && typeof(T).IsAssignableFrom(typeof(TB)))
                ? (T)(object)f((TA)(object)v)
                : v;
        }

        private static Style GMap<TA, TB>(Style style, Func<TA, TB> f)
        {
            style = MapIfCompatible(style, f);

            if (style == null)
            {
                return null;
            }

            style.Info = GMap(style.Info, f);
            style.Toolbar = GMap(style.Toolbar, f);

            return style;
        }

        private static ToolbarStyle GMap<TA, TB>(ToolbarStyle toolbar, Func<TA, TB> f)
        {
            toolbar = MapIfCompatible(toolbar, f);

            if (toolbar == null)
            {
                return null;
            }

            toolbar.Button = GMap(toolbar.Button, f);
            toolbar.Clock = GMap(toolbar.Clock, f);
            toolbar.Label = GMap(toolbar.Label, f);
            toolbar.WindowLabel = GMap(toolbar.WindowLabel, f);

            return toolbar;
        }

        private static StyleInfo GMap<TA, TB>(StyleInfo info, Func<TA, TB> f)
        {
            info = MapIfCompatible(info, f);

            if (info == null)
            {
                return null;
            }

            info.Author = GMap(info.Author, f);
            info.Date = GMap(info.Date, f);
            info.Comments = GMap(info.Comments, f);
            info.Name = GMap(info.Name, f);

            return info;
        }

        private static string GMap<TA, TB>(string str, Func<TA, TB> f)
        {
            return MapIfCompatible(str, f);
        }

        private static ButtonStyle GMap<TA, TB>(ButtonStyle button, Func<TA, TB> f)
        {
            button = MapIfCompatible(button, f);

            if (button == null)
            {
                return null;
            }

            button.Background = GMap(button.Background, f);
            button.Text = GMap(button.Text, f);
            button.Pressed = GMap(button.Pressed, f);

            return button;
        }

        private static ControlPart GMap<TA, TB>(ControlPart part, Func<TA, TB> f)
        {
            part = MapIfCompatible(part, f);

            if (part == null)
            {
                return null;
            }

            part.Background = GMap(part.Background, f);
            part.Text = GMap(part.Text, f);

            return part;
        }

        private static TextStyle GMap<TA, TB>(TextStyle text, Func<TA, TB> f)
        {
            text = MapIfCompatible(text, f);

            if (text == null)
            {
                return null;
            }

            text.Font = GMap(text.Font, f);
            text.Color = GMap(text.Color, f);

            return text;
        }

        private static Texture GMap<TA, TB>(Texture tex, Func<TA, TB> f)
        {
            tex = MapIfCompatible(tex, f);

            if (tex == null)
            {
                return null;
            }

            tex.Color1 = GMap(tex.Color1, f);
            tex.Color2 = GMap(tex.Color2, f);
            tex.Color3 = GMap(tex.Color3, f);
            tex.Color4 = GMap(tex.Color4, f);
            tex.Color5 = GMap(tex.Color5, f);

            return tex;
        }

        private static uint GMap<TA, TB>(uint color, Func<TA, TB> f)
        {
            return MapIfCompatible(color, f);
        }

        private static TAggregate GFold<TAggregate, TA>(Style style, TAggregate seed, Func<TAggregate, TA, TAggregate> f)
        {
            if (style is TA)
            {
                seed = f(seed, (TA)(object)style);
            }

            seed = GFold(style.Info, seed, f);
            seed = GFold(style.Toolbar, seed, f);

            return seed;
        }

        private static TAggregate GFold<TAggregate, TA>(ToolbarStyle toolbar, TAggregate seed, Func<TAggregate, TA, TAggregate> f)
        {
            if (toolbar is TA)
            {
                seed = f(seed, (TA)(object)toolbar);
            }

            seed = GFold(toolbar.Button, seed, f);
            seed = GFold(toolbar.Clock, seed, f);
            seed = GFold(toolbar.Label, seed, f);
            seed = GFold(toolbar.WindowLabel, seed, f);

            return seed;
        }

        private static TAggregate GFold<TAggregate, TA>(StyleInfo info, TAggregate seed, Func<TAggregate, TA, TAggregate> f)
        {
            if (info is TA)
            {
                seed = f(seed, (TA)(object)info);
            }

            seed = GFold(info.Author, seed, f);
            seed = GFold(info.Date, seed, f);
            seed = GFold(info.Comments, seed, f);
            seed = GFold(info.Name, seed, f);

            return seed;
        }

        private static TAggregate GFold<TAggregate, TA>(string str, TAggregate seed, Func<TAggregate, TA, TAggregate> f)
        {
            if (str is TA)
            {
                seed = f(seed, (TA)(object)str);
            }

            return seed;
        }

        private static TAggregate GFold<TAggregate, TA>(ButtonStyle button, TAggregate seed, Func<TAggregate, TA, TAggregate> f)
        {
            if (button is TA)
            {
                seed = f(seed, (TA)(object)button);
            }

            seed = GFold(button.Background, seed, f);
            seed = GFold(button.Text, seed, f);
            seed = GFold(button.Pressed, seed, f);

            return seed;
        }

        private static TAggregate GFold<TAggregate, TA>(ControlPart part, TAggregate seed, Func<TAggregate, TA, TAggregate> f)
        {
            if (part is TA)
            {
                seed = f(seed, (TA)(object)part);
            }

            seed = GFold(part.Background, seed, f);
            seed = GFold(part.Text, seed, f);

            return seed;
        }

        private static TAggregate GFold<TAggregate, TA>(TextStyle text, TAggregate seed, Func<TAggregate, TA, TAggregate> f)
        {
            if (text is TA)
            {
                seed = f(seed, (TA)(object)text);
            }

            seed = GFold(text.Font, seed, f);
            seed = GFold(text.Color, seed, f);

            return seed;
        }

        private static TAggregate GFold<TAggregate, TA>(Texture tex, TAggregate seed, Func<TAggregate, TA, TAggregate> f)
        {
            if (tex is TA)
            {
                seed = f(seed, (TA)(object)tex);
            }

            seed = GFold(tex.Color1, seed, f);
            seed = GFold(tex.Color2, seed, f);
            seed = GFold(tex.Color3, seed, f);
            seed = GFold(tex.Color4, seed, f);
            seed = GFold(tex.Color5, seed, f);

            return seed;
        }

        private static TAggregate GFold<TAggregate, TA>(uint color, TAggregate seed, Func<TAggregate, TA, TAggregate> f)
        {
            if (color is TA)
            {
                seed = f(seed, (TA)(object)color);
            }

            return seed;
        }
    }

    public class Style
    {
        public Style() { }

        public Style(Style copyFrom, StyleInfo info = null, ToolbarStyle toolbar = null)
        {
            this.Info = info ?? copyFrom.Info;
            this.Toolbar = toolbar ?? copyFrom.Toolbar;
        }

        public StyleInfo Info { get; set; }

        public ToolbarStyle Toolbar { get; set; }
    }

    public class ToolbarStyle
    {
        public ToolbarStyle() { }

        public ToolbarStyle(ToolbarStyle copyFrom, ControlPart label = null, ControlPart windowLabel = null, ControlPart clock = null, ButtonStyle button = null)
        {
            this.Label = label ?? copyFrom.Label;
            this.WindowLabel = windowLabel ?? copyFrom.WindowLabel;
            this.Clock = clock ?? copyFrom.Clock;
            this.Button = button ?? copyFrom.Button;
        }

        public ControlPart Label { get; set; }

        public ControlPart WindowLabel { get; set; }

        public ControlPart Clock { get; set; }

        public ButtonStyle Button { get; set; }

    }

    public class ControlPart
    {
        public ControlPart() { }

        public ControlPart(ControlPart copyFrom, Texture background = null, TextStyle text = null)
        {
            this.Background = background ?? copyFrom.Background;
            this.Text = text ?? copyFrom.Text;
        }

        public Texture Background { get; set; }

        public TextStyle Text { get; set; }
    }

    public class ButtonStyle : ControlPart
    {
        public ButtonStyle() { }

        public ButtonStyle(ButtonStyle copyFrom, Texture background = null, TextStyle text = null, ControlPart pressed = null)
        {
            this.Background = background ?? copyFrom.Background;
            this.Text = text ?? copyFrom.Text;
            this.Pressed = pressed ?? copyFrom.Pressed;
        }

        public ControlPart Pressed { get; set; }
    }

    public class TextStyle
    {
        public TextStyle() { }

        public TextStyle(TextStyle copyFrom, string font = null, uint? color = null)
        {
            this.Font = font ?? copyFrom.Font;
            this.Color = color ?? copyFrom.Color;
        }

        public string Font { get; set; }

        public uint Color { get; set; }
    }

    public class Texture
    {
        public Texture() { }

        public Texture(Texture copyFrom, uint? color1 = null, uint? color2 = null, uint? color3 = null, uint? color4 = null, uint? color5 = null)
        {
            this.Color1 = color1 ?? copyFrom.Color1;
            this.Color2 = color2 ?? copyFrom.Color2;
            this.Color3 = color3 ?? copyFrom.Color3;
            this.Color4 = color4 ?? copyFrom.Color4;
            this.Color5 = color5 ?? copyFrom.Color5;
        }

        public uint Color1 { get; set; }

        public uint Color2 { get; set; }

        public uint Color3 { get; set; }

        public uint Color4 { get; set; }

        public uint Color5 { get; set; }
    }

    public class StyleInfo
    {
        public StyleInfo() { }

        public StyleInfo(StyleInfo copyFrom, string author = null, string comments = null, string date = null, string name = null)
        {
            this.Author = author ?? copyFrom.Author;
            this.Comments = comments ?? copyFrom.Comments;
            this.Date = date ?? copyFrom.Date;
            this.Name = name ?? copyFrom.Name;
        }

        public string Author { get; set; }

        public string Comments { get; set; }

        public string Date { get; set; }

        public string Name { get; set; }
    }
}
