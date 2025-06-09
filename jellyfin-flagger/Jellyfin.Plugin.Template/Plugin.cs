using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using System.Collections.Generic;
using System.Linq; // Added for Select

namespace Jellyfin.Plugin.ContentFlagger
{
    public class ContentFlaggerPlugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        private readonly ILogger<ContentFlaggerPlugin> _logger;
        private readonly IApplicationPaths _appPaths;
        private readonly ILibraryManager _libraryManager;

        public ContentFlaggerPlugin(IApplicationPaths appPaths, IXmlSerializer xmlSerializer, ILogger<ContentFlaggerPlugin> logger, ILibraryManager libraryManager)
            : base(appPaths, xmlSerializer)
        {
            _logger = logger;
            _appPaths = appPaths;
            _libraryManager = libraryManager;
        }

        public override string Name => "Content Flagger";
        public override Guid Id => new Guid("f045375e-f67f-4e6e-babc-2f9d0f883d05");

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "ContentFlagger",
                    EmbeddedResourcePath = GetType().Namespace + ".Web.contentflagger.js"
                },
                new PluginPageInfo
                {
                    Name = "ContentFlaggerAdmin",
                    EmbeddedResourcePath = GetType().Namespace + ".Web.admin.html"
                }
            };
        }

        public async Task FlagContent(string itemId, string issue, string comment)
        {
            var item = _libraryManager.GetItemById(Guid.Parse(itemId));
            string title = item?.Name ?? "Unknown";
            string filePath = Configuration.FlagFilePath ?? Path.Combine(_appPaths.DataPath, "flags.csv");

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            string line = $"\"{title}\",\"{issue}\",\"{comment}\",\"N\",\"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}\"";
            await File.AppendAllTextAsync(filePath, line + Environment.NewLine);
            _logger.LogInformation($"Flagged: {title}, Issue: {issue}, Comment: {comment}");
        }

        public async Task ToggleCompleted(string title, string issue, string timestamp)
        {
            string filePath = Configuration.FlagFilePath ?? Path.Combine(_appPaths.DataPath, "flags.csv");
            var lines = await File.ReadAllLinesAsync(filePath);
            for (int i = 0; i < lines.Length; i++)
            {
                var parts = ParseCsvLine(lines[i]);
                if (parts[0] == title && parts[1] == issue && parts[4] == timestamp)
                {
                    parts[3] = parts[3] == "N" ? "Y" : "N";
                    lines[i] = $"\"{parts[0]}\",\"{parts[1]}\",\"{parts[2]}\",\"{parts[3]}\",\"{parts[4]}\"";
                }
            }
            await File.WriteAllLinesAsync(filePath, lines);
        }

        private string[] ParseCsvLine(string line)
        {
            return line.Split(new[] { "\",\"" }, StringSplitOptions.None).Select(s => s.Trim('"')).ToArray();
        }
    }

    public class PluginConfiguration : BasePluginConfiguration
    {
        public string? FlagFilePath { get; set; } // Made nullable
        public string[] IssueCategories { get; set; } = new[] { "Missing or Wrong CC", "Bad Audio", "Wrong Info" };
    }
}
