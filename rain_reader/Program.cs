using System;
using System.Reflection;
using System.IO;
using System.Data;


// grab the folder location

string FolderLocation;
string CurrentFile;

//grab the location of the datafiles from the first args value, check its valid or exit and say why.
if (args.Length = 0)
{
    Console.WriteLine("Please specifiy the path to the datafiles in the first argument");
    Environment.Exit(1);
}
else
{
    FolderLocation = args[0];
    if (!Directory.Exists(FolderLocation))
    {
        Console.WriteLine("Specified folder does not exist: {0}", FolderLocation);
        Environment.Exit(1);
    }
}

//build the data structures

var Gauges = new DataTable();
Gauges.Columns.Add("Device ID", typeof(int));
Gauges.Columns.Add("Device Name", typeof(string));
Gauges.Columns.Add("Location", typeof(string));

var GaugeReadings = new DataTable();
GaugeReadings.Columns.Add("Device Id", typeof(int));
GaugeReadings.Columns.Add("Time", typeof(DateTime));
GaugeReadings.Columns.Add("Rainfall", typeof(int));

//load the gauge info
using (var reader = new StreamReader(FolderLocation + "Devices.csv"))
{
    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
    {
        // load the Guage info into the table
        using (var dr = new CsvDataReader(csv))
        {
            Gauges.Load(dr);
            //this is such a cheat, should add some kind of error checking

        }
    }
}

//load all the gauge readings

//enumerate the files that might contain info
//slurp them up into the table

