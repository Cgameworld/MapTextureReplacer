# Map Texture Replacer

![Mod Window Opened](https://raw.githubusercontent.com/Cgameworld/MapTextureReplacer/master/screenshot.jpg)

This mod allows you to easily replace grass, dirt and cliff map textures in your game.

This mod takes advantage of the fact that map textures are stored as standard unity textures, making them straightforward to read and set. 

Hopefully when the full official editor releases, it will include map theming tools, but in the meantime this mod makes basic theming possible.

# Requirements
- BepInEx 5
- HookUI

# Installation
1) Download the mod from either [thunderstore.io](https://thunderstore.io/c/cities-skylines-ii/p/Cgameworld/MapTextureReplacer) or from the mod's releases page on [GitHub](https://github.com/Cgameworld/MapTextureReplacer/releases) 
2) If installing from GitHub, place MapTextureReplacer folder inside `BepInEx/plugins` and optionally download the example texture pack DesertMapTheme.zip

# Instructions

1. To open, click on the HookUI button near the top left of the screen and select the mod from the dropdown
2. Click "Load Texture Pack" to load a texture pack or click on "Select Image" to replace an individual texture. 

# Texture Packs

This mod has support to load in texture packs via zip files. This mod includes one example texture pack, a desert map theme.

The vanilla game uses 4096x4096 sized map textures by default for grass, 2048x2048 for dirt and cliff though smaller scaled textures will successfully replace. For example, the example desert map theme pack uses 1024x1024 sized textures.

To make your own pack, just compress some or all of the following filenames into a zip file

>Grass_BaseColor.png   
Grass_Normal.png  
Dirt_BaseColor.png  
Dirt_Normal.png  
Cliff_BaseColor.png  
Cliff_Normal.png 

# Disclaimer

This mod is experimental, so things might not work as expected

# Acknowledgements

Thanks to the Cities Skylines 2 modding community for making modding more accessible

# Conclusion

[Source Code](https://github.com/Cgameworld/MapTextureReplacer/)   
[Discord](https://discord.gg/tDZhaMrgsQ)

If you like this mod be sure to leave an upvote! Thanks