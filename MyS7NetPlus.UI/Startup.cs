using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
namespace MyS7NetPlus.UI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // DI服务注册
        public void ConfigureServices(IServiceCollection services)
        {
            // 启用控制器，用于WebApi
            services.AddControllers().AddNewtonsoftJson(options => {
                // 屏蔽循环引用处理，否则要报错
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });
        }

        // http请求管道
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            //开启错误页面
            app.UseDeveloperExceptionPage();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
