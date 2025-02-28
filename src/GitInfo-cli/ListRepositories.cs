using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;


namespace deftware.GitInfo.cli;

internal static class ListRepositories {
   public static void Run(DirectoryInfo searchPathDirInfo,
                          bool recursive, bool includeStatus, bool includeStashes, bool includePendingChanges, bool includeTags, bool includeLocalBranches, bool summarize,
                          TextWriter dataOut, ILogger programLogger, ILogger listReposLogger, ILogger gitLogger) {

      string[] excludeDirNames = [".vs", "bin", "obj", "$RECYCLE.BIN"];

      listReposLogger?.LogDebug("Will{orNot} recurse subdirectories to find repositories", recursive             ? string.Empty : " not");
      listReposLogger?.LogDebug("Will{orNot} include status"                             , includeStatus         ? string.Empty : " not");
      listReposLogger?.LogDebug("Will{orNot} also list pending changes"                  , includePendingChanges ? string.Empty : " not");
      listReposLogger?.LogDebug("Will{orNot} also list stashes"                          , includeStashes        ? string.Empty : " not");
      listReposLogger?.LogDebug("Will{orNot} also list tags"                             , includeTags           ? string.Empty : " not");
      listReposLogger?.LogDebug("Will{orNot} also list local branches"                   , includeStashes        ? string.Empty : " not");
      listReposLogger?.LogDebug("Will {listOrSummarize}", summarize ? "summarize instead of listing" : "list instead of summarizing");
      if (excludeDirNames.Any())
         listReposLogger?.LogDebug("Will exclude directories having any of these {count} names:  {dirNames}", excludeDirNames.Length, excludeDirNames);
      else
         listReposLogger?.LogDebug("Will not exclude any directories from the search");

      var excludeDirsHashSet = new HashSet<string>(excludeDirNames);

      if (recursive) {
         enumerateTree(searchPathDirInfo,
                       dir => {
                          bool isRepo = emitDirIfIsRepo(dir);
                          bool pruneHere = isRepo;
                          //bool isExcluded = isExcludedDir(dir);
                          //bool isRepo = !isExcluded && emitDirIfIsRepo(dir);
                          //bool pruneHere = isExcluded || isRepo;
                          return pruneHere;
                       },
                       excludeCertainDirs,
                       listReposLogger);
      }
      else {
         emitDirIfIsRepo(searchPathDirInfo);
      }

      return;


      bool excludeCertainDirs(DirectoryInfo dir) {
         bool include = !excludeDirsHashSet.Contains(dir.Name);
         if (!include) listReposLogger?.LogTrace("Excluding directory by name: [{dir}]", dir);
         return include;
      }


      bool emitDirIfIsRepo(DirectoryInfo dirInfo) {
         bool isRepo = GitOps.IsValidRepository(dirInfo.FullName, gitLogger);
         listReposLogger?.LogDebug("[{dir}] {isOrNot} a valid repository.  {emitOrNot}",
                                   isRepo ? "is" : "is not", dirInfo, isRepo ? "emitting..." : "not emitting");

         if (isRepo) {
            string repositoryPath = dirInfo.FullName;
            emitRepository(dataOut, repositoryPath, includeStatus, includePendingChanges, includeStashes, includeTags, includeLocalBranches, summarize,
                          listReposLogger, gitLogger);
         }

         return isRepo;
      }
   }


   private static void emitRepository(TextWriter dataOut, string repositoryPath,
                                      bool includeStatus, bool includePendingChanges, bool includeStashes, bool includeTags, bool includeLocalBranches, bool summarize,
                                      ILogger? listReposLogger, ILogger? gitLogger) {
      Repository repository = new Repository(repositoryPath);
      dataOut.WriteRepository(repositoryPath, repository, includeStatus, includePendingChanges, includeStashes, includeTags, includeLocalBranches, summarize,
                              listReposLogger, gitLogger);
   }


   private static void enumerateTree(DirectoryInfo fromDirectory, Func<DirectoryInfo, bool> actAndQueryPruneHereFunc, 
                                     Func<DirectoryInfo, bool>? includeFunc = null, ILogger? logger = null) {
      logger?.LogTrace("directory [{dir}] - enumerating...", fromDirectory);
      bool pruneHere = actAndQueryPruneHereFunc(fromDirectory);
      logger?.LogTrace("directory [{dir}] - enumerated. pruneHere? {recurseDeeper}", fromDirectory, pruneHere);

      if (!pruneHere) {
         logger?.LogTrace("directory [{dir}] - operating on subdirectories...", fromDirectory);
         IEnumerable<DirectoryInfo> accessibleSubDirs = fromDirectory.SafeEnumerateDirectories("*", SearchOption.TopDirectoryOnly, includeFunc, logger);
         foreach (DirectoryInfo subDirInfo in accessibleSubDirs) {
            enumerateTree(subDirInfo, actAndQueryPruneHereFunc, includeFunc, logger); // recursive call!
         }
      }
   }


   private static (string joined, int count) joinStringAndCount(string delimiter, IEnumerable<string> stringsToJoin)
      => stringsToJoin.Aggregate(seed: (new StringBuilder(), 0),
                                 ((StringBuilder sb, int count) accumulated, string next)
                                       => (accumulated.sb.AppendIf(accumulated.count > 0, delimiter)
                                                         .Append(next),
                                           accumulated.count + 1),
                                 ((StringBuilder sb, int count) result)
                                       => (result.sb.ToString(),
                                           result.count));


   private static void WriteRepository(this TextWriter writer, string repositoryPath, Repository repository,
                                       bool includeStatus, bool includePendingChanges, bool includeStashes, bool includeTags, bool includeLocalBranches, bool summarize,
                                       ILogger? listReposLogger, ILogger? gitLogger) {

      var repositoryStatusLoader = new Lazy<RepositoryStatus>(() => GitOps.Repository_RetrieveStatus(repository, gitLogger));
      RepositoryStatus getRepoStatus() => repositoryStatusLoader.Value;

      var repositoryBranchesLoader = new Lazy<ImmutableList<Branch>>(() => GitOps.Repository_RetrieveBranches(repository, gitLogger).ToImmutableList());
      ImmutableList<Branch> getBranches() => repositoryBranchesLoader.Value;

      writer.WriteLine("Repository [{0}]{1}", repositoryPath,
                       addDetails(getRepoDetails()));

      if (includePendingChanges) writePendingChanges();
      if (includeStashes) writeStashes();
      if (includeTags) writeTags();
      if (includeLocalBranches) writeBranches();

      return;


      IEnumerable<string> getRepoDetails() {
         yield return $"atBranch: {repository.Head.FriendlyName}";
         if (includeStatus)
            yield return $"isDirty:{getRepoStatus().IsDirty}";
      }


      void writePendingChanges() {
         RepositoryStatus repositoryStatus = getRepoStatus();
         FileStatus flagsToIgnore = FileStatus.Ignored
                                  | FileStatus.Unaltered;
         IEnumerable<StatusEntry> pendingChangeStatuses = repositoryStatus.Where(s => (s.State & ~flagsToIgnore) != 0);

         if (summarize) {
            writer.WriteLine("Repository [{0}] :: pending changes: {1}", repositoryPath, pendingChangeStatuses.Count());
         }
         else {
            int count = list(pendingChangeStatuses, statusEntry => writer.WriteLine("   file: {0} :: {1}", statusEntry.FilePath, statusEntry.State));
            listReposLogger?.LogDebug("Repository [{repoPath}]: Listed {count} pending changes", repository.Info.Path, count);
         }
      }


      void writeStashes() {
         StashCollection stashes = GitOps.Repository_RetrieveStashes(repository, gitLogger);

         if (summarize) {
            writer.WriteLine("Repository [{0}] :: stashes: {1}", repositoryPath, stashes.Count());
         }
         else {
            int count = list(stashes, stash => writer.WriteLine("   stash: {0} :: message: \"{1}\"", stash.FriendlyName, stash.Message));
            listReposLogger?.LogDebug("Repository [{repoPath}]: Listed {count} stashes", repository.Info.Path, count);
         }
      }


      void writeTags() {
         TagCollection tags = repository.Tags;

         if (summarize) {
            writer.WriteLine("Repository [{0}] :: tags: {1}", repositoryPath, tags.Count());
         }
         else {
            int count = list(tags, tag => writer.WriteLine("   tag: {0}", tag.FriendlyName));
            listReposLogger?.LogDebug("Repository [{repoPath}]: Listed {count} tags", repository.Info.Path, count);
         }
      }


      void writeBranches() {
         ImmutableList<Branch> branches = getBranches();
         IEnumerable<Branch> localBranches = branches.Where(b => !b.IsRemote);

         if (summarize) {
            writer.WriteLine("Repository [{0}] :: local branches: {1}", repositoryPath, localBranches.Count());
         }
         else {
            int count = list(localBranches, writeBranch);
            listReposLogger?.LogDebug("Repository [{repoPath}]: Listed {count} branches", repository.Info.Path, count);
         }
      }


      void writeBranch(Branch branch)
         => writer.WriteLine("   branch: {0} {1}",
                             formatBranchFlags(branch),
                             branch.FriendlyName
                             // branch.IsTracking
                             //       ? " :: common ancestor: " + (branch.TrackingDetails
                             //                                         ?.CommonAncestor
                             //                                         ?.Id.Sha[..8]
                             //                                         ?? "(none)")
                             //       : string.Empty
                            );

   }


   private static string addDetails(IEnumerable<string> detailParts)
      => string.Concat(detailParts.Select(d => " :: " + d));


   private static string formatBranchFlags(Branch branch)
      => (branch.IsTracking ? "t" : "-")
       + ((branch.TrackingDetails?.BehindBy ?? 0) > 0 ? "b" : "=")
       + ((branch.TrackingDetails?.AheadBy  ?? 0) > 0 ? "a" : "=")
         ;


   private static int list<T>(IEnumerable<T> items, Action<T> listAction) {
      int count = 0;
      foreach (T item in items) {
         ++count;
         listAction(item);
      }
      return count;
   }
}



public static class Extensions {
   public static string Truncate(this string s, int desiredLength)
      => s.Length <= desiredLength
               ? s
               : s[..(desiredLength - 1)] + '…';
}
