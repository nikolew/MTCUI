using System;
using System.Collections.Generic;
using System.Text;

namespace MTCCore.DTO.Times
{
    public class AddTimeDto
    {
        public int GroupId { get; set; }
        public string Time { get; set; } // "HH:mm"
    }
}
