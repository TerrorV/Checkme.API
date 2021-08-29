using System;
using System.Collections.Generic;
using System.Text;

namespace Checkme.BL.Abstract
{
    public class CheckList
    {
        public Guid Id { get; set; }
        public List<string> Outstanding { get; set; }
        public List<string> Done { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
