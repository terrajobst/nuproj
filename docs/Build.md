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

Further more you need to install [NuGet.Common package that provides additional targets](http://www.nuget.org/packages/NuProj.Common).
This package needs to be installed to all non-NuProj projects that are directly or indirectly referenced by NuProj project.