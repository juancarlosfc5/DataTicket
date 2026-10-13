using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace DataTicket.IntegrationTests.Infrastructure;

/// <summary>Línea de log capturada: categoría, nivel y mensaje ya formateado (con la excepción, si la hay).</summary>
public sealed record CapturedLog(string Category, LogLevel Level, string Message);

/// <summary>
/// Proveedor de log en memoria para que una prueba lea lo que registra el host
/// (<see cref="ApiFactoryOptions.LoggerProvider"/>). Seguro ante concurrencia.
/// </summary>
public sealed class CapturingLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentQueue<CapturedLog> _entries = new();

    public IReadOnlyList<CapturedLog> Entries => [.. _entries];

    public ILogger CreateLogger(string categoryName) => new CapturingLogger(categoryName, _entries);

    public void Dispose()
    {
    }

    private sealed class CapturingLogger(string category, ConcurrentQueue<CapturedLog> entries) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(
            LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);
            entries.Enqueue(new CapturedLog(category, logLevel, exception is null ? message : $"{message} {exception}"));
        }
    }
}
