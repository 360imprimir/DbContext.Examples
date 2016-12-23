using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectContextExample;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;

namespace Context
{
    [TestClass]
    public class ObjectContextTests
    {
        #region Initialize
        [TestInitialize]
        public void TestInitialize()
        {
            Utils.CleanDB();
        }
        #endregion

        #region Tests
        [TestMethod]
        public void TestAddEntity()
        {
            using (var context = new ObjectContextExampleEntities())
            {
                // Arrange
                var entity = new Entity();
                context.Entities.AddObject(entity);
                context.SaveChanges();

                // Act
                var realEntity = context.Entities.FirstOrDefault();

                // Assert
                Assert.IsNotNull(realEntity);
            }
        }

        [TestMethod]
        public void TestDeleteEntity()
        {
            using (var context = new ObjectContextExampleEntities())
            {
                // Arrange
                var entity = new Entity();
                context.Entities.AddObject(entity);
                context.SaveChanges();

                // Act
                context.Entities.DeleteObject(entity);
                context.SaveChanges();

                // Assert
                Assert.IsNull(context.Entities.FirstOrDefault());
            }
        }

        [TestMethod]
        public void TestReloadEntity()
        {
            using (var context = new ObjectContextExampleEntities())
            {
                // Arrange
                var entity = new Entity();
                context.Entities.AddObject(entity);
                context.SaveChanges();

                using (var context2 = new ObjectContextExampleEntities())
                {
                    var entity2 = context2.Entities.First();
                    entity2.Description = "Reloaded entity";
                    context2.SaveChanges();
                }

                // Act
                context.Refresh(RefreshMode.StoreWins, entity);

                // Assert
                Assert.AreEqual("Reloaded entity", entity.Description);
            }
        }

        [TestMethod]
        public void TestDetachEntity()
        {
            using (var context = new ObjectContextExampleEntities())
            {
                // Arrange
                var entity = new Entity();
                context.Entities.AddObject(entity);
                context.SaveChanges();

                // Act
                context.Entities.Detach(entity);
                entity.Description = "Detached entity";
                context.SaveChanges();

                // Assert
                Assert.AreEqual(null, context.Entities.First().Description);
            }
        }

        [TestMethod]
        public void TestIsAttachedEntity()
        {
            using (var context = new ObjectContextExampleEntities())
            {
                // Arrange
                var entity = new Entity();
                context.Entities.AddObject(entity);
                context.SaveChanges();
                int id = entity.EntityID;

                // Act
                bool isAttached = context.ObjectStateManager.GetObjectStateEntries(~EntityState.Detached)
                          .Where(e => !e.IsRelationship)
                          .Select(e => e.Entity)
                          .OfType<Entity>()
                          .Any(x => x.EntityID == id);

                // Assert
                Assert.IsTrue(isAttached);
            }
        }

        [TestMethod]
        public void TestIsNotAttachedEntity()
        {
            using (var context = new ObjectContextExampleEntities())
            {
                // Arrange
                var entity = new Entity();
                entity.EntityID = 1;

                // Act
                bool isAttached = context.ObjectStateManager.GetObjectStateEntries(~EntityState.Detached)
                          .Where(e => !e.IsRelationship)
                          .Select(e => e.Entity)
                          .OfType<Entity>()
                          .Any(x => x.EntityID == 1);

                // Assert
                Assert.IsFalse(isAttached);
            }
        }

        [TestMethod]
        public void TestModifiedEntity()
        {
            using (var context = new ObjectContextExampleEntities())
            {
                // Arrange
                var entity = new Entity();
                entity.Description = "Not modified";
                context.Entities.AddObject(entity);
                context.SaveChanges();

                context.Detach(entity);

                // Act
                entity.Description = "Modified";
                context.Attach(entity);
                context.ObjectStateManager.ChangeObjectState(entity, EntityState.Modified);
                
                context.SaveChanges();
            }

            using (var context = new ObjectContextExampleEntities())
            {
                // Assert
                Assert.AreEqual("Modified", context.Entities.First().Description);
            }
        }

        [TestMethod]
        public void TestModifiedProperty()
        {
            using (var context = new ObjectContextExampleEntities())
            {
                // Arrange
                var entity = new Entity();
                entity.Description = "Not modified";
                context.Entities.AddObject(entity);
                context.SaveChanges();

                context.Detach(entity);

                // Act
                entity.Description = "Modified";
                context.Entities.Attach(entity);
                context.ObjectStateManager.GetObjectStateEntry(entity).SetModifiedProperty("Description");

                context.SaveChanges();
            }

            using (var context = new ObjectContextExampleEntities())
            {
                // Assert
                Assert.AreEqual("Modified", context.Entities.First().Description);
            }
        }

        [TestMethod]
        public void TestNotModifiedProperty()
        {
            using (var context = new ObjectContextExampleEntities())
            {
                // Arrange
                var entity = new Entity();
                entity.Description = "Not modified";
                context.Entities.AddObject(entity);
                context.SaveChanges();

                // Act
                entity.Description = "Modified";
                context.ObjectStateManager.GetObjectStateEntry(entity).RejectPropertyChanges("Description");

                context.SaveChanges();
            }

            using (var context = new ObjectContextExampleEntities())
            {
                // Assert
                Assert.AreEqual("Not modified", context.Entities.First().Description);
            }
        }

        [TestMethod]
        public void TestOnlyAddParentToContext()
        {
            using (var context = new ObjectContextExampleEntities())
            {
                // Arrange
                var entity = new Entity
                {
                    ChildEntity = new ChildEntity()
                };
                context.Entities.AddObject(entity);
                context.SaveChanges();

                // Act
                bool entityExists = context.Entities.FirstOrDefault() != null;
                bool childEntityExists = context.ChildEntities.FirstOrDefault() != null;

                context.SaveChanges();

                // Assert
                Assert.IsTrue(entityExists);
                Assert.IsTrue(childEntityExists);
            }
        }

        [TestMethod]
        public void TestOnlyAddChildToContextSettingRelationInParent()
        {
            using (var context = new ObjectContextExampleEntities())
            {
                // Arrange
                var childEntity = new ChildEntity();
                var entity = new Entity
                {
                    ChildEntity = childEntity
                };
                context.ChildEntities.AddObject(childEntity);
                context.SaveChanges();

                // Act
                bool entityExists = context.Entities.FirstOrDefault() != null;
                bool childEntityExists = context.ChildEntities.FirstOrDefault() != null;

                // Assert
                Assert.IsTrue(entityExists);
                Assert.IsTrue(childEntityExists);
            }
        }

        [TestMethod]
        public void TestOnlyAddChildToContextSettingRelationInChild()
        {
            using (var context = new ObjectContextExampleEntities())
            {
                // Arrange
                var childEntity = new ChildEntity();
                childEntity.Entities.Add(new Entity());
                context.ChildEntities.AddObject(childEntity);
                context.SaveChanges();

                // Act
                bool entityExists = context.Entities.FirstOrDefault() != null;
                bool childEntityExists = context.ChildEntities.FirstOrDefault() != null;

                // Assert
                Assert.IsTrue(entityExists);
                Assert.IsTrue(childEntityExists);
            }
        }

        /*[TestMethod]
        public void TestNavigationPropertyWhenEntityDoesNotExists()
        {
            using (var context = new ObjectContextExampleEntities())
            {
                // Arrange
                var childEntity = new ChildEntity();
                context.ChildEntities.AddObject(childEntity);
                context.SaveChanges();

                var entity = new Entity();
                entity.ChildEntityID = childEntity.ChildEntityID;
                context.Entities.AddObject(entity);

                // Act
                var navigationProperty = entity.ChildEntity;

                // Assert
                Assert.IsNotNull(navigationProperty);
            }
        }

        [TestMethod]
        public void TestNavigationPropertySettingIdWithSaveChanges()
        {
            using (var context = new ObjectContextExampleEntities())
            {
                // Arrange
                var childEntity = new ChildEntity();
                context.ChildEntities.AddObject(childEntity);
                context.SaveChanges();

                var entity = new Entity();
                entity.ChildEntityID = childEntity.ChildEntityID;
                context.Entities.AddObject(entity);
                context.SaveChanges();

                // Act
                var navigationProperty = entity.ChildEntity;

                // Assert
                Assert.IsNotNull(navigationProperty);
            }
        }

        [TestMethod]
        public void TestIdSettingNavigationPropertyWithoutSaveChanges()
        {
            using (var context = new ObjectContextExampleEntities())
            {
                // Arrange
                var childEntity = new ChildEntity();
                context.ChildEntities.AddObject(childEntity);
                context.SaveChanges();

                var entity = new Entity();
                entity.ChildEntity = childEntity;

                // Act
                bool hasId = entity.ChildEntityID.HasValue;

                // Assert
                Assert.IsTrue(hasId);
            }
        }

        [TestMethod]
        public void TestIdSettingNavigationPropertyWithSaveChanges()
        {
            using (var context = new ObjectContextExampleEntities())
            {
                // Arrange
                var childEntity = new ChildEntity();
                context.ChildEntities.AddObject(childEntity);
                context.SaveChanges();

                var entity = new Entity();
                entity.ChildEntity = childEntity;
                context.SaveChanges();

                // Act
                bool hasId = entity.ChildEntityID.HasValue;

                // Assert
                Assert.IsTrue(hasId);
            }
        }
        */
        #endregion
    }
}
