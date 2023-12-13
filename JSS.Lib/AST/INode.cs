using JSS.Lib.Execution;

namespace JSS.Lib.AST;

internal abstract class INode
{
    virtual public Completion Evaluate(VM vm) { throw new NotImplementedException($"{GetType().Name}'s Evaluate is not yet implemented."); }

    // NOTE: If a Node doesn't override BoundNames, then it has no bound names
    // 8.2.1 Static Semantics: BoundNames, https://tc39.es/ecma262/#sec-static-semantics-boundnames
    virtual public List<string> BoundNames() { return new List<string>(); }
}
