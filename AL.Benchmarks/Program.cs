#region
using BenchmarkDotNet.Running;
#endregion

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                 .Run(args);