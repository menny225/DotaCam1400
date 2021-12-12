using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Media;
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

        private async void Form1_Load(object sender, EventArgs e)
        {
            string npp, file, EditTime;

            //Поиск расположения NotePad++
            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"Applications\notepad++.exe\shell\open\command"))
                npp = key?.GetValue("")?.ToString().Replace("\"%1\"", "");

            //Обновление скрипта NotePad++
            File.WriteAllText(npp.Replace("notepad++.exe", "shortcuts.xml").Replace("\"", ""), Properties.Resources.shortcuts.ToString());

            //Поиск расположения Файла client.dll
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam"))
                file = key?.GetValue("SteamPath")?.ToString().Replace("/", "\\") + @"\steamapps\common\dota 2 beta\game\dota\bin\win64\client.dll";

            //Считывание даты последнего изменения файла client.dll
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\DotaCam1400"))
                EditTime = key?.GetValue("EditTime")?.ToString();
            
            //Если дата последнего изменения (программой) не совпадает с нынешней датой последнего изменения (файл был обновлён не программой) - выполняется модификация
            if (EditTime != File.GetLastWriteTime(file).ToString())
            {
                Cmd($"start \"\" {npp} -multiInst \"{file}\"");
                await Task.Delay(800);
                SendKeys.Send("{F9}");
                await Task.Delay(800);
                Cmd($"taskkill /f /im notepad++.exe");
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\DotaCam1400"))
                    key?.SetValue("EditTime", DateTime.Now);
                SystemSounds.Asterisk.Play();
            }

            //Если дата последнего изменения файла не поменялась (не вышло обновление) - выдаётся ошибка об отсутствии обновления
            else
                MessageBox.Show("Обновлений небыло, файл не изменён!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }

        void Cmd(string line)
        {
            Process.Start(new ProcessStartInfo { FileName = "cmd", Arguments = $"/c {line}", WindowStyle = ProcessWindowStyle.Hidden }).WaitForExit();
        }
    }
}
