using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;


namespace deftware.GitInfo.cli;

public static class DirectoryInfoExtensions {
   public static IEnumerable<DirectoryInfo> SafeEnumerateDirectories(this DirectoryInfo dir, string pattern, SearchOption searchOption,
                                                                     Func<DirectoryInfo, bool>? includeFunc = null, ILogger? logger = null) {
      try {
         var subdirs = dir.EnumerateDirectories(pattern, searchOption);
         return includeFunc is null
                      ? subdirs
                      : subdirs.Where(includeFunc);
      }
      catch (Exception exception) {
         logger?.LogWarning(exception, "Cannot list subdirectories under [{dir}] - skipping it.", dir);
         return Enumerable.Empty<DirectoryInfo>();
      }
   }


   public static StringBuilder AppendIf(this StringBuilder sb, bool condition, string appendee)
      => condition
               ? sb.Append(appendee)
               : sb;
}
