﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Services;

namespace TASVideos.Pages.Wiki
{
	[AllowAnonymous]
	public class ViewSourceModel : BasePageModel
	{
		private readonly IWikiPages _wikiPages;

		public ViewSourceModel(IWikiPages wikiPages)
		{
			_wikiPages = wikiPages;
		}

		[FromQuery]
		public string? Path { get; set; }

		[FromQuery]
		public int? Revision { get; set; }

		public WikiPage WikiPage { get; set; } = new();

		public async Task<IActionResult> OnGet()
		{
			Path = Path?.Trim('/') ?? "";
			var wikiPage = await _wikiPages.Page(Path, Revision);

			if (wikiPage != null)
			{
				WikiPage = wikiPage;
				return Page();
			}

			return NotFound();
		}
	}
}
