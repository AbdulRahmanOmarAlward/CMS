﻿using System;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Identity;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;

using Panther.CMS.Entities;

namespace Panther.CMS.Setup
{
    public static class Initializer
    {
        private const string defaultAdminRole = "DefaultAdminRole";
        private const string defaultAdminUserName = "DefaultAdminUserName";
        private const string defaultAdminPassword = "DefaultAdminPassword";

        public static async Task Initialize(IApplicationBuilder app)
        {
            var appEnv = app.ApplicationServices.GetService<IApplicationEnvironment>();
            var configuration = new ConfigurationBuilder(appEnv.ApplicationBasePath)
                .AddJsonFile("config.json", true)
                .AddEnvironmentVariables().Build();


            var userManager = app.ApplicationServices.GetService<UserManager<User>>();
            var roleManager = app.ApplicationServices.GetService<RoleManager<Role>>();
            var defaultAdminRoleValue = configuration[defaultAdminRole];
            if (!await roleManager.RoleExistsAsync(defaultAdminRoleValue))
            {
                await roleManager.CreateAsync(new Role {Name = defaultAdminRoleValue });
            }

            var user = await userManager.FindByNameAsync(configuration[defaultAdminUserName]);
            if (user == null)
            {
                user = new User { Username = configuration[defaultAdminUserName]};
                await userManager.CreateAsync(user, configuration[defaultAdminPassword]);
                await userManager.AddToRoleAsync(user, defaultAdminRoleValue);
                await userManager.AddClaimAsync(user, new Claim("Administration", "Allowed"));
            }
        }
    }
}