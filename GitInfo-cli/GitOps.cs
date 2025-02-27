using System;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;


namespace deftware.GitInfo.cli;

public static class GitOps {
   public static bool IsValidRepository(string directoryPath, ILogger? logger) {
      logger?.LogTrace("Calling Repository.IsValid for directory [{dir}]", directoryPath);
      bool isValidRepo = Repository.IsValid(directoryPath);
      logger?.LogTrace("Called Repository.IsValid for directory [{dir}]: isValidRepo: {isValidRepo}", directoryPath, isValidRepo);
      return isValidRepo;
   }


   public static RepositoryStatus Repository_RetrieveStatus(Repository repository, ILogger? logger) {
      logger?.LogTrace("Repository [{repoPath}]: Retrieving status...", repository.Info.Path);
      RepositoryStatus status = time(repository.RetrieveStatus, out long elapsed_ticks); // can be slow!
      logger?.LogTrace("Repository [{repoPath}]: Retrieved status in {sec:F5} seconds", repository.Info.Path, TimeSpan.FromTicks(elapsed_ticks).TotalSeconds);
      return status;
   }


   public static StashCollection Repository_RetrieveStashes(Repository repository, ILogger? logger) {
      logger?.LogTrace("Repository [{repoPath}]: Retrieving stashes...", repository.Info.Path);
      StashCollection stashes = time(() => repository.Stashes, out long elapsed_ticks);
      logger?.LogTrace("Repository [{repoPath}]: Retrieved stashes in {sec:F5} seconds", repository.Info.Path, TimeSpan.FromTicks(elapsed_ticks).TotalSeconds);
      return stashes;
   }


   public static BranchCollection Repository_RetrieveBranches(Repository repository, ILogger? logger) {
      logger?.LogTrace("Repository [{repoPath}]: Retrieving branches...", repository.Info.Path);
      BranchCollection branches = time(() => repository.Branches, out long elapsed_ticks);
      logger?.LogTrace("Repository [{repoPath}]: Retrieved branches in {sec:F5} seconds", repository.Info.Path, TimeSpan.FromTicks(elapsed_ticks).TotalSeconds);
      return branches;
   }


   private static TResult time<TResult>(Func<TResult> func, out long elapsed_ticks) {
      long start_ticks = DateTime.Now.Ticks;
      TResult result = func();
      elapsed_ticks = DateTime.Now.Ticks - start_ticks;
      return result;
   }
}
