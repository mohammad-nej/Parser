using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    class LexerOutPut
    {
        public enum OutPut { TYPE, TEXT};
        public OutPut output;
        public LexerOutPut()
        {
            output = new OutPut();
        }
    }
}
