using UnityEngine;
using System.Collections.Generic;

public class DefaultCreatures {

	// public static Dictionary<string, string> defaultCreatures = new Dictionary<string, string> {

	// 	{"HAILER", 
	// 		"1%-5.379086%7.60694%0\n2%3.863863%7.651207%0\n3%10.3837%14.37072%0\n4%-3.772128%10.28738%0\n5%-6.625983%10.56094%0\n--%%--\n6%5%1\n7%1%4\n8%4%5\n9%1%2\n10%2%3\n--%%--\n11%10%9\n13%7%9\n"
	// 	},
	// 	{"SPIDER",
	// 		"14%-0.08246826%12.23149%0\n15%-3.09183%8.529411%0\n16%3.112808%8.436859%0\n17%-4.063362%13.29584%0\n18%-7.998681%8.483135%0\n19%-8.091276%14.9155%0\n20%-13.50813%8.575687%0\n21%3.948339%13.71232%0\n22%8.257298%8.390583%0\n23%7.701725%15.37826%0\n24%12.70189%8.529411%0\n--%%--\n25%14%15\n26%15%16\n27%16%14\n28%16%21\n29%22%21\n30%16%23\n31%23%24\n32%15%17\n33%17%18\n34%15%19\n35%19%20\n--%%--\n36%35%34\n37%33%32\n38%34%25\n39%32%25\n40%28%27\n41%30%27\n42%28%29\n43%31%30\n"
	// 	},
	// 	{"SPRING",
	// 		"1%-0.4886592%8.154865%0\n2%3.443405%8.283751%0\n3%-0.4423616%11.45438%0\n4%4.215277%11.85731%0\n5%-0.2144896%15.11072%0\n--%%--\n6%1%2\n7%2%3\n8%3%4\n9%4%5\n--%%--\n10%6%7\n11%7%8\n12%8%9"
	// 	},
	// 	{"FROGGER",
	// 		"1%-4.274516%10.51822%0\n2%2.322201%12.90204%0\n3%1.039504%9.142942%0\n4%5.208263%6.484066%0\n5%-0.8845364%8.684515%0\n6%-7.523664%6.71328%0\n7%-2.530315%5.796426%0\n--%%--\n8%1%2\n9%2%3\n10%3%4\n11%5%1\n12%5%6\n13%6%7\n--%%--\n14%13%12\n15%11%12\n16%11%8\n17%8%9\n18%9%10"
	// 	},
	// 	{"ROO",
	// 		"54%2.447156%14.6516%0\n55%-6.772751%8.587149%0\n56%-6.500259%12.05461%0\n57%-0.322772%3.823653%0\n58%-5.938925%5.435742%0\n66%1.138665%18.46781%0\n67%6.024715%17.9294%0\n--%%--\n59%55%58\n60%58%57\n61%57%55\n63%55%56\n64%56%54\n65%54%55\n68%66%54\n69%54%67\n--%%--\n70%68%63\n71%63%69\n72%64%59\n73%64%61\n74%64%60\n75%60%63\n76%65%59\n77%65%61"
	// 	}
	// };

	public static Dictionary<string, string> defaultCreatures = new Dictionary<string, string> {

		{"FROGGER",
			"{\"name\":\"FROGGER\",\"joints\":[{\"id\":44,\"x\":-3.921333,\"y\":9.716,\"weight\":1},{\"id\":45,\"x\":3.105333,\"y\":11.144,\"weight\":1},{\"id\":47,\"x\":1.605333,\"y\":8.143333,\"weight\":1},{\"id\":48,\"x\":6.02,\"y\":6.609667,\"weight\":1},{\"id\":49,\"x\":-1.032,\"y\":7.785,\"weight\":1},{\"id\":50,\"x\":-5.403667,\"y\":6.007667,\"weight\":1},{\"id\":62,\"x\":-2.399238,\"y\":5.084381,\"weight\":1}],\"bones\":[{\"id\":52,\"startJointID\":49,\"endJointID\":44,\"weight\":1,\"legacy\":false},{\"id\":53,\"startJointID\":49,\"endJointID\":50,\"weight\":1,\"legacy\":false},{\"id\":54,\"startJointID\":44,\"endJointID\":45,\"weight\":1,\"legacy\":false},{\"id\":55,\"startJointID\":45,\"endJointID\":47,\"weight\":1,\"legacy\":false},{\"id\":56,\"startJointID\":48,\"endJointID\":47,\"weight\":1,\"legacy\":false},{\"id\":63,\"startJointID\":62,\"endJointID\":50,\"weight\":1,\"legacy\":false}],\"muscles\":[{\"id\":57,\"startBoneID\":55,\"endBoneID\":56,\"strength\":1500,\"canExpand\":true},{\"id\":58,\"startBoneID\":55,\"endBoneID\":54,\"strength\":1500,\"canExpand\":true},{\"id\":59,\"startBoneID\":52,\"endBoneID\":54,\"strength\":1500,\"canExpand\":true},{\"id\":60,\"startBoneID\":52,\"endBoneID\":53,\"strength\":1500,\"canExpand\":true},{\"id\":64,\"startBoneID\":63,\"endBoneID\":53,\"strength\":1500,\"canExpand\":true}]}"
		},
		{"ROO",
			"{\"name\":\"ROO\",\"joints\":[{\"id\":54,\"x\":2.447156,\"y\":14.6516,\"weight\":1},{\"id\":55,\"x\":-6.772751,\"y\":8.587149,\"weight\":1},{\"id\":56,\"x\":-6.500259,\"y\":12.05461,\"weight\":1},{\"id\":57,\"x\":-0.322772,\"y\":3.823653,\"weight\":1},{\"id\":58,\"x\":-5.938925,\"y\":5.435742,\"weight\":1},{\"id\":66,\"x\":1.138665,\"y\":18.46781,\"weight\":1},{\"id\":67,\"x\":6.024715,\"y\":17.9294,\"weight\":1}],\"bones\":[{\"id\":59,\"startJointID\":55,\"endJointID\":58,\"weight\":1,\"legacy\":false},{\"id\":60,\"startJointID\":58,\"endJointID\":57,\"weight\":1,\"legacy\":false},{\"id\":61,\"startJointID\":57,\"endJointID\":55,\"weight\":1,\"legacy\":false},{\"id\":63,\"startJointID\":55,\"endJointID\":56,\"weight\":1,\"legacy\":false},{\"id\":64,\"startJointID\":56,\"endJointID\":54,\"weight\":1,\"legacy\":false},{\"id\":65,\"startJointID\":54,\"endJointID\":55,\"weight\":1,\"legacy\":false},{\"id\":68,\"startJointID\":66,\"endJointID\":54,\"weight\":1,\"legacy\":false},{\"id\":69,\"startJointID\":54,\"endJointID\":67,\"weight\":1,\"legacy\":false}],\"muscles\":[{\"id\":70,\"startBoneID\":68,\"endBoneID\":63,\"strength\":1500,\"canExpand\":true},{\"id\":71,\"startBoneID\":63,\"endBoneID\":69,\"strength\":1500,\"canExpand\":true},{\"id\":72,\"startBoneID\":64,\"endBoneID\":59,\"strength\":1500,\"canExpand\":true},{\"id\":73,\"startBoneID\":64,\"endBoneID\":61,\"strength\":1500,\"canExpand\":true},{\"id\":74,\"startBoneID\":64,\"endBoneID\":60,\"strength\":1500,\"canExpand\":true},{\"id\":75,\"startBoneID\":60,\"endBoneID\":63,\"strength\":1500,\"canExpand\":true},{\"id\":76,\"startBoneID\":65,\"endBoneID\":59,\"strength\":1500,\"canExpand\":true},{\"id\":77,\"startBoneID\":65,\"endBoneID\":61,\"strength\":1500,\"canExpand\":true}]}"
		},
		{"HAILER", 
			"{\"name\":\"HAILER\",\"joints\":[{\"id\":1,\"x\":-5.379086,\"y\":7.60694,\"weight\":1},{\"id\":2,\"x\":3.863863,\"y\":7.651207,\"weight\":1},{\"id\":3,\"x\":10.3837,\"y\":14.37072,\"weight\":1},{\"id\":4,\"x\":-3.772128,\"y\":10.28738,\"weight\":1},{\"id\":5,\"x\":-6.625983,\"y\":10.56094,\"weight\":1}],\"bones\":[{\"id\":6,\"startJointID\":5,\"endJointID\":1,\"weight\":1,\"legacy\":false},{\"id\":7,\"startJointID\":1,\"endJointID\":4,\"weight\":1,\"legacy\":false},{\"id\":8,\"startJointID\":4,\"endJointID\":5,\"weight\":1,\"legacy\":false},{\"id\":9,\"startJointID\":1,\"endJointID\":2,\"weight\":1,\"legacy\":false},{\"id\":10,\"startJointID\":2,\"endJointID\":3,\"weight\":1,\"legacy\":false}],\"muscles\":[{\"id\":11,\"startBoneID\":10,\"endBoneID\":9,\"strength\":1500,\"canExpand\":true},{\"id\":13,\"startBoneID\":7,\"endBoneID\":9,\"strength\":1500,\"canExpand\":true}]}"
		},
		{"SPIDER",
			"{\"name\":\"SPIDER\",\"joints\":[{\"id\":14,\"x\":-0.08246826,\"y\":12.23149,\"weight\":1},{\"id\":15,\"x\":-3.09183,\"y\":8.529411,\"weight\":1},{\"id\":16,\"x\":3.112808,\"y\":8.436859,\"weight\":1},{\"id\":17,\"x\":-4.063362,\"y\":13.29584,\"weight\":1},{\"id\":18,\"x\":-7.998681,\"y\":8.483135,\"weight\":1},{\"id\":19,\"x\":-8.091276,\"y\":14.9155,\"weight\":1},{\"id\":20,\"x\":-13.50813,\"y\":8.575687,\"weight\":1},{\"id\":21,\"x\":3.948339,\"y\":13.71232,\"weight\":1},{\"id\":22,\"x\":8.257298,\"y\":8.390583,\"weight\":1},{\"id\":23,\"x\":7.701725,\"y\":15.37826,\"weight\":1},{\"id\":24,\"x\":12.70189,\"y\":8.529411,\"weight\":1}],\"bones\":[{\"id\":25,\"startJointID\":14,\"endJointID\":15,\"weight\":1,\"legacy\":false},{\"id\":26,\"startJointID\":15,\"endJointID\":16,\"weight\":1,\"legacy\":false},{\"id\":27,\"startJointID\":16,\"endJointID\":14,\"weight\":1,\"legacy\":false},{\"id\":28,\"startJointID\":16,\"endJointID\":21,\"weight\":1,\"legacy\":false},{\"id\":29,\"startJointID\":22,\"endJointID\":21,\"weight\":1,\"legacy\":false},{\"id\":30,\"startJointID\":16,\"endJointID\":23,\"weight\":1,\"legacy\":false},{\"id\":31,\"startJointID\":23,\"endJointID\":24,\"weight\":1,\"legacy\":false},{\"id\":32,\"startJointID\":15,\"endJointID\":17,\"weight\":1,\"legacy\":false},{\"id\":33,\"startJointID\":17,\"endJointID\":18,\"weight\":1,\"legacy\":false},{\"id\":34,\"startJointID\":15,\"endJointID\":19,\"weight\":1,\"legacy\":false},{\"id\":35,\"startJointID\":19,\"endJointID\":20,\"weight\":1,\"legacy\":false}],\"muscles\":[{\"id\":36,\"startBoneID\":35,\"endBoneID\":34,\"strength\":1500,\"canExpand\":true},{\"id\":37,\"startBoneID\":33,\"endBoneID\":32,\"strength\":1500,\"canExpand\":true},{\"id\":38,\"startBoneID\":34,\"endBoneID\":25,\"strength\":1500,\"canExpand\":true},{\"id\":39,\"startBoneID\":32,\"endBoneID\":25,\"strength\":1500,\"canExpand\":true},{\"id\":40,\"startBoneID\":28,\"endBoneID\":27,\"strength\":1500,\"canExpand\":true},{\"id\":41,\"startBoneID\":30,\"endBoneID\":27,\"strength\":1500,\"canExpand\":true},{\"id\":42,\"startBoneID\":28,\"endBoneID\":29,\"strength\":1500,\"canExpand\":true},{\"id\":43,\"startBoneID\":31,\"endBoneID\":30,\"strength\":1500,\"canExpand\":true}]}"
		},
		{"SPRING",
			"{\"name\":\"SPRING\",\"joints\":[{\"id\":1,\"x\":-0.4886592,\"y\":8.154865,\"weight\":1},{\"id\":2,\"x\":3.443405,\"y\":8.283751,\"weight\":1},{\"id\":3,\"x\":-0.4423616,\"y\":11.45438,\"weight\":1},{\"id\":4,\"x\":4.215277,\"y\":11.85731,\"weight\":1},{\"id\":5,\"x\":-0.2144896,\"y\":15.11072,\"weight\":1}],\"bones\":[{\"id\":6,\"startJointID\":1,\"endJointID\":2,\"weight\":1,\"legacy\":false},{\"id\":7,\"startJointID\":2,\"endJointID\":3,\"weight\":1,\"legacy\":false},{\"id\":8,\"startJointID\":3,\"endJointID\":4,\"weight\":1,\"legacy\":false},{\"id\":9,\"startJointID\":4,\"endJointID\":5,\"weight\":1,\"legacy\":false}],\"muscles\":[{\"id\":10,\"startBoneID\":6,\"endBoneID\":7,\"strength\":1500,\"canExpand\":true},{\"id\":11,\"startBoneID\":7,\"endBoneID\":8,\"strength\":1500,\"canExpand\":true},{\"id\":12,\"startBoneID\":8,\"endBoneID\":9,\"strength\":1500,\"canExpand\":true}]}"
		}
	};

	// public static List<CreatureDesign> defaultCreatures2 = new List<CreatureDesign> {
		
	// 	new CreatureDesign("Frogger", new List<JointData> {
	// 		new JointData(44, new Vector2(-3.921333f, 9.716f), 1f),
	// 		new JointData(45, new Vector2(3.105333f, 11.144f), 1f),
	// 		new JointData(47, new Vector2(1.605333f, 8.143333f), 1f),
	// 		new JointData(48, new Vector2(6.02f, 6.609667f), 1f),
	// 		new JointData(49, new Vector2(-1.032f, 7.785f), 1f),
	// 		new JointData(50, new Vector2(-5.403667f, 6.007667f), 1f),
	// 		new JointData(62, new Vector2(-2.399238f, 5.084381f), 1f),
	// 	}, new List<BoneData> {
	// 		new BoneData(52, 49, 44, 1f),
	// 		new BoneData(53, 49, 50, 1f),
	// 		new BoneData(54, 44, 45, 1f),
	// 		new BoneData(55, 45, 47, 1f),
	// 		new BoneData(56, 48, 47, 1f),
	// 		new BoneData(63, 62, 50, 1f),
	// 	}, new List<MuscleData> {
	// 		new MuscleData(57, 55, 56, Muscle.Defaults.MaxForce, true),
	// 		new MuscleData(58, 55, 54, Muscle.Defaults.MaxForce, true),
	// 		new MuscleData(59, 52, 54, Muscle.Defaults.MaxForce, true),
	// 		new MuscleData(60, 52, 53, Muscle.Defaults.MaxForce, true),
	// 		new MuscleData(64, 63, 53, Muscle.Defaults.MaxForce, true)
	// 	}),
	// };
}