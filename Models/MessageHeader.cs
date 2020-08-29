using System;
using System.Collections.Generic;

namespace EndDeviceService.Models
{
    public class MessageHeader
    {
        public int BusType { get; set; }
        public int NetNo { get; set; }
        public int SrcChannel1 { get; set; }
        public int SrcChannel2 { get; set; }
        public int DesChannel1 { get; set; }
        public int DesChannel2 { get; set; }
    }
}