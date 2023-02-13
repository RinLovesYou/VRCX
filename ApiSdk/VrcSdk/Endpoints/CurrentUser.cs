using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace VrcSdk;

public partial class ApiSession
{
	public async Task<bool> GetCurrentUser()
	{
		var (status, responseJson) = await WebRequestApi.DoRequest(new WebRequestApi.RequestData
		{
			url = "auth/user",
			method = HttpMethod.Get
		});
		if (status == HttpStatusCode.OK)
		{
			return true;
		}
		return false;
	}
}