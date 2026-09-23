# Security policy

## Supported versions

Only the latest release of TidyFlow receives fixes. Please update before reporting a problem.

| Version | Supported |
|---|---|
| 2.x (latest) | Yes |
| 1.x | No |

## Reporting a vulnerability

Please **don't open a public issue** for security problems.

Report them privately through GitHub instead: open the repository's **Security** tab and select **Report a vulnerability**. Include what you found, how to reproduce it, and what an attacker could do with it.

You can expect an acknowledgement within a week. Once a fix is ready it's released and the advisory is published, crediting you unless you'd rather stay anonymous.

## What's in scope

TidyFlow moves files on your own PC and never connects to the network, so the things worth reporting are mostly local:

- Moving, overwriting or deleting files it shouldn't (for example, escaping the configured folders)
- Anything that lets another user or process run code through TidyFlow's scheduled task, startup entry or settings files
- Vulnerable dependencies that affect the shipped app

Portable builds are not code-signed, so Windows SmartScreen may warn about them. That's expected and not a vulnerability; only download TidyFlow from this repository's [Releases](https://github.com/ProfessorMoose74/TidyPackRat/releases) page.
