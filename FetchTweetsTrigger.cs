using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Storage;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace matthewg.WeirdAlAI
{
    public static class FetchTweetsTrigger
    {
        [FunctionName("FetchTweetsTrigger")]
        public static void Run(
            [TimerTrigger("0 12 * * * *")]TimerInfo myTimer,
            [Blob("state/max-tweet", FileAccess.Read)]string maxTweetIn,
            [Blob("state/max-tweet", FileAccess.Write)]out string maxTweetOut,
            ILogger log)
        {
            var twitter = new TwitterClient(Utils.TwitterCredentials());

            long maxTweet = 0;
            long.TryParse(maxTweetIn, out maxTweet);

            log.LogInformation(message: $"Running tweet fetch from {maxTweet}...");
            var searchParameter = new SearchTweetsParameters("\"weird AI\"")
            {
                Lang = LanguageFilter.English,
                PageSize = 100,
                SearchType = SearchResultType.Recent,
                SinceId = maxTweet,
            };
            var search = twitter.Search.SearchTweetsAsync(searchParameter);
            search.Wait();
            if (!search.IsCompletedSuccessfully) {
                log.LogError(message: $"Search error: {search.Status}");
            }

            foreach (ITweet tweet in search.Result)
            {
                log.LogInformation(message: $"Got tweet: {tweet.ToString()}");
                var rtTask = twitter.Tweets.PublishRetweetAsync(tweet);
                rtTask.Wait();
                log.LogInformation(message: $"RT completed with status: {rtTask.Status}");
                if (tweet.Id > maxTweet)
                {
                    maxTweet = tweet.Id;
                }
            }
            maxTweetOut = maxTweet.ToString();
            log.LogInformation($"Finished with max tweet {maxTweetOut}.");
        }
    }
}
