# Unity Utils

## HierarchyHighlight

### Usage

Name a game object in the Scene hierarchy in one of the following formats:

* Default colour: `{} TEXT`
* Colour initials: `{y} TEXT`
	* Valid initials: `w`, `m`, `g`, `b`, `p`, `d`, `y`, `o`, `r`
* Unity build-in named colours: `{teal} TEXT`
	* Valid names: `red`, `cyan`, `blue`, `darkblue`, `lightblue`, `purple`, `yellow`, `lime`, `fuchsia`, `white`, `silver`, `grey`, `black`, `orange`, `brown`, `maroon`, `green`, `olive`, `navy`, `teal`, `aqua`, `magenta`
* Hex colors: `{#ff0099} TEXT`
	* 3 digit hex also valid: `{#F09} Text`
	* HEX not case sensitive

### Inheriting colors

* Child objects with the default color `{} TEXT` will inherit the color of the next colored parent (if there is one). The child's color will be 10% darker and 10% more saturated.
* Child objects whose direct parents are not colored, but have a colored parent further up the hierarchy will respect their nesting level and be the same color as other objects of the same level.

### Shortcuts

* `Alt+Shift+C` / `Option+Shift+C`: Cycle selected object through colour initials

### Preview

![Hierarchy Screenshot](https://i.imgur.com/VpmpNs0.png)
