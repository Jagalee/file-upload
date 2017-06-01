using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SocialStorageAPIAccess.Startup))]
namespace SocialStorageAPIAccess
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
