using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using TestsHelpers;
using TM.Shared;
using Xunit;

namespace TM.Data.Update.UnitTests
{
   [SuppressMessage("ReSharper", "ExceptionNotDocumented")]
   [SuppressMessage("ReSharper", "PossibleUnintendedReferenceComparison")]
   public class UpdateServiceUnitTests
   {

      private const int TrainingProviderId = 1;
      private const string ArchiveDirectoryPath = @"e:\Projects\TrainingManager\archive\";

      public UpdateServiceUnitTests()
      {
         Database.SetInitializer<UpdateDbContext>(null);
      }

      [SuppressMessage("ReSharper", "UnusedMember.Global")]
      public static IEnumerable<object> CanProcessUpdateTestData
      {
         get
         {
            var currentDateTime = DateTime.Parse("2000-01-01T12:00:00", DateTimeFormatInfo.InvariantInfo);
            var allowedUpdateHours =new List<int>
            {
               currentDateTime.Hour
            };

            var disallowedUpdateHours = new List<int>
            {
               currentDateTime.Hour + 1
            };

            var updateFrequency = TimeSpan.FromHours(24.0);

            // decision on allowed update hours
            yield return new object[]
            {
               currentDateTime,
               allowedUpdateHours,
               null,
               updateFrequency,
               true
            };

            yield return new object[]
            {
               currentDateTime,
               disallowedUpdateHours,
               null,
               updateFrequency,
               false
            };


            // decision on UpdateResult == null
            yield return new object[]
            {
               currentDateTime,
               allowedUpdateHours,
               null,
               updateFrequency,
               true
            };


            // decision on UpdateResult
            yield return new object[]
            {
               currentDateTime,
               allowedUpdateHours,
               new UpdateService.UpdateEventInfo
               {
                  StartedOn = currentDateTime - updateFrequency - TimeSpan.FromSeconds(1.0),
                  UpdateResult = UpdateResult.NeedManualResolve
               },
               updateFrequency,
               false
            };

            yield return new object[]
            {
               currentDateTime,
               allowedUpdateHours,
               new UpdateService.UpdateEventInfo
               {
                  StartedOn = currentDateTime - updateFrequency - TimeSpan.FromSeconds(1.0),
                  UpdateResult = UpdateResult.Success
               },
               updateFrequency,
               true
            };

            yield return new object[]
            {
               currentDateTime,
               allowedUpdateHours,
               new UpdateService.UpdateEventInfo
               {
                  StartedOn = currentDateTime - updateFrequency - TimeSpan.FromSeconds(1.0),
                  UpdateResult = UpdateResult.Error
               },
               updateFrequency,
               true
            };

            yield return new object[]
            {
               currentDateTime,
               allowedUpdateHours,
               new UpdateService.UpdateEventInfo
               {
                  StartedOn = currentDateTime - updateFrequency - TimeSpan.FromSeconds(1.0),
                  UpdateResult = UpdateResult.Resolved
               },
               updateFrequency,
               true
            };

            // decision on LastUpdateTime
            yield return new object[]
            {
               currentDateTime,
               allowedUpdateHours,
               new UpdateService.UpdateEventInfo
               {
                  StartedOn = currentDateTime - updateFrequency - TimeSpan.FromSeconds(1.0)
               },
               updateFrequency,
               true
            };

            yield return new object[]
            {
               currentDateTime,
               allowedUpdateHours,
               new UpdateService.UpdateEventInfo
               {
                  StartedOn = currentDateTime - updateFrequency 
               },
               updateFrequency,
               true
            };

            yield return new object[]
            {
               currentDateTime,
               allowedUpdateHours,
               new UpdateService.UpdateEventInfo
               {
                  StartedOn = currentDateTime - updateFrequency + TimeSpan.FromSeconds(1.0)
               },
               updateFrequency,
               false
            };

           
         }
      }

      [Theory]
      [MemberData("CanProcessUpdateTestData")]
      internal void Should_CanProcessUpdate(DateTime currentDateTime, List<int> allowedUpdateUtcHours,
         UpdateService.UpdateEventInfo updateEventInfo, TimeSpan updateFrequency, bool expectedResult)
      {
         // Arrage
         var dateTimeProxy = Mock.Of<IDateTimeProxy>(x => x.UtcNow == currentDateTime);

         var sut = new UpdateService(string.Empty, ArchiveDirectoryPath, new DbContextFactory<UpdateDbContext>(), dateTimeProxy,
            ActivatorProxy.Instance);

         // Act
         var result = sut.CanProcessUpdate(updateFrequency, allowedUpdateUtcHours, updateEventInfo);

         // Assert
         Assert.Equal(expectedResult, result);
      }


      [Fact]
      public async Task Should_ReturnLastUpdateEventInfo()
      {
         // Arrange
         var lastUpdateDateTime = DateTime.Parse("2000-01-01T12:00:00", DateTimeFormatInfo.InvariantInfo);

         var updateEvents = new List<UpdateEvent>
         {
            new UpdateEvent
            {
               TrainingProviderId = TrainingProviderId,
               StartedOn = lastUpdateDateTime - TimeSpan.FromHours(1.0)
            },
            new UpdateEvent
            {
               TrainingProviderId = TrainingProviderId,
               StartedOn = lastUpdateDateTime
            },
            new UpdateEvent
            {
               TrainingProviderId = TrainingProviderId,
               StartedOn = lastUpdateDateTime - TimeSpan.FromHours(2.0)
            }
         }.AsQueryable();

         var mockUpdateEventSet = MockDbSetFactory.CreateAsyncDbSetMock(updateEvents);

         var mockContext = Mock.Of<UpdateDbContext>(x => x.UpdateEvents == mockUpdateEventSet.Object);

         var sut = new UpdateService(string.Empty, ArchiveDirectoryPath, new DbContextFactory<UpdateDbContext>());

         // Act
         var resultLastUpdateEventInfo = await sut.GetLastUpdateEventInfoAsync(TrainingProviderId, mockContext);

         // Assert
         Assert.NotNull(resultLastUpdateEventInfo);
         Assert.Equal(lastUpdateDateTime, resultLastUpdateEventInfo.StartedOn);
      }

      [Fact]
      public async Task Should_UpdateTrainingCatalog()
      {
         // Arrange
         var trainingProvider = new TrainingProvider();
         var description = "test update";
         var mediaPathsContainer = new UpdateService.ServerMediaPathsContainer();

         var updateDbContext = Mock.Of<UpdateDbContext>(x => x.UpdateEvents.Add(It.IsAny<UpdateEvent>()) == new UpdateEvent() &&
            x.AuthorsResolves.Local == new ObservableCollection<AuthorResolve>());
         var mockContext = Mock.Get(updateDbContext);

         var mockTrainingCatalog = new Mock<ITrainingCatalog>();
         var mockActivator = new Mock<IActivatorProxy>();

         mockActivator.Setup(x => x.CreateInstance<ITrainingCatalog>(It.IsAny<string>()))
            .Returns(mockTrainingCatalog.Object);

         var sut = new UpdateService(string.Empty, ArchiveDirectoryPath, new DbContextFactory<UpdateDbContext>(), DateTimeProxy.Instance, mockActivator.Object);

         // Act
         await sut.UpdateTrainingCatalogAsync(mockContext.Object, trainingProvider, description, mediaPathsContainer);

         // Assert
         mockContext.Verify(x => x.UpdateEvents.Add(It.IsAny<UpdateEvent>()), Times.Once);
         mockContext.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));

         mockActivator.Verify(x => x.CreateInstance<ITrainingCatalog>(It.IsAny<string>()), Times.Once);

         mockTrainingCatalog.Verify(x => x.Initialize(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<LocationType>(),It.IsAny<IMediaPath>(), It.IsAny<string>()), Times.Once());

         mockTrainingCatalog.Verify(x => x.UpdateAsync(It.IsAny<UpdateEvent>(), It.IsAny<UpdateDbContext>(),
            It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);

         mockTrainingCatalog.Verify(x => x.Dispose(), Times.Once);
      }

      [Fact]
      public async Task Should_LogErrorToDb_When_UpdateTrainingCatalogThrowAggregateException()
      {
         // Arrange
         var trainingProvider = new TrainingProvider();
         var description = "test update";
         var mediaPathsContainer = new UpdateService.ServerMediaPathsContainer();

         var updateDbContext = Mock.Of<UpdateDbContext>(x => x.UpdateEvents.Add(It.IsAny<UpdateEvent>()) == new UpdateEvent());
         Mock.Get(updateDbContext).Setup(x => x.SetStateToDetached(It.IsAny<UpdateEvent>()));

         var mockContextFromFactory = new Mock<UpdateDbContext>();
         mockContextFromFactory.Setup(x => x.SetStateToModified(It.IsAny<UpdateEvent>()));

         var dbContextFactory = Mock.Of<IDbContextFactory<UpdateDbContext>>(x => x.CreateDbContext() == mockContextFromFactory.Object);
         

         var mockTrainingCatalog = new Mock<ITrainingCatalog>();
         mockTrainingCatalog.Setup(x => x.UpdateAsync(It.IsAny<UpdateEvent>(), It.IsAny<UpdateDbContext>(),
            It.IsAny<bool>(), It.IsAny<bool>())).Throws<AggregateException>();

         var mockActivator = new Mock<IActivatorProxy>();
         mockActivator.Setup(x => x.CreateInstance<ITrainingCatalog>(It.IsAny<string>()))
            .Returns(mockTrainingCatalog.Object);

         var sut = new UpdateService(string.Empty, ArchiveDirectoryPath, dbContextFactory, DateTimeProxy.Instance, mockActivator.Object);

         // Act
         await sut.UpdateTrainingCatalogAsync(updateDbContext, trainingProvider, description, mediaPathsContainer);

         // Assert
         Mock.Get(updateDbContext).Verify(x => x.SaveChangesAsync(), Times.Once);
         
         mockTrainingCatalog.Verify(x => x.UpdateAsync(It.IsAny<UpdateEvent>(), It.IsAny<UpdateDbContext>(),
            It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
         mockTrainingCatalog.Verify(x => x.Dispose(), Times.Once);

         Mock.Get(dbContextFactory).Verify(x => x.CreateDbContext(), Times.Once);

         mockContextFromFactory.Verify(x => x.SetStateToModified(It.IsAny<UpdateEvent>()), Times.Once());
         mockContextFromFactory.Verify(x => x.SaveChanges(), Times.Once);
      }
   }
}
