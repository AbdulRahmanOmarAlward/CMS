﻿using System;
using System.Net;

using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.StaticFiles;

using Newtonsoft.Json;

using Panther.CMS;
using Panther.CMS.Constraints;
using Panther.CMS.Extensions;
using Panther.CMS.Setup;

namespace Microsoft.Framework.DependencyInjection
{
    /// <summary>
    /// Summary description for BuilderExtensions
    /// </summary>
    public static class BuilderExtensions
    {
        public static IApplicationBuilder UsePanther(this IApplicationBuilder app)
        {
            UsePanther(app, config =>
            {
                config.Test = "Test";
                config.LoginPath = "/login";
            });

            return app;
        }

        public static IApplicationBuilder UsePanther(this IApplicationBuilder app, Action<PantherConfig> config)
        {

            var pantherConfig = new PantherConfig();
            config(pantherConfig);

            app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = new StaticFilesMimeType() })
            .UseIdentity()
            .UseCookieAuthentication(options =>
            {
                options.LoginPath = new PathString(pantherConfig.LoginPath);
                options.AutomaticAuthentication = true;
                options.Notifications = new CookieAuthenticationNotifications()
                {
                    OnApplyRedirect = (context) =>
                    {
                        if (context.Request.IsApiRequest())
                        {
                            var jsonResponse = JsonConvert.SerializeObject(new
                            {
                                status = context.Response.StatusCode,
                                headers = new
                                {
                                    location = context.RedirectUri
                                }
                            }, Formatting.None);
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            context.Response.Headers.Append("X-Responded-JSON", jsonResponse);
                        }
                        else
                        {
                            context.Response.Redirect(context.RedirectUri);
                        }
                    }
                };
            })
            .UseMiddleware<PantherMiddleware>()
            .UseMvc(routes =>
            {
                routes.MapRoute(null, "api/{controller}", new { controller = "Site" });
                routes.MapRoute("default", "{controller}/{action}", new { controller = "Home", action = "Index" }, new { route = new TestConstaint() });
                routes.MapRoute("defaultCulture", "{culture}/{*url}", new { culture = "nl-nl", controller = "Panther", action = "CurrentPage" }, new { host = new HostConstraint() });
                routes.MapRoute("cmsroute", "{*url}", new { culture = "nl-nl", controller = "Panther", action = "CurrentPage" }, new { host = new HostConstraint() });
            });

            Initializer.Initialize(app.ApplicationServices).Wait();

            return app;
        }
    }
}