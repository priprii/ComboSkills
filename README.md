# ComboSkills
An ACT plugin for FFXIV which enables the creation of skill combos

It should be noted that although ComboSkills is a solution for the GCD issue of FFXIV macros, it does not behave the same way. ComboSkills combos are not automated in sequence, instead each skill in a combo sequence is activated when the keybinding of the initial skill of the combo is activated. Basically, instead of having to press multiple buttons for multiple skills, you're only pressing 1 button multiple times and each time a skill is detected as being used in the combat log, the keybinding for the button being pressed changes to the binding of the next skill in the combo.

Feel free to support me with a donation [here](https://streamlabs.com/primpri) if this project was useful to you!

# Installation
- Download the latest version on the Releases page [here](https://github.com/priprii/ComboSkills/releases)
- Extract to AppData\Roaming\Advanced Combat Tracker\Plugins\CombatSkills
- In ACT on the 'Plugins' tab, click 'Browse' and find 'ComboSkills.dll' in the plugin directory
- Click 'Add/Enable Plugin'
- Note: In order for job class detection to function, [Cactbot](https://github.com/quisquous/cactbot) must also be installed and loaded before ComboSkills
- When ComboSkills has been loaded, you'll find a new subtab on the Plugins tab called 'ComboSkills.dll' where you can setup skills/combos

# Skills
The Skills tab is where you add skills that you want to use in combos.
- **Skill Name**: Must be exactly how it shows in the in-game combat log when you use a skill
- **Alternative Name**: For skills which switch to other skills on proc (Eg. Dragoon's 'True Thrust' becomes 'Raiden Thrust'), otherwise leave this field blank
- **Key Binding**: The key you press when using the skill in-game
- - Currently using lazy implementation of this so you have to type out the key as they're shown [here](https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.keys?view=windowsdesktop-6.0), Eg. 'D1' for the '1' key, 'NumPad1' for '1' on numpad
- **Modifier Keys**: Check any of these if you use them with the skill key binding, Eg. Ctrl + Shift + 1
- **Add/Update Skill**: Adds a new skill to the skills list (or update if an item in the list was selected), with the properties in the above fields
- **Remove Skill**: Removes the selected item from the skills list

# Combos
The Combos tab is where you setup combos using the skills added on the Skills tab.
- **Class**: Select the job class that the combo will be active for
- **Expiration**: The time in seconds until a combo is considered broken, set to 0 for no expiration. Eg. If a Dragoon uses True Thrust, the potency bonus when following with Disembowel is only available for 30 seconds, so setting this to 30 will cause a 'True Thrust > Disembowel' combo to reset if the combo keybinding isn't activated again within 30 seconds.
- **Select Skill**: This is a list of skills added to the Skills list on the Skills tab that you may want to add to a combo
- **Add/Remove Skill**: Adds/Removes the skill selected in the aformentioned dropdown box to the combo
- **Combo Sequence**: A preview of the current combo setup, if you add 'True Thrust' and then add 'Disembowel' from the skill list, you'll see 'True Thrust > Disembowel', whereby the combo starts from the left and ends with the skill on the right
- **Add/Update Combo**: When you have finished adding skills to the combo, click this to add it to the combo list (or update a combo selected in the list)
- **Remove Combo**: Removes the combo selected in the combo list

Eg. If I have a 'True Thrust > Disembowel' combo and 'True Thrust' is bound to NumPad1 while 'Disembowel' is bound to NumPad2, I press 'NumPad1' to trigger 'True Thrust', and then I press it again to trigger 'Disembowel'. ComboSkills will send 'NumPad2' instead of the 2nd 'NumPad1' when it receives notice of 'True Thrust' being used. When a combo is at the end of the sequence, the initial skill can be activated again.

I would suggest binding skills in a combo to keybinds that you wouldn't typically press like 'Ctrl + Shift + F1' and put them on a hidden actionbar, so that you have more normal bindings to work with for other things. Eg. 'Disembowel' is a skill that you only ever use in combo after 'True Thrust' and it doesn't have a cooldown, so it's safe to hide it away and give it a keybinding that we'll never directly use.

You can also break combo by activating the initial skill of a different combo (which from my limited experience is how combos are broken in-game). Eg. If I have a single target combo of 'True Thrust > Disembowel > Chaos Thrust' and an AoE combo of 'Doom Spike > Sonic Thrust > Coerthan Torment', if I do 'True Thrust > Disembowel' but then switch to AoE with 'Doom Spike', the single target combo will be reset while the AoE combo will be triggering.

# Issues
- The current implementation does not allow assigning the same skill multiple times for combos that may use the same initial skill, Eg. 'True Thrust > Disembowel' and 'True Thrust > Vorpal Thrust', for this instance with Dragoon you'll need to setup one combo initiated with 'True Thrust' while the other combo initiates from the 2nd skill in the chain (Disembowel or Vorpal Thrust). I'm not sure whether this affects any other job classes as I'm a sprout, I haven't played many classes yet.
