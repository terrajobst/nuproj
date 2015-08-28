# Checked-in Toolkit

On the build machine, you shouldn't install NuProj. Instead, you should restore
the [NuGet package that provides the build server support](http://www.nuget.org/packages/NuProj).

In order for MSBuild to find the `NuProj.targets` that NuProj files depend on
you need to change the `NuProjPath` property in your .nuproj file:

```xml
<PropertyGroup>
	<NuProjPath Condition=" '$(NuProjPath)' == '' ">..\packages\NuProj.[Version]\</NuProjPath>
</PropertyGroup>
```
