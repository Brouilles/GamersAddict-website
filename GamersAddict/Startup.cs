using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GamersAddict.Startup))]
namespace GamersAddict
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Bot
            DiscordBot bot = new DiscordBot();

            // ASP
            ConfigureAuth(app);
        }
    }
}
