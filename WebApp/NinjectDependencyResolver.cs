using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Ninject;
using MvcAsyncChat.Domain;
using MvcAsyncChat.Svcs;

namespace MvcAsyncChat
{
    public class NinjectDependencyResolver : IDependencyResolver
    {
        readonly IKernel kernel = new StandardKernel();

        public NinjectDependencyResolver()
        {
            kernel.Bind<IAuthSvc>().To<FormsAuthSvc>();
            kernel.Bind<IMessageRepo>().To<InMemMessageRepo>();
            kernel.Bind<ICallbackQueue>().To<CallbackQueue>().InSingletonScope();
            kernel.Bind<IDateTimeSvc>().To<DateTimeSvc>();
            kernel.Bind<ITimerSvc>().To<TimerSvc>().InSingletonScope();
            kernel.Bind<IChatRoom>().To<ChatRoom>().InSingletonScope();
            kernel.Bind<IControllerActivator>().To<NinjectControllerActivator>();
        }

        public object GetService(Type serviceType)
        {
            try
            {
                return kernel.Get(serviceType);
            }
            catch (ActivationException)
            {
                return null;
            }
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return kernel.GetAll(serviceType);
        }
    }
}