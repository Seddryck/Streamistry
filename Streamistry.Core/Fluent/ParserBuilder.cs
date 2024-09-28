using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Streamistry.Pipes.Parsers;

namespace Streamistry.Fluent;

public class ParserBuilder<TInput, TOutput> : PipeElementBuilder<TInput, TOutput>, IBuilder<IDualRoute>
{
    protected IFormatProvider? FormatProvider { get; set; }
    protected ParserDelegate<TInput, TOutput> ParseFunction { get; }

    public ParserBuilder(IPipeBuilder<TInput> upstream, ParserDelegate<TInput, TOutput> parseFunction)
        : base(upstream)
    => (ParseFunction) = (parseFunction);

    public ParserBuilder<TInput, TOutput> WithFormatProvider(IFormatProvider formatProvider)
    {
        FormatProvider = formatProvider;
        return this;
    }

    public override IChainablePort<TOutput> OnBuildPipeElement()
        => new Parser<TInput, TOutput>(Upstream.BuildPipeElement(), ParseFunction);
    IDualRoute IBuilder<IDualRoute>.BuildPipeElement()
        => base.BuildPipeElement().Pipe is IDualRoute dual ? dual : throw new InvalidCastException();
    IDualRoute IBuilder<IDualRoute>.OnBuildPipeElement()
        => throw new NotImplementedException();
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

public class SpecializedParserBuilder<TInput, TOutput> : PipeElementBuilder<TInput, TOutput>, IBuilder<IDualRoute>
{
    protected Type Type { get; }
    protected IFormatProvider? FormatProvider { get; set; }

    public SpecializedParserBuilder(IPipeBuilder<TInput> upstream, Type type, IFormatProvider? formatProvider)
        : base(upstream)
        => (Type, FormatProvider) = (type, formatProvider);

    public override IChainablePort<TOutput> OnBuildPipeElement()
        => (IChainablePort<TOutput>)Activator.CreateInstance(
                Type
                , Upstream.BuildPipeElement()
                , FormatProvider)!;

    IDualRoute IBuilder<IDualRoute>.BuildPipeElement()
        => base.BuildPipeElement().Pipe is IDualRoute dual ? dual : throw new InvalidCastException();
    IDualRoute IBuilder<IDualRoute>.OnBuildPipeElement()
        => throw new NotImplementedException();

    public SpecializedParserBuilder<TInput, TOutput> WithFormatProvider(IFormatProvider formatProvider)
    {
        FormatProvider = formatProvider;
        return this;
    }

    public RoutesBuilder Route<TPort, TNext>(Func<IDualRoute, IChainablePort> port, Segment<TPort, TNext> segment)
    {
        var routeBuilder = new RoutesBuilder(this);
        routeBuilder.Add(port, segment);
        return routeBuilder;
    }

    public RoutesBuilder Route<TPort, TNext>(Func<IDualRoute, IChainablePort> port, Func<BasePipeBuilder<TPort>, BasePipeBuilder<TNext>> path)
    {
        var routeBuilder = new RoutesBuilder(this);
        routeBuilder.Add(port, new Segment<TPort, TNext>(path));
        return routeBuilder;
    }

    public ConvergerBuilder<TNext> Converge<TNext>()
        => new(this);
}


