using System.Collections.Generic;

namespace TM.Data.Update
{
   public class ChangesResult<TDbEntity, TParseModelEntity>
   {
      public ChangesResult(
         ICollection<EntityPair<TDbEntity, TParseModelEntity>> unmodifiedEntities,
         ICollection<EntityPair<TDbEntity, TParseModelEntity>> modifiedEntities,
         ICollection<EntityPair<TDbEntity, TParseModelEntity>> newEntities,
         ICollection<TDbEntity> deletedEntities)
      {
         UnmodifiedEntities = unmodifiedEntities;
         ModifiedEntities = modifiedEntities;
         NewEntities = newEntities;
         DeletedEntities = deletedEntities;
      }

      public ICollection<EntityPair<TDbEntity, TParseModelEntity>> UnmodifiedEntities { get; private set; }

      public ICollection<EntityPair<TDbEntity, TParseModelEntity>> ModifiedEntities { get; private set; }

      public ICollection<EntityPair<TDbEntity, TParseModelEntity>> NewEntities { get; private set; }

      public ICollection<TDbEntity> DeletedEntities { get; private set; }
   }
}