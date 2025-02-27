using System;
using System.ComponentModel;
using Spectre.Console.Cli;


namespace deftware.GitInfo.cli;

partial class MainCommand {

   public sealed class Settings : CommandSettings {
      [Description("Path to search. Defaults to current directory.")]
      [CommandArgument(0, "[path]")]
      public string? SearchPath { get; init; }

      [Description("Recurse subdirectories.")]
      [CommandOption("-r|--recursive")]
      [DefaultValue(false)]
      public bool RecurseSubDirectories { get; init; }

      [Description("Include all - [italic]this can be slow[/].")]
      [CommandOption("-a|--all")]
      [DefaultValue(false)]
      public bool IncludeAll { get; init; }

      [Description("Include status for each repository - [italic]this can be slow[/].")]
      [CommandOption("-s|--status")]
      [DefaultValue(false)]
      public bool IncludeStatus { get; init; }

      [Description("Also list local branches for each repository.")]
      [CommandOption("-b|--branches")]
      [DefaultValue(false)]
      public bool ListLocalBranches { get; init; }

      [Description("Also list local tags for each repository.")]
      [CommandOption("-t|--tags")]
      [DefaultValue(false)]
      public bool ListTags { get; init; }

      [Description("Also list pending changes for each repository - [italic]this can be slow[/].")]
      [CommandOption("-c|--changes")]
      [DefaultValue(false)]
      public bool ListPendingChanges { get; init; }

      [Description("Also list stashes for each repository.")]
      [CommandOption("-h|--stashes")]
      [DefaultValue(false)]
      public bool ListStashes { get; init; }

      [Description("Summarize instead of listing.")]
      [CommandOption("-u|--sum|--summarize")]
      [DefaultValue(false)]
      public bool Summarize { get; init; }

      [Description("How verbose should the output be. 0 means show only the requested data. Valid values are 0-3.")]
      [CommandOption("-v|--verbosity")]
      public int? Verbosity { get; init; }
   }
}
