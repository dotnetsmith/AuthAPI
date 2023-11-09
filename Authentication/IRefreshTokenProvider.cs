namespace AuthAPI.Authentication
{
    public interface IRefreshTokenProvider
    {
        (string, DateTime) Generate();
    }
}