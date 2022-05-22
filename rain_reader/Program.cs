using System;
using System.Reflection;
using System.IO;
using System.Data;
using CsvHelper;
using System.Globalization;

//constants
const int PADDING_LABEL = 25;
const int PADDING_AVG = 6;
const int TIME_PERIOD_HOURS = -4;


//variables
string FolderLocation;
string[] data_files;

DateTime last_reading_time;
Double rainfall_average;
Double rainfall_max;
Double rainfall_latest;

DataTable Gauges;
DataTable GaugeReadings;


// Check for the data folder
FolderLocation = ""; 

//grab the location of the datafiles from the first args value, check it is valid or exit and say why.
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
    //now check there is at least a devices.csv and a Data*.csv file in the folder
    if (!File.Exists(FolderLocation + "\\devices.csv"))
    {
        Console.WriteLine("Could not find 'Devices.csv' file in data folder.");
        Environment.Exit(2);
    }
    if (!Directory.GetFiles(FolderLocation, "Data*.csv").Any())
    {
        Console.WriteLine("Could not find any 'Data*.csv' files in data folder.");
        Environment.Exit(3);
    }
}

//files exist and we know where they are, let's import the information

//build the data structures prior ot import
Gauges = new DataTable();
Gauges.Columns.Add("Device ID", typeof(Int32));
Gauges.Columns.Add("Device Name", typeof(string));
Gauges.Columns.Add("Location", typeof(string));

GaugeReadings = new DataTable();
GaugeReadings.Columns.Add("Device ID", typeof(Int32));
GaugeReadings.Columns.Add("Time", typeof(DateTime));
GaugeReadings.Columns.Add("Rainfall", typeof(Double));

//import the ALL of the data (this could be a problem if they never delete old data)
LoadFileIntoTable(FolderLocation + "\\Devices.csv", Gauges);

data_files = Directory.GetFiles(FolderLocation, "Data*.csv");
foreach (var df in data_files)
{
    LoadFileIntoTable(df, GaugeReadings);
}

//Start processing information

//get the latest timestamp
last_reading_time = Convert.ToDateTime(GaugeReadings.Compute("max([Time])", string.Empty));

//changed my mind on calcs. Just going to bash it out with excessive force.

foreach (DataRow gauge in Gauges.Rows)
{
    Object calc_object;
    DataRow[] select_results;
    string id;

    //enumerate name
    id = gauge["Device ID"].ToString();

    Console.ForegroundColor = ConsoleColor.White;
    Console.Write($"{gauge["Location"]} (#{id})".PadRight(PADDING_LABEL));

    //calc average and make the colour changes as required.

    calc_object = GaugeReadings.Compute("AVG([Rainfall])", $"(([device id] = {id})) and ([Time] > #{last_reading_time.AddHours(TIME_PERIOD_HOURS)}#)").ToString();

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
    calc_object = GaugeReadings.Compute("MAX([Rainfall])", $"(([device id] = {id})) and ([Time] > #{last_reading_time.AddHours(TIME_PERIOD_HOURS)}#)").ToString();
    if (calc_object == "")
        rainfall_max = 0;
    else
        //this is bonkers but I haven't done type conversion for decades, so what do I know
        rainfall_max = Convert.ToDouble(calc_object.ToString(), CultureInfo.InvariantCulture);

    if (rainfall_max > 30.0)
        Console.ForegroundColor = ConsoleColor.Red;

    Console.Write(rainfall_average.ToString("F2").PadLeft(PADDING_AVG));

    //indicate increase/decrease (Compare Last reading to average value)
    select_results = GaugeReadings.Select($"(([device id] = {id})) and ([Time] > #{last_reading_time.AddHours(TIME_PERIOD_HOURS)}#)", "[Time] DESC");
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

//Disclaimer section (make it obvious when the last received reading was)
Console.WriteLine();
Console.WriteLine($"Last time stamp: {last_reading_time}");


//end program

bool LoadFileIntoTable(string filename, DataTable table)
{
    using (var reader = new StreamReader(filename))
    {
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            // load the Guage info into the table
            using (var dr = new CsvDataReader(csv))
            {
                try
                {
                    table.Load(dr);
                    //this is such a cheat, should add some kind of error checking
                    //need to filter out error values like blank lines. although current docs indicate that this is supposed to be the case but quick testing of loading the files indicates otherwise.
                    //will need to check CsvHelper version specs against documentation. in the meantime I have removed the blank line in the Devices.txt file to skip the error condition.
                    return true;
                }
                catch
                {
                    return false;
                }
            }
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




