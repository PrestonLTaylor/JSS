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
