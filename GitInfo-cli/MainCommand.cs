using System;
using System.ComponentModel;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Spectre.Console.Cli;


namespace deftware.GitInfo.cli;

[Description("Gits.. er.. Gets info for git repositories.")]
internal sealed partial class MainCommand : Command<MainCommand.Settings> {

   public override int Execute(CommandContext context, Settings settings) {
      LogLevel minimumLogLevel = settings.Verbosity switch
         {
            1 => LogLevel.Information,
            2 => LogLevel.Debug,
            3 => LogLevel.Trace,
            _ => LogLevel.Warning,
         };
      (ILogger mainLogger, ILogger argsLogger, ILogger listReposLogger, ILogger gitOpsLogger)
            = buildLoggers(minimumLogLevel);
      argsLogger?.LogDebug("Values from command line:  SearchPath:[{SearchPath}], RecurseSubDirectories:{RecurseSubDirectories}, ListLocalBranches:{ListLocalBranches}, Verbosity:{Verbosity}",
                           settings.SearchPath, settings.RecurseSubDirectories, settings.ListLocalBranches, settings.Verbosity);

      argsLogger?.LogDebug("Minimum log level set to: [{logLevel}]", minimumLogLevel);

      DirectoryInfo searchPathDirInfo = new DirectoryInfo(settings.SearchPath
                                                          ?? Directory.GetCurrentDirectory());
      argsLogger?.LogDebug("Search path set to: [{dir}]", searchPathDirInfo);

      mainLogger?.LogDebug("Will output data to stdout");
      TextWriter dataOut = Console.Out;

      ListRepositories.Run(searchPathDirInfo,
                           settings.RecurseSubDirectories,
                           settings.IncludeAll || settings.IncludeStatus,
                           settings.IncludeAll || settings.ListStashes,
                           settings.IncludeAll || settings.ListPendingChanges,
                           settings.IncludeAll || settings.ListTags,
                           settings.IncludeAll || settings.ListLocalBranches,
                           settings.Summarize,
                           dataOut,
                           mainLogger, listReposLogger, gitOpsLogger);

      // AnsiConsole.MarkupLine($"Total file size for [green]{searchPattern}[/] files in [green]{searchPath}[/]: [blue]{totalFileSize:N0}[/] bytes");
      return 0; // 0 means success
   }


   private static (ILogger programLogger, ILogger argsLogger, ILogger listReposLogger, ILogger gitOpsLogger) buildLoggers(LogLevel minimumLogLevel) {
      ILoggerFactory factory = buildLoggerFactory(minimumLogLevel);
      return (buildLogger(factory, "Program"),
              buildLogger(factory, "Args"),
              buildLogger(factory, "ListRepos"),
              buildLogger(factory, "GitOps"));
   }


   private static ILoggerFactory buildLoggerFactory(LogLevel minimumLogLevel)
      => LoggerFactory.Create(builder => builder
                                        .AddSimpleConsole(options => {
                                           options.IncludeScopes = true;
                                           options.SingleLine = true;
                                           // options.TimestampFormat = "HH:mm:ss ";
                                           options.TimestampFormat = "HH:mm:ss.fffffff ";
                                           options.ColorBehavior = LoggerColorBehavior.Enabled;
                                        })
                                        .SetMinimumLevel(minimumLogLevel));


   private static ILogger buildLogger(ILoggerFactory loggerFactory, string category)
      => loggerFactory.CreateLogger(category);
}
