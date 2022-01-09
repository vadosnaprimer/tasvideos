using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TASVideos.Core.HttpClientExtensions;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Services.ExternalMediaPublisher.Distributors
{
	public class TwitterDistributor : IPostDistributor
	{
		private readonly HttpClient _client;
		private readonly AppSettings.TwitterConnection _settings;
		private readonly ILogger<TwitterDistributor> _logger;

		private static readonly Random Rng = new();

		public TwitterDistributor(
			AppSettings appSettings,
			IHttpClientFactory httpClientFactory,
			ILogger<TwitterDistributor> logger)
		{
			_settings = appSettings.Twitter;
			_client = httpClientFactory.CreateClient(HttpClients.Twitter);
			_logger = logger;
		}

		public IEnumerable<PostType> Types => new[] { PostType.Announcement };

		public async Task Post(IPostable post)
		{
			if (!_settings.IsEnabled())
			{
				return;
			}

			TwitterPost twitterPost = GenerateTwitterMessage(post);

			if (twitterPost.Text != null)
			{
				_client.SetBearerToken(_settings.AccessToken);
				var response = await _client.PostAsJsonAsync("tweets", twitterPost);

				if (!response.IsSuccessStatusCode)
				{
					if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
						response.StatusCode == System.Net.HttpStatusCode.Forbidden)
					{
						// Try again after refreshing the access token.
					}
					else
					{
						_logger.LogError($"[{DateTime.Now}] An error occurred sending a message to Twitter.");
						_logger.LogError(await response.Content.ReadAsStringAsync());
					}
				}
			}
		}

		private static TwitterPost GenerateTwitterMessage(IPostable post)
		{
			return post.Group switch
			{
				PostGroups.Submission => new TwitterPost(post),
				_ => new TwitterPost("")
			};
		}

		private class TwitterPost
		{
			public string Text { get; set; }

			public TwitterPost(string text)
			{
				this.Text = text;
			}

			public TwitterPost(IPostable post)
			{
				this.Text = $"{post.Announcement}{Environment.NewLine}{post.Link}";
			}
		}
	}
}