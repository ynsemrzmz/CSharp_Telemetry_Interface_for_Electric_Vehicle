using System;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;
using System.IO;


namespace TelemetryRev1._0
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }



        static string fileName = "ygm_ett_log.txt";

        StreamWriter writer = new StreamWriter(fileName);

        private void add_title()
        {
            writer.Write("zaman\thucre1\thucre2\thucre3\thucre4\thucre5\thucre6\thucre7\thucre8\thucre9\thucre10\thucre11\thucre12\thucre13\thucre14\thucre15\thucre16\thucre17\thucre18\thucre19\thucre20\thucre21\thucre22\tminHucre\tmaxHucre\tGerilim\tsicaklik1\tsicaklik2\tsicaklik3\tsicaklik4\tmaxSicaklik\takim\tharcananEnerji\tkalanEnerji\tyüzde\tmotorSicakligi\thiz");
            writer.WriteLine("");
        }

        /*********************************/

        int timerCounter = 0;
        int hour = 0;
        int minute = 0;
        int second = 0;
        int lapNum = 0;

        /*********************************/

        char[] gelen_ham_data = new char[255];
        char[] gelen_ham_data_chcksm = new char[255];
        char[] gelen_dogrulanmis_data = new char[255];
        int gelen_data_boyut_kontrolu = 0;
        string chcksm_hesaplanan = "";
        string gelen_chcksm = "";
        string ayristirilacak_veri = "";
        string[] ayristirilan_data =  new string[36];
        //uint8_t chcksum = 0;
        int[] cellPercentages = new int[22];
       

        const byte numChars = 255;
        char[] receivedChars = new char[numChars];
        int[]veri = new int[36];
        bool newData = false;
        bool flag = false;
        int i = 0;
        static bool recvInProgress = false;
        byte bitis = 0xff;
        byte rem = 0x41;

        bool connectionFlag = false;

        /************************************/
        private void init_ports()
        {
            string[] ports;
            string[] baudRates = { "2400", "4800", "9600", "14400", "19200", "38400", "57600", "115200" };

            ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                comboBoxPort.Items.Add(port);
            }

            foreach (string baudRate in baudRates)
            {
                comboBoxBaud.Items.Add(baudRate);
            }

        }

        private void init_form()
        {

            WindowState = FormWindowState.Maximized;
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        /************************************/
    
        private void connect()
        {
            if (!serialPort1.IsOpen)
            {
                try
                {
                    if (comboBoxBaud.SelectedItem != null && comboBoxPort.SelectedItem != null)
                    {
                        serialPort1.PortName = comboBoxPort.SelectedItem.ToString();
                        serialPort1.BaudRate = Convert.ToInt32(comboBoxBaud.SelectedItem);
                        serialPort1.Open();
                        labelConnectionStatus.Text = "Açık";
                        labelConnectionStatus.ForeColor = Color.Green;

                        connectionFlag = true;

                    }
                    else
                    {
                        MessageBox.Show("Lütfen port ve baud rate seçimini yapınız", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Bağlantı zaten açık", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void disconnect()
        {

            if (serialPort1.IsOpen)
            {
                serialPort1.DiscardInBuffer();
                try
                {
                    serialPort1.Close();
                    labelConnectionStatus.Text = "Kapalı";
                    labelConnectionStatus.ForeColor = Color.Red;
                    connectionFlag = false;    

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Veri Akışı Durduruldu","Uyarı",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Bağlantı zaten açık değil", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void close_port()
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
            }
        }

        /************************************/

        private void update_time()
        {
            timerWriteTime.Start();
        }

        private void write_time()
        {
            labelTime.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void write_date()
        {
            labelDate.Text = DateTime.Now.ToString(" dd MMM  yyyy , dddd ");
        }

        /************************************/

        private void start_stopwatch()
        {
            stopwatch.Enabled = true;
            buttonStart.Enabled = false;
            stopwatch.Start();

        }

        private void stop_stopwatch()
        {
            stopwatch.Enabled = false;
            stopwatch.Stop();
            buttonStart.Enabled = true;
            buttonStart.Text = "Devam et";
        }

        private void reset_stopwatch()
        {
            if (stopwatch.Enabled == false)
            {   
                labelMilisecond.Text = ".....";
                labelSecond.Text = ".....";
                labelMinute.Text = ".....";
                labelHour.Text = ".....";
                buttonStart.Enabled = true;
                buttonStart.Text = "Başla";
            }
            else
            {
                MessageBox.Show("Lütfen önce kronometreyi durdurunuz", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /************************************/

        private int map(int input, int inMin, int inMax, int outMin, int outMax)
        {
            return ((((input - inMin) * (outMax - outMin)) / (inMax - inMin)) + outMin);
        }

        /************************************/

        private void Form1_Load(object sender, EventArgs e)
        {   

            init_form();
            init_ports();
            update_time();
            write_date();

            serialPort1.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
       
            add_title();

        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            connect();
        }
       
        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            disconnect();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            close_port();
        }

        private void stopwatch_Tick(object sender, EventArgs e)
        {
            timerCounter++;

            if (timerCounter > 9)
            {
                
                timerCounter = 0;
                second++;
            }

            if (second > 59)
            {
                
                second = 0;
                minute++;
            }

            if (minute > 59)
            {
                
                hour = 0;
                hour++;
            }

            labelMilisecond.Text = Convert.ToString(timerCounter);
            labelSecond.Text = Convert.ToString(second);
            labelMinute.Text = Convert.ToString(minute);
            labelHour.Text = Convert.ToString(hour);
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            start_stopwatch();
            
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            stop_stopwatch();
           
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            reset_stopwatch();
           
        }

        private void timerWriteTime_Tick(object sender, EventArgs e)
        {
            write_time();
        }

        private void buttonLapInc_Click(object sender, EventArgs e)
        {
            lapNum++;
            labelLap.Text = Convert.ToString(lapNum);
        }

        private void buttonLapReset_Click(object sender, EventArgs e)
        {
            lapNum = 0 ;
            labelLap.Text = Convert.ToString(lapNum);
        }

        private void DataReceivedHandler( object sender,SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;

            byte ndx = 0;
            char startMarker = '<';
            char endMarker = '>';
            char rc;

            while (sp.IsOpen  && newData == false)
            {
                
                rc = Convert.ToChar(sp.ReadChar());

                if (recvInProgress == true)
                {
                    if (rc != endMarker & rc != startMarker)
                    {
                        receivedChars[ndx] = rc;
                        ndx++;


                        if (ndx >= numChars)
                        {
                            ndx = numChars - 1;
                        }
                    }
                    else
                    {
                        receivedChars[ndx] = '\0';
                        recvInProgress = false;
                        ndx = 0;
                        newData = true;
                        i = 0;
                        if (rc == startMarker)
                        {
                            flag = true;

                        }
                    }
                }

                else if (rc == startMarker)
                {
                    recvInProgress = true;
                }
            }


            if (newData == true)
            {

                if (flag)
                {
                    recvInProgress = true;
                    newData = false;
                    flag = false;
                }

                int ham_data_kontrol = 1, ham_data_chcksm_kontrol = 0;
                int ham_data_sayac = 0, ham_data_chcksm_sayac = 0;

                for (int gelen_veri_sayac = 0; gelen_veri_sayac <  receivedChars.Length; gelen_veri_sayac++)
                {
                    if (receivedChars[0] == '[' && ham_data_kontrol == 1)
                    {
                        gelen_ham_data[gelen_veri_sayac] = receivedChars[gelen_veri_sayac];

                        if (receivedChars[gelen_veri_sayac] == ']')
                        {
                            ham_data_sayac = gelen_veri_sayac + 1;
                            ham_data_chcksm_kontrol = 1;
                        }

                        if (ham_data_chcksm_kontrol == 1)
                        {
                            gelen_data_boyut_kontrolu = gelen_veri_sayac + 1;
                            if (gelen_data_boyut_kontrolu ==  receivedChars.Length)
                            {
                                gelen_data_boyut_kontrolu = gelen_veri_sayac;
                            }
                            gelen_ham_data_chcksm[gelen_veri_sayac + 1 - ham_data_sayac] = receivedChars[gelen_data_boyut_kontrolu];

                            if (receivedChars[gelen_data_boyut_kontrolu] == '*')
                            {
                                ham_data_kontrol = 0;
                                ham_data_chcksm_kontrol = 0;
                                ham_data_chcksm_sayac = gelen_veri_sayac + 1 - ham_data_sayac;
                            }
                        }
                    }
                }



                for (int chcksm_sayac = 0; chcksm_sayac < ham_data_chcksm_sayac; chcksm_sayac++)
                {
                    gelen_chcksm += gelen_ham_data_chcksm[chcksm_sayac];
                }

                int k = 0, l = 0;

                for (k = 0; k < ham_data_sayac; k++)
                {
                    rem ^= (byte)gelen_ham_data[k];
                    for (l = 0; l < 8; l++)
                    {
                        if ((rem & 0x80) != 0)
                        {
                            rem = (byte)((rem << 1) ^ 0x07);
                        }
                        else
                        {
                            rem = (byte)(rem << 1);
                        }
                    }
                }

                if (gelen_chcksm.Length != 0)
                {
                    if (Convert.ToDecimal(rem) == Convert.ToDecimal(gelen_chcksm))
                    {
                        if (gelen_ham_data[0] == '[' && gelen_ham_data[ham_data_sayac - 1] == ']')
                        {
                            for (int dogrulanmis_data_sayac = 0; dogrulanmis_data_sayac < ham_data_sayac - 2; dogrulanmis_data_sayac++)
                            {
                                gelen_dogrulanmis_data[dogrulanmis_data_sayac] = gelen_ham_data[dogrulanmis_data_sayac + 1];
                            }
                        }


                        for (int j = 0; j < ham_data_sayac; j++)
                        {
                            ayristirilacak_veri += gelen_dogrulanmis_data[j];
                        }

                        ayristirilan_data = ayristirilacak_veri.Split('|');


                        n0.Text = ayristirilan_data[0];
                        n1.Text = ayristirilan_data[1];
                        n2.Text = ayristirilan_data[2];
                        n3.Text = ayristirilan_data[3];
                        n4.Text = ayristirilan_data[4];
                        n5.Text = ayristirilan_data[5];
                        n6.Text = ayristirilan_data[6];
                        n7.Text = ayristirilan_data[7];
                        n8.Text = ayristirilan_data[8];
                        n9.Text = ayristirilan_data[9];
                        n10.Text = ayristirilan_data[10];
                        n11.Text = ayristirilan_data[11];
                        n12.Text = ayristirilan_data[12];
                        n13.Text = ayristirilan_data[13];
                        n14.Text = ayristirilan_data[14];
                        n15.Text = ayristirilan_data[15];
                        n16.Text = ayristirilan_data[16];
                        n17.Text = ayristirilan_data[17];
                        n18.Text = ayristirilan_data[18];
                        n19.Text = ayristirilan_data[19];
                        n20.Text = ayristirilan_data[20];
                        n21.Text = ayristirilan_data[21];

                        labelMaxCellVoltage.Text = ayristirilan_data[22];
                        labelMinCellVoltage.Text = ayristirilan_data[23];
                        labelVoltage.Text = (Convert.ToDecimal(ayristirilan_data[24]) / 100) + "";

                        labelMotorVoltage.Text = ((Convert.ToDecimal(ayristirilan_data[24]) / 100) - 1 ) + "";

                        x0.Text = (Convert.ToDecimal(ayristirilan_data[25]) / 100) + "";

                        x1.Text = (Convert.ToDecimal(ayristirilan_data[26]) / 100) + "";
                        x2.Text = (Convert.ToDecimal(ayristirilan_data[27]) / 100) + "";
                        x3.Text = (Convert.ToDecimal(ayristirilan_data[28]) / 100) + "";
                        labelBatteryHeat.Text = (Convert.ToDecimal(ayristirilan_data[29]) / 100) + "";

                        labelCurrent.Text = (Convert.ToDecimal(ayristirilan_data[30]) / 100) + "";
                        labelWattHour.Text = ayristirilan_data[31];
                        labelRemainingEnergy.Text = ayristirilan_data[32];
                        labelEnergyPercent.Text = ayristirilan_data[33];
                        labelSpeed.Text = ayristirilan_data[34];
                        labelMotorHeat.Text = (Convert.ToDecimal(ayristirilan_data[35]) / 100) + "";
                        m0.Text = labelMotorHeat.Text;

                        writer.Write(labelTime.Text);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[0]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[1]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[2]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[3]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[4]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[5]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[6]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[7]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[8]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[9]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[10]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[11]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[12]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[13]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[14]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[15]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[16]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[17]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[18]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[19]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[20]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[21]);
                        writer.Write("\t");

                        writer.Write(ayristirilan_data[22]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[23]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[24]);
                        writer.Write("\t");

                        writer.Write(ayristirilan_data[25]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[26]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[27]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[28]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[29]);
                        writer.Write("\t");

                        writer.Write(ayristirilan_data[30]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[31]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[32]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[33]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[34]);
                        writer.Write("\t");
                        writer.Write(ayristirilan_data[35]);
                        writer.WriteLine();

                        for(int m = 0; l<21; l++)
                        {
                            cellPercentages[m] = Convert.ToInt16(ayristirilan_data[m]);
                        }

                        p0.Value = map(cellPercentages[0], 260, 420, 0, 100);
                        p1.Value = map(cellPercentages[1], 260, 420, 0, 100);
                        p2.Value = map(cellPercentages[2], 260, 420, 0, 100);
                        p3.Value = map(cellPercentages[3], 260, 420, 0, 100);
                        p4.Value = map(cellPercentages[4], 260, 420, 0, 100);
                        p5.Value = map(cellPercentages[5], 260, 420, 0, 100);
                        p6.Value = map(cellPercentages[6], 260, 420, 0, 100);
                        p7.Value = map(cellPercentages[7], 260, 420, 0, 100);
                        p8.Value = map(cellPercentages[8], 260, 420, 0, 100);
                        p9.Value = map(cellPercentages[9], 260, 420, 0, 100);
                        p10.Value = map(cellPercentages[10], 260, 420, 0, 100);
                        p11.Value = map(cellPercentages[11], 260, 420, 0, 100);
                        p12.Value = map(cellPercentages[12], 260, 420, 0, 100);
                        p13.Value = map(cellPercentages[13], 260, 420, 0, 100);
                        p14.Value = map(cellPercentages[14], 260, 420, 0, 100);
                        p15.Value = map(cellPercentages[15], 260, 420, 0, 100);
                        p16.Value = map(cellPercentages[16], 260, 420, 0, 100);
                        p17.Value = map(cellPercentages[17], 260, 420, 0, 100);
                        p18.Value = map(cellPercentages[18], 260, 420, 0, 100);
                        p19.Value = map(cellPercentages[19], 260, 420, 0, 100);
                        p20.Value = map(cellPercentages[20], 260, 420, 0, 100);
                        p21.Value = map(cellPercentages[21], 260, 420, 0, 100);



                        // ayristirilan_data icersinde tum dataların ayrilmis bir sekilde atanır
                        // sende onlari form ekranindaki uygun yerlere yerlestirisin
                        // gelen datalar string oldugu icin matematiksel islemler yapmak istersen
                        // onlari once convert.todecimal std fonksiyonuna solarsin sonrailgiil islemi yapıp 
                        // ilgili alana yazdiriikwn sonuna  + "" seklinde yazdirisin

                        // ornek kullanim 
                        //  n0.Text = Convert.ToDecimal(ayristirilan_data[1]) / 100 + ""  ;
                        // yukaridaki gibi kullanirsin zaten progress barlarlar icin boyle matematiksel islemler gerekecek
                        // ama onda sadece Convert.ToDecimal(ayristirilan_data[1]) / 100 bunu yapar birakkirs


                    }

                }

                serialPort1.DiscardInBuffer();
                rem = 0x41;
                gelen_chcksm = "";
                ayristirilacak_veri = "";
                chcksm_hesaplanan = "";
                newData = false;
            }




        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {   
            if(!serialPort1.IsOpen)
                init_ports();
        }

        private void buttonLog_Click(object sender, EventArgs e)
        {
            writer.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            writer.Close();
        }
    }
}

