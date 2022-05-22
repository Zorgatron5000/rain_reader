using System.Data;
using System.Globalization;
using utils;



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
    Console.WriteLine("Please specifiy the path to the data files in the first argument e.g. rain_reader c:\\ftp\\gauges");
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

//build the data structures prior to import
Gauges = new DataTable();
Gauges.Columns.Add("Device ID", typeof(Int32));
Gauges.Columns.Add("Device Name", typeof(string));
Gauges.Columns.Add("Location", typeof(string));

GaugeReadings = new DataTable();
GaugeReadings.TableName = "Readings";
GaugeReadings.Columns.Add("Device ID", typeof(Int32));
GaugeReadings.Columns.Add("Time", typeof(DateTime));
GaugeReadings.Columns.Add("Rainfall", typeof(Double));

//import the ALL of the data (this could be a problem if they never delete old data)
if (Utils.LoadFileIntoTable(FolderLocation + "\\Devices.csv", Gauges))
{
    data_files = Directory.GetFiles(FolderLocation, "Data*.csv");
    foreach (var df in data_files)
    {
        if (!Utils.LoadFileIntoTable(df, GaugeReadings))
        { 
            Console.WriteLine($"There was an error importing {df}, please check the file strucutre. Trying other files.");
        }
    }
}
else 
{
    Console.WriteLine("There was an error while importing Gauge information, please check the file structure.");
    Environment.Exit(4);
}

/*  Checking data is actually in the tables debug only 

OutputToScreen("Gauges", Gauges.Rows);
OutputToScreen("Gauge Readings", GaugeReadings.Rows);

//*/

//Start processing information

//get the latest timestamp and write it to screen
last_reading_time = Convert.ToDateTime(GaugeReadings.Compute("max([Time])", string.Empty));
Console.WriteLine($"Last reading received at: {last_reading_time}");
Console.WriteLine($"Cut off time: {last_reading_time.AddHours(TIME_PERIOD_HOURS)}");
Console.WriteLine();

//now process the Gauges

foreach (DataRow g in Gauges.Rows)
{
    object calc_object;
    DataRow[] select_results;
    string id;
    string filter_string;

     //get some filtering sorted out
    id = g["Device ID"].ToString();
    filter_string = $"(([Device ID] = {id})) and ([Time] > #{last_reading_time.AddHours(TIME_PERIOD_HOURS).AddMinutes(-1).ToString("yyyy/MM/dd HH:mm:ss")}#)";

    Console.ForegroundColor = ConsoleColor.White;
    Console.Write($"(#{id}) {g["Location"]}".PadRight(PADDING_LABEL));

    //calc average and make the colour changes as required.
    calc_object = GaugeReadings.Compute("AVG([Rainfall])", filter_string).ToString();

    if ((calc_object != null) && (calc_object != ""))
        rainfall_average = Convert.ToDouble(calc_object.ToString(), CultureInfo.InvariantCulture);  //this is bonkers but I haven't done forced type conversion for decades, so what do I know
    else
        rainfall_average = 0;

    if (rainfall_average < 10.0)
        Console.ForegroundColor = ConsoleColor.Green;
    else if (rainfall_average < 15.0)
        Console.ForegroundColor = ConsoleColor.DarkYellow;
    else
        Console.ForegroundColor = ConsoleColor.Red;

    //need to include a check for the >30mm outlier
    calc_object = GaugeReadings.Compute("MAX([Rainfall])", filter_string).ToString();
    if ((calc_object != null) && (calc_object != ""))
        rainfall_max = Convert.ToDouble(calc_object.ToString(), CultureInfo.InvariantCulture);      //this is bonkers but I haven't done forced type conversion for decades, so what do I know
    else
        rainfall_max = 0;

    if (rainfall_max > 30.0)
        Console.ForegroundColor = ConsoleColor.Red;

    Console.Write(rainfall_average.ToString("F2").PadLeft(PADDING_AVG));


    //indicate increase/decrease (Compare Last reading to average value)
    select_results = GaugeReadings.Select(filter_string, "[Time] DESC");
    if (select_results.Length == 0)
        rainfall_latest = 0;
        //there also won't be an average if there are no results in this time period. 
        //possibly worth doing this first before the rest of the calcs, save some time
    else
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


//end program









