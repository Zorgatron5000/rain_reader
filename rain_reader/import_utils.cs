using System.Data;
using System.Globalization;
using CsvHelper;

namespace utils;

public static partial class Utils
{
    public static bool LoadFileIntoTable(string filename, DataTable table)
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

    public static void OutputToScreen(string title, DataRowCollection show_results)
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
}

