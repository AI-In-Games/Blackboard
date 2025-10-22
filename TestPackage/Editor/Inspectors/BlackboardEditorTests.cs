using System.Collections.Generic;
using System.Linq;
using AiInGames.Blackboard.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AiInGames.Blackboard.Tests.Editor.Inspectors
{
    /// <summary>
    /// Tests the critical flow: Serialized Asset → Runtime API
    /// These tests verify that modifying the blackboard asset through the editor
    /// properly updates the runtime blackboard API and triggers change notifications.
    /// </summary>
    [TestFixture]
    public class BlackboardEditorTests
    {
        BlackboardAsset m_Blackboard;

        [SetUp]
        public void SetUp()
        {
            m_Blackboard = ScriptableObject.CreateInstance<BlackboardAsset>();
        }

        [TearDown]
        public void TearDown()
        {
            if (m_Blackboard != null)
            {
                Object.DestroyImmediate(m_Blackboard);
            }
        }

        [Test]
        public void ModifyingAsset_UpdatesRuntimeAPI()
        {
            BlackboardEditorHelper.SetValue(m_Blackboard, "Health", typeof(int), 100);

            var runtimeValue = m_Blackboard.Runtime.GetValue<int>("Health");

            Assert.AreEqual(100, runtimeValue);
        }

        [Test]
        public void ModifyingAsset_TriggersChangeNotification()
        {
            BlackboardEditorHelper.SetValue(m_Blackboard, "Counter", typeof(int), 0);

            int notificationCount = 0;
            int receivedValue = 0;

            m_Blackboard.Runtime.Subscribe("Counter", () =>
            {
                notificationCount++;
                receivedValue = m_Blackboard.Runtime.GetValue<int>("Counter");
            });

            BlackboardEditorHelper.SetValue(m_Blackboard, "Counter", typeof(int), 42);

            Assert.AreEqual(1, notificationCount, "Change notification should fire once");
            Assert.AreEqual(42, receivedValue, "Notification should receive new value");
        }

        [Test]
        public void ModifyingAsset_WithSameValue_DoesNotTriggerNotification()
        {
            BlackboardEditorHelper.SetValue(m_Blackboard, "Stable", typeof(int), 10);

            int notificationCount = 0;
            m_Blackboard.Runtime.Subscribe("Stable", () => notificationCount++);

            BlackboardEditorHelper.SetValue(m_Blackboard, "Stable", typeof(int), 10);

            Assert.AreEqual(0, notificationCount, "No notification when value unchanged");
        }

        [Test]
        public void AddingNewKey_MakesItAccessibleViaRuntimeAPI()
        {
            BlackboardEditorHelper.SetValue(m_Blackboard, "NewKey", typeof(string), "test");

            Assert.IsTrue(m_Blackboard.Runtime.HasKey<string>("NewKey"));
            Assert.AreEqual("test", m_Blackboard.Runtime.GetValue<string>("NewKey"));
        }

        [Test]
        public void SettingListValue_PreservesListContents()
        {
            var list = new List<int> { 1, 2, 3 };

            BlackboardEditorHelper.SetValue(m_Blackboard, "Numbers", typeof(List<int>), list);

            var result = m_Blackboard.Runtime.GetValue<List<int>>("Numbers");
            Assert.AreEqual(3, result.Count);
            CollectionAssert.AreEqual(list, result);
        }

        [Test]
        public void ModifyingListValue_TriggersChangeNotification()
        {
            BlackboardEditorHelper.SetValue(m_Blackboard, "Items", typeof(List<string>), new List<string> { "a" });

            int notificationCount = 0;
            m_Blackboard.Runtime.Subscribe("Items", () => notificationCount++);

            var newList = new List<string> { "a", "b", "c" };
            BlackboardEditorHelper.SetValue(m_Blackboard, "Items", typeof(List<string>), newList);

            Assert.AreEqual(1, notificationCount);
        }

        [Test]
        public void ParentBlackboard_Serialization_PreservesReference()
        {
            var parent = ScriptableObject.CreateInstance<BlackboardAsset>();
            var serializedObject = new SerializedObject(m_Blackboard);

            serializedObject.FindProperty("m_ParentBlackboard").objectReferenceValue = parent;
            serializedObject.ApplyModifiedProperties();

            Assert.AreEqual(parent, m_Blackboard.m_ParentBlackboard);
            Assert.AreSame(parent.Runtime, m_Blackboard.Runtime.Parent);

            Object.DestroyImmediate(parent);
        }

        [Test]
        public void MultipleValueTypes_AllAccessibleViaRuntimeAPI()
        {
            BlackboardEditorHelper.SetValue(m_Blackboard, "IntKey", typeof(int), 42);
            BlackboardEditorHelper.SetValue(m_Blackboard, "FloatKey", typeof(float), 3.14f);
            BlackboardEditorHelper.SetValue(m_Blackboard, "BoolKey", typeof(bool), true);
            BlackboardEditorHelper.SetValue(m_Blackboard, "StringKey", typeof(string), "hello");

            Assert.AreEqual(42, m_Blackboard.Runtime.GetValue<int>("IntKey"));
            Assert.AreEqual(3.14f, m_Blackboard.Runtime.GetValue<float>("FloatKey"), 0.001f);
            Assert.AreEqual(true, m_Blackboard.Runtime.GetValue<bool>("BoolKey"));
            Assert.AreEqual("hello", m_Blackboard.Runtime.GetValue<string>("StringKey"));
        }

        [Test]
        public void GetAllEntries_ReturnsAllModifiedValues()
        {
            BlackboardEditorHelper.SetValue(m_Blackboard, "Key1", typeof(int), 1);
            BlackboardEditorHelper.SetValue(m_Blackboard, "Key2", typeof(string), "test");

            var entries = new List<(string name, System.Type type, object value)>(
                BlackboardEditorHelper.GetAllEntries(m_Blackboard));

            Assert.AreEqual(2, entries.Count);
            Assert.IsTrue(entries.Exists(e => e.name == "Key1" && (int)e.value == 1));
            Assert.IsTrue(entries.Exists(e => e.name == "Key2" && (string)e.value == "test"));
        }

        [Test]
        public void ChangeNotification_ReceivesCorrectValueType()
        {
            BlackboardEditorHelper.SetValue(m_Blackboard, "TypedKey", typeof(float), 0f);

            float receivedValue = 0f;
            m_Blackboard.Runtime.Subscribe("TypedKey", () => receivedValue = m_Blackboard.Runtime.GetValue<float>("TypedKey"));

            BlackboardEditorHelper.SetValue(m_Blackboard, "TypedKey", typeof(float), 99.5f);

            Assert.AreEqual(99.5f, receivedValue, 0.001f);
        }

        [Test]
        public void AddingKey_WithSameNameAsParentKey_PreventsDuplicate()
        {
            var parent = ScriptableObject.CreateInstance<BlackboardAsset>();
            m_Blackboard.m_ParentBlackboard = parent;
            m_Blackboard.SyncToRuntime();

            BlackboardEditorHelper.SetValue(parent, "SharedKey", typeof(int), 10);

            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "Cannot create key 'SharedKey': key already exists in parent blackboard hierarchy");
            BlackboardEditorHelper.SetValue(m_Blackboard, "SharedKey", typeof(int), 20, checkParentConflict: true);

            var localEntries = BlackboardEditorHelper.GetAllEntries(m_Blackboard, includeInherited: false).ToList();
            Assert.IsFalse(localEntries.Any(e => e.name == "SharedKey"), "Should not create duplicate key in child");

            Object.DestroyImmediate(parent);
        }

        [Test]
        public void AddingKey_WithoutParentConflictCheck_AllowsOverride()
        {
            var parent = ScriptableObject.CreateInstance<BlackboardAsset>();
            m_Blackboard.m_ParentBlackboard = parent;
            m_Blackboard.SyncToRuntime();

            BlackboardEditorHelper.SetValue(parent, "OverrideKey", typeof(int), 10);
            BlackboardEditorHelper.SetValue(m_Blackboard, "OverrideKey", typeof(int), 20, checkParentConflict: false);

            var localEntries = BlackboardEditorHelper.GetAllEntries(m_Blackboard, includeInherited: false).ToList();
            Assert.IsTrue(localEntries.Any(e => e.name == "OverrideKey"), "Should allow override when check disabled");
            Assert.AreEqual(20, m_Blackboard.Runtime.GetValue<int>("OverrideKey"), "Should use child value");

            Object.DestroyImmediate(parent);
        }

        [Test]
        public void ParentDeletion_HandledGracefully()
        {
            var parent = ScriptableObject.CreateInstance<BlackboardAsset>();
            m_Blackboard.m_ParentBlackboard = parent;
            m_Blackboard.SyncToRuntime();

            parent.Runtime.SetValue<int>("ParentKey", 42);
            Assert.AreEqual(42, m_Blackboard.Runtime.GetValue<int>("ParentKey"));

            Object.DestroyImmediate(parent);
            m_Blackboard.m_ParentBlackboard = null;
            m_Blackboard.SyncToRuntime();

            Assert.IsFalse(m_Blackboard.Runtime.HasKey<int>("ParentKey"), "Parent keys should not be accessible after parent deletion");
        }

        [Test]
        public void IsKeyInParent_FindsKeyInDirectParent()
        {
            var parent = ScriptableObject.CreateInstance<BlackboardAsset>();
            BlackboardEditorHelper.SetValue(parent, "ParentKey", typeof(string), "value");

            bool found = BlackboardEditorHelper.IsKeyInParent(parent, "ParentKey");
            Assert.IsTrue(found, "Should find key in direct parent");

            Object.DestroyImmediate(parent);
        }

        [Test]
        public void IsKeyInParent_FindsKeyInGrandparent()
        {
            var grandparent = ScriptableObject.CreateInstance<BlackboardAsset>();
            var parent = ScriptableObject.CreateInstance<BlackboardAsset>();

            parent.m_ParentBlackboard = grandparent;
            BlackboardEditorHelper.SetValue(grandparent, "GrandparentKey", typeof(int), 99);

            bool found = BlackboardEditorHelper.IsKeyInParent(parent, "GrandparentKey");
            Assert.IsTrue(found, "Should find key traversing parent chain");

            Object.DestroyImmediate(grandparent);
            Object.DestroyImmediate(parent);
        }

        [Test]
        public void IsKeyInParent_ReturnsFalseForNonExistentKey()
        {
            var parent = ScriptableObject.CreateInstance<BlackboardAsset>();
            BlackboardEditorHelper.SetValue(parent, "ExistingKey", typeof(int), 1);

            bool found = BlackboardEditorHelper.IsKeyInParent(parent, "NonExistent");
            Assert.IsFalse(found, "Should return false for non-existent key");

            Object.DestroyImmediate(parent);
        }

        [Test]
        public void GetAllEntries_WithInheritance_IncludesParentKeys()
        {
            var parent = ScriptableObject.CreateInstance<BlackboardAsset>();
            BlackboardEditorHelper.SetValue(parent, "ParentKey", typeof(int), 100);
            BlackboardEditorHelper.SetValue(m_Blackboard, "ChildKey", typeof(string), "child");

            m_Blackboard.m_ParentBlackboard = parent;

            var allEntries = BlackboardEditorHelper.GetAllEntries(m_Blackboard, includeInherited: true).ToList();
            var localEntries = BlackboardEditorHelper.GetAllEntries(m_Blackboard, includeInherited: false).ToList();

            Assert.AreEqual(2, allEntries.Count, "Should include both child and parent keys");
            Assert.AreEqual(1, localEntries.Count, "Should only include child keys");
            Assert.IsTrue(allEntries.Any(e => e.name == "ParentKey"), "Should find parent key");
            Assert.IsTrue(allEntries.Any(e => e.name == "ChildKey"), "Should find child key");

            Object.DestroyImmediate(parent);
        }

        [Test]
        public void OnAnyValueChanged_FiresWhenModifyingAsset()
        {
            string changedKey = null;

            m_Blackboard.Runtime.OnAnyValueChanged += (key) =>
            {
                changedKey = key;
            };

            BlackboardEditorHelper.SetValue(m_Blackboard, "TestKey", typeof(int), 42);

            Assert.AreEqual("TestKey", changedKey);
        }

        [Test]
        public void GetEntriesForEditor_ReturnsEntriesForType()
        {
            BlackboardEditorHelper.SetValue(m_Blackboard, "Health", typeof(int), 100);
            BlackboardEditorHelper.SetValue(m_Blackboard, "Score", typeof(int), 50);
            BlackboardEditorHelper.SetValue(m_Blackboard, "Name", typeof(string), "Player");

            var intKeys = m_Blackboard.Runtime.GetAllEntries()
                .Where(e => e.type == typeof(int))
                .Select(e => e.key)
                .ToList();
            var stringKeys = m_Blackboard.Runtime.GetAllEntries()
                .Where(e => e.type == typeof(string))
                .Select(e => e.key)
                .ToList();

            Assert.AreEqual(2, intKeys.Count);
            Assert.Contains("Health", intKeys);
            Assert.Contains("Score", intKeys);

            Assert.AreEqual(1, stringKeys.Count);
            Assert.Contains("Name", stringKeys);
        }

        [Test]
        public void GetEntriesForEditor_WithNoEntriesOfType_ReturnsEmpty()
        {
            BlackboardEditorHelper.SetValue(m_Blackboard, "Health", typeof(int), 100);

            var floatKeys = m_Blackboard.Runtime.GetAllEntries()
                .Where(e => e.type == typeof(float))
                .Select(e => e.key)
                .ToList();

            Assert.AreEqual(0, floatKeys.Count);
        }

        [Test]
        public void GetEntriesForEditor_WithMultipleTypes_ReturnsCorrectEntries()
        {
            BlackboardEditorHelper.SetValue(m_Blackboard, "Position", typeof(Vector3), Vector3.zero);
            BlackboardEditorHelper.SetValue(m_Blackboard, "Target", typeof(GameObject), null);
            BlackboardEditorHelper.SetValue(m_Blackboard, "IsActive", typeof(bool), true);

            var vector3Keys = m_Blackboard.Runtime.GetAllEntries()
                .Where(e => e.type == typeof(Vector3))
                .Select(e => e.key)
                .ToList();
            var gameObjectKeys = m_Blackboard.Runtime.GetAllEntries()
                .Where(e => e.type == typeof(GameObject))
                .Select(e => e.key)
                .ToList();
            var boolKeys = m_Blackboard.Runtime.GetAllEntries()
                .Where(e => e.type == typeof(bool))
                .Select(e => e.key)
                .ToList();

            Assert.AreEqual(1, vector3Keys.Count);
            Assert.Contains("Position", vector3Keys);

            Assert.AreEqual(1, gameObjectKeys.Count);
            Assert.Contains("Target", gameObjectKeys);

            Assert.AreEqual(1, boolKeys.Count);
            Assert.Contains("IsActive", boolKeys);
        }

        [Test]
        public void SaveRuntimeDataToAsset_SavesRuntimeChangesToAsset()
        {
            m_Blackboard.Runtime.SetValue("Health", 100);
            m_Blackboard.Runtime.SetValue("Name", "Player");

            BlackboardEditorHelper.SaveRuntimeDataToAsset(m_Blackboard);

            var entries = m_Blackboard.Runtime.GetAllEntries().ToList();
            var intEntries = entries.Where(e => e.type == typeof(int)).ToList();
            var stringEntries = entries.Where(e => e.type == typeof(string)).ToList();

            Assert.AreEqual(1, intEntries.Count);
            Assert.AreEqual("Health", intEntries[0].key);
            Assert.AreEqual(100, intEntries[0].value);

            Assert.AreEqual(1, stringEntries.Count);
            Assert.AreEqual("Name", stringEntries[0].key);
            Assert.AreEqual("Player", stringEntries[0].value);
        }

        [Test]
        public void SaveRuntimeDataToAsset_CreatesNewEntriesForNewKeys()
        {
            m_Blackboard.Runtime.SetValue("NewKey1", 42);
            m_Blackboard.Runtime.SetValue("NewKey2", 3.14f);

            BlackboardEditorHelper.SaveRuntimeDataToAsset(m_Blackboard);

            var blackboardCopy = ScriptableObject.CreateInstance<BlackboardAsset>();
            var json = JsonUtility.ToJson(m_Blackboard);
            JsonUtility.FromJsonOverwrite(json, blackboardCopy);

            Assert.AreEqual(42, blackboardCopy.Runtime.GetValue<int>("NewKey1"));
            Assert.AreEqual(3.14f, blackboardCopy.Runtime.GetValue<float>("NewKey2"), 0.001f);

            Object.DestroyImmediate(blackboardCopy);
        }

        [Test]
        public void SaveRuntimeDataToAsset_UpdatesExistingEntries()
        {
            BlackboardEditorHelper.SetValue(m_Blackboard, "Score", typeof(int), 50);

            m_Blackboard.Runtime.SetValue("Score", 100);

            BlackboardEditorHelper.SaveRuntimeDataToAsset(m_Blackboard);

            Assert.AreEqual(100, m_Blackboard.Runtime.GetValue<int>("Score"));

            var blackboardCopy = ScriptableObject.CreateInstance<BlackboardAsset>();
            var json = JsonUtility.ToJson(m_Blackboard);
            JsonUtility.FromJsonOverwrite(json, blackboardCopy);

            Assert.AreEqual(100, blackboardCopy.Runtime.GetValue<int>("Score"));

            Object.DestroyImmediate(blackboardCopy);
        }
    }
}
