Assumptions and implications for the Rain Reader

Devices.csv
- manually generated list of devices doing reporting
- couple of errors in the data have been cleaned up
- - blank line of values has been removed
- - Gauge 8 has a different (currently set to 99999 but can be corrected later) Device ID

Data*.csv
- Any files with the filename pattern above will be read and processed, unchanged.
- no blank lines in the files
- only device IDs that match those in the Devices.csv file will be reported on

Overall assumptions
- latest time stamp in any Data#.csv file will be used as the current time
- missing records from devices will not be included in the average rainfall value calulation. So a device with 1 reading in the last 4 hours will have an average of that value. 3 readings would be averaged over the three.
