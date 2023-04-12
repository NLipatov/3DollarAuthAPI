using System.Text.Json;

namespace AuthAPI.Services.ResponseSerializer
{
    public class ResponseSerializer : IResponseSerializer
    {
        public string Serialize(object responseData)
        {
            return JsonSerializer.Serialize(responseData);
        }
    }
}
