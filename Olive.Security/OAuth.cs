﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using Olive.Web;

namespace Olive.Security
{
    public class OAuth
    {
        public readonly static OAuth Instance = new OAuth();

        public readonly AsyncEvent<ExternalLoginInfo> ExternalLoginAuthenticated = new AsyncEvent<ExternalLoginInfo>();

        public async Task LogOff()
        {
            await Context.Http.SignOutAsync();
            Context.Http.Session.Perform(s => s.Clear());
        }

        public async Task LoginBy(string provider)
        {
            if (Context.Request.Query["ReturnUrl"].ToString().IsEmpty())
            {
                // it's mandatory, otherwise Challenge() immediately returns to Login page
                throw new InvalidOperationException("Request has no ReturnUrl.");
            }

            await Context.Http.ChallengeAsync(provider, new AuthenticationProperties
            {
                RedirectUri = "/ExternalLoginCallback",
                Items = { new KeyValuePair<string, string>("LoginProvider", provider) }
            });
        }

        public async Task NotifyExternalLoginAuthenticated(ExternalLoginInfo info)
        {
            if (!ExternalLoginAuthenticated.IsHandled())
                throw new InvalidOperationException("ExternalLogin requested but no handler found for ExternalLoginAuthenticated event");

            await ExternalLoginAuthenticated.Raise(info);
        }

        public ClaimsPrincipal DecodeJwt(string jwt)
        {
            if (jwt.IsEmpty()) return null;
            return new JwtSecurityTokenHandler().ValidateToken(jwt, new TokenValidationParameters(), out var token);
        }
    }
}