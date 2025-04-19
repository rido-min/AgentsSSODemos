# AgentsSSODemos

There are 3 demos to compare different SSO flows

## 1. Basic

Uses `oAuthFlow` directly from the user code

## 2. Manual

Uses `UserAuthorization` to handle signin/signout, needs to use the `OnSigninSuccessEvent`

## 3. Auto

Signin automagically, users need to call GetToken to retrieve the token when needed
