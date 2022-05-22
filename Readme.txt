Assumptions and implications for the Rain Reader

usage: rain_reader.exe <path to data files>

General assumptions
- Latest time stamp in any Data*.csv file will be used as the current time
- Average calculation will be across all readings falling within the time frame (4 hours prior to the latest reading, including the one exactly 4 hours prior) for that Gauge.
- Devices with no readings in the time period will show zero values
- Devices with same ID numbers will show identical summary results

Devices.csv
- Looks like a manually generated list of devices doing reporting
- couple of errors in the data have been cleaned up
- - line of blank values has been removed
- - Gauge 8 has a different Device ID (currently set to 99999 but can be corrected later) having duplicated IDs gives duplicated results
- Devices will be shown in the order they are entered in the csv file. No sorting will be applied.

Data*.csv
- Any files with the filename pattern above will be read and processed, unchanged.
- no blank lines in the files
- no missing fields in the files (every line has 3 data items, in the correct format)
- only device IDs that match those in the Devices.csv file will be reported on, headless Device IDs will not be rejected, just ignored.
- duplicate readings will be accepted, regardless of location.
- order of data entries makes no difference.




