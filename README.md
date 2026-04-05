# RayLight

An experemental Nintendo file editor built with Raylib and ImGui for viewing and editing game asset files.

## Features

RayLight supports viewing and editing the following Nintendo file formats:

- **BYML** - Binary YAML used for game configuration and data files
- **AAMP** - Binary parameter files
- **MSBT** - Message text files
- **SZS** - Compressed archives (SARC + Yaz0)

## Supported Games

Currently optimized for Super Mario 3D World (switch) and Bowsers Fury

Functional for Super Mario 3D World (Wii U) *1

#### Known issues

1. Games using V1 BYAML are loaded via hacks and saved as V2, as such break engines that dont support that correctly.

## Building

### Prerequisites

- .NET 9 SDK or later


### Build Steps

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/RayLight.git
   cd RayLight
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the project:
   ```bash
   dotnet build
   ```

4. Run the editor:
   ```bash
   dotnet run --project RayLight
   ```

## Usage

1. Navigate the file tree in the explorer panel
2. click a file to open it in the appropriate editor
3. Make changes using the editor windows
4. Use **File > Save** to write changes back to the archive/file

### Editor Windows

- **BYAML Editor** - Tree view of binary YAML data with editable values
- **AAMP Editor** - View and edit binary parameter files
- **MSBT Editor** - View and edit message text files
- **SZS Editor** - Browse files within compressed archives

## Contributing

Contributions are welcome! Please follow these guidelines:

### Commit Message Format

All commit messages should follow this format:

Comma sepperated list of change tittles
Multi-title commits must be seperated by ; and sorted by importance.

The following Priority is expected:
- New Format
- New feature
- User experience change
- Bugfix
- Optimisation
- Documentation

### AI-Written / Multi Developer Commits

**Important:** Any commit that contains code primarily written with AI assistance **must** start with state that at the top of the description.

Any Code written via the assistance of an LLM or another user, that commit **must** attribute the AI/Colaberator at the top of the description.

Examples:
- `Intergrated with assistance from @{git_user}`
- `Written with help from OpenCode's BigPickle model`
- `A Claude written commit to finally fix this bug`

This helps maintain transparency about contributions and ensures proper code review.

### Pull Requests

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-feature`)
3. Commit your changes
4. Push to the branch (`git push origin feature/my-feature`)
5. Open a Pull Request

## Dependencies

- [Raylib-cs](https://github.com/notnotmelon/raylib-cs) - Bindings for Raylib
- [rlImgui-cs](https://github.com/NotNotTech/rlImGui) - ImGui bindings for Raylib
- [BymlLibrary](https://github.com/EPD-Libraries/BymlLibrary) - BYML parsing
- [AampLibrary](https://github.com/EPD-Libraries/AampLibrary) - AAMP parsing
- [MsbtLib](https://github.com/Ploaj/IONET) - MSBT parsing
- [SarcLibrary](https://github.com/sonic2kk/SarcLibrary) - SZS/SARC handling
- [BfresLibrary](https://github.com/Ploaj/IONET) - BFRES model parsing
