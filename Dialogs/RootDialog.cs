﻿namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using System.Text.RegularExpressions;

    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private const string FoundObjext = "I Found an Object";

        private const string LostObject = "I Lost an Object";

        private const string OtherOption = "Other";

        private const string FacilitiesIssue = "Asset of this Building Broke Down.";


        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (message.Text.ToLower().Contains("help") || message.Text.ToLower().Contains("support") || message.Text.ToLower().Contains("problem"))
            {
                await context.Forward(new SupportDialog(), this.ResumeAfterSupportDialog, message, CancellationToken.None);
            }
            if (message.Text.ToLower().Contains("hi") || message.Text.ToLower().Contains("hello") || message.Text.ToLower().Contains("hey"))
            {
                await context.PostAsync($"¯/_(ツ)_/¯ Hi. I'm the facilities bot.");
                this.ShowOptions(context);
            }
            else
            {
                this.ShowOptions(context);
            }
        }

        private void ShowOptions(IDialogContext context)
        {
            PromptDialog.Choice(context, this.OnOptionSelected, new List<string>() { FoundObjext, LostObject, OtherOption, FacilitiesIssue }, "How can I help you?", "Not a valid option", 3);
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case FoundObjext:
                        context.Call(new FoundDialog(), this.ResumeAfterOptionDialog);
                        break;

                    case LostObject:
                        context.Call(new LostDialog(), this.ResumeAfterOptionDialog);
                        break;

                    case FacilitiesIssue:
                        context.Call(new FacilitiesProblem(), this.ResumeAfterOptionDialog);
                        break;

                    case OtherOption:
                        context.Call(new FacilitiesProblem(), this.ResumeAfterOptionDialog);
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync($"Ooops! Too many attempts :(. But don't worry, I'm handling that exception and you can try again!");

                context.Wait(this.MessageReceivedAsync);
            }
        }

        private async Task ResumeAfterSupportDialog(IDialogContext context, IAwaitable<int> result)
        {
            var ticketNumber = await result;

            await context.PostAsync($"Thanks for contacting our support team. Your ticket number is {ticketNumber}.");
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task ResumeAfterOptionDialog(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var message = await result;
            }
            catch (Exception ex)
            {
                await context.PostAsync($"Failed with message: {ex.Message}");
            }
            finally
            {
                context.Wait(this.MessageReceivedAsync);
            }
        }
    }
}
