using Newtonsoft.Json;

namespace VrcSdk;

public partial class ApiSession
{
	public async Task<dynamic> GetConfig()
	{
		var (status, responseJson) = await WebRequestApi.DoRequest(new WebRequestApi.RequestData
		{
			url = "config",
			method = HttpMethod.Get
		});
		return JsonConvert.DeserializeObject(responseJson);
	}
}