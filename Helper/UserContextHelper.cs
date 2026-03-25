
    using System.Security.Claims;
    using TTSteelWebAPI.Model.Login;

  namespace TTSteelWebAPI.Helper
{
        public class UserContextHelper
        {
            public static UserContext GetUserContext(ClaimsPrincipal user)
            {
                return new UserContext
                {
                    Username = user.Identity?.Name,
                    Password = user.FindFirst("PW")?.Value,
                    Database = user.FindFirst("Database")?.Value
                };
            }
        }
    }


