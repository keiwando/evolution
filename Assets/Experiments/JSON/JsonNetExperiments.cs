// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Newtonsoft.Json;
// using Newtonsoft.Json.Linq;

// namespace Keiwando.Experiments {

//     public class JsonNetExperiments : MonoBehaviour {

//         private class Data {
//             public int ID { get; set; }
//             public string Name { get; set; } = "Unnamed";
//         }

//         private class Container {
//             public int ID { get; set; }
//             public List<Data> items { get; set; }
//         }

//         void Start() {
//             // TestDefaultValuesWhenMissing();
//             // TestRenamedPropertyDeserialization();
//             // TestPartialDecode();
//             // TestDetailedEncoding();
//             TestNestedObjects();
//         }

//         private void TestDetailedEncoding() {

//             var item = new JObject();
//             item["ID"] = 5;
//             item["Name"] = "Keiwan";

//             var items = new List<JObject>() { item };
//             var json = new JObject();
//             json["ID"] = 10;
//             json["items"] = JToken.FromObject(items);

//             Debug.Log(json.ToString());
//         }

//         private void TestNestedObjects() {

//             var item = new JObject();
//             item["ID"] = 5;
//             item["Name"] = "Keiwan";

//             var child = new JObject();
//             child["X"] = JToken.FromObject(new string[] { "A", "B", "C" });
//             child["Y"] = 10;
//             item["child"] = child;

//             Debug.Log(item.ToString());
//         }

//         private void TestPartialDecode() {

//             var encoded = @"
//             {
//                 'ID': 1,
//                 'items': [
//                     {
//                         'ID': 5,
//                         'Name': 'Keiwan'
//                     },
//                     {
//                         'ID': 100,
//                         'Name': 'Unnamed',
//                         'additionalData': 'some string'
//                     }
//                 ]
//             }";

//             var json = JObject.Parse(encoded);

//             var id = json["ID"];
//             var items = json["items"].ToObject<List<JObject>>();
            
//             Debug.Log(string.Format("ID: {0}", id.ToObject<int>()));
//             foreach (var item in items) {
//                 Debug.Log(string.Format("item: {0}", item.ToString()));
//             }
//         }

//         private void TestRenamedPropertyDeserialization() {

//             // encoded has been created from a previous version of the Data
//             // class, which had the Property 'name' instead of 'Name'.
//             var encoded = @"
//             {
//                 'ID': 5,
//                 '_name': 'Keiwan'
//             }";

//             // When decoding it, we want the serialized value for 'name'
//             // to be used as the value of 'Name'

//             var jObject = JObject.Parse(encoded);
//             var id = jObject["ID"];
//             var name = jObject["Name"] ?? jObject["_name"];
//             var data = new Data() { ID = id.ToObject<int>(), Name = name.ToString() };

//             Debug.Log(string.Format("ID: {0}", data.ID));
//             Debug.Log(string.Format("Name: {0}", data.Name));
//         }

//         private void TestDefaultValuesWhenMissing() {

//             // encoded has been created from a previous version of the Data
//             // class, which was missing the Name property.
//             var encoded = @"
//             {
//                 'ID': 5  
//             }";

//             // When decoding it, we want the default value of Name to be assigned
//             // in the decoded Data instance

//             var data = JsonConvert.DeserializeObject<Data>(encoded);

//             Debug.Log(string.Format("ID: {0}", data.ID));
//             Debug.Log(string.Format("Name: {0}", data.Name));

//             // Prints
//             // ID: 5
//             // Name: Unnamed
//         }
//     }
// }
