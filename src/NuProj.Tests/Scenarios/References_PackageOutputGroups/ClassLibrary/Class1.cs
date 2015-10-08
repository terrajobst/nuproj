using System;
using System.Xml.Serialization;

namespace ClassLibrary
{
    /// <summary>
    /// Does stuff.
    /// </summary>
    [XmlType(Namespace = "urn:Foo1")]
    [XmlRoot(Namespace = "urn:Foo1")]
    public class Class1
    {
        public int Prop { get; set; }
        
        /// <summary>
        /// Says hello.
        /// </summary>
        public static void SayHello()
        {
           Console.WriteLine(Resources.Text.HelloWorld); 
        }
    }
}
