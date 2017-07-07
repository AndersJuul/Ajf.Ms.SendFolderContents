using System;
using System.Configuration;
using Ajf.Nuget.Logging;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Topshelf;

namespace SendFolderContents
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Log.Logger = StandardLoggerProvider.GetLogger();

            try
            {
                var appSettings = new AppSettings();

                HostFactory.Run(x => //1
                {
                    x.Service<Worker>(s => //2
                    {
                        try
                        {
                            s.ConstructUsing(name => new Worker(appSettings)); //3
                            s.WhenStarted(tc =>
                            {
                                Log.Logger.Information("Starting service:" + appSettings.Revision);
                                tc.Start();
                                Log.Logger.Information("Service started.");
                            }); //4
                            s.WhenStopped(tc =>
                            {
                                Log.Logger.Information("Stopping service.");
                                tc.Stop();
                                Log.Logger.Information("Service stopped.");
                            }); //5
                            s.WhenPaused(tc =>
                            {
                                Log.Logger.Information("Pausing service.");
                                tc.Stop();
                                Log.Logger.Information("Service paused.");
                            }); //5

                            s.WhenContinued(tc =>
                            {
                                Log.Logger.Information("Continuing service.");
                                tc.Start();
                                Log.Logger.Information("Service continued.");
                            }); //5
                            s.WhenSessionChanged((w, sca) =>
                            {
                                Log.Logger.Information("Session changed: " + w);
                                Log.Logger.Information("Session changed: " + sca);
                            });



                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Fra program");
                            throw;
                        }
                    });
                    x.RunAsLocalSystem(); //6

                    x.SetDescription(appSettings.Description); //7
                    x.SetDisplayName("AJF.SendFolderContents"); //8
                    x.SetServiceName("AJF.SendFolderContents"); //9
                }); //10        }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Yderste");
                throw;
            }
        }
    }
}