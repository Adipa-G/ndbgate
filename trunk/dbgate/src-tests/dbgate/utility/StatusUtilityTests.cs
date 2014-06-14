﻿using System.Collections.Generic;
using dbgate.utility.support;
using NUnit.Framework;

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

            StatusManager.SetStatus(rootEntity, DbClassStatus.Modified);

            Assert.AreEqual(rootEntity.Status, DbClassStatus.Modified);
            Assert.AreEqual(leafEntityA.Status, DbClassStatus.Modified);
            Assert.AreEqual(leafEntityB.Status, DbClassStatus.Modified);
            Assert.AreEqual(leafEntityNotNull.Status, DbClassStatus.Modified);
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

            rootEntity.Status = DbClassStatus.Modified;
            bool modifiedRoot = StatusManager.IsModified(rootEntity);

            rootEntity.Status = DbClassStatus.Unmodified;
            leafEntityA.Status = DbClassStatus.New;
            bool modifiedLeafCollection = StatusManager.IsModified(rootEntity);

            leafEntityA.Status = DbClassStatus.Unmodified;
            leafEntityNotNull.Status = DbClassStatus.Deleted;
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

            ICollection<IDbClass> childern = StatusManager.GetImmidiateChildrenAndClear(rootEntity);

            Assert.IsTrue(childern.Contains(leafEntityA));
            Assert.IsTrue(childern.Contains(leafEntityB));
            Assert.IsTrue(childern.Contains(leafEntityNotNull));
            Assert.IsTrue(rootEntity.LeafEntities.Count == 0);
            Assert.IsNull(rootEntity.LeafEntityNotNull);
        }
    }
}