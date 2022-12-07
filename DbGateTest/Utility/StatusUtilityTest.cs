using System.Collections.Generic;
using DbGate.Utility.Support;
using Xunit;

namespace DbGate.Utility
{
    [Collection("Sequential")]
    public class StatusUtilityTests
    {
        [Fact]
        public void StatusUtility_SetStatus_WithMultiLevelHirachy_ShouldSetStatus()
        {
            var rootEntity = new RootEntity();
            var leafEntityA = new LeafEntity();
            rootEntity.LeafEntities.Add(leafEntityA);
            var leafEntityB = new LeafEntity();
            rootEntity.LeafEntities.Add(leafEntityB);
            var leafEntityNotNull = new LeafEntity();
            rootEntity.LeafEntityNotNull = leafEntityNotNull;
            rootEntity.LeafEntityNull = null;

            StatusManager.SetStatus(rootEntity, EntityStatus.Modified);

            Assert.Equal(EntityStatus.Modified, rootEntity.Status);
            Assert.Equal(EntityStatus.Modified, leafEntityA.Status);
            Assert.Equal(EntityStatus.Modified, leafEntityB.Status);
            Assert.Equal(EntityStatus.Modified, leafEntityNotNull.Status);
        }

        [Fact]
        public void StatusUtility_IsModified_WithMultiLevelHirachy_ShouldGetStatus()
        {
            var rootEntity = new RootEntity();
            var leafEntityA = new LeafEntity();
            rootEntity.LeafEntities.Add(leafEntityA);
            var leafEntityB = new LeafEntity();
            rootEntity.LeafEntities.Add(leafEntityB);
            var leafEntityNotNull = new LeafEntity();
            rootEntity.LeafEntityNotNull = leafEntityNotNull;
            rootEntity.LeafEntityNull = null;

            var unModifiedRoot = StatusManager.IsModified(rootEntity);

            rootEntity.Status = EntityStatus.Modified;
            var modifiedRoot = StatusManager.IsModified(rootEntity);

            rootEntity.Status = EntityStatus.Unmodified;
            leafEntityA.Status = EntityStatus.New;
            var modifiedLeafCollection = StatusManager.IsModified(rootEntity);

            leafEntityA.Status = EntityStatus.Unmodified;
            leafEntityNotNull.Status = EntityStatus.Deleted;
            var modifiedLeafSubEntity = StatusManager.IsModified(rootEntity);

            Assert.False(unModifiedRoot);
            Assert.True(modifiedRoot);
            Assert.True(modifiedLeafCollection);
            Assert.True(modifiedLeafSubEntity);
        }

        [Fact]
        public void StatusUtility_GetImmidiateChildrenAndClear_WithMultiLevelHirachy_ShouldGetChildren()
        {
            var rootEntity = new RootEntity();
            var leafEntityA = new LeafEntity();
            rootEntity.LeafEntities.Add(leafEntityA);
            var leafEntityB = new LeafEntity();
            rootEntity.LeafEntities.Add(leafEntityB);
            var leafEntityNotNull = new LeafEntity();
            rootEntity.LeafEntityNotNull = leafEntityNotNull;
            rootEntity.LeafEntityNull = null;

            var childern = StatusManager.GetImmidiateChildrenAndClear(rootEntity);

            Assert.True(childern.Contains(leafEntityA));
            Assert.True(childern.Contains(leafEntityB));
            Assert.True(childern.Contains(leafEntityNotNull));
            Assert.True(rootEntity.LeafEntities.Count == 0);
            Assert.Null(rootEntity.LeafEntityNotNull);
        }
    }
}