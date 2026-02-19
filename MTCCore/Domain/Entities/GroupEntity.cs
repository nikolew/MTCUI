using Microsoft.EntityFrameworkCore;
using MTCCore.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Text;
using System.Xml.Linq;

namespace MTCCore.Domain.Entities
{
    [Table("Groups")]
    public class GroupEntity
    {
        public int Id { get; set; }

        public string GroupName { get; set; }
        public string Color { get; set; }

        public ICollection<TimeEntity> Times { get; set; } = new List<TimeEntity>();
        public ICollection<NodeEntity> Nodes { get; set; } = new List<NodeEntity>();


        [NotMapped]
        public Color GroupColor
        {
            get { return ColorTranslator.FromHtml(Color); }

        }
    }
}
