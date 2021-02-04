using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnoHKUtils.Tests
{
    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        public void TestObjectSerialization()
        {
            Debug.WriteLine(JsonConvert.SerializeObject("Test"));
            Debug.WriteLine(JsonConvert.SerializeObject(1234));
            Debug.WriteLine(JsonConvert.SerializeObject(1234f));
            Debug.WriteLine(JsonConvert.SerializeObject(1234.56));
            Debug.WriteLine(JsonConvert.SerializeObject(new { Name = "Yes", Num = new { Yes2 = "Yes2", Yes1 = "yes1" } } ));

            string customSerialized = JsonConvert.SerializeObject(new TestSObject());
            Debug.WriteLine(customSerialized);

            JObject customDeserialized = JsonConvert.DeserializeObject(customSerialized) as JObject;
            Debug.WriteLine(customDeserialized);
            Debug.WriteLine(customDeserialized["FirstProp"]);
        }

        public class TestSObject
        {
            public string FirstProp { get; } = "Nice!";
            public float SecondProp { get; } = 123.45f;

            public TestSubSObject ThirdProp { get; } = new();
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class TestSubSObject
        {
            [JsonProperty]
            public string SubFirstProp { get; } = "Works!";
            public string SubSecondProp { get; } = "and it's blowin' my mind!";
        }
    }
}
