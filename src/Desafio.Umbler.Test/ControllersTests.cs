using Desafio.Umbler.Controllers;
using Desafio.Umbler.Models;
using Desafio.Umbler.Repository;
using DnsClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Desafio.Umbler.Test
{
    [TestClass]
    public class ControllersTest
    {
        private readonly ILookupClient _lookupClient = new LookupClient();

        [TestMethod]
        public void Home_Index_returns_View()
        {
            //arrange 
            var controller = new HomeController();

            //act
            var response = controller.Index();
            var result = response as ViewResult;

            //assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Home_Error_returns_View_With_Model()
        {
            //arrange 
            var controller = new HomeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            //act
            var response = controller.Error();
            var result = response as ViewResult;
            var model = result.Model as ErrorViewModel;

            //assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public async Task Domain_In_Database()
        {
            //arrange 
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: "Find_searches_url")
                .Options;

            var domain = new Domain { Id = 1, Ip = "192.168.0.1", Name = "test.com", UpdatedAt = DateTime.Now, HostedAt = "umbler.corp", Ttl = 60, WhoIs = "Ns.umbler.com" };

            // Insert seed data into the database using one instance of the context
            using (var db = new DatabaseContext(options))
            {
                db.Domains.Add(domain);
                db.SaveChanges();
            }

            // Use a clean instance of the context to run the test
            using (var db = new DatabaseContext(options))
            {
                var controller = new DomainController(db, _lookupClient);

                //act
                var response = await controller.Get("test.com") as OkObjectResult;
                var obj = response.Value as DomainModel;
                Assert.AreEqual(obj.Id, domain.Id);
                Assert.AreEqual(obj.Ip, domain.Ip);
                Assert.AreEqual(obj.Name, domain.Name);
            }
        }

        [TestMethod]
        public async Task Domain_Not_In_Database()
        {
            //arrange 
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: "Find_searches_url")
                .Options;

            // Use a clean instance of the context to run the test
            using (var db = new DatabaseContext(options))
            {
                var controller = new DomainController(db, _lookupClient);

                //act
                var response = await controller.Get("test.com") as OkObjectResult;
                var obj = response.Value as DomainModel;
                Assert.IsNotNull(obj);
            }
        }

        [TestMethod]
        public async Task Domain_Moking_LookupClient()
        {
            //arrange 
            var mock = new Mock<ILookupClient>();
            var domainName = "test.com";
            LookupClient lookupClient = new LookupClient();

            var responseDns = new Mock<IDnsQueryResponse>();
            mock.Setup(l => l.QueryAsync(domainName, QueryType.ANY)).Returns(Task.FromResult(responseDns.Object));

            //arrange 
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: "Find_searches_url")
                .Options;

            // Use a clean instance of the context to run the test
            using (var db = new DatabaseContext(options))
            {
                //inject lookupClient in controller constructor
                var controller = new DomainController(db, _lookupClient);

                //act
                var response = await controller.Get("test.com") as OkObjectResult;
                var obj = response.Value as DomainModel;
                Assert.IsNotNull(obj);
            }
        }

        [TestMethod]
        public void Domain_Moking_WhoisClient()
        {
            //arrange
            //whois is a static class, we need to create a class to "wrapper" in a mockable version of WhoisClient
            //var whoisClient = new Mock<IWhoisClient>();
            //var domainName = "test.com";

            //whoisClient.Setup(l => l.QueryAsync(domainName)).Return();

            ////arrange 
            //var options = new DbContextOptionsBuilder<DatabaseContext>()
            //    .UseInMemoryDatabase(databaseName: "Find_searches_url")
            //    .Options;

            //// Use a clean instance of the context to run the test
            //using (var db = new DatabaseContext(options))
            //{
            //    //inject IWhoisClient in controller's constructor
            //    var controller = new DomainController(db/*,IWhoisClient, ILookupClient*/);

            //    //act
            //    var response = controller.Get("test.com");
            //    var result = response.Result as OkObjectResult;
            //    var obj = result.Value as Domain;
            //    Assert.IsNotNull(obj);
            //}
        }
    }
}