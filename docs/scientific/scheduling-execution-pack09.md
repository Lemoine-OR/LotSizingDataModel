# Scheduling execution - Pack 09

Pack 09 introduces the local scheduling execution substrate that was absent
from the installed worktree.

It adds:
- ProductionSchedulingProfile;
- macro/micro bucket mode;
- micro-period setup-state variables;
- sequence-dependent changeover variables;
- state-transition constraints;
- carry-over across macro-period boundaries when explicitly Allowed;
- normalized WorkCenterSchedulingDecision;
- independent checker-side scheduling semantics.

The Pack 08 admission guard is relaxed only when a WorkCenter has an explicit
SchedulingProfile capable of representing the transition semantics.

No assumption is made that sequence-dependent setup can be executed correctly
inside the old pure big-bucket state alone.
