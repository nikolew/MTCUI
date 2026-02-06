using System;
using System.Collections.Generic;
using System.Text;

namespace MTCCore.Messages.Timer
{
    public record TimerTickMessage(TimeSpan Time);
}
