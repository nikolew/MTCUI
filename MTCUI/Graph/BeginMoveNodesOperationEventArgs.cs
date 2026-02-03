using System;
using System.Collections.Generic;
using System.Text;

namespace MTCUI.Graph
{
    public class BeginMoveNodesOperationEventArgs : EventArgs
    {
        public Guid[] NodeGuids { get; } = null;

        public BeginMoveNodesOperationEventArgs(Guid[] nodeGuids)
        {
            NodeGuids = nodeGuids;
        }
    }
}
