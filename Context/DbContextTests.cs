using DbContextExample;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Entity;
using System.Linq;

namespace Context
{
    [TestClass]
    public class DbContextTests
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
            using(var context = new DbContextExampleEntities())
            {
                // Arrange
                var entity = new Entity();
                context.Entities.Add(entity);
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
            using (var context = new DbContextExampleEntities())
            {
                // Arrange
                var entity = new Entity();
                context.Entities.Add(entity);
                context.SaveChanges();

                // Act
                context.Entities.Remove(entity);
                context.SaveChanges();

                // Assert
                Assert.IsNull(context.Entities.FirstOrDefault());
            }
        }

        [TestMethod]
        public void TestReloadEntity()
        {
            using (var context = new DbContextExampleEntities())
            {
                // Arrange
                var entity = new Entity();
                context.Entities.Add(entity);
                context.SaveChanges();

                using (var context2 = new DbContextExampleEntities())
                {
                    var entity2 = context2.Entities.First();
                    entity2.Description = "Reloaded entity";
                    context2.SaveChanges();
                }

                // Act
                context.Entry(entity).Reload();

                // Assert
                Assert.AreEqual("Reloaded entity", entity.Description);
            }
        }

        [TestMethod]
        public void TestDetachEntity()
        {
            using (var context = new DbContextExampleEntities())
            {
                // Arrange
                var entity = new Entity();
                context.Entities.Add(entity);
                context.SaveChanges();

                // Act
                context.Entry(entity).State = EntityState.Detached;
                entity.Description = "Detached entity";
                context.SaveChanges();

                // Assert
                Assert.AreEqual(null, context.Entities.First().Description);
            }
        }

        [TestMethod]
        public void TestIsAttachedEntity()
        {
            using (var context = new DbContextExampleEntities())
            {
                // Arrange
                var entity = new Entity();
                context.Entities.Add(entity);
                context.SaveChanges();
                int id = entity.EntityID;

                // Act
                bool isAttached = context.Entities.Local.Any(x => x.EntityID == id);

                // Assert
                Assert.IsTrue(isAttached);
            }
        }

        [TestMethod]
        public void TestIsNotAttachedEntity()
        {
            using (var context = new DbContextExampleEntities())
            {
                // Arrange
                var entity = new Entity();
                entity.EntityID = 1;

                // Act
                bool isAttached = context.Entities.Local.Any(x => x.EntityID == 1);

                // Assert
                Assert.IsFalse(isAttached);
            }
        }

        [TestMethod]
        public void TestModifiedEntity()
        {
            using (var context = new DbContextExampleEntities())
            {
                // Arrange
                var entity = new Entity();
                entity.Description = "Not modified";
                context.Entities.Add(entity);
                context.SaveChanges();

                context.Entry(entity).State = EntityState.Detached;

                // Act
                entity.Description = "Modified";
                context.Entities.Attach(entity);
                context.Entry(entity).State = EntityState.Modified;

                context.SaveChanges();
            }

            using (var context = new DbContextExampleEntities())
            {
                // Assert
                Assert.AreEqual("Modified", context.Entities.First().Description);
            }
        }

        [TestMethod]
        public void TestModifiedProperty()
        {
            using (var context = new DbContextExampleEntities())
            {
                // Arrange
                var entity = new Entity();
                entity.Description = "Not modified";
                context.Entities.Add(entity);
                context.SaveChanges();

                context.Entry(entity).State = EntityState.Detached;

                // Act
                entity.Description = "Modified";
                context.Entities.Attach(entity);
                context.Entry(entity).Property(a => a.Description).IsModified = true;

                context.SaveChanges();
            }

            using (var context = new DbContextExampleEntities())
            {
                // Assert
                Assert.AreEqual("Modified", context.Entities.First().Description);
            }
        }

        [TestMethod]
        public void TestNotModifiedProperty()
        {
            using (var context = new DbContextExampleEntities())
            {
                // Arrange
                var entity = new Entity();
                entity.Description = "Not modified";
                context.Entities.Add(entity);
                context.SaveChanges();

                // Act
                entity.Description = "Modified";
                context.Entry(entity).Property(a => a.Description).IsModified = false;

                context.SaveChanges();
            }

            using (var context = new DbContextExampleEntities())
            {
                // Assert
                Assert.AreEqual("Not modified", context.Entities.First().Description);
            }
        }

        [TestMethod]
        public void TestOnlyAddParentToContext()
        {
            using (var context = new DbContextExampleEntities())
            {
                // Arrange
                var entity = new Entity
                {
                    ChildEntity = new ChildEntity()
                };
                context.Entities.Add(entity);
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
            using (var context = new DbContextExampleEntities())
            {
                // Arrange
                var childEntity = new ChildEntity();
                var entity = new Entity
                {
                    ChildEntity = childEntity
                };
                context.ChildEntities.Add(childEntity);
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
            using (var context = new DbContextExampleEntities())
            {
                // Arrange
                var childEntity = new ChildEntity();
                childEntity.Entities.Add(new Entity());
                context.ChildEntities.Add(childEntity);
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
        public void TestNavigationPropertyWhenEntityNotKnown()
        {
            ChildEntity childEntity;
            using (var context = new DbContextExampleEntities())
            {
                // Arrange
                childEntity = new ChildEntity();
                context.ChildEntities.Add(childEntity);
                context.SaveChanges();
                context.Entry(childEntity).State = EntityState.Detached;

                var entity = new Entity();
                entity.ChildEntityID = childEntity.ChildEntityID;
                context.Entities.Add(entity);
                context.SaveChanges();
                
                // Act
                var navigationProperty = entity.ChildEntity;

                // Assert
                Assert.IsNotNull(navigationProperty);
            }
        }

        [TestMethod]
        public void TestNavigationPropertyWhenEntityNotKnownFixed()
        {
            ChildEntity childEntity;
            using (var context = new DbContextExampleEntities())
            {
                // Arrange
                childEntity = new ChildEntity();
                context.ChildEntities.Add(childEntity);
                context.SaveChanges();
                context.Entry(childEntity).State = EntityState.Detached;

                var entity = context.Entities.Create();
                entity.ChildEntityID = childEntity.ChildEntityID;
                context.Entities.Add(entity);
                context.SaveChanges();

                // Act
                var navigationProperty = entity.ChildEntity;

                // Assert
                Assert.IsNotNull(navigationProperty);
            }
        }
        #endregion
    }
}
