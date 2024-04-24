using JSS.Lib.Execution;

namespace JSS.Lib.AST.Values;

internal sealed class List : Value
{
    public List() { }
    public List(IEnumerable<Value> values)
    {
        Values = values.ToList();
    }

    override public ValueType Type() { throw new InvalidOperationException("Tried to get the Type of List"); }

    public void Add(Value value) => Values.Add(value);
    public void ListConcatenation(List list) => Values.AddRange(list.Values);


    // 7.3.17 CreateArrayFromList ( elements ), https://tc39.es/ecma262/#sec-createarrayfromlist
    public Array CreateArrayFromList(VM vm)
    {
        // 1. Let array be ! ArrayCreate(0).
        var array = MUST(Array.ArrayCreate(vm, 0));

        // 2. Let n be 0.
        var n = 0;

        // 3. For each element e of elements, do
        foreach (var e in Values)
        {
            // FIXME: We don't call ToString on n and we just use C#'s ToString
            // a. Perform ! CreateDataPropertyOrThrow(array, ! ToString(𝔽(n)), e).
            MUST(Object.CreateDataPropertyOrThrow(vm, array, n.ToString(), e));

            // b. Set n to n + 1.
            n += 1;
        }

        // 4. Return array.
        return array;
    }

    public int Count => Values.Count;

    public Value this[int i]
    {
        get
        {
            // This supports the default JS behaviour of excluded parameters defaulting to undefined
            if (i >= Values.Count)
            {
                return Undefined.The;
            }

            return Values[i];
        }
    }

    public List<Value> Values { get; } = new();
}
