using NDiscoPlus.PhilipsHue.Api.Models.Responses.Generic;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Models;
internal class HueRequestException : HueException
{
    public HueRequestException() : base()
    {
    }

    public HueRequestException(string? message) : base(message)
    {
    }

    public HueRequestException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public static HueRequestException CouldNotSerializeRequest() => new("Could not serialize request.");
    public static HueRequestException CouldNotDeserializeResponse() => new("Could not deserialize response.");

    public static HueRequestException FromResponse(HttpStatusCode statusCode, IReadOnlyList<HueError>? errors)
    {
        StringBuilder sb = new();

        sb.Append("Hue request failed with ");
        sb.Append(statusCode);
        sb.Append(" status code.");
        sb.AppendLine();

        if (errors?.Count > 0)
        {
            sb.Append("The following errors were returned by the API:\n");
            BuildErrors(sb, errors);
        }
        else
        {
            sb.Append("No additional error data was provided by the API.\n");
        }

        return new HueRequestException(sb.ToString());
    }

    public static HueRequestException FromErrors(IEnumerable<HueError> errors)
    {
        StringBuilder sb = new("Hue request failed with the following errors:\n");
        BuildErrors(sb, errors);

        return new HueRequestException(sb.ToString());
    }

    private static void BuildErrors(StringBuilder sb, IEnumerable<HueError> errors)
    {
        int i = 0;
        foreach (HueError err in errors)
        {
            sb.Append("  ");
            sb.Append(i);
            sb.Append(": ");
            sb.Append(err.Description);
            sb.AppendLine();

            i++;
        }

        // enumerable was empty
        if (i == 0)
            sb.Append("  ?: No error data available.");
    }
}
