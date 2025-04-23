using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.App;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Builder.UserAuth.TokenService;
using Microsoft.Agents.Core.Models;
using Microsoft.MarkedNet;
using System.Text.RegularExpressions;

namespace AgentsSSOBasic;

public class BasicAgent : AgentApplication
{
    private readonly OAuthFlow _oAuthFlow;
    private readonly OAuthFlow _ghTokenProvider;
    public BasicAgent(AgentApplicationOptions options) : base(options)
    {
        _oAuthFlow = new OAuthFlow(new OAuthSettings { AzureBotOAuthConnectionName = "SSOSelf", Text = "login", Title= "Login"});
        _ghTokenProvider = new OAuthFlow(new OAuthSettings { AzureBotOAuthConnectionName = "GH", Text = "login in GH", Title = "Login" });

        OnConversationUpdate("membersAdded", Welcome);
        OnMessage("/help", Welcome);
        OnMessage("/me", Me);
        OnMessage("/login", Login);
        OnMessage("/logout", Logout);
        OnMessage("/prs", Prs);
        OnActivity(ActivityTypes.Invoke, OnInvokeActivity);
        OnActivity(ActivityTypes.Message, OnMessageActivity);
    }

    private async Task Prs(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
    {
        
       var ghRokenResponse =  await _ghTokenProvider.BeginFlowAsync(turnContext, null, cancellationToken);
       if (ghRokenResponse != null)
        {
            string prs = await GHClient.GetPRs(ghRokenResponse.Token);
            await turnContext.SendActivityAsync($"{prs}");
        }
    }

    private Task Welcome(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken) =>
       turnContext.SendActivityAsync("type /me to query graph, /login to login, /logout to logout or /help /prs to see this message");

    private Task Login(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken) =>
        _oAuthFlow.BeginFlowAsync(turnContext, null, cancellationToken);

    private async Task Logout(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
    {
        await _ghTokenProvider.SignOutUserAsync(turnContext, cancellationToken);
        await _oAuthFlow.SignOutUserAsync(turnContext, cancellationToken);
        
    }

    private Task OnInvokeActivity(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken) =>
       _oAuthFlow.ContinueFlowAsync(turnContext, DateTime.UtcNow + TimeSpan.FromMinutes(5), cancellationToken);

    private async Task OnMessageActivity(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
    {
        if (Regex.Match(turnContext.Activity.Text, @"(\d{6})").Success)
        {
            var tokenResponse = await _oAuthFlow.ContinueFlowAsync(turnContext, DateTime.UtcNow + TimeSpan.FromMinutes(5), cancellationToken);
            if (tokenResponse != null)
            {
                string displayName = await GraphClient.GetDisplayName(tokenResponse.Token);
                await turnContext.SendActivityAsync("Your display name is: " + displayName);
            }

            var ghRokenResponse = await _ghTokenProvider.ContinueFlowAsync(turnContext, DateTime.UtcNow + TimeSpan.FromMinutes(5), cancellationToken);
            if (ghRokenResponse != null)
            {
                string prs = await GHClient.GetPRs(ghRokenResponse.Token);
                await turnContext.SendActivityAsync($"{prs}");
            }
        }
        else
        {
            var tokenResponse = await _oAuthFlow.BeginFlowAsync(turnContext, null, cancellationToken);
            string displayName = await GraphClient.GetDisplayName(tokenResponse.Token);
            await turnContext.SendActivityAsync($"**{displayName} said:** {turnContext.Activity.Text}", cancellationToken: cancellationToken);
        }
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
