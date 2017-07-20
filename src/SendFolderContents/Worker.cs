using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SendGrid;
using Serilog;

namespace SendFolderContents
{
    public class Worker
    {
        private readonly IAppSettings _appSettings;
        private BackgroundWorker _backgroundWorker;

        public Worker(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public bool WorkDone { get; set; }

        public void Start()
        {
            try
            {
                _backgroundWorker = new BackgroundWorker
                {
                    WorkerSupportsCancellation = true
                };
                _backgroundWorker.DoWork += _backgroundWorker_DoWork;
                _backgroundWorker.RunWorkerCompleted += _backgroundWorker_RunWorkerCompleted;
                _backgroundWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "During Start", new object[0]);
                throw;
            }
        }

        private void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            WorkDone = true;
        }

        private void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Log.Logger.Information("Doing work");

            try
            {
                DoWorkInternal(sender);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "During do work", new object[0]);
                throw;
            }
        }

        private void DoWorkInternal(object sender)
        {
            WorkDone = false;

            var lastSend = DateTime.MinValue;
            
            while (true)
            {
                var backgroundWorker = sender as BackgroundWorker;
                if (backgroundWorker == null || backgroundWorker.CancellationPending)
                {
                    Log.Information("backgroundworker.CancellationPending: {@backgroundWorker}", backgroundWorker);
                    return;
                }

                Thread.Sleep(1 * 1000);

                if (_appSettings.Minutes.Contains (DateTime.Now.Minute) )
                {
                    if (_appSettings.Hours.Contains( DateTime.Now.Hour))
                    {
                        if (DateTime.Now.Subtract(lastSend) > _appSettings.MinSleep)
                        {
                            Log.Information("AppSettings match up with current time. Sending.");
                            foreach (var job in _appSettings.Jobs)
                            {
                                SendMail(job.Path, job.Receiver).Wait();
                            }
                            lastSend = DateTime.Now;
                        }
                    }
                }
            }
        }

        private async Task SendMail(string path, string email)
        {
            var folderContents = GetFolderContents(path,path).ToArray();
            



            var fromMailAddress = new MailAddress("andersjuulsfirma@gmail.com");
            var networkCredential = new NetworkCredential("azure_9ce305d9c6ab84b3d5fb7723deca51f5@azure.com",
                "6oi3vEdJl5GldVc");
            var transport = new Web(networkCredential);


            var subjectLine = "Filer paa JuulNas, " + path;

            // Create the email object first, then add the properties.
            var myMessage = new SendGridMessage {From = fromMailAddress};

            myMessage.AddTo(email);
            myMessage.AddCc("andersjuulsfirma@gmail.com");

            var bytes = Encoding.Default.GetBytes(subjectLine);
            myMessage.Subject = Encoding.UTF8.GetString(bytes);

            var html = $"Filer i {path}:<BR/>";
            html += "--<BR/>";
            html += $"Ialt : {folderContents.Count()}<BR/>";
            html += "--<BR/>";
            foreach (var line in folderContents)
            {
                html += line + "<BR/>";
            }
            myMessage.Html = html;

            // Send the email.
            Log.Logger.Information("Should send: {@email}, {@subject}, {@body}",email,subjectLine,html);
            if (ConfigurationManager.AppSettings["PerformActualSend"] == "1")
            {
                await transport.DeliverAsync(myMessage);
            }
        }

        private IEnumerable<string> GetFolderContents(string path, string originalPath)
        {
            var result = new List<string>();
            foreach (var file in Directory.GetFiles(path).OrderByDescending(x=>x))
            {
                result.Add(file.Replace(originalPath,""));
            }

            foreach (var dir in Directory.GetDirectories(path).OrderByDescending(x => x))
            {
                result.AddRange(GetFolderContents(dir, originalPath));
            }
            return result;
        }


        public void Stop()
        {
            if (_backgroundWorker != null)
            {
                _backgroundWorker.CancelAsync();

                while (!WorkDone)
                {
                    Thread.Sleep(500);
                }
            }
        }
    }
}