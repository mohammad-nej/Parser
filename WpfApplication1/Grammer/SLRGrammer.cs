using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    class SLRGrammer
    {
        public string left { get; set; }
        public bool isEpsilon;

        public List<RightSide> Rights { get; set; }
        public List<string> Firsts { get; }
        public List<string> Follows { get; set; }
        public SLRGrammer()
        {
            Rights = new List<RightSide>();
            isEpsilon = false;
            Firsts = new List<string>();
            Follows = new List<string>();
        }

        public bool AddFollow(string str)
        {
            bool anyThingAdded = false;
            if (str == "epsilon")
            {
                return false;
            }

            if (!Follows.Contains(str))
            {
                Follows.Add(str);
                anyThingAdded = true;
            }

            return anyThingAdded;
        }
        public bool AddFirst(string frs)
        {
            bool anyThingAdded = false;
            if (!Firsts.Contains(frs))
            {
                Firsts.Add(frs);
                anyThingAdded = true;
            }
            if (frs == "epsilon")
                isEpsilon = true;
            return anyThingAdded;
        }
    };

}
