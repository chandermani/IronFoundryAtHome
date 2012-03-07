using System;
using System.IO;
using System.Threading;

namespace FoldingClientMock
{
    class Program
    {
        static String baseContents =
@"Current Work Unit
-----------------
Name: {0}
Tag: {1}
Download time: {2}
Due time: {3}
Progress: {4}%  [|_________]
";

        static void Main(string[] args)
        {
            const String FILE_NAME = "unitinfo.txt";
            String Name = "Mock-" + DateTime.Now.Ticks.ToString();
            String Tag = "Tag";
            String DownloadTime = DateTime.Now.ToString("MMMM dd hh:mm:ss");
            String DueTime = "May 5 03:01:01";
            Int32  Progress = 0;


            while (Progress < 100)
            {
                Progress = Math.Min(Progress + new Random((Int32) System.DateTime.Now.Ticks).Next(11), 100);
                if (System.IO.File.Exists(FILE_NAME))
                {
                    System.IO.File.Delete(FILE_NAME);
                }

                using (StreamWriter sw = System.IO.File.CreateText(FILE_NAME))
                {
                    String contents = String.Format(baseContents, Name, Tag, DownloadTime, DueTime, Progress);
                    sw.Write(contents);
                    sw.Close();

                    Console.WriteLine("-------------------------");
                    Console.WriteLine(contents);
                    Console.WriteLine("-------------------------");
                }
                Thread.Sleep(30000);
            }
 //           Console.ReadLine();
        }
    }
}
