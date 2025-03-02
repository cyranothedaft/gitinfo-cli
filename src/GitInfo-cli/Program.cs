using System;
using Spectre.Console.Cli;


namespace deftware.GitInfo.cli;

internal static class Program {
   static int Main(string[] args) {
      var app = new CommandApp<MainCommand>();
      return app.Run(args);
   }
}
