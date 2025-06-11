using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace toolbox.Model
{
    public class Difference
    {
        public string Path { get; set; }
        public string ValueOne { get; set; }
        public string ValueTwo { get; set; }

        public Difference(string path, string valueOne, string valueTwo)
        {
            Path = path;
            ValueOne = valueOne;
            ValueTwo = valueTwo;
        }
    }
}
