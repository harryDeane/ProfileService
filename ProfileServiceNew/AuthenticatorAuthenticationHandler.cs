using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;


public class AuthenticatorAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly HttpClient _httpClient;

    public AuthenticatorAuthenticationHandler(IHttpClientFactory httpClientFactory, IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Extract credentials from the Authorization header
        string authorizationHeader = Request.Headers["Authorization"];
        string credentials = authorizationHeader.Replace("Basic ", string.Empty);
        string decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(credentials));
        string[] credentialParts = decodedCredentials.Split(':', 2);

        if (credentialParts.Length != 2)
        {
            return AuthenticateResult.Fail("Invalid Authorization header format");
        }

        string username = credentialParts[0];
        string password = credentialParts[1];

        // Make a request to the Authenticator API for user authentication
        var response = await _httpClient.GetAsync($"https://web.socem.plymouth.ac.uk/COMP2001/auth/api/users?username={username}&password={password}");

        if (response.IsSuccessStatusCode)
        {
            // Parse the response to get user information
            var userJson = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<UserModel>(userJson);

            // Fetch user roles from the Authenticator API
            var rolesResponse = await _httpClient.GetAsync($"https://web.socem.plymouth.ac.uk/COMP2001/auth/api/users/{user.UserName}/roles");
            var rolesJson = await rolesResponse.Content.ReadAsStringAsync();
            var roles = JsonConvert.DeserializeObject<List<RoleModel>>(rolesJson);

            // Create claims based on user information and roles
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            // Add other claims based on user information
        };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Role));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);

            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        return AuthenticateResult.Fail("Authentication failed");
    }
}
