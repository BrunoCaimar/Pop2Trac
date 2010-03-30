using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrunoCaimar.Pop2Trac
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Out.WriteLine("Starting...");
            new Pop2Trac().Run();
            Console.Out.WriteLine("Finish!");
            //Console.In.Read();
        }
    }
}
