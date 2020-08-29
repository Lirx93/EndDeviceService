using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


public struct WeaponStruct
{
	public uint type;//武器类型
	public uint num;
};

namespace EndDeviceService.Models
{
    public class Weapon
    {
        public WeaponType Type { get; set; }//武器类型
        public uint num { get; set; }//当前挂点
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WeaponType
    {
    PL10 = 0,
    PL12 = 1,
    PL15 = 2
    }
}