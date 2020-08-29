using System;
using System.Collections.Generic;

public struct PositionStruct
{
    public uint longitude;
    public uint latitude;
    public uint altitude;
};

namespace EndDeviceService.Models
{
    public class Position
    {
        public uint Longitude { get; set; }
        public uint Latitude { get; set; }
        public uint Altitude { get; set; }
    }
}