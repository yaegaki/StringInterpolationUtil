using System;
using System.Collections.Generic;
using System.Linq;

namespace StringInterpolationUtil
{
    public readonly struct Format
    {
        public readonly IReadOnlyList<string> Strings;
        public readonly IReadOnlyList<string> Formats;
        public readonly IReadOnlyList<object> Keys;

        public static Format Parse(FormattableString s)
            => Parse(s.Format, s.GetArguments());

        private static Format Parse(string format, object[] keys)
        {
            var strings = new List<string>(keys.Length + 1);
            var formats = new List<string>(keys.Length);

            var startIndex = 0;
            for (var i = 0; i < format.Length; i++)
            {
                var c = format[i];
                if (i + 1 == format.Length)
                {
                    strings.Add(format.Substring(startIndex, i - startIndex + 1));
                    break;
                }

                if (c == '{' && format[i+1] != '{')
                {
                    strings.Add(format.Substring(startIndex, i - startIndex));
                    var interceptStartIndex = i;
                    for (i = i + 2; i < format.Length && format[i] != '}'; i++) ;
                    var x = format.Substring(interceptStartIndex, i - interceptStartIndex).Split(':');
                    formats.Add(x.Length != 2 ? string.Empty : x[1].Trim());
                    startIndex = i + 1;
                    if (startIndex == format.Length)
                    {
                        strings.Add(string.Empty);
                        break;
                    }
                }
            }

            return new Format(strings, formats, keys.ToArray());
        }

        private Format(IReadOnlyList<string> strings, IReadOnlyList<string> formats, IReadOnlyList<object> keys)
            => (this.Strings, this.Formats, this.Keys) = (strings, formats, keys);
    }
}
