using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AiInGames.Blackboard.Tests
{
    [TestFixture]
    public class BlackboardAssetTests
    {
        private BlackboardAsset asset;

        [SetUp]
        public void SetUp()
        {
            asset = ScriptableObject.CreateInstance<BlackboardAsset>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Runtime_WhenAccessed_CreatesBlackboard()
        {
            var runtime = asset.Runtime;

            Assert.IsNotNull(runtime);
            Assert.IsInstanceOf<Blackboard>(runtime);
        }

        [Test]
        public void Runtime_WhenAccessedMultipleTimes_ReturnsSameInstance()
        {
            var runtime1 = asset.Runtime;
            var runtime2 = asset.Runtime;

            Assert.AreSame(runtime1, runtime2);
        }

        [Test]
        public void SetParent_ToSelf_PreventsCircularReference()
        {
            LogAssert.Expect(LogType.Error, "Blackboard cannot be its own parent");

            ((Blackboard)asset.Runtime).Parent = asset.Runtime;

            Assert.IsNull(asset.Runtime.Parent, "Parent should remain null after self-assignment");
        }

        [Test]
        public void SetParent_DirectCycle_PreventsCircularReference()
        {
            var parentAsset = ScriptableObject.CreateInstance<BlackboardAsset>();

            ((Blackboard)asset.Runtime).Parent = parentAsset.Runtime;

            LogAssert.Expect(LogType.Error, "Cannot set parent: would create circular reference");
            ((Blackboard)parentAsset.Runtime).Parent = asset.Runtime;

            Assert.IsNull(parentAsset.Runtime.Parent, "Parent should remain null after circular assignment");
            Assert.AreSame(parentAsset.Runtime, asset.Runtime.Parent, "Child's parent should not change");

            Object.DestroyImmediate(parentAsset);
        }

        [Test]
        public void SetParent_MultiLevelCycle_PreventsCircularReference()
        {
            var assetA = ScriptableObject.CreateInstance<BlackboardAsset>();
            var assetB = ScriptableObject.CreateInstance<BlackboardAsset>();
            var assetC = ScriptableObject.CreateInstance<BlackboardAsset>();

            ((Blackboard)assetA.Runtime).Parent = assetB.Runtime;
            ((Blackboard)assetB.Runtime).Parent = assetC.Runtime;

            LogAssert.Expect(LogType.Error, "Cannot set parent: would create circular reference");
            ((Blackboard)assetC.Runtime).Parent = assetA.Runtime;

            Assert.IsNull(assetC.Runtime.Parent, "Should prevent cycle at third level");
            Assert.AreSame(assetB.Runtime, assetA.Runtime.Parent);
            Assert.AreSame(assetC.Runtime, assetB.Runtime.Parent);

            Object.DestroyImmediate(assetA);
            Object.DestroyImmediate(assetB);
            Object.DestroyImmediate(assetC);
        }

        [Test]
        public void SetParent_UpdatesRuntimeParent()
        {
            var parentAsset = ScriptableObject.CreateInstance<BlackboardAsset>();

            // Access Runtime to initialize it
            var childRuntime = asset.Runtime;
            var parentRuntime = parentAsset.Runtime;

            ((Blackboard)childRuntime).Parent = parentRuntime;

            Assert.AreSame(parentRuntime, childRuntime.Parent);

            Object.DestroyImmediate(parentAsset);
        }

        [Test]
        public void SetParent_BeforeRuntimeInitialized_DoesNotThrow()
        {
            var parentAsset = ScriptableObject.CreateInstance<BlackboardAsset>();

            Assert.DoesNotThrow(() =>
            {
                ((Blackboard)asset.Runtime).Parent = parentAsset.Runtime;
            });

            Assert.AreSame(parentAsset.Runtime, asset.Runtime.Parent);

            Object.DestroyImmediate(parentAsset);
        }

        [Test]
        public void ParentHierarchy_PropagatesValuesToRuntime()
        {
            var parentAsset = ScriptableObject.CreateInstance<BlackboardAsset>();

            parentAsset.Runtime.SetValue<int>("ParentValue", 100);
            ((Blackboard)asset.Runtime).Parent = parentAsset.Runtime;

            var result = asset.Runtime.GetValue<int>("ParentValue");

            Assert.AreEqual(100, result);

            Object.DestroyImmediate(parentAsset);
        }

        [Test]
        public void LoadTestAsset_InitializesRuntimeWithSerializedData()
        {
            // Load a test asset that should have pre-serialized data
            var testAsset = Resources.Load<BlackboardAsset>("TestBlackboardAsset");

            if (testAsset != null)
            {
                // Verify Runtime is initialized
                Assert.IsNotNull(testAsset.Runtime);

                // Verify it has the expected data from the asset
                // (This test requires the test asset to be created with known values)
                Assert.IsTrue(testAsset.Runtime.HasKey<int>("TestInt"));
                Assert.AreEqual(42, testAsset.Runtime.GetValue<int>("TestInt"));
            }
            else
            {
                Assert.Inconclusive("TestBlackboardAsset not found in Resources. Create test asset first.");
            }
        }

        [Test]
        public void MultipleAssets_HaveSeparateRuntimeInstances()
        {
            var asset1 = ScriptableObject.CreateInstance<BlackboardAsset>();
            var asset2 = ScriptableObject.CreateInstance<BlackboardAsset>();

            var runtime1 = asset1.Runtime;
            var runtime2 = asset2.Runtime;

            Assert.AreNotSame(runtime1, runtime2);

            runtime1.SetValue<int>("Value", 1);
            runtime2.SetValue<int>("Value", 2);

            Assert.AreEqual(1, runtime1.GetValue<int>("Value"));
            Assert.AreEqual(2, runtime2.GetValue<int>("Value"));

            Object.DestroyImmediate(asset1);
            Object.DestroyImmediate(asset2);
        }

        [Test]
        public void LoadTestParentAsset_HasCorrectHierarchy()
        {
            var parentAsset = Resources.Load<BlackboardAsset>("TestParentBlackboardAsset");
            var childAsset = Resources.Load<BlackboardAsset>("TestChildBlackboardAsset");

            if (parentAsset != null && childAsset != null)
            {
                Assert.AreSame(parentAsset.Runtime, childAsset.Runtime.Parent);
                Assert.IsTrue(childAsset.Runtime.HasKey<int>("ParentValue"));
                Assert.AreEqual(999, childAsset.Runtime.GetValue<int>("ParentValue"));
                Assert.AreEqual(123, childAsset.Runtime.GetValue<int>("ChildValue"));
            }
            else
            {
                Assert.Inconclusive("Test assets not found. Ensure TestParentBlackboardAsset and TestChildBlackboardAsset exist in Resources folder.");
            }
        }

        [Test]
        public void Runtime_AfterSerialization_MaintainsData()
        {
            // Set some values on the runtime
            asset.Runtime.SetValue<int>("SerializedInt", 100);
            asset.Runtime.SetValue<string>("SerializedString", "Test");
            asset.Runtime.SetValue<Vector3>("SerializedVector", new Vector3(5, 10, 15));

            // Simulate serialization by calling OnBeforeSerialize
            var serializable = asset as UnityEngine.ISerializationCallbackReceiver;
            serializable.OnBeforeSerialize();

            // Clear the runtime to simulate deserialization
            asset.Runtime.ClearAll();

            // Simulate deserialization
            serializable.OnAfterDeserialize();

            // Verify data was restored
            Assert.AreEqual(100, asset.Runtime.GetValue<int>("SerializedInt"));
            Assert.AreEqual("Test", asset.Runtime.GetValue<string>("SerializedString"));
            Assert.AreEqual(new Vector3(5, 10, 15), asset.Runtime.GetValue<Vector3>("SerializedVector"));
        }

        [Test]
        public void SyncToRuntime_WithNotifyChanges_TriggersCallbacks()
        {
            int callbackCount = 0;
            string lastChangedKey = null;

            asset.Runtime.SetValue<int>("Key1", 10);
            asset.Runtime.SetValue<int>("Key2", 20);

            asset.Runtime.OnAnyValueChanged += (key) =>
            {
                callbackCount++;
                lastChangedKey = key;
            };

            // SyncToRuntime with notifyChanges=true should trigger callbacks
            asset.SyncToRuntime(notifyChanges: true);

            Assert.Greater(callbackCount, 0, "Should have triggered callbacks for existing keys");
        }
    }
}
