using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using UnityEngine;

namespace LingoEngine.Unity.Core
{
    /// <summary>
    /// Simple logger that forwards messages to Unity's <see cref="Debug"/> class.
    /// </summary>
    public class UnityLogger : ILogger
    {
        private readonly string _categoryName;

        public UnityLogger(string categoryName)
        {
            _categoryName = categoryName;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            string message = formatter(state, exception);
            if (exception != null)
                message += $" Exception: {exception}";

            string logLine = $"[{_categoryName}] [{logLevel}] {message}";

            switch (logLevel)
            {
                case LogLevel.Critical:
                case LogLevel.Error:
                    Debug.LogError(logLine);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(logLine);
                    break;
                default:
                    Debug.Log(logLine);
                    break;
            }
        }
    }

    public class UnityLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new UnityLogger(categoryName);

        public void Dispose() { }
    }

    public static class IoCRegistration
    {
        public static IServiceCollection AddUnityLogging(this IServiceCollection services)
        {
            services.AddSingleton<ILoggerProvider, UnityLoggerProvider>();
            services.AddSingleton<ILoggerFactory>(sp =>
            {
                var provider = sp.GetRequiredService<ILoggerProvider>();
                var factory = LoggerFactory.Create(builder =>
                {
                    builder.ClearProviders();
                    builder.AddProvider(provider);
                });
                return factory;
            });

            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

            return services;
        }
    }
}
