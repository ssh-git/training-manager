namespace TM.Data.Update
{
   public struct EntityPair<TDbEntity, TParseModelEntity>
   {
      public EntityPair(TDbEntity dbEntity, TParseModelEntity parseModelEntity)
         : this()
      {
         DbEntity = dbEntity;
         ParseModelEntity = parseModelEntity;
      }

      public TDbEntity DbEntity { get; private set; }
      public TParseModelEntity ParseModelEntity { get; private set; }
   }
}