using Serilog;
using Serilog.Events;

namespace BudgetApp.Shared.Tools;

public static class SerilogConfiguration
{
    public static void Configure(string serviceName)
    {
        var logDirectory = @"C:\logs\" + serviceName;
        var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss};{Level};{Message}{NewLine}";
        var rollingInterval = RollingInterval.Day;
        var flushInterval = TimeSpan.FromSeconds(1);
        var fileSizeLimit = 1048576; // 1 Mo
        var retainedFileCountLimit = 31;

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console(outputTemplate: outputTemplate)
            .WriteTo.Debug()
            .WriteTo.File(
                Path.Combine(logDirectory, "global-.log"),
                outputTemplate: outputTemplate,
                rollingInterval: rollingInterval,
                shared: true,
                flushToDiskInterval: flushInterval,
                buffered: false,
                fileSizeLimitBytes: fileSizeLimit,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: retainedFileCountLimit)

            // Logs par niveau
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Verbose)
                .WriteTo.File(Path.Combine(logDirectory, "verbose-.log"),
                    outputTemplate: outputTemplate,
                    rollingInterval: rollingInterval,
                    shared: true,
                    flushToDiskInterval: flushInterval,
                    buffered: false,
                    fileSizeLimitBytes: fileSizeLimit,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: retainedFileCountLimit))

            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug)
                .WriteTo.File(Path.Combine(logDirectory, "debug-.log"),
                    outputTemplate: outputTemplate,
                    rollingInterval: rollingInterval,
                    shared: true,
                    flushToDiskInterval: flushInterval,
                    buffered: false,
                    fileSizeLimitBytes: fileSizeLimit,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: retainedFileCountLimit))

            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information)
                .WriteTo.File(Path.Combine(logDirectory, "info-.log"),
                    outputTemplate: outputTemplate,
                    rollingInterval: rollingInterval,
                    shared: true,
                    flushToDiskInterval: flushInterval,
                    buffered: false,
                    fileSizeLimitBytes: fileSizeLimit,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: retainedFileCountLimit))

            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
                .WriteTo.File(Path.Combine(logDirectory, "warning-.log"),
                    outputTemplate: outputTemplate,
                    rollingInterval: rollingInterval,
                    shared: true,
                    flushToDiskInterval: flushInterval,
                    buffered: false,
                    fileSizeLimitBytes: fileSizeLimit,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: retainedFileCountLimit))

            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error)
                .WriteTo.File(Path.Combine(logDirectory, "error-.log"),
                    outputTemplate: outputTemplate,
                    rollingInterval: rollingInterval,
                    shared: true,
                    flushToDiskInterval: flushInterval,
                    buffered: false,
                    fileSizeLimitBytes: fileSizeLimit,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: retainedFileCountLimit))

            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Fatal)
                .WriteTo.File(Path.Combine(logDirectory, "fatal-.log"),
                    outputTemplate: outputTemplate,
                    rollingInterval: rollingInterval,
                    shared: true,
                    flushToDiskInterval: flushInterval,
                    buffered: false,
                    fileSizeLimitBytes: fileSizeLimit,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: retainedFileCountLimit))
            .CreateLogger();
    }
}