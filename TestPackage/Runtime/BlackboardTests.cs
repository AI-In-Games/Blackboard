using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;

namespace AiInGames.Blackboard.Tests
{
    [TestFixture]
    public class BlackboardTests
    {
        private Blackboard blackboard;

        [SetUp]
        public void SetUp()
        {
            blackboard = ScriptableObject.CreateInstance<Blackboard>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(blackboard);
        }

        [Test]
        public void SetValue_ThenGetValue_ReturnsCorrectValue()
        {
            blackboard.SetValue<int>("TestInt", 42);

            var result = blackboard.GetValue<int>("TestInt");
            Assert.AreEqual(42, result);
        }

        [Test]
        public void TryGetValue_WithExistingKey_ReturnsTrue()
        {
            blackboard.SetValue<string>("TestString", "Hello");

            bool success = blackboard.TryGetValue<string>("TestString", out string value);

            Assert.IsTrue(success);
            Assert.AreEqual("Hello", value);
        }

        [Test]
        public void TryGetValue_WithNonExistentKey_ReturnsFalse()
        {
            bool success = blackboard.TryGetValue<float>("NonExistent", out float value);

            Assert.IsFalse(success);
            Assert.AreEqual(default(float), value);
        }

        [Test]
        public void GetValue_WithNonExistentKey_ReturnsDefaultValue()
        {
            var result = blackboard.GetValue<int>("Missing", 99);

            Assert.AreEqual(99, result);
        }

        [Test]
        public void SetValue_MultipleTypes_StoresCorrectly()
        {
            blackboard.SetValue<int>("Int", 10);
            blackboard.SetValue<float>("Float", 3.14f);
            blackboard.SetValue<string>("String", "Test");

            Assert.AreEqual(10, blackboard.GetValue<int>("Int"));
            Assert.AreEqual(3.14f, blackboard.GetValue<float>("Float"), 0.001f);
            Assert.AreEqual("Test", blackboard.GetValue<string>("String"));
        }

        [Test]
        public void SetValue_OverwritesExisting_UpdatesValue()
        {
            blackboard.SetValue<int>("Counter", 1);
            blackboard.SetValue<int>("Counter", 2);
            blackboard.SetValue<int>("Counter", 3);

            Assert.AreEqual(3, blackboard.GetValue<int>("Counter"));
        }

        [Test]
        public void HasKey_WithExistingKey_ReturnsTrue()
        {
            blackboard.SetValue<int>("Exists", 100);

            Assert.IsTrue(blackboard.HasKey<int>("Exists"));
        }

        [Test]
        public void HasKey_WithNonExistentKey_ReturnsFalse()
        {
            Assert.IsFalse(blackboard.HasKey<int>("DoesNotExist"));
        }

        [Test]
        public void Remove_ExistingKey_RemovesValue()
        {
            blackboard.SetValue<int>("ToRemove", 123);

            bool removed = blackboard.Remove<int>("ToRemove");

            Assert.IsTrue(removed);
            Assert.IsFalse(blackboard.HasKey<int>("ToRemove"));
        }

        [Test]
        public void Remove_NonExistentKey_ReturnsFalse()
        {
            bool removed = blackboard.Remove<int>("NotThere");

            Assert.IsFalse(removed);
        }

        [Test]
        public void Clear_RemovesAllValues()
        {
            blackboard.SetValue<int>("Key1", 1);
            blackboard.SetValue<string>("Key2", "Two");

            blackboard.ClearAll();

            Assert.IsFalse(blackboard.HasKey<int>("Key1"));
            Assert.IsFalse(blackboard.HasKey<string>("Key2"));
            Assert.AreEqual(0, blackboard.KeyCount());
        }

        [Test]
        public void Subscribe_WhenValueChanges_InvokesCallback()
        {
            int callbackValue = 0;
            int callbackCount = 0;

            blackboard.Subscribe("Observable", () =>
            {
                callbackValue = blackboard.GetValue<int>("Observable");
                callbackCount++;
            });

            blackboard.SetValue<int>("Observable", 42);

            Assert.AreEqual(42, callbackValue);
            Assert.AreEqual(1, callbackCount);
        }

        [Test]
        public void Subscribe_WhenValueSetToSame_DoesNotInvokeCallback()
        {
            int callbackCount = 0;

            blackboard.SetValue<int>("Stable", 10);
            blackboard.Subscribe("Stable", () => callbackCount++);

            blackboard.SetValue<int>("Stable", 10);

            Assert.AreEqual(0, callbackCount);
        }

        [Test]
        public void Unsubscribe_StopsReceivingCallbacks()
        {
            int callbackCount = 0;

            void Callback() => callbackCount++;

            blackboard.Subscribe("Unsubscribed", Callback);
            blackboard.SetValue<int>("Unsubscribed", 1);

            blackboard.Unsubscribe("Unsubscribed", Callback);
            blackboard.SetValue<int>("Unsubscribed", 2);

            Assert.AreEqual(1, callbackCount);
        }

        [Test]
        public void Subscribe_MultipleCallbacks_AllInvoked()
        {
            int count1 = 0, count2 = 0, count3 = 0;

            blackboard.Subscribe("Multi", () => count1++);
            blackboard.Subscribe("Multi", () => count2++);
            blackboard.Subscribe("Multi", () => count3++);

            blackboard.SetValue<int>("Multi", 100);

            Assert.AreEqual(1, count1);
            Assert.AreEqual(1, count2);
            Assert.AreEqual(1, count3);
        }

        [Test]
        public void GetValue_WithParent_FallsBackToParent()
        {
            var parent = ScriptableObject.CreateInstance<Blackboard>();
            blackboard.Parent = parent;

            parent.SetValue<int>("Inherited", 999);

            var result = blackboard.GetValue<int>("Inherited");

            Assert.AreEqual(999, result);

            Object.DestroyImmediate(parent);
        }

        [Test]
        public void GetValue_LocalOverridesParent()
        {
            var parent = ScriptableObject.CreateInstance<Blackboard>();
            blackboard.Parent = parent;

            parent.SetValue<string>("Override", "Parent");
            blackboard.SetValue<string>("Override", "Child");

            var result = blackboard.GetValue<string>("Override");

            Assert.AreEqual("Child", result);

            Object.DestroyImmediate(parent);
        }

        [Test]
        public void HasKey_ChecksParent()
        {
            var parent = ScriptableObject.CreateInstance<Blackboard>();
            blackboard.Parent = parent;

            parent.SetValue<int>("ParentKey", 123);

            Assert.IsTrue(blackboard.HasKey<int>("ParentKey"));

            Object.DestroyImmediate(parent);
        }

        [Test]
        public void GetSet_ManyOperations_IsEfficient()
        {
            blackboard.SetValue<int>("Perf", 1);

            System.GC.Collect();
            var gcBefore = System.GC.CollectionCount(0);

            for (int i = 0; i < 1000; i++)
            {
                blackboard.SetValue<int>("Perf", i);
                var value = blackboard.GetValue<int>("Perf");
            }

            var gcAfter = System.GC.CollectionCount(0);

            Assert.AreEqual(gcBefore, gcAfter, "Should not allocate during Get/Set");
        }

        [Test]
        public void SetValue_WithNull_StoresNull()
        {
            blackboard.SetValue<string>("Nullable", null);

            var result = blackboard.GetValue<string>("Nullable");
            Assert.IsNull(result);
        }

        [Test]
        public void Count_ReflectsNumberOfEntries()
        {
            Assert.AreEqual(0, blackboard.KeyCount());

            blackboard.SetValue<int>("Key1", 1);
            Assert.AreEqual(1, blackboard.KeyCount());

            blackboard.SetValue<int>("Key2", 2);
            Assert.AreEqual(2, blackboard.KeyCount());

            blackboard.Remove<int>("Key1");
            Assert.AreEqual(1, blackboard.KeyCount());
        }

        [Test]
        public void SetValue_WithListOfInts_StoresCorrectly()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };

            blackboard.SetValue<List<int>>("IntList", list);
            var result = blackboard.GetValue<List<int>>("IntList");

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Count);
            CollectionAssert.AreEqual(list, result);
        }

        [Test]
        public void SetValue_WithListOfFloats_StoresCorrectly()
        {
            var list = new List<float> { 1.5f, 2.7f, 3.14f };

            blackboard.SetValue<List<float>>("FloatList", list);
            var result = blackboard.GetValue<List<float>>("FloatList");

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            CollectionAssert.AreEqual(list, result);
        }

        [Test]
        public void SetValue_WithListOfStrings_StoresCorrectly()
        {
            var list = new List<string> { "one", "two", "three" };

            blackboard.SetValue<List<string>>("StringList", list);
            var result = blackboard.GetValue<List<string>>("StringList");

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            CollectionAssert.AreEqual(list, result);
        }

        [Test]
        public void SetValue_WithListOfBools_StoresCorrectly()
        {
            var list = new List<bool> { true, false, true };

            blackboard.SetValue<List<bool>>("BoolList", list);
            var result = blackboard.GetValue<List<bool>>("BoolList");

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            CollectionAssert.AreEqual(list, result);
        }

        [Test]
        public void SetValue_WithListOfVector3_StoresCorrectly()
        {
            var list = new List<Vector3>
            {
                new Vector3(1, 2, 3),
                new Vector3(4, 5, 6),
                Vector3.zero
            };

            blackboard.SetValue<List<Vector3>>("Vector3List", list);
            var result = blackboard.GetValue<List<Vector3>>("Vector3List");

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            for (int i = 0; i < list.Count; i++)
            {
                Assert.AreEqual(list[i], result[i]);
            }
        }

        [Test]
        public void SetValue_WithListOfGameObjects_StoresCorrectly()
        {
            var go1 = new GameObject("Test1");
            var go2 = new GameObject("Test2");
            var list = new List<GameObject> { go1, go2 };

            blackboard.SetValue<List<GameObject>>("GameObjectList", list);
            var result = blackboard.GetValue<List<GameObject>>("GameObjectList");

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(go1, result[0]);
            Assert.AreEqual(go2, result[1]);

            Object.DestroyImmediate(go1);
            Object.DestroyImmediate(go2);
        }

        [Test]
        public void SetValue_WithListOfTransforms_StoresCorrectly()
        {
            var go1 = new GameObject("Test1");
            var go2 = new GameObject("Test2");
            var list = new List<Transform> { go1.transform, go2.transform };

            blackboard.SetValue<List<Transform>>("TransformList", list);
            var result = blackboard.GetValue<List<Transform>>("TransformList");

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(go1.transform, result[0]);
            Assert.AreEqual(go2.transform, result[1]);

            Object.DestroyImmediate(go1);
            Object.DestroyImmediate(go2);
        }

        [Test]
        public void SetValue_WithEmptyList_StoresCorrectly()
        {
            var list = new List<int>();

            blackboard.SetValue<List<int>>("EmptyList", list);
            var result = blackboard.GetValue<List<int>>("EmptyList");

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void SetValue_ListModification_UpdatesCorrectly()
        {
            var list = new List<int> { 1, 2, 3 };

            blackboard.SetValue<List<int>>("ModifiableList", list);

            list.Add(4);
            list.Add(5);
            blackboard.SetValue<List<int>>("ModifiableList", list);

            var result = blackboard.GetValue<List<int>>("ModifiableList");

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(5, result[4]);
        }

        [Test]
        public void TryGetValue_WithListKey_ReturnsCorrectList()
        {
            var list = new List<string> { "test1", "test2" };

            blackboard.SetValue<List<string>>("StringList", list);
            bool success = blackboard.TryGetValue<List<string>>("StringList", out List<string> result);

            Assert.IsTrue(success);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            CollectionAssert.AreEqual(list, result);
        }

        [Test]
        public void Subscribe_ListValueChanges_InvokesCallback()
        {
            List<int> callbackValue = null;
            int callbackCount = 0;

            blackboard.Subscribe("ObservableList", () =>
            {
                callbackValue = blackboard.GetValue<List<int>>("ObservableList");
                callbackCount++;
            });

            var list = new List<int> { 10, 20, 30 };
            blackboard.SetValue<List<int>>("ObservableList", list);

            Assert.AreEqual(1, callbackCount);
            Assert.IsNotNull(callbackValue);
            CollectionAssert.AreEqual(list, callbackValue);
        }


        [Test]
        public void SetParent_ToSelf_PreventsCircularReference()
        {
            LogAssert.Expect(LogType.Error, "Blackboard cannot be its own parent");

            blackboard.Parent = blackboard;

            Assert.IsNull(blackboard.Parent, "Parent should remain null after self-assignment");
        }

        [Test]
        public void SetParent_DirectCycle_PreventsCircularReference()
        {
            var parentBlackboard = ScriptableObject.CreateInstance<Blackboard>();

            blackboard.Parent = parentBlackboard;

            LogAssert.Expect(LogType.Error, "Cannot set parent: would create circular reference");
            parentBlackboard.Parent = blackboard;

            Assert.IsNull(parentBlackboard.Parent, "Parent should remain null after circular assignment");
            Assert.AreEqual(parentBlackboard, blackboard.Parent, "Child's parent should not change");

            Object.DestroyImmediate(parentBlackboard);
        }

        [Test]
        public void SetParent_MultiLevelCycle_PreventsCircularReference()
        {
            var blackboardA = ScriptableObject.CreateInstance<Blackboard>();
            var blackboardB = ScriptableObject.CreateInstance<Blackboard>();
            var blackboardC = ScriptableObject.CreateInstance<Blackboard>();

            blackboardA.Parent = blackboardB;
            blackboardB.Parent = blackboardC;

            LogAssert.Expect(LogType.Error, "Cannot set parent: would create circular reference");
            blackboardC.Parent = blackboardA;

            Assert.IsNull(blackboardC.Parent, "Should prevent cycle at third level");
            Assert.AreEqual(blackboardB, blackboardA.Parent);
            Assert.AreEqual(blackboardC, blackboardB.Parent);

            Object.DestroyImmediate(blackboardA);
            Object.DestroyImmediate(blackboardB);
            Object.DestroyImmediate(blackboardC);
        }

        [Test]
        public void SetParent_ValidMultiLevelHierarchy_Works()
        {
            var blackboardA = ScriptableObject.CreateInstance<Blackboard>();
            var blackboardB = ScriptableObject.CreateInstance<Blackboard>();
            var blackboardC = ScriptableObject.CreateInstance<Blackboard>();

            blackboardC.SetValue<int>("Level3", 3);
            blackboardB.SetValue<int>("Level2", 2);
            blackboardA.SetValue<int>("Level1", 1);

            blackboardA.Parent = blackboardB;
            blackboardB.Parent = blackboardC;

            Assert.AreEqual(1, blackboardA.GetValue<int>("Level1"));
            Assert.AreEqual(2, blackboardA.GetValue<int>("Level2"));
            Assert.AreEqual(3, blackboardA.GetValue<int>("Level3"));

            Object.DestroyImmediate(blackboardA);
            Object.DestroyImmediate(blackboardB);
            Object.DestroyImmediate(blackboardC);
        }

        [Test]
        public void SetParent_AfterPreviousAssignment_AllowsReassignment()
        {
            var parent1 = ScriptableObject.CreateInstance<Blackboard>();
            var parent2 = ScriptableObject.CreateInstance<Blackboard>();

            parent1.SetValue<string>("Source", "Parent1");
            parent2.SetValue<string>("Source", "Parent2");

            blackboard.Parent = parent1;
            Assert.AreEqual("Parent1", blackboard.GetValue<string>("Source"));

            blackboard.Parent = parent2;
            Assert.AreEqual("Parent2", blackboard.GetValue<string>("Source"));

            Object.DestroyImmediate(parent1);
            Object.DestroyImmediate(parent2);
        }

        [Test]
        public void GetValue_WithDeepParentChain_IteratesCorrectly()
        {
            var level1 = ScriptableObject.CreateInstance<Blackboard>();
            var level2 = ScriptableObject.CreateInstance<Blackboard>();
            var level3 = ScriptableObject.CreateInstance<Blackboard>();
            var level4 = ScriptableObject.CreateInstance<Blackboard>();

            level4.SetValue<int>("DeepValue", 42);

            level1.Parent = level2;
            level2.Parent = level3;
            level3.Parent = level4;

            var result = level1.GetValue<int>("DeepValue");
            Assert.AreEqual(42, result, "Should traverse 4 levels of parent chain iteratively");

            Object.DestroyImmediate(level1);
            Object.DestroyImmediate(level2);
            Object.DestroyImmediate(level3);
            Object.DestroyImmediate(level4);
        }

        [Test]
        public void HasKey_WithDeepParentChain_IteratesCorrectly()
        {
            var level1 = ScriptableObject.CreateInstance<Blackboard>();
            var level2 = ScriptableObject.CreateInstance<Blackboard>();
            var level3 = ScriptableObject.CreateInstance<Blackboard>();

            level3.SetValue<string>("DeepKey", "value");

            level1.Parent = level2;
            level2.Parent = level3;

            Assert.IsTrue(level1.HasKey<string>("DeepKey"), "Should find key in deep parent chain");

            Object.DestroyImmediate(level1);
            Object.DestroyImmediate(level2);
            Object.DestroyImmediate(level3);
        }

        [Test]
        public void OnAnyValueChanged_FiresWhenValueChanges()
        {
            string changedKey = null;
            int callbackCount = 0;

            blackboard.OnAnyValueChanged += (key) =>
            {
                changedKey = key;
                callbackCount++;
            };

            blackboard.SetValue("TestKey", 42);

            Assert.AreEqual("TestKey", changedKey);
            Assert.AreEqual(1, callbackCount);
        }

        [Test]
        public void OnAnyValueChanged_DoesNotFireWhenValueUnchanged()
        {
            int callbackCount = 0;

            blackboard.SetValue("TestKey", 42);

            blackboard.OnAnyValueChanged += (key) =>
            {
                callbackCount++;
            };

            blackboard.SetValue("TestKey", 42);

            Assert.AreEqual(0, callbackCount);
        }

        [Test]
        public void OnAnyValueChanged_FiresForMultipleKeys()
        {
            var changedKeys = new List<string>();

            blackboard.OnAnyValueChanged += (key) =>
            {
                changedKeys.Add(key);
            };

            blackboard.SetValue("Key1", 1);
            blackboard.SetValue("Key2", "test");
            blackboard.SetValue("Key3", true);

            Assert.AreEqual(3, changedKeys.Count);
            Assert.Contains("Key1", changedKeys);
            Assert.Contains("Key2", changedKeys);
            Assert.Contains("Key3", changedKeys);
        }

    }
}
