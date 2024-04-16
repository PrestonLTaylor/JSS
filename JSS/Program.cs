using JSS.CLI;
using JSS.Lib.Execution;

var completion = Realm.InitializeHostDefinedRealm(out VM globalVm);
if (completion.IsAbruptCompletion())
{
    Print.PrintCompletion(completion);
    return;
}

var repl = new Repl(globalVm);
repl.ExecuteRepl();