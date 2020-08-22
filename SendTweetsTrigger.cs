using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace matthewg.WeirdAlAI
{
    public static class SendTweetsTrigger
    {
        [FunctionName("SendTweetsTrigger")]
        public static async Task Run([EventHubTrigger("sendtweets", Connection = "SendTweetsHubRead")] EventData[] events, ILogger log)
        {
            var exceptions = new List<Exception>();
            var twitter = new TwitterClient(Utils.TwitterCredentials());
            log.LogInformation("Starting up");

            foreach (EventData eventData in events)
            {
                string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                try
                {
                    // Replace these two lines with your processing logic.
                    log.LogInformation($"Got tweet to RT: {messageBody}");
                    var rtTask = twitter.Tweets.PublishRetweetAsync(Int32.Parse(messageBody));
                    rtTask.Wait();
                    log.LogInformation($"RT'd with status: {rtTask.Status}");
                    Thread.Sleep(millisecondsTimeout: 10000);
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    log.LogError($"Couldn't RT '{messageBody}': {e}");
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }
    }
}
