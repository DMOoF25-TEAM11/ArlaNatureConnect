using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ArlaNatureConnect.Infrastructure.Persistence;
using ArlaNatureConnect.Domain.Entities;

namespace TestInfrastructure.Persistence
{
	[TestClass]
	public class AppDbContextTest
	{
		private DbContextOptions<AppDbContext> CreateOptions()
		{
			DbContextOptionsBuilder<AppDbContext> builder = new DbContextOptionsBuilder<AppDbContext>();
			builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
			return builder.Options;
		}

		[TestMethod]
		public void OnModelCreating_Configures_AutoInclude_Person()
		{
			DbContextOptions<AppDbContext> options = CreateOptions();
			using AppDbContext ctx = new(options);

			IModel model = ctx.Model;
			IEntityType? personEntity = model.FindEntityType(typeof(Person));
			Assert.IsNotNull(personEntity, "Owner entity should be in the model");

			INavigation? pRole = personEntity!.FindNavigation(nameof(Person.Role));
			INavigation? pAddress = personEntity.FindNavigation(nameof(Person.Address));
			INavigation? pFarms = personEntity.FindNavigation(nameof(Person.Farms));

			Assert.IsNotNull(pRole, "Owner.Role navigation should exist");
			Assert.IsNotNull(pAddress, "Owner.Address navigation should exist");
			Assert.IsNotNull(pFarms, "Owner.Farms navigation should exist");

			// AutoInclude sets navigation to eager-loaded in the EF model
			Assert.IsTrue(pRole!.IsEagerLoaded, "Owner.Role should be configured for AutoInclude");
			Assert.IsTrue(pAddress!.IsEagerLoaded, "Owner.Address should be configured for AutoInclude");
			Assert.IsTrue(pFarms!.IsEagerLoaded, "Owner.Farms should be configured for AutoInclude");
		}

		[TestMethod]
		public void OnModelCreating_Configures_AutoInclude_Farm()
		{
			DbContextOptions<AppDbContext> options = CreateOptions();
			using AppDbContext ctx = new(options);

			IModel model = ctx.Model;
			IEntityType? farmEntity = model.FindEntityType(typeof(Farm));
			Assert.IsNotNull(farmEntity, "Farm entity should be in the model");

			INavigation? fAddress = farmEntity!.FindNavigation(nameof(Farm.Address));
			INavigation? fPerson = farmEntity.FindNavigation(nameof(Farm.Owner));

			Assert.IsNotNull(fAddress, "Farm.Address navigation should exist");
			Assert.IsNotNull(fPerson, "Farm.Owner navigation should exist");

			// Accept Farm navigations being AutoIncluded
			Assert.IsTrue(fAddress!.IsEagerLoaded, "Farm.Address should be configured for AutoInclude");
			Assert.IsTrue(fPerson!.IsEagerLoaded, "Farm.Owner should be configured for AutoInclude");
		}

		[TestMethod]
		public void OnModelCreating_Configures_AutoInclude_NatureArea()
		{
			DbContextOptions<AppDbContext> options = CreateOptions();
			using AppDbContext ctx = new(options);

			IModel model = ctx.Model;
			IEntityType? natureAreaEntity = model.FindEntityType(typeof(NatureArea));
			Assert.IsNotNull(natureAreaEntity, "NatureArea entity should be in the model");

			INavigation? nCoordinates = natureAreaEntity!.FindNavigation(nameof(NatureArea.Coordinates));
			INavigation? nImages = natureAreaEntity.FindNavigation(nameof(NatureArea.Images));

			Assert.IsNotNull(nCoordinates, "NatureArea.NatureAreaCoordinate navigation should exist");
			Assert.IsNotNull(nImages, "NatureArea.Images navigation should exist");

			Assert.IsTrue(nCoordinates!.IsEagerLoaded, "NatureArea.NatureAreaCoordinate should be configured for AutoInclude");
			Assert.IsTrue(nImages!.IsEagerLoaded, "NatureArea.Images should be configured for AutoInclude");
		}
	}
}
