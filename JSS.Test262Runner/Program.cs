// TODO: Setup ILogger (or something similar) that we can use for this project
using JSS.Test262Runner;
using System.Text;

var test262RepositoryCloner = new GitHubCloner("tc39/test262");
test262RepositoryCloner.CloneRepositoryIfNotAlreadyPresent();

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("\nStarting the test-262 runner...");
var test262Runner = new Test262Runner();
test262Runner.StartRunner();
