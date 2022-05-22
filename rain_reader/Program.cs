using System;
using System.Reflection;
using System.IO;
using System.Data;
using CsvHelper;
using System.Globalization;

// grab the folder location

string FolderLocation;

FolderLocation = ""; //because compilers are silly now

//grab the location of the datafiles from the first args value, check it
//s valid or exit and say why.
if (args.Length == 0)
{
    Console.WriteLine("Please specifiy the path to the data files in the first argument");
    Environment.Exit(1);
}
else
{
    FolderLocation = args[0];
    if (!Directory.Exists(FolderLocation))
    {
        Console.WriteLine($"Specified folder does not exist: {FolderLocation}");
        Environment.Exit(1);
    }
}

//build the data structures for import

var Gauges = new DataTable();
Gauges.Columns.Add("Device ID", typeof(Int32));
Gauges.Columns.Add("Device Name", typeof(string));
Gauges.Columns.Add("Location", typeof(string));

var GaugeReadings = new DataTable();
GaugeReadings.Columns.Add("Device ID", typeof(Int32));
GaugeReadings.Columns.Add("Time", typeof(DateTime));
GaugeReadings.Columns.Add("Rainfall", typeof(Double));

//load the Gauge info
using (var reader = new StreamReader(FolderLocation + "\\Devices.csv"))

using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
{
    // load the Guage info into the table
    using (var dr = new CsvDataReader(csv))
    {
        Gauges.Load(dr);
        //this is such a cheat, should add some kind of error checking
        //need to filter out error values like blank lines. although current docs indicate that this is supposed to be the case but quick testing of loading the files indicates otherwise.
        //will need to check CsvHelper version specs against documentation. in the meantime I have removed the blank line in the Devices.txt file to skip the error condition.
    }
}

//load all the gauge readings
string[] files = Directory.GetFiles(FolderLocation, "Data*.csv");

foreach (var myFile in files)
{
    using (var reader = new StreamReader(myFile))
    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
    {
        // load the Guage Readings info into the table
        using (var dr = new CsvDataReader(csv))
        {
            GaugeReadings.Load(dr);
            //this is such a cheat, should add some kind of error checking
            //blanks and duplicates will still need to be filtered
        }
    }
}


/*  Checking data is actually in the tables debug only 

dump_results("Gauges", Gauges.Rows);
dump_results("Gauge Readings", GaugeReadings.Rows);

void dump_results(string title, DataRowCollection show_results)
{
    Console.WriteLine(title);
    Console.WriteLine();

    foreach (DataRow dataRow in show_results)
    {
        foreach (var item in dataRow.ItemArray)
        {
            Console.Write("{0}, ", item);
        }
        Console.WriteLine();
    }
}

//*/

//Start processing information


DateTime last_reading_time;
Double rainfall_average;
Double rainfall_max;
Double rainfall_latest;
Boolean rainfall_increasing;
Object calc_object;


//get the latest timestamp
last_reading_time = Convert.ToDateTime(GaugeReadings.Compute("max([Time])", string.Empty));

//changed my mind on calcs. Just going to bash it out with excessive force.

foreach (DataRow gauge in Gauges.Rows)
{
    //enumerate name
    var id = gauge["Device ID"].ToString();

    Console.ForegroundColor = ConsoleColor.White;
    Console.Write($"#{id}({gauge["Location"]}) ");

    //calc average and make the colour changes as required.

    calc_object = GaugeReadings.Compute("AVG([Rainfall])", $"(([device id] = {id})) and ([Time] > #{last_reading_time.AddHours(-4)}#)").ToString();

    if (calc_object == "")
        rainfall_average = 0;
    else
        //this is bonkers but I haven't done forced type conversion for decades, so what do I know
        rainfall_average = Convert.ToDouble(calc_object.ToString(), CultureInfo.InvariantCulture);

    if (rainfall_average < 10.0)
        Console.ForegroundColor = ConsoleColor.Green;
    else if (rainfall_average < 15.0)
        Console.ForegroundColor = ConsoleColor.DarkYellow;
    else
        Console.ForegroundColor = ConsoleColor.Red;

    //need to include a check for the >30mm outlier
    calc_object = GaugeReadings.Compute("MAX([Rainfall])", $"(([device id] = {id})) and ([Time] > #{last_reading_time.AddHours(-4)}#)").ToString();
    if (calc_object == "")
        rainfall_max = 0;
    else
        //this is bonkers but I haven't done type conversion for decades, so what do I know
        rainfall_max = Convert.ToDouble(calc_object.ToString(), CultureInfo.InvariantCulture);

    if (rainfall_max > 30.0)
        Console.ForegroundColor = ConsoleColor.Red;

    Console.Write(rainfall_average.ToString("F2"));

    //indicate increase/decrease (Compare Last reading to average value)
    DataRow[] select_results;
    select_results = GaugeReadings.Select($"(([device id] = {id})) and ([Time] > #{last_reading_time.AddHours(-4)}#)", "[Time] DESC");
    if (select_results.Length == 0)
        rainfall_latest = 0;
        //there also won't be an average if there are no results in this time period. 
        //possibly worth doing this first before the rest of the calcs, save some time
    else
        //this is bonkers but I haven't done type conversion for decades, so what do I know
        rainfall_latest = Convert.ToDouble(select_results[0]["Rainfall"].ToString(), CultureInfo.InvariantCulture);

    if (rainfall_latest > rainfall_average)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(" Increasing");
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(" Decreasing");
    }

    //clean up the line
    Console.ResetColor();
    Console.WriteLine();


}



Console.WriteLine();
Console.WriteLine();






