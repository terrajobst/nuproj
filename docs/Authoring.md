# Authoring

## Metadata

Most of the metadata is represented via simple MSBuild properties:

```xml
<PropertyGroup>
  <Id>HelloWorld</Id>
  <Version>1.0.0</Version>
  <Title>HelloWorld</Title>
  <Authors>The World</Authors>
  <Owners>$(Authors)</Owners>
  <Description>Hello World</Description>
  <!-- Optional information:
  <ReleaseNotes></ReleaseNotes>
  <Summary>Hello World</Summary>
  <ProjectUrl>http://example.com</ProjectUrl>
  <LicenseUrl>http://example.com/license</LicenseUrl>
  <Copyright>Copyright (c)</Copyright>
  <RequireLicenseAcceptance>False</RequireLicenseAcceptance>
  <DevelopmentDependency>False</DevelopmentDependency>
  <Tags>HelloWorld</Tags>
  -->
</PropertyGroup>
```

This allows defining the metadata in terms of other MSBuild properties (such as
`Owners` is simply defined via the `Authors` property). Also the, properties
can be defined via command line arguments passed to MSBuild.

## Dependencies

Package dependencies are expressed via MSBuild item groups:

```xml
<ItemGroup>
  <Dependency Include="RouteMagic">
    <Version>1.1.6</Version>
  </Dependency>
</ItemGroup>
```

The package ID is passed via the standard `Include` attribute. The version is
specified as a custom metadata element within the item. Also, you can indicate
that the dependency is only there when targeting a certain framework:

```xml
<ItemGroup>
  <Dependency Include="RouteMagic">
    <Version>1.1.6</Version>
    <TargetFramework>net40</TargetFramework>
  </Dependency>
</ItemGroup>
```

Packages marked as development dependencies will be ignored.

```xml
<ItemGroup>
  <Dependency Include="Fody">
    <Version>1.25.0</Version>
    <DevelopmentDependency>true</DevelopmentDependency>
  </Dependency>
</ItemGroup>
```

## Framework Assemblies

NuGet supports adding references to framework assemblies as well. You can
specify those via the `FrameworkReference` item:

```xml
<ItemGroup>
  <FrameworkReference Include="System.dll" />
  <FrameworkReference Include="System.Core.dll" />
</ItemGroup>
```

As with dependencies, those can be specific to a certain target framework:

```xml
<ItemGroup>
  <FrameworkReference Include="System.dll" />
  <FrameworkReference Include="System.Core.dll">
    <TargetFramework>net40</TargetFramework>
  </FrameworkReference>
</ItemGroup>
```

## Packaging Files

Packaging files is done as follows:

```xml
<ItemGroup>
  <!-- Package readme.txt nuproj folder. -->
  <Content Include="readme.txt" />
  <!-- Package all .css files in content\css directory. -->
  <Content Include="content\css\**\*.css" />
  <!-- You can also add library this way. -->
  <Content Include="lib\net40\HelloWorld.dll" />
</ItemGroup>
```

The path of file is relative to NuProj project file. The files must be in 
project directory or its subdirectory. You can include files outside project 
directory by using `Link` metadata to specify file path in output package.

```xml
<ItemGroup>
  <!-- Package icon.png. -->
  <Content Include="..\common\icon.png">
    <Link>icon.png</Link>
  </Content>
</ItemGroup>
```

## Using Project References

You can reference other projects to automatically include their output in 
package. By default the project is considered to be a library and will be
put into `lib` folder. The package directory can be controlled by using 
`PackageDirectory` metadata. Recognized values are: `Lib`, `Tools`, `Build`,
`Content` and `Root`. 

```xml
<ItemGroup>
  <!-- Package output of Library.csproj to lib directory. -->
  <ProjectReference Include="..\Library\Library.csproj" />
  <!-- Package output of Tools.csproj to tools directory. -->
  <ProjectReference Include="..\Tools\Tools.csproj">
    <PackageDirectory>Tools</PackageDirectory>
  </ProjectReference>
  <!-- Package output of Tasks.csproj to build directory. -->
  <ProjectReference Include="..\Tasks\Tasks.csproj">
    <PackageDirectory>Build</PackageDirectory>
  </ProjectReference>
</ItemGroup>
```

You can also reference other NuProj projects to generate package dependencies.
Content of `Lib` directory of `Dependency` package will be removed from `Lib` 
directory of dependent package. `Dependency` package will be added as a NuGet 
dependency.

```xml
<ItemGroup>
  <!-- Add NuGet dependency. -->
  <ProjectReference Include="..\Dependency\Dependency.nuproj" />
</ItemGroup>
```

## Controlling Library References

For a given a target platform, NuGet will add references to all the libraries in
the corresponding lib folder. You can override this behavior by using the
`Reference` item:

```xml
<ItemGroup>
  <Reference Include="HelloWorld.dll" />
</ItemGroup>
```

Please note that the reference item doesn't support the `TargetFramework`
metadata. Instead, NuGet will only add the references if the file is also
packaged in the corresponding lib folder. If it's not the reference is simply
ignored. In other words, the target framework is already controlled by the fact
that the file might or might not be in the corresponding lib folder.

## Merging Nuspec Template

In addition to using MSBuild properties and items, you can use existing nuspec
file as `NuSpecTemplate` property. The file will be used to populate default 
values. Specified MSBuild properties will overwrite the properties. MSBuild
items will be added to files, framework assemblies, references or dependencies.

```xml
<PropertyGroup>
  <Id>HelloWorld</Id>
  <Version>1.0.0</Version>
  <Title>HelloWorld</Title>
  <Authors>The World</Authors>
  <Owners>$(Authors)</Owners>
  <Description>Hello World</Description>
  <NuSpecTemplate>HelloWorldTemplate.nuspec</NuSpecTemplate>
</PropertyGroup>
```
