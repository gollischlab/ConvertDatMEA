using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.IO;
using Mcs.RawDataFileIO;
using Mcs.DataStream;


// Fernando Rozenblit, 2018

namespace ExtractChannels2
{
    class ChannelExtractor
    {
        const int amplifierMinVolt = -4096; //mv
        const int amplifierMaxVolt = 4096; //mv

        const int auxiliaryGain = 1;
        const int electrodeGain = 1100; // MEA-256 gain

        public delegate void OutputFunction(string line);
        public delegate void ProgressUpdate(double percent, string type = "");

        OutputFunction _outputFunction = null;
        ProgressUpdate _progressUpdate = null;
        BinaryWriter _channelWriter = null;
        BinaryWriter _auxWriter = null;

        private void OutputText(string text)
        {
            _outputFunction(text);
        }

        private void OutputLine(string line)
        {
            OutputText(line + "\r\n");
        }

        public ChannelExtractor(OutputFunction function, ProgressUpdate updater, BinaryWriter datWriter)
        {
            _outputFunction = function;
            _progressUpdate = updater;
            _channelWriter = datWriter; // Dat file
        }

        private byte[] ConvertRange(int[] data, InfoChannel electrode, int gain = 1)
        {
            byte[] newData = new byte[sizeof(ushort) * data.Count()];
            ConvertRange(data, electrode, gain, newData);

            return newData;
        }

        private static void ConvertRange(int[] data, InfoChannel electrode, int gain, byte[] newData)
        {
            if (2 * data.Count() != newData.Count())
                throw new ArgumentException("Data and newData must be the same size");

            double voltToSample = (1 << 16) / (amplifierMaxVolt - amplifierMinVolt); // Output should have a 16-bit range
            double electrodeConvFactor = electrode.ConversionFactor            // Picovolt (assuming an exponent of -12)
                                       * Math.Pow(10, electrode.Unit.Exponent) // Volt
                                       * 1e3                                   // Millivolt
                                       * gain;
            double adzero = electrode.ADZero;

            int dataCount = data.Count();
            Parallel.For(0, dataCount, i =>
           {
               double valueInMilliVolts = (data[i] - adzero) * electrodeConvFactor;
               valueInMilliVolts = (valueInMilliVolts - amplifierMinVolt) * voltToSample; // Convert to 16-bit sample centered around 0

               // Rounding is important to avoid noise
               byte[] inBytes = BitConverter.GetBytes((ushort)Math.Round(valueInMilliVolts));
               newData[2 * i] = inBytes[0];
               newData[2 * i + 1] = inBytes[1];
           });
        }


        private void WriteBin(InfoStreamAnalog analogInfo, byte[] data)
        {
            switch (analogInfo.DataSubType)
            {
                case enAnalogSubType.Electrode:
                    _channelWriter.Write(data);
                    break;

                case enAnalogSubType.Auxiliary:
                    _auxWriter.Write(data);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(string.Format("Invalid stream type: {0}", analogInfo.DataSubType.ToString()));
            }
        }

        Dictionary<string, int> layout252 = null;
        private int GetElectrodeOrder(InfoChannel electrode, int nChannels)
        {
            if (nChannels == 252)
            {
                if (layout252 == null)
                {
                    layout252 = new Dictionary<string, int>(){
                            { "A2",1},{"A3",2},{"A4",3},{"A5",4},{"A6",5},{"A7",6},{"A8",7},{"A9",8},{"A10",9},{"A11",10},{"A12",11},{"A13",12},{"A14",13},{"A15",14},
                        { "B1",15},{"B2",16},{"B3",17},{"B4",18},{"B5",19},{"B6",20},{"B7",21},{"B8",22},{"B9",23},{"B10",24},{"B11",25},{"B12",26},{"B13",27},{"B14",28},{"B15",29},{"B16",30},
                        { "C1",31},{"C2",32},{"C3",33},{"C4",34},{"C5",35},{"C6",36},{"C7",37},{"C8",38},{"C9",39},{"C10",40},{"C11",41},{"C12",42},{"C13",43},{"C14",44},{"C15",45},{"C16",46},
                        { "D1",47},{"D2",48},{"D3",49},{"D4",50},{"D5",51},{"D6",52},{"D7",53},{"D8",54},{"D9",55},{"D10",56},{"D11",57},{"D12",58},{"D13",59},{"D14",60},{"D15",61},{"D16",62},
                        { "E1",63},{"E2",64},{"E3",65},{"E4",66},{"E5",67},{"E6",68},{"E7",69},{"E8",70},{"E9",71},{"E10",72},{"E11",73},{"E12",74},{"E13",75},{"E14",76},{"E15",77},{"E16",78},
                        { "F1",79},{"F2",80},{"F3",81},{"F4",82},{"F5",83},{"F6",84},{"F7",85},{"F8",86},{"F9",87},{"F10",88},{"F11",89},{"F12",90},{"F13",91},{"F14",92},{"F15",93},{"F16",94},
                        { "G1",95},{"G2",96},{"G3",97},{"G4",98},{"G5",99},{"G6",100},{"G7",101},{"G8",102},{"G9",103},{"G10",104},{"G11",105},{"G12",106},{"G13",107},{"G14",108},{"G15",109},{"G16",110},
                        { "H1",111},{"H2",112},{"H3",113},{"H4",114},{"H5",115},{"H6",116},{"H7",117},{"H8",118},{"H9",119},{"H10",120},{"H11",121},{"H12",122},{"H13",123},{"H14",124},{"H15",125},{"H16",126},
                        { "J1",127},{"J2",128},{"J3",129},{"J4",130},{"J5",131},{"J6",132},{"J7",133},{"J8",134},{"J9",135},{"J10",136},{"J11",137},{"J12",138},{"J13",139},{"J14",140},{"J15",141},{"J16",142},
                        { "K1",143},{"K2",144},{"K3",145},{"K4",146},{"K5",147},{"K6",148},{"K7",149},{"K8",150},{"K9",151},{"K10",152},{"K11",153},{"K12",154},{"K13",155},{"K14",156},{"K15",157},{"K16",158},
                        { "L1",159},{"L2",160},{"L3",161},{"L4",162},{"L5",163},{"L6",164},{"L7",165},{"L8",166},{"L9",167},{"L10",168},{"L11",169},{"L12",170},{"L13",171},{"L14",172},{"L15",173},{"L16",174},
                        { "M1",175},{"M2",176},{"M3",177},{"M4",178},{"M5",179},{"M6",180},{"M7",181},{"M8",182},{"M9",183},{"M10",184},{"M11",185},{"M12",186},{"M13",187},{"M14",188},{"M15",189},{"M16",190},
                        { "N1",191},{"N2",192},{"N3",193},{"N4",194},{"N5",195},{"N6",196},{"N7",197},{"N8",198},{"N9",199},{"N10",200},{"N11",201},{"N12",202},{"N13",203},{"N14",204},{"N15",205},{"N16",206},
                        { "O1",207},{"O2",208},{"O3",209},{"O4",210},{"O5",211},{"O6",212},{"O7",213},{"O8",214},{"O9",215},{"O10",216},{"O11",217},{"O12",218},{"O13",219},{"O14",220},{"O15",221},{"O16",222},
                        { "P1",223},{"P2",224},{"P3",225},{"P4",226},{"P5",227},{"P6",228},{"P7",229},{"P8",230},{"P9",231},{"P10",232},{"P11",233},{"P12",234},{"P13",235},{"P14",236},{"P15",237},{"P16",238},
                            { "R2",239},{"R3",240},{"R4",241},{"R5",242},{"R6",243},{"R7",244},{"R8",245},{"R9",246},{"R10",247},{"R11",248},{"R12",249},{"R13",250},{"R14",251},{"R15",252}};
                }
                return layout252[electrode.Label];
            }
            return electrode.ID - 1;
        }

        private int GetBinID(InfoStreamAnalog streaminfo, InfoChannel electrode)
        {
            switch (streaminfo.DataSubType)
            {
                case enAnalogSubType.Electrode:
                    return GetElectrodeOrder(electrode, 252);

                case enAnalogSubType.Auxiliary:
                    if (Int32.TryParse(electrode.Label, out int auxnumber))
                    {
                        return /* 252 + */ auxnumber; // Start at zero
                    }
                    else
                    {
                        return electrode.ID;
                    }

                default:
                    return 0xBEEF;
            }
        }

        private void PrintMetadata(Reader fileReader)
        {
            int numRec = fileReader.Recordings.Count;
            if (numRec > 1)
            {
                OutputLine(String.Format("Number of recordings: {0}\r\n", numRec));
            }

            foreach (int recordId in fileReader.Recordings)
            {
                if (numRec > 1)
                    OutputLine(String.Format("Recording {0}", recordId));

                var header = fileReader.RecordingHdr[recordId];

                // Count the total number of channels in recording (analog + electrodes, for example)
                List<int> numChannels = new List<int>();
                foreach (var analogStream in header.AnalogStreams)
                    numChannels.Add(analogStream.Value.Entities.Count);
                OutputLine(String.Format("Number of channels: {0} ({1})", numChannels.Sum(), string.Join(" + ", numChannels)));

                // Loop over each recorded stream (analog data, filtered data, raw data, etc...)
                foreach (var analogStream in header.AnalogStreams)
                {
                    OutputLine("");
                    OutputLine(String.Format("{0}", analogStream.Value.Label));
                    OutputLine(String.Format("{0}", analogStream.Key));
                    OutputLine(String.Format("{0}", analogStream.Value.DataSubType));
                }

                OutputLine("");
            }
        }

        public void ExtractBins(string filepath, bool onlyMetadata, BinaryWriter auxWriter)
        {
            Reader fileReader = new Reader();
            fileReader.FileOpen(filepath);

            // Metadata
            PrintMetadata(fileReader);
            if (onlyMetadata)
            {
                fileReader.FileClose();
                return;
            }

            // Frame time file for this stimulus
            _auxWriter = auxWriter;

            // Expect only one recording as guaranteed by FileProcessor::VerifyFiles
            var pair = fileReader.RecordingHdr.FirstOrDefault();
            var recordId = pair.Key;
            var header = pair.Value;

            // Iterate over analog data and filtered data
            foreach (var analogStream in header.AnalogStreams.Where(v => 
                   (v.Value.DataSubType == enAnalogSubType.Auxiliary && v.Value.Label.Contains("Analog"))
                || (v.Value.DataSubType == enAnalogSubType.Electrode && v.Value.Label.Contains("Filter"))
            ))
            {
                var analogInfo = analogStream.Value;
                var analogGuid = analogStream.Key;
                int signalGain = GetGain(analogInfo, false);

                long tF = header.Duration;
                long t0 = 0;
                long stride = 10000;

                // Nchannel × Nsamples
                int[] buffer = new int[analogInfo.Entities.Count * stride];
                byte[] bytebuffer = new byte[analogInfo.Entities.Count * stride * sizeof(ushort)];

                while (t0 * 100 < tF)
                {
                    var entitiesIDs = analogInfo.Entities.GetIDs();
                    int nEntities = entitiesIDs.Count();

                    // Someone should check this factor of *100. There's something odd about it...
                    long maxSample = Math.Min((t0 + stride) * 100, tF);

                    var dataChunk = fileReader.GetChannelData<int>(recordId, analogGuid, entitiesIDs, t0 * 100, maxSample);

                    foreach (var channelChunk in dataChunk)
                    {
                        // channelChunk is weird. It has only one element.
                        var rawdata = channelChunk.Value[t0 * 100];

                        // Order the electrodes by channel map
                        var electrode = analogInfo.Entities[channelChunk.Key];
                        int keyID = GetBinID(analogInfo, electrode) - 1; // Indexing starts at zero

                        // stores the chunk in place at the Nchannels vs Nsamples matrix
                        for (int t = 0; t < Math.Min(stride, rawdata.Length); t++)
                        {
                            buffer[nEntities * t + keyID] = (rawdata[t]);
                        }
                    }

                    ConvertRange(buffer, analogInfo.Entities.First(), signalGain, bytebuffer);

                    WriteBin(analogInfo, bytebuffer);

                    _progressUpdate((float)t0 * 100 / tF, analogInfo.DataSubType == enAnalogSubType.Auxiliary ? analogInfo.DataSubType.ToString() : "");

                    t0 += stride;
                }
            };

            fileReader.FileClose();
        }

        // Gets the amplifier gain (inverts electrode gain if needed)
        private static int GetGain(InfoStreamAnalog analogInfo, bool invertSignal = false)
        {
            switch (analogInfo.DataSubType)
            {
                case enAnalogSubType.Electrode:
                    return invertSignal ? -electrodeGain : electrodeGain;
                case enAnalogSubType.Auxiliary:
                    return auxiliaryGain;
                default:
                    return 1;
            }
        }

        private void DisplayProgress(InfoChannel electrode, int BinID, int stimulusId)
        {
            OutputLine(String.Format("{3,2} - {0,3} ({1}, bin {2,3})", electrode.ID, electrode.Label, BinID, stimulusId));
        }
    }
}
