﻿using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using Kucoin.Net.Objects;
using Kucoin.Net.UnitTests.TestImplementations;

namespace Kucoin.Net.UnitTests
{
    [TestFixture]
    public class KucoinClientTests
    {
        [TestCase()]
        public void TestConversions()
        {
            var ignoreMethods = new []{"GetServerTime", "GetFiatPrices"};
            var defaultParameterValues = new Dictionary<string, object>
            {
                { "pageSize", 10 },
                { "funds", null },
                { "limit", 20 }
            };

            var methods = typeof(KucoinClient).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            var callResultMethods = methods.Where(m => m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(WebCallResult<>));
            foreach (var method in callResultMethods)
            {
                if (ignoreMethods.Contains(method.Name))
                    continue;

                var expectedType = method.ReturnType.GetGenericArguments()[0];
                var expected = typeof(TestHelpers).GetMethod("CreateObjectWithTestParameters").MakeGenericMethod(expectedType).Invoke(null, null);
                var parameters = TestHelpers.CreateParametersForMethod(method, defaultParameterValues);
                var client = TestHelpers.CreateResponseClient(SerializeExpected(expected), new KucoinClientOptions(){ ApiCredentials = new KucoinApiCredentials("Test", "Test", "Test") });

                // act
                var result = method.Invoke(client, parameters);
                var callResult = result.GetType().GetProperty("Success").GetValue(result);
                var data = result.GetType().GetProperty("Data").GetValue(result);

                // assert
                Assert.AreEqual(true, callResult);
                Assert.IsTrue(TestHelpers.AreEqual(expected, data), method.Name);
            }
        }

        public string SerializeExpected<T>(T data)
        {
            var result = new KucoinResult<T>()
            {
                Code = 200000,
                Data = data,
                Message = null
            };

            return JsonConvert.SerializeObject(result);
        }
    }
}
