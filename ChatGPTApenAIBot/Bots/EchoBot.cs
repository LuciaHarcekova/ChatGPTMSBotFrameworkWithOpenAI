// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.18.1

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatGPTApenAIBot.Bots
{
    public class EchoBot : ActivityHandler
    {
        private readonly IConfiguration _configuration;
        public EchoBot(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var replyText = $"Echo: {turnContext.Activity.Text}";
            string inputPromptText = turnContext.Activity.Text;
            var resultResponse = await CompletionAPIHandler(inputPromptText);
            if (resultResponse != null)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(resultResponse));
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("I'm having some trouble comminicating with our " +
                    "servers. Please try again later."), cancellationToken);
            }
        }

        private async Task<string?> CompletionAPIHandler(string inputPromptText)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(_configuration["OpenAI:CompletionEndpoint"]);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuration["OpenAI:APIKey"]}");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, client.BaseAddress);
            var body = $"{{\r\n    \"model\": \"text-davinci-003\",\r\n    \"prompt\": \"{inputPromptText}\",\r\n    \"max_tokens\": 100,\r\n    \"temperature\": 0\r\n  }}";
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            request.Content = content;

            try
            {
                var airesponse = await client.SendAsync(request).ConfigureAwait(false);
                airesponse.EnsureSuccessStatusCode();
                string responseString = await airesponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                JObject jsonObject = JObject.Parse(responseString);

                if (jsonObject != null)
                {
                    if (jsonObject["choices"][0] != null)
                    {
                        return (string)jsonObject["choices"][0]["text"];
                    }
                }

                return null;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome! I am Chat GPT chbot for the teams channel.";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
