using Crosser.Resolve;

namespace Crosser.DependencyResolver.Tests.Model
{
    public interface IHub<T> where T : class
    {
        T Instance();
    }

    public abstract class BaseHub<T> : IHub<T> where T : class
    {
        public abstract T Instance();
    }

    public class FakeHub : BaseHub<FakeHub>
    {
        public override FakeHub Instance()
        {
            return new FakeHub();
        }
    }

    public interface IPerson
    {
        string Name { get; set; }
    }
    public class Person : IPerson
    {
        public string Name { get; set; }

        public Person()
        {

        }
    }

    public class Student : Person
    {
    }

    public interface IPersonService
    {
        void Save(IPerson p);
    }

    public class PersonService : IPersonService
    {
        private IPersonRepository personRepository;

        public PersonService()
        {
            this.personRepository = One<IPersonRepository>.Get();
        }
        public PersonService(IPersonRepository _personRepository)
        {
            this.personRepository = _personRepository;
        }
        public void Save(IPerson p) {this.personRepository.Save(p); }
    }

    //PersonRepository
    public interface IPersonRepository
    {
        void Save(IPerson p);
    }
    public class PersonRepository : IPersonRepository
    {
        public void Save(IPerson p) { }
    }
}
