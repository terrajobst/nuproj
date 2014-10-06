using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2
{
    public class A2Class
    {
        public int Two()
        {
            var a3 = new A3.A3Class();
            
            using (var x = new Newtonsoft.Json.JsonTextWriter(System.IO.File.AppendText("")))
            {
                return 2;
            }
        }
    }
}
