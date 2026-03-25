

using TTSteelWebAPI.Helper;
    using TTSteelWebAPI.Interface;
    using TTSteelWebAPI.Model.Login;


namespace TTSteelWebAPI.Service
{
        public class CurrentUserService : ICurrentUserInterface
    {
            private readonly IHttpContextAccessor _httpContextAccessor;

            public CurrentUserService(IHttpContextAccessor httpContextAccessor)
            {
                _httpContextAccessor = httpContextAccessor;
            }

            public UserContext GetUser()
            {
                var user = _httpContextAccessor.HttpContext?.User;

                if (user == null)
                    return new UserContext();

                return UserContextHelper.GetUserContext(user);
            }
        }
 }


