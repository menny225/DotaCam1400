using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotaCam1400
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string file, EditTime;
            Regex reg = new Regex("");
            Match match;  
            //Поиск расположения Файла client.dll
            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"dota2\Shell\Open\Command"))
                file = Regex.Match(key.GetValue("").ToString(), "[^a-z](.*?.game?)").Groups[1].Value + @"\dota\bin\win64\client.dll";

            //Считывание даты последнего изменения файла client.dll
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\DotaCam1400"))
                EditTime = key?.GetValue("EditTime")?.ToString();

            // Модификация файла в случае разности в дате редактирования
            if (EditTime == File.GetLastWriteTime(file).ToString())
            {
                byte[] sourceBytes = StringHexToByteArray("3132303000");
                byte[] targetBytes = StringHexToByteArray("3134303000");
                BinaryReplace($"{file}", sourceBytes, $"{file.Replace("client.dll","client_path.dll")}", targetBytes);
                File.Delete(file);
                File.Move(file.Replace("client.dll", "client_path.dll"), file);

                //Запись даты модификации
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\DotaCam1400"))
                    key?.SetValue("EditTime", DateTime.Now);

                MessageBox.Show("Файл успешно модифицирован!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
            }
            else
                MessageBox.Show("Обновлений небыло, файл не изменён!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
            Close();
        }

        //Механизм поиска и замены нужных байт в файле
        public static void BinaryReplace(string sourceFile, byte[] sourceSeq, string targetFile, byte[] targetSeq)
        {
            FileStream sourceStream = File.OpenRead(sourceFile);
            FileStream targetStream = File.Create(targetFile);

            try
            {
                int b;
                long foundSeqOffset = -1;
                int searchByteCursor = 0;

                while ((b = sourceStream.ReadByte()) != -1)
                {
                    if (sourceSeq[searchByteCursor] == b)
                    {
                        if (searchByteCursor == sourceSeq.Length - 1)
                        {
                            targetStream.Write(targetSeq, 0, targetSeq.Length);
                            searchByteCursor = 0;
                            foundSeqOffset = -1;
                        }
                        else
                        {
                            if (searchByteCursor == 0)
                            {
                                foundSeqOffset = sourceStream.Position - 1;
                            }

                            ++searchByteCursor;
                        }
                    }
                    else
                    {
                        if (searchByteCursor == 0)
                        {
                            targetStream.WriteByte((byte)b);
                        }
                        else
                        {
                            targetStream.WriteByte(sourceSeq[0]);
                            sourceStream.Position = foundSeqOffset + 1;
                            searchByteCursor = 0;
                            foundSeqOffset = -1;
                        }
                    }
                }
            }
            finally
            {
                sourceStream.Dispose();
                targetStream.Dispose();
            }
        }

        //Механизм конвертации строкового представления байт в массив байт
        public static byte[] StringHexToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}
