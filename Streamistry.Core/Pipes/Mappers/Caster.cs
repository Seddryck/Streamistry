using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Streamistry.Pipes.Mappers;
internal class CasterHelper<TInput, TOutput>
{
    public static bool HasInheritanceConversion()
    {
        return typeof(TOutput).IsAssignableFrom(typeof(TInput));
    }

    public static Func<TInput, TOutput>? GetImplicitOperator()
    {
        // Check for implicit conversion operator methods in the source type
        var implicitOperator = typeof(TInput).GetMethods(BindingFlags.Static | BindingFlags.Public)
            .FirstOrDefault(m => m.Name == "op_Implicit"
                      && m.ReturnType == typeof(TOutput)
                      && m.GetParameters().Length == 1
                      && m.GetParameters()[0].ParameterType == typeof(TInput));

        implicitOperator ??= typeof(TOutput).GetMethods(BindingFlags.Static | BindingFlags.Public)
            .FirstOrDefault(m => m.Name == "op_Implicit"
                      && m.ReturnType == typeof(TOutput)
                      && m.GetParameters().Length == 1
                      && m.GetParameters()[0].ParameterType == typeof(TInput));

        if (implicitOperator != null)
        {
            // Convert the MethodInfo to a delegate
            return (Func<TInput, TOutput>)Delegate.CreateDelegate(typeof(Func<TInput, TOutput>), implicitOperator);
        }
        return null;
    }

    public static Func<TInput, TOutput>? GetExplicitOperator()
    {
        // Now check for explicit conversion operator methods in the source type
        var explicitOperator = typeof(TInput).GetMethods(BindingFlags.Static | BindingFlags.Public)
            .FirstOrDefault(m => m.Name == "op_Explicit"
                      && m.ReturnType == typeof(TOutput)
                      && m.GetParameters().Length == 1
                      && m.GetParameters()[0].ParameterType == typeof(TInput));

        // Check for explicit conversion operator methods in the destination type
        explicitOperator ??= typeof(TOutput).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .FirstOrDefault(m => m.Name == "op_Explicit"
                          && m.ReturnType == typeof(TOutput)
                          && m.GetParameters().Length == 1
                          && m.GetParameters()[0].ParameterType == typeof(TInput));

        if (explicitOperator != null)
        {
            // Convert the MethodInfo to a delegate
            return (Func<TInput, TOutput>)Delegate.CreateDelegate(typeof(Func<TInput, TOutput>), explicitOperator);
        }
        return null;
    }


}

public class Caster<TInput, TOutput> : Mapper<TInput, TOutput>
{
    public Caster()
            : base(input => new InternalCaster(
                CasterHelper<TInput, TOutput>.GetImplicitOperator()).Cast(input))
    { }

    public Caster(IChainablePort<TInput> upstream)
            : base(input => new InternalCaster(
                CasterHelper<TInput, TOutput>.GetImplicitOperator()).Cast(input)
            , upstream)
    { }

    private class InternalCaster
    {
        private Func<TInput, TOutput>? ImplicitOperator { get; }

        public InternalCaster(Func<TInput, TOutput>? implicitOperator)
        {
            ImplicitOperator = implicitOperator;
        }

        public TOutput Cast(TInput input)
        {
            if (input is null)
                return default!;

            if (ImplicitOperator is not null)
                return ImplicitOperator.Invoke(input);
            else if (input is TOutput outputCasted)
                return outputCasted;
            if (input is IConvertible)
                try
                {
                    return (TOutput)Convert.ChangeType(input, typeof(TOutput));
                }
                catch (Exception ex)
                {
                    throw new InvalidCastException($"Specified cast from '{typeof(TInput).Name}' to '{typeof(TOutput).Name}' is not valid.", ex);
                }
            else
                try
                {
                    return (TOutput)(object)input;
                }
                catch (Exception ex)
                {
                    throw new InvalidCastException($"Specified cast from '{typeof(TInput).Name}' to '{typeof(TOutput).Name}' is not valid.", ex);
                }
        }
    }
}

public class SafeCaster<TInput, TOutput> : TryRouterPipe<TInput, TOutput>
{
    private InternalCaster Caster { get; }

    public SafeCaster()
            : this(null)
    { }

    public SafeCaster(IChainablePort<TInput>? upstream)
            : base(upstream)
    {
        Caster = new InternalCaster(
            CasterHelper<TInput, TOutput>.GetImplicitOperator(),
            CasterHelper<TInput, TOutput>.GetExplicitOperator());
    }

    protected override bool TryInvoke(TInput obj, [NotNullWhen(true)] out TOutput? value)
        => Caster.TryCast(obj, out value);

    private class InternalCaster
    {
        private Func<TInput, TOutput>? ImplicitOperator { get; }
        private Func<TInput, TOutput>? ExplicitOperator { get; }

        public InternalCaster(Func<TInput, TOutput>? implicitOperator, Func<TInput, TOutput>? explicitOperator)
        {
            ImplicitOperator = implicitOperator;
            ExplicitOperator = explicitOperator;
        }

        public bool TryCast(TInput input, out TOutput output)
        {
            if (input is null)
            {
                output = default!;
                return true;
            }

            if (ImplicitOperator is not null)
            {
                output = ImplicitOperator.Invoke(input);
                return true;
            }

            if (ExplicitOperator is not null)
            {
                try
                {
                    output = ExplicitOperator.Invoke(input);
                    return true;
                }
                catch
                {
                    output = default!;
                    return false;
                }
            }

            if (input is TOutput outputCasted)
            {
                output = outputCasted;
                return true;
            }

            if (input is IConvertible)
                try
                {
                    output = (TOutput)Convert.ChangeType(input, typeof(TOutput));
                    return true;
                }
                catch
                { }

            output = default!;
            return false;
        }
    }
}
