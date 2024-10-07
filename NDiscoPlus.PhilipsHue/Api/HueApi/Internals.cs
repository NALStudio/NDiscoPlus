using NDiscoPlus.PhilipsHue.Api.Models.RequestBuilders.Base;
using NDiscoPlus.PhilipsHue.Api.Models.Responses;
using NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
using NDiscoPlus.PhilipsHue.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.HueApi;
public partial class LocalHueApi
{
    private async Task<TResp> GetAsync<TResp>(string url)
    {
        HttpResponseMessage response = await http.GetAsync(url);
        return await HandleResponse<TResp>(response);
    }

    /// <summary>
    /// Fetch data as an array and expect it to only contain 0-1 items.
    /// </summary>
    private async Task<TResp?> GetSingleOrDefaultAsync<TResp>(string url)
    {
        ImmutableArray<TResp> resps = await GetAsync<ImmutableArray<TResp>>(url);
        return resps.Length switch
        {
            0 => default,
            1 => resps[0],
            int len => throw new HueException($"Expected single value in array, got {len} values instead.")
        };
    }

    private async Task<TResp> PostAsync<TResp>(string url, HttpContent content)
    {
        HttpResponseMessage response = await http.PostAsync(url, content);
        return await HandleResponse<TResp>(response);
    }
    private async Task<ImmutableArray<HueResourceIdentifier>> PostAsync(string url, HueRequestBuilder request)
    {
        JsonContent jsonContent = JsonContent.Create(request.Build());
        return await PostAsync<ImmutableArray<HueResourceIdentifier>>(url, jsonContent);
    }

    private async Task<TResp> PutAsync<TResp>(string url, HttpContent content)
    {
        HttpResponseMessage response = await http.PutAsync(url, content);
        return await HandleResponse<TResp>(response);
    }
    private async Task<ImmutableArray<HueResourceIdentifier>> PutAsync(string url, HueRequestBuilder request)
    {
        JsonContent jsonContent = JsonContent.Create(request.Build());
        return await PutAsync<ImmutableArray<HueResourceIdentifier>>(url, jsonContent);
    }

    private async Task<ImmutableArray<HueResourceIdentifier>> DeleteAsync(string url)
    {
        HttpResponseMessage response = await http.DeleteAsync(url);
        return await HandleResponse<ImmutableArray<HueResourceIdentifier>>(response);
    }

    private static async Task<TResp> HandleResponse<TResp>(HttpResponseMessage response)
    {
        HueResponse<TResp>? resp = await response.Content.ReadFromJsonAsync<HueResponse<TResp>>();
        if (!response.IsSuccessStatusCode)
            throw HueRequestException.FromResponse(response.StatusCode, resp?.Errors);
        if (resp is null)
            throw HueRequestException.CouldNotDeserializeResponse();
        if (resp.Errors.Length > 0)
            throw HueRequestException.FromErrors(resp.Errors);

        return resp.Data;
    }
}
