﻿using CommandLine;

namespace FileIssues
{
    public class FileIssuesOptions
    {
        [Option(
            'f',
            "rules-file-path",
            Required = true,
            HelpText = "The path to the CSV file containing the rule porting information")]
        public string RulesFilePath { get; set; }

        [Option(
            'o',
            "repo-owner",
            Required = true,
            HelpText = "The owner of the repo in which issues should be filed")]
        public string RepoOwner { get; set; }

        [Option(
            'r',
            "repo-name",
            Required = true,
            HelpText = "The name of the repo in which issues should be filed")]
        public string RepoName { get; set; }

        [Option(
            't',
            "token",
            Required = true,
            HelpText = "Your GitHub personal access token")]
        public string Token { get; set; }
    }
}
