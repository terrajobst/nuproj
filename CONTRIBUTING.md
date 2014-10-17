# How to contribute

We welcome contributions to NuProj! However, we do have some requests on how
contributions should be done. Please read them in order to avoid surprises
down the road.

## Choosing something to work on

The [issue tracker](https://github.com/terrajobst/nuproj/issues) has a list of
items that you can start working in:

* **Features**. Those are issues marked as 'enhancement'.

* **Bugs**. Those are issues marked as 'bug'.

In order to avoid overlap, it's a good idea to comment on the item and let
everybody know that you want to work on it.

## Creating a pull request

1. Make sure that there is a corresponding issue for your change first. If there
   is none, create one.
2. Create a fork in GitHub
3. Create a branch off the `master` branch. Name it something that that makes
   sense, such as `issue-123`.
4. Commit your changes and push your changes to GitHub
5. Create a pull request against our `master` branch

## DOs and DON'Ts

* **DO** follow our coding style (see below)
* **DON'T** surprise us with big pull requests. Instead, file an issue and start
  a discussion so we can agree on a direction before you invest a large amount
  of time.
* **DO** include tests when adding new features. When fixing bugs, start with
  adding a test that highlights how the current behavior is broken.
* **DO** keep the discussions focused. When a new or related topic comes up
  it's often better to create new issue than to side track the discussion.

## C# Coding Style

The general rule we follow is "use Visual Studio defaults".

1. We use Allman style braces
2. We use four spaces of indentation (no tabs)
3. We use "_camelCase" private members and use "readonly" where possible
4. We avoid `this.` unless absolutely necessary
5. We always specify the visiblity, even if it's the default (i.e.
   `private string _foo` not `string _foo`)
6. Namespace imports should be specified at the top of the file, *outside* of
   `namespace` declarations and should be sorted alphabetically, with `System.`
   namespaces at the top and blank lines between different top level groups

### Example File:

```C#
using System.Aardvarks;
using System.IO;
using System.Zebras;

using Microsoft.CoolStuff;
using Microsoft.CoolStuff.Build;

using Zebra.Crossing;

namespace NuProj.More.AndMore
{
    public class MyClass
    {
        private readonly ISomethingService _something;

        public MyClass(ISomethingService something)
        {
            _something = something;
        }

        public ISomethingService SomethingService
        {
            get { return _something; }
        }
    }
}
```

## MSBuild Coding Style

1. Use two spaces for indentation
2. Put XML attributes on separate lines
3. All names should be PascalCased (targets, properties, item lists)
4. Mark private properties and targets by prefixing them with an underscore.
   Declaring something as private means that we don't intend customers to
   override, depend, or use those targets or properties. In other words, we
   reserve the right to change them.
5. Add comments to targets so that it's easier to understand the flow

### Example File:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">

  <!-- Some properties -->

  <PropertyGroup>
    <Property>SomeValue</Property>
  </PropertyGroup>

  <!--
  ==============================================================================
  _SomePrivateTarget
  ==============================================================================

  This is a private target and is thus prefixed with an underscore.

  INPUTS:
    $(SomeProperty)         Explanation of what it controls in this target.
    @(SomeItemGroup)        Explanation of what it represents in this target.

  OUTPUTS:
    @(AnotherItemGroup)     Explanation of what it represents.

  ============================================================================== -->

  <Target Name="_NuProjGetProjectClosure">

    <!-- Run some task -->
    <SomeTask Property1="$(SomeProperty)"
              Property2="@(SomeItemGroup)">
        <Output TaskParameter="Property3"
                ItemName="AnotherItemGroup" />
    </SomeTask>

  </Target>

</Project>
```
