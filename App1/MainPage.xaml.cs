using EasyModbus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
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
            Apparent_power
        };

        public MainPage()
        {
            this.InitializeComponent();
            ModInitialize();
            InitConnectionPeriodic();
        }

        private int messageErrorCounter = 0;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SetModbusIP();
            SetModbusPort();
            CancelCommPeriodic();
        }

        private float[] MeasurementsFloats = new float[Enum.GetNames(typeof(ModbusMeasurements)).Length];
        private float[] SetPointsFloats = new float[Enum.GetNames(typeof(ModbusSetpoints)).Length];

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
            //TODO: Sync with variable
        }

        private IAsyncAction m_workItem;
        private bool conectionModFlagM;
        private String errorMessage = "";
        private void AsyConnect()
        {
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

            m_workItem = Windows.System.Threading.ThreadPool.RunAsync(workItemHandler, WorkItemPriority.Normal, WorkItemOptions.TimeSliced);

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

            m_workItem.Completed = workCompletedHandler;
        }

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
                    bool[] readCoils = modbusClientLocal.ReadCoils(4000, 5);    //Read 10 Coils from Server, starting with address 10
                    Int32[] MeasurementsRegisters = modbusClientLocal.ReadInputRegisters(MEASURE_ADDRESS, 2 * Enum.GetNames(typeof(ModbusMeasurements)).Length);    //Read Holding Registers from Server, starting with Address 1
                    Int32[] SetpointRegisters = modbusClientLocal.ReadHoldingRegisters(SETPOINT_ADDRESS, 2 * Enum.GetNames(typeof(ModbusSetpoints)).Length);    //Read Holding Registers from Server, starting with Address 1

                    //Get Measurements
                    float[] temp_measurementsFloats = new float[0];
                    LoadFloatsFromReg(ref temp_measurementsFloats, MeasurementsRegisters);
                    System.Array.Copy(temp_measurementsFloats, MeasurementsFloats, temp_measurementsFloats.Length);

                    //Get Setpoints
                    float[] temp_setpointsFloats = new float[0];
                    LoadFloatsFromReg(ref temp_setpointsFloats, SetpointRegisters);
                    System.Array.Copy(temp_setpointsFloats, SetPointsFloats, temp_setpointsFloats.Length); ;

                    //Show in UI
                    _ = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync
                    (
                        CoreDispatcherPriority.Normal,
                        () =>
                        {
                            ProgFreq.Text = SetPointsFloats[(int)ModbusSetpoints.Program_frequency].ToString();
                            ProgVoltAC.Text = SetPointsFloats[(int)ModbusSetpoints.Program_voltage_AC].ToString();
                            ProgVoltDC.Text = SetPointsFloats[(int)ModbusSetpoints.Program_voltage_DC].ToString();
                            PowLim.Text = SetPointsFloats[(int)ModbusSetpoints.Power_limit].ToString();
                            CuLimABC.Text = SetPointsFloats[(int)ModbusSetpoints.Current_limit_ABC].ToString();
                            KVALim.Text = SetPointsFloats[(int)ModbusSetpoints.KVA_Limit].ToString();
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
                if (messageErrorCounter > 1)
                {
                    messageErrorCounter = 0;
                    CancelCommPeriodic();
                }

            }

            return Task.CompletedTask;
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

    }
}