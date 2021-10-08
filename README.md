# Muxy GameLink Unity Example

## Requirements

Before you get started you will want to register your extension in the [Muxy Dashboard](dev.muxy.io)

## Getting Started

Simply copy the GameLink folder into your new or existing Unity project, in the `Packages/` directory

GameLinkTestScene.unity shows example usages of GameLink. Be sure to set your GameLink ClientId in the CMuxyGameLink component.

## Documentation

Documentation for the GameLink C# library beyond example usages is currently non-existent, but you may refer to the [C++ documentation](dev.muxy.io/docs/api) for the time being as the two libraries are very close in usage. C# documentation is currently being worked on and will be coming soon enough. 

## Unity 2018 
This package does not work out-of-the box for Unity 2018 and older versions. To use this library, copy this repository into a folder in the `Packages/` folder
of your unity project, and then copy the correct dll (Likely `Runtime/x64/cgamelink.dll`) to the root of your project directory.

Overall, the resulting folder structure should look like:

```
Unity2018Project
|   Unity2018Project.sln
|   cgamelink.dll
+-- Assets
|   | ...
|
+-- Packages
|   | ...
|   +-- Gamelink
|   |   +-- GameLink~
|   |   +-- Runtime
|   |   +-- SampleScene
|   |   +-- Walkthrough~
```
 