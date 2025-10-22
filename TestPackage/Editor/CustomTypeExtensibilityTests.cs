using NUnit.Framework;
using UnityEngine;
using System.Linq;
using AiInGames.Blackboard.Editor;

namespace AiInGames.Blackboard.Tests.Editor
{
    public class CustomTypeExtensibilityTests
    {
        [Test]
        public void CustomType_IsDiscoveredByFactory()
        {
            var supportedTypes = BlackboardValuesFactory.GetAllSupportedTypes().ToList();
            Assert.IsTrue(supportedTypes.Contains(typeof(CustomPlayerData)),
                "CustomPlayerData should be automatically discovered by TypeCache");
        }

        [Test]
        public void CustomType_CanCreateEntry()
        {
            var entry = BlackboardValuesFactory.CreateEntry("TestKey", typeof(CustomPlayerData));
            Assert.IsNotNull(entry, "Should be able to create entry for custom type");
            Assert.AreEqual("TestKey", entry.Key);
            Assert.AreEqual(typeof(CustomPlayerData), entry.GetValueType());
        }

        [Test]
        public void CustomType_CanSetAndGetValue()
        {
            var entry = BlackboardValuesFactory.CreateEntry("TestKey", typeof(CustomPlayerData));
            var testData = new CustomPlayerData
            {
                PlayerName = "TestPlayer",
                Level = 42,
                Health = 100.5f
            };

            entry.SetValue(testData);
            var retrievedValue = (CustomPlayerData)entry.GetValue();

            Assert.AreEqual("TestPlayer", retrievedValue.PlayerName);
            Assert.AreEqual(42, retrievedValue.Level);
            Assert.AreEqual(100.5f, retrievedValue.Health);
        }

        [Test]
        public void CustomType_CanBeAddedToBlackboardAsset()
        {
            var blackboard = ScriptableObject.CreateInstance<BlackboardAsset>();

            BlackboardEditorHelper.SetValue(blackboard, "PlayerData", typeof(CustomPlayerData), new CustomPlayerData
            {
                PlayerName = "Hero",
                Level = 10,
                Health = 75.0f
            });

            var entries = BlackboardEditorHelper.GetAllEntries(blackboard).ToList();
            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual("PlayerData", entries[0].name);
            Assert.AreEqual(typeof(CustomPlayerData), entries[0].type);

            var playerData = (CustomPlayerData)entries[0].value;
            Assert.AreEqual("Hero", playerData.PlayerName);
            Assert.AreEqual(10, playerData.Level);
            Assert.AreEqual(75.0f, playerData.Health);

            Object.DestroyImmediate(blackboard);
        }

        [Test]
        public void CustomType_CanBeUsedInRuntimeAPI()
        {
            var blackboard = ScriptableObject.CreateInstance<BlackboardAsset>();

            BlackboardEditorHelper.SetValue(blackboard, "PlayerData", typeof(CustomPlayerData), new CustomPlayerData
            {
                PlayerName = "Warrior",
                Level = 20,
                Health = 150.0f
            });

            var runtime = blackboard.Runtime;
            Assert.IsTrue(runtime.TryGetValue<CustomPlayerData>("PlayerData", out var playerData));
            Assert.AreEqual("Warrior", playerData.PlayerName);
            Assert.AreEqual(20, playerData.Level);
            Assert.AreEqual(150.0f, playerData.Health);

            runtime.SetValue("PlayerData", new CustomPlayerData
            {
                PlayerName = "Mage",
                Level = 25,
                Health = 120.0f
            });

            Assert.IsTrue(runtime.TryGetValue<CustomPlayerData>("PlayerData", out playerData));
            Assert.AreEqual("Mage", playerData.PlayerName);

            Object.DestroyImmediate(blackboard);
        }

        [Test]
        public void CustomType_FromTestAsset_CanBeRead()
        {
            var testAsset = Resources.Load<BlackboardAsset>("BlackboardWithCustomType");

            if (testAsset == null)
            {
                Assert.Inconclusive("Test asset 'TestBlackboards/BlackboardWithCustomType' not found. " +
                    "Create this asset manually with a CustomPlayerData value to enable this test.");
                return;
            }

            var entries = BlackboardEditorHelper.GetAllEntries(testAsset).ToList();
            var customTypeEntries = entries.Where(e => e.type == typeof(CustomPlayerData)).ToList();

            Assert.IsTrue(customTypeEntries.Count > 0,
                "Test asset should contain at least one CustomPlayerData entry");

            var entry = customTypeEntries[0];
            var playerData = (CustomPlayerData)entry.value;

            Assert.IsNotNull(playerData);
            Assert.IsFalse(string.IsNullOrEmpty(playerData.PlayerName),
                "PlayerName should be set in test asset");
        }

        [Test]
        public void CustomType_AppearsInSupportedTypesList()
        {
            var supportedTypes = BlackboardEditorHelper.SupportedTypes.ToList();
            var customTypeEntry = supportedTypes.FirstOrDefault(t => t.type == typeof(CustomPlayerData));

            Assert.IsNotNull(customTypeEntry);
            Assert.AreEqual("Custom Player Data", customTypeEntry.displayName);
        }

        [Test]
        public void CustomType_HasCorrectDisplayName()
        {
            var displayName = BlackboardEditorHelper.GetDisplayName(typeof(CustomPlayerData));
            Assert.AreEqual("Custom Player Data", displayName);
        }

#if UNITY_EDITOR
        [Test]
        public void CustomType_CanCreateInspectorElement()
        {
            var entry = BlackboardValuesFactory.CreateEntry("TestKey", typeof(CustomPlayerData));
            entry.SetValue(new CustomPlayerData
            {
                PlayerName = "Inspector Test",
                Level = 5,
                Health = 50.0f
            });

            var element = entry.CreateInspectorElement(readOnly: false, onValueChanged: null);
            Assert.IsNotNull(element, "Custom type should provide inspector element");
        }
#endif
    }
}
