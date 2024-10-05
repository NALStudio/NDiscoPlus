using NDiscoPlus.PhilipsHue.Authentication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Entertainment;
public class HueEntertainment
{
    private readonly HueCredentials credentials;

    public HueEntertainment(HueCredentials credentials)
    {
        this.credentials = credentials;
    }
}
