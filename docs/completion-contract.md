# Completion and queues

Delivery is synchronous and exceptions propagate to the caller. Normal exhaustion
and cooperative Stop complete downstream once per run. Failure is not successful
completion: partial buffers are not flushed on a failed delivery. Starting a fresh
run clears pending data and completion state. A Pipeline prepares all roots with
the same run identity before delivering any records; sources started independently
are independent executions.

PipeBuffer.Flush releases its current batch without completing. A positive
maxCapacity flushes a full batch; final completion flushes a partial batch. A null
capacity preserves the historical explicitly unbounded buffering mode.

Union and positional zip complete after all inputs complete. Zip pairs in arrival
order, includes null elements, and discards unmatched tails after all completions.
It emits no result for an empty input. Once a completed input queue is exhausted,
remaining unmatched values are discarded immediately without filling other queues;
terminal completion still waits for every input. Each zip input queue defaults to 1024 items;
capacity=null explicitly requests an unbounded queue. Overflow policies are Throw
(default, fail the run), DropNewest (ignore incoming item), and DropOldest (evict
oldest pending item). Lossy policies deliberately change pairing. Blocking is not
supported because roots execute sequentially and blocking would deadlock. Configure
capacity for known sequential input sizes. Keyed zip and threaded scheduling remain
outside this contract.

Migration: completion callbacks on a union now run once after every input. A source
that starts when the union completes must not also be an input of that union
(that is a dependency cycle). Chain dependent sources from the specific predecessor
source's completion. Reset or emit intermediate aggregate results explicitly at
that predecessor boundary when needed. Preparation-triggered sources inherit the
active pipeline run identity so queued inputs are preserved.
