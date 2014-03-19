using System;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Web;
using System.Web;
using EPiServerTemplates.Models.Pages;
using EPiServer.ServiceLocation;

namespace EPiServerTemplates.Business
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class SetupDefaultData : IInitializableHttpModule
    {
        private static bool? _generateDefaultData;
        private static object _lock = new object();

        public void Initialize(InitializationEngine context)
        {
            var sites = context.Locate.Advanced.GetInstance<SiteDefinitionRepository>();
            _generateDefaultData = context.HostType == HostType.WebApplication && !sites.List().Any();           
        }

        public void Preload(string[] parameters) { }

        public void Uninitialize(InitializationEngine context) { }

        public void InitializeHttpEvents(HttpApplication application)
        {
            if (!_generateDefaultData.HasValue || _generateDefaultData.Value)
            {
                application.BeginRequest += application_BeginRequest;
            }
        }

        static void application_BeginRequest(object sender, EventArgs e)
        {
            if (!_generateDefaultData.HasValue || !_generateDefaultData.Value)
            {
                return;
            }

            lock (_lock)
            {
                if (!_generateDefaultData.Value)
                {
                    return;
                }

                var sites = ServiceLocator.Current.GetInstance<SiteDefinitionRepository>();
                var repo = ServiceLocator.Current.GetInstance<IContentRepository>();

                var startPage = repo.GetDefault<StartPage>(PageReference.RootPage);
                startPage.Name = "Start";
                startPage.MainBody = new XhtmlString("Hello World!");
                
                SiteDefinition newSite = new SiteDefinition();
                newSite.Name = "Default Site";
                newSite.StartPage = repo.Save(startPage, EPiServer.DataAccess.SaveAction.Publish, EPiServer.Security.AccessLevel.NoAccess).ToReferenceWithoutVersion();
                newSite.SiteUrl = new Uri(((HttpApplication)sender).Context.Request.Url.GetLeftPart(UriPartial.Authority));
                newSite.Hosts.Add(new HostDefinition() { Name = "*" });
                sites.Save(newSite);

                _generateDefaultData = false;

                ((HttpApplication)sender).Context.Response.Redirect(VirtualPathUtilityEx.ToAbsolute(UriSupport.InternalUIUrl.Path));
            }
        }
    }
}