namespace AuthAPI.Services.ResponseSerializer
{
    public interface IResponseSerializer
    {
        string Serialize(object responseData);
    }
}
