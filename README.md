# JsonValidator
Analisador sintático de JSON em C#

## How to use

Validate only
```sh
$ JsonValidator <file.json>
```

Generate C# equivalent (Mock)
```sh
$ JsonValidator <file.json> -o <out.cs>
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

Generate
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
