using MTCCore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTCCore.Domain.Entities
{
    public class PositionEntity
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public int NodeId { get; set; }
        public NodeEntity Node { get; set; }
    }
}
