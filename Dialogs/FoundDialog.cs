namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.FormFlow;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class FoundDialog : IDialog<object>
    {


        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Welcome to the lost and found interface!");

            var FoundObjFormDialog = FormDialog.FromForm(this.BuildFoundObjForm, FormOptions.PromptInStart);

            context.Call(FoundObjFormDialog, this.ResumeAfterFoundObjFormDialog);
        }


        private IForm<FoundObjectQuery> BuildFoundObjForm()
        {
            OnCompletionAsyncDelegate<FoundObjectQuery> processfoundobjectsearch = async (context, state) =>
            {
                await context.PostAsync($"Ok. Verifying if the {state.ParticularObject} isn't reported after {state.Object.ToString("MM/dd")} by someone else ...");
            };

            return new FormBuilder<FoundObjectQuery>()
                .Field(nameof(FoundObjectQuery.ParticularObject))
                .Message("I'm glad to hear you find someone's {ParticularObject} back!")
                .AddRemainingFields()
                .OnCompletion(processfoundobjectsearch)
                .Build();
        }

        private async Task ResumeAfterFoundObjFormDialog(IDialogContext context, IAwaitable<FoundObjectQuery> result)
        {
            try
            {
                var searchQuery = await result;

                var FoundItemHits = await this.GetFoundObjects(searchQuery);

                await context.PostAsync($"I found in total {FoundItemHits.Count()} hits for your search criteria. Is the object you reported in the displayed items below? ");

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                foreach (var FoundItemHit in FoundItemHits)
                {
                    HeroCard heroCard = new HeroCard()
                    {
                        Title = FoundItemHit.Name,
                        Subtitle = $"Let's see what I can find for you !",
                        Images = new List<CardImage>()
                        {
                            new CardImage() { Url = FoundItemHit.Image }
                        },
                        Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "More details",
                                Type = ActionTypes.OpenUrl,
                                Value = $"https://www.bing.com/images/search?q=lost+items+" + HttpUtility.UrlEncode(FoundItemHit.Name)
                            }
                        }
                    };

                    resultMessage.Attachments.Add(heroCard.ToAttachment());
                }

                await context.PostAsync(resultMessage);
                await context.PostAsync($"Thank you for using the faciliies bot. Have a nice day!");
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation. Quitting from the LostAndFoundDialog";
                }
                else
                {
                    reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                context.Done<object>(null);
            }
        }

        private async Task<IEnumerable<LostOrFoundItem>> GetFoundObjects(FoundObjectQuery searchQuery)
        {
            var ItemsList = new List<LostOrFoundItem>();

            // Filling the hotels results manually just for demo purposes
            for (int i = 1; i <= 5; i++)
            {
                var random = new Random(i);
                LostOrFoundItem iteminlist = new LostOrFoundItem()
                {
                    Name = $"{searchQuery.ParticularObject}: Occurence {i}",
                    Location = searchQuery.ParticularObject,
                    Image = $"https://placeholdit.imgix.net/~text?txtsize=35&txt=FoundObject+{i}&w=500&h=260"
                };

                ItemsList.Add(iteminlist);
            }

            return ItemsList;


        }
    }

}