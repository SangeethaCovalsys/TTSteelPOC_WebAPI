using TTSteelWebAPI.Model.Login;

namespace TTSteelWebAPI.Interface
{
    public interface ICurrentUserInterface
    {
        UserContext GetUser();
    }
}
