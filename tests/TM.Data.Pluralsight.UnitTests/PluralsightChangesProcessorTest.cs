using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using TM.Data.Update;
using TM.Shared;
using TM.Shared.Parse;
using Xunit;

namespace TM.Data.Pluralsight.UnitTests
{
   public class PluralsightChangesProcessorTest
   {

      public IChangesDetector<TrainingProviderAuthor, PluralsightAuthor> AuthorsChangesDetector
      {
         get
         {
            var authorsChangesDetector = new PluralsightAuthorBackupCreator();
            return authorsChangesDetector;
         }
      }

      private const int TrainingProviderId = 100;

      [Fact]
      public async Task Should_ReturnAuthorsChanges()
      {
         // Arrange
         var existingAuthors = new List<TrainingProviderAuthor>
         {
            new TrainingProviderAuthor
            {
               TrainingProviderId = TrainingProviderId,
               AuthorId = 1,
               FullName = "n s1",
               SiteUrl = "http://example.com/n-s1",
               UrlName = "n-s1"
            },
            new TrainingProviderAuthor
            {
               TrainingProviderId = TrainingProviderId,
               AuthorId = 2,
               FullName = "n s2",
               SiteUrl = "http://example.com/n-s2",
               UrlName = "n-s2"
            },
            new TrainingProviderAuthor
            {
               TrainingProviderId = TrainingProviderId,
               AuthorId = 3,
               FullName = "n s3",
               SiteUrl = "http://example.com/n-s3",
               UrlName = "n-s3"
            }
         };

         var processingAuthorsDictionary = new List<PluralsightAuthor>
         {
            new PluralsightAuthor
            {
               FullName = "n s1",
               SiteUrl = "http://example.com/n-s1",
               UrlName = "n-s1"
            },
            new PluralsightAuthor
            {
               FullName = "n s2",
               SiteUrl = "http://example.com/n-s2modified",
               UrlName = "n-s2"
            },
            new PluralsightAuthor
            {
               FullName = "n s4",
               SiteUrl = "http://example.com/n-s4",
               UrlName = "n-s4"
            }
         }.ToDictionary(x => x, x => x, UrlNameNaturalKeyEqualityComparer<IAuthorUrlNameNaturalKey>.Instance);


         var parseResultMock = new Mock<IUpdateContentParseResult<PluralsightCategory, PluralsightCourse, PluralsightAuthor>>();

         parseResultMock.SetupGet(x => x.AuthorsParseResult.AuthorsExceptWhoseUrlNullContainer)
           .Returns(processingAuthorsDictionary);

         var dataServiceMock = new Mock<IPluralsightDataService>();

         var dataServiceAuthorResultFake = new Author();
         dataServiceMock.Setup(x => x.GetAuthorAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(dataServiceAuthorResultFake));


         var sut = new PluralsightChangesProcessor(parseResultMock.Object, dataServiceMock.Object);

         // Act
         var result = await sut.GetAuthorsChangesAsync(TrainingProviderId, existingAuthors, AuthorsChangesDetector);

         // Assert

         // deleted authors
         Assert.Equal(1, result.DeletedEntities.Count);
         Assert.Same(existingAuthors[2], result.DeletedEntities.Single(x => x.AuthorId == existingAuthors[2].AuthorId));

         // modified authors
         Assert.Equal(1, result.ModifiedEntities.Count);

         var existingAuthor = existingAuthors[1];
         var processingAuthor = processingAuthorsDictionary.Values.Single(x => x.UrlName == existingAuthor.UrlName);
         EntityPair<TrainingProviderAuthor, PluralsightAuthor> authorPair = result.ModifiedEntities.Single(x => x.DbEntity.AuthorId == existingAuthor.AuthorId);

         Assert.Same(existingAuthor, authorPair.DbEntity);
         Assert.Same(processingAuthor, authorPair.ParseModelEntity);
         Assert.Equal(authorPair.DbEntity.AuthorId, authorPair.ParseModelEntity.Id);

         // unmodified authors
         Assert.Equal(1, result.UnmodifiedEntities.Count);

         existingAuthor = existingAuthors[0];
         processingAuthor = processingAuthorsDictionary.Values.Single(x => x.UrlName == existingAuthor.UrlName);
         authorPair = result.UnmodifiedEntities.Single(x => x.DbEntity.AuthorId == existingAuthor.AuthorId);

         Assert.Same(existingAuthor, authorPair.DbEntity);
         Assert.Same(processingAuthor, authorPair.ParseModelEntity);
         Assert.Equal(authorPair.DbEntity.AuthorId, authorPair.ParseModelEntity.Id);

         // new authors
         Assert.Equal(1, result.NewEntities.Count);

         processingAuthor = processingAuthorsDictionary.Values.ToList()[2];
         authorPair = result.NewEntities.Single(x => x.ParseModelEntity.UrlName == processingAuthor.UrlName);

         Assert.Same(processingAuthor, authorPair.ParseModelEntity);
         Assert.Same(authorPair.DbEntity.Author, dataServiceAuthorResultFake);

         CheckFieldsEquality(authorPair);
      }


      private void CheckFieldsEquality(EntityPair<TrainingProviderAuthor, PluralsightAuthor> authorPair)
      {
         Assert.Equal(authorPair.ParseModelEntity.Id, authorPair.DbEntity.AuthorId);
         Assert.Equal(authorPair.ParseModelEntity.FullName, authorPair.DbEntity.FullName);
         Assert.Equal(authorPair.ParseModelEntity.SiteUrl, authorPair.DbEntity.SiteUrl);
         Assert.Equal(authorPair.ParseModelEntity.UrlName, authorPair.DbEntity.UrlName);
      }
   }
}
