using System;
using System.Collections.Generic;

namespace BenchTests
{
    /// <summary>
    /// Parses and manages command-line arguments for benchmark selection.
    /// </summary>
    public class CommandLineParser
    {
        private readonly Dictionary<string, HashSet<int>> commands = new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> aliases;

        public CommandLineParser(string[] args)
        {
            aliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "--bigint", "-b",
                    "--polynomials", "-p",
                    "--curves", "-c",
                    "--run-all", "-a",
                    "--help", "-h"
                };

            Parse(args);
        }

        private void Parse(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (!aliases.Contains(arg))
                    continue;

                if (arg.Equals("--run-all", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("-a", StringComparison.OrdinalIgnoreCase))
                {
                    commands["--run-all"] = new HashSet<int>();
                    continue;
                }

                if (arg.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("-h", StringComparison.OrdinalIgnoreCase))
                    continue;

                var indices = new HashSet<int>();
                int j = i + 1;

                while (j < args.Length && !aliases.Contains(args[j]))
                {
                    if (int.TryParse(args[j], out int index))
                        indices.Add(index);
                    j++;
                }

                commands[arg] = indices;
            }
        }

        public bool HasCommand(string longForm, string shortForm)
        {
            return commands.ContainsKey(longForm) || commands.ContainsKey(shortForm);
        }

        public HashSet<int> GetTestIndices(string longForm, string shortForm)
        {
            if (commands.TryGetValue(longForm, out var indices))
                return indices;

            if (commands.TryGetValue(shortForm, out indices))
                return indices;

            return new HashSet<int>();
        }
    }
}
