using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Food_Web.Startup))]
namespace Food_Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();
        }
        //public void Configuration(IAppBuilder app)
        //{
        //    app.MapSignalR(); // Thêm cấu hình SignalR
        //}

    }
}
