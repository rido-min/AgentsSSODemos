using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.App;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;

namespace AgentsSSOAuto;

public class AutoAgent : AgentApplication
{
    public AutoAgent(AgentApplicationOptions options) : base(options)
    {
        OnConversationUpdate("membersAdded", Welcome);
        OnMessage("/help", Welcome);
        OnMessage("/me", Me, autoSignInHandlers: ["graph"]);
        OnMessage("/logout", Logout);
        OnActivity(ActivityTypes.Message, OnMessageActivity, rank: RouteRank.Last, ["graph", "gh"]);
    }

    private Task Welcome(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken) =>
       turnContext.SendActivityAsync("type /me to query graph, /logout to logout or /help to see this message");
    
    private Task Logout(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken) =>  
        UserAuthorization.SignOutUserAsync(turnContext, turnState);
    
    private async Task OnMessageActivity(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
    {
        var tokenGh = await UserAuthorization.GetTurnTokenAsync(turnContext, "gh", cancellationToken: cancellationToken);
        var tokenGraph = await UserAuthorization.GetTurnTokenAsync(turnContext, "graph", cancellationToken: cancellationToken);

        if (string.IsNullOrEmpty(tokenGh))
        {
            await turnContext.SendActivityAsync($"The auto sign in process failed and no access token is available", cancellationToken: cancellationToken);
            return;
        }
        var displayName = await GraphClient.GetDisplayName(tokenGraph);
        await turnContext.SendActivityAsync($"**{displayName} said:** {turnContext.Activity.Text}", cancellationToken: cancellationToken);

        var prs = await GHClient.GetPRs(tokenGh);
        if (!string.IsNullOrEmpty(prs))
        {
            await turnContext.SendActivityAsync($"{prs}", cancellationToken: cancellationToken);
        }
        else
        {
            await turnContext.SendActivityAsync("No PRs found or you are not authorized to view them.", cancellationToken: cancellationToken);
        }

    }

    private async Task Me(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
    {
        var token = await UserAuthorization.GetTurnTokenAsync(turnContext, "graph");

        if (token != null)
        {
            string displayName = await GraphClient.GetDisplayName(token);
            await turnContext.SendActivityAsync($"Your display name is: {displayName}");
        }
        else
        {
            await turnContext.SendActivityAsync("Token not available, login first.");
        }
    }
}
