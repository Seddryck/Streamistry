# Running aggregates

UsingExpressif().Count(), .Sum(), .Min(), .Max() use Expressif 2.93.1 IAccumulator
implementations. Aggregate<T>(factory) accepts other verified upstream accumulators.
Factories must return independent instances. Each pipe initializes at construction
and obtains fresh initialized state at each run or explicit Reset. Completion keeps
Current available and emits no extra result. Every input produces one running
result; empty input emits nothing. State storage for these four aggregates is O(1).

Count returns int and includes null, unlike legacy Core Count which ignores null.
Sum/min/max use Expressif decimal numeric conversion and reject null. Sum starts at
zero; min/max start null. Decimal overflow throws instead of wrapping a CLR integer.
There is no integer truncation or automatic decimal-to-int adaptation. Use explicit
CLR output conversion when its rounding/truncation policy is wanted.

Average and median remain legacy compatibility APIs: Expressif 2.93.1 does not
provide their accumulators. Required upstream work is tracked at
https://github.com/Seddryck/Expressif/issues/1340. Do not replace these with a
collection fold that buffers the stream. Average can use bounded state; an exact
median inherently retains order statistics (the legacy implementation stores the
values). Enable these migrations only after verifying the published upstream API
and its numeric semantics. Existing Core aggregate APIs remain available to preserve
their generic CLR numeric types and completion callbacks.
