using System.Collections;
using System.Reflection;
using global::Expressif.Values;
using global::Expressif.Types;

namespace Streamistry.Expressif;

/// <summary>Representation conversion only; field traversal and calculations belong to Expressif.</summary>
public static class StructuredValues
{
    public static object? FromClr(object? value) => ConvertValue(value, new HashSet<object>(ReferenceEqualityComparer.Instance));

    private static object? ConvertValue(object? value, HashSet<object> path)
    {
        if (value is null || value is DBNull) return null;
        if (IsScalar(value)) return value;
        if (!path.Add(value)) throw new ArgumentException("Cyclic CLR rows cannot be represented as records.");
        try
        {
            return ConvertComposite(value, path);
        }
        finally { path.Remove(value); }
    }

    private static bool IsScalar(object value)
        => value is string || value.GetType().IsValueType || value is IExpressifValueType;

    private static object ConvertComposite(object value, HashSet<object> path)
        => value switch
        {
            IEnumerable<KeyValuePair<string, object?>> fields => ConvertFields(fields, path),
            IDictionary dictionary => ConvertDictionary(dictionary, path),
            IEnumerable collection => collection.Cast<object?>().Select(item => ConvertValue(item, path)).ToArray(),
            _ => ConvertProperties(value, path)
        };

    private static RecordValue ConvertFields(IEnumerable<KeyValuePair<string, object?>> fields, HashSet<object> path)
    {
        var record = new RecordValue();
        foreach (var pair in fields) record.Set(pair.Key, ConvertValue(pair.Value, path));
        return record;
    }

    private static RecordValue ConvertDictionary(IDictionary dictionary, HashSet<object> path)
    {
        var record = new RecordValue();
        foreach (DictionaryEntry pair in dictionary)
            record.Set(pair.Key as string ?? throw new ArgumentException("Record keys must be strings."), ConvertValue(pair.Value, path));
        return record;
    }

    private static RecordValue ConvertProperties(object value, HashSet<object> path)
    {
        var record = new RecordValue();
        foreach (var property in value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            if (property.CanRead && property.GetIndexParameters().Length == 0)
                record.Set(property.Name, ConvertValue(property.GetValue(value), path));
        return record;
    }
}

