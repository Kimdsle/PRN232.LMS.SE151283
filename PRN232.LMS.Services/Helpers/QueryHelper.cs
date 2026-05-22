using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace PRN232.LMS.Services.Helpers;

public static class QueryHelper
{
    /// <summary>
    /// Apply case-insensitive contains search across the provided string properties.
    /// Returns the original query unchanged if search is null/empty.
    /// </summary>
    public static IQueryable<T> ApplySearch<T>(
        IQueryable<T> query,
        string? search,
        params Expression<Func<T, string?>>[] searchableFields)
    {
        if (string.IsNullOrWhiteSpace(search) || searchableFields.Length == 0)
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        var searchValue = Expression.Constant(search.ToLower(), typeof(string));
        var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;

        Expression? combined = null;
        foreach (var field in searchableFields)
        {
            // x => (x.Field == null ? "" : x.Field).ToLower().Contains(searchValue)
            var fieldExpr = Expression.Invoke(field, parameter);
            var nullCheck = Expression.Coalesce(fieldExpr, Expression.Constant(string.Empty));
            var toLower = Expression.Call(nullCheck, toLowerMethod);
            var contains = Expression.Call(toLower, containsMethod, searchValue);
            combined = combined == null ? (Expression)contains : Expression.OrElse(combined, contains);
        }

        if (combined == null) return query;
        var lambda = Expression.Lambda<Func<T, bool>>(combined, parameter);
        return query.Where(lambda);
    }

    /// <summary>
    /// Apply sort expression like "fullName,-dateOfBirth" -> OrderBy(FullName).ThenByDescending(DateOfBirth).
    /// If sort is null/empty, falls back to the provided default sort (must not be null/empty).
    /// </summary>
    public static IQueryable<T> ApplySort<T>(IQueryable<T> query, string? sort, string defaultSort)
    {
        var spec = string.IsNullOrWhiteSpace(sort) ? defaultSort : sort;
        var parts = spec.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var dynamicOrder = string.Join(", ", parts.Select(p =>
        {
            var desc = p.StartsWith("-");
            var field = desc ? p.Substring(1) : p;
            // Capitalize first letter to match C# property naming (StudentId, not studentId)
            field = char.ToUpper(field[0]) + field.Substring(1);
            return desc ? $"{field} descending" : field;
        }));

        return query.OrderBy(dynamicOrder);
    }

    /// <summary>
    /// Reduce a single object to a dictionary containing only the requested fields.
    /// If fields is null/empty, returns all public instance properties of the object.
    /// </summary>
    public static IDictionary<string, object?> ApplyFields<T>(T obj, string? fields) where T : class
    {
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var requested = string.IsNullOrWhiteSpace(fields)
            ? props.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase)
            : fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var result = new Dictionary<string, object?>();
        foreach (var prop in props)
        {
            if (requested.Contains(prop.Name))
                result[prop.Name] = prop.GetValue(obj);
        }
        return result;
    }

    /// <summary>
    /// Check whether an expand string (e.g. "student,course") contains the given keyword.
    /// </summary>
    public static bool ShouldExpand(string? expand, string keyword)
    {
        if (string.IsNullOrWhiteSpace(expand)) return false;
        return expand.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                     .Any(part => string.Equals(part, keyword, StringComparison.OrdinalIgnoreCase));
    }
}
