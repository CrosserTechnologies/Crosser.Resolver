using System;
using Crosser.DependencyResolver.Tests.Model;
using Xunit;
using System.Collections.Generic;
using Crosser.Resolve;
using Crosser.Resolve.Model;


// TODO: Properties test and more test for enabled, rewritable

namespace Crosser.DependencyResolver.Tests
{
    public class OneTests
    {
        [Fact]
        public void ShouldNotBeAbleToMapToClass()
        {
            var ex = Assert.ThrowsAny<TypeInitializationException>(()=> { One<string>.As(() => ""); });
            Assert.IsType(typeof(ResolverException), ex.InnerException);
        }

        [Fact]
        public void CanGetInstanceFromInterface()
        {
            //Arrange
            One<IPerson>.Reset();
            One<IPerson>.As(() => new Person {Name = "steve"});
            //Act
            var p = One<IPerson>.Get();

            //Assert
            Assert.True(p != null && p.Name == "steve");
        }

        [Fact]
        public void OverridingShouldFailWhenOverridingIsFalse()
        {
            //Arrange
            One<IPerson>.Reset();
            One<IPerson>.As(() => new Person { Name = "steve" });
            
            //Assert
            Assert.ThrowsAny<Exception>(() => { One<IPerson>.As(() => new Student() { Name = "steve" }); });
        }


        [Fact]
        public void OverridingShouldBeOkWhenOverridingIsTrue()
        {
            //Arrange
            One<IPerson>.Reset();
            One<IPerson>.As(() => new Person { Name = "steve" }, true);
            One<IPerson>.As(() => new Student() { Name = "ben" });
            var p = One<IPerson>.Get();
            //Assert
            Assert.True(p.Name == "ben");
        }

        [Fact]
        public void OverridingShouldFailWhenSwitchingToFalseInTheMiddle()
        {
            //Arrange
            One<IPerson>.Reset();
            One<IPerson>.As(() => new Person { Name = "steve" }, true);
            One<IPerson>.As(() => new Student() { Name = "ben" });            
            //Assert
            Assert.ThrowsAny<Exception>(() => { One<IPerson>.As(() => new Person() { Name = "steve" }); });
        }

        [Fact]
        public void OverridingShouldBeOkWhenSwitchingDoingMultipleOverrides()
        {
            //Arrange
            One<IPerson>.Reset();
            One<IPerson>.As(() => new Person { Name = "steve" }, true);
            One<IPerson>.As(() => new Student() { Name = "ben" },true);
            One<IPerson>.As(() => new Person { Name = "steve" });

            var p = One<IPerson>.Get();
            //Assert
            Assert.True(p.Name == "steve");
        }

        [Fact]
        public void CanHaveMultipleDependencies()
        {
            //Arrange
            One<IPersonRepository>.Reset();
            One<IPersonService>.Reset();
 
            One<IPersonService>.As(()=>new PersonService());
            One<IPersonRepository>.As(() => new PersonRepository());

            var service = One<IPersonService>.Get();
            //Assert
            Assert.True(service != null);
        }

        [Fact]
        public void CanHaveMultipleDependenciesWithCtorInjection()
        {
            //Arrange
            One<IPersonRepository>.Reset();
            One<IPersonService>.Reset();

            One<IPersonService>.As(() => new PersonService(One<IPersonRepository>.Get()));
            One<IPersonRepository>.As(() => new PersonRepository());

            var service = One<IPersonService>.Get();
            //Assert
            Assert.True(service != null);
        }

        [Fact]
        public void CanHaveMultipleDependenciesWithCtorConstruction()
        {
            //Arrange
            One<IPersonRepository>.Reset();
            One<IPersonService>.Reset();

            One<IPersonService>.As(() => new PersonService(new PersonRepository()));

            var service = One<IPersonService>.Get();
            //Assert
            Assert.True(service != null);
        }


        [Fact]
        public void CantGetInstanceWhenInterfaceDisabled()
        {
            //Arrange
            One<IPerson>.Reset();

            One<IPerson>.As(() => new Person());

            One<IPerson>.Disable();

            var p = One<IPerson>.Get();
            //Assert
            Assert.True(p == null);
        }

        [Fact]
        public void CantGetInstanceWhenInterfaceEnabledAfterBeingDisabled()
        {
            //Arrange
            One<IPerson>.Reset();

            One<IPerson>.As(() => new Person());

            var p = One<IPerson>.Get();
            Assert.True(p != null);
            One<IPerson>.Disable();
            p = One<IPerson>.Get();            
            Assert.True(p == null);
            One<IPerson>.Enable();
            p = One<IPerson>.Get();
            Assert.True(p != null);
        }

        [Fact]
        public void CanGetMetadataFromInstance()
        {
            One<IPerson>.Reset();

            var props = new Dictionary<string, object>() { { "num", 123 }, { "str", "hello world" } };
            One<IPerson>.As(() => new Person(), properties: props);

            var p = One<IPerson>.Get();
            Assert.True(p != null && One<IPerson>.Properties.Count == 2 && (int)One<IPerson>.Properties["num"] == 123);            
        }

        [Fact]
        public void CanAddGeneric()
        {
            One<IHub<FakeHub>>.Reset();

            One<IHub<FakeHub>>.As(() => new FakeHub());

            var p = One<IHub<FakeHub>>.Get();
            
            Assert.True(p != null);
        }
    }
}
