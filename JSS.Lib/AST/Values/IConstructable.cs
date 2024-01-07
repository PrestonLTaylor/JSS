using JSS.Lib.Execution;

namespace JSS.Lib.AST.Values;

internal interface IConstructable
{
    // 7.3.15 Construct ( F [ , argumentsList [ , newTarget ] ] ), https://tc39.es/ecma262/#sec-construct
    static public Completion Construct(VM vm, IConstructable F, List? argumentsList)
    {
        // FIXME: 1. If newTarget is not present, set newTarget to F.

        // 2. If argumentsList is not present, set argumentsList to a new empty List.
        argumentsList ??= new List();

        // 3. Return ? F.[[Construct]](argumentsList, newTarget).
        return F.Construct(vm, argumentsList);
    }

    public Completion Construct(VM vm, List argumentsList);
}
