using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.App;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;

namespace AgentsSSOAuto;

public class AuthAgent : AgentApplication
{
    public AuthAgent(AgentApplicationOptions options) : base(options)
    {
        OnConversationUpdate("membersAdded", Welcome);
        OnMessage("/help", Welcome);
        OnMessage("/me", Me);
        OnMessage("/logout", Logout);
        OnActivity(ActivityTypes.Message, OnMessageActivity);
    }

    private Task Welcome(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken) =>
       turnContext.SendActivityAsync("type /me to query graph, /logout to logout or /help to see this message");
    
    private Task Logout(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken) =>  
        UserAuthorization.SignOutUserAsync(turnContext, turnState);
    
    private async Task OnMessageActivity(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
    {
        var token = UserAuthorization.GetTurnToken("graph");
        
        if (string.IsNullOrEmpty(token))
        {
            await turnContext.SendActivityAsync($"The auto sign in process failed and no access token is available", cancellationToken: cancellationToken);
            return;
        }
        var displayName = await GraphClient.GetDisplayName(token);
        await turnContext.SendActivityAsync($"**{displayName} said:** {turnContext.Activity.Text}", cancellationToken: cancellationToken);
    }

    private async Task Me(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
    {
        var token = UserAuthorization.GetTurnToken("graph");

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
