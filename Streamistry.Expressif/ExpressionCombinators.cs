using Streamistry.Pipes.Combinators;

namespace Streamistry.Expressif;

public static partial class StructureAdapters
{
    public static Zipper<T1, T2, TOutput> ZipExpression<T1, T2, TOutput>(IChainablePort<T1?> input1, IChainablePort<T2?> input2, string definition,
        int? capacity = 1024, QueueOverflowPolicy overflowPolicy = QueueOverflowPolicy.Throw)
    {
        var calculate = Combination<TOutput>(definition);
        return new(input1, input2, (v1, v2) => calculate([v1, v2]), capacity, overflowPolicy);
    }
    public static Zipper<T1, T2, T3, TOutput> ZipExpression<T1, T2, T3, TOutput>(IChainablePort<T1?> input1, IChainablePort<T2?> input2, IChainablePort<T3?> input3, string definition,
        int? capacity = 1024, QueueOverflowPolicy overflowPolicy = QueueOverflowPolicy.Throw)
    {
        var calculate = Combination<TOutput>(definition);
        return new(input1, input2, input3, (v1, v2, v3) => calculate([v1, v2, v3]), capacity, overflowPolicy);
    }
    public static Zipper<T1, T2, T3, T4, TOutput> ZipExpression<T1, T2, T3, T4, TOutput>(IChainablePort<T1?> input1, IChainablePort<T2?> input2, IChainablePort<T3?> input3, IChainablePort<T4?> input4, string definition,
        int? capacity = 1024, QueueOverflowPolicy overflowPolicy = QueueOverflowPolicy.Throw)
    {
        var calculate = Combination<TOutput>(definition);
        return new(input1, input2, input3, input4, (v1, v2, v3, v4) => calculate([v1, v2, v3, v4]), capacity, overflowPolicy);
    }
    public static Zipper<T1, T2, T3, T4, T5, TOutput> ZipExpression<T1, T2, T3, T4, T5, TOutput>(IChainablePort<T1?> input1, IChainablePort<T2?> input2, IChainablePort<T3?> input3, IChainablePort<T4?> input4, IChainablePort<T5?> input5, string definition,
        int? capacity = 1024, QueueOverflowPolicy overflowPolicy = QueueOverflowPolicy.Throw)
    {
        var calculate = Combination<TOutput>(definition);
        return new(input1, input2, input3, input4, input5, (v1, v2, v3, v4, v5) => calculate([v1, v2, v3, v4, v5]), capacity, overflowPolicy);
    }
    public static Zipper<T1, T2, T3, T4, T5, T6, TOutput> ZipExpression<T1, T2, T3, T4, T5, T6, TOutput>(IChainablePort<T1?> input1, IChainablePort<T2?> input2, IChainablePort<T3?> input3, IChainablePort<T4?> input4, IChainablePort<T5?> input5, IChainablePort<T6?> input6, string definition,
        int? capacity = 1024, QueueOverflowPolicy overflowPolicy = QueueOverflowPolicy.Throw)
    {
        var calculate = Combination<TOutput>(definition);
        return new(input1, input2, input3, input4, input5, input6, (v1, v2, v3, v4, v5, v6) => calculate([v1, v2, v3, v4, v5, v6]), capacity, overflowPolicy);
    }
}
