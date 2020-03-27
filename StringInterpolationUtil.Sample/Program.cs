using System;
using System.Collections.Generic;

namespace StringInterpolationUtil.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = DictionaryBuilder.Create<(int i, int j)>(p => $@"
                i: {p.P(t => t.i)}
                j: {p.P(t => t.j)}
                
                jj: {p.P(t => t.j + t.j)}

                dict: {p.P(t => t.i, DictionaryBuilder.Create<int>(pp => $@"
                    s: 1
                    v: {pp.P(v => v)}
                "))}
            ");

            foreach (var kv in builder.ToDictionary((30, 40)))
            {
                if (kv.Value is Dictionary<string, object> dic)
                {
                    Console.WriteLine($"key:{kv.Key}");
                    foreach (var kv2 in dic)
                    {
                        Console.WriteLine($"    key:{kv2.Key} value:{kv2.Value}");
                    }
                }
                else
                {
                    Console.WriteLine($"key:{kv.Key} value:{kv.Value}");
                }
            }
        }
    }
}
