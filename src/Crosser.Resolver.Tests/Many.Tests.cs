using System;
using System.Linq;
//using System.Runtime.Remoting.Messaging;
using Crosser.DependencyResolver.Tests.Model;
using Xunit;
using Crosser.Resolve;
using System.Collections.Generic;
//using Crosser.Resolve.Model;

namespace Crosser.DependencyResolver.Tests
{
    public class ManyTests
    {
        [Fact]
        public void ShouldNotBeAbleToMapToClass()
        {
            
            var ex = Assert.Throws<TypeInitializationException>(()=> { Many<string>.Add(() => ""); });
            Assert.IsType(typeof(ArgumentException), ex.InnerException);
        }

        [Fact]
        public void CanMapManyOfTheSameType()
        {
            //Arrange
            Many<IPerson>.Reset();
            Many<IPerson>.Add(() => new Person { Name = "steve" });
            Many<IPerson>.Add(() => new Student { Name = "donny" });
            //Act
            var p = Many<IPerson>.GetAll();
            //Assert
            Assert.True(p != null && p.Count() == 2 && p.Count(x => x.Name == "donny") == 1);
        }
        
        [Fact]
        public void CanAddSameMoreThanOnceIfRewritable()
        { 
            Many<IPerson>.Reset();
            Many<IPerson>.Add(() => new Person { Name = "steve" }, true);
            Many<IPerson>.Add(() => new Person() { Name = "donny" });

            var p = Many<IPerson>.GetAll();
            Assert.True(Many<IPerson>.GetAll().Count() == 1 && Many<IPerson>.GetAll().Count(x => x.Name == "donny") == 1);
        }

        [Fact]
        public void CanAddNamedInstance()
        {
            Many<IPerson>.Reset();
            Many<IPerson>.Add(() => new Person { Name = "steve" }, namedInstance: "steve");
            

            Assert.True(Many<IPerson>.GetNamedInstance("steve").Name == "steve");
        }

        [Fact]
        public void ShouldReturnNullForMissingNamedInstance()
        {
            Many<IPerson>.Reset();
            var i = Many<IPerson>.GetNamedInstance("steve");

            Assert.True(i == null);
        }

        [Fact]
        public void CanOverrideNamedInstance()
        {
            Many<IPerson>.Reset();
            Many<IPerson>.Add(() => new Person { Name = "steve" },rewritable: true, namedInstance: "steve");
            Many<IPerson>.Add(() => new Person { Name = "steve2" }, namedInstance: "steve");

            Assert.True(Many<IPerson>.GetNamedInstance("steve").Name == "steve2");
        }

        [Fact]
        public void CanRemoveByType()
        {
            Many<IPerson>.Reset();
            Many<IPerson>.Add(() => new Person { Name = "steve" });

            Assert.True(Many<IPerson>.GetAll().Count(p => p.Name == "steve") == 1);

            Many<IPerson>.Remove<Person>();

            Assert.True(Many<IPerson>.GetAll().Count() == 0);
        }

        [Fact]
        public void CanGetPropertiesFromNamedInstance()
        {
            Many<IPerson>.Reset();
            Many<IPerson>.Add(() => new Person { Name = "steve" }, rewritable: true, namedInstance: "steve", properties : new Dictionary<string, object>() { { "a", 1 }, { "b", 2 } });
            
            Assert.True((int)Many<IPerson>.Properties("steve")["b"] == 2);
        }

        [Fact]
        public void PropertiesFromNamedInstanceIsNullIfNotSet()
        {
            Many<IPerson>.Reset();
            Many<IPerson>.Add(() => new Person { Name = "steve" }, rewritable: true, namedInstance: "steve");

            Assert.True(Many<IPerson>.Properties("steve") == null);
        }

        [Fact]
        public void PropertiesFromMisingNamedInstanceIsNull()
        {
            Many<IPerson>.Reset();
            Many<IPerson>.Add(() => new Person { Name = "steve" }, rewritable: true);

            Assert.True(Many<IPerson>.Properties("ben") == null);
        }

        [Fact]
        public void CanGetPropertiesFromInstance()
        {
            Many<IPerson>.Reset();
            Many<IPerson>.Add(() => new Person { Name = "steve" }, rewritable: true, namedInstance: "steve", properties: new Dictionary<string, object>() { { "a", 1 }, { "b", 2 } } );

            Assert.True((int)Many<IPerson>.Properties<Person>()["b"] == 2);
        }

        [Fact]
        public void CanGetPropertiesFromDifferentNamedInstances()
        {
            Many<IPerson>.Reset();
            Many<IPerson>.Add(() => new Person { Name = "steve" }, rewritable: true, namedInstance: "steve", properties: new Dictionary<string, object>() { { "a", 1 }, { "b", 2 } });
            Many<IPerson>.Add(() => new Student { Name = "ben" }, rewritable: true, namedInstance: "ben", properties: new Dictionary<string, object>() { { "a", 3 }, { "b", 4 } });

            Assert.True((int)Many<IPerson>.Properties("steve")["b"] == 2);
            Assert.True((int)Many<IPerson>.Properties("ben")["b"] == 4);

            Assert.True((int)Many<IPerson>.Properties<Person>()["b"] == 2);
            Assert.True((int)Many<IPerson>.Properties<Student>()["b"] == 4);
        }


        [Fact]
        public void CanDisableAll()
        {
            Many<IPerson>.Reset();
            Many<IPerson>.Add(() => new Person { Name = "steve" });
            Many<IPerson>.Add(() => new Student { Name = "ben" });

            var count = Many<IPerson>.GetAll().Count();
            Assert.True(count == 2);

            Many<IPerson>.DisableAll();
            count = Many<IPerson>.GetAll().Count();

            Assert.True(count == 0);
        }

        [Fact]
        public void CanDisableAllOfType()
        {
            Many<IPerson>.Reset();
            Many<IPerson>.Add(() => new Person { Name = "steve" });
            Many<IPerson>.Add(() => new Student { Name = "ben" });

            var count = Many<IPerson>.GetAll().Count();
            Assert.True(count == 2);

            Many<IPerson>.DisableAllOf<Person>();
            
            Assert.True(Many<IPerson>.GetAll().First().Name == "ben" && Many<IPerson>.GetAll().Count() == 1);
        }

        [Fact]
        public void CanEnableAll()
        {
            Many<IPerson>.Reset();
            Many<IPerson>.Add(() => new Person { Name = "steve" });
            Many<IPerson>.Add(() => new Student { Name = "ben" });

            var count = Many<IPerson>.GetAll().Count();
            Assert.True(count == 2);

            Many<IPerson>.DisableAll();
            count = Many<IPerson>.GetAll().Count();

            Assert.True(count == 0);

            Many<IPerson>.EnableAll();
            count = Many<IPerson>.GetAll().Count();
            Assert.True(count == 2);
        }

        [Fact]
        public void CanEnableAllOfType()
        {
            Many<IPerson>.Reset();
            Many<IPerson>.Add(() => new Person { Name = "steve" });
            Many<IPerson>.Add(() => new Student { Name = "ben" });

            var count = Many<IPerson>.GetAll().Count();
            Assert.True(count == 2);

            Many<IPerson>.DisableAllOf<Person>();

            Assert.True(Many<IPerson>.GetAll().First().Name == "ben" && Many<IPerson>.GetAll().Count() == 1);

            Many<IPerson>.EnableAllOf<Person>();

            count = Many<IPerson>.GetAll().Count();
            Assert.True(count == 2);
        }

    }
}
