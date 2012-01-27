# MonoGame Automated Tests

*This document is currently a very rough draft!  Updates coming soon.*

## Overview

Describe the general structure of the tests.
- TestGameBase
  - Sets up Content root directory
  - Adds IFrameInfoSource service
  - Suppresses 'extra' updates and draws that happen after the call to
    Game.Exit()
  - Provides an ExitCondition property, Predicate<FrameInfo>
  - Handles calling XNA Run or MonoGame's Run(Synchronous)

## Affordances

- Paths.\*
- On Windows, can be run with NUnit GUI or Console
- On Mac/Linux, a custom runner is provided and can be invoked through
  the ordinary Run Project command.

## Special Considerations

- Paths.SetStandardWorkingDirectory() in [SetUp]
- Note that all platforms are forced to run in Synchronous mode and
  that this doesn't always work perfectly on all platforms yet.
- Note that MacOS synchronous mode, in particular, is a bit goofy right
  now.

## Visual Tests

- [RequiresSTA]
- Assets/ReferenceImages
  - Content
  - Copy if newer
- CapturedFrames/{testname}
- Diffs/{testname}
- For similarity thresholds, prefer Constants.StandardRequiredSimilarity
  unless there is a very good reason to choose another value.

## NUnit Configuration

- For debugger support, Run tests directly in the NUnit process, (note
  that this causes a slight hang when exiting NUnit after running a
  visual test) otherwise choose 'single separate process'
  - GUI: Tools > Settings > Test Loader > Assembly Isolation
- Disable shadow copying (corrects path issues)
  - GUI: Tools > Settings > Test Loader > Advanced
  - CLI: /noshadow
