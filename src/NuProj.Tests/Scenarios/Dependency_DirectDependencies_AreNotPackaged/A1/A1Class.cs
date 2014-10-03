using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A1
{
    class Context : DbContext
    {

    }

    public class A1Class
    {
        public static void  Foo()
        {

            var foo = new A3.A3Class(); ;
        }
    }
}
