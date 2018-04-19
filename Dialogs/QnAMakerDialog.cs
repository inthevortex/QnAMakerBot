using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using static QnAMaker.SSMLHelper;
using static QnAMaker.Helper;

namespace QnAMaker
{
    [Serializable]
    public class QnAMakerDialog : LuisDialog<object>
    {
        public QnAMakerDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"], 
            ConfigurationManager.AppSettings["LuisAPIKey"], 
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
        }

        [LuisIntent("")]
        [LuisIntent("None")]
        [LuisIntent("Questions")]
        public async Task WelcomeIntent(IDialogContext context, LuisResult result)
        {
            var message = context.MakeMessage();
            var moderationRequired = await CallContentModeratorAsync(result.Query);

            if (!moderationRequired)
            {
                var correctedQuery = await CallBingSpellCheckAsync(result.Query);
                var res = await CallQnAMakerAsync(correctedQuery);

                if (res == "No good match found in the KB")
                {
                    var searchList = await CallBingSearchAsync(correctedQuery);
                    var buttons = new List<CardAction>();

                    foreach (var item in searchList)
                        buttons.Add(new CardAction(ActionTypes.OpenUrl, title: item.Name, value: item.Url));

                    var card = new HeroCard(title: "Online Search Results", text: "The search results are as follows:", buttons: buttons);

                    message.Attachments = new List<Attachment>()
                {
                    card.ToAttachment()
                };
                    message.Speak = Speak("Online Search Results");
                }
                else
                {
                    message.Speak = Speak(res);
                    message.Text = res;
                    message.InputHint = InputHints.ExpectingInput;
                } 
            }
            else
            {
                message.Speak = Speak("Please do not use offensive or explicit language.");
                message.Text = "Please do not use offensive or explicit language.";
            }

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }
    }
}