using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EndDeviceService.Models;
using StackExchange.Redis;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Newtonsoft.Json;
using System.Net;

namespace EndDeviceService.Services
{
    public class MessageTranService
    {
        public Position position{get;set;}
        public List<Weapon> weaponList{get;set;}
        private ConnectionMultiplexer _connectionMultiplexer;
        private ISubscriber _iSubscriber;
        private IDatabase _iDatabase;
        public MessageTranService(ConnectionMultiplexer connectionMultiplexer)
        {
            weaponList = new List<Weapon>();
            position = new Position();
            _connectionMultiplexer = connectionMultiplexer;  
            _iSubscriber = connectionMultiplexer.GetSubscriber();
            _iDatabase = connectionMultiplexer.GetDatabase();
            BeginRecv();
        }

        private void BeginRecv()
        {
            _iSubscriber.Subscribe("BusSim", (channel, value) => {
                var data = Convert.FromBase64String(value);
                var messageHeader = GetMessageHeader(data);
                if (data.Length > 40)
                {
                    var valueWithoutHeader = Convert.ToBase64String(data.Skip(40).ToArray());
                    if(messageHeader.BusType == 3 && messageHeader.DesChannel1 == 2)//from rd
                    {
                        Type type = typeof(PositionStruct);
                        PositionStruct positionStruct= (PositionStruct)BytesToStruct(data.Skip(40).ToArray(), type);
                        
                        position.Altitude = (uint)System.Net.IPAddress.NetworkToHostOrder((int)positionStruct.altitude);
                        position.Longitude = (uint)System.Net.IPAddress.NetworkToHostOrder((int)positionStruct.longitude);
                        position.Latitude = (uint)System.Net.IPAddress.NetworkToHostOrder((int)positionStruct.latitude);
                        
                    }
                    else if (messageHeader.BusType == 3 && messageHeader.DesChannel1 == 6)//from smp
                    {
                        Type type = typeof(WeaponStruct);
                        WeaponStruct weaponStruct1= (WeaponStruct)BytesToStruct(data.Skip(40).ToArray(), type);
                        WeaponStruct weaponStruct2= (WeaponStruct)BytesToStruct(data.Skip(48).ToArray(), type);
                        weaponStruct1.type = (uint)System.Net.IPAddress.NetworkToHostOrder((int)weaponStruct1.type);
                        weaponStruct2.type = (uint)System.Net.IPAddress.NetworkToHostOrder((int)weaponStruct2.type);
                        weaponStruct1.num = (uint)System.Net.IPAddress.NetworkToHostOrder((int)weaponStruct1.num);
                        weaponStruct2.num = (uint)System.Net.IPAddress.NetworkToHostOrder((int)weaponStruct2.num);


                        Weapon weapon1 = new Weapon();
                        Weapon weapon2 = new Weapon();
                        if(weaponStruct1.type == 1){
                            weapon1.Type = WeaponType.PL10;
                        }
                        weapon1.num = weaponStruct1.num;
                        if(weaponStruct2.type == 2){
                            weapon2.Type = WeaponType.PL12;
                        }
                        weapon2.num = weaponStruct2.num;
                        weaponList.Clear();
                        weaponList.Add(weapon1);
                        weaponList.Add(weapon2);
                    }
                   
                }
            });
        }

        private void EndRecv()
        {
            _iSubscriber.UnsubscribeAll();
        }
        
        public MessageHeader GetMessageHeader(byte[] body)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in body)
            {
                string s1 = Convert.ToString(b, 2).PadLeft(8, '0');
                stringBuilder.Append(s1);
            }
            string msgValueBinaryStr = stringBuilder.ToString();

            MessageHeader messageheader = new MessageHeader();
            
            messageheader.BusType = Convert.ToInt32(msgValueBinaryStr.Substring(32, 8), 2);
            messageheader.NetNo = Convert.ToInt32(msgValueBinaryStr.Substring(40, 8), 2);
            messageheader.SrcChannel1 = Convert.ToInt32(msgValueBinaryStr.Substring(48, 32), 2);
            messageheader.SrcChannel2 = Convert.ToInt32(msgValueBinaryStr.Substring(80, 32), 2);
            messageheader.DesChannel1 = Convert.ToInt32(msgValueBinaryStr.Substring(112, 32), 2);
            messageheader.DesChannel2 = Convert.ToInt32(msgValueBinaryStr.Substring(144, 32), 2);

            return messageheader;
        }

        public object BytesToStruct(byte[] bytes, Type type)
        {
            int size = Marshal.SizeOf(type);
            if (size > bytes.Length)
            {
                return null;
            }
            //分配结构体内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将byte数组拷贝到分配好的内存空间
            Marshal.Copy(bytes, 0, structPtr, size);
            //将内存空间转换为目标结构体
            object obj = Marshal.PtrToStructure(structPtr, type);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            return obj;
        }

        public List<byte> GetSendData(object obj, int bustype, int src1, int src2, int des1, int des2)
        {
            List<byte> msgHeadList = new List<byte>();

            //MsgHeader bustype
            msgHeadList.Add((byte)bustype);

            // Msgheader NetNo
            msgHeadList.Add((byte)0);

            //Msgheader  sc1 sc2 dc1 dc2
            byte[] sc1 = HostToNetBytes(src1);
            byte[] sc2 = HostToNetBytes(src2);
            byte[] dc1 = HostToNetBytes(des1);
            byte[] dc2 = HostToNetBytes(des2);

            //Msgheader nodeId, timeTag;
            byte[] node = HostToNetBytes((byte)0);
            byte[] time = HostToNetBytes((byte)0);

            ListAddBytes(msgHeadList, sc1, 4);
            ListAddBytes(msgHeadList, sc2, 4);
            ListAddBytes(msgHeadList, dc1, 4);
            ListAddBytes(msgHeadList, dc2, 4);
            ListAddBytes(msgHeadList, node, 4);
            ListAddBytes(msgHeadList, time, 4);

            //msgheader reserve[10]
            for (int i = 0; i < 10; i++)
            {
                msgHeadList.Add((byte)0);
            }

            List<byte> msgBodyList = new List<byte>();
            ListAddBytes(msgBodyList,SerializeObject(obj),SerializeObject(obj).Count());
            //msg leng 
            byte[] leng = HostToNetBytes(msgBodyList.Count + msgHeadList.Count);

            List<byte> msg = new List<byte>();  
            ListAddBytes(msg, leng, 4);

            foreach (byte headbyte in msgHeadList)
            {
                msg.Add(headbyte);
            }

            foreach (byte bodybyte in msgBodyList)
            {
                msg.Add(bodybyte);
            }
            return msg;
        }
        
        public void MessageTranToSMP(Weapon weapon)
        {
            WeaponStruct weaponStruct;
            weaponStruct.type = (uint)weapon.Type;
            weaponStruct.num = weapon.num;

            weaponStruct.type = (uint)System.Net.IPAddress.HostToNetworkOrder((int)weaponStruct.type);
            weaponStruct.num = (uint)System.Net.IPAddress.HostToNetworkOrder((int)weaponStruct.num);
            List<byte> msg = GetSendData(weaponStruct, 3, 4, 0, 4, 0);
            SendData(msg.ToArray());
        }

        private void SendData(byte[] data)
        {
            var sendData = Convert.ToBase64String(data);
            var key = data[4].ToString() + "_" + data[5].ToString() + "_";
            key += getInt(data, 6).ToString() + "_";
            key += getInt(data, 10).ToString() + "_";
            key += getInt(data, 14).ToString() + "_";
            key += getInt(data, 18).ToString();
            _iSubscriber.Publish("BusSim", sendData);
            var value = Convert.ToBase64String(data.Skip(40).ToArray());
            _iDatabase.StringSet(key, value);
        }

        private int getInt(byte[] data, int index)
        {
            string value = "";
            for (int i=index; i<index+4; i++) {
                value += Convert.ToString(data[i], 16).PadLeft(2, '0');
            }
            return Convert.ToInt32(value, 16);
        }

        public byte[] HostToNetBytes(int hostData)
        {
            int netData = System.Net.IPAddress.HostToNetworkOrder(hostData);

            return BitConverter.GetBytes((uint)netData);
        }

        public void ListAddBytes(List<byte> list, byte[] byteArray, int len)
        {
            for (int i = 0; i < len; i++)
            {
                list.Add(byteArray[i]);
            }
        }

        public byte[] SerializeObject(object structObj)
        {
            int size = Marshal.SizeOf(structObj);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structObj, buffer, false);
                byte[] bytes = new byte[size];
                Marshal.Copy(buffer, bytes, 0, size);
                return bytes;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }
}