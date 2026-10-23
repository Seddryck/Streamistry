using global::Expressif.Values.Casters;

namespace Streamistry.Expressif;

public static class ScalarAdapters
{
    /// <summary>Uses Expressif's checked conversion rules, preserving successful CLR nulls.</summary>
    public static Func<TInput, TOutput> Convert<TInput, TOutput>()
    {
        var caster = new Caster();
        var target = Nullable.GetUnderlyingType(typeof(TOutput)) ?? typeof(TOutput);
        if (target != typeof(int) && target != typeof(decimal) && target != typeof(bool)
            && target != typeof(string) && target != typeof(DateOnly) && target != typeof(DateTime)
            && target != typeof(TimeOnly))
            throw new NotSupportedException("Use a CLR cast adapter for this target type.");
        return input =>
        {
            if (input is null || input is DBNull) return ExpressionAdapters.Adapt<TOutput>(null);
            if (!caster.TryCast(input, target, out var output))
                throw new InvalidCastException($"Expressif could not convert input to {target}.");
            return ExpressionAdapters.Adapt<TOutput>(output);
        };
    }
}
