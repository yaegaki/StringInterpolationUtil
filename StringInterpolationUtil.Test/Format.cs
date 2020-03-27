using System;
using Xunit;

namespace StringInterpolationUtil.Test
{
    public class FormatTest
    {
        [Fact]
        public void TestParseFormat()
        {
            TestParseFormatCore($"", Array.Empty<string>(), Array.Empty<string>(), Array.Empty<object>());
            TestParseFormatCore($"abc", new[] { "abc" }, Array.Empty<string>(), Array.Empty<object>());
            TestParseFormatCore($"{1}abc", new[] { "", "abc" }, new[] { "" }, new object[] { 1 });
            TestParseFormatCore($"abc{1}", new[] { "abc", "" }, new[] { "" }, new object[] { 1 });
            TestParseFormatCore($"{1}abc{1}", new[] { "", "abc", "" }, new[] { "", "" }, new object[] { 1, 1 });
            TestParseFormatCore($"qq{1}abc{1}pp", new[] { "qq", "abc", "pp" }, new[] { "", "" }, new object[] { 1, 1 });
            TestParseFormatCore($"qq{'1'}abc{1}pp", new[] { "qq", "abc", "pp" }, new[] { "", "" }, new object[] { '1', 1 });
            TestParseFormatCore($"{"1"}qq{'1'}abc{1}pp", new[] { "", "qq", "abc", "pp" }, new[] { "", "", "" }, new object[] { "1", '1', 1 });
            TestParseFormatCore($"qq{null}abc{1}pp", new[] { "qq", "abc", "pp" }, new[] { "", "" }, new object[] { null, 1 });
            TestParseFormatCore($"{1:9999}", new[] { "", "" }, new[] { "9999" }, new object[] { 1 });
            TestParseFormatCore($"{1:9999}x{1: x}", new[] { "", "x", "" }, new[] { "9999", "x" }, new object[] { 1, 1 });
        }

        private void TestParseFormatCore(FormattableString formattable, string[] strings, string[] formats, object[] keys)
        {
            var format = Format.Parse(formattable);
            Assert.Equal(strings, format.Strings);
            Assert.Equal(formats, format.Formats);
            Assert.Equal(keys, format.Keys);
        }
    }
}
