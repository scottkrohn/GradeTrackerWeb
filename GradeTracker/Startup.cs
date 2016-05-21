using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GradeTracker.Startup))]
namespace GradeTracker
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
