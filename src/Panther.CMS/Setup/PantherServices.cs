﻿using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Identity;
using Microsoft.Framework.DependencyInjection;

using Panther.CMS.Entities;
using Panther.CMS.Interfaces;
using Panther.CMS.Routing;
using Panther.CMS.Services.Content;
using Panther.CMS.Services.Media;
using Panther.CMS.Services.Page;
using Panther.CMS.Services.Site;
using Panther.CMS.Storage.Media;
using Panther.CMS.Storage.Resource;
using Panther.CMS.Storage.Role;
using Panther.CMS.Storage.User;


namespace Panther.CMS.Setup
{
    public class PantherServices
    {
        public static void GetDefaultServices(IServiceCollection collection)
        {
            //var describe = new ServiceDescriptor(configuration);
            //Options
            collection.AddTransient<IPageService, PageService>();
            collection.AddTransient<ISiteService, SiteService>();
            collection.AddTransient<IContentService, ContentService>();
            collection.AddTransient<IMediaService, MediaService>();
            collection.AddSingleton<IPantherContext, PantherContext>();
            collection.AddTransient<IPantherFileSystem, PantherFileSystem>();
            collection.AddTransient<IPantherRouter, PantherRouter>();
            //collection.AddTransient<IEmailParser, EmailParser>();

            collection.AddScoped<IUserStore<User>, UserStore>();
            collection.AddScoped<IRoleStore<Role>, RoleStore>();
            collection.AddTransient<IResourceStore, ResourceStore>();
            collection.AddTransient<IMediaStore, MediaStore>();
            //Storages
        }
    }
}