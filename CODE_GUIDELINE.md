# Code guideline

## Do not re-invent the wheel

The main goal of the project is to have a central point for everything related to Kingdom Hearts. If you want to use a functionality, do not start to code it before to check if it already exists. If you are not happy with the code, you do not understand how you can re-use it or if you want to improve it, check the [contributing guide](CONTRIBUTING.md).

## The power of refactoring

It is okay to not strictly respect the content below this document (however it is a best practice to follow it in order to achieve optimal and comformant code). Regardless the technology, the programming language used or the coding style, as long as something works it is fine; it can be always optimized and refactored later.

## Property of the code

Any portion of code committed to the repository belong to OpenKH and its community and it is subject to the specified [license](LICENSE).

# Naming convention

Pascal case is used everywhere in the code. When it is using a short version of a name (eg. from Kingdom Hearts to KH), only the first letter is capitalized (eg. Kh).

## Libraries

Every library should start with `OpenKh.*`, where `*` is the short name of the game (eg. `Kh1`, `KhBbs`, `KhReCom` etc.). If the content is more related to a specific engine, use the name of the engine instead (eg. `Osaka` for KhBbs and Kh3d, `Unreal` for Kh02 and Kh3). If the content is generic, it has to be placed in `Common`, `Imaging`, `Messages` or other classes that defines a common group.

## Graphics tools

Every GUI tool should start with `OpenKh.Tools.*`, where `*` is a short name of the application. It is preferrable to use a verb at the end of the name to give an idea of the capability of the tool (eg. `Viewer`, `Converter`, `Editor`). All the common logic for the GUI tools should be placed in `OpenKh.Tools.Common`

## Command tools

Every command line tool should start with `OpenKh.Command.*`, where `*` is a short name of the application. There are no particular restrictions about how to name a command line tool.

# Technology

## Programming language

Right now, all the code base is written in C# 7.0.

The framework used for the common libraries is .Net Standard 2.0, which is compatible with .Net Core 2.0, .Net Framework 4.6.1, Mono 5.4 and others.

The framework used for the tools is .Net Framework 4.7.1, which currently works only on Windows.

## Third party libraries

The main framework for GUI is `WPF`. For the rendering engine in the tools, `SharpDx` is currently used. As a game engine it is preferrable to use `MonoGame`.

# Coding style

## Casting

Avoid hard type casting like `(Foo)obj` since it can easily throw a `InvalidCastException` on runtime. Prefer to use `obj is Foo` to check if `obj` is or inhert the type `Foo`. Use `obj as Foo` to cast on type `Foo`, which returns `null` if `obj` is not or does not inhert `Foo`, instead of throwing a runtime exception.

To avoid reduntant code and use both `is` and `as` operators, it is possible to use the following statement:
```csharp
if (obj as Foo foo)
    foo.DoSomething();
else
    // do some failure logic
```

## Interfaces

They are a powerful instrument used for **inversion of control** and **contract** definition.

A good example is [`OpenKh.Imaging.IImage`](https://github.com/Xeeynamo/OpenKh/blob/master/OpenKh.Imaging/IImage.cs), which defines the minimum amount of information to represent an image. All the methods that accepts a `IImage` as parameter can accept any image class that implements `IImage` interface.

## Helper methods

C# comes with this excellent feature called [extension methods](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods) that allows to add any member to existing class or interfaces.

Taking [`OpenKh.Imaging.IImageRead`](https://github.com/Xeeynamo/OpenKh/blob/master/OpenKh.Imaging/IImageRead.cs), which is a super set of `IImage`, the class [`OpenKh.Imaging.ImageHelpers`](https://github.com/Xeeynamo/OpenKh/blob/master/OpenKh.Imaging/ImageHelpers.cs) contains a bunch of methods that can be used with any ``IImageRead`.

If we take `public static void SaveImage(this IImageRead imageRead, string fileName)` as an example, it is possible to invoke `IImageRead.SaveImage(fileName)` without having that method implemented in any of the interface implementations. Now, all the classes that implements `IImageRead` can use `SaveImage` (eg. `new Imgd(input).SaveImage(output)`).

## Lambda functions

In C# it is possible to use functions as first class citizens. This becomes really useful to inject some logic into a method.

Eg.

```csharp
var funcFilterBarFiles = fileName => Path.GetExtension(fileName) == ".bar";
var funcFilterBarWithImagesOnly = fileName => new Bar(fileName).Any(x => x.Type == ImageType);
var fileList = idx.GetFiles(condition ? funcFilterBarFiles : funcFilterBarWithImagesOnly);
[...]
IEnumerable<Entry> GetFiles(Func<string, bool> filter) =>
    entries.Where(filter ?? DefaultFilter);
```

## LINQ

This is one of the most powerful libraries of C# that comes for free. With [LINQ](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/basic-linq-query-operations) it is possible to perform operations to list of data in a SQL/NoSQL way, filtering, transforming, aggregating and manipulating lists. It can completely replace for loops and can dramatically reduces lines of code.

Eg. This is a portion of code using a common for loop:

```csharp
List<Foo> GetList(Entry[] entries)
{
    List<Foo> list = new List<Foo>();
    for (int i = 0; i < entries.Count; i++)
    {
        if (entries[i].Length > 0)
            list.Add(new Foo(entries[i]));
    }
    return list;
}
```

This is the same portion of code, but using LINQ:

```csharp
IEnumerable<Foo> GetList(IEnumerable<Entries> entries) => entries
    .Where(entry => entry.Length > 0)
    .Select(entry => new Foo(entry));
```

## Paradigm

It is preferrable to use a functional approach, which removes the over usage of branch conditions, protective code and null checking, while adding code readability.

The functional paradigm comes with the concept of immutable objects rather than classes full of conditional logic. Imagine that as the `string` or `DateTime` type in C#, where once the object is created it cannot be modified.

## Testing

Every line of code is supposed to be covered by unit testing. The approach used is TDD ([Test Driven Development](https://www.amazon.com/dp/0321146530)), where the tests are written before the implementation, as requirement or check list.

The advantage of it is to be 100% sure that everything is working as expected, giving more confidence when refactoring. Since the tests are automated, they just requires few seconds to run.