using System;
using System.Reflection;
using System.IO;
using System.Data;
using CsvHelper;
using System.Globalization;

// grab the folder location

string FolderLocation;

FolderLocation = ""; //because compilers are silly now

//grab the location of the datafiles from the first args value, check its valid or exit and say why.
if (args.Length == 0)
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
Gauges.Columns.Add("Device ID", typeof(Int32));
Gauges.Columns.Add("Device Name", typeof(string));
Gauges.Columns.Add("Location", typeof(string));

var GaugeReadings = new DataTable();
GaugeReadings.Columns.Add("Device Id", typeof(Int32));
GaugeReadings.Columns.Add("Time", typeof(DateTime));
GaugeReadings.Columns.Add("Rainfall", typeof(Int32));

//load the gauge info
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

//let's just check we have useful data on the screen

///*  Checking data is actually in the tables debug only

foreach (DataRow dataRow in Gauges.Rows)
{
    foreach (var item in dataRow.ItemArray)
    {
        Console.Write("{0}, ", item);
    }
    Console.WriteLine();
}


foreach (DataRow dataRow in GaugeReadings.Rows)
{
    foreach (var item in dataRow.ItemArray)
    {
        Console.Write("{0}, ", item);
    }
    Console.WriteLine();
}
//*/

//time to join the tables and get some results


//pinched this code from https://stackoverflow.com/questions/665754/inner-join-of-datatables-in-c-sharp to reduce pain


/*
private DataTable JoinDataTables(DataTable t1, DataTable t2, params Func<DataRow, DataRow, bool>[] joinOn)
{
    DataTable result = new DataTable();
    foreach (DataColumn col in t1.Columns)
    {
        if (result.Columns[col.ColumnName] == null)
            result.Columns.Add(col.ColumnName, col.DataType);
    }
    foreach (DataColumn col in t2.Columns)
    {
        if (result.Columns[col.ColumnName] == null)
            result.Columns.Add(col.ColumnName, col.DataType);
    }
    foreach (DataRow row1 in t1.Rows)
    {
        var joinRows = t2.AsEnumerable().Where(row2 =>
        {
            foreach (var parameter in joinOn)
            {
                if (!parameter(row1, row2)) return false;
            }
            return true;
        });
        foreach (DataRow fromRow in joinRows)
        {
            DataRow insertRow = result.NewRow();
            foreach (DataColumn col1 in t1.Columns)
            {
                insertRow[col1.ColumnName] = row1[col1.ColumnName];
            }
            foreach (DataColumn col2 in t2.Columns)
            {
                insertRow[col2.ColumnName] = fromRow[col2.ColumnName];
            }
            result.Rows.Add(insertRow);
        }
    }
    return result;
}


var test = JoinDataTables(Gauges, GaugeReadings,
               (row1, row2) =>
               row1.Field<int32>("Device ID") == row2.Field<int32>("Device ID"));



*/


