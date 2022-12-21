using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SPP_DepInjectionCon;

namespace Test
{
    public class configTest
    {
        [Test]
        public void Register_InterfaceImplementation_SuccessfulRegistration()
        {
            config configuration = new config();
            configuration.Register(typeof(IService), typeof(ServiceImplInt));

            ImplementationInfo actual = configuration.RegisteredDependencies[typeof(IService)][0];

            Assert.AreEqual(LifeTime.InstancePerDependency, actual.LifeTime, "LifeTimes are not equal.");
            Assert.AreEqual(typeof(ServiceImplInt), actual.ImplClassType, "ImplClassType are not equal.");
        }

        [Test]
        public void Register_AbstructImplementation_SuccessfulRegistration()
        {
            config configuration = new config();
            configuration.Register(typeof(AbstractService), typeof(ServiceImplAbstr));

            ImplementationInfo actual = configuration.RegisteredDependencies[typeof(AbstractService)][0];

            Assert.AreEqual(LifeTime.InstancePerDependency, actual.LifeTime, "LifeTimes are not equal.");
            Assert.AreEqual(typeof(ServiceImplAbstr), actual.ImplClassType, "ImplClassType are not equal.");
        }

        [Test]
        public void Register_AsSelfRegistration_SuccessfulRegistration()
        {
            config configuration = new config();
            configuration.Register(typeof(ServiceImplInt), typeof(ServiceImplInt));

            ImplementationInfo actual = configuration.RegisteredDependencies[typeof(ServiceImplInt)][0];

            Assert.AreEqual(LifeTime.InstancePerDependency, actual.LifeTime, "LifeTimes are not equal.");
            Assert.AreEqual(typeof(ServiceImplInt), actual.ImplClassType, "ImplClassType are not equal.");
        }

        [Test]
        public void Register_GenericRegister_SuccessfulRegistration()
        {
            config configuration = new config();
            configuration.Register<IService, ServiceImplInt>();

            ImplementationInfo actual = configuration.RegisteredDependencies[typeof(IService)][0];

            Assert.AreEqual(LifeTime.InstancePerDependency, actual.LifeTime, "LifeTimes are not equal.");
            Assert.AreEqual(typeof(ServiceImplInt), actual.ImplClassType, "ImplClassType are not equal.");
        }

        [Test]
        public void Register_Singleton_SuccessfulRegistration()
        {
            config configuration = new config();
            configuration.Register(typeof(IService), typeof(ServiceImplInt), LifeTime.Singleton);

            ImplementationInfo actual = configuration.RegisteredDependencies[typeof(IService)][0];

            Assert.AreEqual(LifeTime.Singleton, actual.LifeTime, "LifeTimes are not equal.");
            Assert.AreEqual(typeof(ServiceImplInt), actual.ImplClassType, "ImplClassType are not equal.");
        }

        [Test]
        public void Register_DoubleRegistration_OneElement()
        {
            config configuration = new config();
            configuration.Register<IService, ServiceImplInt>();
            configuration.Register<IService, ServiceImplInt>();

            Assert.AreEqual(1, configuration.RegisteredDependencies[typeof(IService)].Count, "There is a duplication of elements.");
        }

        [Test]
        public void Register_AbstractClass_RegistrationException()
        {
            config configuration = new config();
            Assert.Throws<Exception>(() => configuration.Register(typeof(IService), typeof(AbstractService)));
        }

        [Test]
        public void Register_TwoDifferentClasses_RegistrationException()
        {
            config configuration = new config();
            Assert.Throws<Exception>(() => configuration.Register(typeof(ServiceImplInt), typeof(ServiceImplIntSecond)));
        }
    }
}
