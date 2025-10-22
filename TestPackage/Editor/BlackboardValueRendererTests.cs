using AiInGames.Blackboard.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace AiInGames.Blackboard.Tests.Editor
{
    /// <summary>
    /// Smoke tests for BlackboardValueRenderer.
    /// These verify the renderer doesn't crash, not implementation details.
    /// </summary>
    [TestFixture]
    public class BlackboardValueRendererTests
    {
        [Test]
        public void CreateValueField_ForPrimitiveTypes_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => BlackboardValueRenderer.CreateValueField(typeof(int), 42, false, null));
            Assert.DoesNotThrow(() => BlackboardValueRenderer.CreateValueField(typeof(float), 3.14f, false, null));
            Assert.DoesNotThrow(() => BlackboardValueRenderer.CreateValueField(typeof(bool), true, false, null));
            Assert.DoesNotThrow(() => BlackboardValueRenderer.CreateValueField(typeof(string), "test", false, null));
        }

        [Test]
        public void CreateValueField_ForUnityTypes_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => BlackboardValueRenderer.CreateValueField(typeof(Vector3), Vector3.zero, false, null));
            Assert.DoesNotThrow(() => BlackboardValueRenderer.CreateValueField(typeof(GameObject), null, false, null));
        }

        [Test]
        public void CreateValueField_ReturnsVisualElement()
        {
            var element = BlackboardValueRenderer.CreateValueField(typeof(int), 42, false, null);

            Assert.IsNotNull(element);
            Assert.IsInstanceOf<VisualElement>(element);
        }

        [Test]
        public void CreateValueField_ReadOnly_CreatesDisabledField()
        {
            var element = BlackboardValueRenderer.CreateValueField(typeof(int), 42, true, null);

            Assert.IsNotNull(element);
            Assert.IsFalse(element.enabledSelf, "Read-only field should be disabled");
        }

        [Test]
        public void CreateValueField_NotReadOnly_CreatesEnabledField()
        {
            var element = BlackboardValueRenderer.CreateValueField(typeof(int), 42, false, null);

            Assert.IsNotNull(element);
            Assert.IsTrue(element.enabledSelf, "Editable field should be enabled");
        }
    }
}
