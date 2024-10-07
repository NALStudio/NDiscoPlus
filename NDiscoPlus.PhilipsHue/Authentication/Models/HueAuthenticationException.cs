using NDiscoPlus.PhilipsHue.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Authentication.Models;
public class HueAuthenticationException : HueException
{
    public HueAuthenticationException() : base()
    {
    }

    public HueAuthenticationException(string? message) : base(message)
    {
    }

    public HueAuthenticationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
