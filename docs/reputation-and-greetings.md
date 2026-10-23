# Reputation and greeting selection

The handheld saloon report stopped on GetReputationThreshold (CTDA 575) in
vDialogueSloanGeneric before selecting a greeting. That failure also left the
presentation's conversation lifetime active for saving and rejected new requests.

Player reputation now owns independent fame and infamy counters. Winning REPU
DATA supplies each maximum; winning/default fReputationThresholdOne/Two/Three
supply the fractional tier boundaries. Raw, percentage and threshold queries and
reached Add/Remove/SetReputation script commands share these counters. Save v22
stores them and rejects invalid counters or changed REPU scales. Older saves
start at zero: reputation mutation commands were previously unsupported.
Automatic crime/disguise adjustments, notifications and complete reputation UI
remain unbound.

Function identities follow [xEdit CTDA documentation](https://tes5edit.github.io/fopdoc/FalloutNV/Records/Subrecords/CTDA.html).
The independent axes and named-standing matrix follow the documented
[reputation query contract](https://geckwiki.com/index.php/GetReputationThreshold)
and [REPU scale meaning](https://geckwiki.com/index.php/Reputation).

Random INFO candidates use a bounded pool, the authored Random End marker and
one saved script random draw. Non-random records do not join that pool; quest
boundaries retain their order. See the [GECK dialogue documentation](https://geckwiki.com/index.php/Category:Dialogue).
Exact retail random cadence and comprehensive dialogue routing remain unverified.
A failed presentation releases modal input and stops counting as an active
conversation continuation. A new explicit request can create a fresh transition
owner when speech is idle and healthy; prior results and said history remain.

GetDead initializes source corpse state when queried. GetDeadCount counts retained
lethal transitions by their exact actor base, including unloaded/deleted instances.
Authored initial corpses are not runtime kills. Resurrection and respawn remain
unsupported; no claim is made for those historical-count transitions.

Synthetic contracts cover all sixteen named standings, boundaries, clamping,
independent counters, cold state, invalid inputs, random pools and source group
boundaries. Save checks cover v21 migration and missing v22 counters. The owned
bar audit selects greetings for the reported settler, Easy Pete and Sunny Smiles;
it deliberately does not execute result scripts or voices. Ordinary handheld
speech, choices, results, saving and broader quest progression require their own
acceptance.
