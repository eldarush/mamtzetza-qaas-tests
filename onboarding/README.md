# Offline onboarding materials

Run `python onboarding/build_content.py` from this repository root to build the course HTML source. `content/` is packaged at ZIP root alongside coordinated repository snapshots under `sources/`, measured verification evidence and a manifest. Relative links are validated after assembly. The HTML needs no web server or third-party CDN.

Teacher answers and the optional assessment seed are under `content/teacher/`; student exercises are under `content/student/`. These are deliberately separated for instructor distribution. The proposed practical is new and must be reconciled with the original exam when it is identified.

Graph editing, regeneration and finalization remain blocked until the parent explicitly confirms that QaaS Shampoo has stopped and identifies its final graph and exam. `graph-status.html` records this gate; it is not a substitute graph. A package made while this gate is pending must be labeled a review package, not the final requested ZIP.
