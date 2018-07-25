namespace MultiDialogsBot
{
    using System;
    using Microsoft.Bot.Builder.FormFlow;

    [Serializable]
    public class LostObjectQuery
    {
        [Prompt("What {&} did you loose?")]
        public string ParticularObject { get; set; }
        
        [Prompt("When did you loose the {&}?")]
        public DateTime Object { get; set; }

        [Prompt("What {&} does it have?")]
        public string Colour { get; set; }

        // [Prompt("How many {&} do you want to stay?")]
        // public string Nights { get; set; }
    }
}