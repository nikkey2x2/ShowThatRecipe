
## Description
ShowThatRecipe is a mod that simply allows you to click a good to show it on a recipe popup
### Installation
1. Install [BepInEx](https://github.com/BepInEx/BepInEx)
2. Download `ShowThatRecipe.dll` from [releases page](https://github.com/nikkey2x2/ShowThatRecipe/releases/latest)
3. Drop `ShowThatRecipe.dll` into `BepInEx/plugins` directory
### Currently implemented
* Building construction materials
* Gathering huts
* Farms
* Hearth fuel slots
* Mine
* Geyser pump
* Rain collector
* Storage (only while in list mode)
* Resource deposits
### TODO
* Woodcutter -> this is probably just an internal storage.
* Static GoodSetSlot - a single good (with amount) you see on glade events. Surprisingly tough to add a button to.
* Dynamic GoodSetSlot - multiple ingredients you can choose from a radial menu. The middle of it is doable for sure, goods in a circle are a little tricky.
* Resource monitor at the top
## Compiling
A few caveats if you would like to compile this yourself:
* Visual Studio advised
* BepInExRequired
* copy_dll.bat is a post-build script that copies a compiled DLL into a BepInEx directory, you may leave it empty
* ShowThatRecipe.csproj contains a few local assembly references, so you would have to fix paths to them
