# ConvertDatMEA

[![build](https://github.com/gollischlab/ConvertDatMEA/actions/workflows/build.yml/badge.svg)](https://github.com/gollischlab/ConvertDatMEA/actions/workflows/build.yml)

Console application to convert channel data from multielectrode array (MEA) recordings to binary DAT files for spike sorting.

# Usage

The application accepts individual MSRD or MCD files or a directory and converts the files into one continuous binary DAT file treating them as one recording.
Separate DAT files containing the analog channels are placed into the subdirectory "analog".
The file bininfo.txt contains the number of channels, the sampling rate in Hz, and the voltage conversion factor, followed by the number of samples per input file in separate lines.
The conversion is summarized in conversion_output_logger.txt.

The directory or specific files to convert can either be provided as command-line arguments or by drag-and-drop onto the ConvertDatMEA.exe file.
The command-line interface allows to specify further options.

```
ConvertDatMEA.exe [options] "filepath1" ["filepath2" [...]]

  Options
  -------
    -help                      Show this information
    -metadata                  Show meta data of all files without processing
    -nowait                    Close application when done
    -wN                        Window width. N is the number of columns
    -hN                        Window height. N is the number of rows
    -channelorder "file.txt"   Specify a custom channel order in a textfile
```

**Note**: The application relies on the *Multi Channel DataManager* by *Multi Channel Systems MCS GmbH*. The software may have to be installed.
