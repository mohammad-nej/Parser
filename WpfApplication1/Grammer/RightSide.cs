using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1
{

    class RightSide
    {
        public bool isEpsilon { get; set; }
        public string Left { get; set; }
        private string right;

        public string Right
        {
            get
            {
                return right;
            }
            set
            {
                if (value.Contains(' '))
                    words = value.Split(' ');
                else
                {
                    words = new string[1];
                    words[0] = value;
                }
                right = value;
            }
        }
        public string[] words;
        public int Number { get; set; }
        public List<string> mini_Follows { get; }
        public List<string> mini_Firsts { get; }
        public RightSide()
        {
            mini_Firsts = new List<string>();
            mini_Follows = new List<string>();
            isEpsilon = false;
            words = new string[1];
        }
        public bool AddFollow(string str)
        {
            bool anyThingAdded = false;
            if (str == "epsilon")
            {
                return false;
            }

            if (!mini_Follows.Contains(str))
            {
                mini_Follows.Add(str);
                anyThingAdded = true;
            }

            return anyThingAdded;
        }
        public bool AddFirst(string frs)
        {
            bool anyThingAdded = false;
            if (!mini_Firsts.Contains(frs))
            {
                mini_Firsts.Add(frs);
                anyThingAdded = true;
            }
            if (frs == "epsilon")
                isEpsilon = true;
            return anyThingAdded;
        }
    }
}
