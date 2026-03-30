# ConferenceExample

This is an example which I use in some of my conference talks.

![](logo.png)

## Getting Started

After cloning the repository, run the following script once to set up the git hooks and install required tools:

```bash
./init-hooks.sh
```

This will:
- Register the `.githooks` directory so git picks up the pre-commit hook
- Install [CSharpier](https://csharpier.com/) as a local dotnet tool if not already present

The pre-commit hook automatically formats all staged C# files with CSharpier before each commit. **The hook must be active** to ensure consistent code formatting across the team. Commits that skip the hook may introduce formatting inconsistencies.