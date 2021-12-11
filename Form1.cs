using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
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
            string npp, file;
            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"Applications\notepad++.exe\shell\open\command"))
                npp = key.GetValue("").ToString().Replace("\"%1\"", "");
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam"))
                file = key.GetValue("SteamPath").ToString().Replace("/", "\\") + @"\steamapps\common\dota 2 beta\game\dota\bin\win64\client.dll";
            Cmd($"start \"\" {npp} -multiInst \"{file}\"");
            await Task.Delay(800);
            SendKeys.Send("{F9}");
            await Task.Delay(800);
            Cmd($"taskkill /f /im notepad++.exe");
            SystemSounds.Exclamation.Play();
            Close();
        }

        void Cmd(string line)
        {
            Process.Start(new ProcessStartInfo { FileName = "cmd", Arguments = $"/c {line}", WindowStyle = ProcessWindowStyle.Hidden }).WaitForExit();
        }
    }
}
