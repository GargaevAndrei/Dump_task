
using System;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;


namespace PV15_form
{
    public partial class Form1 : Form
    {
        int stepIndex1;
        int buf_words_count = 4;
        static int N = 4;// Int32.Parse(Form2.textBox1.Text);           // глубина фильтрации
        byte[] bufer1 = new byte[1024];
        byte[] buf_bytes1 = new byte[9];
        int[] buf_words1 = new int[4];
        float[] buf_words1_filtered = new float[4];
        float[,] buf_filter = new float[4, N];
        bool startRead1;
        int[] sum1 = new int[4];
        float[] sred1 = new float[4];
        float[] signal1 = new float[4];
        short[] y = new short[4];
        int n = 0;    // new short[4];

        public Form1()
        {
            InitializeComponent();
            findPorts();
        }
        Form2 Form2 = new Form2();	// создаем вторую форму
        Form3 Form3 = new Form3();	// создаем третью форму
        private void findPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            Form2.comboBox1.Items.AddRange(ports);
            Form2.comboBox2.Items.AddRange(ports);
            Form2.comboBox3.Items.AddRange(ports);
        }
        private void buttonOpenPort_Click(object sender, EventArgs e)
        {
            if (Form2.comboBox1.Text == String.Empty)
                textBox5.Text = "Please select port1";
            else
            {
                try
                {
                    if (!serialPort1.IsOpen)
                    {
                        serialPort1.PortName = Form2.comboBox1.Text;
                        serialPort1.Open();
                        progressBar1.Value = 100;
                        buttonOpenPort.Enabled = false;
                        buttonClosePort.Enabled = true;
                        buttonStart.Enabled = true;
                        buttonStop.Enabled = true;

                    }
                    else
                        MessageBox.Show("Port1 isn't opened");
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Unauthorized Access 1");
                }
            }
            if (Form2.comboBox2.Text == String.Empty)
                textBox5.Text = "Please select port2";
            else
            {
                try
                {
                    if (!serialPort2.IsOpen)
                    {
                        serialPort2.PortName = Form2.comboBox2.Text;
                        serialPort2.Open();
                        progressBar2.Value = 100;
                        buttonOpenPort.Enabled = false;
                        buttonClosePort.Enabled = true;
                        buttonStart.Enabled = true;
                        buttonStop.Enabled = true;

                    }
                    else
                        MessageBox.Show("Port2 isn't opened");
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Unauthorized Access 2");
                }
            }
            if (Form2.comboBox3.Text == String.Empty)
                textBox3.Text = "Please select port1";
            else
            {
                try
                {
                    if (!serialPort3.IsOpen)
                    {
                        serialPort3.PortName = Form2.comboBox3.Text;
                        serialPort3.Open();
                        progressBar3.Value = 100;
                        buttonOpenPort.Enabled = false;
                        buttonClosePort.Enabled = true;
                        buttonStart.Enabled = true;
                        buttonStop.Enabled = true;

                    }
                    else
                        MessageBox.Show("Port3 isn't opened");
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Unauthorized Access 3");
                }
            }
        }

        private void buttonClosePort_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
            progressBar1.Value = 0;
            serialPort2.Close();
            progressBar2.Value = 0;
            serialPort3.Close();
            progressBar3.Value = 0;
            buttonOpenPort.Enabled = true;
            buttonClosePort.Enabled = false;
            buttonStart.Enabled = false;
            buttonStop.Enabled = false;
            //buttonAdd.Enabled = false;
        }

        int p1 = 0, p2 = 0, p3 = 0;

        private void readPort1()
        {

            int i = 0, j = 0, q = 0;
            
            int bytesRead1 = 0;
            int sum1_index = 0;

            bytesRead1 = serialPort1.Read(bufer1, 0, bufer1.Length); //bufer2 для PV15

            startRead1 = false;

            //-------------------------------------------------------------------------------------------------------	
            if (serialPort1.IsOpen && bytesRead1 > 0)
            {
                sum1_index = 0;
                for (q = 0; q < buf_words_count; q++)
                {
                    sred1[q] = 0;
                    sum1[q] = 0;
                    signal1[q] = 0;
                }

                for (i = 0; i < bytesRead1; i++)
                {
                    if (bufer1[i] == 216)   //синхронизация по первому байту пакета (D8)
                    {
                        stepIndex1 = 0;
                        startRead1 = true;
                    }
                    if (startRead1)
                    {
                        buf_bytes1[stepIndex1] = bufer1[i]; // чтение и запись пакета
                        ++stepIndex1;
                    }
                    if (stepIndex1 == 9 && startRead1)      // 9 - количество байтов в пакете
                    {
                        startRead1 = false;
                        sum1_index++;
                        //------------- Реализация протокола ПВ15 ------------------------
                        for (q = 0, j = 1; q < 4; q++, j += 2)
                        {
                            buf_words1[q] = (int)(((Int16)(buf_bytes1[j] << 7) & Convert.ToInt16("0011111110000000", 2)) | ((Int16)(buf_bytes1[j + 1] << 0) & Convert.ToInt16("0000000001111111", 2)) | ((Int16)(buf_bytes1[j] << 8) & Convert.ToInt16("0100000000000000", 2)) | ((Int16)(buf_bytes1[j] << 9) & Convert.ToInt16("1000000000000000", 2)));
                            sum1[q] += buf_words1[q];
                            //buf_words1_filtered[q] = filter((buf_words1[q]), q);
                        }
                        for (q = 0; q < buf_words_count; q++)
                        {
                            sred1[q] = sum1[q] / sum1_index;
                            buf_words1_filtered[q] = filter(sred1[q], q);
                            //signal1[q] = (sred1[q] - zeroes1[q]) * k[q]; // умножать на коэф
                        }

                        //textBox5.AppendText(sred1[0].ToString() + ";  " + sred1[1].ToString() + ";  " + sred1[2].ToString() + ";  " + sred1[3].ToString() + ";  " );
                        //textBox1.Text = sred1[0].ToString();
                        //textBox2.Text = sred1[1].ToString();

                        //Form3.chart1.Series[0].Points.AddY(sred1[0]);
                        //Form3.chart2.Series[0].Points.AddY(sred1[0]);

                        //if (p1 > 120)
                        //{
                        //    Form3.chart1.Series[0].Points.RemoveAt(0);
                        //    Form3.chart2.Series[0].Points.RemoveAt(0);
                        //}
                        //p1++;
                    }

                }
					
        }
			
	}

        private void readPort2()
        {

            int i = 0, j = 0, q = 0;

            int bytesRead1 = 0;
            int sum1_index = 0;

            bytesRead1 = serialPort2.Read(bufer1, 0, bufer1.Length); //bufer2 для PV15

            startRead1 = false;

            //-------------------------------------------------------------------------------------------------------	
            if (serialPort2.IsOpen && bytesRead1 > 0)
            {
                sum1_index = 0;
                for (q = 0; q < buf_words_count; q++)
                {
                    sred1[q] = 0;
                    sum1[q] = 0;
                    signal1[q] = 0;
                }

                for (i = 0; i < bytesRead1; i++)
                {
                    if (bufer1[i] == 216)   //синхронизация по первому байту пакета (D8)
                    {
                        stepIndex1 = 0;
                        startRead1 = true;
                    }
                    if (startRead1)
                    {
                        buf_bytes1[stepIndex1] = bufer1[i]; // чтение и запись пакета
                        ++stepIndex1;
                    }
                    if (stepIndex1 == 9 && startRead1)      // 9 - количество байтов в пакете
                    {
                        startRead1 = false;
                        sum1_index++;
                        //------------- Реализация протокола ПВ15 ------------------------
                        for (q = 0, j = 1; q < 4; q++, j += 2)
                        {
                            buf_words1[q] = (int)(((Int16)(buf_bytes1[j] << 7) & Convert.ToInt16("0011111110000000", 2)) | ((Int16)(buf_bytes1[j + 1] << 0) & Convert.ToInt16("0000000001111111", 2)) | ((Int16)(buf_bytes1[j] << 8) & Convert.ToInt16("0100000000000000", 2)) | ((Int16)(buf_bytes1[j] << 9) & Convert.ToInt16("1000000000000000", 2)));
                            sum1[q] += buf_words1[q];
                            //buf_words1_filtered[q] = filter(buf_words1[q], q);
                        }
                        for (q = 0; q < buf_words_count; q++)
                        {
                            sred1[q] = sum1[q] / sum1_index;
                            buf_words1_filtered[q] = filter(sred1[q], q);
                            //signal1[q] = (sred1[q] - zeroes1[q]) * k[q]; // умножать на коэф
                        }
                        //textBox3.AppendText(buf_words1[0].ToString() + ";  " + buf_words1[1].ToString() + ";  " + buf_words1[2].ToString() + ";  " + buf_words1[3].ToString() + ";  " + Environment.NewLine);
                        //textBox5.AppendText(sred1[0].ToString() + ";  ");
                        //textBox3.Text = sred1[0].ToString();

                        //Form3.chart1.Series[1].Points.AddY(sred1[0]);
                        //Form3.chart2.Series[1].Points.AddY(buf_words1_filtered[0]);

                        //if (p2 > 120)
                        //{
                        //    Form3.chart1.Series[1].Points.RemoveAt(0);
                        //    Form3.chart2.Series[1].Points.RemoveAt(0);

                        //}
                        //p2++;
                    }

                }

            }

        }

        private void readPort3()
        {

            int i = 0, j = 0, q = 0;

            int bytesRead1 = 0;
            int sum1_index = 0;

            bytesRead1 = serialPort3.Read(bufer1, 0, bufer1.Length); //bufer2 для PV15

            startRead1 = false;

            //-------------------------------------------------------------------------------------------------------	
            if (serialPort3.IsOpen && bytesRead1 > 0)
            {
                sum1_index = 0;
                for (q = 0; q < buf_words_count; q++)
                {
                    sred1[q] = 0;
                    sum1[q] = 0;
                    signal1[q] = 0;
                }

                for (i = 0; i < bytesRead1; i++)
                {
                    if (bufer1[i] == 216)   //синхронизация по первому байту пакета (D8)
                    {
                        stepIndex1 = 0;
                        startRead1 = true;
                    }
                    if (startRead1)
                    {
                        buf_bytes1[stepIndex1] = bufer1[i]; // чтение и запись пакета
                        ++stepIndex1;
                    }
                    if (stepIndex1 == 9 && startRead1)      // 9 - количество байтов в пакете
                    {
                        startRead1 = false;
                        sum1_index++;
                        //------------- Реализация протокола ПВ15 ------------------------
                        for (q = 0, j = 1; q < 4; q++, j += 2)
                        {
                            buf_words1[q] = (int)(((Int16)(buf_bytes1[j] << 7) & Convert.ToInt16("0011111110000000", 2)) | ((Int16)(buf_bytes1[j + 1] << 0) & Convert.ToInt16("0000000001111111", 2)) | ((Int16)(buf_bytes1[j] << 8) & Convert.ToInt16("0100000000000000", 2)) | ((Int16)(buf_bytes1[j] << 9) & Convert.ToInt16("1000000000000000", 2)));
                            sum1[q] += buf_words1[q];
                            //buf_words1_filtered[q] = filter(buf_words1[q], q);
                        }
                        for (q = 0; q < buf_words_count; q++)
                        {
                            sred1[q] = sum1[q] / sum1_index;
                            buf_words1_filtered[q] = filter(sred1[q], q);
                            //signal1[q] = (sred1[q] - zeroes1[q]) * k[q]; // умножать на коэф
                        }
                        //textBox1.AppendText(buf_words1[0].ToString() + ";  " + buf_words1[1].ToString() + ";  " + buf_words1[2].ToString() + ";  " + buf_words1[3].ToString() + ";  " + Environment.NewLine);
                        //textBox5.AppendText(sred1[0].ToString() + ";  " + Environment.NewLine);
                        //textBox4.Text = sred1[0].ToString();

                        //Form3.chart2.Series[1].Points.AddY(sred1[0]);

                        //if (p3 > 120)
                        //    Form3.chart2.Series[1].Points.RemoveAt(0);
                        //p3++;
                    }

                }

            }

        }

        private float filter (float x, int q)
        {
            y[q] += (short)(x - buf_filter[q, n]);
            buf_filter[q, n] = x;
            if (q == 3)
                n = (short)((n + 1) % N);
            return (y[q]/N);
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void refreshArray()
        {
            int q;
            for (q = 0; q < 9; q++)
                buf_bytes1[q] = 0;
            for (q = 0; q < 4; q++)
            {
                buf_words1[q] = 0;
                buf_words1_filtered[q] = 0;
            }
            for (q = 0; q < bufer1.Length; q++)
                bufer1[q] = 0;
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            Form3.chart1.Series[0].Points.Clear();
            Form3.chart1.Series[1].Points.Clear();
            Form3.chart2.Series[0].Points.Clear();
            Form3.chart2.Series[1].Points.Clear();
            dataGridView2.Rows.Clear();
            dataGridView3.Rows.Clear();
            p1 = 0;
            p2 = 0;
            p3 = 0;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            double delta;  // для автомасштаба графика

            readPort1();
            textBox1.Text = String.Format("{0:0.0}", buf_words1_filtered[0]);
            textBox2.Text = String.Format("{0:0.0}", buf_words1_filtered[1]);
            textBox5.AppendText(buf_words1_filtered[0].ToString() + ";  " + buf_words1_filtered[1].ToString() + ";  " + buf_words1_filtered[2].ToString() + ";  " + buf_words1_filtered[3].ToString() + ";  ");

            refreshArray();

            //----------------------------------------------------------------------------
            readPort2();
            textBox5.AppendText(buf_words1_filtered[0].ToString() + ";  ");
            textBox3.Text = String.Format("{0:0.0}", buf_words1_filtered[0]);

            Form3.chart1.Series[1].Points.AddY(sred1[0]);
            Form3.chart2.Series[1].Points.AddY(buf_words1_filtered[0]);
            if (p2 > 120)
            {
                Form3.chart1.Series[1].Points.RemoveAt(0);
                Form3.chart2.Series[1].Points.RemoveAt(0);

            }
            p2++;
            refreshArray();

            //----------------------------------------------------------------------------
            readPort3();
            textBox5.AppendText(buf_words1_filtered[0].ToString() + ";  " + Environment.NewLine);
            textBox4.Text = String.Format("{0:0.0}", buf_words1_filtered[0]);
            refreshArray();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Form2.comboBox1.SelectedItem = "COM14";
            Form2.comboBox2.SelectedItem = "COM12";
            Form2.comboBox3.SelectedItem = "COM13";

            Form3.chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            Form3.chart1.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
            Form3.chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
            Form3.chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            Form3.chart1.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            Form3.chart1.ChartAreas[0].AxisY.ScrollBar.IsPositionedInside = true;
            Form3.chart1.ChartAreas[0].CursorX.AutoScroll = true;
            Form3.chart1.ChartAreas[0].CursorY.IsUserEnabled = true;
            Form3.chart1.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;

            Form3.chart2.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            Form3.chart2.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
            Form3.chart2.ChartAreas[0].CursorX.IsUserEnabled = true;
            Form3.chart2.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            Form3.chart1.ChartAreas[0].CursorX.AutoScroll = true;
            Form3.chart2.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            Form3.chart2.ChartAreas[0].AxisY.ScrollBar.IsPositionedInside = true;
            Form3.chart2.ChartAreas[0].CursorY.IsUserEnabled = true;
            Form3.chart2.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Текстовый документ (*.txt)|*.txt|Все файлы (*.*)|*.*";

            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StreamWriter streamWriter = new StreamWriter(saveFileDialog1.FileName);
                streamWriter.WriteLine(textBox5.Text);
                streamWriter.Close();
            }
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Cursor Files|*.txt";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Stream sr;
                if ((sr = openFileDialog1.OpenFile()) != null)
                {
                    String ReadFile = openFileDialog1.InitialDirectory + openFileDialog1.FileName;
                    String data = File.ReadAllText(ReadFile);
                    //MessageBox.Show(data);
                    textBox1.Text = data;
                }
                sr.Close();
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            dataGridView2.Rows.Add(String.Format("{0:0.0}", buf_words1_filtered[0]), String.Format("{0:0.0}", buf_words1_filtered[1]), String.Format("{0:0.0}", buf_words1_filtered[2]), String.Format("{0:0.0}", buf_words1_filtered[3]));
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2.Show();
        }

        private void осциллографToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form3.Show();
        }

        private void buttonCalculation_Click(object sender, EventArgs e)
        {

        }
    }
}
