namespace ApricotFramework.DataOps;

/// <summary>
/// Addresses an operation by its group and name.
/// </summary>
/// <remarks>
/// Both parts match ordinally, so a group or name in the wrong case addresses nothing. A null part
/// is stored as an empty string, which is also the group of an operation declared without one.
/// </remarks>
public readonly struct OpKey : IEquatable<OpKey>
{
    /// <summary>
    /// Gets the group the operation is declared in.
    /// </summary>
    public string Group { get; }

    /// <summary>
    /// Gets the name of the operation within its group.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Creates a key addressing an operation in a group.
    /// </summary>
    /// <param name="group">The group name.</param>
    /// <param name="name">The operation name.</param>
    public OpKey(string? group, string? name)
    {
        this.Group = group ?? string.Empty;
        this.Name = name ?? string.Empty;
    }

    /// <summary>
    /// Creates a key addressing an operation declared without a group.
    /// </summary>
    /// <param name="name">The operation name.</param>
    public OpKey(string? name) : this(null, name)
    {
    }

    /// <summary>
    /// Creates a key addressing an operation declared without a group.
    /// </summary>
    /// <param name="name">The operation name.</param>
    /// <returns>The key.</returns>
    public static OpKey Of(string? name)
    {
        return new OpKey(name);
    }

    /// <summary>
    /// Creates a key addressing an operation in a group.
    /// </summary>
    /// <param name="group">The group name.</param>
    /// <param name="name">The operation name.</param>
    /// <returns>The key.</returns>
    public static OpKey Of(string? group, string? name)
    {
        return new OpKey(group, name);
    }

    /// <summary>
    /// Compares two keys for equality.
    /// </summary>
    /// <param name="left">The left key.</param>
    /// <param name="right">The right key.</param>
    /// <returns>True when both parts match.</returns>
    public static bool operator ==(OpKey left, OpKey right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Compares two keys for inequality.
    /// </summary>
    /// <param name="left">The left key.</param>
    /// <param name="right">The right key.</param>
    /// <returns>True when either part differs.</returns>
    public static bool operator !=(OpKey left, OpKey right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Determines whether this key addresses the same operation as another.
    /// </summary>
    /// <param name="other">The key to compare with.</param>
    /// <returns>True when both parts match.</returns>
    public bool Equals(OpKey other)
    {
        return string.Equals(this.Name, other.Name, StringComparison.Ordinal)
            && string.Equals(this.Group, other.Group, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is OpKey other && this.Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(
            StringComparer.Ordinal.GetHashCode(this.Name),
            StringComparer.Ordinal.GetHashCode(this.Group));
    }

    /// <summary>
    /// Renders the key as <c>group/name</c>, as it appears in error messages.
    /// </summary>
    /// <returns>The rendered key.</returns>
    public override string ToString()
    {
        return $"{this.Group}/{this.Name}";
    }
}
