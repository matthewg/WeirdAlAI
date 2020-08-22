using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;
using System.Text.Json;
using Tweetinvi;
using Tweetinvi.Models;

namespace matthewg.WeirdAlAI
{
    public static class Utils
    {  
        private const string KV_URI = "https://weirdalai-kv.vault.azure.net/";
        private const string KV_TWITTER_CRED_SECRET = "twittercreds";

        private class KeyVaultTwitterCreds
        {
            public string access_token { get; set; }
            public string access_token_secret { get; set; }
            public string api_key { get; set; }
            public string api_key_secret { get; set; }
        }

        public static ITwitterCredentials TwitterCredentials()
        {
            var secretClient = new SecretClient(new Uri(KV_URI), new DefaultAzureCredential());
            var secret = secretClient.GetSecret(KV_TWITTER_CRED_SECRET);
            var creds = System.Text.Json.JsonSerializer.Deserialize<KeyVaultTwitterCreds>(secret.Value.Value);
            return new TwitterCredentials(
                creds.api_key, creds.api_key_secret, creds.access_token, creds.access_token_secret);
        }
    }
}