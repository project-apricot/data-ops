// ExecutorTests depends on Dapper's process-wide column matching, which every test in this assembly
// shares. Serializing keeps a second writer from turning that into an intermittent failure.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
