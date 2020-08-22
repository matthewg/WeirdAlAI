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
            Auth.SetCredentials(Utils.TwitterCredentials());

            long maxTweet = 0;
            long.TryParse(maxTweetIn, out maxTweet);

            log.LogInformation($"Running tweet fetch from {maxTweet}...");
            var searchParameter = new SearchTweetsParameters("\"weird AI\"")
            {
                Lang = LanguageFilter.English,
                MaximumNumberOfResults = 100,
                SearchType = SearchResultType.Recent,
                SinceId = maxTweet,
            };
            foreach (ITweet tweet in Search.SearchTweets(searchParameter))
            {
                log.LogInformation($"Got tweet: {tweet.ToString()}");
                var rtTask = tweet.PublishRetweetAsync();
                rtTask.Wait();
                log.LogInformation($"Task completed with status: {rtTask.Status}");
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
