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
    public class LostDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Welcome to the lost and found interface!");

            var LostObjFormDialog = FormDialog.FromForm(this.BuildLostObjForm, FormOptions.PromptInStart);

            context.Call(LostObjFormDialog, this.ResumeAfterLostObjFormDialog);
        }

        private IForm<LostObjectQuery> BuildLostObjForm()
        {
            OnCompletionAsyncDelegate<LostObjectQuery> processlostobjectsearch = async (context, state) =>
            {
                await context.PostAsync($"Ok. Searching for {state.ParticularObject} in our database reported after {state.Object.ToString("MM/dd")} ...");
            };

            return new FormBuilder<LostObjectQuery>()
                .Field(nameof(LostObjectQuery.ParticularObject))
                .Message("I'm sorry to hear you lost your {ParticularObject}.")
                .AddRemainingFields()
                .OnCompletion(processlostobjectsearch)
                .Build();
        }

        private async Task ResumeAfterLostObjFormDialog(IDialogContext context, IAwaitable<LostObjectQuery> result)
        {
            try
            {
                var searchQuery = await result;

                var LostItemHits = await this.GetLostObjects(searchQuery);

                await context.PostAsync($"I found in total {LostItemHits.Count()} hits for your search criteria:");

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                foreach (var LostItemHit in LostItemHits)
                {
                    HeroCard heroCard = new HeroCard()
                    {
                        Title = LostItemHit.Name,
                        Subtitle = $"Let's see what I can find for you !",
                        Images = new List<CardImage>()
                        {
                            new CardImage() { Url = LostItemHit.Image }
                        },
                        Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "More details",
                                Type = ActionTypes.OpenUrl,
                                Value = $"https://www.bing.com/images/search?q=lost+items+" + HttpUtility.UrlEncode(LostItemHit.Name)
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

        private async Task<IEnumerable<LostOrFoundItem>> GetLostObjects(LostObjectQuery searchQuery)
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
                    Image = $"https://placeholdit.imgix.net/~text?txtsize=35&txt=LostObject+{i}&w=500&h=260"
                };

                ItemsList.Add(iteminlist);
            }

            return ItemsList;
 

        }
    }
}