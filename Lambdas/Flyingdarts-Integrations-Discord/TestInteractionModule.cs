using System.Threading.Tasks;
using Discord.Interactions;

public class TestInteractionModule : InteractionModuleBase
{
    [SlashCommand("huur", "hmm?")]
    public async Task Huurduur()
    {
        await RespondAsync("Huuuurduuuur");
    }
}