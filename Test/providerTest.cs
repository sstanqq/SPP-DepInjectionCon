using SPP_DepInjectionCon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SPP_DepInjectionCon;

namespace Test
{
    public class providerTest
    {
        [Test]
        public void Resolve_InterfaceImplementation_RequiredTypeObject()
        {
            config configuration = new config();
            configuration.Register(typeof(IService), typeof(ServiceImplInt));

            provider provider = new provider(configuration);
            var service = provider.Resolve<IService>();

            Assert.AreEqual(service.GetType(), typeof(ServiceImplInt));
        }

        [Test]
        public void Resolve_AbstractImplementation_RequiredTypeObject()
        {
            config configuration = new config();
            configuration.Register(typeof(AbstractService), typeof(ServiceImplAbstr));

            provider provider = new provider(configuration);
            var service = provider.Resolve<AbstractService>();

            Assert.AreEqual(service.GetType(), typeof(ServiceImplAbstr));
        }

        [Test]
        public void Resolve_AsSelfImplementation_RequiredTypeObject()
        {
            config configuration = new config();
            configuration.Register(typeof(ServiceImplInt), typeof(ServiceImplInt));

            provider provider = new provider(configuration);
            var service = provider.Resolve<ServiceImplInt>();

            Assert.AreEqual(service.GetType(), typeof(ServiceImplInt));
        }

        [Test]
        public void Resolve_Recursion_RequiredTypeObject()
        {
            config configuration = new config();
            configuration.Register<IComplexService, ComplexServiceImpl>();
            configuration.Register<IRepository, RepositoryImpl>();

            provider provider = new provider(configuration);
            var complexService = provider.Resolve<IComplexService>();
            ComplexServiceImpl serviceImpl = (ComplexServiceImpl)complexService;

            Assert.AreEqual(complexService.GetType(), typeof(ComplexServiceImpl));
            Assert.AreEqual(serviceImpl.Repository.GetType(), typeof(RepositoryImpl));
        }

        [Test]
        public void Resolve_WithoutConfiguration_Exception()
        {
            config configuration = new config();

            provider provider = new provider(configuration);
            Assert.Throws<Exception>(() => provider.Resolve<IService>());
        }

        [Test]
        public void Resolve_LifeTime_InstancePerDependency()
        {
            config configuration = new config();
            configuration.Register<IService, ServiceImplInt>(LifeTime.InstancePerDependency);

            provider provider = new provider(configuration);
            var impl1 = provider.Resolve<IService>();
            var impl2 = provider.Resolve<IService>();

            Assert.AreNotSame(impl1, impl2);
        }

        [Test]
        public void Resolve_LifeTime_Singleton()
        {
            config configuration = new config();
            configuration.Register<IService, ServiceImplInt>(LifeTime.Singleton);

            provider provider = new provider(configuration);
            var impl1 = provider.Resolve<IService>();
            var impl2 = provider.Resolve<IService>();

            Assert.AreSame(impl1, impl2);
        }


        [Test]
        public void Resolve_MultipleImplementations_RequiredTypeObject()
        {
            config configuration = new config();
            configuration.Register(typeof(IService), typeof(ServiceImplInt));
            configuration.Register(typeof(IService), typeof(ServiceImplIntSecond));

            provider provider = new provider(configuration);
            var services = provider.Resolve<IEnumerable<IService>>();

            Assert.AreEqual(services.ElementAt(0).GetType(), typeof(ServiceImplInt));
            Assert.AreEqual(services.ElementAt(1).GetType(), typeof(ServiceImplIntSecond));
        }
    }
}
