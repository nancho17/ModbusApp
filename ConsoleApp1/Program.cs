using System;
using System.Diagnostics;



namespace ConsoleApp1
{
    class Program
    {
        private EasyModbus.ModbusClient modConsoleClient;
        enum ModbusSetpoints
        {
            Program_frequency = 0,
            Program_frequency_A,
            Program_frequency_B,
            Program_frequency_C,
            Program_voltage_AC,
            Program_voltage_AC_output_A,
            Program_voltage_AC_output_B,
            Program_voltage_AC_output_C,
            Program_voltage_DC,
            Program_voltage_DC_output_A,
            Program_voltage_DC_output_B,
            Program_voltage_DC_output_C,
            Power_limit,
            Power_limit_output_A,
            Power_limit_output_B,
            Power_limit_output_C,
            Current_limit_ABC,
            Current_limit_output_A,
            Current_limit_output_B,
            Current_limit_output_C,
            KVA_Limit,
            KVA_Limit_output_A,
            KVA_Limit_output_B,
            KVA_Limit_output_C,
            Phase_offset_output_B,
            Phase_offset_output_C,
            Enable_output
        };

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
                    modConsoleClient.IPAddress = ipText;
                }
            }
        }

        void UpdateConnectedChanged(object sender)
        {
            if (modConsoleClient.Connected)
            {
                Console.WriteLine("Connected!");
            }
        }

        void UpdateReceiveData(object sender)
        {
            //Console.WriteLine("Received Modbus Frame from TCP");
        }

        void UpdateSendData(object sender)
        {
            //Console.WriteLine("Sended a Modbus Frame");

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
            Int32[] configForm = new Int32[0];
            try
            {
                if (modConsoleClient.Connected)
                {
                    Console.WriteLine("Reading Measurement Setpoints");
                    Int32[] response = modConsoleClient.ReadHoldingRegisters(SETPOINT_ADDRESS, 2 * Enum.GetNames(typeof(ModbusSetpoints)).Length);
                    LoadFloatsFromReg(ref temp_measurementsFloats, response);
                    configForm = modConsoleClient.ReadHoldingRegisters(8000, 2);
                }
            }
            catch (Exception value)
            {
                Console.WriteLine(value.Message);
            }

            Console.WriteLine("Setpoints Readed data: ");
            if (temp_measurementsFloats.Length >= Enum.GetNames(typeof(ModbusSetpoints)).Length)
            {
                switch (configForm[0])
                {
                    case 1:
                        DrawPhaseA(temp_measurementsFloats);
                        break;

                    case 2:
                        DrawPhaseA(temp_measurementsFloats);
                        DrawPhaseB(temp_measurementsFloats);
                        break;

                    case 3:
                        DrawAll(temp_measurementsFloats);
                        break;

                    default:
                        Console.WriteLine("-------------------------------------");
                        Console.WriteLine("Form: {0} ", configForm[0]);
                        break;
                }
            }
        }
        private void DrawPhaseA(float[] floatData)
        {
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("Phase A setpoints");
            Console.WriteLine("Frequency: {0} ", floatData[(int)ModbusSetpoints.Program_frequency_A]);
            Console.WriteLine("Power limit: {0} ", floatData[(int)ModbusSetpoints.Power_limit_output_A]);
            Console.WriteLine("KVA Limit {0} ", floatData[(int)ModbusSetpoints.KVA_Limit_output_A]);
            Console.WriteLine("Current limit {0} ", floatData[(int)ModbusSetpoints.Current_limit_output_A]);
            Console.WriteLine("Power limit {0} ", floatData[(int)ModbusSetpoints.Power_limit_output_A]);
            Console.WriteLine("Program voltage DC {0} ", floatData[(int)ModbusSetpoints.Program_voltage_DC_output_A]);
            Console.WriteLine("Program voltage AC {0} ", floatData[(int)ModbusSetpoints.Program_voltage_AC_output_A]);
            Console.WriteLine("Power limit {0} ", floatData[(int)ModbusSetpoints.Power_limit_output_A]);

        }
        private void DrawPhaseB(float[] floatData)
        {
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("Phase B setpoints");
            Console.WriteLine("Frequency: {0} ", floatData[(int)ModbusSetpoints.Program_frequency_B]);
            Console.WriteLine("Power limit: {0} ", floatData[(int)ModbusSetpoints.Power_limit_output_B]);
            Console.WriteLine("KVA Limit {0} ", floatData[(int)ModbusSetpoints.KVA_Limit_output_B]);
            Console.WriteLine("Current limit {0} ", floatData[(int)ModbusSetpoints.Current_limit_output_B]);
            Console.WriteLine("Power limit {0} ", floatData[(int)ModbusSetpoints.Power_limit_output_B]);
            Console.WriteLine("Program voltage DC {0} ", floatData[(int)ModbusSetpoints.Program_voltage_DC_output_B]);
            Console.WriteLine("Program voltage AC {0} ", floatData[(int)ModbusSetpoints.Program_voltage_AC_output_B]);
            Console.WriteLine("Power limit {0} ", floatData[(int)ModbusSetpoints.Power_limit_output_B]);

        }
        private void DrawAll(float[] floatData)
        {
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("Setpoints");
            Console.WriteLine("Frequency: {0} ", floatData[(int)ModbusSetpoints.Program_frequency]);
            Console.WriteLine("Power limit: {0} ", floatData[(int)ModbusSetpoints.Power_limit]);
            Console.WriteLine("KVA Limit {0} ", floatData[(int)ModbusSetpoints.KVA_Limit]);
            Console.WriteLine("Current limit {0} ", floatData[(int)ModbusSetpoints.Current_limit_ABC]);
            Console.WriteLine("Power limit {0} ", floatData[(int)ModbusSetpoints.Power_limit]);
            Console.WriteLine("Program voltage DC {0} ", floatData[(int)ModbusSetpoints.Program_voltage_DC]);
            Console.WriteLine("Program voltage AC {0} ", floatData[(int)ModbusSetpoints.Program_voltage_AC]);
            Console.WriteLine("Power limit {0} ", floatData[(int)ModbusSetpoints.Power_limit]);

        }

        private void SendData()
        {

            float[] temp_floats = new float[] { 60, 60, 60, 60 };

            int[] int_data = new int[0];

            for (int i = 0; i < temp_floats.Length; i++)
            {

                int ab = (int)(0x0000FFFF & BitConverter.SingleToInt32Bits(temp_floats[i]));
                int aa = (int)(0x0000FFFF & BitConverter.SingleToInt32Bits(temp_floats[i]) >> 16);

                Array.Resize(ref int_data, int_data.Length + 1);
                int_data[int_data.GetUpperBound(0)] = ab;

                Array.Resize(ref int_data, int_data.Length + 1);
                int_data[int_data.GetUpperBound(0)] = aa;
            };


            try
            {
                if (modConsoleClient.Connected)
                {
                    Console.WriteLine("-------------------------------------");
                    Console.WriteLine("Setting Frequency TO 60 Hz");
                    modConsoleClient.WriteMultipleRegisters(3000, int_data);
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