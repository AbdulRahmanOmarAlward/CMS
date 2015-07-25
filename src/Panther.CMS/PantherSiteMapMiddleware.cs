﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.DependencyInjection;

using Panther.CMS.Attributes;
using Panther.CMS.Controllers;
using Panther.CMS.Extensions;
using Panther.CMS.Interfaces;
using Panther.CMS.Models;
using Panther.CMS.Services.Page;

namespace Panther.CMS
{
    public class PantherSiteMapMiddleware
    {
        private readonly IServiceProvider serviceProvider;
        private readonly RequestDelegate next;
        private readonly IPageService pageService;
        private readonly IPantherContext context;
        
        public PantherSiteMapMiddleware(RequestDelegate next,
            IServiceProvider serviceProvider,
            IPantherContext context,
            IPageService pageService
            )
        {
            this.next = next;
            this.serviceProvider = serviceProvider;
            this.pageService = pageService;
            this.context = context;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path.Value.Contains("sitemap.xml"))
            {
                var pages = pageService.Get().MakeTree();
                var sitemap = new GoogleSiteMap();

                BuildSitemap(sitemap, pages);
                httpContext.Response.ContentType = "text/xml";
                await httpContext.Response.WriteAsync(sitemap.GetXMLString(), Encoding.UTF8);

                //return Content(sitemap.GetXMLString(), "text/xml", Encoding.UTF8);
                return;
            }

            await next.Invoke(httpContext);
        }

        private void BuildSitemap(GoogleSiteMap sitemap, IEnumerable<Entities.Page> pages)
        {
            foreach (var page in pages)
            {
                AddPage(sitemap, page);
                if (page.Children.Any())
                    BuildSitemap(sitemap, page.Children);
            }
        }

        private void AddPage(GoogleSiteMap sitemap, Entities.Page page)
        {
            var url =  (context.Site.Url + "/" + page.Url).TrimEnd(new[] { '/' });
            var properties = page.GetProperties<GoogleSitemapProperties>();
            sitemap.Create(url, properties.LastModified, properties.Priority, properties.ChangeFrequency);
        }

        public class GoogleSitemapProperties
        {
            [DefaultValue(typeof(ChangeFrequency), "Daily")]
            public ChangeFrequency ChangeFrequency { get; set; }
            [DefaultDate("")]
            public DateTime LastModified { get; set; }
            [DefaultValue("1.0")]
            public string Priority { get; set; }
        }
    }
}
