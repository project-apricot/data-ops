using Xunit.Sdk;
using Xunit.v3;

// Every test class here calls AddDataOperations, which writes Dapper's process-wide column matching.
// Running classes in parallel lets one overwrite what another is asserting on.
[assembly: Parallelization(Mode = ParallelMode.None)]
