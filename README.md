# JsonParser 
[![Build status](https://ci.appveyor.com/api/projects/status/gvgvskk5jxfri1bo?svg=true)](https://ci.appveyor.com/project/lucasteles/jsonparser) [![Source Browser](https://img.shields.io/badge/Browse-Source-green.svg)](http://sourcebrowser.io/Browse/lucasteles/JsonParser/)

 JSON parser in C#


 - **Online test [HERE!!](http://lucasteles.github.io/JsonParser)**
 
## How to use

Validate only
```sh
$ JsonParser <file.json>
```

Generate C# equivalent (Mock)
```sh
$ JsonParser <file.json> -o <out.cs>
```

##Example

On file
```js
[
{
	"Name":"Rodrigo",
	"Idade":26,
	"DtNasc":"22/01/89",
	"values":[1,2,3,4,5]
},
{
	"Name":"Thais",
	"Idade":23,
	"DtNasc":"31/01/94"
},
{
	"Name":"Lucas",
	"Idade":24,
	"DtNasc":"11/12/91"
},
{
	"Name":"Alessa",
	"Idade":2,
	"DtNasc":"10/03/2010"
}
]
```

Generates this
```cs
var data = new List < object > () {
	new Dictionary < string, object > () {
		{
			"Name", "Rodrigo"
		}, {
			"Idade", 26
		}, {
			"DtNasc", "22/01/89"
		}, {
			"valores", new List < object > () {
				1, 2, 3, 4, 5
			}
		}
	}, new Dictionary < string, object > () {
		{
			"Name", "Thais"
		}, {
			"Idade", 23
		}, {
			"DtNasc", "31/01/94"
		}
	}, new Dictionary < string, object > () {
		{
			"Name", "Lucas"
		}, {
			"Idade", 24
		}, {
			"DtNasc", "11/12/91"
		}
	}, new Dictionary < string, object > () {
		{
			"Name", "Alessa"
		}, {
			"Idade", 2
		}, {
			"DtNasc", "10/03/2010"
		}
	}
};
```

Thanks
