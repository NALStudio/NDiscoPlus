using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Models;
public class HueException : Exception
{
    public HueException() : base()
    {
    }

    public HueException(string? message) : base(message)
    {
    }

    public HueException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
