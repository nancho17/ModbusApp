using System;
using System.Diagnostics;



namespace ConsoleApp1
{
    class Program
    {
        private EasyModbus.ModbusClient modConsoleClient;

        static void Main(string[] args)
        {
            Program ConsoleProgram = new();

            ConsoleProgram.ModInitialize();
            ConsoleProgram.ModConnect();
            ConsoleProgram.GetResponse();
            ConsoleProgram.SendData();
            ConsoleProgram.ModEnd();

        }

        private void ModInitialize()
        {
            modConsoleClient = new EasyModbus.ModbusClient();
            modConsoleClient.Port = 502;
            modConsoleClient.ReceiveDataChanged += new EasyModbus.ModbusClient.ReceiveDataChangedHandler(UpdateReceiveData);
            modConsoleClient.SendDataChanged += new EasyModbus.ModbusClient.SendDataChangedHandler(UpdateSendData);
            modConsoleClient.ConnectedChanged += new EasyModbus.ModbusClient.ConnectedChangedHandler(UpdateConnectedChanged);

            Console.WriteLine("MODBUS TCP");
            Console.WriteLine("Insert front panel local IP");
            var ipText = Console.ReadLine();
            if (ipText != null)
            {
                if (ipText.Length > 3)
                {
                    Console.WriteLine(ipText.Length.ToString());
                    modConsoleClient.IPAddress = ipText;
                }
            }
        }

        void UpdateConnectedChanged(object sender)
        {
            Console.WriteLine("0_ UpdateConnectedChanged call");
            if (modConsoleClient.Connected)
            {
                Console.WriteLine("Connected!");
            }
        }

        void UpdateReceiveData(object sender)
        {
            Console.WriteLine("1_ UpdateReceiveData call");
        }

        void UpdateSendData(object sender)
        {
            Console.WriteLine("2_ UpdateSendData call");
        }

        private void ModConnect()
        {
            try
            {
                Console.WriteLine("Connecting...");
                modConsoleClient.Connect(); //Connect Modbus TCP        

            }
            catch (Exception value)
            {
                Console.WriteLine(value.Message);
            }
        }

        private void GetResponse()
        {
            const int SETPOINT_ADDRESS = 3000;
            float[] temp_measurementsFloats = new float[0];
            try
            {
                if (modConsoleClient.Connected)
                {
                    Console.WriteLine("Reading some data");
                    Int32[] response = modConsoleClient.ReadHoldingRegisters(SETPOINT_ADDRESS, 20);
                    LoadFloatsFromReg(ref temp_measurementsFloats, response);
                }
            }
            catch (Exception value)
            {
                Console.WriteLine(value.Message);
            }

            Console.WriteLine("Received data (Setpoints):");
            for (int i = 0; i < temp_measurementsFloats.Length; i++)
            {
                Console.WriteLine("Float {0}:{1} ", i, temp_measurementsFloats[i]);
            }

        }
        private void SendData()
        {
            int[] data = new int[4];
            Array.Fill(data, 60);
            try
            {
                if (modConsoleClient.Connected)
                {
                    Console.WriteLine("Sending Some data");
                    modConsoleClient.WriteMultipleRegisters(3000, data);
                    Console.WriteLine("60 Hz to Frequency setpoint sended");
                }
            }
            catch (Exception value)
            {
                Console.WriteLine(value.Message);
            }


        }

        private void ModEnd()
        {
            try
            {
                modConsoleClient.Disconnect();
            }
            catch (Exception value)
            {
                Console.WriteLine(value.Message);
            }
            Console.WriteLine("Program End");
            Console.ReadKey();

        }

        public void LoadFloatsFromReg(ref float[] destination, int[] values)
        {
            int aux_val;
            for (int i = 0; i + 1 < values.Length; i += 2)
            {
                Array.Resize(ref destination, destination.Length + 1);
                aux_val = values[i + 1] << 16;
                aux_val += values[i] & 0x0000FFFF;
                destination[destination.GetUpperBound(0)] = BitConverter.Int32BitsToSingle(aux_val);
            }
        }
    }
}