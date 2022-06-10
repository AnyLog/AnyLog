// ----------------------------------------------------------------------------
// <copyright file="Logger.cs" company="AnyLog">
// Copyright (c) AnyLog Project
// </copyright>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using AnyLog.Base;

namespace AnyLog;

/// <summary>
/// The logger.
/// </summary>
public sealed class Logger : ILogger, IDisposable, IAsyncDisposable
{
    // ┌────────────────────────────────────────────────────────────────┐
    // │ PRIVATE Fields                                                 │
    // └────────────────────────────────────────────────────────────────┘
    private readonly Logger? parentLogger;

    private bool isDisposed;

    private volatile Task? logTask;

    private ImmutableList<ILogSink> logSinks = ImmutableList.Create<ILogSink>();

    private ImmutableList<Predicate<ILogMessage>> logFilters = ImmutableList.Create<Predicate<ILogMessage>>();

    private volatile BlockingCollection<LogMessage> logMessageQueue = new();

    private ILogLevel? minimumLogLevel;

    // ┌────────────────────────────────────────────────────────────────┐
    // │ PUBLIC Constructors                                            │
    // └────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Initializes a new instance of the <see cref="Logger"/> class.
    /// </summary>
    /// <param name="name">The name of the <see cref="Logger"/>.</param>
    public Logger(string name)
    {
        StartAsync().GetAwaiter().GetResult();

        Name = name;

        AddFilter(message => message.LogLevel.CompareTo(MinimumLogLevel) >= 0);
    }

   // ┌────────────────────────────────────────────────────────────────┐
   // │ PRIVATE Constructors                                           │
   // └────────────────────────────────────────────────────────────────┘

    /// <summary>
    /// Initializes a new instance of the <see cref="Logger"/> class.
    /// </summary>
    /// <param name="name">The name of the <see cref="Logger"/>.</param>
    /// <param name="parentLog">The parent <see cref="Logger"/>.</param>
    private Logger(string name, Logger parentLog)
        : this(name)
    {
        parentLogger = parentLog;
    }

   // ┌────────────────────────────────────────────────────────────────┐
   // │ PUBLIC Properties                                              │
   // └────────────────────────────────────────────────────────────────┘

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public ILogLevel MinimumLogLevel
    {
        get => minimumLogLevel ?? parentLogger?.MinimumLogLevel ?? LogLevel.Info;
        set => minimumLogLevel = value;
    }

    // ┌────────────────────────────────────────────────────────────────┐
    // │ PUBLIC Methods                                                 │
    // └────────────────────────────────────────────────────────────────┘

    /// <inheritdoc/>
    public void Dispose()
    {
        Stop();

        WaitUntilStoppedAsync().GetAwaiter().GetResult();

        logMessageQueue.Dispose();

        isDisposed = true;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        Stop();

        await WaitUntilStoppedAsync().ConfigureAwait(false);

        logMessageQueue.Dispose();

        isDisposed = true;
    }

    /// <inheritdoc/>
    public ILogger Log(ILogLevel logLevel, object payload)
    {
        Log(new LogMessage(logLevel, payload));

        return this;
    }

    /// <inheritdoc/>
    public ILogger Debug(object payload) => Log(LogLevel.Debug, payload);

    /// <inheritdoc/>
    public ILogger Info(object payload) => Log(LogLevel.Info, payload);

    /// <inheritdoc/>
    public ILogger Warning(object payload) => Log(LogLevel.Warning, payload);

    /// <inheritdoc/>
    public ILogger Error(object payload) => Log(LogLevel.Error, payload);

    /// <inheritdoc/>
    public ILogger Fatal(object payload) => Log(LogLevel.Fatal, payload);

    /// <inheritdoc/>
    public void Flush()
    {
        FlushAsync().GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    public async Task FlushAsync()
    {
        Stop();

        await WaitUntilStoppedAsync().ConfigureAwait(false);

        await StartAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public ILogger GetLog(string name) => new Logger(name, this);

    /// <inheritdoc/>
    public void AttachLogSink(ILogSink logSink) => logSinks = logSinks.Add(logSink);

    /// <inheritdoc/>
    public void DetachLogSink(ILogSink logSink) => logSinks = logSinks.Remove(logSink);

    /// <inheritdoc/>
    public void AddFilter(Predicate<ILogMessage> filter) => logFilters = logFilters.Add(filter);

    /// <inheritdoc/>
    public void RemoveFilter(Predicate<ILogMessage> filter) => logFilters = logFilters.Remove(filter);

    /// <inheritdoc/>
    public ILogger GetLogger(string name) => new Logger(name, this);

   // ┌────────────────────────────────────────────────────────────────┐
   // │ PRIVATE Methods                                                │
   // └────────────────────────────────────────────────────────────────┘
    private Task StartAsync()
    {
        if (logTask != null)
        {
            return Task.CompletedTask;
        }

        TaskCompletionSource taskCompletionSource = new();

        logTask = Task.Run(async () => await RunAsync(taskCompletionSource).ConfigureAwait(false));

        return taskCompletionSource.Task;
    }

    private void Stop()
    {
        BlockingCollection<LogMessage> logMessageQueue = this.logMessageQueue;

        this.logMessageQueue = new BlockingCollection<LogMessage>();

        logMessageQueue.CompleteAdding();
    }

    private async Task WaitUntilStoppedAsync()
    {
        if (logTask is not null)
        {
            await logTask.ConfigureAwait(false);
            logTask = null;
        }
    }

    private void Log(LogMessage logMessage)
    {
        if (isDisposed)
        {
            throw new ObjectDisposedException(nameof(Logger));
        }

        logMessage.AddSender(Name);

        logMessageQueue.Add(logMessage);
    }

    private async Task RunAsync(TaskCompletionSource taskCompletionSource)
    {
        BlockingCollection<LogMessage> logMessageQueue = this.logMessageQueue;

        taskCompletionSource.SetResult();

        while (true)
        {
            try
            {
                LogMessage logMessage = logMessageQueue.Take();

                if (!logFilters.TrueForAll(filter => filter(logMessage)))
                {
                    continue;
                }

                parentLogger?.Log(logMessage);

                Task[] tasks = logSinks.Select(ls => ls.WriteLogMessageAsync(logMessage)).ToArray();

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (InvalidOperationException)
            {
                return;
            }
        }
    }
}
