using System;
using System.Linq;

namespace StringInterpolationUtil
{

    public interface IStringBuilder
    {
        void Append(string s);
        void Append(char[] source, int index, int length);
        void Append(char v, string fmt);
        void Append(int v, string fmt);

        void Append<T>(T v, string fmt);
    }

    public class CharBufferedStringBuilder : IStringBuilder
    {
        public char[] Buffer => buffer;
        public int Length => length;

        private char[] buffer;
        private int length;

        public CharBufferedStringBuilder() : this(10)
        {
        }

        public CharBufferedStringBuilder(int capacity)
            => this.buffer = new char[capacity];

        public void Clear()
            => this.length = 0;

        public void Append(string s)
        {
            Ensure(this.length + s.Length);

            s.CopyTo(0, this.buffer, this.length, s.Length);
            this.length += s.Length;
        }

        public void Append(char[] source, int index, int length)
        {
            Ensure(this.length + length);

            Array.Copy(source, index, this.buffer, this.length, length);
            this.length += length;
        }

        public void Append<T>(T v, string fmt)
            => Append(Stringer<T>.ToString(v, fmt));

        public void Append(char v, string format)
        {
            Ensure(this.length + 1);
            this.buffer[this.length] = v;
            this.length++;
        }

        public void Append(int v, string fmt)
        {
            var negative = v < 0;
            if (negative)
            {
                Ensure(this.length + 1);
                this.buffer[this.length] = '-';
                this.length++;
                v = -v;
            }

            var startLength = this.length;

            while (v >= 0)
            {
                Ensure(this.length + 1);
                this.buffer[this.length] = (char)((v % 10) + '0');
                this.length++;
                v /= 10;
                if (v == 0) break;
            }

            // reverse
            var len = (this.length - startLength) / 2;
            for (var i = 0; i < len; i++)
            {
                ref var a = ref this.buffer[startLength + i];
                ref var b = ref this.buffer[this.length - 1 - i];
                (a, b) = (b, a);
            }
        }

        private void Ensure(int size)
        {
            if (this.buffer.Length >= size) return;
            Array.Resize(ref this.buffer, size * 2);
        }

        class Stringer<T>
        {
            private static readonly Func<T, string, string> toString;

            static Stringer()
            {
                toString = GetToString();
            }

            private static Func<T, string, string> GetToString()
            {
                switch (typeof(T))
                {
                    case Type t when t == typeof(sbyte) :
                        return (v, fmt) => ((sbyte)(object)v).ToString(fmt);
                    case Type t when t == typeof(short) :
                        return (v, fmt) => ((short)(object)v).ToString(fmt);
                    case Type t when t == typeof(ushort) :
                        return (v, fmt) => ((ushort)(object)v).ToString(fmt);
                    case Type t when t == typeof(uint) :
                        return (v, fmt) => ((uint)(object)v).ToString(fmt);
                    case Type t when t == typeof(long) :
                        return (v, fmt) => ((long)(object)v).ToString(fmt);
                    case Type t when t == typeof(ulong) :
                        return (v, fmt) => ((ulong)(object)v).ToString(fmt);
                    case Type t when t == typeof(float) :
                        return (v, fmt) => ((float)(object)v).ToString(fmt);
                    case Type t when t == typeof(double) :
                        return (v, fmt) => ((double)(object)v).ToString(fmt);
                    default:
                        return (v, _) => v.ToString();
                }
            }

            public static string ToString(T v, string fmt)
                => toString(v, fmt);
        }
    }

    public interface IPlaceholderAdapter
    {
        void Adapt(int v, string fmt);
        void Adapt(char v, string fmt);
        void Adapt<T>(T v, string fmt);
    }

    public readonly struct StringBuilderPlaceHolderAdapter : IPlaceholderAdapter
    {
        private readonly IStringBuilder stringBuilder;

        public StringBuilderPlaceHolderAdapter(IStringBuilder stringBuilder)
            => this.stringBuilder = stringBuilder;

        public void Adapt(int v, string fmt)
            => this.stringBuilder.Append(v, fmt);

        public void Adapt(char v, string fmt)
            => this.stringBuilder.Append(v, fmt);

        public void Adapt<T>(T v, string fmt)
            => this.stringBuilder.Append(v, fmt);
    }

    public interface ITransformer<T, K>
    {
        K Transform(T v);
    }

    public readonly struct Placeholder<T, TAdapter>
        where TAdapter : IPlaceholderAdapter
    {
        public PlaceholderValue P(Func<T, int> f)
            => new PlaceholderValue((adapter, v, format) => adapter.Adapt(f(v), format));

        public PlaceholderValue P(Func<T, char> f)
            => new PlaceholderValue((adapter, v, format) => adapter.Adapt(f(v), format));

        public PlaceholderValue P<K>(Func<T, K> f)
            => new PlaceholderValue((adapter, v, format) => adapter.Adapt(f(v), format));

        public PlaceholderValue P<K, J>(Func<T, K> selector, ITransformer<K, J> transformer)
            => new PlaceholderValue((adapter, v, format) => adapter.Adapt(transformer.Transform(selector(v)), format));

        public readonly struct PlaceholderValue
        {
            private readonly Action<TAdapter, T, string> adapt;

            public PlaceholderValue(Action<TAdapter, T, string> adapt)
                => this.adapt = adapt;
            
            public void Adapt(TAdapter adapter, T v, string format)
                => this.adapt(adapter, v, format);
        }
    }

    public readonly struct FormatStringBuilder<T>
    {
        private readonly string[] strings;
        private readonly string[] formats;
        private readonly Action<StringBuilderPlaceHolderAdapter, T, string>[] appendActions;

        public FormatStringBuilder(Func<Placeholder<T, StringBuilderPlaceHolderAdapter>, FormattableString> f)
        {
            var format = Format.Parse(f(new Placeholder<T, StringBuilderPlaceHolderAdapter>()));
            if (format.Strings.Count == 0)
            {
                this.strings = default;
                this.formats = default;
                this.appendActions = default;
                return;
            }

            this.strings = format.Strings.ToArray();
            this.formats = format.Formats.ToArray();

            this.appendActions = format.Keys
                .Select(key =>
                {
                    if (key is Placeholder<T, StringBuilderPlaceHolderAdapter>.PlaceholderValue pv)
                    {
                        return new Action<StringBuilderPlaceHolderAdapter, T, string>((adapter, v, fmt) => pv.Adapt(adapter, v, fmt));
                    }
                    else
                    {
                        return new Action<StringBuilderPlaceHolderAdapter, T, string>((adapter, v, fmt) => adapter.Adapt(v, fmt));
                    }
                })
                .ToArray();
        }

        public void Apply(IStringBuilder stringBuilder, T value)
        {
            if (this.strings == null) return;

            for (var i = 0; i < this.strings.Length - 1; i++)
            {
                stringBuilder.Append(this.strings[i]);
                this.appendActions[i](new StringBuilderPlaceHolderAdapter(stringBuilder), value, this.formats[i]);
            }

            stringBuilder.Append(strings[strings.Length - 1]);
        }
    }
}
