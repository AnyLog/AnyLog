// ----------------------------------------------------------------------------
// <copyright file="LogMessage.cs" company="AnyLog">
// Copyright (c) AnyLog Project
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;

using AnyLog.Base;

namespace AnyLog;

/// <summary>
/// A log message.
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal sealed class LogMessage : ILogMessage
{
    // ┌────────────────────────────────────────────────────────────────┐
    // │ PRIVATE Fields                                                 │
    // └────────────────────────────────────────────────────────────────┘
    private readonly Stack<string> senders = new();

    // ┌────────────────────────────────────────────────────────────────┐
    // │ PUBLIC Constructors                                            │
    // └────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Initializes a new instance of the <see cref="LogMessage"/> class.
    /// </summary>
    /// <param name="logLevel">The <see cref="LogLevel"/>.</param>
    /// <param name="message">The message.</param>
    internal LogMessage(ILogLevel logLevel, object message)
    {
        LogLevel = logLevel;
        Payload = message;
    }

    // ┌────────────────────────────────────────────────────────────────┐
    // │ PUBLIC Properties                                              │
    // └────────────────────────────────────────────────────────────────┘

    /// <inheritdoc/>
    public DateTime Timestamp { get; } = DateTime.Now;

    /// <inheritdoc/>
    public ILogLevel LogLevel { get; }

    /// <inheritdoc/>
    public object Payload { get; }

    /// <inheritdoc/>
    public string Sender => senders.Count > 0 ? senders.Peek() : string.Empty;

    /// <summary>
    /// Adds <paramref name="sender"/> to the list of senders.
    /// </summary>
    /// <param name="sender">The sender to add.</param>
    internal void AddSender(string sender) => senders.Push(sender);

   // ┌────────────────────────────────────────────────────────────────┐
   // │ PRIVATE Methods                                                │
   // └────────────────────────────────────────────────────────────────┘
    private string? GetDebuggerDisplay() => Payload is null ? string.Empty : Payload.ToString();
}
