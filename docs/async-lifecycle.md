# Awaitable execution

Await Pipeline.StartAsync(token) or Source.StartAsync(token). Execution remains
sequential on the calling flow; this API does not introduce worker threads.
Reader sources accept Func<CancellationToken, ValueTask<ReaderLease>> for opening
resources. Use StartAsync for these factories; Start deliberately rejects them.
DbDataReader.ReadAsync receives the token. Plain IDataReader.Read remains synchronous:
cancellation is checked before/after each read but cannot interrupt a blocking call.
Downstream delegates likewise execute synchronously.

Owned leases are disposed in finally on cancellation, setup/read/delivery failure
or normal completion. Factory implementations must dispose partially opened
resources if they throw before returning a lease. SetupAsync and CleanupAsync are
awaitable extension points. No async void or detached task is used.

Stop requests a cooperative stop. Cleanup occurs after the current read/delivery
returns; Stop does not race reader disposal against an active read. Stop completes
the partial stream; token cancellation throws and does not signal successful
completion. Disposing an active source requests Stop; await its active task to
observe cleanup. A disposed source cannot restart.

Concurrent or reentrant executions of the same source/pipeline throw. Sequential
restarts get fresh run state and a fresh factory reader/enumerator. Borrowed readers
resume their existing cursor. Compatibility WaitOnPrepared/WaitOnCompleted callbacks
are synchronous: use them with synchronous factories. For asynchronous dependencies,
await preparation explicitly before starting dependent sources; do not put async
void callbacks into those hooks.
