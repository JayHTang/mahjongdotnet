using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Mahjong.Startup))]
namespace Mahjong
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
