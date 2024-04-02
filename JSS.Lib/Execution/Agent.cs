namespace JSS.Lib.Execution;

// 9.7 Agents, https://tc39.es/ecma262/#sec-agents
internal sealed class Agent
{
    private Agent()
    {
        LittleEndian = true;
        CanBlock = true;
        // FIXME: Verify if these are true
        IsLockFree1 = false;
        IsLockFree2 = false;
        IsLockFree8 = false;
    }

    static public Agent AgentSignifier()
    {
        // 1. Let AR be the Agent Record of the surrounding agent.
        // 2. Return AR.[[Signifier]].
        return _signifier; 
    }

    static private readonly Agent _signifier = new();

    public bool LittleEndian { get; }
    public bool CanBlock { get; }
    public bool IsLockFree1 { get; }
    public bool IsLockFree2 { get; }
    public bool IsLockFree8 { get; }
    // FIXME: [[CandidateExecution]]
    // FIXME: [[KeptAlive]]
}
