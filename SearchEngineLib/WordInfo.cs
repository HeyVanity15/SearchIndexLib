using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngineLib
{
    public struct WordInfo
    {
        public int? Id { get; set; }
        public string Field { get; set; }
        public int Position { get; set; }
    }
}
