// Every test class here calls AddDataOperations, which writes Dapper's process-wide column matching.
// Running classes in parallel lets one overwrite what another is asserting on.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
