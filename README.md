# HollowKnight.ToggleableBindings
This mod allows you to bring up a menu at any time which will let you enable or disable bindings. This can be used to enable bindings when fighting in the Hall of Gods or at any other point in the game. Mod authors can create their own custom bindings using the `BindingManager` and `Binding` classes.

<img title="Screenshot of Bindings Menu" src="https://share.wildbook.me/j8VwOP3hvr3e7DKi.jpg" width="800"/>

[A changelog is available here.](./CHANGELOG.md)

[Testing video #3](https://youtu.be/vFd8THRPUGc) - Only a few more bugs to iron out.

[Testing video #2](https://www.youtube.com/watch?v=mbTqIC7KFvY) - It's coming together decently.

[Testing video #1](https://youtu.be/1n8NwYg6ZK4) - The video is somewhat out-of-date but gives a general idea.

# Configuration
`ToggleableBindings` provides multiple configuration settings, such as the ability to allow specific charms even when the Charms binding is active (by default, charms needed for game progression - such as Grimmchild and Kingsoul - are allowed if you enable the setting) and the option to enforce any binding restrictions (you need to be near a bench to apply/restore the default bindings otherwise).

_Settings files have the extension `.json` and are located in your saves folder, in a folder named `ToggleableBindings.QuickSettings`._

***Make sure to keep the format of each setting intact or the settings file will fail to load! A robust text editor like Notepad++ or VS Code is highly recommended.***

## Global Settings
*Located in `Settings.Global.json`.*

#### **`EnforceBindingRestrictions`** [_Default: `true`_]
Bindings can sometimes restrict you from applying them - for instance, the default vanilla bindings will only allow you to enable or disable them when you're near a bench. If this is `false`, you can enable and disable bindings whenever you like.

#### **`OpenBindingUI`** [_Default: `Down,DreamNail`_]
This is the keybind that is used to open the Bindings menu. This uses actions instead of specific buttons or keys. For example, the default bind requires you to press whatever keys/buttons you have bound to the down direction and the Dream Nail to open the menu. The value is comma-separated and case-insensitive. An example bind would be '`Up,SuperDash`'. Valid actions you can bind are listed below.
* Left, Right, Up, Down
* RS_Left, RS_Right, RS_Up, RS_Down
* Jump
* Attack
* Dash
* SuperDash
* Cast
* QuickCast
* DreamNail
* Focus
* QuickMap
* Inventory

### Binding-Specific Settings
*Located in `Settings.Global.json`.*

#### *`NailBinding.`***`MaxBoundNailDamage`** [_Default: `13`_]
When the `Nail` binding is active, this determines the nail's maximum amount of damage. The default is vanilla.

#### *`ShellBinding.`***`MaxBoundShellHealth`** [_Default: `4`_]
When the `Shell` binding is active, this determines the maximum amount of health the Knight can have. The default is vanilla.

#### *`CharmsBinding.`***`AllowEssentialCharms`** [_Default: `false`_]
When the `Charms` binding is active, this determines whether the charms listed in `EssentialCharms` can be equipped despite the binding being active.

#### *`CharmsBinding.`***`EssentialCharms`** [_Default: `36, 40` (Kingsoul/Void Heart and Grimmchild)_]
This is a list of charm IDs. When the `Charms` binding is active and `AllowEssentialCharms` is set to `true`, the charms with the same IDs as the ones listed here can be equipped despite the binding being active. [All of the valid charm IDs and their respective charms can be seen in this image.](https://cdn.discordapp.com/attachments/462200562620825600/548520742246154260/charmID.png)

## Per-Save Settings
*Located in `Settings.Save#.json` where `#` is the save slot.*

By default, there are no user-friendly editable settings here, but you may wish to change values here in some circumstances.

#### *`BindingManager.`***`RegisteredBindings`**
These are the bindings registered in this specific save. `$type` is the internal type used for each binding, while `WasApplied` is used to tell whether the binding should be applied when the save is loaded. If for some reason a binding is resulting in you not being able to load a save, you can disable it by setting its `WasApplied` value to `false`.

# Uninstalling
There is no special proceedure you have to follow to uninstall - the mod automatically undoes any changes made when the game is saved.

# Mod Devs - Creating a Custom Binding
Creating and registering a new binding is relatively easy, discounting the actual behavior part.

First, you'll of course want to add a reference to `ToggleableBindings.dll`.