using Microsoft.VisualStudio.TestTools.UnitTesting;
using Msmc.Patterns.Messenger;
using System;
using System.Collections.Generic;
using System.Text;

namespace Msmc.Patterns.Messenger.Tests
{
    [TestClass()]
    public class MessengerTests
    {
        private class TestMessage
        {

        }

        private class DerivedTestMessage : TestMessage
        {

        }

        private interface ISome
        {

        }

        private class SomeInterfaceClass : ISome
        {

        }

        [TestMethod()]
        public void DefaultIsNotNull()
        {
            Assert.IsNotNull(Messenger.Default);
        }

        [TestMethod()]
        public void TestRegisterAndSend()
        {
            Messenger.ResetDefault();

            var received = false;
            Messenger.Default.Register<TestMessage>(this, (message) =>
            {
                received = true;
            });

            Messenger.Default.Send(new TestMessage());

            Assert.IsTrue(received);
        }

        [TestMethod]
        public void TestUnregister()
        {
            Messenger.ResetDefault();

            var received = false;
            Messenger.Default.Register<TestMessage>(this, (message) =>
            {
                received = true;
            });

            Messenger.Default.Unregister<TestMessage>(this);

            Messenger.Default.Send(new TestMessage());

            Assert.IsFalse(received);
        }

        [TestMethod]
        public void TestUnregisterWithOtherRecipient()
        {
            Messenger.ResetDefault();

            var received = false;
            Messenger.Default.Register<TestMessage>(this, (message) =>
            {
                received = true;
            });

            Messenger.Default.Unregister<TestMessage>(new object { });

            Messenger.Default.Send(new TestMessage());

            Assert.IsTrue(received);
        }

        [TestMethod]
        public void TestDerivedMessage()
        {
            Messenger.ResetDefault();

            var received = false;
            Messenger.Default.Register<TestMessage>(this, true, (message) =>
            {
                received = true;
            });

            Messenger.Default.Send(new DerivedTestMessage());

            Assert.IsTrue(received);
        }

        [TestMethod]
        public void TestInterfaceMessage()
        {
            Messenger.ResetDefault();

            var received = false;
            Messenger.Default.Register<ISome>(this, true, (message) =>
            {
                received = true;
            });

            Messenger.Default.Send(new SomeInterfaceClass());

            Assert.IsTrue(received);
        }
    }
}