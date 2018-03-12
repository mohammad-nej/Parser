using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    class WordType
    {
        //in class noe ye token ro malum mikone ke bar asase ina midim be class Visualizer ke baramun rangi pangi kone
        public enum boundType { COMMENT, KEYWORD, LEXERROR,TEXT,STRING };
        public boundType type;
    }
}
