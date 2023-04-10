using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Identity.Web;

namespace Leng.Application.Services
{
    public static class LengAuthenticationService
    {
        public static string getMsalId(AuthenticationState authState)
        {
            var _msalId = string.Empty;

            // Checks if the user has been authenticated.
            if (authState.User.Identity.IsAuthenticated)
            {
                _msalId = authState.User.GetMsalAccountId();
            }

            return _msalId;
        }
    }
}
