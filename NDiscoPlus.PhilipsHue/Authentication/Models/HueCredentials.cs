using System.Text.Json.Serialization;

namespace NDiscoPlus.PhilipsHue.Authentication.Models;
public readonly record struct HueCredentials(string AppKey, string? ClientKey);
