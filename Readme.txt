Salar.Bois is a fast, light and powerful binary serializer for .NET Framework.
With Bois you can serialize your existing objects with almost no change.

BOIS can serialize almost anything as long as they satisfy these conditions.

* Having parameter-less public constructor.
* Polymorphous properties have limited support, which only base type mentioned in the property will be serialized/deserialized.
* Collections/Lists should be generic and implement either of IList<> or IDictionary<>

Some classes have special support, which are listed below:

* NameValueCollection, ObservableCollection<>
* Version, Color and Guid