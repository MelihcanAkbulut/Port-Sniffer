using System;
using System.Threading;
using System.Collections.Generic;
using PcapDotNet.Base;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Arp;
using PcapDotNet.Packets.Dns;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.Gre;
using PcapDotNet.Packets.Http;
using PcapDotNet.Packets.Icmp;
using PcapDotNet.Packets.Igmp;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.IpV6;
using PcapDotNet.Packets.Transport;
using PcapDotNet.Core.Extensions;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Data.SQLite;

namespace MultiThreading
{
    class Program
    {

        static Queue<Packet> kuyruk = new Queue<Packet>();
        static Queue<Packet> kuyruk2 = new Queue<Packet>();
        static MacAddress sourceMAC;
        static MacAddress destinationMAC;
        static string sourceIP_str;
        static ushort sourcePort;
        static ushort destinationPort;
        static string str;
        static IpV4Address sourceIP;
        static IpV4Address destinationIP;
        static LivePacketDevice selectedDevice;
        static Dictionary<ushort, DateTime> pingID = new Dictionary<ushort, DateTime>();
        static SQLiteConnection con = new SQLiteConnection(@"Data Source = C:\DB Browser for SQLite\deneme.db"); 

        static void Main(string[] args)
        {
            con.Open();
            // Retrieve the device list from the local machine
            IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;
            // Take the selected adapter
            selectedDevice = allDevices[2 - 1];

            sourceMAC = selectedDevice.GetMacAddress();
            sourceIP_str = null;

            foreach (DeviceAddress address in selectedDevice.Addresses)
            {
                if (address.Address.Family == SocketAddressFamily.Internet)
                    sourceIP_str = address.Address.ToString().Substring(9, address.Address.ToString().Length - 9);
            }

            sourceMAC = selectedDevice.GetMacAddress();
            sourceIP = new IpV4Address(sourceIP_str);
            ushort port1 = 555;
            ushort port2 = 24;

            Thread thread2 = new Thread(TcpDinle);
            Thread thread3 = new Thread(TcpYorumla);
            Thread thread4 = new Thread(UdpDinle);
            Thread thread5 = new Thread(UdpYorumla);
            

            thread2.Start();
            thread3.Start();
            thread4.Start();
            thread5.Start();
        }

        
        static void TcpDinle()
        {

            using (PacketCommunicator communicator =
                selectedDevice.Open(65536,                                 
                                                                            
                                    PacketDeviceOpenAttributes.Promiscuous, 
                                    1000))                                  
            {
                // Compile the filter
                using (BerkeleyPacketFilter filter = communicator.CreateFilter("ip and tcp and dst " + sourceIP.ToString())) //and eth.dst == "+sourceMAC.ToString()
                {
                    // Set the filter
                    communicator.SetFilter(filter);
                }
                Console.WriteLine("Paketler Dinleniyor...");

                // Retrieve the packets
                Packet packet;
                do
                {
                    PacketCommunicatorReceiveResult result = communicator.ReceivePacket(out packet);
                    switch (result)
                    {
                        case PacketCommunicatorReceiveResult.Timeout:
                            // Timeout elapsed
                            continue;
                        case PacketCommunicatorReceiveResult.Ok:
                            lock (kuyruk2)
                            {
                                kuyruk2.Enqueue(packet);
                                break;
                            }

                        default:
                            throw new InvalidOperationException("The result " + result + " should never be reached here");
                    }
                } while (true);
            }
        }

        static void TcpYorumla()
        {
            while (true)
            {

                Packet p;
                if (kuyruk2.Count > 0)
                    lock (kuyruk2)
                    {
                        p = kuyruk2.Dequeue();
                    }
                else continue;

                DateTime zaman = p.Timestamp;
                IpV4Datagram ip = p.Ethernet.IpV4;
                TcpDatagram tcp = ip.Tcp;
                UdpDatagram udp = ip.Udp;
                Console.WriteLine(p);
                Console.WriteLine(" Zaman = " + zaman + " / " + " Port =  TCP - " + tcp.DestinationPort + " / " + " Kaynak = " + ip.Source);
                Kaydet(zaman, ip.Source, "TCP" , tcp.DestinationPort);
                /*using (PacketCommunicator communicator = selectedDevice.Open(100, // name of the device
                                                                         PacketDeviceOpenAttributes.Promiscuous, // promiscuous mode
                                                                         1000)) // read timeout*/
                //communicator.SendPacket(BuildUdpPacket());
                //Thread.Sleep(100);

            }
        }
        
        static void UdpDinle()
        {
            using (PacketCommunicator communicator =
                selectedDevice.Open(65536,                                  // portion of the packet to capture
                                                                            // 65536 guarantees that the whole packet will be captured on all the link layers
                                    PacketDeviceOpenAttributes.Promiscuous, // promiscuous mode
                                    1000))                                  // read timeout
            {
                // Compile the filter
                using (BerkeleyPacketFilter filter = communicator.CreateFilter("ip and udp and dst " + sourceIP.ToString())) //and eth.dst == "+sourceMAC.ToString()
                {
                    // Set the filter
                    communicator.SetFilter(filter);
                }

                // Retrieve the packets
                Packet packet;
                do
                {
                    PacketCommunicatorReceiveResult result = communicator.ReceivePacket(out packet);
                    switch (result)
                    {
                        case PacketCommunicatorReceiveResult.Timeout:
                            // Timeout elapsed
                            continue;
                        case PacketCommunicatorReceiveResult.Ok:
                            lock (kuyruk)
                            {
                                kuyruk.Enqueue(packet);
                                break;
                            }

                        default:
                            throw new InvalidOperationException("The result " + result + " should never be reached here");
                    }
                } while (true);
            }
        }

        /*private static Packet BuildUdpPacket()
        {
            sourceMAC = selectedDevice.GetMacAddress();
            EthernetLayer ethernetLayer =
                new EthernetLayer
                {
                    Source = new MacAddress("01:01:01:01:01:01"),
                    Destination = new MacAddress("1C:4B:D6:D3:FE:AF"),
                    EtherType = EthernetType.None, // Will be filled automatically.
                };

            IpV4Layer ipV4Layer =
                new IpV4Layer
                {
                    Source = new IpV4Address("1.2.3.4"),
                    CurrentDestination = new IpV4Address("192.168.1.34"),
                    Fragmentation = IpV4Fragmentation.None,
                    HeaderChecksum = null, // Will be filled automatically.
                    Identification = 123,
                    Options = IpV4Options.None,
                    Protocol = null, // Will be filled automatically.
                    Ttl = 100,
                    TypeOfService = 0,
                };

            UdpLayer udpLayer =
                new UdpLayer
                {
                    SourcePort = 4050,
                    DestinationPort = 25,
                    Checksum = null, // Will be filled automatically.
                    CalculateChecksumValue = true,
                };

            PayloadLayer payloadLayer =
                new PayloadLayer
                {
                    Data = new Datagram(Encoding.ASCII.GetBytes("hello world")),
                };

            PacketBuilder builder = new PacketBuilder(ethernetLayer, ipV4Layer, udpLayer, payloadLayer);

            return builder.Build(DateTime.Now);
        }*/

        static void UdpYorumla()
        {
            while (true)
            {

                Packet p;
                if (kuyruk.Count > 0)
                    lock (kuyruk)
                    {
                        p = kuyruk.Dequeue();
                    }
                else continue;

                DateTime zaman = p.Timestamp;
                IpV4Datagram ip = p.Ethernet.IpV4;
                IcmpDatagram icmp = ip.Icmp;
                UdpDatagram udp = ip.Udp;
                ushort portr = udp.DestinationPort;
                Console.WriteLine(p);
                Console.WriteLine("Zaman = " + zaman + " / " + " Port =  UDP - " + udp.DestinationPort + " / " + " Kaynak = " + ip.Source);
                Kaydet(zaman, ip.Source, "UDP" , udp.DestinationPort);
                //Thread.Sleep(100);

            }
        }

        static void Kaydet(DateTime zaman , IpV4Address kaynak , string protokol , ushort port  )
        {
           
            
            var cmd = new SQLiteCommand(con);
            cmd.CommandText = "INSERT INTO Islem (Zaman,Kaynak,Protokol,Port) VALUES (@a,@b,@c,@d)";
            cmd.Parameters.AddWithValue("@a", zaman);
            cmd.Parameters.AddWithValue("@b", kaynak);
            cmd.Parameters.AddWithValue("@c", protokol);
            cmd.Parameters.AddWithValue("@d", port);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

        }
    }
}