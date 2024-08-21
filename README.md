# Catdocs
## Maintain OpenAPI documentations with ease
Welcome to **Catdocs**! This repository contains tools, libraries, and a CLI for managing OpenAPI documentation, specifically designed for teams working collaboratively on APIs.

## Introduction

Catdocs is an OpenAPI documentation tool that streamlines the process of managing API documentation for teams. By enabling the splitting of OpenAPI documents, Catdocs allows different teams or individuals to work on separate parts of the API documentation, ensuring efficient and organized collaboration.

## Features

- **OpenAPI Documentation Management**: Easily handle large OpenAPI documents by splitting them into manageable parts.
- **Team Collaboration**: Supports collaborative work by allowing different team members to edit specific sections of the API documentation.
- **External Reference Handling**: Automatically resolves and manages external references in OpenAPI documents.
- **CLI Tool**: Includes a command-line interface for easy integration into your workflow.
- **Validation**: Validates OpenAPI documents to catch errors before publishing.
- **Version Control Integration**: Seamlessly integrates with Git for version control, enabling team members to push and pull changes effortlessly.

## Benefits

- **Scalability**: Manage large and complex OpenAPI documents by breaking them down into smaller, manageable parts.
- **Team Efficiency**: Improve team efficiency by allowing members to focus on their specific areas of responsibility.
- **Error Reduction**: Reduce errors by validating OpenAPI documents before publishing.
- **Seamless Integration**: Integrate Catdocs into your existing workflow with ease, thanks to its CLI and version control capabilities.

## How It Works

1. **Splitting Documentation**: Catdocs splits your main OpenAPI document into multiple files, each representing different parts of the API (e.g., paths, responses, schemas).
2. **Editing Parts Independently**: Team members can work on their assigned parts independently, using their preferred tools.
3. **Merging Changes**: Catdocs merges the individual parts back into a single cohesive OpenAPI document, resolving any references.
4. **Validation**: Before finalizing the documentation, Catdocs validates the OpenAPI document to ensure there are no errors.
5. **Version Control**: Use Git to manage changes, enabling team collaboration and maintaining a history of modifications.

## Getting Started

## Commands
Catdocs supports several commands to help you work with OpenAPI documents: stats, split, bundle, and convert.

### 0. stats
The stats command analyzes an OpenAPI specification file, validates it, and prints out information about its components such as the number of API paths, request bodies, and responses.

Usage:
```bash
catdocs --file example-openapi.yaml --spec-ver 3 --format json
```
Options:
- **--file** (Aliases: --source, --spec, -s): The path to the OpenAPI spec file.
- **--spec-ver** (Aliases: --spec-version, -v): The OpenAPI specification version (2 or 3). Default is 3.
- **--format**: The format of the OpenAPI file (json or yaml). Default is based on the file extension.

### 1. split
This command splits an OpenAPI document into separate reusable components, each in its own file, with the main document containing references to these components.

Usage:
```bash
catdocs split --file OpenApi.yaml --spec-ver 3 --format yaml --outputDir examples/bundle-pipeline
```

Options:
- **--file** (Aliases: --source, --spec, -s): Path to the source OpenAPI documentation file.
- **--spec-ver** (Aliases: --spec-version, -v): The OpenAPI specification version (2 or 3). Default is 3.
- **--format**: The format of the OpenAPI file (json or yaml). Default is based on the file extension.
- **--outputDir**: The directory path where the output files will be saved. This directory will contain an OpenAPI file with external references to each component file path.

### 2. bundle
This command bundles a previously split OpenAPI file by putting its components back into their places, validates the document, logs any errors, and outputs a single bundled OpenAPI file if there are no validation errors.

Usage:
```bash
catdocs bundle --file examples/bundle-pipeline/OpenApi.yaml --spec-ver 3 --format yaml --output examples/bundle-pipeline/output.yaml
```

Options:
- **--file** (Aliases: --source, --spec, -s): The path to the OpenAPI spec file.
- **--spec-ver** (Aliases: --spec-version, -v): The OpenAPI specification version (2 or 3). Default is 3.
- **--format**: The format of the OpenAPI file (json or yaml). Default is based on the file extension.
- **--output**: The destination filename for the output of this command, a bundled OpenAPI file.

### 4. convert
Converts an OpenAPI document from JSON to YAML or vice versa, depending on the source file format. It also validates the source file and lists any errors found during the process.

Usage:
```bash
catdocs convert --file docs.json --spec-ver 3 --format json --output docs.yaml
```

Options:
- **--file** (Aliases: --source, --spec, -s): The source OpenAPI filename.
- **--spec-ver** (Aliases: --spec-version, -v): The OpenAPI specification version (2 or 3). Default is 3.
- **--format**: The source file format (json or yaml). The output will be in the opposite format.
- **--output**: The destination filename for the output file of this command.

## Contributing

We welcome contributions!

## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contact
imun22 with gmail in the end!
---
