using EasyModbus;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Core;
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
    /// An empty page that can be used on its own or navigated to within messageErrorCounter Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private EasyModbus.ModbusClient modbusClientLocal;
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

        enum ModbusMeasurements
        {
            Frequency = 0,
            Output_A_frequency,
            Output_B_frequency,
            Output_C_frequency,
            Voltage_line_to_line_ACDC,
            Output_A_voltage_line_to_line_ACDC,
            Output_B_voltage_line_to_line_ACDC,
            Output_C_voltage_line_to_line_ACDC,
            Voltage_line_to_line_AC,
            Output_A_voltage_line_to_line_AC,
            Output_B_voltage_line_to_line_AC,
            Output_C_voltage_line_to_line_AC,
            Voltage_line_to_line_DC,
            Output_A_voltage_line_to_line_DC,
            Output_B_voltage_line_to_line_DC,
            Output_C_voltage_line_to_line_DC,
            Voltage_ACDC,
            Output_A_voltage_ACDC,
            Output_B_voltage_ACDC,
            Output_C_voltage_ACDC,
            Voltage_AC,
            Output_A_voltage_AC,
            Output_B_voltage_AC,
            Output_C_voltage_AC,
            Voltage_DC,
            Output_A_voltage_DC,
            Output_B_voltage_DC,
            Output_C_voltage_DC,
            Current_ACDC,
            Output_A_current_ACDC,
            Output_B_current_ACDC,
            Output_C_current_ACDC,
            Current_DC,
            Output_A_current_DC,
            Output_B_current_DC,
            Output_C_current_DC,
            Active_power,
            Output_A_active_power,
            Output_B_active_power,
            Output_C_active_power,
            Apparent_power,
            Output_A_apparent_power,
            Output_B_apparent_power,
            Output_C_apparent_power
        };

        enum ModbusFlagData
        {
            Output_Enable = 0,
            other_enable

        }

        public MainPage()
        {
            this.InitializeComponent();
            ModInitialize();
            InitConnectionPeriodic();
        }

        public int messageErrorCounter = 0;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SetModbusIP();
            SetModbusPort();
            CancelCommPeriodic();
        }

        private float[] MeasurementsFloats = new float[Enum.GetNames(typeof(ModbusMeasurements)).Length];
        private float[] SetPointsFloats = new float[Enum.GetNames(typeof(ModbusSetpoints)).Length];
        private bool[] FlagDataBools = new bool[5];

        private void ModInitialize()
        {
            modbusClientLocal = new EasyModbus.ModbusClient();
            modbusClientLocal.ReceiveDataChanged += new EasyModbus.ModbusClient.ReceiveDataChangedHandler(UpdateReceiveData);
            modbusClientLocal.SendDataChanged += new EasyModbus.ModbusClient.SendDataChangedHandler(UpdateSendData);
            modbusClientLocal.ConnectedChanged += new EasyModbus.ModbusClient.ConnectedChangedHandler(UpdateConnectedChanged);

            modbusClientLocal.IPAddress = "192.168.107.190";
            AsyConnect();

            ipBox.Text = modbusClientLocal.IPAddress;
            portBox.Text = modbusClientLocal.Port.ToString();
        }

        private void SetModbusIP()
        {
            String ipText = ipBox.Text;

            if (ipText.Length > 2)
            {
                modbusClientLocal.IPAddress = ipText;
            }
            connStatusBlock.Text = ipText;
        }

        private void IpBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //TODO: Parse text
        }

        private void SetModbusPort()
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

        private void PortBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //TODO: Parse text
        }
        private void OutEnSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(sender);
            //TODO: Sync with variable
        }




        /// <summary>
        /// Modbus Write phase A setpoints
        /// </summary>
        private void writeSetpointsPhaseA()
        {
            float[] temp_setpointsFloats = new float[Enum.GetNames(typeof(ModbusSetpoints)).Length];
            try
            {
                bool[] OutputState = new bool[1];
                System.Array.Copy(SetPointsFloats, temp_setpointsFloats, SetPointsFloats.Length);

                OutputState = modbusClientLocal.ReadCoils(4000, 1);
                if (modbusClientLocal.Connected && !OutputState[0])
                {
                    float aux = Single.Parse(ASetProgFreq.Text);
                    temp_setpointsFloats[(int)ModbusSetpoints.Program_frequency] = aux;
                    temp_setpointsFloats[(int)ModbusSetpoints.Program_frequency_A] = aux;
                    temp_setpointsFloats[(int)ModbusSetpoints.Program_frequency_B] = aux;
                    temp_setpointsFloats[(int)ModbusSetpoints.Program_frequency_C] = aux;

                    temp_setpointsFloats[(int)ModbusSetpoints.Program_voltage_AC_output_A] = Single.Parse(ASetProgVoltAC.Text);
                    temp_setpointsFloats[(int)ModbusSetpoints.Program_voltage_DC_output_A] = Single.Parse(ASetProgVoltDC.Text);
                    temp_setpointsFloats[(int)ModbusSetpoints.Power_limit_output_A] = Single.Parse(ASetPowLim.Text);
                    temp_setpointsFloats[(int)ModbusSetpoints.Current_limit_output_A] = Single.Parse(ASetCuLimABC.Text);
                    temp_setpointsFloats[(int)ModbusSetpoints.KVA_Limit_output_A] = Single.Parse(ASetKVALim.Text);

                    for (int k = 0; k < temp_setpointsFloats.Length; k++)
                    {
                        Debug.WriteLine("Float {0} = {1}", k, temp_setpointsFloats[k]);
                    }

                    int[] int_data = new int[0];
                    for (int i = 0; i < temp_setpointsFloats.Length; i++)
                    {

                        int ab = (int)(0x0000FFFF & BitConverter.SingleToInt32Bits(temp_setpointsFloats[i]));
                        int aa = (int)(0x0000FFFF & BitConverter.SingleToInt32Bits(temp_setpointsFloats[i]) >> 16);

                        Array.Resize(ref int_data, int_data.Length + 1);
                        int_data[int_data.GetUpperBound(0)] = ab;

                        Array.Resize(ref int_data, int_data.Length + 1);
                        int_data[int_data.GetUpperBound(0)] = aa;
                    };

                    for (int k = 0; k < int_data.Length; k++)
                    {
                        Debug.WriteLine("Data {0} = {1:X}", k, int_data[k]);
                    }

                    modbusClientLocal.WriteMultipleRegisters(3000, int_data);

                }

            }

            catch (Exception value)
            {
                errorMessage = value.Message;
            }

        }

        /// <summary>
        /// Modbus Write phase B setpoints
        /// </summary>
        private void writeSetpointsPhaseB()
        {
            float[] temp_setpointsFloats = new float[Enum.GetNames(typeof(ModbusSetpoints)).Length];
            try
            {
                bool[] OutputState = new bool[1];
                System.Array.Copy(SetPointsFloats, temp_setpointsFloats, SetPointsFloats.Length);

                OutputState = modbusClientLocal.ReadCoils(4000, 1);
                if (modbusClientLocal.Connected && !OutputState[0])
                {
                    float aux = Single.Parse(ASetProgFreq.Text);
                    temp_setpointsFloats[(int)ModbusSetpoints.Program_frequency] = aux;
                    temp_setpointsFloats[(int)ModbusSetpoints.Program_frequency_A] = aux;
                    temp_setpointsFloats[(int)ModbusSetpoints.Program_frequency_B] = aux;
                    temp_setpointsFloats[(int)ModbusSetpoints.Program_frequency_C] = aux;

                    temp_setpointsFloats[(int)ModbusSetpoints.Program_voltage_AC_output_B] = Single.Parse(BSetProgVoltAC.Text);
                    temp_setpointsFloats[(int)ModbusSetpoints.Program_voltage_DC_output_B] = Single.Parse(BSetProgVoltDC.Text);
                    temp_setpointsFloats[(int)ModbusSetpoints.Power_limit_output_B] = Single.Parse(BSetPowLim.Text);
                    temp_setpointsFloats[(int)ModbusSetpoints.Current_limit_output_B] = Single.Parse(BSetCuLimABC.Text);
                    temp_setpointsFloats[(int)ModbusSetpoints.KVA_Limit_output_B] = Single.Parse(BSetKVALim.Text);

                    for (int k = 0; k < temp_setpointsFloats.Length; k++)
                    {
                        Debug.WriteLine("Float {0} = {1}", k, temp_setpointsFloats[k]);
                    }

                    int[] int_data = new int[0];
                    for (int i = 0; i < temp_setpointsFloats.Length; i++)
                    {

                        int ab = (int)(0x0000FFFF & BitConverter.SingleToInt32Bits(temp_setpointsFloats[i]));
                        int aa = (int)(0x0000FFFF & BitConverter.SingleToInt32Bits(temp_setpointsFloats[i]) >> 16);

                        Array.Resize(ref int_data, int_data.Length + 1);
                        int_data[int_data.GetUpperBound(0)] = ab;

                        Array.Resize(ref int_data, int_data.Length + 1);
                        int_data[int_data.GetUpperBound(0)] = aa;
                    };

                    for (int k = 0; k < int_data.Length; k++)
                    {
                        Debug.WriteLine("Data {0} = {1:X}", k, int_data[k]);
                    }

                    modbusClientLocal.WriteMultipleRegisters(3000, int_data);

                }

            }

            catch (Exception value)
            {
                errorMessage = value.Message;
            }
        }


        /// <summary>
        ///  Modbus Write phase C setpoints
        /// </summary>
        private void writeSetpointsPhaseC()
        {
            float[] temp_setpointsFloats = new float[Enum.GetNames(typeof(ModbusSetpoints)).Length];
            try
            {
                bool[] OutputState = new bool[1];
                System.Array.Copy(SetPointsFloats, temp_setpointsFloats, SetPointsFloats.Length);

                OutputState = modbusClientLocal.ReadCoils(4000, 1);
                if (modbusClientLocal.Connected && !OutputState[0])
                {
                    float aux = Single.Parse(CSetProgFreq.Text);
                    temp_setpointsFloats[(int)ModbusSetpoints.Program_frequency] = aux;
                    temp_setpointsFloats[(int)ModbusSetpoints.Program_frequency_A] = aux;
                    temp_setpointsFloats[(int)ModbusSetpoints.Program_frequency_B] = aux;
                    temp_setpointsFloats[(int)ModbusSetpoints.Program_frequency_C] = aux;

                    temp_setpointsFloats[(int)ModbusSetpoints.Program_voltage_AC_output_C] = Single.Parse(CSetProgVoltAC.Text);
                    temp_setpointsFloats[(int)ModbusSetpoints.Program_voltage_DC_output_C] = Single.Parse(CSetProgVoltDC.Text);
                    temp_setpointsFloats[(int)ModbusSetpoints.Power_limit_output_C] = Single.Parse(CSetPowLim.Text);
                    temp_setpointsFloats[(int)ModbusSetpoints.Current_limit_output_C] = Single.Parse(CSetCuLimABC.Text);
                    temp_setpointsFloats[(int)ModbusSetpoints.KVA_Limit_output_C] = Single.Parse(CSetKVALim.Text);

                    for (int k = 0; k < temp_setpointsFloats.Length; k++)
                    {
                        Debug.WriteLine("Float {0} = {1}", k, temp_setpointsFloats[k]);
                    }

                    int[] int_data = new int[0];
                    for (int i = 0; i < temp_setpointsFloats.Length; i++)
                    {
                        int ab = (int)(0x0000FFFF & BitConverter.SingleToInt32Bits(temp_setpointsFloats[i]));
                        int aa = (int)(0x0000FFFF & BitConverter.SingleToInt32Bits(temp_setpointsFloats[i]) >> 16);

                        Array.Resize(ref int_data, int_data.Length + 1);
                        int_data[int_data.GetUpperBound(0)] = ab;

                        Array.Resize(ref int_data, int_data.Length + 1);
                        int_data[int_data.GetUpperBound(0)] = aa;
                    };

                    for (int k = 0; k < int_data.Length; k++)
                    {
                        Debug.WriteLine("Data {0} = {1:X}", k, int_data[k]);
                    }

                    modbusClientLocal.WriteMultipleRegisters(3000, int_data);
                }
            }

            catch (Exception value)
            {
                errorMessage = value.Message;
            }
        }




        private IAsyncAction modbusConnectionWorkItem;
        private bool conectionModFlagM;
        private String errorMessage = "";
        private void AsyConnect()
        {
            DispatchedHandler firstshowHandler = new DispatchedHandler
           (
               /*- Show "connecting" -*/

               () =>
               {
                   SolidColorBrush startColorBrush = new SolidColorBrush(Colors.DarkOrange);
                   noteGrid.Background = startColorBrush;
                   connStatusBlock.Text = "Connecting...";
               }
           );
            var otherIgnored = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, firstshowHandler);

            /*- Proceeed to connect -*/
            WorkItemHandler workItemHandler = new WorkItemHandler
               (
                   (IAsyncAction action) =>
                   {
                       try
                       {
                           modbusClientLocal.Disconnect();
                           Debug.WriteLine("Asycon");
                           modbusClientLocal.Connect(); //Connect Modbus TCP        
                           conectionModFlagM = true;
                       }
                       catch (Exception value)
                       {
                           errorMessage = value.Message;
                           conectionModFlagM = false;
                       }
                   }
               );

            modbusConnectionWorkItem = Windows.System.Threading.ThreadPool.RunAsync(workItemHandler, WorkItemPriority.Normal, WorkItemOptions.TimeSliced);

            DispatchedHandler showHandler = new DispatchedHandler
            (
                () =>
            {
                SolidColorBrush aColorBrush = new SolidColorBrush(Colors.DarkRed);
                if (conectionModFlagM)
                {
                    connStatusBlock.Text = "Connected";
                    aColorBrush = new SolidColorBrush(Colors.DarkOliveGreen);
                }
                else
                {
                    Debug.WriteLine("Not connected");
                    connStatusBlock.Text = errorMessage;
                    aColorBrush = new SolidColorBrush(Colors.DarkRed);
                }
                noteGrid.Background = aColorBrush;
            });

            AsyncActionCompletedHandler workCompletedHandler = new AsyncActionCompletedHandler
            (
                (IAsyncAction asyncInfo, AsyncStatus asyncStatus) =>
                {
                    if (asyncStatus == AsyncStatus.Canceled)
                    {
                        return;
                    }
                    var ignored = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, showHandler);
                }
            );

            modbusConnectionWorkItem.Completed = workCompletedHandler;
        }

        //Modbus events callbacks
        void UpdateConnectedChanged(object sender)
        {
            Debug.WriteLine("0_ UpdateConnectedChanged call");
            if (modbusClientLocal.Connected)
            {
                Debug.Indent();
                Debug.WriteLine("UC Connected!");
                Debug.Unindent();
                CancelConnectionPeriodic();


            }
            else
            {
                Debug.WriteLine("UC NOT Connected!");
            }
        }

        void UpdateReceiveData(object sender)
        {
            Debug.WriteLine("1_ UpdateReceiveData call");
            //byte[]  modbusClientLocal.receiveData;
        }

        void UpdateSendData(object sender)
        {
            Debug.WriteLine("2_ UpdateSendData call");
        }

        /*  ---  Connection Periodic ---    */
        ThreadPoolTimer ConnectionPeriodicTimer;

        public void CancelConnectionPeriodic()
        {
            if (ConnectionPeriodicTimer != null)
            {
                ConnectionPeriodicTimer.Cancel();
                ConnectionPeriodicTimer = null;
            }
        }

        public void InitConnectionPeriodic()
        {
            Debug.WriteLine("Conection go");
            TimeSpan period;
            period = TimeSpan.FromSeconds(2);

            modbusClientLocal.Disconnect();
            if (ConnectionPeriodicTimer != null || period == null)
            {
                return;
            }

            ConnectionPeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer
            (
                (source) =>
                {
                    AsyConnect();
                },
                period,
                (source) =>
                {

                    Debug.WriteLine("Connectionp Cancelled");
                    InitCommPeriodic();
                }
            );
        }

        /*  ---  Comm Periodic    ---    */
        private Task RequestModbusInThreadAsync()
        {
            try
            {
                if (modbusClientLocal.Connected)
                {
                    const int MEASURE_ADDRESS = 1000;
                    const int SETPOINT_ADDRESS = 3000;
                    Debug.WriteLine("Asking...");
                    //Modbus Register Request
                    bool[] ReadCoils = modbusClientLocal.ReadCoils(4000, 4);    //Read 10 Coils from Server, starting with address 10
                    Int32[] MeasurementsRegisters = modbusClientLocal.ReadInputRegisters(MEASURE_ADDRESS, 2 * Enum.GetNames(typeof(ModbusMeasurements)).Length);    //Read Holding Registers from Server, starting with Address 1
                    Int32[] SetpointRegisters = modbusClientLocal.ReadHoldingRegisters(SETPOINT_ADDRESS, 2 * Enum.GetNames(typeof(ModbusSetpoints)).Length);    //Read Holding Registers from Server, starting with Address 1
                    Int32[] ConfigForm = modbusClientLocal.ReadHoldingRegisters(8000, 2);

                    //Get Some Flags
                    System.Array.Copy(ReadCoils, FlagDataBools, ReadCoils.Length);

                    //Get Measurements
                    float[] temp_measurementsFloats = new float[0];
                    LoadFloatsFromReg(ref temp_measurementsFloats, MeasurementsRegisters);
                    System.Array.Copy(temp_measurementsFloats, MeasurementsFloats, temp_measurementsFloats.Length);

                    //Get Setpoints
                    float[] temp_setpointsFloats = new float[0];
                    LoadFloatsFromReg(ref temp_setpointsFloats, SetpointRegisters);
                    System.Array.Copy(temp_setpointsFloats, SetPointsFloats, temp_setpointsFloats.Length); ;

                    ThreeModeDataSet = new List<MeasureThree>();
                    SingleModeDataSet = new List<MeasureSingle>();
                    SplitModeDataSet = new List<MeasureSplit>();


                    //MeasureThree data        
                    AddCustomDataListThree(ref ThreeModeDataSet);
                    AddCustomDataListSingle(ref SingleModeDataSet);
                    AddCustomDataListSplit(ref SplitModeDataSet);

                    //Show received data
                    _ = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync
                    (
                        CoreDispatcherPriority.Normal,
                    () =>
                        {
                            //Set Switch Buton
                            OutEnSwitch.IsOn = FlagDataBools[(int)ModbusFlagData.Output_Enable];

                            //Setpoint data 
                            switch (ConfigForm[0])
                            {
                                case 1:
                                    //Set data in table   
                                    dataGrid1.ItemsSource = SingleModeDataSet;

                                    SetPhA.Visibility = Visibility.Visible;
                                    SetPhB.Visibility = Visibility.Collapsed;
                                    SetPhC.Visibility = Visibility.Collapsed;
                                    AProgFreq.Text = SetPointsFloats[(int)ModbusSetpoints.Program_frequency_A].ToString();
                                    AProgVoltAC.Text = SetPointsFloats[(int)ModbusSetpoints.Program_voltage_AC_output_A].ToString();
                                    AProgVoltDC.Text = SetPointsFloats[(int)ModbusSetpoints.Program_voltage_DC_output_A].ToString();
                                    APowLim.Text = SetPointsFloats[(int)ModbusSetpoints.Power_limit_output_A].ToString();
                                    ACuLimABC.Text = SetPointsFloats[(int)ModbusSetpoints.Current_limit_output_A].ToString();
                                    AKVALim.Text = SetPointsFloats[(int)ModbusSetpoints.KVA_Limit_output_A].ToString();
                                    break;

                                case 2:
                                    //Set data in table   
                                    dataGrid1.ItemsSource = SplitModeDataSet;

                                    SetPhA.Visibility = Visibility.Visible;
                                    SetPhB.Visibility = Visibility.Visible;
                                    SetPhC.Visibility = Visibility.Collapsed;

                                    AProgFreq.Text = SetPointsFloats[(int)ModbusSetpoints.Program_frequency_A].ToString();
                                    AProgVoltAC.Text = SetPointsFloats[(int)ModbusSetpoints.Program_voltage_AC_output_A].ToString();
                                    AProgVoltDC.Text = SetPointsFloats[(int)ModbusSetpoints.Program_voltage_DC_output_A].ToString();
                                    APowLim.Text = SetPointsFloats[(int)ModbusSetpoints.Power_limit_output_A].ToString();
                                    ACuLimABC.Text = SetPointsFloats[(int)ModbusSetpoints.Current_limit_output_A].ToString();
                                    AKVALim.Text = SetPointsFloats[(int)ModbusSetpoints.KVA_Limit_output_A].ToString();

                                    BProgFreq.Text = SetPointsFloats[(int)ModbusSetpoints.Program_frequency_B].ToString();
                                    BProgVoltAC.Text = SetPointsFloats[(int)ModbusSetpoints.Program_voltage_AC_output_B].ToString();
                                    BProgVoltDC.Text = SetPointsFloats[(int)ModbusSetpoints.Program_voltage_DC_output_B].ToString();
                                    BPowLim.Text = SetPointsFloats[(int)ModbusSetpoints.Power_limit_output_B].ToString();
                                    BCuLimABC.Text = SetPointsFloats[(int)ModbusSetpoints.Current_limit_output_B].ToString();
                                    BKVALim.Text = SetPointsFloats[(int)ModbusSetpoints.KVA_Limit_output_B].ToString();

                                    break;
                                case 3:
                                    dataGrid1.ItemsSource = ThreeModeDataSet;

                                    AProgFreq.Text = "Three";
                                    SetPhA.Visibility = Visibility.Visible;
                                    SetPhB.Visibility = Visibility.Visible;
                                    SetPhC.Visibility = Visibility.Visible;

                                    AProgFreq.Text = SetPointsFloats[(int)ModbusSetpoints.Program_frequency_A].ToString();
                                    AProgVoltAC.Text = SetPointsFloats[(int)ModbusSetpoints.Program_voltage_AC_output_A].ToString();
                                    AProgVoltDC.Text = SetPointsFloats[(int)ModbusSetpoints.Program_voltage_DC_output_A].ToString();
                                    APowLim.Text = SetPointsFloats[(int)ModbusSetpoints.Power_limit_output_A].ToString();
                                    ACuLimABC.Text = SetPointsFloats[(int)ModbusSetpoints.Current_limit_output_A].ToString();
                                    AKVALim.Text = SetPointsFloats[(int)ModbusSetpoints.KVA_Limit_output_A].ToString();

                                    BProgFreq.Text = SetPointsFloats[(int)ModbusSetpoints.Program_frequency_B].ToString();
                                    BProgVoltAC.Text = SetPointsFloats[(int)ModbusSetpoints.Program_voltage_AC_output_B].ToString();
                                    BProgVoltDC.Text = SetPointsFloats[(int)ModbusSetpoints.Program_voltage_DC_output_B].ToString();
                                    BPowLim.Text = SetPointsFloats[(int)ModbusSetpoints.Power_limit_output_B].ToString();
                                    BCuLimABC.Text = SetPointsFloats[(int)ModbusSetpoints.Current_limit_output_B].ToString();
                                    BKVALim.Text = SetPointsFloats[(int)ModbusSetpoints.KVA_Limit_output_B].ToString();

                                    CProgFreq.Text = SetPointsFloats[(int)ModbusSetpoints.Program_frequency_C].ToString();
                                    CProgVoltAC.Text = SetPointsFloats[(int)ModbusSetpoints.Program_voltage_AC_output_C].ToString();
                                    CProgVoltDC.Text = SetPointsFloats[(int)ModbusSetpoints.Program_voltage_DC_output_C].ToString();
                                    CPowLim.Text = SetPointsFloats[(int)ModbusSetpoints.Power_limit_output_C].ToString();
                                    CCuLimABC.Text = SetPointsFloats[(int)ModbusSetpoints.Current_limit_output_C].ToString();
                                    CKVALim.Text = SetPointsFloats[(int)ModbusSetpoints.KVA_Limit_output_C].ToString();
                                    break;

                                default:
                                    break;
                            }
                        }
                    );
                }
                else
                {
                    messageErrorCounter = 0;
                    CancelCommPeriodic();
                }
            }

            catch (Exception value)
            {
                messageErrorCounter++;
                Debug.Write("Fail: ");
                Debug.WriteLine(value.Message);
                if (messageErrorCounter > 0)
                {
                    messageErrorCounter = 0;
                    CancelCommPeriodic();
                }

            }
            return Task.CompletedTask;
        }

        public void LoadFloatsFromReg(ref float[] destination, Int32[] values)
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

        private ThreadPoolTimer CommPeriodicTimer;
        public void InitCommPeriodic()
        {
            TimeSpan period;
            period = TimeSpan.FromMilliseconds(1500);
            Debug.WriteLine("Comm go");
            if (CommPeriodicTimer != null || period == null)
            {
                return;
            }

            CommPeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer
            (
                async (source) =>
                {
                    await RequestModbusInThreadAsync();

                },
                period,
                (source) =>
                {
                    Debug.WriteLine("Comm Cancelled");
                    InitConnectionPeriodic();
                }
            );
        }

        public void CancelCommPeriodic()
        {
            if (CommPeriodicTimer != null)
            {
                CommPeriodicTimer.Cancel();
                CommPeriodicTimer = null;
            }
        }

        string[] RowNames = new string[8]
        {
            "Frequency",
            "Voltage L-N RMS (AC+DC)",//1
            "Voltage L-N RMS (AC)",
            "Voltage L-N (DC)",//3
            "Current RMS (AC+DC)",
            "Current (DC)",
            "Power",//6
            "Aparent Power",//7

        };

        //0 Phase A
        //1 Phase B
        //2 Phase C
        //3 All
        int[,] RowStructs = new int[8, 4]
        {
            {
                (int)ModbusMeasurements.Output_A_frequency,
                (int)ModbusMeasurements.Output_B_frequency,
                (int)ModbusMeasurements.Output_C_frequency,
                (int)ModbusMeasurements.Frequency
            }
            ,
            {
                (int) ModbusMeasurements.Output_A_voltage_ACDC,
                (int) ModbusMeasurements.Output_B_voltage_ACDC,
                (int) ModbusMeasurements.Output_C_voltage_ACDC,
                (int) ModbusMeasurements.Voltage_ACDC
            }
            ,
            {
                (int) ModbusMeasurements.Output_A_voltage_line_to_line_AC,
                (int) ModbusMeasurements.Output_B_voltage_line_to_line_AC,
                (int) ModbusMeasurements.Output_C_voltage_line_to_line_AC,
                (int) ModbusMeasurements.Voltage_line_to_line_AC
            }
            ,
            {
                (int) ModbusMeasurements.Output_A_voltage_line_to_line_DC,
                (int) ModbusMeasurements.Output_B_voltage_line_to_line_DC,
                (int) ModbusMeasurements.Output_C_voltage_line_to_line_DC,
                (int) ModbusMeasurements.Voltage_line_to_line_DC
            }
            ,
            {
                (int) ModbusMeasurements.Output_A_current_ACDC,
                (int) ModbusMeasurements.Output_B_current_ACDC,
                (int) ModbusMeasurements.Output_C_current_ACDC,
                (int) ModbusMeasurements.Current_ACDC
            }
            ,
            {
                (int) ModbusMeasurements.Output_A_current_DC,
                (int) ModbusMeasurements.Output_B_current_DC,
                (int) ModbusMeasurements.Output_C_current_DC,
                (int) ModbusMeasurements.Current_DC
            }
            ,
            {
                (int) ModbusMeasurements.Output_A_active_power,
                (int) ModbusMeasurements.Output_B_active_power,
                (int) ModbusMeasurements.Output_C_active_power,
                (int) ModbusMeasurements.Active_power
            }
            ,
            {
                (int) ModbusMeasurements.Output_A_apparent_power,
                (int) ModbusMeasurements.Output_B_apparent_power,
                (int) ModbusMeasurements.Output_C_apparent_power,
                (int) ModbusMeasurements.Apparent_power
            }

        };

        public static List<MeasureThree> ThreeModeDataSet;

        private void AddCustomDataListThree(ref List<MeasureThree> SomeList)
        {
            for (int i = 0; i < RowNames.Length; i++)
            {
                SomeList.Add
                (
                    new MeasureThree
                    (
                        RowNames[i],
                        MeasurementsFloats[RowStructs[i, 0]].ToString(),
                        MeasurementsFloats[RowStructs[i, 1]].ToString(),
                        MeasurementsFloats[RowStructs[i, 2]].ToString(),
                        MeasurementsFloats[RowStructs[i, 3]].ToString()
                    )
                );
            }
        }

        public static List<MeasureSingle> SingleModeDataSet;

        private void AddCustomDataListSingle(ref List<MeasureSingle> SomeList)
        {
            for (int i = 0; i < RowNames.Length; i++)
            {
                SomeList.Add
                (
                    new MeasureSingle
                    (
                        RowNames[i],
                        MeasurementsFloats[RowStructs[i, 0]].ToString()
                    )
                );
            }
        }

        public static List<MeasureSplit> SplitModeDataSet;

        private void AddCustomDataListSplit(ref List<MeasureSplit> SomeList)
        {
            for (int i = 0; i < RowNames.Length; i++)
            {
                float total;
                switch (i)
                {
                    case 1:
                    case 6:
                    case 7:
                        total = MeasurementsFloats[RowStructs[i, 0]] + (MeasurementsFloats[RowStructs[i, 1]]);
                        break;

                    case 3:
                        total = MeasurementsFloats[RowStructs[i, 0]] - (MeasurementsFloats[RowStructs[i, 1]]);
                        break;

                    default:
                        total = MeasurementsFloats[RowStructs[i, 3]];
                        break;
                }

                SomeList.Add
                (
                    new MeasureSplit
                    (
                        RowNames[i],
                        MeasurementsFloats[RowStructs[i, 0]].ToString(),
                        MeasurementsFloats[RowStructs[i, 1]].ToString(),
                        total.ToString()
                    )
                );
            }
        }

        private IAsyncAction modbusWriteWorkItemA;

        private void Update_Setpoint_ClickA(object sender, RoutedEventArgs e)
        {
            DispatchedHandler workItemHandler = new DispatchedHandler
               (
                   () =>
                   {
                       writeSetpointsPhaseA();
                   }
               );
            modbusWriteWorkItemA = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, workItemHandler);
        }

        private void Update_Setpoint_ClickB(object sender, RoutedEventArgs e)
        {
            DispatchedHandler workItemHandler = new DispatchedHandler
                (
                    () =>
                    {
                        writeSetpointsPhaseB();
                    }
                );
            modbusWriteWorkItemA = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, workItemHandler);
        }

        private void Update_Setpoint_ClickC(object sender, RoutedEventArgs e)
        {
            DispatchedHandler workItemHandler = new DispatchedHandler
                (
                    () =>
                    {
                        writeSetpointsPhaseC();
                    }
                );
            modbusWriteWorkItemA = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, workItemHandler);
        }

        private void OutEnSwitch_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //TODO: WAIT SWITCH State Change
            bool OutputState = !OutEnSwitch.IsOn;

            try
            {
                if (modbusClientLocal.Connected)
                {
                    bool[] SetCoils = new bool[1];
                    SetCoils[0] = OutputState;
                    Debug.WriteLine("To Send Modbus {0}", SetCoils[0]);
                    modbusClientLocal.WriteMultipleCoils(4000, SetCoils);

                }
            }
            catch (Exception value)
            {
                Debug.WriteLine(value.Message);
            }

        }


        private void dataGrid1_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString() == "ZeroColumn")
            {
                e.Column.Header = "Measurements";
            }
            if (e.Column.Header.ToString() == "OneColumn")
            {
                e.Column.Header = "Phase A";
            }
            if (e.Column.Header.ToString() == "TwoColumn")
            {
                e.Column.Header = "Phase B";
            }
            if (e.Column.Header.ToString() == "ThreeColumn")
            {
                e.Column.Header = "Phase C";
            }
            if (e.Column.Header.ToString() == "FourColumn")
            {
                e.Column.Header = "Total";
            }
            if (e.Column.Header.ToString() == "SplitColumn")
            {
                e.Column.Header = "Split A-B";
            }
        }
    }

    public class MeasureThree
    {
        public String ZeroColumn { get; set; }
        public String OneColumn { get; set; }
        public String TwoColumn { get; set; }
        public String ThreeColumn { get; set; }
        public String FourColumn { get; set; }

        public MeasureThree(String zeroColumn, String A,
            String B, String C, String D)
        {
            this.ZeroColumn = zeroColumn;
            this.OneColumn = A;
            this.TwoColumn = B;
            this.ThreeColumn = C;
            this.FourColumn = D;
        }
    }

    public class MeasureSingle
    {
        public String ZeroColumn { get; set; }
        public String OneColumn { get; set; }

        public MeasureSingle(String zeroColumn, String A)
        {
            this.ZeroColumn = zeroColumn;
            this.OneColumn = A;
        }
    }
    public class MeasureSplit
    {
        public String ZeroColumn { get; set; }
        public String OneColumn { get; set; }
        public String TwoColumn { get; set; }
        public String SplitColumn { get; set; }

        public MeasureSplit(String zeroColumn, String A,
            String B, String C)
        {
            this.ZeroColumn = zeroColumn;
            this.OneColumn = A;
            this.TwoColumn = B;
            this.SplitColumn = C;
        }
    }

}