using System.Collections.Generic;
using NUnit.Framework;
using dbgate.utility.support;

namespace dbgate.utility
{
    public class StatusUtilityTests
    {
        [Test]
        public void StatusUtility_SetStatus_WithMultiLevelHirachy_ShouldSetStatus()
        {
            RootEntity rootEntity = new RootEntity();
            LeafEntity leafEntityA = new LeafEntity();
            rootEntity.LeafEntities.Add(leafEntityA);
            LeafEntity leafEntityB = new LeafEntity();
            rootEntity.LeafEntities.Add(leafEntityB);
            LeafEntity leafEntityNotNull = new LeafEntity();
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
            RootEntity rootEntity = new RootEntity();
            LeafEntity leafEntityA = new LeafEntity();
            rootEntity.LeafEntities.Add(leafEntityA);
            LeafEntity leafEntityB = new LeafEntity();
            rootEntity.LeafEntities.Add(leafEntityB);
            LeafEntity leafEntityNotNull = new LeafEntity();
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
            RootEntity rootEntity = new RootEntity();
            LeafEntity leafEntityA = new LeafEntity();
            rootEntity.LeafEntities.Add(leafEntityA);
            LeafEntity leafEntityB = new LeafEntity();
            rootEntity.LeafEntities.Add(leafEntityB);
            LeafEntity leafEntityNotNull = new LeafEntity();
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
