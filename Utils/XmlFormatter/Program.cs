using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using FreeLibSet.IO;

namespace XmlFormatter
{
  class Program
  {
    static void Main(string[] args)
    {
      if (args.Length != 1)
      {
        Console.WriteLine("Usage: XmlFormatter filePath");
        Environment.ExitCode = 1;
        return;
      }
      try
      {
        AbsPath path = new AbsPath(args[0]);
        XmlDocument xmlDoc = FileTools.ReadXmlDocument(path);
        FileTools.WriteXmlDocument(path, xmlDoc);
      }
      catch (Exception e)
      {
        Console.WriteLine("Error: " + e.Message);
        Environment.ExitCode = 1;
      }
    }
  }
}
