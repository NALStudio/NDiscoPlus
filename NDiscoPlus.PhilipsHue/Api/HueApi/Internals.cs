using NDiscoPlus.PhilipsHue.Api.Constants;
using NDiscoPlus.PhilipsHue.Api.Models.RequestBuilders.Base;
using NDiscoPlus.PhilipsHue.Api.Models.Responses;
using NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
using NDiscoPlus.PhilipsHue.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.HueApi;
public partial class LocalHueApi
{
    private async Task<TResp> GetAsync<TResp>(ClipV2Endpoint endpoint)
    {
        HttpResponseMessage response = await http.GetAsync(endpoint.GetUri());
        return await HandleResponse<TResp>(response);
    }

    /// <summary>
    /// Fetch data as an array and expect it to only contain a single item.
    /// </summary>
    private async Task<TResp> GetSingleAsync<TResp>(ClipV2Endpoint endpoint)
    {
        HttpResponseMessage response = await http.GetAsync(endpoint.GetUri());
        ImmutableArray<TResp> resps = await HandleResponse<ImmutableArray<TResp>>(response);
        if (resps.Length != 1) // If light was not found, response handling throws on 404
            throw new HueException($"Expected single value in array, got {resps.Length} values instead.");
        return resps[0];
    }

    /// <summary>
    /// Fetch data as an array and expect it to only contain a single item. If value was not found (404), return null.
    /// </summary>
    private async Task<TResp?> TryGetSingleAsync<TResp>(ClipV2Endpoint endpoint) where TResp : class
    {
        HttpResponseMessage response = await http.GetAsync(endpoint.GetUri());
        ImmutableArray<TResp> resps = await HandleResponse(response, on404Received: () => ImmutableArray<TResp>.Empty);
        if (resps.Length > 1)
            throw new HueException($"Expected <= 1 values in array, got {resps.Length} values instead.");
        return resps.Length > 0 ? resps[0] : null;
    }

    private async Task<TResp> PostAsync<TResp>(ClipV2Endpoint endpoint, HttpContent content)
    {
        HttpResponseMessage response = await http.PostAsync(endpoint.GetUri(), content);
        return await HandleResponse<TResp>(response);
    }
    private async Task<ImmutableArray<HueResourceIdentifier>> PostAsync(ClipV2Endpoint endpoint, HueRequestBuilder request)
    {
        JsonContent jsonContent = JsonContent.Create(request.Build());
        return await PostAsync<ImmutableArray<HueResourceIdentifier>>(endpoint, jsonContent);
    }

    private async Task<TResp> PutAsync<TResp>(ClipV2Endpoint endpoint, HttpContent content)
    {
        string json = await content.ReadAsStringAsync();
        HttpResponseMessage response = await http.PutAsync(endpoint.GetUri(), content);
        return await HandleResponse<TResp>(response);
    }
    private async Task<ImmutableArray<HueResourceIdentifier>> PutAsync(ClipV2Endpoint endpoint, HueRequestBuilder request)
    {
        JsonContent jsonContent = JsonContent.Create(request.Build());
        return await PutAsync<ImmutableArray<HueResourceIdentifier>>(endpoint, jsonContent);
    }

    private async Task<ImmutableArray<HueResourceIdentifier>> DeleteAsync(ClipV2Endpoint endpoint)
    {
        HttpResponseMessage response = await http.DeleteAsync(endpoint.GetUri());
        return await HandleResponse<ImmutableArray<HueResourceIdentifier>>(response);
    }

    private static async Task<TResp> HandleResponse<TResp>(HttpResponseMessage response, Func<TResp>? on404Received = null)
    {
        HueResponse<TResp>? resp;
        try
        {
            resp = await response.Content.ReadFromJsonAsync<HueResponse<TResp>>();
        }
        catch (JsonException e)
        {
            string content = await response.Content.ReadAsStringAsync();
            e.Data.Add("ResponseContent", content);
            throw;
        }

        bool skip404 = on404Received is not null && response.StatusCode == HttpStatusCode.NotFound;
        if (!response.IsSuccessStatusCode && !skip404)
            throw HueRequestException.FromResponse(response.StatusCode, resp?.Errors);
        if (resp is null)
            throw HueRequestException.CouldNotDeserializeResponse();
        if (resp.Errors.Length > 0 && !skip404)
            throw HueRequestException.FromErrors(resp.Errors);

        if (skip404)
            return on404Received!.Invoke();
        else
            return resp.Data;
    }
}
