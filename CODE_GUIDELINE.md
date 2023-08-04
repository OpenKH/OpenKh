# Code guideline

## Do not re-invent the wheel

The main goal of the project is to have a central point for everything related to Kingdom Hearts. If you want to add functionality, do not start coding something unless you've checked and made sure it doesn't already exist. If you are not happy with the code, do not understand how you can use it, or if you want to improve upon it, check the [contributing guide](CONTRIBUTING.md) or ask for assistance from other maintainers.

## The power of refactoring

It is okay to not strictly respect the content below this document (however it is a best practice to follow it in order to achieve optimal and comformant code). Regardless of the technology, the programming language used, or the coding style, as long as something works, it is fine; it can be always optimized and refactored later if needed.

## Property of the code

Any portion of code committed to the repository belongs to OpenKH and its community and it is subject to the [specified license](LICENSE).

# Naming convention

Pascal case is used everywhere in the code. When it is using a short version of a name (E.g. from Kingdom Hearts to KH), only the first letter is capitalized (E.g. Kh).

## Libraries

Every library should start with `OpenKh.*`, where `*` is the short name of the game (E.g. `Kh1`, `KhBbs`, `KhReCom` etc.). If the content is more related to a specific engine, use the name of the engine instead (E.g. `Tokyo` for Kh1 and Kh2, `Osaka` for KhBbs and Kh3d, `Unreal` for Kh02 and Kh3). If the content is generic, it should be placed in `Common`, `Imaging`, `Messages` or other classes that defines a common group shared between games.

## Graphics tools

Every GUI tool should start with `OpenKh.Tools.*`, where `*` is a short name of the application. It is preferrable to use an appropriate short description at the end of the name to give an idea of the capability of the tool (E.g. `Viewer`, `Converter`, `Editor`). All the common logic for the GUI tools should be placed in `OpenKh.Tools.Common`

## Command tools

Every command line tool should start with `OpenKh.Command.*`, where `*` is a short name of the application. There are no particular restrictions about how to name a command line tool otherwise.

# Technology

## Programming language

Currently, the entire codebase is written in C# 7.0.

The framework used for the common libraries is .Net Standard 2.0, which is compatible with .Net Core 2.0, .Net Framework 4.6.1, Mono 5.4 and others.

The framework used for the tools is .Net Framework 4.7.1, which currently only works on Windows. There are plans to rectify this over time as the project progresses.

## Third party libraries

The main framework for GUI is `WPF`. For the rendering engine in the tools, `SharpDx` is currently used. `Monogame` will be preferred for the game engine.

# Coding style

## Casting

Avoid hard type casting like `(Foo)obj` since it can easily throw an `InvalidCastException` on runtime. Preferably, use `obj is Foo` to check if `obj` is or inhert the type `Foo`. Use `obj as Foo` to cast on type `Foo`, which returns `null` if `obj` is not or does not inhert `Foo`, instead of throwing a runtime exception.

To avoid reduntant code and use both `is` and `as` operators, it is possible to use the following statement:
```csharp
if (obj as Foo foo)
    foo.DoSomething();
else
    // do some failure logic
```

## Interfaces

Interfaces are a powerful instrument used for **inversion of control** and **contract** definition.

A good example is [`OpenKh.Imaging.IImage`](https://github.com/OpenKH/OpenKh/blob/master/OpenKh.Imaging/IImage.cs), which defines the minimum amount of information to represent an image. All the methods that accept `IImage` as a parameter can accept any image class that implements the `IImage` interface.

## Helper methods

C# comes with this excellent feature called [extension methods](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods) that allow the addition of any member to existing classes or interfaces.

Taking [`OpenKh.Imaging.IImageRead`](https://github.com/OpenKH/OpenKh/blob/master/OpenKh.Imaging/IImageRead.cs), which is a super set of `IImage`, the class [`OpenKh.Imaging.ImageHelpers`](https://github.com/OpenKH/OpenKh/blob/master/OpenKh.Imaging/ImageHelpers.cs) contains a bunch of methods that can be used with any ``IImageRead`.

If we take `public static void SaveImage(this IImageRead imageRead, string fileName)` as an example, it is possible to invoke `IImageRead.SaveImage(fileName)` without having that method implemented in any of the interface implementations. Now, all the classes that implement `IImageRead` can use `SaveImage` (E.g. `new Imgd(input).SaveImage(output)`).

## Lambda functions

In C# it is possible to use functions as first class citizens. This is really useful for injecting logic into a method.

E.g.

```csharp
var funcFilterBarFiles = fileName => Path.GetExtension(fileName) == ".bar";
var funcFilterBarWithImagesOnly = fileName => new Bar(fileName).Any(x => x.Type == ImageType);
var fileList = idx.GetFiles(condition ? funcFilterBarFiles : funcFilterBarWithImagesOnly);
[...]
IEnumerable<Entry> GetFiles(Func<string, bool> filter) =>
    entries.Where(filter ?? DefaultFilter);
```

## LINQ

This is one of the most powerful libraries of C# that can be used for free. With [LINQ](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/basic-linq-query-operations) it is possible to perform operations to list data in a SQL/NoSQL way, filtering, transforming, aggregating, and manipulating lists. It can completely replace loops and can dramatically reduce lines of code.

This is a portion of code using a common for a loop:

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

It is preferrable to use a functional approach, which removes the overuse of branch conditions, protective code, and null checking, while supplementing code readability.

The functional paradigm comes with the concept of immutable objects rather than classes full of conditional logic. Imagine the `string` or `DateTime` type in C#, where once the object is created it cannot be modified.

## Testing

Every line of code is supposed to be covered by unit testing. The approach used is TDD ([Test Driven Development](https://www.amazon.com/dp/0321146530)), where the tests are written before the implementation, as requirements or a checklist.

The advantage of this method is it allows us to be 100% sure that everything works as expected. This gives more confidence in the event of refactoring. Since tests are automated, they require only seconds to run.

## Conclusion

While it may be the best practice to follow these guidelines to make code more readable, easy to use, extensive, and portable, they're not necessarily strictly enforced! We recognize that everyone has their own coding style and preferences, so taking that into account is important for a project of this scope. As such, discussion and questions regarding someone's syntax in comparison to another's is perfectly fine as long as it remains civil and provides a good experience for everyone involved. [Built by a friendly community, for a friendly community!](CODE_OF_CONDUCT.md)
