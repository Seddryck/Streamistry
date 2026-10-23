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
        if (value is string || value.GetType().IsValueType || value is IExpressifValueType) return value;
        if (!path.Add(value)) throw new ArgumentException("Cyclic CLR rows cannot be represented as records.");
        try
        {
            var record = new RecordValue();
            if (value is IEnumerable<KeyValuePair<string, object?>> fields)
            {
                foreach (var pair in fields) record.Set(pair.Key, ConvertValue(pair.Value, path));
                return record;
            }
            if (value is IDictionary dictionary)
            {
                foreach (DictionaryEntry pair in dictionary)
                    record.Set(pair.Key as string ?? throw new ArgumentException("Record keys must be strings."), ConvertValue(pair.Value, path));
                return record;
            }
            if (value is IEnumerable collection)
                return collection.Cast<object?>().Select(item => ConvertValue(item, path)).ToArray();
            foreach (var property in value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                if (property.CanRead && property.GetIndexParameters().Length == 0)
                    record.Set(property.Name, ConvertValue(property.GetValue(value), path));
            return record;
        }
        finally { path.Remove(value); }
    }
}

