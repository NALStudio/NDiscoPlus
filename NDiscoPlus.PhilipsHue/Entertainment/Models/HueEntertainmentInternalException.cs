using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Entertainment.Models;
internal class HueEntertainmentInternalException : HueEntertainmentException
{
    public HueEntertainmentInternalException() : base()
    {
    }

    public HueEntertainmentInternalException(string? message) : base(message)
    {
    }

    public HueEntertainmentInternalException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
