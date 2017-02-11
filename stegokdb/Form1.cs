using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;

namespace stegokdb
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

          int size = 0;
        private void button1_Click(object sender, EventArgs e)
        {
            string FilePic;
            string FileText;
            OpenFileDialog dPic = new OpenFileDialog();
            dPic.Filter = "Файлы изображений (*.bmp)|*.bmp|Все файлы (*.*)|*.*";
            if (dPic.ShowDialog() == DialogResult.OK)
            {
                FilePic = dPic.FileName;
            }
            else
            {
                FilePic = "";
                return;
            }

            FileStream rFile;
            try
            {
                rFile = new FileStream(FilePic, FileMode.Open); //открываем поток
            }
            catch (IOException)
            {
                MessageBox.Show("Ошибка открытия файла", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Bitmap bPic = new Bitmap(rFile);

            OpenFileDialog dText = new OpenFileDialog();
            dText.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            if (dText.ShowDialog() == DialogResult.OK)
            {
                FileText = dText.FileName;
            }
            else
            {
                FileText = "";
                return;
            }

            FileStream rText;
            try
            {
                rText = new FileStream(FileText, FileMode.Open); //открываем поток
            }
            catch (IOException)
            {
                MessageBox.Show("Ошибка открытия файла", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            BinaryReader bText = new BinaryReader(rText, Encoding.ASCII);

            List<byte> bList = new List<byte>();
            while (bText.PeekChar() != -1)
            { //считали весь текстовый файл для шифрования в лист байт
                bList.Add(bText.ReadByte());
            }
            int CountText = bList.Count; // в CountText - количество в байтах текста, который нужно закодировать
            bText.Close();
            rFile.Close();

            //проверяем, поместиться ли исходный текст в картинке
            if (CountText > ((bPic.Width * bPic.Height)) - 4)
            {
                MessageBox.Show("Выбранная картинка мала для размещения выбранного текста", "Информация", MessageBoxButtons.OK);
                return;
            }

            BitArray bits = GetFileBits(FileText);
            size = bits.Length + 8;
            WriteCountText(bits, bPic, CountText);
           // WriteCountText(bList, bPic,CountText); //записываем количество символов для шифрования
          
            
            pictureBox1.Image = bPic;

            String sFilePic;
            SaveFileDialog dSavePic = new SaveFileDialog();
            dSavePic.Filter = "Файлы изображений (*.bmp)|*.bmp|Все файлы (*.*)|*.*";
            if (dSavePic.ShowDialog() == DialogResult.OK)
            {
                sFilePic = dSavePic.FileName;
            }
            else
            {
                sFilePic = "";
                return;
            };

            FileStream wFile;
            try
            {
                wFile = new FileStream(sFilePic, FileMode.Create); //открываем поток на запись результатов
            }
            catch (IOException)
            {
                MessageBox.Show("Ошибка открытия файла на запись", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bPic.Save(wFile, System.Drawing.Imaging.ImageFormat.Bmp);
            wFile.Close(); //закрываем поток
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string FilePic;
            OpenFileDialog dPic = new OpenFileDialog();
            dPic.Filter = "Файлы изображений (*.bmp)|*.bmp|Все файлы (*.*)|*.*";
            if (dPic.ShowDialog() == DialogResult.OK)
            {
                FilePic = dPic.FileName;
            }
            else
            {
                FilePic = "";
                return;
            }

            FileStream rFile;
            try
            {
                rFile = new FileStream(FilePic, FileMode.Open); //открываем поток
            }
            catch (IOException)
            {
                MessageBox.Show("Ошибка открытия файла", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Bitmap bPic = new Bitmap(rFile);
            //ReadCountText(bPic);
            ReadT(bPic,size);
            /*
            int countSymbol = ReadCountText(bPic); //считали количество зашифрованных символов
            byte[] message = new byte[countSymbol];
            //read symbol....
            string strMessage = Encoding.GetEncoding(1251).GetString(message);

            string sFileText;
            SaveFileDialog dSaveText = new SaveFileDialog();
            dSaveText.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            if (dSaveText.ShowDialog() == DialogResult.OK)
            {
                sFileText = dSaveText.FileName;
            }
            else
            {
                sFileText = "";
                return;
            };

            FileStream wFile;
            try
            {
                wFile = new FileStream(sFileText, FileMode.Create); //открываем поток на запись результатов
            }
            catch (IOException)
            {
                MessageBox.Show("Ошибка открытия файла на запись", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            StreamWriter wText = new StreamWriter(wFile, Encoding.Default);
            wText.Write(strMessage);
            MessageBox.Show("Текст записан в файл", "Информация", MessageBoxButtons.OK);
            wText.Close();
            wFile.Close(); //закрываем поток*/

            //ReadText(bPic);
            rFile.Close();
        }


        /*Записыает  символов для шифрования */
        private void WriteCountText(BitArray messbit, Bitmap src, int count)
        {
            /*byte[] rez = new byte[1];
            rez[0] = BitToByte(messbit); 
            string m = Encoding.GetEncoding(1251).GetString(rez);
            MessageBox.Show(m);*/

            byte[] Symbol = Encoding.GetEncoding(1251).GetBytes("/");
            BitArray ArrBeginSymbol = ByteToBit(Symbol[0]);
            int messagelength = messbit.Length + 8;
           // int messagelength = messbit.Length;
            int inputtextlength = 0;
            int messindex = 0;
            for (int i = 0; i < src.Width; i++)
            {
                for (int j = 0; j < src.Height; j++)
                {
                    Color pColor = src.GetPixel(i, j);
                    double l = 0.1;
                    //double y = (0.299 * pColor.R + 0.587 * pColor.G + 0.114 * pColor.B); //0.3  0.59   0.11
                    double y = (0.3 * pColor.R + 0.59 * pColor.G + 0.11 * pColor.B);
                    double u = (-0.14713 * pColor.R - 0.28886 * pColor.G + 0.436 * pColor.B);
                    double v = (0.615 * pColor.R - 0.51499 * pColor.G - 0.10001 * pColor.B);

                    int R = src.GetPixel(i, j).R;
                    int G = src.GetPixel(i, j).G;
                    int B = src.GetPixel(i, j).B;
                    double ly = l * y; 
                    double newB = 0;

                    if (i >= 4 && j >= 4 && j < src.Height - 4 && i < src.Width - 4)
                    {
                        if (messindex < 8)
                        {
                            if (ArrBeginSymbol[messindex] == true)
                            {
                                newB = (int)1;
                            }
                            if (ArrBeginSymbol[messindex] == false)
                            {
                                newB = (int)0;
                            }
                        }
                        // if (messindex < messagelength )
                        if (messindex < messagelength && messindex > 7)
                        {
                            if (messbit[messindex - 8] == true)
                            {
                                newB = (int)B + ly;
                                if (newB > 255) newB = 255;
                            }
                            if (messbit[messindex - 8] == false)
                            {
                                newB = (int)B - ly;
                                if (newB < 0) newB = 0;
                            }
                            inputtextlength++;
                        }
                        else
                            newB = B;

                        messindex++;
                    }
                    else newB = B;

                    int newBB = Convert.ToInt32(newB);
                    src.SetPixel(i, j, Color.FromArgb(R, G, newBB)); 
                }
            }
        }


        private void ReadT(Bitmap src, int sizemessage)
        {
            int imlength = src.Width * src.Height * 32;
            int length = 0;
            BitArray bitarray = new BitArray(sizemessage);
            int lbyte = imlength / 8;
            byte[] rez = new byte[lbyte];

            for (int i = 0; i < src.Width; i++)//0
            {
                for (int j = 0; j < src.Height; j++)
                {
                    Color pColor = src.GetPixel(i, j);

                    int R = src.GetPixel(i, j).R;
                    int G = src.GetPixel(i, j).G;
                    int B = src.GetPixel(i, j).B;
                    int newBB = 0;
                    int newB = 0;
                    /* for (int x = 1; x<=2; x++)
                     {
                         newBB += src.GetPixel(i, j + x).B + src.GetPixel(i, j - x).B + src.GetPixel(i + x, j).B + src.GetPixel(i - x, j).B;
                     }*/

                    if (i >= 4 && j >= 4 && j < src.Height - 4 && i < src.Width - 4)
                    {
                        if ((i >= 2 && j >= 2) && (i < src.Width - 2 && j < src.Height - 2) /*&& (j < sizemessage)*/)
                        {
                            newBB = src.GetPixel(i, j + 1).B + src.GetPixel(i, j - 1).B + src.GetPixel(i + 1, j).B + src.GetPixel(i - 1, j).B +
                                    src.GetPixel(i, j + 2).B + src.GetPixel(i, j - 2).B + src.GetPixel(i + 2, j).B + src.GetPixel(i - 2, j).B;
                            newB = newBB / 8;
                        }
                        if (length < sizemessage)
                        {
                            if (B > newB)
                                bitarray[length] = true;
                            if (B < newB)
                                bitarray[length] = false;
                        }
                        length++;
                    }
                }
            }
        
            int bitarraylength = bitarray.Length;
            var r = ToBitString(bitarray);
            var res = GetBytes(r);
            string m = " ";
            for (int y = 0; y < res.Length - 1; y++)
            {
                rez[y] = res[y+1];
            }
              m = Encoding.GetEncoding(1251).GetString(rez);
             MessageBox.Show(m);
        }

        private static byte[] GetBytes(string bitString)
        {
            byte[] result = Enumerable.Range(0, bitString.Length / 8).
                Select(pos => Convert.ToByte(
                    bitString.Substring(pos * 8, 8),
                    2)
                ).ToArray();

            List<byte> mahByteArray = new List<byte>();
            for (int i = result.Length - 1; i >= 0; i--)
            {
                mahByteArray.Add(result[i]);
            }

            return mahByteArray.ToArray();
        }

        private static String ToBitString(BitArray bits)
        {
            var sb = new StringBuilder();

            for (int i = bits.Count - 1; i >= 0; i--)
            {
                char c = bits[i] ? '1' : '0';
                sb.Append(c);
            }

            return sb.ToString();
        }

       private byte ConvertToByte(BitArray bits)
        {
            if (bits.Count != 8)
            {
                throw new ArgumentException("bits");
            }
            byte[] bytes = new byte[1];
            bits.CopyTo(bytes, 0);
            return bytes[0];
        }
        private BitArray ByteToBit(byte src)
        {
            BitArray bitArray = new BitArray(8);
            bool st = false;
            for (int i = 0; i < 8; i++)
            {
                if ((src >> i & 1) == 1)
                {
                    st = true;
                }
                else st = false;
                bitArray[i] = st;
            }
            return bitArray;
        }

        private byte BitToByte(BitArray scr)
        {
            byte num = 0;
            for (int i = 0; i < scr.Count; i++)
                if (scr[i] == true)
                    num += (byte)Math.Pow(2, i);
            return num;
        }

        private BitArray GetFileBits(string filename)
        {
             byte[] bytes;
             using (FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read))
             {
                  bytes = new byte[file.Length];
                  file.Read(bytes, 0, (int)file.Length);
             }
             return new BitArray(bytes);
        }
        public static void PrintValues(IEnumerable myList, int myWidth)
        {
            int i = myWidth;
            foreach (Object obj in myList)
            {
                if (i <= 0)
                {
                    i = myWidth;
                    Console.WriteLine();
                }
                i--;
                Console.Write("{0,8}", obj);
            }
            Console.WriteLine();
        }
    }
}

    
