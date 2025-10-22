using System.Collections.Generic;
using AiInGames.Blackboard.Editor;
using NUnit.Framework;
using UnityEngine;

namespace AiInGames.Blackboard.Tests.Editor
{
    [TestFixture]
    public class BlackboardWindowViewModelTests
    {
        BlackboardWindowViewModel m_ViewModel;
        IBlackboard m_Blackboard;

        [SetUp]
        public void SetUp()
        {
            m_ViewModel = new BlackboardWindowViewModel();
            m_Blackboard = new Blackboard();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void SetTarget_UpdatesTargetBlackboard()
        {
            m_ViewModel.SetTarget(m_Blackboard);

            Assert.AreEqual(m_Blackboard, m_ViewModel.TargetBlackboard);
        }

        [Test]
        public void SetTarget_FiresOnDataChanged()
        {
            bool eventFired = false;
            m_ViewModel.OnDataChanged += () => eventFired = true;

            m_ViewModel.SetTarget(m_Blackboard);

            Assert.IsTrue(eventFired);
        }

        [Test]
        public void GetEntries_WhenBlackboardIsNull_ReturnsEmptyList()
        {
            var entries = m_ViewModel.GetEntries();

            Assert.IsEmpty(entries);
        }

        [Test]
        public void GetEntries_ReturnsCorrectEntries()
        {
            m_Blackboard.SetValue<int>("TestInt", 42);
            m_Blackboard.SetValue<string>("TestString", "hello");
            m_ViewModel.SetTarget(m_Blackboard);

            var entries = m_ViewModel.GetEntries();

            Assert.AreEqual(2, entries.Count);
            Assert.IsTrue(entries[0].Name == "TestInt" || entries[0].Name == "TestString");
        }

        [Test]
        public void SetValue_UpdatesBlackboardValue()
        {
            m_Blackboard.SetValue<int>("Counter", 0);
            m_ViewModel.SetTarget(m_Blackboard);

            m_ViewModel.SetValue("Counter", typeof(int), 99);

            Assert.AreEqual(99, m_Blackboard.GetValue<int>("Counter"));
        }

        [Test]
        public void SetValue_FiresOnDataChanged()
        {
            m_Blackboard.SetValue<int>("Counter", 0);
            m_ViewModel.SetTarget(m_Blackboard);

            bool eventFired = false;
            m_ViewModel.OnDataChanged += () => eventFired = true;

            m_ViewModel.SetValue("Counter", typeof(int), 99);

            Assert.IsTrue(eventFired);
        }

        [TestCase(typeof(int), 42)]
        [TestCase(typeof(float), 3.14f)]
        [TestCase(typeof(bool), true)]
        [TestCase(typeof(string), "test")]
        public void SetValue_SupportsBasicTypes(System.Type type, object value)
        {
            ((Blackboard)m_Blackboard).SetValue("TestKey", type, GetDefaultValue(type));
            m_ViewModel.SetTarget(m_Blackboard);

            m_ViewModel.SetValue("TestKey", type, value);

            var entries = m_ViewModel.GetEntries();
            var entry = entries[0];
            Assert.AreEqual(value, entry.Value);
        }

        [Test]
        public void SetValue_SupportsList()
        {
            var list = new List<int> { 1, 2, 3 };
            m_Blackboard.SetValue<List<int>>("TestList", new List<int>());
            m_ViewModel.SetTarget(m_Blackboard);

            m_ViewModel.SetValue("TestList", typeof(List<int>), list);

            var result = m_Blackboard.GetValue<List<int>>("TestList");
            CollectionAssert.AreEqual(list, result);
        }

        object GetDefaultValue(System.Type type)
        {
            if (type == typeof(int)) return 0;
            if (type == typeof(float)) return 0f;
            if (type == typeof(bool)) return false;
            if (type == typeof(string)) return string.Empty;
            return null;
        }
    }
}
