using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Authentication.Models;

internal class AuthenticationResponse
{
    public AuthenticationSuccessResponse? Success { get; }
    public AuthenticationErrorResponse? Error { get; }

    public AuthenticationResponse(AuthenticationSuccessResponse? success, AuthenticationErrorResponse? error)
    {
        Success = success;
        Error = error;
    }
}

internal class AuthenticationSuccessResponse
{
    public string Username { get; }
    public string? ClientKey { get; }

    public AuthenticationSuccessResponse(string username, string? clientKey)
    {
        Username = username;
        ClientKey = clientKey;
    }
}

internal class AuthenticationErrorResponse
{
    public int Type { get; }
    public string Address { get; }
    public string Description { get; }

    public AuthenticationErrorResponse(int type, string address, string description)
    {
        Type = type;
        Address = address;
        Description = description;
    }
}