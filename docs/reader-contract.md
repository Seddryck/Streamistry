# Reader sources

Existing IDataReader constructors borrow the reader: callers retain ownership of
the reader, command, connection and backing stream. Factory constructors execute
at Start and return a ReaderLease. ReaderLease.Own disposes the reader followed by
explicit dependencies in supplied order (normally command, connection, stream).
Borrow leaves everything open. Cleanup runs on completion, read or setup failure,
and Stop, and is idempotent. Factories must clean up resources opened before they
can return a lease. A factory opens fresh resources for every execution; a borrowed
reader resumes its current position and cannot be replayed after exhaustion.

Dictionary and immutable-array sources snapshot each row in ordinal column order.
DBNull becomes null. Dictionary names are case-sensitive; duplicate names fail
with ArgumentException rather than silently discarding a column. Use arrays when
duplicate names must be preserved. Names are not interned. Snapshots remain valid
after advancing/disposal, but values are shallow copies: provider-specific mutable
objects or reader-backed LOBs must be materialized by a provider adapter.
Single-value sources read column zero, map DBNull to default(T), and retain their
existing typed IDataRecord getter behavior. Use nullable output types for nulls.

Core exposes only BCL reader, dictionary, array and scalar types. Optional packages
adapt these values to their own record representations.

The protected DataReader getter/setter remains for existing subclasses. Supplied
readers are accessible before execution. Assigning a replacement borrows it, releases
any current owned lease, and uses the replacement on subsequent runs. Factory-backed
readers exist only during execution.
