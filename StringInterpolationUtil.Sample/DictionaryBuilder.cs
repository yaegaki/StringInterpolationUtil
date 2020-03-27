using System;
using System.Collections.Generic;
using System.Text;

namespace StringInterpolationUtil.Sample
{
    public class DictionaryPlaceholderAdapter : IPlaceholderAdapter
    {
        public object LastAdaptedValue { get; private set; }

        public void Adapt(int v, string fmt)
            => Adapt<int>(v, fmt);

        public void Adapt(char v, string fmt)
            => Adapt<char>(v, fmt);

        public void Adapt<T>(T v, string fmt)
        {
            this.LastAdaptedValue = v;
        }
    }

    public static class DictionaryBuilder
    {
        public static DictionaryBuilder<T> Create<T>(Func<Placeholder<T, DictionaryPlaceholderAdapter>, FormattableString> f)
        {
            var p = new Placeholder<T, DictionaryPlaceholderAdapter>();
            var format = Format.Parse(f(p));
            if (format.Strings.Count == 0) return DictionaryBuilder<T>.Empty;

            return new DictionaryBuilder<T>(format);;
        }
    }

    public class DictionaryBuilder<T> : ITransformer<T, Dictionary<string, object>>
    {
        public static readonly DictionaryBuilder<T> Empty = new DictionaryBuilder<T>(default);
        private Entry[] entries;

        internal DictionaryBuilder(Format format)
        {
            var state = ParserState.Key;
            var key = string.Empty;
            var entry = default(Entry);
            var isInserted = false;
            var sb = new StringBuilder();
            var entries = new List<Entry>();

            void Parse(string s, bool lastString)
            {
                for (var i = 0; i < s.Length; i++)
                {
                    var c = s[i];
                    if (c == '\n' || (lastString && (i + 1) == s.Length))
                    {
                        if (state == ParserState.Value)
                        {
                            var value = sb.ToString().Trim();

                            if (isInserted)
                            {
                                if (value.Length == 0)
                                {
                                    entries.Add(entry);
                                }
                            }
                            else
                            {
                                entries.Add(new Entry(key, _ => value));
                            }
                        }


                        sb.Clear();
                        isInserted = false;
                        key = string.Empty;
                        entry = default;
                        state = ParserState.Key;
                        continue;
                    }

                    switch (state)
                    {
                        case ParserState.Key:
                            if (c == ':')
                            {
                                key = sb.ToString().Trim();
                                sb.Clear();
                                if (key.Length == 0)
                                {
                                    state = ParserState.IgnoreLine;
                                }
                                else
                                {
                                    state = ParserState.Value;
                                }
                            }
                            else
                            {
                                sb.Append(c);
                            }
                            break;
                        case ParserState.Value:
                            sb.Append(c);
                            break;
                        default:
                            break;
                    }
                }
            }

            var adapter = new DictionaryPlaceholderAdapter();
            for (var i = 0; i < format.Strings.Count; i++)
            {
                var lastString = i == format.Keys.Count;
                Parse(format.Strings[i], lastString);
                if (lastString) break;
                var value = sb.ToString().Trim();
                sb.Clear();
                var obj = format.Keys[i];

                if (state != ParserState.Value || value.Length != 0)
                {
                    state = ParserState.IgnoreLine;
                }
                else
                {
                    isInserted = true;
                    var fmt = format.Formats[i];
                    if (obj is Placeholder<T, DictionaryPlaceholderAdapter>.PlaceholderValue pv)
                    {
                        entry = new Entry(key, v =>
                        {
                            pv.Adapt(adapter, v, fmt);
                            return adapter.LastAdaptedValue;
                        });
                    }
                    else
                    {
                        entry = new Entry(key, _ => obj);
                    }
                }
            }

            this.entries = entries.ToArray();
        }

        public Dictionary<string, object> ToDictionary(T value)
        {
            if (entries == null) return new Dictionary<string, object>();

            var d = new Dictionary<string, object>();
            foreach (var e in this.entries)
            {
                d[e.Key] = e.F(value);
            }

            return d;
        }

        Dictionary<string, object> ITransformer<T, Dictionary<string, object>>.Transform(T v)
            => ToDictionary(v);

        enum ParserState
        {
            Key,
            Value,
            IgnoreLine,
        }

        readonly struct Entry
        {
            public readonly string Key;
            public readonly Func<T, object> F;

            public Entry(string key, Func<T, object> f)
                => (this.Key, this.F) = (key, f);
        }
    }
}
