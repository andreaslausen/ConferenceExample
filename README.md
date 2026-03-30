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

## Architecture Documentation

The architecture documentation follows the [arc42](https://arc42.org/) template and is written in AsciiDoc. The source files are located in [src/documentation/](src/documentation/).

The published documentation is available at: **https://andreaslausen.github.io/ConferenceExample/**

### Generating the Documentation

The documentation is built using [Asciidoctor](https://asciidoctor.org/) via Docker. Make sure Docker is running, then execute:

```bash
./build-docs.sh
```

This will:
- Render the AsciiDoc sources from `src/documentation/` into `docs/index.html`
- Process any embedded diagrams using [Asciidoctor Diagram](https://docs.asciidoctor.org/diagram-extension/latest/)
- Copy the generated images into the `docs/images/` directory

The output can be previewed by opening `docs/index.html` in a browser. Committing changes to `docs/` will update the GitHub Pages site.