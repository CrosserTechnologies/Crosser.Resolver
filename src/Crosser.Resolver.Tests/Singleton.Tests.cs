using System;
using Crosser.DependencyResolver.Tests.Model;
using Xunit;
using System.Collections.Generic;
using Crosser.Resolve;
using Crosser.Resolve.Model;

namespace Crosser.DependencyResolver.Tests
{
    public class SingletonTests
    {
        [Fact]
        public void ShouldNotBeAbleToMapToClass()
        {
            var ex = Assert.ThrowsAny<TypeInitializationException>(()=> { Singleton<Person>.As(() => new Person { Name = "Ben"}); });
            Assert.IsType(typeof(ResolverException), ex.InnerException);
        }

        [Fact]
        public void CanSetSingleton()
        {
            //Arrange
            Singleton<IPerson>.Reset();
            Singleton<IPerson>.As(() => new Person { Name = "steve" });
            //Act
            var p = Singleton<IPerson>.Get();
            //Assert
            Assert.True(p != null && p.Name == "steve");
        }

        [Fact]
        public void ChangesIsPersistent()
        {
            //Arrange
            Singleton<IPerson>.Reset();
            Singleton<IPerson>.As(() => new Person { Name = "steve" });
            //Act
            var p = Singleton<IPerson>.Get();
            p.Name = "donny";
            var d = Singleton<IPerson>.Get();
            //Assert
            Assert.True(d != null && d.Name == "donny");
        }

        [Fact]
        public void OverridingShouldFailWhenOverridingIsFalse()
        {
            //Arrange
            Singleton<IPerson>.Reset();
            Singleton<IPerson>.As(() => new Person { Name = "steve" });

            //Assert
            Assert.ThrowsAny<Exception>(() => { Singleton<IPerson>.As(() => new Student() { Name = "steve" }); });
        }


        [Fact]
        public void OverridingShouldBeOkWhenOverridingIsTrue()
        {
            //Arrange
            Singleton<IPerson>.Reset();
            Singleton<IPerson>.As(() => new Person { Name = "steve" }, true);
            Singleton<IPerson>.As(() => new Student() { Name = "ben" });
            var p = Singleton<IPerson>.Get();
            //Assert
            Assert.True(p.Name == "ben");
        }

        [Fact]
        public void OverridingShouldFailWhenSwitchingToFalseInTheMiddle()
        {
            //Arrange
            Singleton<IPerson>.Reset();
            Singleton<IPerson>.As(() => new Person { Name = "steve" }, true);
            Singleton<IPerson>.As(() => new Student() { Name = "ben" });
            //Assert
            Assert.ThrowsAny<Exception>(() => { Singleton<IPerson>.As(() => new Person() { Name = "steve" }); });
        }

        [Fact]
        public void OverridingShouldBeOkWhenSwitchingDoingMultipleOverrides()
        {
            //Arrange
            Singleton<IPerson>.Reset();
            Singleton<IPerson>.As(() => new Person { Name = "steve" }, true);
            Singleton<IPerson>.As(() => new Student() { Name = "ben" }, true);
            Singleton<IPerson>.As(() => new Person { Name = "steve" });
            var p = Singleton<IPerson>.Get();
            //Assert
            Assert.True(p.Name == "steve");
        }

        [Fact]
        public void CantGetInstanceWhenInterfaceDisabled()
        {
            //Arrange

            Singleton<IPerson>.Reset();

            Singleton<IPerson>.As(() => new Person());

            Singleton<IPerson>.Disable();

            var p = Singleton<IPerson>.Get();
            //Assert
            Assert.True(p == null);
        }

        [Fact]
        public void CantGetInstanceWhenInterfaceEnabledAfterBeingDisabled()
        {
            //Arrange
            Singleton<IPerson>.Reset();

            Singleton<IPerson>.As(() => new Person());

            var p = Singleton<IPerson>.Get();
            Assert.True(p != null);
            Singleton<IPerson>.Disable();
            p = Singleton<IPerson>.Get();
            Assert.True(p == null);
            Singleton<IPerson>.Enable();
            p = Singleton<IPerson>.Get();
            Assert.True(p != null);
        }

        [Fact]
        public void CanGetMetadataFromInstance()
        {
            Singleton<IPerson>.Reset();

            var props = new Dictionary<string, object>() { { "num", 123 }, { "str", "hello world" } };
            Singleton<IPerson>.As(() => new Person(), properties: props);

            var p = Singleton<IPerson>.Get();
            Assert.True(p != null && Singleton<IPerson>.Properties.ContainsKey("num") && (int)Singleton<IPerson>.Properties["num"] == 123);
        }
    }
}
