using EasyModbus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private EasyModbus.ModbusClient modbusClientLocal;
        public MainPage()
        {
            this.InitializeComponent();
            ModInitialize();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            TestConnect();
        }

        private void TestConnect() //public static void mod (string args)
        {
            Debug.AutoFlush = true;
            ModConnectToServer();

            try
            {
                if (modbusClientLocal.Connected)
                {
                    modbusClientLocal.WriteMultipleCoils(4, new bool[] { true, true, true, true, true, true, true, true, true, true });    //Write Coils starting with Address 5
                    bool[] readCoils = modbusClientLocal.ReadCoils(8048, 10);                        //Read 10 Coils from Server, starting with address 10
                    int[] readHoldingRegisters = modbusClientLocal.ReadHoldingRegisters(3000, 10);    //Read 10 Holding Registers from Server, starting with Address 1
                    byte[] aux_bytes;
                    int aux_val;
                    float aux_float;

                    //Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
                    Debug.Indent();
                    Debug.WriteLine("He go");

                    // Console Output
                    for (int i = 0; i < readCoils.Length; i++)
                        Debug.WriteLine("Value of Coil " + (8048 + i) + " " + readCoils[i].ToString());

                    for (int i = 0; i < readHoldingRegisters.Length; i++)
                        Debug.WriteLine("Value of HoldingRegister " + (3000 + i) + " " + readHoldingRegisters[i].ToString());

                    aux_val = (readHoldingRegisters[1] << 16) + readHoldingRegisters[0];
                    aux_bytes = BitConverter.GetBytes(aux_val);
                    aux_float = BitConverter.ToSingle(aux_bytes, 0);
                    Debug.WriteLine("Value of Last " + aux_float.ToString());
                    Debug.Unindent();

                    SolidColorBrush greenBrush = new SolidColorBrush(Colors.DarkGreen);
                    noteGrid.Background = greenBrush;
                    connStatusBlock.Text = "Connected";

                    modbusClientLocal.Disconnect();//Disconnect from 
                }
            }
            catch (Exception value)
            {
                Debug.WriteLine(value.Message);
                SolidColorBrush aColorBrush = new SolidColorBrush(Colors.IndianRed);
                noteGrid.Background = aColorBrush;
                connStatusBlock.Text = "Comm Error";
            }
        }

        private void ModInitialize()
        {
            modbusClientLocal = new ModbusClient();    //Ip-Address and Port of Modbus-TCP-Server
            ipBox.Text = modbusClientLocal.IPAddress;
            portBox.Text = modbusClientLocal.Port.ToString();

        }

        private void ModConnectToServer()
        {
            try
            {
                modbusClientLocal.Connect();                                                    //Connect to Server        
            }
            catch (Exception value)
            {
                Debug.WriteLine(value.Message);
                SolidColorBrush aColorBrush = new SolidColorBrush(Colors.DarkRed);
                noteGrid.Background = aColorBrush;
                connStatusBlock.Text = value.Message;
            }

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            String ipText = ipBox.Text;

            if (ipText.Length > 2)
            {
                modbusClientLocal.IPAddress = ipText;
            }

            connStatusBlock.Text = ipText;
        }

        private void portBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (portBox.Text.Length > 2)
            {
                try
                {
                    int valRes = Int32.Parse(portBox.Text);
                    modbusClientLocal.Port = valRes;
                    Debug.WriteLine("K");
                }
                catch (FormatException f)
                {
                    Debug.WriteLine("Can't Parse M: " + f.Message);
                }
            }
        }

    }
}
