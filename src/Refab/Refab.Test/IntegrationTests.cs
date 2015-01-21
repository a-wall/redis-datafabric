using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using NUnit.Framework;
using StackExchange.Redis;

namespace Refab.Test
{
    /// <summary>
    /// A running instance of redis is required.
    /// </summary>
    [TestFixture]
    public class IntegrationTests
    {
        private ConnectionMultiplexer _redis;
        private Random _random = new Random();

        [SetUp]
        public void SetUp()
        {
            _redis = ConnectionMultiplexer.Connect("localhost");
        }

        [Test]
        public void SimpleWriteTest()
        {
            var db = _redis.GetDatabase();
            db.StringSet("my-test-key", 1);
        }

        [Test]
        public void SimpleWriteReadTest()
        {
            var testValue = "value is " + _random.Next(100);
            var db = _redis.GetDatabase();
            db.StringSet("my-test-key", testValue);
            var v = db.StringGet("my-test-key").ToString();
            Assert.AreEqual(testValue, v);
        }

        [Test]
        public void LastValueCacheTest()
        {
            var testValue = "value is " + _random.Next(100);
            // set initial value
            var observer = _redis.GetObserver("my-test-key");
            observer.OnNext(testValue);

            // subscribe
            var observable = _redis.GetObservable("my-test-key");
            var v = observable.FirstOrDefaultAsync().Wait();
            Assert.AreEqual(testValue, v);
        }

        [Test]
        public void PublishSubscribeTest()
        {
            var d = new CompositeDisposable();
            var evt = new ManualResetEventSlim();
            var ct = 0;

            var observer = _redis.GetObserver("my-test-key");
            d.Add(Observable.Interval(TimeSpan.FromSeconds(2)).Subscribe(l => observer.OnNext("value is " + l)));

            // subscribe
            var observable = _redis.GetObservable("my-test-key");
            d.Add(observable.Subscribe(v =>
            {
                Debug.WriteLine(v);
                if (++ct > 4) evt.Set();
            }));
            evt.Wait(TimeSpan.FromMinutes(1));
            d.Dispose();
            Assert.Greater(ct, 0);
        }

        [Test]
        public void SubscribeToNewKeyTest()
        {
            var d = new CompositeDisposable();
            var evt = new ManualResetEventSlim();
            var ct = 0;

            // ensure test key is missing
            var db = _redis.GetDatabase();
            db.KeyDelete("my-test-key");

            // subscribe
            var observable = _redis.GetObservable("my-test-key");
            d.Add(observable.Subscribe(v =>
            {
                Debug.WriteLine(v);
                ++ct;
                evt.Set();
            }));
            evt.Wait(TimeSpan.FromSeconds(5));
            d.Dispose();
            Assert.AreEqual(ct, 0);
        }

        [Test]
        public void SubscribeToNewKeyFirstPublishTest()
        {
            var d = new CompositeDisposable();
            var evt = new ManualResetEventSlim();
            var ct = 0;

            // ensure test key is missing
            var db = _redis.GetDatabase();
            db.KeyDelete("my-test-key");

            // subscribe
            var observable = _redis.GetObservable("my-test-key");
            d.Add(observable.Subscribe(v =>
            {
                Debug.WriteLine(v);
                ++ct;
                evt.Set();
            }));

            var observer = _redis.GetObserver("my-test-key");
            observer.OnNext("value is " + _random.Next(100));

            evt.Wait(TimeSpan.FromSeconds(5));
            d.Dispose();
            Assert.AreEqual(ct, 1);
        }
    }
}
