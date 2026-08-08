# CPLEX compatibility pack

This pack updates the project so that the newest visible compatible
CPLEX installation is preferred automatically, while remaining usable
with CPLEX 20.1.

Automatic priority:

1. 22.2.x
2. 22.1.2
3. 22.1.1
4. 22.1
5. 20.1

IBM-documented environment variables included:

- CPLEX_STUDIO_DIR2212
- CPLEX_STUDIO_DIR2211
- CPLEX_STUDIO_DIR221
- CPLEX_STUDIO_DIR201

The project additionally accepts both CPLEX_STUDIO_DIR222 and
CPLEX_STUDIO_DIR2220 aliases for 22.2 installations.

For the user's current installer:

    ILOG_CPLEX_OS_20.10_WIN_X86_64

the expected installation family is 20.1 and the normal environment
variable is:

    CPLEX_STUDIO_DIR201

The managed assembly directory is expected under:

    <CPLEX Studio root>\cplex\bin\x64_win64

Important:
Do not mix managed assemblies from one CPLEX release with native runtime
libraries from another release.
