using System.Text.Json;

namespace AuthAPI.Extensions.ResponseSerializeExtension
{
    public static class ResponseSerializer
    {
        public static string AsJSON(this object responseData)
        {
            return JsonSerializer.Serialize(responseData);
        }
    }
}
