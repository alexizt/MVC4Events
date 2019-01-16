using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using mvc4events.Models;
using Unity.Mvc4;

namespace mvc4events
{
    public static class Bootstrapper
    {
        public static IUnityContainer Initialise()
        {
            var container = BuildUnityContainer();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));

            return container;
        }

        private static IUnityContainer BuildUnityContainer()
        {
            var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();    
            //List<Event> ListEvents = mvc4events.Models.Event.GetEvents();
            //container.RegisterInstance<List<Event>>(ListEvents, new ContainerControlledLifetimeManager());
            container.RegisterType<IEventsRepository, EventsRepository>(new ContainerControlledLifetimeManager());
            RegisterTypes(container);

            return container;
        }

        public static void RegisterTypes(IUnityContainer container)
        {

        }
    }
}