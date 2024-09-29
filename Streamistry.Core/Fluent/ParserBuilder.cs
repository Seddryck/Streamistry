using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
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
    public IPipeBuilder<TInput> Upstream { get; }
    public IFormatProvider? FormatProvider { get; protected set; }

    public ParserBuilder(IPipeBuilder<TInput> upstream)
        => Upstream = upstream;

    public SpecializedParserBuilder<TInput, DateOnly> AsDate()
        => new (Upstream, typeof(DateParser), FormatProvider);
    public SpecializedParserBuilder<TInput, DateTime> AsDateTime()
        => new (Upstream, typeof(DateTimeParser), FormatProvider);

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
        => IsFormatProviderEnabled(Type)
            ? (IChainablePort<TOutput>)Activator.CreateInstance(
                Type
                , Upstream.BuildPipeElement()
                , FormatProvider)!
            : (IChainablePort<TOutput>)Activator.CreateInstance(
                Type
                , Upstream.BuildPipeElement()
                )!;

    private bool IsFormatProviderEnabled(Type type)
    {
        var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                        .Where(x => x.GetParameters().Length <= 2);
        return ctors.Any(x => x.GetParameters().Length == 2);
    }

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


