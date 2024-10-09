using NDiscoPlus.PhilipsHue.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Entertainment.Models;
public class HueEntertainmentException : HueException
{
    public HueEntertainmentException() : base()
    {
    }

    public HueEntertainmentException(string? message) : base(message)
    {
    }

    public HueEntertainmentException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
