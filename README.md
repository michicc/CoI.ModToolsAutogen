# ModToolsAutogen mod for Captain of Industry

This is a mod for Captain of Industry (COI) that aims to improve moding by allowing access to several internal helpers used for auto-generating assets.

It provides several in-game console commands listed below.

## How to use

The following commands are implemented. Arguments in <> are optional.

- `generate_layout_entity_icons <idSubstring> <pitchDegrees> <yawDegrees> <fovDegrees>`: Generate layout entity icons shown in the build menu or research tree. Works similar to `generate_layout_entity_mesh_templates`.
	- `idSubstring`: If set, icons will only be generated for entities with a matching proto if. If empty, all icons will be generated.
	- `pitchDegrees`: Camera pitch. Default values is 35°.
	- `yawDegrees`: Camera yaw. Default values is 120° unless the Proto has `YawForGeneratedIcon` set.
	- `fovDegrees`: Camera field-of-view. Default values is 20°.

## How to compile

A normal Visual Studio solution and .csproj file are provided.

1. Before you can compile the mod, you need to provide the path of your COI installation directory. You can get this path from the Steam client via `Properties...` -> `Local Files` -> `Browse` (a typical install path might be `C:\Program Files (x86)\Steam\steamapps\common\Captain of Industry`).
2. Make a copy of the file `Options.user.example` in your clone and rename the copy to `Options.user`.
3. Open `Options.user` in a text editor and change the path to match your system.
4. You should now be able to build the solution.

In `Options.user` you can also configure some more build options.
If `CopyToModDirectory` is set to the default of `true`, the build output will be automatically copied to the COI mod directory in app data.
