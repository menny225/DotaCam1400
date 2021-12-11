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
            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"Applications\notepad++.exe\shell\open\command"))
                npp = key.GetValue("").ToString().Replace("\"%1\"", "");
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam"))
                file = key.GetValue("SteamPath").ToString().Replace("/", "\\") + @"\steamapps\common\dota 2 beta\game\dota\bin\win64\client.dll";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\DotaCam1400"))
                EditTime = key.GetValue("EditTime").ToString();
            if (EditTime != File.GetLastWriteTime(file).ToString())
            {
                Cmd($"start \"\" {npp} -multiInst \"{file}\"");
                await Task.Delay(800);
                SendKeys.Send("{F9}");
                await Task.Delay(800);
                Cmd($"taskkill /f /im notepad++.exe");
                SystemSounds.Asterisk.Play();
            }
            else
                MessageBox.Show("Обновлений небыло, файл не изменён", "Ошибка");
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\DotaCam1400"))
                key.SetValue("EditTime", File.GetLastWriteTime(file));
            Close();
        }

        void Cmd(string line)
        {
            Process.Start(new ProcessStartInfo { FileName = "cmd", Arguments = $"/c {line}", WindowStyle = ProcessWindowStyle.Hidden }).WaitForExit();
        }
    }
}
