﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using CsvHelper;
using log4net;
using Octokit;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace FileIssues
{
    internal class Program
    {
        private const int Succeeded = 0;
        private const int Failed = 1;

        private readonly ILog _log = LogHelper.GetLogger();
        private readonly FileIssuesOptions _options;

        private static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<FileIssuesOptions>(args)
                .MapResult(
                    options => new Program(options).Run().Result,
                    errs => Failed
                    );
        }

        private Program(FileIssuesOptions options)
        {
            _options = options;

        }

        private async Task<int> Run()
        {
            if (!File.Exists(_options.RulesFilePath))
            {
                _log.ErrorFormat(Resources.ErrorRulesFileNotFound, _options.RulesFilePath);
                return Failed;
            }

            var rulesToPort = GetRulePortingInfo();
            await CreateIssues(rulesToPort);

            return Succeeded;
        }

        private IEnumerable<PortingInfo> GetRulePortingInfo()
        {
            var rulesToPort = new List<PortingInfo>();

            var reader = new StreamReader(new FileStream(_options.RulesFilePath, System.IO.FileMode.Open));
            var csv = new CsvReader(reader);
            csv.Configuration.RegisterClassMap<PortingInfoClassMap>();

            while (csv.Read())
            {
                var portingInfo = csv.GetRecord<PortingInfo>();
                if (portingInfo.ShouldPort)
                {
                    rulesToPort.Add(portingInfo);
                }
            }

            return rulesToPort;
        }

        private async Task CreateIssues(IEnumerable<PortingInfo> rulesToPort)
        {
            IIssuesClient client = GitHub.GetIssuesClient(_options.Token);
            var existingIssues = await client.GetAllForRepository(_options.RepoOwner, _options.RepoName);

            foreach (var ruleToPort in rulesToPort.Take(5))
            {
                string title = MakeIssueTitle(ruleToPort);
                var matchingIssues = existingIssues.Where(issue => issue.Title == title);
                if (matchingIssues.Any())
                {
                    _log.WarnFormat(Resources.WarningIssueExists, ruleToPort.Id, matchingIssues.First().Number);
                }
                else
                { 
                    _log.InfoFormat(Resources.InfoFilingIssue, ruleToPort.Id);

                    NewIssue newIssue = CreateIssue(ruleToPort, title);
                    Issue issue = await client.Create(_options.RepoOwner, _options.RepoName, newIssue);

                    _log.InfoFormat(Resources.InfoIssueCreated, issue.Number);
                }
            }
        }

        private string MakeIssueTitle(PortingInfo ruleToPort)
        {
            // Don't localize this. Otherwise people with different locales would file issues
            // with different titles, and you would get duplicates.
            const string FxCopPortTitlePrefix = "Port FxCop rule";

            return $"{FxCopPortTitlePrefix} {ruleToPort.Id}: {ruleToPort.Name}";
        }

        private NewIssue CreateIssue(PortingInfo ruleToPort, string title)
        {
            const string FxCopPortLabel = "fxcop port";

            var newIssue = new NewIssue(title);
            newIssue.Labels.Add(FxCopPortLabel);

            return newIssue;
        }
    }
}
