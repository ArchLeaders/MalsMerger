namespace MalsMerger;

public class Flags
{
    private readonly Dictionary<string, string?> _flags = [];

    public static Flags Parse(string[] args)
    {
        Flags result = new();

        foreach ((string key, int index) in args.Select((x, i) => (key: x, i)).Where(x => x.key.StartsWith('-'))) {
            int valueIndex = index + 1;
            string name = key.Replace("-", string.Empty);

            if (!string.IsNullOrEmpty(name)) {
                result._flags[name.ToLower()] = args.Length > valueIndex
                    ? args[valueIndex] : null;
            }
        }

        return result;
    }

    public bool TryGet<T>(out T? value, params string[] keyVariants)
    {
        try {
            value = Get<T>(keyVariants);
            return true;
        }
        catch {
            value = default;
            return false;
        }
    }

    public T Get<T>(T @default, params string[] keyVariants)
    {
        try {
            return Get<T>(keyVariants);
        }
        catch {
            return @default;
        }
    }

    public T Get<T>(params string[] keyVariants)
    {
        Type type = typeof(T);
        string key = keyVariants.Where(_flags.ContainsKey).FirstOrDefault()
            ?? throw new KeyNotFoundException($"Could not find a matching key from the provided key variants: '{string.Join(", ", keyVariants)}'");
        string? value = _flags[key];

        if (type == typeof(string)) {
            return (T)(object)(_flags[key] ?? string.Empty);
        }

        if (type == typeof(bool)) {
            if (value is null) {
                return (T)(object)true;
            }
            else if (bool.TryParse(value, out bool result)) {
                return (T)(object)result;
            }

            goto Failure;
        }

        if (type.IsPrimitive && type != typeof(bool)) {
            if (double.TryParse(value, out double result)) {
                return (T)Convert.ChangeType(result, type);
            }

            goto Failure;
        }

        if (type == typeof(decimal)) {
            if (decimal.TryParse(value, out decimal result)) {
                return (T)(object)result;
            }

            goto Failure;
        }

    Failure:
        throw new InvalidCastException($"Could not cast '{value}' to {type}");
    }
}