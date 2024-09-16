using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Pipes.Parsers;

namespace Streamistry.Fluent;
public class ParserBuilder<TInput, TOutput> : PipeElementBuilder<TInput, TOutput>
{
    protected IFormatProvider? FormatProvider { get; set; }
    protected ParserDelegate<TInput, TOutput> ParseFunction { get; }

    public ParserBuilder(IPipeBuilder<TInput> upstream, ParserDelegate<TInput, TOutput> parseFunction)
        : base(upstream)
    => ParseFunction = parseFunction;

    public ParserBuilder<TInput, TOutput> WithFormatProvider(IFormatProvider formatProvider)
    {
        FormatProvider = formatProvider;
        return this;
    }

    public override IChainablePort<TOutput> OnBuildPipeElement()
        => new Parser<TInput, TOutput>(Upstream.BuildPipeElement(), ParseFunction);
}

public class ParserBuilder<TInput>
{
    protected IPipeBuilder<TInput> Upstream { get; }
    protected IFormatProvider? FormatProvider { get; set; }

    public ParserBuilder(IPipeBuilder<TInput> upstream)
        => Upstream = upstream;

    public SpecializedParserBuilder<TInput, DateOnly> AsDate()
        => new SpecializedParserBuilder<TInput, DateOnly>(Upstream, typeof(DateParser), FormatProvider);
    public SpecializedParserBuilder<TInput, DateTime> AsDateTime()
        => new SpecializedParserBuilder<TInput, DateTime>(Upstream, typeof(DateTimeParser), FormatProvider);

    public ParserBuilder<TInput> WithFormatProvider(IFormatProvider formatProvider)
    {
        FormatProvider = formatProvider;
        return this;
    }
}

public class SpecializedParserBuilder<TInput, TOutput> : PipeElementBuilder<TInput, TOutput>
{
    protected Type Type { get; }
    protected IFormatProvider? FormatProvider { get; set; }

    public SpecializedParserBuilder(IPipeBuilder<TInput> upstream, Type type, IFormatProvider? formatProvider)
        : base(upstream)
        => (Type, FormatProvider) = (type, formatProvider);

    public override IChainablePort<TOutput> OnBuildPipeElement()
    {
        return (IChainablePort<TOutput>)Activator.CreateInstance(Type, Upstream.BuildPipeElement(), FormatProvider)!;
    }

    public SpecializedParserBuilder<TInput, TOutput> WithFormatProvider(IFormatProvider formatProvider)
    {
        FormatProvider = formatProvider;
        return this;
    }
}

//internal class UniversalParserBuilder<TInput, TOutput> : PipeElementBuilder<TInput, TOutput>
//{
//    protected Func<TAccumulate?, TInput?, TAccumulate?>? Accumulator { get; }
//    protected Func<TAccumulate?, TOutput?>? Selector { get; set; } = x => (TOutput?)Convert.ChangeType(x, typeof(TOutput));
//    protected TAccumulate? Seed { get; set; } = default;

//    public UniversalParserBuilder(IPipeBuilder<TInput> upstream, Func<TAccumulate?, TInput?, TAccumulate?> accumulator)
//        : base(upstream)
//        => (Accumulator) = (accumulator);

//    public UniversalParserBuilder<TInput, TAccumulate, TOutput> WithSelector(Func<TAccumulate?, TOutput?>? selector)
//    {
//        Selector = selector;
//        return this;
//    }

//    public UniversalParserBuilder<TInput, TAccumulate, TOutput> WithSeed(TAccumulate? seed)
//    {
//        Seed = seed;
//        return this;
//    }

//    public override IChainablePort<TOutput> OnBuildPipeElement()
//        => new Parser<TInput, TAccumulate, TOutput>(
//                Upstream.BuildPipeElement()
//                , Accumulator ?? throw new InvalidOperationException()
//                , Selector ?? throw new InvalidOperationException()
//                , Seed
//            );

//}
