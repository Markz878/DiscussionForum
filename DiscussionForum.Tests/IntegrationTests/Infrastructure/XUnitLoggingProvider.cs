namespace DiscussionForum.Tests.IntegrationTests.Infrastructure;

internal class XUnitLoggingProvider : ILoggerProvider
{
    private readonly ITestOutputHelper? testOutputHelper;
    private readonly LoggerExternalScopeProvider scopeProvider = new();

    public XUnitLoggingProvider(ITestOutputHelper? testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new XUnitLogger(testOutputHelper, scopeProvider, categoryName);
    }

    public void Dispose()
    {
    }
}

internal class XUnitLogger<T> : XUnitLogger, ILogger<T>
{
    public XUnitLogger(ITestOutputHelper? testOutputHelper, LoggerExternalScopeProvider scopeProvider)
    : base(testOutputHelper, scopeProvider, typeof(T).FullName ?? throw new ArgumentNullException("T type name"))
    {
    }
}

internal class XUnitLogger : ILogger
{
    private readonly ITestOutputHelper? _testOutputHelper;
    private readonly string _categoryName;
    private readonly LoggerExternalScopeProvider _scopeProvider;

    public static ILogger CreateLogger(ITestOutputHelper testOutputHelper)
    {
        return new XUnitLogger(testOutputHelper, new LoggerExternalScopeProvider(), "");
    }

    public static ILogger<T> CreateLogger<T>(ITestOutputHelper testOutputHelper)
    {
        return new XUnitLogger<T>(testOutputHelper, new LoggerExternalScopeProvider());
    }

    public XUnitLogger(ITestOutputHelper? testOutputHelper, LoggerExternalScopeProvider scopeProvider, string categoryName)
    {
        _testOutputHelper = testOutputHelper;
        _scopeProvider = scopeProvider;
        _categoryName = categoryName;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return _scopeProvider.Push(state);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        try
        {
            StringBuilder sb = new();
            sb.Append(GetLogLevelString(logLevel))
              .Append(" [").Append(_categoryName).Append("] ")
              .Append(formatter(state, exception));

            if (exception != null)
            {
                sb.Append('\n').Append(exception);
            }

            // Append scopes
            _scopeProvider.ForEachScope((scope, state) =>
            {
                state.Append("\n => ");
                state.Append(scope);
            }, sb);

            _testOutputHelper?.WriteLine(sb.ToString());
        }
        catch
        {
        }
    }

    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
        };
    }
}
