# StringInterpolationUtil
 C# string interpolation

```csharp
// zero allocation string interpolation

var sb = new CharBufferedStringBuilder();
// create a teamplate using string interpolation
var fsb = new FormatStringBuilder<(int i, int j)>(p => $"i: {p.P(t => t.i)}, j: {p.P(t => t.j)}");

// append data to stringbuilder
fsb.Apply(sb, (100, 200));
for (var i = 0; i < sb.Length; i++)
{
    Console.WriteLine($"index: {i} char:{sb.Buffer[i]}");
}

/* output
index: 0 char:i
index: 1 char::
index: 2 char: 
index: 3 char:1
index: 4 char:0
index: 5 char:0
index: 6 char:,
index: 7 char: 
index: 8 char:j
index: 9 char::
index: 10 char: 
index: 11 char:2
index: 12 char:0
index: 13 char:0
*/
```

```csharp
// create dictionary from string interpolation

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

/* output
key:i value:30
key:j value:40
key:jj value:80
key:dict
    key:s value:1
    key:v value:30
*/
```
