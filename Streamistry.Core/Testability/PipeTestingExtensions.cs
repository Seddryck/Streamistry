using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streamistry.Testability;
public static class PipeTestingExtensions
{
    public static TOutput? EmitAndGetOutput<TInput, TOutput>(this BaseSingleRouterPipe<TInput, TOutput> pipe, TInput value)
    {
        TOutput? result = default;
        pipe.RegisterDownstream(x => result = x);
        pipe.Emit(value);
        return result;
    }

    public static bool EmitAndAnyOutput<TInput, TOutput>(this BaseSingleRouterPipe<TInput, TOutput> pipe, TInput value)
    {
        var anyResult = false;
        pipe.RegisterDownstream(x => anyResult = true);
        pipe.Emit(value);
        return anyResult;
    }

    public static TOutput?[] EmitAndGetManyOutputs<TInput, TOutput>(this BaseSingleRouterPipe<TInput, TOutput> pipe, TInput value)
    {
        var results = new List<TOutput?>();
        pipe.RegisterDownstream(results.Add);
        pipe.Emit(value);
        return [.. results];
    }

    public static TOutput? EmitAndGetOutput<TInput, TOutput>(this IProcessablePipe<TInput> input, TInput value, IChainablePipe<TOutput> output)
    {
        TOutput? result = default;
        output.RegisterDownstream(x => result = x);
        input.Emit(value);
        return result;
    }

    public static bool EmitAndAnyOutput<TInput, TOutput>(this IProcessablePipe<TInput> input, TInput value, IChainablePipe<TOutput> output)
    {
        var anyResult = false;
        output.RegisterDownstream(x => anyResult = true);
        input.Emit(value);
        return anyResult;
    }

    public static TOutput? EmitAndGetOutput<TInput, TOutput>(this IDualRoute<TOutput, TInput> input, TInput value)
    {
        TOutput? result = default;
        input.Main.RegisterDownstream(x => result = x);
        input.Emit(value);
        return result;
    }

    public static bool EmitAndAnyOutput<TInput, TOutput>(this IDualRoute<TOutput, TInput> input, TInput value)
    {
        var anyResult = false;
        input.Main.RegisterDownstream(x => anyResult = true);
        input.Emit(value);
        return anyResult;
    }

    public static TInput? EmitAndGetAlternateOutput<TInput, TOutput>(this IDualRoute<TOutput, TInput> input, TInput value)
    {
        TInput? result = default;
        input.Alternate.RegisterDownstream(x => result = x);
        input.Emit(value);
        return result;
    }

    public static bool EmitAndAnyAlternateOutput<TInput, TOutput>(this IDualRoute<TOutput, TInput> input, TInput value)
    {
        var anyResult = false;
        input.Alternate.RegisterDownstream(x => anyResult = true);
        input.Emit(value);
        return anyResult;
    }

    public static object?[] EmitAndGetOutputs<TInput>(this IProcessablePipe<TInput> input, TInput value, IChainablePipe[] outputs)
    {
        var result = new List<object?>();
        foreach (var output in outputs)
            output.RegisterDownstreamIfPossible(x => result.Add(x));

        input.Emit(value);
        return [.. result];
    }

    public static bool[] EmitAndAnyOutputs<TInput>(this IProcessablePipe<TInput> input, TInput value, IChainablePipe[] outputs)
    {
        var results = new bool[outputs.Length];

        for (var i = 0; i < outputs.Length; i++)
        {
            var index = i;  // Capture the current index for the lambda
            outputs[i].RegisterDownstreamIfPossible(x => results[index] = true);
        }

        input.Emit(value);
        return results;
    }

    public static TOutput?[] GetOutputs<TOutput>(this IChainablePort<TOutput> output, Action action)
    {
        var outputs = new List<TOutput?>();
        output.RegisterDownstream(outputs.Add);
        action.Invoke();
        return [.. outputs];
    }

    public static void RegisterDownstreamIfPossible(this IChainablePipe pipe, Action<object?> action)
    {
        // Get the type of the pipe object
        var pipeType = pipe.GetType();

        // Find the IChainablePipe<T> interface implemented by this object
        var chainablePipeInterface = pipeType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IChainablePipe<>));

        if (chainablePipeInterface != null)
        {
            // Get the generic argument T of IChainablePipe<T>
            var genericArgumentType = chainablePipeInterface.GetGenericArguments()[0];

            // Create a new Action<T?> that wraps the original Action<object?>
            var method = typeof(PipeTestingExtensions)
                .GetMethod(nameof(ConvertAction), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(genericArgumentType);

            var genericAction = method.Invoke(null, [action]);

            // Get the RegisterDownstream method of IChainablePipe<T>
            var registerMethod = chainablePipeInterface.GetMethod("RegisterDownstream", [genericAction!.GetType(), typeof(Action)]);

            // Invoke RegisterDownstream with the dynamic Action<T?>
            registerMethod?.Invoke(pipe, [genericAction, null]);
        }
        else
        {
            Console.WriteLine("The object does not implement IChainablePipe<T>.");
        }
    }

    private static Action<T?> ConvertAction<T>(Action<object?> action)
    {
        return obj => action(obj);
    }
}
