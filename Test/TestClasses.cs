namespace Test
{
    public interface IService { }
    public abstract class AbstractService : IService { }
    public class ServiceImplAbstr : AbstractService
    {
        public ServiceImplAbstr()
        { }
    }

    public class ServiceImplInt : IService
    {
        public ServiceImplInt()
        { }
    }
    public class ServiceImplIntSecond : IService { }

    interface IComplexService { }
    class ComplexServiceImpl : IComplexService
    {
        public IRepository Repository;
        public ComplexServiceImpl(IRepository repository)
        {
            Repository = repository;
        }
    }

    interface IRepository { }
    class RepositoryImpl : IRepository
    {
        public RepositoryImpl() { }
    }
    interface IGenService<TRepository> where TRepository : IRepository { }
    class GenServiceImpl<TRepository> : IGenService<TRepository>
        where TRepository : IRepository
    {
        public GenServiceImpl(TRepository repository) { }
    }
}