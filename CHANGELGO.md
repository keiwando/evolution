## v3.0.6

- Fixed Web build in Safari by forcing the use of WebGL 1.0

## v3.0.5

- Fixed creatures with more than 100% fitness to still be properly sorted.

## v3.0.4

- Fixed a bug that could cause some not-yet-fully-placed muscles and bones to get stuck and stay on the screen in the creature editor when touches got cancelled.

## v3.0.3

- Fixed a bug that caused neural network settings changes to not be applied immediately

## v3.0.0

### New Features

- Import and export save files (so you can share them with your friends)
- Distance markers
- New and improved editor UI
- New save file selection UI
- Search save files
- Rename save files
- Undo & Redo
- Selection tool
- Move bones and multiple joints at once
- Pinch to zoom
- Customize muscle strength
- Customize bone and joint weights
- Multiple selection algorithms
- Multiple recombination algorithms
- Multiple mutation algorithms
- Improved physics simulation performance
- Improved visual simulation clarity
- Option to hide muscles completely
- Zoom during the simulation
- Change the population size during the simulation
- Rendering the neural network visualization is now about 200x faster
- Smaller save files
- New Help UI with improved performance

### Fixes

- Added iPhone X/XS support
- Fixed memory leaks
- Improved deterministic playback
- The simulation is now more stable so there should be fewer glitches

## v2.0.3

- Fixed a bug that caused the autosaver to accidentally delete old save files instead of just overwriting the current autosave.

## v2.0.2

- Help section translation in Russian, Portuguese and German
- Visualization of muscle contraction / expansion
- Important fixes and other improvements

## v2.0.1

- new default creature "ROO"
- the screen will not go to sleep during the simulation any more
- minor fixes

## v2.0

- You can now move joints after you’ve placed them
- There’s also an optional grid to help you with placing the joints
- Customizable Neural Network size
- Delete saved creatures and simulations
- Option to watch the creatures one at a time while they simulate
- Option to simulate a generation in multiple batches
- Adjustable advanced simulation settings
- You’ll get to see more stats about the best creatures

## v1.1

- Added the option to save and load simulations
- Increase hitbox sizes for bones and joints when building creatures
- Added "FROGGER" as a new default creature