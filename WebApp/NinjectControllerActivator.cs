using System;
using System.Web.Mvc;
using System.Web.Routing;
using Ninject;

namespace MvcAsyncChat
{
    public class NinjectControllerActivator : IControllerActivator
    {
        readonly IKernel kernel;
        
        public NinjectControllerActivator(IKernel kernel)
        {
            this.kernel = kernel;
        }
            
        public IController Create(RequestContext requestContext, Type controllerType)
        {
            return kernel.Get(controllerType) as IController;
        }
    }
}