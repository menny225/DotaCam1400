﻿using Microsoft.Win32;
using System;
using System.IO;
using System.Text.RegularExpressions;
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
            // Модификация файла в случае разности в дате редактирования
            if (EditTime() != File.GetLastWriteTime(FindDLL()).ToString())
                Modify(FindDLL());
            else
            if (MessageBox.Show("Файл уже модифицирован! \n\nОткатить изменения?", "Информация", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                Modify(FindDLL(), restore: true);
            Close();
        }

        //Считывание даты последнего изменения файла client.dll
        public static string EditTime()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\DotaCam1400"))
                return key?.GetValue("EditTime")?.ToString();
        }

        //Поиск расположения Файла client.dll
        public static string FindDLL()
        {
            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"dota2\Shell\Open\Command"))
                return Regex.Match(key?.GetValue("").ToString(), "[^a-z](.*?.game?)").Groups[1].Value + @"\dota\bin\win64\client.dll";
        }

        //Механизм модификации
        public static void Modify(string file, bool restore = false)
        {
            //Модификация файла
            BinaryReplace($"{file}", StringHexToByteArray(restore ? "3134303000": "3132303000"), $"{file.Replace("client.dll", "client_path.dll")}", StringHexToByteArray(restore ? "3132303000" : "3134303000"));
            File.Delete(file);
            File.Move(file.Replace("client.dll", "client_path.dll"), file);

            //Запись даты модификации
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\DotaCam1400"))
                key?.SetValue("EditTime", restore ? "" : DateTime.Now.ToString());

            //Информирование об успешной модификации
            MessageBox.Show(restore ? "Файл успешно восстановлен!":"Файл успешно модифицирован!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
