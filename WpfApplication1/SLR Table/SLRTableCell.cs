using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    class SLRTableCell
    {
        public enum Type { REDUCE,SHIFT,ACCEPT};
        public List<string> Number { get; set; }
        public List<Type> Action { get; set; }
        public bool isEpsilon { get; set; }
        
        public SLRTableCell()
        {
            isEpsilon = false;
            Action = new List<Type>();
            Number = new List<string>();
        }
    }
}
