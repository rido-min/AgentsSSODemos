using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.App;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Builder.UserAuth.TokenService;
using Microsoft.Agents.Core.Models;
using Microsoft.MarkedNet;

namespace AgentsSSO;

public class AuthAgent : AgentApplication
{
    private readonly OAuthFlow _oAuthFlow;
    public AuthAgent(AgentApplicationOptions options) : base(options)
    {
        _oAuthFlow = new OAuthFlow(new OAuthSettings { AzureBotOAuthConnectionName = "SSOSelf", Text = "login", Title= "Login"});

        OnConversationUpdate("membersAdded", Welcome);
        OnMessage("/help", Welcome);
        OnMessage("/me", Me);
        OnMessage("/login", Login);
        OnMessage("/logout", Logout);
        OnActivity(ActivityTypes.Invoke, OnInvokeActivity);
        OnActivity(ActivityTypes.Message, OnMessageActivity);
    }

    private Task Welcome(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken) =>
       turnContext.SendActivityAsync("type /me to query graph, /login to login, /logout to logout or /help to see this message");

    private Task Login(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken) =>
        _oAuthFlow.BeginFlowAsync(turnContext, null, cancellationToken);

    private Task Logout(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken) =>  
        _oAuthFlow.SignOutUserAsync(turnContext, cancellationToken);

    private Task OnInvokeActivity(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken) =>
       _oAuthFlow.ContinueFlowAsync(turnContext, DateTime.UtcNow + TimeSpan.FromMinutes(5), cancellationToken);

    private async Task OnMessageActivity(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
    {
        var tokenResponse = await _oAuthFlow.ContinueFlowAsync(turnContext, DateTime.UtcNow + TimeSpan.FromMinutes(5), cancellationToken);

        if (tokenResponse != null)
        {
            await turnContext.SendActivityAsync("Login Successful, token length" + tokenResponse.Token.Length);
        }
        await turnContext.SendActivityAsync("You said: " + turnContext.Activity.Text);
    }

    private async Task Me(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
    {
        var tokenResponse = await _oAuthFlow.BeginFlowAsync(turnContext, null, cancellationToken);

        if (tokenResponse != null)
        {
            string displayName = await GraphClient.GetDisplayName(tokenResponse.Token);
            await turnContext.SendActivityAsync($"Your display name is: {displayName}");
        }
        else
        {
            await turnContext.SendActivityAsync("Token not available, login first.");
        }
    }
}
