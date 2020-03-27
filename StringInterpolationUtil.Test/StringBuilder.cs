using System;
using System.Text;
using Xunit;

namespace StringInterpolationUtil.Test
{
    public class StringBuilderTest
    {
        [Fact]
        public void TestFormatStringBuilder()
        {
            TestFormatStringBuilderCore(
                new (int i, int j)[] {(1, 2), (99, 123), (123456789, 12345678)},
                p => $"i: {p.P(t => t.i)} j: {p.P(t => t.j)}",
                v => $"i: {v.i} j: {v.j}"
            );

            TestFormatStringBuilderCore(
                new (int i, string j)[] {(1, "he"), (99, "llo"), (53, "!"), (-32145, "999ss")},
                p => $"i: {p.P(t => t.i)} j: {p.P(t => t.j)}",
                v => $"i: {v.i} j: {v.j}"
            );

            TestFormatStringBuilderCore(
                new (int i, string j, char k)[] {(1, "he", 'k'), (99, "llo", 'h') },
                p => $"i: {p.P(t => t.i)} j: {p.P(t => t.j)} k: {p.P(t => t.k)}",
                v => $"i: {v.i} j: {v.j} k: {v.k}"
            );
        }

        private void TestFormatStringBuilderCore<T>(T[] values, Func<Placeholder<T, StringBuilderPlaceHolderAdapter>, FormattableString> func, Func<T, string> expected)
        {
            foreach (var v in values)
            {
                var stringBuilder = new CharBufferedStringBuilder();
                var formatStringBuilder = new FormatStringBuilder<T>(func);
                formatStringBuilder.Apply(stringBuilder, v);
                var got = new string(stringBuilder.Buffer, 0, stringBuilder.Length);
                Assert.Equal(expected(v), got);
            }
        }
    }
}
