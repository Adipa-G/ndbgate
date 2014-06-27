using System.Collections.Generic;
using DbGate.Utility.Support;
using NUnit.Framework;

namespace DbGate.Utility
{
    public class StatusUtilityTests
    {
        [Test]
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

            Assert.AreEqual(rootEntity.Status, EntityStatus.Modified);
            Assert.AreEqual(leafEntityA.Status, EntityStatus.Modified);
            Assert.AreEqual(leafEntityB.Status, EntityStatus.Modified);
            Assert.AreEqual(leafEntityNotNull.Status, EntityStatus.Modified);
        }

        [Test]
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

            bool unModifiedRoot = StatusManager.IsModified(rootEntity);

            rootEntity.Status = EntityStatus.Modified;
            bool modifiedRoot = StatusManager.IsModified(rootEntity);

            rootEntity.Status = EntityStatus.Unmodified;
            leafEntityA.Status = EntityStatus.New;
            bool modifiedLeafCollection = StatusManager.IsModified(rootEntity);

            leafEntityA.Status = EntityStatus.Unmodified;
            leafEntityNotNull.Status = EntityStatus.Deleted;
            bool modifiedLeafSubEntity = StatusManager.IsModified(rootEntity);

            Assert.IsFalse(unModifiedRoot);
            Assert.IsTrue(modifiedRoot);
            Assert.IsTrue(modifiedLeafCollection);
            Assert.IsTrue(modifiedLeafSubEntity);
        }

        [Test]
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

            ICollection<IClientEntity> childern = StatusManager.GetImmidiateChildrenAndClear(rootEntity);

            Assert.IsTrue(childern.Contains(leafEntityA));
            Assert.IsTrue(childern.Contains(leafEntityB));
            Assert.IsTrue(childern.Contains(leafEntityNotNull));
            Assert.IsTrue(rootEntity.LeafEntities.Count == 0);
            Assert.IsNull(rootEntity.LeafEntityNotNull);
        }
    }
}