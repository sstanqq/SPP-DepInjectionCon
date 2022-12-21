using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SPP_DepInjectionCon
{
    public class provider
    {
        private readonly config _configuration;

        private readonly ConcurrentDictionary<Type, object> _singletonImplementations;

        private readonly Stack<Type> _recursionStack;

        public provider(config configuration)
        {
            _configuration = configuration;
            _singletonImplementations = new ConcurrentDictionary<Type, object>();
            _recursionStack = new Stack<Type>();
        }

        public TDependency Resolve<TDependency>()
        {
            return (TDependency)Resolve(typeof(TDependency));
        }

        // Возвращает конкретную реализацию интерфейса. Возвращает объект(класс)
        private object Resolve(Type dependencyType)
        {
            List<ImplementationInfo> infos = GetImplementationsInfos(dependencyType);

            if ((infos == null && !dependencyType.IsGenericType) ||
                (infos == null && dependencyType.IsGenericType && dependencyType.GetGenericTypeDefinition() != typeof(IEnumerable<>)))
            {
                throw new Exception("Unregistered dependency");
            }

            if (_recursionStack.Contains(dependencyType))
                return null;

            _recursionStack.Push(dependencyType);
            if (dependencyType.IsGenericType && dependencyType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                Type genericArgumentType = dependencyType.GetGenericArguments()[0];
                infos = GetImplementationsInfos(genericArgumentType);

                if (infos == null)
                {
                    throw new Exception("Unregistered dependency");
                }

                List<object> implementations = new List<object>();
                foreach (ImplementationInfo info in infos)
                {
                    implementations.Add(GetImplementation(info, dependencyType));
                }

                return ConvertToRequiredType(implementations, genericArgumentType);
            }

            object obj = GetImplementation(infos[0], dependencyType);
            _recursionStack.Pop();
            return obj;
        }

        private List<ImplementationInfo> GetImplementationsInfos(Type dependencyType)
        {
            if (_configuration.RegisteredDependencies.ContainsKey(dependencyType))
            {
                return _configuration.RegisteredDependencies[dependencyType];
            }

            if (!dependencyType.IsGenericType)
            {
                return null;
            }

            Type definition = dependencyType.GetGenericTypeDefinition();
            return _configuration.RegisteredDependencies.ContainsKey(definition)
                ? _configuration.RegisteredDependencies[definition]
                : null;
        }

        private object GetImplementation(ImplementationInfo implInfo, Type dependencyType)
        {
            Type innerTypeForOpenGeneric = null;
            if (implInfo.ImplClassType.IsGenericType && implInfo.ImplClassType.IsGenericTypeDefinition &&
                implInfo.ImplClassType.GetGenericArguments()[0].FullName == null)
                innerTypeForOpenGeneric = dependencyType.GetGenericArguments().FirstOrDefault();

            if (implInfo.LifeTime == LifeTime.Singleton)
            {
                if (!_singletonImplementations.ContainsKey(implInfo.ImplClassType))
                {
                    object singleton = CreateInstanceForDependency(implInfo.ImplClassType, innerTypeForOpenGeneric);
                    _singletonImplementations.TryAdd(implInfo.ImplClassType, singleton);
                }

                return _singletonImplementations[implInfo.ImplClassType];
            }

            return CreateInstanceForDependency(implInfo.ImplClassType, innerTypeForOpenGeneric);
        }

        private object CreateInstanceForDependency(Type implClassType, Type innerTypeForOpenGeneric)
        {
            ConstructorInfo[] constructors = implClassType.GetConstructors()
                .OrderByDescending(x => x.GetParameters().Length).ToArray();
            object implInstance = null;

            foreach (ConstructorInfo constructor in constructors)
            {
                ParameterInfo[] parameters = constructor.GetParameters();
                List<object> paramsValues = new List<object>();
                foreach (ParameterInfo parameter in parameters)
                {
                    if (IsDependency(parameter.ParameterType))
                    {
                        paramsValues.Add(Resolve(parameter.ParameterType));
                    }
                    else
                    {
                        object obj = null;
                        try
                        {
                            obj = Activator.CreateInstance(parameter.ParameterType, null);
                        }
                        catch { }

                        paramsValues.Add(obj);
                    }
                }

                try
                {
                    if (innerTypeForOpenGeneric != null)
                        implClassType = implClassType.MakeGenericType(innerTypeForOpenGeneric);
                    implInstance = Activator.CreateInstance(implClassType, paramsValues.ToArray());
                    break;
                }
                catch { }
            }
            return implInstance;
        }

        private object ConvertToRequiredType(List<object> implementations, Type requiredType)
        {
            MethodInfo castMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast))?.MakeGenericMethod(requiredType);
            MethodInfo toListMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList))?.MakeGenericMethod(requiredType);

            IEnumerable<object> itemsToCast = implementations;
            var castedItems = castMethod?.Invoke(null, new[] { itemsToCast });
            return toListMethod?.Invoke(null, new[] { castedItems });
        }

        private bool IsDependency(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return IsDependency(type.GetGenericArguments()[0]);

            return _configuration.RegisteredDependencies.ContainsKey(type);
        }
    }
}
