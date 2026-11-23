#!/usr/bin/env python3
"""Validate FSM_Dungeon experience manifests and originality declarations."""

from __future__ import annotations

import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
EXPERIENCES = ROOT / "docs" / "experiences"
REQUIRED = {"schemaVersion", "id", "title", "status", "originality", "domain", "integrations", "experience", "content", "firstVerticalSlice", "ciAi"}


def fail(path: Path, message: str) -> None:
    print(f"ERROR: {path.relative_to(ROOT)}: {message}", file=sys.stderr)
    raise SystemExit(1)


def main() -> int:
    manifests = sorted(EXPERIENCES.glob("*.json"))
    if not manifests:
        print("ERROR: no experience manifests found", file=sys.stderr)
        return 1

    ids: set[str] = set()
    for path in manifests:
        try:
            data = json.loads(path.read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError) as exc:
            fail(path, f"invalid JSON: {exc}")
        if not isinstance(data, dict):
            fail(path, "manifest root must be an object")
        missing = sorted(REQUIRED - data.keys())
        if missing:
            fail(path, f"missing fields: {', '.join(missing)}")

        identity = data["id"]
        if not isinstance(identity, str) or not identity.strip() or identity in ids:
            fail(path, "id must be a unique non-empty string")
        ids.add(identity)

        originality = data["originality"]
        if not isinstance(originality, dict):
            fail(path, "originality must be an object")
        for field in ("workshopOriginal", "thirdPartyAssets", "thirdPartyRuleText"):
            if type(originality.get(field)) is not bool:
                fail(path, f"originality.{field} must be boolean")
        if originality["workshopOriginal"] is not True:
            fail(path, "experience must declare Workshop-original authorship")
        if originality["thirdPartyAssets"] or originality["thirdPartyRuleText"]:
            fail(path, "third-party assets and copied rule text are prohibited")

        domain = data["domain"]
        if not isinstance(domain, dict) or domain.get("package") != "TheSingularityWorkshop.FSM_Dungeon":
            fail(path, "domain.package must reference the shared FSM_Dungeon package")
        if domain.get("capability") != "fsm-dungeon.underground-environment":
            fail(path, "domain.capability must reference the shared underground-environment capability")

        integrations = data["integrations"]
        if not isinstance(integrations, dict) or integrations.get("actorOntology", {}).get("owner") != "SURPS":
            fail(path, "actor ontology must remain owned by SURPS")
        if data["content"].get("noEngineSpecificContentEnums") is not True:
            fail(path, "experience content must not require engine-specific content enums")

        slice_spec = data["firstVerticalSlice"]
        if not isinstance(slice_spec, dict) or not slice_spec.get("features"):
            fail(path, "firstVerticalSlice.features must be a non-empty list")
        if slice_spec.get("deterministicReplayRequired") is not True:
            fail(path, "deterministic replay is required")

        ci_ai = data["ciAi"]
        if not isinstance(ci_ai, dict) or ci_ai.get("requiresBuildAndTests") is not True:
            fail(path, "CiAi workloads must require build and test evidence")
        if ci_ai.get("requiresContentProvenance") is not True or ci_ai.get("requiresHumanApprovalForProtectedActions") is not True:
            fail(path, "CiAi workloads must preserve provenance and host approval gates")

        print(f"OK: {identity} ({data['status']})")

    print(f"Validated {len(manifests)} experience manifest(s).")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
