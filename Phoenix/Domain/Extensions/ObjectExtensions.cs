namespace SportFeedsBridge.Phoenix.Domain.Extensions;

public static class ObjectExtensions
{
    public static V WithDefault<T, V>(this T value, Func<T, V> getProperty, Func<V> defaultValue) where T : class
    {
        return value != null ? getProperty(value) : defaultValue();
    }

    public static V With<T, V>(this T value, Func<T, V> getProperty) where T : class
    {
        return value.WithDefault(getProperty, () => default(V));
    }

    public static T WithDefault<T>(this T value, Func<T> defaultValue) where T : class
    {
        return value ?? defaultValue();
    }

    public static T WithDefault<T>(this Nullable<T> value, Func<T> defaultValue) where T : struct
    {
        return value.HasValue ? value.Value : defaultValue();
    }

    public static V WithDefault<V>(this string value, Func<string, V> getProperty, Func<V> defaultValue)
    {
        return !string.IsNullOrEmpty(value) ? getProperty(value) : defaultValue();
    }

    public static bool ToStringContains<T>(this T currentValue, string filterString)
    {
        return filterString == null || currentValue.ToString().WithDefault(() => "").ToLower()
            .Contains(filterString.ToLower());
    }
}
