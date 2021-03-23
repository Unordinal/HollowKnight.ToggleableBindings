# HollowKnight.ToggleableBindings
This mod allows you to bring up a menu at any time which will let you enable or disable bindings. This can be used to enable bindings when fighting in the Hall of Gods or at any other point in the game. Mod authors can create their own custom bindings using the `BindingManager` and `Binding` classes.

The default keybind to open the menu that allows you to turn on and off the bindings is `Down` + `SuperDash`. That is, hold the 'Down' direction and then, while holding it, press the 'Crystal Dash' button. You can change this keybind in the settings, which are described below in the `Configuration` section.

<img title="Screenshot of Bindings Menu" src="https://share.wildbook.me/j8VwOP3hvr3e7DKi.jpg" width="800"/>

[A changelog is available here.](./CHANGELOG.md) Note: This mod does not touch the vanilla bindings menu at all.

# Configuration
`ToggleableBindings` provides multiple configuration settings, such as the ability to allow specific charms even when the Charms binding is active (by default, charms needed for game progression - such as Grimmchild and Kingsoul - are allowed if you enable the setting) and the option to enforce any binding restrictions (you need to be near a bench to apply/restore the default bindings otherwise).

_Settings files have the extension `.json` and are located in your saves folder, in a folder named `ToggleableBindings.QuickSettings`._

***Make sure to keep the format of each setting intact or the settings file will fail to load! A robust text editor like Notepad++ or VS Code is highly recommended.***

## Global Settings
*Located in `Settings.Global.json`.*

#### **`EnforceBindingRestrictions`** [_Default: `true`_]
Bindings can sometimes restrict you from applying them - for instance, the default vanilla bindings will only allow you to enable or disable them when you're near a bench. If this is `false`, you can enable and disable bindings whenever you like.

#### **`OpenBindingUI`** [_Default: `Down,SuperDash`_]
This is the keybind that is used to open the Bindings menu. This uses actions instead of specific buttons or keys. For example, the default bind requires you to press whatever keys/buttons you have bound to the down direction and Crystal Dash to open the menu. The value is comma-separated and case-insensitive. An example bind would be '`Up,DreamNail`'. Valid actions you can bind are listed below.
* Left, Right, Up, Down
* Jump
* Attack
* Dash
* SuperDash
* Cast
* QuickCast
* QuickMap
* DreamNail
* Inventory

### Binding-Specific Settings
*Located in `Settings.Global.json`.*

#### **`NailBinding.MaxBoundNailDamage`** [_Default: `13`_]
When the `Nail` binding is active, this determines the nail's maximum amount of damage. The default is vanilla.

#### **`ShellBinding.MaxBoundShellHealth`** [_Default: `4`_]
When the `Shell` binding is active, this determines the maximum amount of health the Knight can have. The default is vanilla.

#### **`CharmsBinding.AllowEssentialCharms`** [_Default: `true`_]
When the `Charms` binding is active, this determines whether the charms listed in `EssentialCharms` can be equipped despite the binding being active.

#### **`CharmsBinding.EssentialCharms`** [_Default: `36, 40` (Kingsoul/Void Heart and Grimmchild)_]
This is a list of charm IDs. When the `Charms` binding is active and `AllowEssentialCharms` is set to `true`, the charms with the same IDs as the ones listed here can be equipped despite the binding being active. [All of the valid charm IDs and their respective charms can be seen in this image.](https://cdn.discordapp.com/attachments/462200562620825600/548520742246154260/charmID.png)

## Per-Save Settings
*Located in `Settings.Save#.json` where `#` is the save slot.*

By default, there are no user-friendly editable settings here, but you may wish to change values here in a few circumstances.

#### **`BindingManager.RegisteredBindings`**
These are the bindings registered in this specific save. `$type` is the internal type used for each binding, while `WasApplied` is used to tell whether the binding should be applied when the save is loaded. If for some reason a binding is resulting in you not being able to load a save, you can disable it by setting its `WasApplied` value to `false`.

# Uninstalling
There is no special proceedure you have to follow to uninstall - the mod automatically undoes any changes made when the game is saved.

# Mod Devs - Creating a Custom Binding
Creating and registering a new binding is relatively easy, discounting the actual behavior part. You can look at the [default bindings](./ToggleableBindings/VanillaBindings) for quick reference.

First, you'll of course want to add a reference to `ToggleableBindings.dll`. You may want the `.xml` file as well as it contains XML documentation.

Create a new file in your project - you may name it whatever you like, but it's recommended to add the word `Binding` to the end.

In the new file, have your class inherit from `ToggleableBindings.Binding`. The constructor of the class should call the base constructor with the name of the binding via `base("YourBindingNameHere")`.

Add the required `OnApplied()` and `OnRestored()` overrides.

In `OnApplied()`, implement whatever you want your binding to do when it's enabled. This usually involves hooking game methods and overriding values.

In `OnRestored()`, make sure any changes your binding made in `OnApplied()` are reverted, as this method is called both when the player disables the binding _and_ when the game is saved (and quit). Ensure that any data that is normally saved to the actual game's save file is reverted in the same frame the method is called, or the changes could inadvertently be saved to the player's save file (which we don't want to have happen).

Optionally override the `DefaultSprite` and `SelectedSprite` properties, and provide your own sprites. These sprites will be shown in the Bindings menu. If they aren't provided, they will have placeholder sprites with a `?` symbol.

Optionally override the `CanBeApplied()` and `CanBeRestored()` methods. These methods determine when the player can enable or disable your binding. The default behavior is to allow applying and restoring as long as the player is within 10 units of a bench. Note that if the player has `EnforceBindingRestrictions` set to `false`, these methods will be ignored. The return value of these methods is `ResultInfo<bool>`. The first parameter is the value of the bool; the second is the message given to the player if the result is `false`.

Once your binding class is set up to your liking, you just have one or two steps left.

In your mod initializer, use the `BindingManager.RegisterBinding` method to register your binding. This will add it to the Bindings menu and allow the player to enable and disable it.

If your mod implements `ITogglableMod`, use the `BindingManager.DeregisterBinding` method in your `Unload()` implementation to deregister your binding. This will call `Restore()` on it if it was enabled before deregistering it.