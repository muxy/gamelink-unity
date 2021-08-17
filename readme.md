# Muxy GameLink Unity Example

## Requirements

Before you get started you will want to register your extension in the [Muxy Dashboard](dev.muxy.io)

## Getting Started

Simply copy the GameLink folder into your new or existing Unity project.

You will need your own JSON library and Websocket library to properly use GameLink.

GameLinkTestScene.unity shows example usages of GameLink. NativeWebsocket was used for the example, so to properly run the test scene download the NativeWebSocket from the Unity Store or at (https://github.com/endel/NativeWebSocket). Be sure to also set your GameLink ClientId in the CMuxyGameLink component.

## Documentation

Documentation for the GameLink C# library beyond example usages is currently non-existent, but you may refer to the [C++ documentation](dev.muxy.io/docs/api) for the time being as the two libraries are very close in usage. C# documentation is currently being worked on and will be coming soon enough. 