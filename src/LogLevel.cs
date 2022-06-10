// ----------------------------------------------------------------------------
// <copyright file="LogLevel.cs" company="AnyLog">
// Copyright (c) AnyLog Project
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics;

using AnyLog.Base;
using Microsoft.Toolkit.Diagnostics;

namespace AnyLog;

/// <summary>
/// The log level.
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public readonly struct LogLevel : ILogLevel, IEquatable<ILogLevel>, IEquatable<LogLevel>
{
    // ┌────────────────────────────────────────────────────────────────┐
    // │ PUBLIC Fields                                                  │
    // └────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// The <see cref="LogLevel"/> for reporting debug messages.
    /// </summary>
    public static readonly ILogLevel Any = new LogLevel("Any", uint.MinValue);

    /// <summary>
    /// The <see cref="LogLevel"/> for reporting debug messages.
    /// </summary>
    public static readonly ILogLevel Debug = new LogLevel("Debug", 100000);

    /// <summary>
    /// The <see cref="LogLevel"/> for reporting informational messages.
    /// </summary>
    public static readonly ILogLevel Info = new LogLevel("Info", 200000);

    /// <summary>
    /// The <see cref="LogLevel"/> for reporting warning messages.
    /// </summary>
    public static readonly ILogLevel Warning = new LogLevel("Warning", 300000);

    /// <summary>
    /// The <see cref="LogLevel"/> for reporting error messages.
    /// </summary>
    public static readonly ILogLevel Error = new LogLevel("Error", 400000);

    /// <summary>
    /// The <see cref="LogLevel"/> for reporting critical messages.
    /// </summary>
    public static readonly ILogLevel Fatal = new LogLevel("Info", 500000);

    /// <summary>
    /// The highest log level.
    /// </summary>
    public static readonly LogLevel Always = new("Always", uint.MaxValue);

    // ┌────────────────────────────────────────────────────────────────┐
    // │ PUBLIC Constructors                                            │
    // └────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Initializes a new instance of the <see cref="LogLevel"/> struct.
    /// </summary>
    /// <param name="name">The name of the <see cref="LogLevel"/>.</param>
    /// <param name="value">The numeric value of the <see cref="LogLevel"/>.</param>
    public LogLevel(string name, uint value)
    {
        Name = name;
        Value = value;
    }

    // ┌────────────────────────────────────────────────────────────────┐
    // │ PUBLIC Properties                                              │
    // └────────────────────────────────────────────────────────────────┘

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public uint Value { get; }

    // ┌────────────────────────────────────────────────────────────────┐
    // │ PUBLIC Methods                                                 │
    // └────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Overloads the == operator comparing <paramref name="left"/> with <paramref name="right"/>.
    /// </summary>
    /// <param name="left">The <see cref="LogLevel"/> on the left side.</param>
    /// <param name="right">The <see cref="LogLevel"/> on the right side.</param>
    /// <returns><c>true</c> if <paramref name="left"/> is equal to <paramref name="right"/>, <c>false</c> otherwise.</returns>
    public static bool operator ==(LogLevel left, LogLevel right) => left.Equals(right);

    /// <summary>
    /// Overloads the != operator comparing <paramref name="left"/> with <paramref name="right"/>.
    /// </summary>
    /// <param name="left">The <see cref="LogLevel"/> on the left side.</param>
    /// <param name="right">The <see cref="LogLevel"/> on the right side.</param>
    /// <returns><c>true</c> if <paramref name="left"/> is unequal to <paramref name="right"/>, <c>false</c> otherwise.</returns>
    public static bool operator !=(LogLevel left, LogLevel right) => !left.Equals(right);

    public static bool operator <(LogLevel left, LogLevel right) => left.Value < right.Value;

    public static bool operator <=(LogLevel left, LogLevel right) => left.Value <= right.Value;

    public static bool operator >(LogLevel left, LogLevel right) => left.Value > right.Value;

    public static bool operator >=(LogLevel left, LogLevel right) => left.Value >= right.Value;

    /// <summary>
    /// Converts the <paramref name="value"/> to a <see cref="ILogLevel"/>.
    /// </summary>
    /// <param name="value"e>The value to parse.</param>
    /// <returns>The corresponding <see cref="ILogLevel"/>.</returns>
    public static ILogLevel Parse(string value)
    {
        Guard.IsNotNull(value, nameof(value));

        return value.ToUpperInvariant() switch
        {
            "ANY" => Any,
            "DEBUG" => Debug,
            "INFO" => Info,
            "WARNING" => Warning,
            "ERROR" => Error,
            "FATAL" => Fatal,
            "ALWAYS" => Always,
            _ => throw new ArgumentException($"Unknown log level: {value}"),
        };
    }

    /// <inheritdoc/>
    public int CompareTo(object? obj)
    {
        if (obj is null)
        {
            return 1;
        }

        if (obj is ILogLevel other)
        {
            return Value.CompareTo(other.Value);
        }

        throw new ArgumentException($"Object must be of type {nameof(ILogLevel)}");
    }

    /// <inheritdoc/>
    public int CompareTo(ILogLevel? other)
    {
        if (other is null)
        {
            return 1;
        }

        return Value.CompareTo(other.Value);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (obj is ILogLevel other)
        {
            return Equals(other);
        }

        return false;
    }

    /// <inheritdoc/>
    public bool Equals(ILogLevel? other)
    {
        if (other is null)
        {
            return false;
        }

        return Value == other.Value;
    }

    /// <inheritdoc/>
    public bool Equals(LogLevel other) => Value == other.Value;

    /// <inheritdoc/>
    public override int GetHashCode() => (int)Value;

    /// <inheritdoc/>
    public override string ToString() => Name;

    // ┌────────────────────────────────────────────────────────────────┐
    // │ PRIVATE Methods                                                │
    // └────────────────────────────────────────────────────────────────┘
    private string GetDebuggerDisplay() => $"Log level: {ToString()}";
}
