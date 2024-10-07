using NDiscoPlus.PhilipsHue.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Authentication.Models;
public class HueLinkButtonNotPressedException : HueAuthenticationException
{
    public HueLinkButtonNotPressedException() : base()
    {
    }

    public HueLinkButtonNotPressedException(string? message) : base(message)
    {
    }

    public HueLinkButtonNotPressedException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
