using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.App;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Builder.UserAuth.TokenService;
using Microsoft.Agents.Core.Models;

namespace AgentsSSO;

public class AuthAgent : AgentApplication
{
    public AuthAgent(AgentApplicationOptions options) : base(options)
    {
        OnConversationUpdate("membersAdded", Welcome);
        OnMessage("/help", Welcome);
        OnMessage("/me", Me);
        OnMessage("/login", Login);
        OnMessage("/logout", Logout);
        OnActivity(ActivityTypes.Message, OnMessageActivity);
        UserAuthorization.OnUserSignInSuccess(OnUserSignInSuccess);
    }

    private async Task OnUserSignInSuccess(ITurnContext turnContext, ITurnState turnState, string handlerName, string token, IActivity initiatingActivity, CancellationToken cancellationToken)
    {
        if (handlerName == "graph")
        {
            string displayName = await GraphClient.GetDisplayName(token);
            await turnContext.SendActivityAsync($"**{displayName} said:** {initiatingActivity.Text}", cancellationToken: cancellationToken);
        }
    }

    private Task Welcome(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken) =>
       turnContext.SendActivityAsync("type /me to query graph, /login to login, /logout to logout or /help to see this message");

    private Task Login(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken) =>
        UserAuthorization.SignInUserAsync(turnContext, turnState, "graph");

    private Task Logout(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken) =>
        UserAuthorization.SignOutUserAsync(turnContext, turnState, "graph");

    private async Task OnMessageActivity(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
    {
        await UserAuthorization.SignInUserAsync(turnContext, turnState, "graph", cancellationToken: cancellationToken);
    }

    private async Task Me(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
    {
        var tokenResponse = UserAuthorization.GetTurnToken("graph");

        if (tokenResponse != null)
        {
            string displayName = await GraphClient.GetDisplayName(tokenResponse);
            await turnContext.SendActivityAsync($"Your display name is: {displayName}");
        }
        else
        {
            await turnContext.SendActivityAsync("Token not available, login first.");
        }
    }
}
