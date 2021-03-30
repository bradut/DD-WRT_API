using System;

namespace DD_WRT_API.Models
{
    public struct WirelessNode
    {
        public string MacAddress { get; set; }
        public string Interface { get; set; }
        public TimeSpan UpTime { get; set; }
        public int TxRateMb { get; set; }
        public int RxRateMb { get; set; }
        public string Info { get; set; }
        public int SignalLeveldB { get; set; }
        public int NoiseLeveldB { get; set; } // Always -90 dB in DD-WRT 

        // SNR = -1*(NoiseLevel [dB] - SignalLevel [dB]) // Recommended values: > 20dB for data, > 25 dB for VoIP
        // http://www.wireless-nets.com/resources/tutorials/define_SNR_values.html
        public int SignalToNoiseRatiodB{ get; set; }  
        public int SignalQuality { get; set; }  // should always be <= 100

        public override string ToString()
        {
            return $"{MacAddress}, {Interface}, {UpTime.ToString().PadLeft(11)}, {TxRateMb.ToString().PadLeft(3)}, {RxRateMb.ToString().PadLeft(3)}, {Info}, " +
                   $"{SignalLeveldB.ToString().PadLeft(2)}, {NoiseLeveldB.ToString().PadLeft(2)}, {SignalToNoiseRatiodB.ToString().PadLeft(2)}, {SignalQuality.ToString().PadLeft(3)}";
        }
    }
}
