using System;
using System.Text;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raven.Server.Routing;

namespace Raven.Server
{
    public class RavenServerStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerfactory)
        {
            var router = new RequestRouter(RouteScanner.Scan());
            app.Run(async context =>
            {
                try
                {
                    await router.HandlePath(context);
                }
                catch (Exception e)
                {
                    if (context.RequestAborted.IsCancellationRequested)
                        return;

                    var response = context.Response;
                    response.StatusCode = 500;
                    var sb = new StringBuilder();
                    sb.Append(context.Request.Path).Append('?').Append(context.Request.QueryString)
                        .AppendLine()
                        .Append("- - - - - - - - - - - - - - - - - - - - -")
                        .AppendLine();
                    sb.Append(e);
                    await response.WriteAsync(e.ToString());
                }
            });
        }
    }
}