namespace Inflection.TypeNode.TypeNode
{
    using System;

    using Immutable.Monads;

    public static class TypePath
    {
        public static TypePath<TFrom, TTo> Create<TFrom, TTo>(Func<TFrom, TTo> get, Func<TFrom, TTo, TFrom> set)
        {
            return new TypePath<TFrom, TTo>(get, Maybe.Return(set));
        }

        public static TypePath<TFrom, TTo> Create<TFrom, TTo>(Func<TFrom, TTo> get, IMaybe<Func<TFrom, TTo, TFrom>> set)
        {
            return new TypePath<TFrom, TTo>(get, set);
        }
    }

    public class TypePath<TFrom, TTo> : ITypePath<TFrom, TTo>
    {
        private readonly Func<TFrom, TTo> get;
        private readonly IMaybe<Func<TFrom, TTo, TFrom>> set;

        public TypePath(Func<TFrom, TTo> get, IMaybe<Func<TFrom, TTo, TFrom>> set)
        {
            this.get = get;
            this.set = set;
        }

        public Func<TFrom, TTo> Get
        {
            get { return this.get; }
        }

        public IMaybe<Func<TFrom, TTo, TFrom>> Set
        {
            get { return this.set; }
        }

        public TFrom UpdateOrDefault(TFrom style, Func<TTo, TTo> f) {
            var set = this.Set.GetValueOrDefault();
            if (set == null) {
                return style;
            }

            return set(style, f(this.Get(style)));
        }

        public ITypePath<TFrom, T> Wrap<T>(Func<TTo, T> vGet, IMaybe<Func<TTo, T, TTo>> mvSet)
        {
            var wGet = new Func<TFrom, T>(x => vGet(this.get(x)));
            var wmSet = this.set.Bind(s => mvSet.FMap(vs => new Func<TFrom, T, TFrom>((n, vn) => s(n, vs(this.get(n), vn)))));

            return new TypePath<TFrom, T>(wGet, wmSet);
        }
    }

    public static class ValuePath
    {
        public static ValuePath<TFrom, TTo> Create<TFrom, TTo>(Func<TFrom, TTo> get, Func<TFrom, TTo, TFrom> set, TTo value)
        {
            return new ValuePath<TFrom, TTo>(get, Maybe.Return(set), value);
        }

        public static ValuePath<TFrom, TTo> Create<TFrom, TTo>(Func<TFrom, TTo> get, IMaybe<Func<TFrom, TTo, TFrom>> set, TTo value)
        {
            return new ValuePath<TFrom, TTo>(get, set, value);
        }
    }
    
    public class ValuePath<TFrom, TTo> : TypePath<TFrom, TTo>, IValuePath<TFrom, TTo>
    {
        private readonly TTo value;
        private readonly bool isNull;

        public ValuePath(Func<TFrom, TTo> get, IMaybe<Func<TFrom, TTo, TFrom>> set, TTo value) : base(get, set)
        {
            this.value = value;
            this.isNull = !(value is TTo);
        }

        public TTo Value
        {
            get { return this.value; }
        }

        public IValuePath<TFrom, T> Wrap<T>(Func<TTo, T> vGet, IMaybe<Func<TTo, T, TTo>> mvSet, T value)
        {
            var wGet = new Func<TFrom, T>(x => vGet(this.Get(x)));
            var wmSet = this.Set.Bind(s => mvSet.FMap(vs => new Func<TFrom, T, TFrom>((n, vn) => s(n, vs(this.Get(n), vn)))));

            return new ValuePath<TFrom, T>(wGet, wmSet, value);
        }

        public bool IsNull
        {
            get { return this.isNull; }
        }
    }
}