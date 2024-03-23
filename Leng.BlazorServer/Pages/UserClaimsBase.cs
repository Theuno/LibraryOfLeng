using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Web;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Leng.BlazorServer.Pages
{
    public class UserClaimsBase : ComponentBase
    {
        // AuthenticationStateProvider service provides the current user's ClaimsPrincipal data.
        [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; }

        protected string _authMessage;
        protected IEnumerable<Claim> _claims = Enumerable.Empty<Claim>();

        private string _msalId { get; set; } = string.Empty;

        // Defines list of claim types that will be displayed after successfull sign-in.
        //private string[] printClaims = { "name", "idp", "oid", "jobTitle", "emails" };

        private string[] printClaims = { "name", "preferred_username", "tid", "oid" };

        protected override async Task OnInitializedAsync()
        {
            await GetClaimsPrincipalData();
        }

        /// <summary>
        /// Retrieves user claims for the signed-in user.
        /// </summary>
        /// <returns></returns>
        private async Task GetClaimsPrincipalData()
        {
            // Gets an AuthenticationState that describes the current user.
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

            var user = authState.User;

            // Checks if the user has been authenticated.
            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                _msalId = user.GetMsalAccountId();
                var displayName = user.GetDisplayName();

                _authMessage = $"{displayName} ({_msalId}) is authenticated.";

                // Sets the claims value in _claims variable.
                // The claims mentioned in printClaims variable are selected only.
                _claims = user.Claims.Where(x => printClaims.Contains(x.Type));
            }
            else
            {
                _authMessage = "The user is NOT authenticated.";
            }
        }
    }
}
