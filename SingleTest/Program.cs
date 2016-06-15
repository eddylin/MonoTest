using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Guid guid = new Guid("{C145DC30-F3D7-4FF8-B31B-91DE147F736F}");
            using (SingleInstance singleInstance = new SingleInstance(guid))
            {
                if(singleInstance.IsFirstInstance)
                {
                    singleInstance.ArgumentsReceived += (o, e) =>
                    {
                        Console.WriteLine("ArgumentReceived!!!!!!!!!!!!!!!!!!!!!");
                    };
                    singleInstance.ListenForArgumentsFromSuccessiveInstances();
                    Console.ReadLine();
                }
                else
                {
                    singleInstance.PassArgumentsToFirstInstance(Environment.GetCommandLineArgs());
                }
            }
        }
    }
}
