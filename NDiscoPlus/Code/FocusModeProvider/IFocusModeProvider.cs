using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Code.FocusModeProvider;
internal interface IFocusModeProvider
{
    public bool IsFocusModeEnabled { get; }

    public void EnableFocusMode();
    public void DisableFocusMode();
}
