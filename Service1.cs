using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;

namespace my_server
{
    public partial class Service1 : ServiceBase
    {

        private readonly Timer timer1 = new Timer();
        private readonly string CurDir = AppDomain.CurrentDomain.BaseDirectory;
        private int timeInterval = 15 * 60 * 1000;
        private string url = "";
        private int running = 0;


        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            this.Log("启动服务");
            this.ReadConfig();
            this.ExecCommand();
            this.InitTimer();
        }

        private void ExecCommand0()
        {
            /*string str = HttpUitls.Get(this.url + "/srv/c12d.c12d_sqlserver2000Srv/getSql");
            this.Log("");
            this.Log(str);
            JObject obj = JObject.Parse(str);
            var d = obj["d"];
            if (d == null) return;*/
            
        }

        private void ExecCommand()
        {
            if (this.running > 0)
            {
                this.Log("running: " + running);
                return;
            }
            else if (this.running < 0)
            {
                this.running = 0;
            }
            this.running++;
            try
            {
                this.ExecCommand0();
            }
            catch (Exception err)
            {
                this.Log(err.Message + "\r\n" + err.StackTrace);
            }
            finally
            {
                this.running--;
            }
        }

        protected override void OnStop()
        {
            this.Log("停止服务");
        }

        private void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.ExecCommand();
        }

        private void InitTimer()
        {
            this.timer1.Elapsed += new ElapsedEventHandler(this.OnTimedEvent);
            this.timer1.Enabled = true;
            this.timer1.Interval = this.timeInterval;
            this.timer1.Start();
        }

        private void FileAdd(string Path, string Str)
        {
            StreamWriter sw = File.AppendText(Path);
            sw.Write(Str);
            sw.Flush();
            sw.Close();
            sw.Dispose();
        }

        private void ReadConfig()
        {
            //this.FileAdd("C:/softwear/c12d_sqlserver2000/test.log", this.CurDir);
            string cnfStr = File.ReadAllText(this.CurDir + "/config.json");
            cnfStr = cnfStr.Trim();
            JObject obj = JObject.Parse(cnfStr);
            var timeInterval = obj.Value<int>("timeInterval");
            if (timeInterval > 20000)
            {
                this.timeInterval = timeInterval;
            }
            var url = obj.Value<string>("url");
            this.url = url;
            this.Log("读取配置:" + obj.ToString());
        }

        private void Log(string str)
        {
            if (!Directory.Exists(this.CurDir + "/log/"))
            {
                Directory.CreateDirectory(this.CurDir + "/log/");
            }
            DateTime now = DateTime.Now;
            string path = this.CurDir + "/log/" + now.ToString("yyyy-MM-dd") + ".log";
            if (!File.Exists(path))
            {
                this.ClearLogs();
            }
            this.FileAdd(path, "[" + now.ToString("yyyy-MM-dd HH:mm:ss.f") + "]  LOG " + str + "\r\n");
        }

        private void ClearLogs()
        {
            if (!Directory.Exists(this.CurDir + "/log/")) return;
            List<FileInfo> delFiles = new List<FileInfo>();
            DateTime now = DateTime.Now;
            DirectoryInfo Folder = new DirectoryInfo(this.CurDir + "/log/");
            foreach (FileInfo file in Folder.GetFiles())
            {
                try
                {
                    DateTime dateTime = DateTime.ParseExact(file.Name.Substring(0, file.Name.Length - ".log".Length), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    if (dateTime.AddDays(31 * 6) < now)
                    {
                        delFiles.Add(file);
                    }
                }
                catch (Exception) { }
            }
            foreach (FileInfo file in delFiles)
            {
                file.Delete();
            }
        }
    }
}
