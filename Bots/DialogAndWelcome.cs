// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    public class DialogAndWelcomeBot<T> : DialogBot<T> where T : Dialog
    {
        public DialogAndWelcomeBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
            : base(conversationState, userState, dialog, logger)
        {
        }

        protected override async Task OnMembersAddedAsync(
            IList<ChannelAccount> membersAdded,
            ITurnContext<IConversationUpdateActivity> turnContext,
            CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                //복습 퀴즈 웰컴 메세지 - herocard 활용
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var attachments = new List<Attachment>();
                    var reply = MessageFactory.Attachment(attachments);
                    reply.Attachments.Add(Card.GetHeroCard().ToAttachment());
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
            }
        }
    }
}
