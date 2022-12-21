using System.Collections.Concurrent;

namespace SPP_DepInjectionCon
{
    // тип интерфейса, тип класса
    // Чтобы задать свзять между классом и интерфейсом
    public class config
    {
        public readonly ConcurrentDictionary<Type, List<ImplementationInfo>> RegisteredDependencies;

        public config()
        {
            RegisteredDependencies = new ConcurrentDictionary<Type, List<ImplementationInfo>>();
        }

        public void Register<TDependency, TImplementation>(LifeTime lifeTime = LifeTime.InstancePerDependency)
        {
            Register(typeof(TDependency), typeof(TImplementation), lifeTime);
        }

        public void Register(Type interfaceType, Type classType, LifeTime lifeTime = LifeTime.InstancePerDependency)
        {
            // Валидация может ли класс реализовать интерфейс
            if (classType.IsAbstract || (!interfaceType.IsAssignableFrom(classType) && !interfaceType.IsGenericTypeDefinition))
            {
                throw new Exception("Dependency registration exception");
            }

            if (!RegisteredDependencies.ContainsKey(interfaceType))
            {
                List<ImplementationInfo> impl = new List<ImplementationInfo> { new ImplementationInfo(lifeTime, classType) };
                RegisteredDependencies.TryAdd(interfaceType, impl);
            }
            else
            {
                ImplementationInfo newImplInfo = new ImplementationInfo(lifeTime, classType);
                if (RegisteredDependencies[interfaceType].Where(implInf => newImplInfo.Equals(implInf)).Count() == 0)
                    RegisteredDependencies[interfaceType].Add(newImplInfo);
            }
        }
    }
}