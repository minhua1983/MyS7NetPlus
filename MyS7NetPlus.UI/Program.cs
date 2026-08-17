using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyS7NetPlus.Common.Tool;
using NLog;
using NLog.Web;
using System.Collections.Concurrent;

namespace MyS7NetPlus.UI
{
    internal static class Program
    {
        // 程序顶层日志
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            /*
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
            //*/

            // 加载NLog配置
            LogManager.Setup().LoadConfigurationFromFile("NLog.config");

            try
            {
                _logger.Info("======== WinForm程序启动 ========");

                // 1.构建AspNetCore主机
                var hostBuilder = Host.CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(webHostBuilder =>
                    {
                        webHostBuilder.UseStartup<Startup>();
                    })
                    // 2.配置DI：注册你的共享单例
                    .ConfigureServices((context, services) =>
                    {
                        // 共享单例，WebAPI Controller 和 MainForm 拿到同一个实例
                        services.AddSingleton<ConcurrentQueue<MyS7Task>, ConcurrentQueue<MyS7Task>>();

                        // 关键：把主窗体注册到DI，不要手动new MainForm()
                        services.AddTransient<MyForm>();
                    })
                    .UseNLog();

                var host = hostBuilder.Build();
                var globalSendkQueue = host.Services.GetRequiredService<ConcurrentQueue<MyS7Task>>();

                // 后台异步启动Kestrel，不阻塞WinForm UI线程，不要 await
                _ = host.RunAsync();
                _logger.Info("Kestrel Web服务已后台启动");

                // 启动WinForm界面
                Application.EnableVisualStyles();
                // 必须加这句，否则在Application.Run实例化任何form都要报错
                Application.SetCompatibleTextRenderingDefault(false);
                //然后可以new任意form
                Application.Run(new MyForm(globalSendkQueue));

                // 窗体关闭，停止Kestrel服务
                _logger.Info("主窗体关闭，准备停止Kestrel");
                host.StopAsync().Wait();
                _logger.Info("Kestrel服务已停止");
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "程序致命异常");
                MessageBox.Show($"程序异常：{ex.Message}");
            }
            finally
            {
                // 非常重要：确保日志缓冲区全部写入磁盘
                LogManager.Shutdown();
            }
        }
    }
}