using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.Plugin.ContentFlagger.Controllers
{
    [Route("ContentFlagger")]
    public class ContentFlaggerController : ControllerBase
    {
        private readonly ContentFlaggerPlugin _plugin;
        private readonly IApplicationPaths _appPaths;

        public ContentFlaggerController(ContentFlaggerPlugin plugin, IApplicationPaths appPaths)
        {
            _plugin = plugin;
            _appPaths = appPaths;
        }

        [HttpPost("Flag")]
        public async Task<IActionResult> FlagContent([FromBody] FlagRequest request)
        {
            if (request.ItemId == null || request.Issue == null || request.Comment == null)
            {
                return BadRequest("ItemId, Issue, and Comment cannot be null");
            }
            await _plugin.FlagContent(request.ItemId, request.Issue, request.Comment);
            return Ok();
        }

        [HttpPost("ToggleCompleted")]
        public async Task<IActionResult> ToggleCompleted([FromBody] ToggleRequest request)
        {
            if (request.Title == null || request.Issue == null || request.Timestamp == null)
            {
                return BadRequest("Title, Issue, and Timestamp cannot be null");
            }
            await _plugin.ToggleCompleted(request.Title, request.Issue, request.Timestamp);
            return Ok();
        }

        [HttpGet("Flags")]
        public async Task<IActionResult> GetFlags()
        {
            string filePath = _plugin.Configuration.FlagFilePath ?? Path.Combine(_appPaths.DataPath, "flags.csv");
            if (!System.IO.File.Exists(filePath))
                return Ok(new List<object>());

            var lines = await System.IO.File.ReadAllLinesAsync(filePath);
            var flags = lines.Select(line =>
            {
                var parts = line.Split(new[] { "\",\"" }, StringSplitOptions.None).Select(s => s.Trim('"')).ToArray();
                return new
                {
                    Title = parts[0],
                    Issue = parts[1],
                    Comment = parts[2],
                    Completed = parts[3],
                    Timestamp = parts[4]
                };
            }).ToList();
            return Ok(flags);
        }
    }

    public class FlagRequest
    {
        public string? ItemId { get; set; }
        public string? Issue { get; set; }
        public string? Comment { get; set; }
    }

    public class ToggleRequest
    {
        public string? Title { get; set; }
        public string? Issue { get; set; }
        public string? Timestamp { get; set; }
    }
}
