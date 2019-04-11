/* 
 * Project:    ViKeepon - Bruface Program - MA1 Project
 * Author:     CAO Hoang Long
 * Created:    Feb 2012
 * Contact:    http://chlong.tk or http://vikeepon.tk
 * Notes:      Control KEEPON via RS232 communication
 */

#region Namespace Inclusions
using System;
using System.Data;
using System.Text;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using SerialPortTerminal.Properties;
using System.Media;
using System.Threading;
using System.IO;
#endregion


namespace SerialPortTerminal
{
  #region Public Enumerations
  public enum DataMode { Text, Hex }
  public enum LogMsgType { Incoming, Outgoing, Normal, Warning, Error };
  
  #endregion

  public partial class frmTerminal : Form
  {
    #region Local Variables

    // The main control for communicating through the RS-232 port
    private SerialPort comport = new SerialPort();

    // Various colors for logging info
    private Color[] LogMsgTypeColor = { Color.Blue, Color.Green, Color.Black, Color.Orange, Color.Red };

    // Temp holder for whether a key was pressed
    private bool KeyHandled = false;

    byte[] buffer = new byte[7];
    int bytesread = 0;
        
    #endregion

    #region Constructor
    public frmTerminal()
    {
      // Build the form
      InitializeComponent();

      // Restore the users settings
      InitializeControlValues();

      // Enable/disable controls based on the current state
      EnableControls();

      // When data is recieved through the port, call this method
      comport.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
    }
    #endregion

    #region Local Methods
    
    /// <summary> Save the user's settings. </summary>
    private void SaveSettings()
    {
        Settings.Default.BaudRate = int.Parse(cmbBaudRate.Text);
        Settings.Default.DataBits = int.Parse(cmbDataBits.Text);
        //Settings.Default.DataMode = CurrentDataMode;
        Settings.Default.Parity = (Parity)Enum.Parse(typeof(Parity), cmbParity.Text);
        Settings.Default.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cmbStopBits.Text);
        Settings.Default.PortName = cmbPortName.Text;
        Settings.Default.Save();
    }

    /// <summary> Populate the form's controls with default settings. </summary>
    private void InitializeControlValues()
    {
      cmbParity.Items.Clear(); cmbParity.Items.AddRange(Enum.GetNames(typeof(Parity)));
      cmbStopBits.Items.Clear(); cmbStopBits.Items.AddRange(Enum.GetNames(typeof(StopBits)));

      cmbParity.Text = Settings.Default.Parity.ToString();
      cmbStopBits.Text = Settings.Default.StopBits.ToString();
      cmbDataBits.Text = Settings.Default.DataBits.ToString();
      cmbParity.Text = Settings.Default.Parity.ToString();
      cmbBaudRate.Text = Settings.Default.BaudRate.ToString();
      //CurrentDataMode = Settings.Default.DataMode;

      cmbPortName.Items.Clear();
      foreach (string s in SerialPort.GetPortNames())
        cmbPortName.Items.Add(s);

      if (cmbPortName.Items.Contains(Settings.Default.PortName)) cmbPortName.Text = Settings.Default.PortName;
      else if (cmbPortName.Items.Count > 0) cmbPortName.SelectedIndex = 0;
      else
      {
        MessageBox.Show(this, "There are no COM Ports detected on this computer.\nPlease install a COM Port and restart this app.", "No COM Ports Installed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        this.Close();
      }
    }

    /// <summary> Enable/disable controls based on the app's current state. </summary>
    private void EnableControls()
    {
      // Enable/disable controls based on whether the port is open or not
      gbPortSettings.Enabled = !comport.IsOpen;
      txtSendData.Enabled = btnSend.Enabled = comport.IsOpen;
      
      groupBox_Mode.Enabled = comport.IsOpen;
      comboBox_language.SelectedIndex = 2;
      groupBox_Sounds.Enabled = comport.IsOpen;
      groupBox_Movements.Enabled = comport.IsOpen;
      groupBox_Macros.Enabled = comport.IsOpen;
        

      if (comport.IsOpen) btnOpenPort.Text = "&Close Port";
      else btnOpenPort.Text = "&Open Port";
    }

    /// <summary> Send the user's data currently entered in the 'send' box.</summary>
    private void SendData()
    {
      {
        try
        {
          // Convert the user's string of hex digits (ex: B4 CA E2) to a byte array
          byte[] data = HexStringToByteArray(txtSendData.Text);

          // Send the binary data out the port
          comport.Write(data, 0, data.Length);

          // Show the hex digits on in the terminal window
          Log(LogMsgType.Outgoing, ByteArrayToHexString(data) + "\n");
        }
        catch (FormatException)
        {
          // Inform the user if the hex string was not properly formatted
          Log(LogMsgType.Error, "Not properly formatted hex string: " + txtSendData.Text + "\n");
        }
      }
      txtSendData.SelectAll();
    }
    /// <summary> Send Up-Down position</summary>
    private void SendPositionUpDown()
    {
        {
            try
            {
                // Convert the user's string of hex digits (ex: B4 CA E2) to a byte array
                byte[] data = HexStringToByteArray("55 00 03 00 80 FF");

                // Send the binary data out the port
                comport.Write(data, 0, data.Length);

                // Show the hex digits on in the terminal window
                Log(LogMsgType.Outgoing, ByteArrayToHexString(data) + "\n");
            }
            catch (FormatException)
            {
                // Inform the user if the hex string was not properly formatted
                //Log(LogMsgType.Error, "Not properly formatted hex string: " + txtSendData.Text + "\n");
            }
        }
        
    }

    /// <summary> Send Left-Right position</summary>
    private void SendPositionLeftRight()
    {
        Send_Commands("55 00 03 00 79 FF");
        Thread.Sleep(500);
        Send_Commands("55 00 03 00 01 FF");
    }

    /// <summary> Send Base position</summary>
    private void SendPositionBase()
    {
        {
            try
            {
                // Convert the user's string of hex digits (ex: B4 CA E2) to a byte array
                byte[] data = HexStringToByteArray("55 00 03 04" + ((byte) (255-numericUpDown3.Value)).ToString("X2") + "A0");

                // Send the binary data out the port
                comport.Write(data, 0, data.Length);

                // Show the hex digits on in the terminal window
                Log(LogMsgType.Outgoing, ByteArrayToHexString(data) + "\n");
            }
            catch (FormatException)
            {
                // Inform the user if the hex string was not properly formatted
                //Log(LogMsgType.Error, "Not properly formatted hex string: " + txtSendData.Text + "\n");
            }
        }

    }

    /// <summary> Send Forward-Backward position</summary>
    private void SendPositionForwardBackward()
    {
        {
            try
            {
                // Convert the user's string of hex digits (ex: B4 CA E2) to a byte array
                byte[] data = HexStringToByteArray("55 00 03 02" + ((byte)(255-numericUpDown4.Value)).ToString("X2") + "A0");

                // Send the binary data out the port
                comport.Write(data, 0, data.Length);

                // Show the hex digits on in the terminal window
                Log(LogMsgType.Outgoing, ByteArrayToHexString(data) + "\n");
            }
            catch (FormatException)
            {
                // Inform the user if the hex string was not properly formatted
                //Log(LogMsgType.Error, "Not properly formatted hex string: " + txtSendData.Text + "\n");
            }
        }

    }

    /// <summary> Log data to the terminal window. </summary>
    /// <param name="msgtype"> The type of message to be written. </param>
    /// <param name="msg"> The string containing the message to be shown. </param>
    private void Log(LogMsgType msgtype, string msg)
    {
      rtfTerminal.Invoke(new EventHandler(delegate
      {
        rtfTerminal.SelectedText = string.Empty;
        rtfTerminal.SelectionFont = new Font(rtfTerminal.SelectionFont, FontStyle.Bold);
        rtfTerminal.SelectionColor = LogMsgTypeColor[(int)msgtype];
        rtfTerminal.AppendText(msg);
        rtfTerminal.ScrollToCaret();
      }));
    }

    private void Display_buttons(byte but_position)
    {
        switch (but_position)
        {
            case 0x04:
                {
                    head_but.Invoke(new EventHandler(delegate
                    {
                        head_but.Checked = true;
                    }));
                    System.Media.SoundPlayer aSoundPlayer = new System.Media.SoundPlayer(@"Sounds\Ouch.WAV");  //Creates a sound player with the mentioned file. You can even load a stream in to this class
                    aSoundPlayer.Play();  //Plays the sound in a new thread
                    SendPositionUpDown();
                    Thread.Sleep(200);
                    comboBox_language.Invoke(new EventHandler(delegate
                    {
                        switch (comboBox_language.Text)
                        {
                            case "English": aSoundPlayer = new System.Media.SoundPlayer(@"Sounds\dont_hit_me.wav"); aSoundPlayer.Play(); break;
                            case "Dutch": aSoundPlayer = new System.Media.SoundPlayer(@"Sounds\dont_hit_me_dutch.wav"); aSoundPlayer.Play(); break;
                            case "French": aSoundPlayer = new System.Media.SoundPlayer(@"Sounds\dont_hit_me_french.wav"); aSoundPlayer.Play(); break;
                            case "Vietnamese": aSoundPlayer = new System.Media.SoundPlayer(@"Sounds\dont_hit_me_viet.wav"); aSoundPlayer.Play(); break;
                            case "Chinese": aSoundPlayer = new System.Media.SoundPlayer(@"Sounds\dont_hit_me_chinese.wav"); aSoundPlayer.Play(); break;
                            case "Shanghainese": aSoundPlayer = new System.Media.SoundPlayer(@"Sounds\dont_hit_me_shanghai.wav"); aSoundPlayer.Play(); break;
                            //default: aSoundPlayer = new System.Media.SoundPlayer(@"Sounds\Hello.WAV"); aSoundPlayer.Play(); break;
                        }
                    }));
                    Thread.Sleep(200);
                    head_but.Invoke(new EventHandler(delegate
                    {
                        head_but.Checked = false;
                    }));
                    break;
                }
            case 0x10:
                {
                    left_but.Invoke(new EventHandler(delegate
                    {
                        left_but.Checked = true;
                    }));
                    Send_Commands("55 00 03 00 00 FF");
                    System.Media.SoundPlayer aSoundPlayer = new System.Media.SoundPlayer(@"Sounds\Laugh.WAV");  //Creates a sound player with the mentioned file. You can even load a stream in to this class
                    aSoundPlayer.Play();  //Plays the sound in a new thread
                    Thread.Sleep(600);
                    Send_Commands("55 00 03 00 79 FF");
                    Thread.Sleep(500);
                    Send_Commands("55 00 03 00 01 FF");
                    
                    left_but.Invoke(new EventHandler(delegate
                    {
                        left_but.Checked = false ;
                    }));
                    break;
                }
            case 0x20:
                {
                    front_but.Invoke(new EventHandler(delegate
                    {
                        front_but.Checked = true;
                    }));
                    Send_Commands("55 00 03 02 FF A0");

                    comboBox_language.Invoke(new EventHandler(delegate
                    {
                        switch (comboBox_language.Text)
                        {
                            case "English": System.Media.SoundPlayer aSoundPlayer = new System.Media.SoundPlayer(@"Sounds\Hello.WAV"); aSoundPlayer.Play(); break;
                            case "Dutch": aSoundPlayer = new System.Media.SoundPlayer(@"Sounds\hello_dutch.wav"); aSoundPlayer.Play(); break;
                            case "French": aSoundPlayer = new System.Media.SoundPlayer(@"Sounds\hello_french.wav"); aSoundPlayer.Play(); break;
                            case "Vietnamese": aSoundPlayer = new System.Media.SoundPlayer(@"Sounds\hello_viet.wav"); aSoundPlayer.Play(); break;
                            case "Chinese": aSoundPlayer = new System.Media.SoundPlayer(@"Sounds\hello_chinese.wav"); aSoundPlayer.Play(); break;
                            case "Shanghainese": aSoundPlayer = new System.Media.SoundPlayer(@"Sounds\hello_chinese_shanghai.wav"); aSoundPlayer.Play(); break;
                            default: aSoundPlayer = new System.Media.SoundPlayer(@"Sounds\Hello.WAV"); aSoundPlayer.Play(); break;
                        }
                    }));
                    Thread.Sleep(500);
                    Send_Commands("55 00 03 02 80 A0");
                    front_but.Invoke(new EventHandler(delegate
                    {
                        front_but.Checked = false ;
                    }));
                    break;
                }
            case 0x40:
                {
                    right_but.Invoke(new EventHandler(delegate
                    {
                        right_but.Checked = true;
                    }));
                    Send_Commands ("55 00 03 00 00 FF");
                    System.Media.SoundPlayer aSoundPlayer = new System.Media.SoundPlayer(@"Sounds\Laugh.WAV");  //Creates a sound player with the mentioned file. You can even load a stream in to this class
                    aSoundPlayer.Play();  //Plays the sound in a new thread
                    Thread.Sleep(600);
                    Send_Commands("55 00 03 00 79 FF");
                    Thread.Sleep(500);
                    Send_Commands("55 00 03 00 01 FF");
                    right_but.Invoke(new EventHandler(delegate
                    {
                        right_but.Checked = false ;
                    }));
                    
                    break;
                }
            case 0x80: // AVOID TOUCHING BACK BUTTON
                {
                    /*
                    back_but.Invoke(new EventHandler(delegate
                    {
                        back_but.Checked = true;
                    }));
                    Send_Commands("55 00 03 02 43 A0");
                    System.Media.SoundPlayer aSoundPlayer = new System.Media.SoundPlayer(@"Sounds\Ouch.WAV");  //Creates a sound player with the mentioned file. You can even load a stream in to this class
                    aSoundPlayer.Play();  //Plays the sound in a new thread
                    Thread.Sleep(1000);
                    Send_Commands("55 00 03 02 80 A0");
                    back_but.Invoke(new EventHandler(delegate
                    {
                        back_but.Checked = false ;
                    }));
                     */
                    break;
                }
            default:
                {                    
                    break;
                }
        }
        
    }

    /// <summary> Convert a string of hex digits (ex: E4 CA B2) to a byte array. </summary>
    /// <param name="s"> The string containing the hex digits (with or without spaces). </param>
    /// <returns> Returns an array of bytes. </returns>
    private byte[] HexStringToByteArray(string s)
    {
      s = s.Replace(" ", "");
      byte[] buffer = new byte[s.Length / 2];
      for (int i = 0; i < s.Length; i += 2)
        buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
      return buffer;
    }

    /// <summary> Converts an array of bytes into a formatted string of hex digits (ex: E4 CA B2)</summary>
    /// <param name="data"> The array of bytes to be translated into a string of hex digits. </param>
    /// <returns> Returns a well formatted string of hex digits with spacing. </returns>
    private string ByteArrayToHexString(byte[] data)
    {
      StringBuilder sb = new StringBuilder(data.Length * 3);
      foreach (byte b in data)
        sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
      return sb.ToString().ToUpper();
    }
    #endregion

    #region Local Properties
    #endregion

    #region Event Handlers
    private void lnkAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      
    }
    
    private void frmTerminal_Shown(object sender, EventArgs e)
    {
      Log(LogMsgType.Error, String.Format("Welcome to ViKeepon!\n"));
    }
    private void frmTerminal_FormClosing(object sender, FormClosingEventArgs e)
    {
      // The form is closing, save the user's preferences
      SaveSettings();
    }
    private void cmbBaudRate_Validating(object sender, CancelEventArgs e)
    { int x; e.Cancel = !int.TryParse(cmbBaudRate.Text, out x); }
    private void cmbDataBits_Validating(object sender, CancelEventArgs e)
    { int x; e.Cancel = !int.TryParse(cmbDataBits.Text, out x); }

    private void btnOpenPort_Click(object sender, EventArgs e)
    {
      // If the port is open, close it.
        if (comport.IsOpen)
        {
            try
            {
                comport.Close();
                Log(LogMsgType.Warning, String.Format(cmbPortName.Text + " is closed!\n"));
            }
            catch (IOException)
            {
                Log(LogMsgType.Error, String.Format(cmbPortName.Text + " is not connected. Try again!\n"));
            }
            
        }
        else
        {
            // Set the port's settings
            comport.BaudRate = int.Parse(cmbBaudRate.Text);
            comport.DataBits = int.Parse(cmbDataBits.Text);
            comport.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cmbStopBits.Text);
            comport.Parity = (Parity)Enum.Parse(typeof(Parity), cmbParity.Text);
            comport.PortName = cmbPortName.Text;

            // Open the port
            try
            {
                comport.Open();
            }
            catch (IOException)
            {
                Log(LogMsgType.Error, String.Format(cmbPortName.Text + " is not connected. Try again!\n"));
            }
        }

      // Change the state of the form's controls
      EnableControls();

      // If the port is open, send focus to the send data box
      if (comport.IsOpen)
      {
          txtSendData.Focus();
          Log(LogMsgType.Outgoing, String.Format(cmbPortName.Text+ " is ready!\n"));
      }
    }
    private void btnSend_Click(object sender, EventArgs e)
    { SendData(); }

    private bool checkbitclear(byte b, int pos)
    {
        return (~b & (1 << pos)) != 0;
    }
    private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        
        try
        {
            // This method will be called when there is data waiting in the port's buffer
            bool PS2_mode = false;

            //Wait for buffer to be full
            Thread.Sleep(10);
            
            // Obtain the number of bytes waiting in the port's buffer
            int bytes = comport.BytesToRead;

            // Create a byte array buffer to hold the incoming data
            byte[] buffer_display = new byte[bytes];
            byte[] buffer = buffer_display;
            // Read the data from the port and store it in our buffer
            if ((bytes >= 7)&&(bytes <12)) // 
            {
                comport.Read(buffer, 0, bytes);
                // Show the user the incoming data in hex format
                Log(LogMsgType.Incoming, ByteArrayToHexString(buffer_display) + "\n");
            }
            else // Clear junk data
            {
                comport.DiscardOutBuffer();
                comport.DiscardInBuffer();
            }


            // Button data received  
            if ((buffer[0] == 0x50) && (buffer.Length > 6))
            {
                Display_buttons(buffer[1]);
            }

            // Check PS2_mode
            radioButton_PS2.Invoke(new EventHandler(delegate
            {
                if (radioButton_PS2.Checked == true)
                    PS2_mode = true;
                else PS2_mode = false;
            }));

            // PS2 data received  
            if ((buffer[0] == 0x45) && (buffer.Length > 6) && (PS2_mode == true))
            {
                // First byte
                if (checkbitclear(buffer[1], 0))// Select
                {
                }
                if (checkbitclear(buffer[1], 3))// Start
                {
                    Display_buttons(0x04);
                }
                if (checkbitclear(buffer[1], 4))// Up
                {
                    SendPositionUpDown();
                }
                if (checkbitclear(buffer[1], 5))// Right
                {
                    SendPositionLeftRight();
                }
                if (checkbitclear(buffer[1], 6))// Down
                {
                    SendPositionUpDown();
                }
                if (checkbitclear(buffer[1], 7))// Left
                {
                    SendPositionLeftRight();
                }
                // Second byte
                if (checkbitclear(buffer[2], 0))// L2
                {
                }
                if (checkbitclear(buffer[2], 1))// R2
                {
                }
                if (checkbitclear(buffer[2], 2))// L1
                {
                }
                if (checkbitclear(buffer[2], 3))// R1
                {
                }
                if (checkbitclear(buffer[2], 4))// /\
                {
                    Display_buttons(0x20);
                }
                if (checkbitclear(buffer[2], 5))// O
                {
                    Display_buttons(0x40);
                }
                if (checkbitclear(buffer[2], 6))// X
                {
                    Display_buttons(0x80);
                }
                if (checkbitclear(buffer[2], 7))// []
                {
                    Display_buttons(0x10);
                }
                // Analog mode
                if ((buffer[3] != 0xFF) && (buffer[4] != 0xFF) && (buffer[5] != 0xFF) && (buffer[6] != 0xFF))
                {
                    // Forward/Backward
                    Send_Commands("55 00 03 02" + ((byte)(buffer[4])).ToString("X2") + "A0");
                    // Base Rotation
                    Send_Commands("55 00 03 04" + ((byte)(buffer[6])).ToString("X2") + "FF");
                }
            }
            
        }
        catch (IndexOutOfRangeException)
        {

        }
      }

 
    private void txtSendData_KeyDown(object sender, KeyEventArgs e)
    { 
      // If the user presses [ENTER], send the data now
      if (KeyHandled = e.KeyCode == Keys.Enter) { e.Handled = true; SendData(); } 
    }
    private void txtSendData_KeyPress(object sender, KeyPressEventArgs e)
    { e.Handled = KeyHandled; }
    #endregion

  

    private void numericUpDown3_ValueChanged(object sender, EventArgs e)
    {
        trackBar3.Value = (byte)numericUpDown3.Value;
        SendPositionBase();
    }

    private void numericUpDown4_ValueChanged(object sender, EventArgs e)
    {
        trackBar4.Value = (byte)numericUpDown4.Value;
        SendPositionForwardBackward();
    }

    private void Send_Commands(string s)
    {
        // Convert the user's string of hex digits (ex: B4 CA E2) to a byte array
        byte[] data = HexStringToByteArray(s);

        // Send the binary data out the port
        comport.Write(data, 0, data.Length);

        // Show the hex digits on in the terminal window
        Log(LogMsgType.Outgoing, ByteArrayToHexString(data) + "\n");
    }

    private void button5_Click(object sender, EventArgs e)
    {
        Send_Commands("55 00 03 06 20 FF");
    }

    private void button15_Click(object sender, EventArgs e)
    {
        Send_Commands("55 00 03 06 21 FF");
    }

    private void button16_Click(object sender, EventArgs e)
    {
        Send_Commands("55 00 03 06 22 FF");
    }

    private void button19_Click(object sender, EventArgs e)
    {
        Send_Commands("55 00 03 06 23 FF");
    }

    private void button18_Click(object sender, EventArgs e)
    {
        Send_Commands("55 00 03 06 24 FF");
    }

    private void button17_Click(object sender, EventArgs e)
    {
        Send_Commands("55 00 03 06 A0 FF");
    }

    private void button22_Click(object sender, EventArgs e)
    {
        Send_Commands("55 00 03 06 A1 FF");
    }

    private void button21_Click(object sender, EventArgs e)
    {
        Send_Commands("55 00 03 06 A2 FF");
    }

    private void button20_Click(object sender, EventArgs e)
    {
        Send_Commands("55 00 03 06 A3 FF");
    }

    private void button1_Click(object sender, EventArgs e)
    {
        Send_Commands("52 00 02 01 91");
    }

    private void button2_Click(object sender, EventArgs e)
    {
        Send_Commands("52 00 02 01 92");
    }

    private void button4_Click(object sender, EventArgs e)
    {
        Send_Commands("52 00 02 01 95");
    }

    private void button3_Click(object sender, EventArgs e)
    {
        Send_Commands("52 00 02 01 96");
    }

    private void button8_Click(object sender, EventArgs e)
    {
        Send_Commands("52 00 02 01 97");
    }

    private void button7_Click(object sender, EventArgs e)
    {
        Send_Commands("52 00 02 01 9A");
    }

    private void button6_Click(object sender, EventArgs e)
    {
        Send_Commands("52 00 02 01 9B");
    }

    private void button14_Click(object sender, EventArgs e)
    {
        Send_Commands("52 00 02 01 9C");
    }

    private void button13_Click(object sender, EventArgs e)
    {
        Send_Commands("52 00 02 01 AC");
    }

    private void button12_Click(object sender, EventArgs e)
    {
        Send_Commands("52 00 02 01 BC");
    }

    private void button11_Click(object sender, EventArgs e)
    {
        Send_Commands("52 00 02 01 BD");
    }

    private void button10_Click(object sender, EventArgs e)
    {
        Send_Commands("52 00 02 01 BF");
    }

    private void button9_Click(object sender, EventArgs e)
    {
        Send_Commands("52 00 02 01 C0");
    }

    private void button23_Click(object sender, EventArgs e)
    {
        Send_Commands("55 01 0D");  // Read 13 bytes motor status
    }

    private void button24_Click(object sender, EventArgs e)
    {
        Send_Commands("50 01 01");  // Read buttons status
                
    }

    private void trackBar3_Scroll(object sender, EventArgs e)
    {
        numericUpDown3.Value = trackBar3.Value;
    }

    private void trackBar4_Scroll(object sender, EventArgs e)
    {
        numericUpDown4.Value = trackBar4.Value;
    }

    private void button25_Click(object sender, EventArgs e)
    {
        SendPositionUpDown();
    }

    private void button26_Click(object sender, EventArgs e)
    {
        SendPositionLeftRight();
    }

    private void button_macro9_Click(object sender, EventArgs e)
    {
        Send_Commands("55 00 03 06 A3 FF");
    }

 


  }
}
